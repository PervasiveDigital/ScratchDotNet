using System;
using System.AddIn.Pipeline;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PervasiveDigital.Scratch.DeploymentHelper.Extensibility.Contracts;
using PervasiveDigital.Scratch.DeploymentHelper.Extensibility.HostViewAdapters;

namespace PervasiveDigital.Scratch.DeploymentHelper.Extensibility.HostSideAdapters
{
    public class DriverContractToViewHostSideAdapter
        : IDriver
    {
        private IDriverContract _contract;
        private System.AddIn.Pipeline.ContractHandle _handle;

        public DriverContractToViewHostSideAdapter(IDriverContract contract)
        {
            _contract = contract;
            _handle = new ContractHandle(contract);
        }

        public void Start(IFirmataEngine firmataEngine)
        {
        }

        public void Stop()
        {
            _contract.Stop();
        }

        public Dictionary<string, string> GetSensorValues()
        {
            return _contract.GetSensorValues();
        }
    }
}
