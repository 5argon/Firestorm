using System;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Hacky and painful JSON parsing to C#
/// </summary>
public class DocumentConverter<T> : JsonConverter<T>
where T : class, new()
{
    private string fullDocumentPath;
    public DocumentConverter(string fullDocumentPath)
    {
        this.fullDocumentPath = fullDocumentPath;
    }

    public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        T result = new T();
        if (reader.TokenType == JsonToken.StartObject)
        {
            reader.Read();
            do
            {
                //Debug.Log($"{reader.TokenType} {reader.Value} {reader.ValueType?.Name} {objectType.Name}");
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        {
                            GatherAndReflectIntoPropertyName(reader, (string)reader.Value, result);
                            break;
                        }
                }

            }
            while (reader.TokenType != JsonToken.EndObject);
        }
        return result;
    }

    private void GatherAndReflectIntoPropertyName(JsonReader reader, string propertyName, T t)
    {
        reader.Read(); // at opening bracket
        reader.Read(); // at "???value"

        string name = (string)reader.Value;

        Debug.Log($"Processing {propertyName} value string {name}");

        FieldInfo field = typeof(T).GetField(propertyName, BindingFlags.Instance | BindingFlags.Public);

        if(field == null)
        {
            //propertyName that came from the server does not match anything.. we left it as is
            reader.Read(); // at the real value now
            reader.Read(); // at EndObject
            reader.Read(); // at the next start field or the real EndObject
            return;
        }

        switch (name)
        {
            case "integerValue":
                reader.Read(); // at the real value now
                //lol int is a string from Google's server
                Debug.Log($"INT {reader.Value}");
                field.SetValue(t, int.Parse(((string)reader.Value)));
                break;
            case "nullValue":
            case "booleanValue":
            case "doubleValue":
            case "timestampValue":
            case "stringValue":
            case "bytesValue":
            case "referenceValue":
                //Stupid reflection set and hope that it matches
                reader.Read(); // at the real value now
                //Debug.Log($"Valued {reader.Value} type {reader.ValueType.Name}");
                field.SetValue(t, reader.Value);
                break;

            case "arrayValue":
                var array = new List<object>();
                reader.Read(); // at start object of array
                reader.Read(); // at "values"
                reader.Read(); // at StartArray
                reader.Read(); // at StartObject 
                do
                {
                    //Debug.Log($"YEA {reader.TokenType}");
                    reader.Read(); // at ???Value
                    string valueText = (string)reader.Value;
                    //Debug.Log($"YEA {reader.TokenType}");
                    reader.Read(); // at the real object now
                                   //Debug.Log($"YEA {reader.TokenType} ADD {reader.Value} tyoe {reader.ValueType}");

                    switch (valueText)
                    {
                        case "geoPointValue":
                            throw new FirestormException($"geoPointValue type not supported! Sorry!");
                        case "mapValue":
                            throw new FirestormException($"mapValue type not supported! Sorry!");
                        case "integerValue":
                            //Int is a string again lol
                            array.Add(int.Parse((string)reader.Value));
                            break;
                        default:
                            //Since it is a list of object we simply add and let user take risk of casting themselves.. yeah!
                            array.Add(reader.Value);
                            break;
                    }
                    reader.Read(); // at EndObject
                    Debug.Log($"YEA {reader.TokenType}");
                    reader.Read(); //Next StartObject or end array
                    Debug.Log($"YEA2 {reader.TokenType}");
                }
                while (reader.TokenType != JsonToken.EndArray && reader.TokenType != JsonToken.None);
                //set the list by reflection
                field.SetValue(t, array);
                reader.Read(); //EndObject of the array type as a whole
                break;
            case "geoPointValue":
                throw new FirestormException($"geoPointValue type not supported! Sorry!");
            case "mapValue":
                throw new FirestormException($"mapValue type not supported! Sorry!");
            default:
                throw new FirestormException($"Did not expecting token type {reader.TokenType} named {name}!");
        }
        reader.Read(); // at EndObject
        Debug.Log($"YEA3 {reader.TokenType}");
        reader.Read(); // at the next start field or the real EndObject
        Debug.Log($"YEA4 {reader.TokenType}");
    }

    public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("name");
        writer.WriteValue(fullDocumentPath);
        writer.WritePropertyName("fields");
        writer.WriteStartObject();

        //REFLESIA OF ETERNITY
        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            //Debug.Log($"Propp {field.Name}");
            writer.WritePropertyName(field.Name);
            var fieldObject = field.GetValue(value);
            WriterDecision(fieldObject);

            void WriterDecision(object obj)
            {
                writer.WriteStartObject();
                var formatted = FirestormUtility.FormatForValueJson(obj);
                switch (obj)
                {
                    case List<object> lo:
                        writer.WritePropertyName(formatted.typeString);
                        writer.WriteStartObject();
                        writer.WritePropertyName("values");
                        writer.WriteStartArray();
                        foreach (object fromArray in lo)
                        {
                            //probably explode if you put List<object> in List<object>
                            WriterDecision(fromArray);
                        }
                        writer.WriteEndArray();
                        writer.WriteEndObject();
                        break;
                    default:
                        writer.WritePropertyName(formatted.typeString);
                        writer.WriteValue(formatted.objectForJson);
                        break;
                }
                writer.WriteEndObject();
            }
        }

        writer.WriteEndObject(); //fields
        writer.WriteEndObject(); //top
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;
}
