using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public struct FirestormCollectionReference
{
    internal StringBuilder stringBuilder;

    private string parentDocument;
    private string collectionName;

    public FirestormCollectionReference(string name)
    {
        stringBuilder = new StringBuilder($"/{name}");
        parentDocument = "";
        collectionName = name;
    }

    public FirestormCollectionReference(FirestormDocumentReference sb, string name)
    {
        parentDocument = sb.stringBuilder.ToString();
        collectionName = name;
        this.stringBuilder = sb.stringBuilder;
        this.stringBuilder.Append($"/{name}");
    }

    public FirestormDocumentReference Document(string name) => new FirestormDocumentReference(this, name);

    /// <summary>
    /// Get all documents in the collection.
    /// Pagination, ordering, field masking, missing document, and transactions are not supported.
    /// </summary>
    public async Task<FirestormQuerySnapshot> GetSnapshotAsync()
    {
        var uwr = await FirestormConfig.Instance.UWRGet(stringBuilder.ToString());
        Debug.Log($"Getting query snapshot : {uwr.downloadHandler.text} {uwr.error}");
        return new FirestormQuerySnapshot(uwr.downloadHandler.text);
    }

    public async Task<FirestormDocumentReference> AddAsync<T>(T documentData) where T : class, new()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// The real C# API will be .WhereGreaterThan .WhereLessThan etc. methods. I am just too lazy to imitate them all.
    /// Runs against only immediate descendant documents of this collection.
    /// </summary>
    /// <summary>
    /// <param name="operationString">The same string as in Javascript API such as "==", "<", "<=", ">", ">=", "array_contains".
    /// They may add more than this in the future. "array_contains" was added later.</param>
    public async Task<FirestormQuerySnapshot> GetSnapshotAsync(params (string fieldName, string operationString, object target)[] queries)
    {
        RunQuery rq = default;

        var fieldFilters = new List<FieldFilter>();

        for (int i = 0; i < queries.Length; i++)
        {
            var query = queries[i];

            var filter = new FieldFilter();

            filter.field = new FieldReference { fieldPath = query.fieldName };
            filter.op = StringToOperator(query.operationString).ToString();
            var formatted = FirestormUtility.FormatForValueJson(query.target);
            filter.value = new Dictionary<string, object>
            {
                [formatted.typeString] = formatted.objectForJson,
            };
            fieldFilters.Add(filter);
        }

        //fieldFilters.Sort();
        //rq.structuredQuery.orderBy = fieldFilters.Select(x => new Order { field = x.field }).ToArray();

        if (queries.Length == 1)
        {
            rq.structuredQuery.where = new Dictionary<string, IFilter>
            {
                ["fieldFilter"] = fieldFilters[0],
            };
        }
        else if (queries.Length > 1)
        {
            CompositeFilter cf = new CompositeFilter
            {
                op = Operator.AND.ToString(),
                filters = fieldFilters.Select(x => new FilterForFieldFilter { fieldFilter = x }).ToArray(),
            };
            rq.structuredQuery.where = new Dictionary<string, IFilter>
            {
                ["compositeFilter"] = cf
            };
        }
        else
        {
            throw new FirestormException($"Query length {queries.Length} invalid!");
        }

        //Apparently REST API can query from more than 1 collection at once!
        //But since this class is a "CollectionReference", it should represent only this collection. So always 1 element in this array.
        rq.structuredQuery.from = new CollectionSelector[]{
                new CollectionSelector{
                    collectionId = collectionName,
                    allDescendants = false,
                }
            };

        string postJson = JsonConvert.SerializeObject(rq, Formatting.Indented);
        File.WriteAllText(Application.dataPath + $"/{UnityEngine.Random.Range(0, 100)}.txt", postJson);
        byte[] postData = Encoding.UTF8.GetBytes(postJson);

        //Path is the parent of this collection.
        var uwr = await FirestormConfig.Instance.UWRPost($"{parentDocument}:runQuery", null, postData);
        //File.WriteAllText(Application.dataPath + $"/{UnityEngine.Random.Range(0, 100)}.txt", uwr.downloadHandler.text);
        
        //Make the format looks like the one came back from "list" REST API
        var ja = JArray.Parse(uwr.downloadHandler.text);
        var newJo = JObject.FromObject(new 
        {
            documents = ja.Children<JObject>().Where(x => x.ContainsKey("document")).Select(x => x["document"].ToObject<object>())
        });
        //Debug.Log($"this is new jo {newJo.ToString()}");
        return new FirestormQuerySnapshot(newJo.ToString());
    }

    private struct RunQuery
    {
        public StructuredQuery structuredQuery;
    }

    private struct StructuredQuery
    {
        // public Projection select;

        //Only one of 3 types of filter allowed here
        public CollectionSelector[] from;
        public Dictionary<string, IFilter> where;
        //public Order[] orderBy;
    }

    private struct Order
    {
        public FieldReference field;
    }

    private struct CollectionSelector
    {
        public string collectionId;
        public bool allDescendants;
    }

    private struct CompositeFilter : IFilter
    {
        public string op;
        public FilterForFieldFilter[] filters; //Composite to unary or to composite not supported
    }

    private struct FilterForFieldFilter
    {
        public FieldFilter fieldFilter;
    }

    private struct FieldFilter : IFilter, IComparable<FieldFilter>
    {
        public FieldReference field;
        public string op;
        public Dictionary<string, object> value;

        public int CompareTo(FieldFilter other)
        {
            switch (op)
            {
                case ">":
                case "<":
                case ">=":
                case "<=":
                    //Range filter have to come first = this instance is lesser
                    return -1;
                default:
                    return this.field.fieldPath.CompareTo(other.field.fieldPath);
            }
        }
    }

    private struct FieldReference
    {
        public string fieldPath;
    }

    private Operator StringToOperator(string operatorString)
    {
        switch(operatorString)
        {
            case "==" : return Operator.EQUAL;
            case ">" : return Operator.GREATER_THAN;
            case "<" : return Operator.LESS_THAN;
            case ">=" : return Operator.GREATER_THAN_OR_EQUAL;
            case "<=" : return Operator.LESS_THAN_OR_EQUAL;
            case "array_contains" : return Operator.ARRAY_CONTAINS;
        }
        throw new FirestormException($"Operator {operatorString} not supported!");
    }

    private enum Operator
    {
        OPERATOR_UNSPECIFIED,
        AND, //for composite only
        LESS_THAN,
        LESS_THAN_OR_EQUAL,
        GREATER_THAN,
        GREATER_THAN_OR_EQUAL,
        EQUAL,
        ARRAY_CONTAINS,
        IS_NAN, //not supported
        IS_NULL, //not supported
    }

    private interface IFilter { }
}
