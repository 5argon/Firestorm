using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;

[Serializable]
public struct FirestormDocumentSnapshot
{
    public string name;
    public DateTime createTime;
    public DateTime updateTime;
    public JToken fields;
    public IEnumerable<JProperty> properties;
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
        var serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            Converters = new JsonConverter[] { new DocumentConverter<T>(name) },
        }
        );
        return fields.ToObject<T>(serializer);
    }

    /// <summary>

    /// DocumentMask : https://firebase.google.com/docs/firestore/reference/rest/v1beta1/DocumentMask
    /// </summary>
    internal string FieldsDocumentMaskJson()
    {
        if (IsEmpty)
        {
            return JsonConvert.SerializeObject(new DocumentMask());
        }
        else
        {
            var mask = new DocumentMask { fieldPaths = properties.Select(x => x.Name).ToArray() };
            return JsonConvert.SerializeObject(mask);
        }
    }

    public FirestormDocumentSnapshot(string jsonString)
    {
        Debug.Log($"Snapshottt from {jsonString}");
        IsEmpty = false;
        var jo = JObject.Parse(jsonString);
        if (jo.ContainsKey(nameof(name)) &&
            jo.ContainsKey(nameof(fields)))
        {
            name = jo[nameof(name)].ToObject<string>();
            fields = jo[nameof(fields)];
            properties = jo.Properties();
            if (jo.ContainsKey(nameof(createTime)) &&
                jo.ContainsKey(nameof(updateTime)))
            {
                createTime = jo[nameof(createTime)].ToObject<DateTime>();
                updateTime = jo[nameof(updateTime)].ToObject<DateTime>();
            }
            else
            {
                createTime = default; 
                updateTime = default; 
            }
        }
        else
        {
            throw new FirestormException($"This object is not a document! {jsonString}");
        }
    }

    public override string ToString() => $"{name} : {createTime} {updateTime} JSON String {fields.ToString()}";
}

[Serializable]
public struct FirestormDocumentValue
{
}