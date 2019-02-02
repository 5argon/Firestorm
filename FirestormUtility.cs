using System;
using System.Collections.Generic;
using System.Reflection;
using E7.Firebase.LitJson;

namespace E7.Firebase
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
                    return ("mapValue", toFormat); //The whole value will be expanded into Json object
                    //throw new FirestormException($"Type {toFormat.GetType().Name} not supported!");
            }
        }

        public static string ToJsonDocument(object value, string fullDocumentPath)
        {
            var writer= new JsonWriter();
            writer.PrettyPrint = true;

            writer.WriteObjectStart();
            writer.WritePropertyName("name"); //document name
            writer.Write(fullDocumentPath);

            WriteFields(value);

            /// <summary>
            /// "fields" is not surrounded by object { } after this call
            /// </summary>
            void WriteFields(object v)
            {
                writer.WritePropertyName("fields");
                writer.WriteObjectStart();

                //REFLESIA OF ETERNITY
                if(v == null)
                {
                    throw new FirestormException($"Found null value in the object you are trying to make into a Json!");
                }

                var fields = v.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    //Debug.Log($"Propp {field.Name}");
                    writer.WritePropertyName(field.Name);
                    var fieldObject = field.GetValue(v);
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
                                if (formatted.typeString == "mapValue")
                                {
                                    writer.WritePropertyName(formatted.typeString);
                                    writer.WriteObjectStart();
                                    WriteFields(formatted.objectForJson);
                                    writer.WriteObjectEnd();
                                }
                                else
                                {
                                    writer.WritePropertyName(formatted.typeString);
                                    writer.WriteSmart(formatted.objectForJson);
                                }
                                break;
                        }
                        writer.WriteObjectEnd();
                    }
                }
                writer.WriteObjectEnd(); //fields
            } //WriteFields

            writer.WriteObjectEnd(); //top
            return writer.ToString();
        }
    }

}