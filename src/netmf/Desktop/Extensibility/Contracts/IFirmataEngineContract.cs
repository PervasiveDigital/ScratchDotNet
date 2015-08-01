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
        /// <summary>
        /// Request reporting of the state of <paramref name="port"/>.
        /// </summary>
        /// <param name="port">The port to be monitored</param>
        /// <param name="value">The bits within <paramref name="port" to be monitored/></param>
        void ReportDigital(byte port, int value);

        /// <summary>
        /// Send a digital command to the board
        /// </summary>
        /// <param name="port">port to send the command to</param>
        /// <param name="value">value to be set/sent</param>
        void SendDigitalMessage(byte port, int value);

        void SendExtendedMessage(byte command, byte[] data);
    }
}
