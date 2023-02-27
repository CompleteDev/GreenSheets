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
            XFont font = new XFont("Arial", 12,XFontStyle.Bold);
            XFont fontSmall = new XFont("Arial", 8);
            XFont fontMedBold = new XFont("Arial", 18, XFontStyle.Bold);
            XFont fontShimBold = new XFont("Arial", 65, XFontStyle.Bold);
            XFont fontBold = new XFont("Arial", 70, XFontStyle.Bold);
            XFont fontShipLarge = new XFont("Arial", 175, XFontStyle.Bold);

            XFont BarcodeFont = new XFont("fre3of9x", 50);

            XImage NBCLogo = XImage.FromFile("Logo/NBCLogo.png");

            gfx.DrawImage(NBCLogo, 20, 20, 85, 85);

            gfx.DrawString("Date: _____________________________", font, XBrushes.Black, new XPoint(325, 45));
            gfx.DrawString("Primary Table #: ___________________", font, XBrushes.Black, new XPoint(325, 85));
            gfx.DrawString("Start Time: _______________________", font, XBrushes.Black, new XPoint(325, 120));
            gfx.DrawString("End Time: _________________________", font, XBrushes.Black, new XPoint(325, 155));
            gfx.DrawString("Team:_______-_______-_______-______", font, XBrushes.Black, new XPoint(325, 190));
            gfx.DrawString("Junk: #___________ NOS: #___________", font, XBrushes.Black, new XPoint(325, 225));

            gfx.DrawString("*" + shipmentNumber + shipmentType + partNumber + "*", BarcodeFont, XBrushes.Black, new XPoint(330, 285));

            gfx.DrawString("NOTES:", fontMedBold, XBrushes.Black, new XPoint(325, 320));

            gfx.DrawString(shipmentType, fontShipLarge, XBrushes.Black, new XPoint(15, 600));
            gfx.DrawString(shipmentTypeName, fontMedBold, XBrushes.Black, new XPoint(80, 630));
            gfx.DrawString(shipmentNumber, fontShimBold, XBrushes.Black, new XPoint(20, 715));

            gfx.DrawString("ACCT: " + accountNumber, fontMedBold, XBrushes.Black, new XPoint(300, 575));
            gfx.DrawString(accountName, font, XBrushes.Black, new XPoint(300, 590));
            gfx.DrawString(accountAddress, font, XBrushes.Black, new XPoint(300, 602));
            gfx.DrawString(accountCity + "," + " " + accountState + " " + accountZip, font, XBrushes.Black, new XPoint(300, 614));


            gfx.DrawString("RCVD: " + recivedDate + " " + creatorInitials, fontMedBold, XBrushes.Black, new XPoint(360, 650));
            gfx.DrawString("SHIPMENT P/T: " + shipmentNumber + " " + partNumber, fontMedBold, XBrushes.Black, new XPoint(360, 670));
            gfx.DrawString("SHIPPER: " + shipper, fontMedBold, XBrushes.Black, new XPoint(360, 690));
            gfx.DrawString(pallets + " PALLETS", fontMedBold, XBrushes.Black, new XPoint(360, 710));
            gfx.DrawString(cartons + " CARTONS", fontMedBold, XBrushes.Black, new XPoint(360, 730));


            //gfx.DrawString(shipmentNumber , font, XBrushes.Black, new XPoint(120, 520));


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


            string responseMessage = string.IsNullOrEmpty(fileName)
                ? "This HTTP triggered function executed successfully."
                : fileName;
                

            return new OkObjectResult(responseMessage);
        }


    }   

}

