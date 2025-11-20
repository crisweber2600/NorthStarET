using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(NS4.Client.Startup))]
namespace NS4.Client
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
        }
    }
}
