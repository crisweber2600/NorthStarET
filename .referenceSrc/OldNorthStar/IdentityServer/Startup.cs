using IdentityServer3.Core.Configuration;
using Microsoft.Owin;
using NorthStar4.IdentityServer.Configuration;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer.Configuration;
using Serilog;
using System.Configuration;

[assembly: OwinStartup(typeof(IdentityServer.Startup))]

namespace IdentityServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Trace(outputTemplate: "{Timestamp} [{Level}] ({Name}){NewLine} {Message}{NewLine}{Exception}")
                .CreateLogger();

            var factory = Factory.Configure();
            factory.ConfigureUserService(ConfigurationManager.ConnectionStrings["LoginConnection"].ConnectionString, ConfigurationManager.AppSettings["CORSOrigin"]);

            var idsrvOptions = new IdentityServerOptions
            {
                Factory = factory,
                SiteName = "North Star Login",
                SigningCertificate = Certificate.Load(),
                RequireSsl = false,
                EnableWelcomePage = false,
                EventsOptions = new EventsOptions
                {
                    RaiseErrorEvents = true,
                    RaiseFailureEvents = true,
                    RaiseInformationEvents = true,
                    RaiseSuccessEvents = true
                }
            };

            app.UseIdentityServer(idsrvOptions);
            //app.UseWelcomePage("/");
        }
    }
}
