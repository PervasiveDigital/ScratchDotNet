//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PervasiveDigital.Scratch.DeploymentHelper.Extensibility.AddInSideAdapters
{
    
    public class IFirmataEngineContractToViewAddInAdapter : PervasiveDigital.Scratch.DeploymentHelper.Extensibility.IFirmataEngine
    {
        private PervasiveDigital.Scratch.DeploymentHelper.Extensibility.Contracts.IFirmataEngineContract _contract;
        private System.AddIn.Pipeline.ContractHandle _handle;
        static IFirmataEngineContractToViewAddInAdapter()
        {
        }
        public IFirmataEngineContractToViewAddInAdapter(PervasiveDigital.Scratch.DeploymentHelper.Extensibility.Contracts.IFirmataEngineContract contract)
        {
            _contract = contract;
            _handle = new System.AddIn.Pipeline.ContractHandle(contract);
        }
        public void ReportDigital(byte port, int value)
        {
            _contract.ReportDigital(port, value);
        }
        public void SendDigitalMessage(byte port, int value)
        {
            _contract.SendDigitalMessage(port, value);
        }
        public void SendExtendedMessage(byte command, byte[] data)
        {
            _contract.SendExtendedMessage(command, data);
        }
        internal PervasiveDigital.Scratch.DeploymentHelper.Extensibility.Contracts.IFirmataEngineContract GetSourceContract()
        {
            return _contract;
        }
    }
}

