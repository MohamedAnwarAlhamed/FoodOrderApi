using Dapper;
using FoodOrderApi.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Dapper.SqlMapper;


namespace FoodOrderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DynamicController : ControllerBase
    {
        private readonly IDbConnection _dbConnection;

        public DynamicController(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // Action 1: استدعاء إجراء وإرجاع النتائج
        [HttpPost("ExecuteProcedure")]
        public async Task<IActionResult> ExecuteProcedure([FromBody] DynamicProcedureRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ProcedureName))
            {
                return BadRequest("Procedure name is required.");
            }

            var parameters = new DynamicParameters();
            foreach (var param in request.Parameters)
            {
                if (param.Value is JsonElement jsonElement)
                {
                    // Convert JsonElement to its appropriate type
                    switch (jsonElement.ValueKind)
                    {
                        case JsonValueKind.String:
                            parameters.Add($"@{param.Key}", jsonElement.GetString());
                            break;
                        case JsonValueKind.Number:
                            if (jsonElement.TryGetInt32(out int intValue))
                            {
                                parameters.Add($"@{param.Key}", intValue);
                            }
                            else if (jsonElement.TryGetDecimal(out decimal decimalValue))
                            {
                                parameters.Add($"@{param.Key}", decimalValue);
                            }
                            break;
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            parameters.Add($"@{param.Key}", jsonElement.GetBoolean());
                            break;
                        default:
                            return BadRequest($"Unsupported parameter type for {param.Key}");
                    }
                }
                else
                {
                    parameters.Add($"@{param.Key}", param.Value);
                }
            }


            try
            {
                var result = await _dbConnection.QueryAsync<dynamic>(request.ProcedureName, parameters, commandType: CommandType.StoredProcedure);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // سجل الخطأ مع تفاصيل إضافية
                return StatusCode(500, $"Internal server error: {ex.Message} | Procedure: {request.ProcedureName} | Parameters: {JsonSerializer.Serialize(parameters)}");
            }
        }

        [HttpPost("ExecuteFunction")]
        public async Task<IActionResult> ExecuteFunction([FromBody] DynamicFunctionRequest request)
        {
            // Validate request
            if (string.IsNullOrEmpty(request.FunctionName))
            {
                return BadRequest("Function name is required.");
            }

            try
            {
                // Create a dynamic parameter object for Dapper
                var dynamicParameters = new DynamicParameters();
                List<string> parameterValues = new List<string>();

                foreach (var param in request.Parameters)
                {
                    if (param.Value is JsonElement jsonElement)
                    {
                        // Handle JsonElement type conversion
                        switch (jsonElement.ValueKind)
                        {
                            case JsonValueKind.String:
                                parameterValues.Add($"'{jsonElement.GetString()}'");
                                break;
                            case JsonValueKind.Number:
                                if (jsonElement.TryGetInt32(out int intValue))
                                {
                                    parameterValues.Add($"{intValue}");
                                }
                                else if (jsonElement.TryGetDecimal(out decimal decimalValue))
                                {
                                    parameterValues.Add($"{decimalValue}");
                                }
                                break;
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                parameterValues.Add(jsonElement.GetBoolean() ? "1" : "0");
                                break;
                            default:
                                return BadRequest($"Unsupported parameter type for {param.Key}");
                        }
                    }
                    else
                    {
                        // For non-JsonElement types, handle directly
                        parameterValues.Add($"{param.Value}");
                    }
                }

                string query = $"SELECT dbo.{request.FunctionName}({string.Join(", ", parameterValues)})";

                // Execute the query
                var result = await _dbConnection.QueryAsync(query);
                return Ok(result);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("ExecuteTableFunction")]
        public async Task<IActionResult> ExecuteTableFunction([FromBody] DynamicFunctionRequest request)
        {
            // Validate request
            if (string.IsNullOrEmpty(request.FunctionName))
            {
                return BadRequest("Function name is required.");
            }

            try
            {

                // Create a dynamic parameter object for Dapper
                var dynamicParameters = new DynamicParameters();
                List<string> parameterValues = new List<string>();

                foreach (var param in request.Parameters)
                {
                    if (param.Value is JsonElement jsonElement)
                    {
                        // Handle JsonElement type conversion
                        switch (jsonElement.ValueKind)
                        {
                            case JsonValueKind.String:
                                parameterValues.Add($"'{jsonElement.GetString()}'");
                                break;
                            case JsonValueKind.Number:
                                if (jsonElement.TryGetInt32(out int intValue))
                                {
                                    parameterValues.Add($"{intValue}");
                                }
                                else if (jsonElement.TryGetDecimal(out decimal decimalValue))
                                {
                                    parameterValues.Add($"{decimalValue}");
                                }
                                break;
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                parameterValues.Add(jsonElement.GetBoolean() ? "1" : "0");
                                break;
                            default:
                                return BadRequest($"Unsupported parameter type for {param.Key}");
                        }
                    }
                    else
                    {
                        // For non-JsonElement types, handle directly
                        parameterValues.Add($"{param.Value}");
                    }
                }

                string query = $"SELECT * FROM dbo.{request.FunctionName}({string.Join(", ", parameterValues)})";

                // Execute the query
                var result = await _dbConnection.QueryAsync(query);
                return Ok(result);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("executeQuery")]
        public async Task<IActionResult> ExecuteQuery([FromBody] DynamicQueryRequest request)
        {
            // Validate request
            if (string.IsNullOrEmpty(request.TableName) || request.Columns == null || request.Columns.Count == 0)
            {
                return BadRequest("Table name and columns are required.");
            }
            var parameters = new DynamicParameters();
            foreach (var whereClauseItem in request.WhereClause)
            {
                if (whereClauseItem.Value is JsonElement jsonElement)
                {
                    // Convert JsonElement based on its type
                    switch (jsonElement.ValueKind)
                    {
                        case JsonValueKind.String:
                            parameters.Add(whereClauseItem.Key, jsonElement.GetString());
                            break;
                        case JsonValueKind.Number:
                            if (jsonElement.TryGetInt32(out int intValue))
                            {
                                parameters.Add(whereClauseItem.Key, intValue);
                            }
                            else if (jsonElement.TryGetDecimal(out decimal decimalValue))
                            {
                                parameters.Add(whereClauseItem.Key, decimalValue);
                            }
                            break;
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            parameters.Add(whereClauseItem.Key, jsonElement.GetBoolean());
                            break;
                        case JsonValueKind.Null:
                            parameters.Add(whereClauseItem.Key, null);
                            break;
                        // Handle other types like arrays, objects, etc., if needed
                        default:
                            return BadRequest($"Unsupported JsonElement type: {jsonElement.ValueKind}");
                    }
                }
                else
                {
                    // If it's not a JsonElement, directly add it as a parameter
                    parameters.Add(whereClauseItem.Key, whereClauseItem.Value);
                }
            }
            // Build the dynamic query
            string columns = string.Join(", ", request.Columns.Select(c => $"[{c}]"));
            string whereClause = string.Empty;

            if (request.WhereClause != null && request.WhereClause.Any())
            {
                whereClause = "WHERE " + string.Join(" AND ", request.WhereClause.Select(w => $"[{w.Key}] = @{w.Key}"));
            }

            string query = $"SELECT {columns} FROM [{request.TableName}] {whereClause};";


            var result = await _dbConnection.QueryAsync(query, parameters);

            return Ok(result);

        }

        [HttpPost("executeJoinQuery")]
        public async Task<IActionResult> ExecuteJoinQuery([FromBody] DynamicJoinQueryRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.TableName))
            {
                return BadRequest("Request body is missing or table name is required.");
            }

            // Validate the request
            if (request.Columns == null || request.Columns.Count == 0)
            {
                return BadRequest("Columns are required.");
            }

            // Initialize query components
            var parameters = new DynamicParameters();
            var queryBuilder = new StringBuilder($"SELECT {string.Join(", ", request.Columns.Select(c => $"{c}"))} FROM {request.TableName}");

            // Add JOINs if specified
            if (request.JoinTables != null && request.JoinTables.Count > 0)
            {
                foreach (var join in request.JoinTables)
                {
                    queryBuilder.Append($" inner JOIN {join.TableName} ON {join.JoinOn}");
                }
            }

            // Build WHERE clause if provided
            if (request.WhereClause != null && request.WhereClause.Any())
            {
                var whereConditions = new List<string>();
                foreach (var whereClauseItem in request.WhereClause)
                {
                    // Ensure the value is converted to the appropriate type
                    if (whereClauseItem.Value is JsonElement jsonElement)
                    {
                        // Convert JsonElement based on its type
                        switch (jsonElement.ValueKind)
                        {
                            case JsonValueKind.String:
                                parameters.Add(whereClauseItem.Key, jsonElement.GetString());
                                break;
                            case JsonValueKind.Number:
                                if (jsonElement.TryGetInt32(out int intValue))
                                {
                                    parameters.Add(whereClauseItem.Key, intValue);
                                }
                                else if (jsonElement.TryGetDecimal(out decimal decimalValue))
                                {
                                    parameters.Add(whereClauseItem.Key, decimalValue);
                                }
                                break;
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                parameters.Add(whereClauseItem.Key, jsonElement.GetBoolean());
                                break;
                            case JsonValueKind.Null:
                                parameters.Add(whereClauseItem.Key, null);
                                break;
                            // Handle other types like arrays, objects, etc., if needed
                            default:
                                return BadRequest($"Unsupported JsonElement type: {jsonElement.ValueKind}");
                        }
                    }
                    whereConditions.Add($"{whereClauseItem.Key} = @{whereClauseItem.Key}");
                }
                queryBuilder.Append($" WHERE {string.Join(" AND ", whereConditions)}");
            }
            var result = await _dbConnection.QueryAsync(queryBuilder.ToString(), parameters);

                // If no results were returned, return an empty array
                if (!result.Any())
                {
                    return Ok(new List<dynamic>()); // Return an empty array if no results
                }

                // Get all columns excluding those in GroupBy for later use
                var nonGroupByColumns = request.Columns
                    .Where(column => !request.GroupBy.Contains(column))
                    .ToList();

            var groupedResult = result
    .GroupBy(r => new
    {  // r.RestaurantId
        GroupKey = string.Join("&", request.GroupBy.Select(column =>
        ((IDictionary<string, object>)r).ContainsKey(column) ?
        ((IDictionary<string, object>)r)[column]?.ToString() : string.Empty)) // Safely access properties
       //GroupKey = string.Join(",", request.GroupBy.Select(column => r[column]))
        //GroupKey = string.Join(",", request.GroupBy.Select(column =>
        //{
        //    if (((IDictionary<string, object>)r).ContainsKey(column))
        //    {
        //        var value = ((IDictionary<string, object>)r)[column];
        //        return value == null ? "N/A" : Convert.ToString(value); // Handle null and convert
        //    }
        //    return "N/A"; // Default for missing keys
        //}))

        //var key = new ExpandoObject() as IDictionary<string, object>;

        //    // Create individual keys for each column in GroupBy
        //    foreach (var column in request.GroupBy)
        //    {
        //        if (((IDictionary<string, object>)r).ContainsKey(column))
        //        {
        //            var value = ((IDictionary<string, object>)r)[column];
        //            key[column] = value != null ? Convert.ToString(value) : "N/A"; // Handle null values
        //        }
        //        else
        //        {
        //            key[column] = "N/A"; // Default for missing keys
        //        }
        //    }

        //    return key;
        })
    .Select(g =>
    {
        dynamic resultfinal = new ExpandoObject();
        //resultfinal.GroupKey = g.Key.GroupKey;
        //foreach (var column in request.GroupBy)
        //{

        //    //((IDictionary<string, object>)resultfinal)[column] = g.Key.GroupKey;
        //}
        for (int i = 0; i < request.GroupBy.Count; i++)
        {
            if (!((IDictionary<string, object>)resultfinal).ContainsKey(request.GroupBy[i]))
            {
                ((IDictionary<string, object>)resultfinal).Add(request.GroupBy[i], g.Key.GroupKey.Split("&")[i]);
            }
           // ((IDictionary<string, object>)resultfinal)[request.GroupBy[i]] = g.Key.GroupKey.Split(",")[i];
            

        }

        

        // Add remaining columns
        var Information = g.Select(x =>
        {
            dynamic result2 = new ExpandoObject();
            var infoDict = (IDictionary<string, object>)result2;

            foreach (var column in request.Columns)
            {
                if (!request.GroupBy.Contains(column))
                {
                    // Add the first value from each non-grouped column
                    if (((IDictionary<string, object>)x).ContainsKey(column))
                    {
                        infoDict[column] = ((IDictionary<string, object>)x)[column]; // Access properties safely
                    }
                }
            }
            return result2;
        }).ToList();

        resultfinal.Information = Information;

        return resultfinal;
    }).ToList();


            return Ok(groupedResult);
            }

        }
}
