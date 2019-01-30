using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

public struct FirestormCollection
{
    internal StringBuilder stringBuilder;
    public FirestormCollection(string name)
    {
        stringBuilder = new StringBuilder($"/{name}");
    }

    public FirestormCollection(FirestormDocument sb, string name)
    {
        this.stringBuilder = sb.stringBuilder;
        this.stringBuilder.Append($"/{name}");
    }

    public FirestormDocument Document(string name) => new FirestormDocument(this, name);

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

    public async Task<FirestormDocument> AddAsync<T>(T documentData) where T : class, new()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// The real C# API will be .WhereGreaterThan .WhereLessThan etc. methods. I am just too lazy to imitate them all.
    /// Runs against only immediate descendant documents of this collection.
    /// </summary>
    public async Task<FirestormQuerySnapshot> GetSnapshotAsync(params (string fieldName, string operationString, object target)[] queries)
    {
        throw new NotImplementedException();
        //Make IFilter
        if(queries.Length == 1)
        {
            var query = queries[0];
            var filter = new FieldFilter();
            filter.field = new FieldReference { fieldPath = query.fieldName };
            filter.op = StringToOperator(query.operationString);
            var formatted = FirestormUtility.FormatForValueJson(query.target);
            filter.value = new Dictionary<string, object>
            {
                
            };
        }
        else
        {
            
        }

        string postJson = "";
        byte[] postData = Encoding.UTF8.GetBytes(postJson);
        var uwr = await FirestormConfig.Instance.UWRPost($"{stringBuilder.ToString()}:runQuery", null, postData);
    }

    private struct RunQuery
    {
        public StructuredQuery structuredQuery;
    }

    private struct StructuredQuery
    {
        // public Projection select;
        // public CollectionSelector from;

        //Only one of 3 types of filter allowed here
        public Dictionary<string, IFilter> where;
    }

    private struct CompositeFilter
    {
        public Operator op;
        public IFilter[] filters;
    }

    private struct FieldFilter
    {
        public FieldReference field;
        public Operator op;
        public Dictionary<string, object> value;
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
