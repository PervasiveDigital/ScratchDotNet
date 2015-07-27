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
    public class FirmataEngineContractToViewHostSideAdapter
        : IFirmataEngine
    {
        private IFirmataEngineContract _contract;
        private System.AddIn.Pipeline.ContractHandle _handle;

        public FirmataEngineContractToViewHostSideAdapter(IFirmataEngineContract contract)
        {
            _contract = contract;
            _handle = new ContractHandle(contract);
        }
    }
}
