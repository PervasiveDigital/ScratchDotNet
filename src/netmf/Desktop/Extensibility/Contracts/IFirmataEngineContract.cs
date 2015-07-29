using System;
using System.AddIn.Contract;
using System.AddIn.Pipeline;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Extensibility.Contracts
{
    [AddInContract]
    public interface IFirmataEngineContract : IContract
    {
        void ReportDigital(byte port, int value);
    }
}
