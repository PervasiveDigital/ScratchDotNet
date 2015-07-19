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
    public class FirmataTargetDevice : TargetDevice
    {
        private string _name;
        private FirmataEngine _firmata;

        public FirmataTargetDevice(string name, FirmataEngine firmata)
        {
            _name = name;
            _firmata = firmata;
        }

        public override void Dispose()
        {
            if (_firmata != null)
            {
                _firmata.Dispose();
                _firmata = null;
            }
        }

        public override string DisplayName
        {
            get { return _name; }
        }

        public FirmataEngine Firmata
        {
            get { return _firmata; }
            set
            {
                if (_firmata != null)
                    throw new Exception("This device is already bound to a firmata engine");
            }
        }
    }
}
