using FPV.Core;
using FPV.Service.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
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

        #endregion



        public FpvOneProTestViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;

            NavigateHomeCommand = new RelayCommand(o =>
            {
                NavigationService.NavigateTo<HomeViewModel>();
            });

            SerialPortTryToConnectCommand = new RelayCommand(o =>
            {
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.Close();
                    SerialPortConnectionStatusMessage = "Connect";
                    SerialPortReceivedMessage = "";
                }
                else
                {
                    // PRO COM13
                    // MK2 COM11
                    _serialPort = new SerialPort("COM13", 115200);
                    _serialPort.Open();
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.DataReceived += SerialPort_DataReceived;
                        SerialPortConnectionStatusMessage = "Connected";
                    }
                }
            });
           


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
