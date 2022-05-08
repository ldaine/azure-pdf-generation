using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PdfGeneration.HtmlToPdf;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace PdfGeneration.AzureFunctions
{
    public static class GeneratePdfFunctions
    {
        [FunctionName(nameof(GeneratePdf))]
        [StorageAccount("AzureWebJobsStorage")]
        public static async Task<IActionResult> GeneratePdf(
            [HttpTrigger(
            AuthorizationLevel.Function, "post", 
            Route = "htmltopdf/{pdfGenerator}/{fileName}")] 
            HttpRequest req,
            string pdfGenerator,
            string fileName,
            [Blob("generated-pdf-files")] BlobContainerClient container,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            log.LogInformation($"fileName: {fileName}");

            await container.CreateIfNotExistsAsync();

            var blobFileName = $"selectpdf-{fileName}.pdf";
            var blobClient = container.GetBlobClient(blobFileName);
            if (blobClient.Exists())
            {
                return new ConflictObjectResult("File with the given file name already exists");
            }

            string content = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(content))
            {
                return new BadRequestResult();
            }

            Stream stream = pdfGenerator switch
            {
                PdfGeneratorType.SelectPdf => HtmlToPdfWithSelectPdf.Convert(content),
                _ => throw new System.NotImplementedException()
            };

            await CreateBlob(blobClient, stream);

            stream.Dispose();

            log.LogInformation($"PDF with name '{blobFileName}' generated");

            return new OkObjectResult("PDF generated");
        }

        #region Helper Methods
        private static async Task CreateBlob(BlobClient blobClient, Stream stream)
        {
            blobClient.Upload(stream);

            // Get the existing properties
            BlobProperties properties = await blobClient.GetPropertiesAsync();

            BlobHttpHeaders headers = new BlobHttpHeaders
            {
                // Set the MIME ContentType every time the properties 
                // are updated or the field will be cleared
                ContentType = "application/pdf",

                // Populate remaining headers with 
                // the pre-existing properties
                CacheControl = properties.CacheControl,
                ContentDisposition = properties.ContentDisposition,
                ContentEncoding = properties.ContentEncoding,
                ContentHash = properties.ContentHash
            };

            // Set the blob's properties.
            await blobClient.SetHttpHeadersAsync(headers);
        }
        #endregion
    }
}
