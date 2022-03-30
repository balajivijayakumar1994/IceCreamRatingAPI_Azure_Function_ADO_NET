using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Web.Http;

namespace IceCreamRatingAPI
{
    public static class Function1
    {
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("CreateRatings")]
        public static async Task<IActionResult> CreateRating(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");



            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UserRating data = JsonConvert.DeserializeObject<UserRating>(requestBody);

            var userAPI = "https://serverlessohapi.azurewebsites.net/api/GetUser?userId=" + data.userId;
            var userResponse = await httpClient.GetAsync(userAPI);

            if (userResponse.StatusCode == HttpStatusCode.BadRequest)
            {
                return new BadRequestErrorMessageResult("Enter valid userId");
            }
            var productAPI = "https://serverlessohapi.azurewebsites.net/api/GetProduct?productId=" + data.productId;
            var productAPIResponse = httpClient.GetAsync(productAPI);

            if (productAPIResponse.Result.StatusCode == HttpStatusCode.BadRequest)
            {
                return new BadRequestErrorMessageResult("Enter valid productId..............");
            }


            if (data.rating < 0 || data.rating > 5)
            {
                return new BadRequestErrorMessageResult("Enter valid rating between 0 to 5.");
            }



            data.id = Guid.NewGuid().ToString();
            data.timestamp = DateTime.Now;
            var responseMessage = JsonConvert.SerializeObject(data);

            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var text = $"EXEC dbo.InsertRatingDetails '{data.id}','{responseMessage}'";

                using (SqlCommand cmd = new SqlCommand(text, conn))
                {
                    // Execute the command and log the # rows affected.
                    var rows = await cmd.ExecuteNonQueryAsync();
                    log.LogInformation($"{rows} rows were updated");
                }
            }

            return new OkObjectResult(responseMessage);
        }


        [FunctionName("GetRating")]
        public static async Task<IActionResult> GetRatingByRatingId(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string id = req.Query["id"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            id = id ?? data?.id;

            if (string.IsNullOrEmpty(id))
            {
                return new BadRequestErrorMessageResult("Enter rating id");
            }

            var jsonData = string.Empty;
            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var text = $"select Data from Ratings where RatingId='{id}'";
                SqlCommand command = new SqlCommand(text, conn);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {

                        jsonData = reader.GetString(0);


                    }
                }
            }

            if (string.IsNullOrWhiteSpace(jsonData))
                return new NotFoundObjectResult("NOT FOUND");
            else
                return new OkObjectResult(jsonData);
        }

        [FunctionName("GetRatings")]
        public static async Task<IActionResult> GetRatingsbyUserId(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string userId = req.Query["userId"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            userId = userId ?? data?.productId;

            if (string.IsNullOrEmpty(userId))
            {
                return new BadRequestErrorMessageResult("Enter userId");
            }

            List<UserRating> resultlist = new List<UserRating>();
            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var text = $"select Data from Ratings where JSON_VALUE(Data, '$.userId') = '{userId}'";
                SqlCommand command = new SqlCommand(text, conn);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        log.LogInformation(reader[0].ToString());
                        var jsondata = reader.GetString(0);

                        var ratingobject = JsonConvert.DeserializeObject<UserRating>(jsondata);
                        resultlist.Add(ratingobject);

                    }
                }
            }

            var responseMessage = JsonConvert.SerializeObject(resultlist);

            if (string.IsNullOrWhiteSpace(responseMessage))
                return new NotFoundObjectResult("NOT FOUND");
            else
                return new OkObjectResult(responseMessage);
        }
    }
}
