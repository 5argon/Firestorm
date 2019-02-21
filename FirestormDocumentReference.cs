using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Auth;
using E7.Firebase.LitJson;
using UnityEngine;

using ValueObject = System.Collections.Generic.Dictionary<string, object>;

namespace E7.Firebase
{
    public struct FirestormDocumentReference
    {
        internal StringBuilder stringBuilder;
        private string documentName;
        private string parent;
        public FirestormDocumentReference(FirestormCollectionReference collection, string name)
        {
            this.stringBuilder = collection.stringBuilder;
            this.parent = stringBuilder.ToString(); //save the parent collection path before appending.
            this.stringBuilder.Append($"/{name}");
            this.documentName = name;
        }

        public FirestormDocumentReference(string fullName)
        {
            fullName = fullName.Substring(fullName.IndexOf("documents") + "documents".Length);
            //Debug.Log($"Ref from full name {fullName}");
            this.stringBuilder = new StringBuilder();
            this.stringBuilder.Append(fullName);

            var split = fullName.Split('/');
            var exceptLastOne = split.Take(split.Length - 1);

            this.documentName = split[split.Length-1];
            this.parent = string.Join("/", exceptLastOne);
        }

        public FirestormCollectionReference Collection(string name) => new FirestormCollectionReference(this, name);

        public async Task UpdateAsync<T>(T documentDataToSet) where T : class
        {
            await DocumentOpsInternal(documentDataToSet, SetOption.MergeAll);
        }

        public async Task SetAsync<T>(T documentDataToSet) where T : class
        {
            await DocumentOpsInternal(documentDataToSet, SetOption.Overwrite);
        }

        /// <summary>
        /// If that value already exist in an array, it won't be added.
        /// </summary>
        public async Task ArrayAppendAsync(string arrayFieldName, object[] toAdd) 
        {
            await DocumentOpsInternal(null, SetOption.ArrayAdd, (arrayFieldName, toAdd));
        }

        /// <summary>
        /// If that value does not exist in an array, nothing would happen 
        /// </summary>
        public async Task ArrayRemoveAsync(string arrayFieldName, object[] toAdd) 
        {
            await DocumentOpsInternal(null, SetOption.ArrayRemove, (arrayFieldName, toAdd));
        }

        private class ArrayDataWrap
        {
            public List<object> data;
        }

        /// <summary>
        /// The clusterfuck
        /// </summary>
        private async Task DocumentOpsInternal(object documentDataToSet, SetOption setOption, (string fieldPath, object[] objs) arrayOperation = default) 
        {
            CommitUpdate commit = null;

            //Build a path for commit. Works both if it is a new document or existing document because we are using `commit` API and not `createDocument` / `patch`.
            string documentPath = $"{FirestormConfig.Instance.DocumentPathFromProjects}{stringBuilder.ToString()}";

            FieldTransformTimestamp[] timestampFieldTransforms = null;
            WriteTransform writeTransformTimestamp = null;
            if (documentDataToSet != null)
            {
                //Add a server time sentinel value support. Scans only top level fields because I am lazy
                var fieldsWithTimestampSentinel = documentDataToSet.GetType().GetFields().Where(x => x.GetCustomAttributes(typeof(ServerTimestamp), inherit: false).Length != 0).Select(x => x.Name);

                timestampFieldTransforms = fieldsWithTimestampSentinel.Select(x => new FieldTransformTimestamp { fieldPath = x, setToServerValue = ServerValue.REQUEST_TIME }).ToArray();

                writeTransformTimestamp = new WriteTransform
                {
                    transform = new DocumentTransform
                    {
                        document = documentPath,
                        fieldTransforms = timestampFieldTransforms
                    }
                };
            }

            if (arrayOperation != default)
            {
                ITransform arrayTransform = null;
                ValueObject[] valueToArrayTransform = arrayOperation.objs.Select(x => FirestormUtility.FormatForValueJson(x)).Select(x => new ValueObject { [x.typeString] = x.objectForJson }).ToArray();

                if (setOption == SetOption.ArrayAdd)
                {
                    arrayTransform = new FieldTransformArrayAppend
                    {
                        fieldPath = arrayOperation.fieldPath,
                        appendMissingElements = new AppendArray
                        {
                            values = valueToArrayTransform
                        }
                    };
                }
                else if (setOption == SetOption.ArrayRemove)
                {
                    arrayTransform = new FieldTransformArrayRemove
                    {
                        fieldPath = arrayOperation.fieldPath,
                        removeAllFromArray = new RemoveArray
                        {
                            values = valueToArrayTransform,
                        }
                    };
                }

                var writeTransformArray = new WriteTransform
                {
                    transform = new DocumentTransform
                    {
                        document = documentPath,
                        fieldTransforms = new ITransform[] { arrayTransform },
                    }
                };

                commit = new CommitUpdate
                {
                    writes = (timestampFieldTransforms != null && timestampFieldTransforms.Length == 0) ? new WriteTransform[] { writeTransformArray } : new WriteTransform[] { writeTransformArray, writeTransformTimestamp },
                };
            }
            else
            {
                //Check if a document is there or not
                var documentServerSnapshot = await GetSnapshotAsync();

                //Document "name" must not be set when creating a new one. The name should be in query parameter "documentId"
                //When updating the name must also be blank. It uses the name from REST URL already.
                string documentJsonToSet = FirestormUtility.ToJsonDocument(documentDataToSet, "");
                //File.WriteAllText(Application.dataPath + "/snap.txt", documentJsonToSet);

                //If there is a data.. we try to build the correct DocumentMask.
                var docSnap = new FirestormDocumentSnapshot(documentJsonToSet);
                string fieldMaskLocal = docSnap.FieldsDocumentMaskJson();

                var localF = JsonMapper.ToObject<DocumentMask>(fieldMaskLocal);
                var mergedFields = new HashSet<string>();
                if (setOption == SetOption.MergeAll)
                {
                    //Getting fields of only local. The server will touch only field presents in the mask = merging.
                    foreach (var f in localF.fieldPaths)
                    {
                        mergedFields.Add(f);
                    }
                }
                else if (setOption == SetOption.Overwrite)
                {
                    //Getting fields of existing data.
                    //Patch a document, using fields from remote combined with local.
                    // Including all local fields ensure update
                    // Including remote fields that does not intersect with local = delete on the server
                    if (documentServerSnapshot.IsEmpty == false)
                    {
                        string fieldMaskRemote = documentServerSnapshot.FieldsDocumentMaskJson();
                        var remoteF = JsonMapper.ToObject<DocumentMask>(fieldMaskRemote);
                        foreach (var f in remoteF.fieldPaths)
                        {
                            mergedFields.Add(f);
                        }
                    }

                    foreach (var f in localF.fieldPaths)
                    {
                        mergedFields.Add(f);
                    }
                }

                var doc = new FirestormDocumentForCommit(documentPath, docSnap.Document);

                var writeUpdate = new WriteUpdate
                {
                    updateMask = new DocumentMask { fieldPaths = mergedFields.ToArray() },
                    update = doc,
                };
                commit = new CommitUpdate
                {
                    writes = (timestampFieldTransforms != null && timestampFieldTransforms.Length == 0) ? new WriteUpdate[] { writeUpdate } : new IWrite[] { writeUpdate, writeTransformTimestamp },
                };
            }

            //byte[] postData = Encoding.UTF8.GetBytes(documentJson);

            // NOTE : I probably shoot myself somewhere while modifying LitJSON, now with nested map field it fails the validation.
            // (probably the object-as-dictionary<string,object> hack cause the lib to miscount object nesting level)
            // But turning off the validation turns out to produce a valid JSON and works with Firestore.. so...

            JsonWriter writer = new JsonWriter();
            writer.Validate = false;
            JsonMapper.ToJson(commit, writer);
            var jsonPostData = writer.ToString();

            byte[] postData = Encoding.UTF8.GetBytes(jsonPostData);
            //Debug.Log($"JPOST {jsonPostData}");

            //lol Android does not support custom verb for UnityWebRequest so we could not use "PATCH"
            //(https://docs.unity3d.com/Manual/UnityWebRequest.html)
            //"custom verbs are permitted on all platforms except for Android"
            //(https://stackoverflow.com/questions/19797842/patch-request-android-volley)
            //(https://answers.unity.com/questions/1230067/trying-to-use-patch-on-a-unitywebrequest-on-androi.html)

            //var uwr = await FirestormConfig.Instance.UWRPatch(stringBuilder.ToString(), updateMaskForUrl, postData);

            //This is now based on POST not PATCH, should work on Android. Also works for both new document and updating old document. Nice!
            var uwr = await FirestormConfig.Instance.UWRPost(":commit", null, postData);

            //The request returns the same document. But we discard it.
            //return new FirestormDocumentSnapshot(uwr.downloadHandler.text).ConvertTo<T>();
        }

