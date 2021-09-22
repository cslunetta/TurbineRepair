using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace TurbineRepair
{
    public static class Turbines
    {
        const double revenuePerKW = 0.12;
        const double technicianCost = 250;
        const double turbineCost = 100;

        [FunctionName("TurbineRepair")]
        [OpenApiOperation(operationId: "Run")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody("application/json", typeof(RequestBodyModel), Description = "JSON request body containing { hours, capacity }")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response message containing a JSON result")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Get request body data
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            int? capacity = data?.capacity;
            int? hours = data?.hours;

            // Return bad request if capactiy or hours are not passed in
            if (capacity == null || hours == null)
                return new BadRequestObjectResult("Please pass capacity and hours in the request body");

            // Formulas to calculate revenue and cost
            double? revenueOpportunity = capacity * revenuePerKW * 24;
            double? costToFix = (hours * technicianCost) + turbineCost;    
            string repairTurbine = revenueOpportunity > costToFix
                ? "Yes"
                : "No";

            return new OkObjectResult(new
            {
                message = repairTurbine,
                revenueOpportunity = $"${revenueOpportunity}",
                costToFix = $"${costToFix}"
            });
        }
    }

    /// <summary>
    /// Model for what a request body should contain.
    /// </summary>
    public class RequestBodyModel
    {
        public int Hours { get; set; }
        public int Capacity { get; set; }
    }
}

