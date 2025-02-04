using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LeaveManagementPortal.Startup))]
namespace LeaveManagementPortal
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
