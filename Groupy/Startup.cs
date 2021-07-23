using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Groupy.Startup))]
namespace Groupy
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
