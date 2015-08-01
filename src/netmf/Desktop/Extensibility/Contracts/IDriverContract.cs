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
    public interface IDriverContract : IContract
    {
        /// <summary>
        /// Called once when this device is selected as the current target for Scratch programs
        /// </summary>
        /// <param name="firmataEngine"></param>
        void Start(IFirmataEngineContract firmataEngine);

        /// <summary>
        /// Called once when we are de-selecting this as the current target for Scratch programs
        /// </summary>
        void Stop();

        /// <summary>
        /// Indicates the top of a program loop or cycle - may be called many times in succession
        /// </summary>
        void StartOfProgram();

        void ExecuteCommand(string verb, string id, IListContract<string> args);

        Dictionary<string, string> GetSensorValues();

        void ProcessDigitalMessage(int port, int value);

        void ProcessAnalogMessage(int port, int value);

        void ProcessExtendedMessage(byte command, byte[] data);
    }
}
