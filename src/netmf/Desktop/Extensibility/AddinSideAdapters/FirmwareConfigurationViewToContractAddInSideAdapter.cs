using System;
using System.AddIn.Pipeline;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PervasiveDigital.Scratch.DeploymentHelper.Extensibility.Contracts;
using PervasiveDigital.Scratch.DeploymentHelper.Extensibility.AddInViews;

namespace PervasiveDigital.Scratch.DeploymentHelper.Extensibility.AddinSideAdapters
{
    [AddInAdapter]
    public class FirmwareConfigurationViewToContractAddInSideAdapter
        : ContractBase, IFirmwareConfigurationContract
    {
        private IFirmwareConfiguration _view;

        public FirmwareConfigurationViewToContractAddInSideAdapter(IFirmwareConfiguration view) 
        {
            _view = view;
        }

        public void Configure()
        {
            _view.Configure();
        }
    }
}
