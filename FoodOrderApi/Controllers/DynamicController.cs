using Dapper;
using FoodOrderApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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


    }
}
