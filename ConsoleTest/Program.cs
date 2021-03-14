using Hbm.Api.Common;
using Hbm.Api.Common.Entities;
using Hbm.Api.Common.Entities.Channels;
using Hbm.Api.Common.Entities.Connectors;
using Hbm.Api.Common.Entities.Problems;
using Hbm.Api.Common.Exceptions;
using Hbm.Api.Common.Messaging;
using Hbm.Api.QuantumX.Connectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleTest
{
    class Program
    {
        public static DaqEnvironment daqEnvironment;
        private static DaqMeasurement daqMeasurement;
        private static List<Device> deviceList;
        private static Device myDevice;
        private static int index;
        private static bool isRun = true;
        private static bool isInt;

        static void Main(string[] args)
        {
            deviceList = new List<Device>();
            InitDaq();
            RegisterForEvents();
            //git test
            while (isRun)
            {
                ShowDevices();
                Console.WriteLine("Select device to conect to :");
                isInt = int.TryParse(Console.ReadLine(), out index);

                if (isInt)
                {
                    if (deviceList.Count >0)
                    {
                        ConnectToSellectedDevice(index);
                        GetSelectedDeviceConnectionTypesAndNames();
                    }
                    

                }
                else
                {
                    isRun = false;
                }
            }

            EndProgram();

        }

        private static void GetSelectedDeviceConnectionTypesAndNames()
        {
            try
            {
                foreach (Connector con in myDevice.Connectors)
                {
                    if (con is QuantumXOffConnector)
                    {
                        var x = con as QuantumXOffConnector;
                        
                        //just output the type of the connector. There are no channels and no signals in this case
                        AddToProtocol(string.Format("ConnectorType={0}", con.GetType()));
                        continue;
                    }
                    foreach (var channel in con.Channels)
                    {
                        if (channel is AnalogInChannel)
                        {
                            //AnalogInChannels do always have a sensor ...
                            AddToProtocol(string.Format("ConnectorType={0};  ChannelName={1}; SensorType={2}",
                                                   con.GetType(),
                                                   con.Channels[0].Name.Trim(),
                                                   (con.Channels[0] as AnalogInChannel).Sensor.SensorType.ToString()));
                        }
                        else
                        {
                            //output connector type and channel name
                            AddToProtocol(string.Format("ConnectorType={0};  ChannelName={1}", con.GetType(), channel.Name.Trim()));
                        }
                    }
                    
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message + " " + ex.ToString()); }
        }

        private static void InitDaq()
        {
            try
            {
                daqEnvironment = DaqEnvironment.GetInstance(); //DaqEnvironment is a singleton
                daqMeasurement = new DaqMeasurement();
                AddToProtocol("DaqEnvironment and DaqMeasurement objects initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.ToString());
            }
        }

        private static void RegisterForEvents()
        {
            try
            {
                //MessageBroker handles all events of the common API.
                MessageBroker.DeviceConnected += MessageBroker_DeviceConnected;
                MessageBroker.DeviceDisconnected += MessageBroker_DeviceDisconnected;
                AddToProtocol("Device event handlers registered");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message + " " + ex.ToString()); }
        }

        private static void ShowDevices()
        {
            try
            {
                // check, which device families are supported...
                List<string> supportedDeviceFamilies = daqEnvironment.GetAvailableDeviceFamilyNames();
                foreach (string family in supportedDeviceFamilies)
                {
                    AddToProtocol("Supported device family:" + family);
                }
                if (deviceList.Count <= 0)
                {
                    deviceList = daqEnvironment.Scan(supportedDeviceFamilies);
                }
                // scan for all supported device families

                // notice that the list of devices already has some information about the devices - 
                // although they are NOT yet connected. The information is delivered by the scan!

                //sort the list by device name
                deviceList = deviceList.OrderBy(n => n.Name).ToList();

                AddToProtocol(string.Format("Found devices:{0}", deviceList.Count));
                foreach (Device dev in deviceList)
                {

                    AddToProtocol(string.Format("Devicename: {0} Serialnumber: {1}  FirmwareVersion: {2}", dev.Name.PadRight(22), dev.SerialNo.PadRight(16), dev.FirmwareVersion));
                }



            }
            catch (Exception ex) { Console.WriteLine(ex.Message + " " + ex.ToString()); }
        }

        private static void ConnectToSellectedDevice(int index)
        {
            try
            {
                if (deviceList.Count > 0)
                {
                    List<Problem> problemList = new List<Problem>();
                    // pick the device that should be used from the scanned device list
                    myDevice = deviceList[index]; //connectionInfo of the device has been filled by the scan!
                    daqEnvironment.Connect(myDevice, out problemList);
                    // when a device is connected, the complete object representation of the device is available
                    // break here and check _deviceList[0] against e.g. _deviceList[1] to see the difference
                    AddToProtocol(string.Format("Device {0} is connected;  It has {1} connectors", myDevice.Name, myDevice.Connectors.Count));
                }
                else
                {
                    Console.WriteLine("No Devices Found! ");
                    Console.WriteLine();
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message + " " + ex.ToString()); }
        }

        private static void MessageBroker_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            AddToProtocol("Connected to device: " + e.UniqueDeviceID);
        }

        private static void MessageBroker_DeviceConnected(object sender, DeviceEventArgs e)
        {
            AddToProtocol("DisConnected from device: " + e.UniqueDeviceID);
        }

        private static void AddToProtocol(string msg)
        {
            Console.WriteLine(msg);
        }

        private static void EndProgram()
        {
            //Cleanup
            MessageBroker.DeviceConnected -= MessageBroker_DeviceConnected;
            MessageBroker.DeviceDisconnected -= MessageBroker_DeviceDisconnected;
            Console.WriteLine("Press eny button to quit");
            Console.Read();
        }

    }
}
