using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace AzureFunctions
{
    public static class EmailLicenseFile
    {
        [FunctionName("EmailLicenseFile")]
        public static void Run(
            [BlobTrigger("licenses/{orderId}.lic", Connection = "AzureWebJobsStorage")] string licenseFileContent,
            [SendGrid(ApiKey = "SendGridApiKey")] ICollector<SendGridMessage> sender,
            [Table("orders", "orders", "{orderId}")] Order order,
            string orderId,
            ILogger log)
        {
            log.LogInformation($"Got order from {order.Email}\nOrder Id: {orderId}");

            var message = new SendGridMessage
            {
                From = new EmailAddress(Environment.GetEnvironmentVariable("EmailSender")),
                Subject = "Your license file",
                HtmlContent = "Thank you for your order"
            };
            message.AddTo(order.Email);
            var plainTextBytes = Encoding.UTF8.GetBytes(licenseFileContent);
            var base64 = Convert.ToBase64String(plainTextBytes);
            message.AddAttachment($"{orderId}.lic", base64, "text/plain");

            if (!order.Email.EndsWith("@test.com"))
            {
                sender.Add(message);
            }
        }
    }
}
