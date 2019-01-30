using System;
using System.Collections.Generic;

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
                return ("stringValue" , st);
            case bool bl:
                return ("booleanValue", bl);
            case List<object> lo:
                return ("arrayValue", null);
            default:
                throw new FirestormException($"Type {toFormat.GetType().Name} not supported!");
        }

    }
}
