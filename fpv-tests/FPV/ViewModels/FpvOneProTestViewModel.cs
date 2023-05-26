using FPV.Core;
using FPV.Service.Common;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FPV.ViewModels
{
    public class FpvOneProTestViewModel : ViewModelBase
    {
        public ICommand NavigateHomeCommand { get; set; }

        #region Properties


        #endregion

        #region SERIAL PORT FOR ABSTRACTION

        private SerialPort _serialPort;

        private string _serialPortReceivedMessage;
        public string SerialPortReceivedMessage
        {
            get { return _serialPortReceivedMessage; }
            set { _serialPortReceivedMessage = value; OnPropertyChanged(); }
        }


        // Button content for serial port connection onyl for GUI
        private string _serialPortConnectionStatusMessage = "Connect";
        public string SerialPortConnectionStatusMessage
        {
            get { return _serialPortConnectionStatusMessage; }
            set { _serialPortConnectionStatusMessage = value; OnPropertyChanged(); }
        }

        // Message from user to serial port
        private string _serialPortCommand;
        public string SerialPortCommand
        {
            get { return _serialPortCommand; }
            set { _serialPortCommand = value; OnPropertyChanged(); }
        }


        public ICommand SerialPortTryToConnectCommand { get; private set; }
        public ICommand SerialPortSendMessageCommand { get; private set; }


        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort?.IsOpen == true)
            {
                string data = _serialPort.ReadExisting();


                // Update Dispatcher property on GUI thred
                if (!string.IsNullOrEmpty(data))
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        SerialPortReceivedMessage += data;
                        MyParser(SerialPortReceivedMessage);

                    });
                }
            }
        }

        #endregion

        #region SERIAL PORT DATA PARSER

        public class JSONData
        {
            public string response { get; set; }
            public string id { get; set; }
            public object result { get; set; }
        }

        public class ResultData
        {
            public string yaw { get; set; }
            public string pitch { get; set; }
            public string roll { get; set; }
        }

        private int FindClosingBraceIndex(string input, int openingBraceIndex)
        {
            int closingBraceIndex = openingBraceIndex;
            int braceCount = 0;

            while (closingBraceIndex < input.Length)
            {
                if (input[closingBraceIndex] == '{')
                {
                    braceCount++;
                }
                else if (input[closingBraceIndex] == '}')
                {
                    braceCount--;
                    if (braceCount == 0)
                        return closingBraceIndex;
                }

                closingBraceIndex++;

                if (closingBraceIndex >= input.Length)
                    break;

                if (input[closingBraceIndex] == '{')
                {
                    int nestedClosingBraceIndex = FindClosingBraceIndex(input, closingBraceIndex);
                    if (nestedClosingBraceIndex != -1)
                        closingBraceIndex = nestedClosingBraceIndex + 1;
                }
            }

            return -1;
        }

        public List<JSONData> FindObjectsWithProperties(string input)
        {
            List<JSONData> foundObjects = new List<JSONData>();

            int startIndex = 0;
            while (startIndex < input.Length)
            {
                int openingBraceIndex = input.IndexOf("{", startIndex);
                if (openingBraceIndex == -1)
                    break;

                int closingBraceIndex = FindClosingBraceIndex(input, openingBraceIndex);
                if (closingBraceIndex == -1)
                    break;

                string jsonObject = input.Substring(openingBraceIndex, closingBraceIndex - openingBraceIndex + 1);

                JSONData data = JsonConvert.DeserializeObject<JSONData>(jsonObject);
                if (data != null && data.response != null && data.id != null)
                {
                    if (data.result is JArray)
                    {
                        // Parse result as an array of objects
                        data.result = ((JArray)data.result).ToObject<List<ResultData>>();
                    }

                    foundObjects.Add(data);
                }

                startIndex = closingBraceIndex + 1;
            }

            return foundObjects;
        }
       
        private void MyParser(string stringToParse)
        {
            List<JSONData> foundObjects = FindObjectsWithProperties(stringToParse);
            foreach (JSONData data in foundObjects)
            {
                Console.WriteLine("****************************************************");
                Console.WriteLine($"response: {data.response}, id: {data.id}");

                if (data.result is List<ResultData>)
                {
                    foreach (var resultItem in (List<ResultData>)data.result)
                    {
                        Console.WriteLine($"yaw: {resultItem.yaw}, pitch: {resultItem.pitch}, roll: {resultItem.roll}");
                    }
                }
                else
                {
                    Console.WriteLine($"result: {data.result}");
                }

                Console.WriteLine("****************************************************");
            }
        }



        #region Automatic connection
      
        private async Task ConnectToSerialPortAsync()
        {
            try
            {

                SerialPortReceivedMessage = "";

                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.Close();
                    SerialPortConnectionStatusMessage = "Connect";
                    SerialPortReceivedMessage = "Serial port disconnected.";
                }
                else
                {
                    // PRO COM13
                    // MK2 COM11
                    var portName = await FindSerialPortWithResponseAsync();
                    if (string.IsNullOrEmpty(portName))
                    {
                        SerialPortConnectionStatusMessage = "Connect";
                        throw new Exception("Failed to find a serial port that provides a response.");
                    }

                    _serialPort = new SerialPort(portName, 115200);
                    _serialPort.Open();
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.DataReceived += SerialPort_DataReceived;
                        SerialPortConnectionStatusMessage = "Connected";
                        _serialPort.WriteLine($"Successfully connected to serial port: {portName}");
                    }
                }
            }
            catch (Exception ex)
            {
                SerialPortReceivedMessage = ex.Message;
            }
        }


        protected async Task<string> FindSerialPortWithResponseAsync()
        {
            try
            {
                // Data ports
                ObservableCollection<string> detectPorts = new ObservableCollection<string>();

                // Get all COM ports
                using (ManagementClass i_Entity = new ManagementClass("Win32_PnPEntity"))
                {
                    foreach (ManagementObject i_Inst in i_Entity.GetInstances())
                    {
                        Object o_Guid = i_Inst.GetPropertyValue("ClassGuid");
                        if (o_Guid == null || o_Guid.ToString().ToUpper() != "{4D36E978-E325-11CE-BFC1-08002BE10318}")
                            continue; // Skip all devices except device class "PORTS"

                        String s_Caption = i_Inst.GetPropertyValue("Caption").ToString();
                        String s_Manufact = i_Inst.GetPropertyValue("Manufacturer").ToString();
                        String s_DeviceID = i_Inst.GetPropertyValue("PnpDeviceID").ToString();
                        String s_RegPath = "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\" + s_DeviceID + "\\Device Parameters";
                        String s_PortName = Registry.GetValue(s_RegPath, "PortName", "").ToString();

                        int s32_Pos = s_Caption.IndexOf(" (COM");
                        if (s32_Pos > 0) // remove COM port from description
                            s_Caption = s_Caption.Substring(0, s32_Pos);

                        if (s_Caption.Contains("USB-SERIAL") || s_Caption.Contains("UART") || s_Caption.Contains("USB"))
                        {
                            detectPorts.Add(s_PortName);
                        }
                    }
                }

                foreach (var port in detectPorts)
                {
                    _serialPort = new SerialPort(port, 115200);
                    _serialPort.Open();

                    if (_serialPort?.IsOpen == true)
                    {
                        _serialPort.Write("test");

                        // Wait for response
                        await Task.Delay(500);

                        if (_serialPort.ReadExisting().Length > 0)
                        {
                            return port;
                        }
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                //This exception we handle in antoher function
                throw ex;
            }
            finally
            {
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.Close();
                }
            }
        }



        #endregion





        private void OnCommandActionExceptionHandler(Exception ex)
        {
            //IsErrorOccurred = true;
            //ContentState = ExceptionHandlerViewModel.CreateView(ex.Message);
        }
        #endregion



        public FpvOneProTestViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;

            NavigateHomeCommand = new RelayCommand(o =>
            {
                // Handler for serial port
                if (_serialPort?.IsOpen == true)
                    _serialPort.Close();

                // Handler for navigate to home view
                NavigationService.NavigateTo<HomeViewModel>();
            });


            SerialPortTryToConnectCommand = new AsyncRelayCommand(ConnectToSerialPortAsync, (ex) => OnCommandActionExceptionHandler(ex));

            SerialPortSendMessageCommand = new RelayCommand(o =>
            {
                if (_serialPort?.IsOpen == true)
                {
                    string startAutomaticTest = "{" +
                                  $" \"command\": \"{2}\", " +
                                  $" \"id\": \"{48}\"," +
                                  $" \"value\": \"{1}\" " +
                                 "}\r\n";
                    //_serialPort.WriteLine(startAutomaticTest);
                    _serialPort.WriteLine(SerialPortCommand);
                    SerialPortCommand = "";
                }
            });
        }




    }
}
