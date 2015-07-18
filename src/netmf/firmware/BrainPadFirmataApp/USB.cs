using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware.UsbClient;
using System.Threading;

namespace USBDevice
{
    class USB
    {

        public static string SerialNumber = "FFFFFFFFFFF";

        private const int WRITE_EP = 1;
        private const int READ_EP = 2;
         
        static UsbStream usbStream = null;

        private static bool ConfigureUSBController(UsbController usbController)
        {


            bool bRet = false;


            // Create the device descriptor


            Configuration.DeviceDescriptor device = new Configuration.DeviceDescriptor(0xDEAD, 0x0110, 0x0200);
            device.bcdUSB = 0x110;
            device.bDeviceClass = 0xFF;     // Vendor defined class
            device.bDeviceSubClass = 0xFF;     // Vendor defined subclass
            device.bDeviceProtocol = 0;
            device.bMaxPacketSize0 = 8;        // Maximum packet size of EP0
            device.iManufacturer = 1;        // String #1 is manufacturer name (see string descriptors below)
            device.iProduct = 2;        // String #2 is product name
            device.iSerialNumber = 3;        // String #3 is the serial number

            // Create the endpoints
            Configuration.Endpoint writeEP = new Configuration.Endpoint(WRITE_EP, Configuration.Endpoint.ATTRIB_Write | Configuration.Endpoint.ATTRIB_Bulk | Configuration.Endpoint.ATTRIB_Synchronous);//Configuration.Endpoint.ATTRIB_Bulk | Configuration.Endpoint.ATTRIB_Write);
            writeEP.wMaxPacketSize = 64;
            writeEP.bInterval = 0;


            Configuration.Endpoint readEP = new Configuration.Endpoint(READ_EP, Configuration.Endpoint.ATTRIB_Read | Configuration.Endpoint.ATTRIB_Bulk | Configuration.Endpoint.ATTRIB_Synchronous);// Configuration.Endpoint.ATTRIB_Bulk | Configuration.Endpoint.ATTRIB_Read);
            readEP.wMaxPacketSize = 64;
            readEP.bInterval = 0;

            Configuration.Endpoint[] usbEndpoints = new Configuration.Endpoint[] { writeEP, readEP };

            // Set up the USB interface
            Configuration.UsbInterface usbInterface = new Configuration.UsbInterface(0, usbEndpoints);
            usbInterface.bInterfaceClass = 0xFF; // Vendor defined class
            usbInterface.bInterfaceSubClass = 0xFF; // Vendor defined subclass
            usbInterface.bInterfaceProtocol = 0;

            // Create array of USB interfaces
            Configuration.UsbInterface[] usbInterfaces = new Configuration.UsbInterface[] { usbInterface };

            // Create configuration descriptor
            Configuration.ConfigurationDescriptor config = new Configuration.ConfigurationDescriptor(200, usbInterfaces);

            // Create the string descriptors
            Configuration.StringDescriptor manufacturerName = new Configuration.StringDescriptor(1, "YourName");
            Configuration.StringDescriptor productName = new Configuration.StringDescriptor(2, "YourProductName");            
            Configuration.StringDescriptor serialNumber = new Configuration.StringDescriptor(3, "0000-0" + USB.SerialNumber.Substring(0, 3) + "-" + USB.SerialNumber.Substring(3, 4) + "-" + USB.SerialNumber.Substring(7, 4));
            Configuration.StringDescriptor displayName = new Configuration.StringDescriptor(4, "YourDisplayName");
            Configuration.StringDescriptor friendlyName = new Configuration.StringDescriptor(5, "YourFriendlyName");

            // Create the final configuration
            Configuration configuration = new Configuration();

            configuration.descriptors = new Configuration.Descriptor[] {
                                                                        device,
                                                                        config,
                                                                        manufacturerName,
                                                                        productName,
                                                                        serialNumber,
                                                                        displayName,
                                                                        friendlyName
                                                                        };
            try
            {
                // Set the configuration
                usbController.Configuration = configuration;

                if (UsbController.ConfigError.ConfigOK != usbController.ConfigurationError)
                {
                    throw new ArgumentException();
                }

                // If all ok, start the USB controller.
                bRet = usbController.Start();
            }
            catch (ArgumentException)
            {
                Debug.Print("Can't configure USB controller, error " + usbController.ConfigurationError.ToString());
            }


            return bRet;


        }

        public static bool Init()
        {

            // See if the hardware supports USB
            UsbController[] controllers = UsbController.GetControllers();
            // Bail out if USB is not supported on this hardware!
            if (0 == controllers.Length)
            {
                Debug.Print("USB is not supported on this hardware!");
                return false;
            }

            foreach (UsbController controller in controllers)
            {

                if (controller.Status != UsbController.PortState.Stopped)
                {
                    controller.Stop();
                    Thread.Sleep(1000);
                }
            }

            // Find a free USB controller
            UsbController usbController = null;
            foreach (UsbController controller in controllers)
            {
                if (controller.Status == UsbController.PortState.Stopped)
                {
                    usbController = controller;
                    break;
                }
            }
            // If no free USB controller
            if (null == usbController)
            {
                Debug.Print("All available USB controllers already in use. Set the device to use Ethernet or Serial debugging.");
                return false;
            }

            try
            {   // Configure the USB controller
                if (ConfigureUSBController(usbController))
                {
                    usbStream = usbController.CreateUsbStream(WRITE_EP, READ_EP);
                    usbStream.ReadTimeout = 60000;
                    usbStream.WriteTimeout = 60000;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                Debug.Print("USB stream could not be created, error " + usbController.ConfigurationError.ToString());
                return false;
            }

            return true;
        }
    }
}
