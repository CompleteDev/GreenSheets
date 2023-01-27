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
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Azure.Storage.Blobs;
using System;
using Azure.Storage.Sas;

namespace GreenSheetCreator
{
    public class CreateGreenSheet
    {
        private readonly ILogger<CreateGreenSheet> _logger;

        public CreateGreenSheet(ILogger<CreateGreenSheet> log)
        {
            _logger = log;
            
        }

        [FunctionName("GreenSheetCreate")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "GreenSheetCreate" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "shipmentType", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **ShipmentType** parameter")]
        [OpenApiParameter(name: "shipmentTypeName", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **shipmentTypeName** parameter")]
        [OpenApiParameter(name: "accountNumber", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **accountNumber** parameter")]
        [OpenApiParameter(name: "recivedDate", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "MM/DD/YY")]
        [OpenApiParameter(name: "creatorInitials", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **MDR** parameter")]
        [OpenApiParameter(name: "shipmentNumber", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **shipmentNumber** parameter")]
        [OpenApiParameter(name: "partNumber", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **partNumber** parameter")]
        [OpenApiParameter(name: "shipper", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **shipper** parameter")]
        [OpenApiParameter(name: "pallets", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **pallets** parameter")]
        [OpenApiParameter(name: "cartons", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **cartons** parameter")]
        [OpenApiParameter(name: "accountName", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **accontName** parameter")]
        [OpenApiParameter(name: "accountAddress", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **accountAddress** parameter")]
        [OpenApiParameter(name: "accountCity", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **accountCity** parameter")]
        [OpenApiParameter(name: "accountState", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **accountState** parameter")]
        [OpenApiParameter(name: "accountZip", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **accountZip** parameter")]



        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            MyFontResolver.Apply();

            string shipmentType = req.Query["shipmentType"];
            string shipmentTypeName = req.Query["shipmentTypeName"];
            string accountNumber = req.Query["accountNumber"];
            string recivedDate = req.Query["recivedDate"];
            string creatorInitials = req.Query["creatorInitials"];
            string shipmentNumber = req.Query["shipmentNumber"];
            string partNumber = req.Query["partNumber"];
            string shipper = req.Query["shipper"];
            string pallets = req.Query["pallets"];
            string cartons = req.Query["cartons"];
            string accountName = req.Query["accountName"];
            string accountAddress = req.Query["accountAddress"];
            string accountCity = req.Query["accountCity"];
            string accountState = req.Query["accountState"];
            string accountZip = req.Query["accountZip"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            shipmentType = shipmentType ?? data?.shipmentType;
            shipmentTypeName = shipmentTypeName ?? data?.shipmentTypeName;
            accountNumber = accountNumber ?? data?.accountNumber;
            recivedDate = recivedDate ?? data?.recivedDate;
            creatorInitials = creatorInitials ?? data?.creatorInitials;
            shipmentNumber = shipmentNumber ?? data?.shipmentNumber;
            partNumber = partNumber ?? data?.partNumber;
            shipper = shipper ?? data?.shipper;
            pallets = pallets ?? data?.pallets;
            cartons = cartons ?? data?.cartons;
            accountName = accountName ?? data?.accountName;
            accountAddress = accountAddress ?? data?.accountAddress;
            accountCity = accountCity ?? data?.accountCity;
            accountState = accountState ?? data?.accountState;
            accountZip = accountZip ?? data?.accountZip;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Arial", 18);
            XFont fontSmall = new XFont("Arial", 8);
            XFont fontBold = new XFont("Arial", 70, XFontStyle.Bold);
            XFont fontBoldSmall = new XFont("Arial", 18, XFontStyle.Bold);

            XFont BarcodeFont = new XFont("fre3of9x", 60);

            

            gfx.DrawString(shipmentType, fontBold, XBrushes.Black, new XPoint(475, 70));

            gfx.DrawString("Date: ___________________", font, XBrushes.Black, new XPoint(125, 35));
            gfx.DrawString("Primary Table #: ___________________", font, XBrushes.Black, new XPoint(40, 70));
            gfx.DrawString("Start Time: ___________________", font, XBrushes.Black, new XPoint(85, 105));
            gfx.DrawString("End Time: ___________________", font, XBrushes.Black, new XPoint(90, 140));
            gfx.DrawString("Team: _______ - _______ - _______ - _______", font, XBrushes.Black, new XPoint(118, 175));
            gfx.DrawString("Junk: #_______(?)  NIF: #_______ = $_______", font, XBrushes.Black, new XPoint(124, 210));

            gfx.DrawString("ACCT: " + accountNumber, font, XBrushes.Black, new XPoint(40, 275));
            gfx.DrawString(accountName, fontSmall, XBrushes.Black, new XPoint(40, 285));
            gfx.DrawString(accountAddress, fontSmall, XBrushes.Black, new XPoint(40, 295));
            gfx.DrawString(accountCity + "," + " " + accountState + " " + accountZip, fontSmall, XBrushes.Black, new XPoint(40, 305));


            gfx.DrawString("RCVD: " + recivedDate + " " + creatorInitials, font, XBrushes.Black,new XPoint(350, 275));
            gfx.DrawString("SHIPMENT P/T: " + shipmentNumber + " " + partNumber, font, XBrushes.Black, new XPoint(350, 290));
            gfx.DrawString("SHIPPER: " + shipper, font, XBrushes.Black, new XPoint(350, 305));
            gfx.DrawString(pallets + " PALLETS", font, XBrushes.Black, new XPoint(350, 320));
            gfx.DrawString(cartons + " CARTONS", font, XBrushes.Black, new XPoint(350, 335));

            gfx.DrawString("*" + shipmentNumber + "*", BarcodeFont, XBrushes.Black, new XPoint(40, 500));
            gfx.DrawString(shipmentNumber , font, XBrushes.Black, new XPoint(120, 520));
            gfx.DrawString("NOTES:", font, XBrushes.Black, new XPoint(40, 570));

            string dateNow = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = dateNow + "_" + shipmentNumber + ".pdf";
            string connectionString = Environment.GetEnvironmentVariable("BlobStorge");
            BlobContainerClient client = new BlobContainerClient(connectionString, Environment.GetEnvironmentVariable("BlobContainer"));
            var blob = client.GetBlobClient(fileName);
            using (MemoryStream ms = new MemoryStream())
            {
                document.Save(ms, false);
                ms.Position = 0;
                blob.Upload(ms);
            }

            //document.Save("C:\\GreenSheetTest\\GreenSheetTest.pdf");

            DateTimeOffset offset = new DateTimeOffset();
            offset.AddMinutes(10); 
            BlobSasBuilder temp = new BlobSasBuilder(BlobSasPermissions.Read, offset);
            temp.ExpiresOn = DateTime.Now.AddDays(10);
            var returnUri = blob.GenerateSasUri(temp);

            string responseMessage = string.IsNullOrEmpty(fileName)
                ? "This HTTP triggered function executed successfully."
                : returnUri.ToString();
                

            return new OkObjectResult(responseMessage);
        }


    }   

}

