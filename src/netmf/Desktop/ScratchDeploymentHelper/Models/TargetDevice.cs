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

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using PervasiveDigital.Scratch.DeploymentHelper.Firmata;
using PervasiveDigital.Scratch.DeploymentHelper.Common;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public abstract class TargetDevice : BindableBase, IDisposable
    {
        public abstract void Dispose();

        public abstract string DisplayName { get; }
    }
}
