using System;
using System.Collections.Generic;
using System.Reflection;

namespace E7.Firestorm
{

    public static class FirestormUtility
    {
        public static (string typeString, object objectForJson) FormatForValueJson(object toFormat)
        {
            switch (toFormat)
            {
                case DateTime dt:
                    dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    return ("timestampValue", dt.ToString("o"));
                case int i:
                    return ("integerValue", i.ToString());
                case double dbl:
                    return ("doubleValue", dbl);
                case string st:
                    return ("stringValue", st);
                case bool bl:
                    return ("booleanValue", bl);
                case List<object> lo:
                    return ("arrayValue", null);
                default:
                    throw new FirestormException($"Type {toFormat.GetType().Name} not supported!");
            }
        }

        public static string WriteJson<T>(T value, string fullDocumentPath)
        {
            var writer= new LitJson.JsonWriter();
            writer.PrettyPrint = true;

            writer.WriteObjectStart();
            writer.WritePropertyName("name");
            writer.Write(fullDocumentPath);
            writer.WritePropertyName("fields");
            writer.WriteObjectStart();

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
                    writer.WriteObjectStart();
                    var formatted = FirestormUtility.FormatForValueJson(obj);
                    switch (obj)
                    {
                        case List<object> lo:
                            writer.WritePropertyName(formatted.typeString);
                            writer.WriteObjectStart();
                            writer.WritePropertyName("values");
                            writer.WriteArrayStart();
                            foreach (object fromArray in lo)
                            {
                                //probably explode if you put List<object> in List<object>
                                WriterDecision(fromArray);
                            }
                            writer.WriteArrayEnd();
                            writer.WriteObjectEnd();
                            break;
                        default:
                            writer.WritePropertyName(formatted.typeString);
                            writer.WriteSmart(formatted.objectForJson);
                            break;
                    }
                    writer.WriteObjectEnd();
                }
            }

            writer.WriteObjectEnd(); //fields
            writer.WriteObjectEnd(); //top
            return writer.ToString();
        }
    }

}