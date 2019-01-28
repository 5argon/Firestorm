using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;


[Serializable]
public struct FirestormDocumentSnapshot
{
    public string name;
    public DateTime createTime;
    public DateTime updateTime;
    public JToken fields;

    public T ConvertTo<T>() where T : class, new()
    => fields.ToObject<T>(JsonSerializer.Create(new JsonSerializerSettings
    {
        Converters = new JsonConverter[] { new DocumentConverter<T>(name) },
    }
    ));

    public FirestormDocumentSnapshot(string jsonString)
    {
        var jo = JObject.Parse(jsonString);
        if (jo.ContainsKey(nameof(name)) &&
            jo.ContainsKey(nameof(fields)))
        {
            name = jo[nameof(name)].ToObject<string>();
            fields = jo[nameof(fields)];
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