//-----------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
//
// This work is licensed under the Creative Commons 
//    Attribution-ShareAlike 4.0 International License.
// http://creativecommons.org/licenses/by-sa/4.0/
//
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ninject.Modules;
using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Server;

namespace PervasiveDigital.Scratch.DeploymentHelper
{
    public class AppModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ILogger>().To<Logger>().InSingletonScope();
            this.Bind<Models.DeviceModel>().ToSelf().InSingletonScope();

            // The device server
            this.Bind<DeviceServer>().ToSelf().InSingletonScope();
        }
    }
}
