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
    public class DynamicSingleValueProcedureRequest
    {
        public string ProcedureName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