        //Super messy class chain to build JSON.. but it works is all that matters in this makeshift API

        private class CommitUpdate
        {
            public IWrite[] writes;
        }

        private interface IWrite
        {
        }

        private interface ITransform
        {
        }

        private class WriteUpdate : IWrite
        {
            public DocumentMask updateMask;
            public FirestormDocumentForCommit update;
        }

        private class WriteTransform : IWrite
        {
            public DocumentTransform transform;
        }

        private class DocumentTransform
        {
            public string document;
            public ITransform[] fieldTransforms;
        }

        private class FieldTransformTimestamp : ITransform
        {
            public string fieldPath;
            public ServerValue setToServerValue = ServerValue.REQUEST_TIME;
        }

        private class FieldTransformArrayAppend : ITransform
        {
            public string fieldPath;
            public AppendArray appendMissingElements;
        }

        private class FieldTransformArrayRemove : ITransform
        {
            public string fieldPath;
            public RemoveArray removeAllFromArray;
        }

        private class AppendArray
        {
            public ValueObject[] values;
        }

        private class RemoveArray
        {
            public ValueObject[] values;
        }

        private enum ServerValue 
        {
            SERVER_VALUE_UNSPECIFIED,
            REQUEST_TIME
        }

        /// <summary>
        /// Deletes an entire document. Deleting fields is not implemented.
        /// </summary>
        public async Task DeleteAsync()
        {
            await FirestormConfig.Instance.UWRDelete(stringBuilder.ToString());
            //Debug.Log($"Delete finished");
        }

        public async Task<FirestormDocumentSnapshot> GetSnapshotAsync()
        {
            //Debug.Log($"do {stringBuilder.ToString()}");
            try
            {
                var uwr = await FirestormConfig.Instance.UWRGet(stringBuilder.ToString());
                Debug.Log($"done {uwr.downloadHandler.text}");
                return new FirestormDocumentSnapshot(uwr.downloadHandler.text);
            }
            catch (FirestormDocumentNotFoundException)
            {
                return FirestormDocumentSnapshot.Empty;
            }
        }

    }

    public enum SetOption
    {
        //Mask field follows the one on the server. So it deletes fields on the server on PATCH request
        Overwrite,
        //Mask field follows all of the one to write.
        MergeAll,
        ArrayAdd,
        ArrayRemove,
    }

}