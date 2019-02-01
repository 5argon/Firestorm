using System;
using UnityEngine;
using System.Linq;
using E7.Firestorm.LitJson;

using ValueObject = System.Collections.Generic.Dictionary<string, object>;
using System.Collections.Generic;

namespace E7.Firestorm
{
    public class ArrayData
    {
        public ValueObject[] values;
    }

    [Serializable]
    public struct FirestormDocumentSnapshot
    {
        private FirestormDocument document;
        private string formattedDataJson;

        public string Name => document.name;
        public FirestormDocument Document => document;

        // public string name;
        // public DateTime createTime;
        // public DateTime updateTime;
        // public JToken fields;
        //public IEnumerable<JProperty> properties;
        public bool IsEmpty { private set; get; }


        public static FirestormDocumentSnapshot Empty
        {
            get
            {
                return new FirestormDocumentSnapshot { IsEmpty = true };
            }
        }

        public T ConvertTo<T>() where T : class, new()
        {
            if (IsEmpty)
            {
                throw new FirestormException($"The document snapshot is empty, please check for IsEmpty instead of trying to convert into an empty instance.");
            }

            //Leave it to LitJSON, we have formatted the json to be ready for convert.
            return JsonMapper.ToObject<T>(formattedDataJson);

            // var serializer = JsonSerializer.Create(new JsonSerializerSettings
            // {
            //     Converters = new JsonConverter[] { new DocumentConverter<T>(name) },
            // }
            // );
            //return JsonMapper.ToObject<T>(
        }

        /// <summary>

        /// DocumentMask : https://firebase.google.com/docs/firestore/reference/rest/v1beta1/DocumentMask
        /// </summary>
        internal string FieldsDocumentMaskJson()
        {
            if (IsEmpty)
            {
                return JsonMapper.ToJson(new DocumentMask());
            }
            else
            {
                var mask = new DocumentMask { fieldPaths = this.document.fields.Keys.ToArray() };
                var m = JsonMapper.ToJson(mask);
                Debug.Log($"Made mask {m}");
                return m;
            }
        }

        public FirestormDocumentSnapshot(string jsonString)
        {
            Debug.Log($"Snapshot from {jsonString}");
            //File.WriteAllText(Application.dataPath + $"/{UnityEngine.Random.Range(0, 100)}.txt", jsonString);
            IsEmpty = false;

            this.document = JsonMapper.ToObject<FirestormDocument>(jsonString);

            //Write in a format that can be map to any object by LitJSON
            var writer = new JsonWriter();
            writer.PrettyPrint = true;
            writer.WriteObjectStart();
            foreach (var field in document.fields.Keys)
            {
                var insideValueText = document.fields[field].First().Key;
                var insideValue = document.fields[field].First().Value;
                writer.WritePropertyName(field);
                ValueTextToWrite(insideValueText, insideValue);
            }
            writer.WriteObjectEnd();

            formattedDataJson =  writer.ToString();
            Debug.Log($"{formattedDataJson}");

            void ValueTextToWrite(string valueText, object value)
            {
                if(value is JsonData jd)
                {
                    value = (JsonData)jd;
                }

                switch (valueText)
                {
                    case "integerValue":
                        //Integer is dangerous because it came as string of number
                        writer.Write(int.Parse((string)value));
                        break;
                    case "doubleValue":
                        writer.Write((double)value);
                        break;
                    case "booleanValue":
                        writer.Write((bool)value);
                        break;
                    case "arrayValue":
                        writer.WriteArrayStart();
                        JsonData al = (JsonData)((Dictionary<string, object>)value)["values"];
                        foreach(JsonData a in al)
                        {
                            //If you put array in array it may explode here
                            ValueTextToWrite(a.Keys.First(), a[a.Keys.First()].UnderlyingPrimitive());
                        }
                        writer.WriteArrayEnd();
                        break;
                    default:
                        //Debug.Log($"AHA {valueText} {value} {value?.GetType().Name}");
                        string casted = (string)value;
                        writer.Write(casted);
                        //Debug.Log($"AHAhh {valueText} {value}");
                        break;
                }
            }

            //var jo = JObject.Parse(jsonString);

            // if (jo.ContainsKey(nameof(name)) &&
            //     jo.ContainsKey(nameof(fields)))
            // {
            //     name = (string)jo[nameof(name)];
            //     fields = jo[nameof(fields)];
            //     properties = fields.Children<JProperty>();
            //     if (jo.ContainsKey(nameof(createTime)) &&
            //         jo.ContainsKey(nameof(updateTime)))
            //     {
            //         createTime = jo[nameof(createTime)].ToObject<DateTime>();
            //         updateTime = jo[nameof(updateTime)].ToObject<DateTime>();
            //     }
            //     else
            //     {
            //         createTime = default;
            //         updateTime = default;
            //     }
            // }
            // else
            // {
            //     throw new FirestormException($"This object is not a document! {jsonString}");
            // }
        }

        public override string ToString() => $"{document.name} : {document.createTime} {document.updateTime} Fields {document.fields.ToString()}";
    }

}