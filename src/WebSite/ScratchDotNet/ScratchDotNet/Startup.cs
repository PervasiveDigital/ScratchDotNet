using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ScratchDotNet.Startup))]
namespace ScratchDotNet
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
