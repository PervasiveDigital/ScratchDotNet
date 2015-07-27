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
    public class DriverViewToContractAddInSideAdapter
        : ContractBase, IDriverContract
    {
        private IDriver _view;

        public DriverViewToContractAddInSideAdapter(IDriver view) 
        {
            _view = view;
        }

        public void Start(IFirmataEngineContract firmataEngine)
        {
            _view.Start(firmataEngine);
        }

        public void Stop()
        {
            _view.Stop();
        }

        public Dictionary<string, string> GetSensorValues()
        {
            return _view.GetSensorValues();
        }
    }
}
