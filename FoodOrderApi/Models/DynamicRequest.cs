namespace FoodOrderApi.Models
{

    // نموذج الطلب للإجراء
    public class DynamicProcedureRequest
    {
        public string ProcedureName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }

    // نموذج الطلب للإجراءات التي ترجع جدول
    public class DynamicFunctionRequest
    {
        public string FunctionName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }

    // نموذج الطلب للإجراءات التي ترجع قيمة واحدة
    public class DynamicQueryRequest
    {
        public string TableName { get; set; }
        public List<string> Columns { get; set; }
        public Dictionary<string, object> WhereClause { get; set; } // where clause column and its value

    }
    public class DynamicJoinQueryRequest
    {
        public string TableName { get; set; }
        public List<string> Columns { get; set; }
        public List<JoinTable> JoinTables { get; set; } // List of join tables
        public Dictionary<string, object> WhereClause { get; set; } // Filtering conditions
        public List<string> GroupBy { get; set; } // Columns to group by
    }

    public class JoinTable
    {
        public string TableName { get; set; } // Name of the table to join
        public string JoinOn { get; set; } // Join condition
    }

}
