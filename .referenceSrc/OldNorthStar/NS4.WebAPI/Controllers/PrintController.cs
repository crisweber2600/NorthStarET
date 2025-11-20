using EntityDto.DTO.Misc;
using System.Security.Claims;
using System.Web.Http;
using Northstar.Core;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
//using Thinktecture.IdentityModel.Client;
using Winnovative;
using NorthStar4.Infrastructure;
using Winnovative.HtmlToPdfClient;
using System.Configuration;

namespace NorthStar4.API.api
{
    [RoutePrefix("api/Print")]
    [Authorize]
    public class PrintController : NSBaseController
    {
        private Winnovative.HtmlToPdfClient.HtmlToPdfConverter pdfConverter = null;
        private PdfConverter pdfConverterLocal = new PdfConverter();
        private TokenManager tokenMgr = new TokenManager();
        
        //private NSBaseDataService dataService = null;

        public PrintController()
        {
            uint ServerPort = uint.Parse(ConfigurationManager.AppSettings["PdfServerPort"]);
            pdfConverter = new Winnovative.HtmlToPdfClient.HtmlToPdfConverter(ConfigurationManager.AppSettings["PdfServerIp"], ServerPort);
            //dataService = new NSBaseDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
        }

        [Route("PrintPageLocal")]
        [HttpPost]
        public HttpResponseMessage PrintPageLocal([FromBody] InputDto_PrintSettings input)
        {
            var token = Request.Headers.Authorization.Parameter;
            pdfConverterLocal.HttpRequestCookies.Add("Authorization", "Bearer " + token);
            pdfConverterLocal.PdfDocumentOptions.ShowHeader = false;
            pdfConverterLocal.PdfDocumentOptions.ShowFooter = false;
            pdfConverterLocal.PdfDocumentOptions.LeftMargin = 20;
            pdfConverterLocal.PdfDocumentOptions.RightMargin = 20;
            pdfConverterLocal.PdfDocumentOptions.TopMargin = 20;
            pdfConverterLocal.PdfDocumentOptions.BottomMargin = 20;
            pdfConverterLocal.PdfDocumentOptions.JpegCompressionEnabled = false;
            pdfConverterLocal.PdfDocumentOptions.JpegCompressionLevel = 0;
            pdfConverterLocal.PdfDocumentOptions.PdfPageOrientation = input.PrintLandscape == true ? Winnovative.PdfPageOrientation.Landscape : Winnovative.PdfPageOrientation.Portrait;
            

            pdfConverterLocal.JavaScriptEnabled = true;
            pdfConverterLocal.PdfDocumentOptions.SinglePage = !(input.PrintMultiPage ?? false);

            if (input.PrintMultiPage != true)
            {
                pdfConverterLocal.PdfDocumentOptions.FitWidth = input.FitWidth ?? true;
                pdfConverterLocal.PdfDocumentOptions.FitHeight = input.FitHeight ?? true;
                pdfConverterLocal.PdfDocumentOptions.StretchToFit = input.StretchToFit ?? true;
            } else
            {
                pdfConverterLocal.PdfDocumentOptions.FitWidth = input.FitWidth ?? false;
                pdfConverterLocal.PdfDocumentOptions.FitHeight = input.FitHeight ?? false;
                pdfConverterLocal.PdfDocumentOptions.StretchToFit = input.StretchToFit ?? false;
            }

            if (input.PrintLandscape == true)
            {
                pdfConverterLocal.HtmlViewerWidth = input.HtmlViewerWidth ?? 1950;
            } else
            {
                pdfConverterLocal.HtmlViewerWidth = input.HtmlViewerWidth ?? 1300;
                if (input.HtmlViewerHeight != null)
                {
                    pdfConverterLocal.HtmlViewerHeight = input.HtmlViewerHeight.Value;
                }
            }

            if (input.ForcePortraitPageSize)
            {
                pdfConverterLocal.PdfDocumentOptions.PdfPageSize = new Winnovative.PdfPageSize(612, 1350);
            }


            QueryStringHelper qHelper = new QueryStringHelper(input.Url);
            //qHelper.SetParameter("SchoolId", input.SchoolId.ToString());
            //qHelper.SetParameter("SchoolYear", input.SchoolYear.ToString());
            //qHelper.SetParameter("GradeId", input.GradeId.ToString());
            //qHelper.SetParameter("TeacherId", input.TeacherId.ToString());
            //qHelper.SetParameter("SectionId", input.SectionId.ToString());
            //qHelper.SetParameter("StudentId", input.StudentId.ToString());
            qHelper.SetParameter("SortParam", input.SortParam);
            if (!string.IsNullOrEmpty(input.SourceBenchmarkDate)) { qHelper.SetParameter("SourceBenchmarkDate", input.SourceBenchmarkDate); }
            if (!string.IsNullOrEmpty(input.GroupsParam)) { qHelper.SetParameter("GroupsParam", input.GroupsParam); };
            if (!string.IsNullOrEmpty(input.SummaryDataParam)) { qHelper.SetParameter("SummaryDataParam", input.SummaryDataParam); };
            if (!string.IsNullOrEmpty(input.GroupsArrayParam)) { qHelper.SetParameter("GroupsArrayParam", input.GroupsArrayParam); };
            qHelper.SetParameter("printmode", "1");

            //pdfConverterLocal.HttpRequestHeaders.Add("Authorization", "Bearer " + token);
            // pdfConverterLocal.he
            pdfConverterLocal.ConversionDelay = 5;
            pdfConverterLocal.LicenseKey = "jgAQARABExEVARAVDxEBEhAPEBMPGBgYGAER";
            Winnovative.Document finalDoc = null;

            PDFGen(ref finalDoc, qHelper.All);

            byte[] bytes = finalDoc.Save();
            //System.IO.File.WriteAllBytes(@"c:\normal.pdf", bytes);
            //finalDoc.Save(string.Format("{0}.pdf", @"c:\linegraph"));

            finalDoc.DetachStream();
            finalDoc.Close();
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(bytes);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }

