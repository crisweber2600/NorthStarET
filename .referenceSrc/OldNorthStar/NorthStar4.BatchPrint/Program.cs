using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Client;
using Thinktecture.IdentityModel.Extensions;
using Winnovative;

namespace NorthStar4.BatchPrint
{
    public static class Constants
    {
        public const string BaseAddress = "http://localhost:16725/identity";

        public const string AuthorizeEndpoint = BaseAddress + "/connect/authorize";
        public const string LogoutEndpoint = BaseAddress + "/connect/endsession";
        public const string TokenEndpoint = BaseAddress + "/connect/token";
        public const string UserInfoEndpoint = BaseAddress + "/connect/userinfo";
        public const string IdentityTokenValidationEndpoint = BaseAddress + "/connect/identitytokenvalidation";
        public const string TokenRevocationEndpoint = BaseAddress + "/connect/revocation";

        public const string AspNetWebApiSampleApi = "http://localhost:16724/#/linegraph/1";
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
            var response = RequestToken();
            ShowResponse(response);
            pdfConverter.HttpRequestCookies.Add("Authorization", "Bearer " + response.AccessToken);

            CallService(response.AccessToken);
            Console.ReadLine();
    

        }

        static TokenResponse RequestToken()
        {
            var client = new OAuth2Client(
                new Uri(Constants.TokenEndpoint),
                "roclient",
                "secret");

            // idsrv supports additional non-standard parameters 
            // that get passed through to the user service
            var optional = new Dictionary<string, string>
            {
                { "acr_values", "onbehalfof:kerri.town@isd196.org" }
            };

            return client.RequestResourceOwnerPasswordAsync("northstar.shannon@gmail.com", "dammit", "idmgr", optional).Result;
        }

        static PdfConverter pdfConverter = new PdfConverter();
        private static void PDFGen(ref Document finalDoc, string url)
        {
            Document pdfDocument = pdfConverter.GetPdfDocumentObjectFromUrl(url);
            pdfConverter.HttpPostFields.Add("jsonPostData", "JSONFieldSettings");
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
        static void CallService(string token)
        {
            var baseAddress = Constants.AspNetWebApiSampleApi;
            //pdfConverter.HttpRequestHeaders.Add("Authorization", "Bearer " + token);
           // pdfConverter.he
            pdfConverter.ConversionDelay = 5;
            pdfConverter.LicenseKey = ConfigurationSettings.AppSettings["pdfKey"];
            Document finalDoc = null;

            PDFGen(ref finalDoc, baseAddress);

            finalDoc.Save(string.Format("{0}.pdf", @"c:\linegraph"));

            finalDoc.DetachStream();
            finalDoc.Close();
            //var client = new HttpClient
            //{
            //    BaseAddress = new Uri(baseAddress)
            //};

            //client.SetBearerToken(token);
            //var response = client.GetStringAsync("").Result;

            //"\nLookupFields:".ConsoleGreen();

            //// write to file
            //FileStream ostrm;
            //StreamWriter writer;
            //TextWriter oldOut = Console.Out;
            //try
            //{
            //    ostrm = new FileStream(@"c:\linegraph.html", FileMode.OpenOrCreate, FileAccess.Write);
            //    writer = new StreamWriter(ostrm);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Cannot open Redirect.txt for writing");
            //    Console.WriteLine(e.Message);
            //    return;
            //}
            //Console.SetOut(writer);
            //Console.WriteLine(response);
            //Console.SetOut(oldOut);
            //writer.Close();
            //ostrm.Close();
            Console.WriteLine("Complete");
        }

        private static void ShowResponse(TokenResponse response)
        {
            if (!response.IsError)
            {
                "Token response:".ConsoleGreen();
                Console.WriteLine(response.Json);

                if (response.AccessToken.Contains("."))
                {
                    "\nAccess Token (decoded):".ConsoleGreen();

                    var parts = response.AccessToken.Split('.');
                    var header = parts[0];
                    var claims = parts[1];

                    Console.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(header))));
                    Console.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(claims))));
                }
            }
            else
            {
                if (response.IsHttpError)
                {
                    "HTTP error: ".ConsoleGreen();
                    Console.WriteLine(response.HttpErrorStatusCode);
                    "HTTP error reason: ".ConsoleGreen();
                    Console.WriteLine(response.HttpErrorReason);
                }
                else
                {
                    "Protocol error response:".ConsoleGreen();
                    Console.WriteLine(response.Json);
                }
            }
        }
    }
}
