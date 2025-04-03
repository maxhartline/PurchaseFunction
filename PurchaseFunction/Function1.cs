using System;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace PurchaseFunction
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function(nameof(Function1))]
        public async Task Run([QueueTrigger("purchases", Connection = "AzureWebJobsStorage")] QueueMessage message)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");

            // Deserialize the message into a Purchase object
            var options = new JsonSerializerOptions // use this to make case insensitive
            {
                PropertyNameCaseInsensitive = true
            };

            var purchase = JsonSerializer.Deserialize<Purchase>(message.MessageText, options); // this is now case insensetivie

            // Null check, log error
            if (purchase == null)
            {
                _logger.LogError("Failed to deserialize the message into a Purchase object.");
                return;       
            }

            // Insert the purchase into a database

            // PASTED FROM BRIGHTSPACE HERE
            // get connection string from app settings
            string? connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("SQL connection string is not in the environment variables.");
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync(); // Note the async
                var query = "INSERT INTO dbo.Purchases (concertId, Email, Name, Phone, Quantity, CreditCard, Expiration, SecurityCode, Address, City, Province, PostalCode, Country) " +
                    "VALUES (@concertId, @Email, @Name, @Phone, @Quantity, @CreditCard, @Expiration, @SecurityCode, @Address, @City, @Province, @PostalCode, @Country)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@concertId", purchase.concertId);
                    cmd.Parameters.AddWithValue("@Email", purchase.Email);
                    cmd.Parameters.AddWithValue("@Name", purchase.Name);
                    cmd.Parameters.AddWithValue("@Phone", purchase.Phone);
                    cmd.Parameters.AddWithValue("@Quantity", purchase.Quantity);
                    cmd.Parameters.AddWithValue("@CreditCard", purchase.CreditCard);
                    cmd.Parameters.AddWithValue("@Expiration", purchase.Expiration);
                    cmd.Parameters.AddWithValue("@SecurityCode", purchase.SecurityCode);
                    cmd.Parameters.AddWithValue("@Address", purchase.Address);
                    cmd.Parameters.AddWithValue("@City", purchase.City);
                    cmd.Parameters.AddWithValue("@Province", purchase.Province);
                    cmd.Parameters.AddWithValue("@PostalCode", purchase.PostalCode);
                    cmd.Parameters.AddWithValue("@Country", purchase.Country);

                    await cmd.ExecuteNonQueryAsync(); // Note the async
                }
            }
        }
    }
}