        [Route("PrintPage")]
        [HttpPost]
        public HttpResponseMessage PrintPage([FromBody] InputDto_PrintSettings input)
        {
            var token = Request.Headers.Authorization.Parameter;
            pdfConverter.HttpRequestCookies.Add("Authorization", "Bearer " + token);
            pdfConverter.PdfDocumentOptions.ShowHeader = false;
            pdfConverter.PdfDocumentOptions.ShowFooter = false;
            pdfConverter.PdfDocumentOptions.LeftMargin = 20;
            pdfConverter.PdfDocumentOptions.RightMargin = 20;
            pdfConverter.PdfDocumentOptions.TopMargin = 20;
            pdfConverter.PdfDocumentOptions.BottomMargin = 20;
            pdfConverter.PdfDocumentOptions.JpegCompressionEnabled = false;
            pdfConverter.PdfDocumentOptions.JpegCompressionLevel = 0;
            pdfConverter.PdfDocumentOptions.PdfPageOrientation = input.PrintLandscape == true ? Winnovative.HtmlToPdfClient.PdfPageOrientation.Landscape : Winnovative.HtmlToPdfClient.PdfPageOrientation.Portrait;
            pdfConverter.PdfDocumentOptions.SinglePage = !(input.PrintMultiPage ?? false);

            pdfConverter.JavaScriptEnabled = true;
            //pdfConverterLocal.InternetSecurityZone = InternetSecurityZone.Intranet;

            if (input.PrintMultiPage != true)
            {
                pdfConverter.PdfDocumentOptions.FitWidth = input.FitWidth ?? true;
                pdfConverter.PdfDocumentOptions.FitHeight = input.FitHeight ?? true;
                pdfConverter.PdfDocumentOptions.StretchToFit = input.StretchToFit ?? true;
            }
            else
            {
                pdfConverter.PdfDocumentOptions.FitWidth = input.FitWidth ?? false;
                pdfConverter.PdfDocumentOptions.FitHeight = input.FitHeight ?? false;
                pdfConverter.PdfDocumentOptions.StretchToFit = input.StretchToFit ?? false;
            }

            if (input.PrintLandscape == true)
            {
                pdfConverter.HtmlViewerWidth = input.HtmlViewerWidth ?? 1950;
            }
            else
            {
                pdfConverter.HtmlViewerWidth = input.HtmlViewerWidth ?? 1300;
                if (input.HtmlViewerHeight != null)
                {
                    pdfConverter.HtmlViewerHeight = input.HtmlViewerHeight.Value;
                }
            }

            if (input.ForcePortraitPageSize)
            {
                pdfConverter.PdfDocumentOptions.PdfPageSize = new Winnovative.HtmlToPdfClient.PdfPageSize(612, 1350);
            }


            QueryStringHelper qHelper = new QueryStringHelper(input.Url);
            //qHelper.SetParameter("SchoolId", input.SchoolId.ToString());
            //qHelper.SetParameter("SchoolYear", input.SchoolYear.ToString());
            //qHelper.SetParameter("GradeId", input.GradeId.ToString());
            //qHelper.SetParameter("TeacherId", input.TeacherId.ToString());
            //qHelper.SetParameter("SectionId", input.SectionId.ToString());
            //qHelper.SetParameter("StudentId", input.StudentId.ToString());
            qHelper.SetParameter("SortParam", input.SortParam);
            if (!string.IsNullOrEmpty(input.SourceBenchmarkDate)) { qHelper.SetParameter("SourceBenchmarkDate", input.SourceBenchmarkDate); }
            if (!string.IsNullOrEmpty(input.GroupsParam)) { qHelper.SetParameter("GroupsParam", input.GroupsParam); };
            if (!string.IsNullOrEmpty(input.SummaryDataParam)) { qHelper.SetParameter("SummaryDataParam", input.SummaryDataParam); };
            if (!string.IsNullOrEmpty(input.GroupsArrayParam)) { qHelper.SetParameter("GroupsArrayParam", input.GroupsArrayParam); };
            qHelper.SetParameter("printmode", "1");

            //pdfConverter.HttpRequestHeaders.Add("Authorization", "Bearer " + token);
            // pdfConverter.he
            pdfConverter.ConversionDelay = 10;
            pdfConverter.LicenseKey = "jgAQARABExEVARAVDxEBEhAPEBMPGBgYGAER";
            pdfConverter.NavigationTimeout = 300;
            //Document finalDoc = null;

            //PDFGen(ref finalDoc, qHelper.All);

            byte[] bytes = null;
            //System.IO.File.WriteAllBytes(@"c:\normal.pdf", bytes);
            //finalDoc.Save(string.Format("{0}.pdf", @"c:\linegraph"));
            bytes = pdfConverter.ConvertUrl(qHelper.All);
            //finalDoc.DetachStream();
            //finalDoc.Close();
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(bytes);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }



        private void PDFGen(ref Winnovative.Document finalDoc, string url)
        {
            pdfConverterLocal.NavigationTimeout = 300;
            Winnovative.Document pdfDocument = pdfConverterLocal.GetPdfDocumentObjectFromUrl(url);
            pdfConverterLocal.HttpPostFields.Add("jsonPostData", "JSONFieldSettings");
            // first document becomes doc
            if (finalDoc == null)
            {
                finalDoc = pdfDocument;
            }
            else
            {
                finalDoc.AppendDocument(pdfDocument);
            }
        }
    }
}
