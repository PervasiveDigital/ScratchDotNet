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
    
    public class IFirmwareConfigurationContractToViewAddInAdapter : PervasiveDigital.Scratch.DeploymentHelper.Extensibility.IFirmwareConfiguration
    {
        private PervasiveDigital.Scratch.DeploymentHelper.Extensibility.Contracts.IFirmwareConfigurationContract _contract;
        private System.AddIn.Pipeline.ContractHandle _handle;
        static IFirmwareConfigurationContractToViewAddInAdapter()
        {
        }
        public IFirmwareConfigurationContractToViewAddInAdapter(PervasiveDigital.Scratch.DeploymentHelper.Extensibility.Contracts.IFirmwareConfigurationContract contract)
        {
            _contract = contract;
            _handle = new System.AddIn.Pipeline.ContractHandle(contract);
        }
        public void Configure()
        {
            _contract.Configure();
        }
        internal PervasiveDigital.Scratch.DeploymentHelper.Extensibility.Contracts.IFirmwareConfigurationContract GetSourceContract()
        {
            return _contract;
        }
    }
}

