using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace NS4.WebAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            NorthStar.EF6.Infrastructure.DtoMappings.Map();

            HttpConfiguration config = GlobalConfiguration.Configuration;
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
            config.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Trace(Serilog.Events.LogEventLevel.Verbose)
                .WriteTo.AzureTableStorage(ConfigurationManager.AppSettings["AzureTableStorageConnectionString"], Serilog.Events.LogEventLevel.Verbose, null, "NS4WebApiLog")
                .CreateLogger();

            //JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            //{
            //    //DateParseHandling = DateParseHandling.DateTime,
            //    DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind
            //    //Formatting = Newtonsoft.Json.Formatting.Indented,
            //    //TypeNameHandling = TypeNameHandling.Objects,
            //    //ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            //};
        }
    }
}
