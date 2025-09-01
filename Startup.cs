using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HR_Administration_System.Startup))]
namespace HR_Administration_System
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
