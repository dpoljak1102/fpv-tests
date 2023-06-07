using FPV.Core;
using FPV.Service.Common;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace FPV.ViewModels
{
    public class FpvOneProTestViewModel : ViewModelBase, IAsyncInitialization
    {
        public ICommand NavigateHomeCommand { get; set; }

        #region Properties

        private bool _isVisibleSwitchButtonM;
        public bool IsVisibleSwitchButtonM
        {
            get { return _isVisibleSwitchButtonM; }
            set { _isVisibleSwitchButtonM = value; OnPropertyChanged(); }
        }

        private bool _isVisibleSwitchButtonU;
        public bool IsVisibleSwitchButtonU
        {
            get { return _isVisibleSwitchButtonU; }
            set { _isVisibleSwitchButtonU = value; OnPropertyChanged(); }
        }

        private bool _isVisibleSwitchButtonD;
        public bool IsVisibleSwitchButtonD
        {
            get { return _isVisibleSwitchButtonD; }
            set { _isVisibleSwitchButtonD = value; OnPropertyChanged(); }
        }

        private bool _isVisibleSwitchButtonL;
        public bool IsVisibleSwitchButtonL
        {
            get { return _isVisibleSwitchButtonL; }
            set { _isVisibleSwitchButtonL = value; OnPropertyChanged(); }
        }

        private bool _isVisibleSwitchButtonR;
        public bool IsVisibleSwitchButtonR
        {
            get { return _isVisibleSwitchButtonR; }
            set { _isVisibleSwitchButtonR = value; OnPropertyChanged(); }
        }
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
        private string _serialPortCalibrationStatusMessage = "Calibration";
        public string SerialPortCalibrationStatusMessage
        {
            get { return _serialPortCalibrationStatusMessage; }
            set { _serialPortCalibrationStatusMessage = value; OnPropertyChanged(); }
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
        public ICommand SerialPortCalibrationCommand { get; private set; }


        /// <summary>
        /// Event handler for the DataReceived event of the SerialPort.
        /// Reads data from the serial port with a delay and updates the UI accordingly.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Event arguments for the DataReceived event.</param>
        private async void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort?.IsOpen == true)
            {
                try
                {
                    // Read data from the serial port with a delay
                    string data = await Task.Run(() => ReadSerialPortDataWithDelay());

                    // Update Dispatcher property on GUI thread
                    if (!string.IsNullOrEmpty(data))
                    {
                        await App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            // Parse the data and update the UI
                            MyParser(data);
                            //SerialPortReceivedMessage = data;

                            if(!IsCalibrationActive)
                                ReceivedMessages.Add(data);
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during data processing
                    SerialPortReceivedMessage = "*EXCEPTION*" + ex.Message;
                }
            }
        }


        /// <summary>
        /// Reads data from the serial port with a specified delay.
        /// </summary>
        /// <param name="millisecondsDelay">The delay in milliseconds before reading the data (optional).</param>
        /// <returns>The data read from the serial port, or an empty string if the data cannot be read.</returns>
        private async Task<string> ReadSerialPortDataWithDelay(int millisecondsDelay = 500)
        {
            try
            {
                // Delay before reading the data
                await Task.Delay(millisecondsDelay);

                // Read data from the serial port
                return _serialPort.ReadExisting();
            }
            catch (Exception)
            {
                // Handle any exceptions that occur during data reading
                return string.Empty;
            }
        }
        #endregion

        #region SERIAL PORT DATA PARSER
        public class JSONData : ObservableObject
        {
            private string _response;
            public string response
            {
                get { return _response; }
                set { _response = value; OnPropertyChanged(); }
            }

            private string _id;
            public string id
            {
                get { return _id; }
                set { _id = value; OnPropertyChanged(); }
            }

            private object _result;

            public object result
            {
                get { return _result; }
                set { _result = value; OnPropertyChanged(); }
            }
        }

        public class ResultData : ObservableObject
        {
            #region Imu

            #region Yaw
            private string _yaw;
            public string yaw
            {
                get { return _yaw; }
                set { _yaw = value; OnPropertyChanged();}
            }
            #endregion

            #region Pitch
            private string _pitch;
            public string pitch
            {
                get { return _pitch; }
                set { _pitch = value; OnPropertyChanged(); }
            }
            #endregion

            #region Roll
            private string _roll;
            public string roll
            {
                get { return _roll; }
                set { _roll = value; OnPropertyChanged(); }
            }
            #endregion

            #endregion

            #region Calibration part 1

            #region Neutral value
            private string _n = "8184";
            public string n
            {
                get { return _n; }
                set { _n = value; OnPropertyChanged(); }
            }
            #endregion

            #region Mid button value
            private string _m;
            public string m
            {
                get { return _m; }
                set { _m = value; OnPropertyChanged(); }
            }
            #endregion

            #region Up button value
            private string _u;
            public string u
            {
                get { return _u; }
                set { _u = value; OnPropertyChanged(); }
            }
            #endregion

            #region Down button value
            private string _d;
            public string d
            {
                get { return _d; }
                set { _d = value; OnPropertyChanged(); }
            }
            #endregion

            #region Left button value
            private string _l;
            public string l
            {
                get { return _l; }
                set { _l = value; OnPropertyChanged(); }
            }
            #endregion

            #region Right button value
            private string _r;
            public string r
            {
                get { return _r; }
                set { _r = value; OnPropertyChanged(); }
            }
            #endregion

            #endregion

            #region Calibration part 2

            #region Mid + Up button value
            private string _mu;
            public string mu
            {
                get { return _mu; }
                set { _mu = value; OnPropertyChanged(); }
            }
            #endregion

            #region Mid + Down button value
            private string _md;
            public string md
            {
                get { return _md; }
                set { _md = value; OnPropertyChanged(); }
            }
            #endregion

            #region Mid + Left button value
            private string _ml;
            public string ml
            {
                get { return _ml; }
                set { _ml = value; OnPropertyChanged(); }
            }
            #endregion

            #region Mid + Right button value
            private string _mr;
            public string mr
            {
                get { return _mr; }
                set { _mr = value; OnPropertyChanged(); }
            }
            #endregion

            #endregion

            #region Calibration part 3

            #region Left + Up button value
            private string _lu;
            public string lu
            {
                get { return _lu; }
                set { _lu = value; OnPropertyChanged(); }
            }
            #endregion

            #region Left + Right button value
            private string _lr;
            public string lr
            {
                get { return _lr; }
                set { _lr = value; OnPropertyChanged(); }
            }
            #endregion

            #region Left + Down button value
            private string _ld;
            public string ld
            {
                get { return _ld; }
                set { _ld = value; OnPropertyChanged(); }
            }
            #endregion

            #region Up + Right button value
            private string _ur;
            public string ur
            {
                get { return _ur; }
                set { _ur = value; OnPropertyChanged(); }
            }
            #endregion

            #region Up + Down button value
            private string _ud;
            public string ud
            {
                get { return _ud; }
                set { _ud = value; OnPropertyChanged(); }
            }
            #endregion

            #region Right + Down button value
            private string _rd;
            public string rd
            {
                get { return _rd; }
                set { _rd = value; OnPropertyChanged(); }
            }
            #endregion
            #endregion
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
            try
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
            catch (Exception ex)
            {
                SerialPortReceivedMessage += ex.Message;
                return new List<JSONData>();
            }
            
        }

        private void MyParser(string stringToParse)
        {
            try
            {
                List<JSONData> foundObjects = FindObjectsWithProperties(stringToParse);
                foreach (JSONData data in foundObjects)
                {
                    Console.Write($"{"{"} id: {data.id} response: {data.response} ");

                    if (data.result is List<ResultData>)
                    {
                        foreach (var resultItem in (List<ResultData>)data.result)
                        {
                            Console.WriteLine($"yaw: {resultItem.yaw}, pitch: {resultItem.pitch}, roll: {resultItem.roll}");
                            if (Convert.ToInt32(data.id) == 13)
                            {
                                //var yawResult = Convert.ToDouble(resultItem.yaw);
                                var yawResult = double.Parse(resultItem.yaw, CultureInfo.InvariantCulture);
                                var pitchResult = double.Parse(resultItem.pitch, CultureInfo.InvariantCulture);
                                var rollResult = double.Parse(resultItem.roll, CultureInfo.InvariantCulture);

                                bool isValidInput = yawResult != 0.0 && pitchResult != 0.0 && rollResult != 0.0;
                                bool isInRange = -180.0 <= yawResult && yawResult <= 180.0 &&
                                                 -90.0 <= pitchResult && pitchResult <= 90.0 &&
                                                 -180.0 <= rollResult && rollResult <= 180.0;

                                if (isValidInput && isInRange)
                                {
                                    UpdateTestParameter(Convert.ToInt32(data.id), 1);
                                }
                                else
                                {
                                    UpdateTestParameter(Convert.ToInt32(data.id), 0);
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"result: {data.result} {"}"} ");

                        if (!string.IsNullOrEmpty(data?.id))
                        {
                            //Bind calibration parameter
                            if (data.id == "70")
                            {
                                CalibrationAdcValue = Convert.ToInt32(data.result);
                                SaveValidTestParametersForCalibration(ActiveCalibrationButton, CalibrationAdcValue);
                            }
                            else
                            {
                                UpdateTestParameter(Convert.ToInt32(data.id), Convert.ToInt32(data.result));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("** PARSER**" + ex.Message);
            }
           
        }

        /// <summary>
        /// Write command to serial port
        /// </summary>
        /// <param name="command"></param>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GenerateTestCommand(int command, int id, int value)
        {
            return $"TESTMODE {{\"command\":\"{command}\",\"id\":\"{id}\",\"value\":\"{value}\"}}".Replace("\n", "").Replace("\r", "");
        }

        private string GenerateTestCommand(int command, int id, Dictionary<string, string> values)
        {
            string valueString = string.Join(",", values.Select(kv => $"\"{kv.Key}\":\"{kv.Value}\""));
            return $"TESTMODE {{\"command\":\"{command}\",\"id\":\"{id}\",\"value\":{{{valueString}}}}}".Replace("\n", "").Replace("\r", "");
        }

        private Dictionary<string, string> GetCalibrationStage1Results()
        {

            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "n", CalibrationResults.n },
                { "m", CalibrationResults.m },
                { "u", CalibrationResults.u },
                { "d", CalibrationResults.d },
                { "l", CalibrationResults.l },
                { "r", CalibrationResults.r }
            };
            return values;
        }

        private Dictionary<string, string> GetCalibrationStage2Results()
        {
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "mu", CalibrationResults.mu },
                { "md", CalibrationResults.md },
                { "ml", CalibrationResults.ml },
                { "mr", CalibrationResults.mr }
            };
            return values;
        }

        private Dictionary<string, string> GetCalibrationStage3Results()
        {
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "lu", CalibrationResults.lu },
                { "lr", CalibrationResults.lr },
                { "ld", CalibrationResults.ld },
                { "ur", CalibrationResults.ur },
                { "ud", CalibrationResults.ud },
                { "rd", CalibrationResults.rd }
            };
            return values;
        }

        private Dictionary<string, string> GetCalibrationStage4Results()
        {
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "difference", "50"},
                { "calibrated", "170"},
                { "countto", "4"},
            };
            return values;
        }



        #region Automatic connection

        private async Task ConnectToSerialPortAsync()
        {
            try
            {
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.Close();
                    SerialPortConnectionStatusMessage = "Connect";
                    ReceivedMessages.Clear();
                    ReceivedMessages.Add("Serial port disconnected");
                }
                else
                {
                    var portName = await FindSerialPortWithResponseAsync(); // Write automatic test restart
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
                ReceivedMessages.Clear();
                ReceivedMessages.Add(ex.Message);
            }
        }

        protected async Task<string> FindSerialPortWithResponseAsync(string commandForSerialPortToOpenConnection = "Test")
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
                        _serialPort.Write(commandForSerialPortToOpenConnection);

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


        #endregion

        #region CALIBRATION

        // Check if we will display data on gui
        private bool _isCalibrationActive;
        public bool IsCalibrationActive
        {
            get { return _isCalibrationActive; }
            set { _isCalibrationActive = value; OnPropertyChanged(); }
        }

        // Get stage of calibration
        private int _calibrationStage;
        public int CalibrationStage
        {
            get { return _calibrationStage; }
            set { _calibrationStage = value; OnPropertyChanged(); }
        }


        // Adc value from goggle when button is pressed
        private int _calibrationAdcValue;
        public int CalibrationAdcValue
        {
            get { return _calibrationAdcValue; }
            set { _calibrationAdcValue = value; OnPropertyChanged(); }
        }

        // Give name of button pressed
        private string _activeCalibrationButton;
        public string ActiveCalibrationButton
        {
            get { return _activeCalibrationButton; }
            set { _activeCalibrationButton = value; OnPropertyChanged(); }
        }

        private bool _test;
        public bool Test
        {
            get { return _test; }
            set { _test = value; OnPropertyChanged(); }
        }

        #region ADC VALUES
        private readonly int _mCalibrationAdcValue = 1642;
        private readonly int _uCalibrationAdcValue = 7945;
        private readonly int _dCalibrationAdcValue = 5422;
        private readonly int _lCalibrationAdcValue = 3286;
        private readonly int _rCalibrationAdcValue = 7275;

        private readonly int _muCalibrationAdcValue = 1550;
        private readonly int _mdCalibrationAdcValue = 1420;
        private readonly int _mlCalibrationAdcValue = 1215;
        private readonly int _mrCalibrationAdcValue = 1528;

        private readonly int _luCalibrationAdcValue = 2952;
        private readonly int _lrCalibrationAdcValue = 2856;
        private readonly int _ldCalibrationAdcValue = 2520;
        private readonly int _urCalibrationAdcValue = 5828;
        private readonly int _udCalibrationAdcValue = 4592;
        private readonly int _rdCalibrationAdcValue = 4360;

        #endregion

        private void SaveValidTestParametersForCalibration(string name, int value)
        {
            bool isValidValue = false;

            switch (name)
            {
                case "m":
                    isValidValue = IsInRange(value, _mCalibrationAdcValue);
                    CalibrationResults.m = isValidValue ? value.ToString() : string.Empty;
                    break;
                case "u":
                    isValidValue = IsInRange(value, _uCalibrationAdcValue);
                    CalibrationResults.u = isValidValue ? value.ToString() : string.Empty;
                    break;
                case "d":
                    isValidValue = IsInRange(value, _dCalibrationAdcValue);
                    CalibrationResults.d = isValidValue ? value.ToString() : string.Empty;
                    break;
                case "l":
                    isValidValue = IsInRange(value, _lCalibrationAdcValue);
                    CalibrationResults.l = isValidValue ? value.ToString() : string.Empty;
                    break;
                case "r":
                    isValidValue = IsInRange(value, _rCalibrationAdcValue);
                    CalibrationResults.r = isValidValue ? value.ToString() : string.Empty;
                    break;
                case "mu":
                    isValidValue = IsInRange(value, _muCalibrationAdcValue);
                    CalibrationResults.mu = isValidValue ? value.ToString() : string.Empty;
                    break;
                case "md":
                    isValidValue = IsInRange(value, _mdCalibrationAdcValue);
                    CalibrationResults.md = isValidValue ? value.ToString() : string.Empty;
                    break;
                case "ml":
                    isValidValue = IsInRange(value, _mlCalibrationAdcValue);
                    CalibrationResults.ml = isValidValue ? value.ToString() : string.Empty;
                    break;
                case "mr":
                    isValidValue = IsInRange(value, _mrCalibrationAdcValue);
                    CalibrationResults.mr = isValidValue ? value.ToString() : string.Empty;
                    break;
                case "lu":
                    isValidValue = IsInRange(value, _luCalibrationAdcValue);
                    CalibrationResults.lu = isValidValue ? value.ToString() : string.Empty;
                    break;
                case "lr":
                    isValidValue = IsInRange(value, _lrCalibrationAdcValue);
                    CalibrationResults.lr = isValidValue ? value.ToString() : string.Empty;
                    break;
                case "ld":
                    isValidValue = IsInRange(value, _ldCalibrationAdcValue);
                    CalibrationResults.ld = isValidValue ? value.ToString() : string.Empty;
                    break;
                case "ur":
                    isValidValue = IsInRange(value, _urCalibrationAdcValue);
                    CalibrationResults.ur = isValidValue ? value.ToString() : string.Empty;
                    break;
                case "ud":
                    isValidValue = IsInRange(value, _udCalibrationAdcValue);
                    CalibrationResults.ud = isValidValue ? value.ToString() : string.Empty;
                    break;
                case "rd":
                    isValidValue = IsInRange(value, _rdCalibrationAdcValue);
                    CalibrationResults.rd = isValidValue ? value.ToString() : string.Empty;
                    break;
            }

            Test = isValidValue;
        }

        private bool IsInRange(int value, int expectedValue, int additionallyDifference = 50)
        {
            return value >= (expectedValue - additionallyDifference) && value <= (expectedValue + additionallyDifference);
        }


        private ResultData _calibrationResults;
        public ResultData CalibrationResults
        {
            get { return _calibrationResults; }
            set { _calibrationResults = value; OnPropertyChanged(); }
        }


        /// <summary>
        /// Checks if all values in the calibration parts are valid.
        /// </summary>
        /// <returns>True if all values are valid, otherwise false.</returns>
        public bool IsAllValuesValidForCalibration()
        {
            // Define the regions as arrays for easier iteration
            string[] part1 = { CalibrationResults.m, CalibrationResults.u, CalibrationResults.d, CalibrationResults.l, CalibrationResults.r };
            string[] part2 = { CalibrationResults.mu, CalibrationResults.md, CalibrationResults.ml, CalibrationResults.mr };
            string[] part3 = { CalibrationResults.lu, CalibrationResults.lr, CalibrationResults.ld, CalibrationResults.ur, CalibrationResults.ud, CalibrationResults.rd };

            // Check values in the Calibration part 1 region
            if (part1.Any(string.IsNullOrEmpty))
                return false;

            // Check values in the Calibration part 2 region
            if (part2.Any(string.IsNullOrEmpty))
                return false;

            // Check values in the Calibration part 3 region
            if (part3.Any(string.IsNullOrEmpty))
                return false;

            // All values are valid
            return true;
        }

        private async Task CalibrationAsync()
        {
            // Restart calibration results
            CalibrationResults = new ResultData();
            // Start calibration
            IsCalibrationActive = true;
            // Restart calibration results on GUI
            UpdateTestParameter(67, 0); 
            try
            {
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.WriteLine(GenerateTestCommand(2, 48, 2));
                    await CalibrationStage1Async();
                    await CalibrationStage2Async();
                    await CalibrationStage3Async();
                    _serialPort.WriteLine(GenerateTestCommand(2, 48, 1));

                    // Give goggle time 
                    await Task.Delay(100);

                    if (IsAllValuesValidForCalibration())
                    {
                        // Write calibration results for stage 1 to goggle
                        _serialPort.WriteLine(GenerateTestCommand(2, 71, GetCalibrationStage1Results()));
                        await Task.Delay(100);

                        // Write calibration results for stage 2 to goggle
                        _serialPort.WriteLine(GenerateTestCommand(2, 72, GetCalibrationStage2Results()));
                        await Task.Delay(100);

                        // Write calibration results for stage 3 to goggle
                        _serialPort.WriteLine(GenerateTestCommand(2, 73, GetCalibrationStage3Results()));
                        await Task.Delay(100);

                        // Write calibration results for stage 4 to goggle
                        _serialPort.WriteLine(GenerateTestCommand(2, 74, GetCalibrationStage4Results()));

                        // Set calibration to succes on GUI
                        UpdateTestParameter(67, 1);
                    }
                    CalibrationAdcValue = 0;
                    IsCalibrationActive = false;
                    ReceivedMessages.Add("Calibration procedure done");
                }
                else
                    throw new Exception("Serial port is not open.");
            }
            catch (Exception ex)
            {
                SerialPortReceivedMessage = ex.Message;
            }
        }

        private async Task CalibrationStage1Async()
        {
            CalibrationStage = 1;
            IsVisibleSwitchButtonR = true;
            ActiveCalibrationButton = "r";
            await Task.Delay(3500);
            IsVisibleSwitchButtonR = false;

            IsVisibleSwitchButtonU = true;
            ActiveCalibrationButton = "u";
            await Task.Delay(3500);
            IsVisibleSwitchButtonU = false;
            CalibrationStage = 2;


            IsVisibleSwitchButtonL = true;
            ActiveCalibrationButton = "l";
            await Task.Delay(3500);
            IsVisibleSwitchButtonL = false;
            CalibrationStage = 3;

            IsVisibleSwitchButtonD = true;
            ActiveCalibrationButton = "d";
            await Task.Delay(3500);
            IsVisibleSwitchButtonD = false;
            CalibrationStage = 4;


            IsVisibleSwitchButtonM = true;
            ActiveCalibrationButton = "m";
            await Task.Delay(3500);
            IsVisibleSwitchButtonM = false;
            CalibrationStage = 5;

            SerialPortCalibrationStatusMessage = "Calibration";

        }

        private async Task CalibrationStage2Async()
        {
            IsVisibleSwitchButtonM = true;
            IsVisibleSwitchButtonR = true;
            ActiveCalibrationButton = "mr";
            await Task.Delay(3500);
            IsVisibleSwitchButtonM = false;
            IsVisibleSwitchButtonR = false;
            CalibrationStage = 6;


            IsVisibleSwitchButtonM = true;
            IsVisibleSwitchButtonU = true;
            ActiveCalibrationButton = "mu";
            await Task.Delay(3500);
            IsVisibleSwitchButtonM = false;
            IsVisibleSwitchButtonU = false;
            CalibrationStage = 7;


            IsVisibleSwitchButtonM = true;
            IsVisibleSwitchButtonL = true;
            ActiveCalibrationButton = "ml";
            await Task.Delay(3500);
            IsVisibleSwitchButtonM = false;
            IsVisibleSwitchButtonL = false;
            CalibrationStage = 8;


            IsVisibleSwitchButtonM = true;
            IsVisibleSwitchButtonD = true;
            ActiveCalibrationButton = "md";
            await Task.Delay(3500);
            IsVisibleSwitchButtonM = false;
            IsVisibleSwitchButtonD = false;
            CalibrationStage = 9;


            SerialPortCalibrationStatusMessage = "Calibration";

        }

        private async Task CalibrationStage3Async()
        {
            IsVisibleSwitchButtonU = true;
            IsVisibleSwitchButtonR = true;
            ActiveCalibrationButton = "ur";
            await Task.Delay(3500);
            IsVisibleSwitchButtonU = false;
            IsVisibleSwitchButtonR = false;
            CalibrationStage = 10;


            IsVisibleSwitchButtonL = true;
            IsVisibleSwitchButtonR = true;
            ActiveCalibrationButton = "lr";
            await Task.Delay(3500);
            IsVisibleSwitchButtonL = false;
            IsVisibleSwitchButtonR = false;
            CalibrationStage = 11;


            IsVisibleSwitchButtonR = true;
            IsVisibleSwitchButtonD = true;
            ActiveCalibrationButton = "rd";
            await Task.Delay(3500);
            IsVisibleSwitchButtonR = false;
            IsVisibleSwitchButtonD = false;
            CalibrationStage = 12;

            IsVisibleSwitchButtonL = true;
            IsVisibleSwitchButtonU = true;
            ActiveCalibrationButton = "lu";
            await Task.Delay(3500);
            IsVisibleSwitchButtonL = false;
            IsVisibleSwitchButtonU = false;
            CalibrationStage = 13;


            IsVisibleSwitchButtonL = true;
            IsVisibleSwitchButtonD = true;
            ActiveCalibrationButton = "ld";
            await Task.Delay(3500);
            IsVisibleSwitchButtonL = false;
            IsVisibleSwitchButtonD = false;
            CalibrationStage = 14;


            IsVisibleSwitchButtonU = true;
            IsVisibleSwitchButtonD = true;
            ActiveCalibrationButton = "ud";
            await Task.Delay(3500);
            IsVisibleSwitchButtonU = false;
            IsVisibleSwitchButtonD = false;
            CalibrationStage = 15;


            SerialPortCalibrationStatusMessage = "Calibration";

        }

        #endregion



        #region GUI BUTTONS

        private static readonly System.Windows.ResourceDictionary _resourceDictionary = App.Current.Resources;
        private static readonly SolidColorBrush _defaultColorBrush = (SolidColorBrush)_resourceDictionary["IconActiveBrush"];
        private static readonly SolidColorBrush _successColorBrush = (SolidColorBrush)_resourceDictionary["GreenBrush"];
        private static readonly SolidColorBrush _failureColorBrush = (SolidColorBrush)_resourceDictionary["RedBrush"];

        //The message that is bound to GUI from serial port
        private ObservableCollection<string> _receivedMessages = new ObservableCollection<string>();
        public ObservableCollection<string> ReceivedMessages
        {
            get { return _receivedMessages; }
            set { _receivedMessages = value; OnPropertyChanged(); }
        }

        // Test command for automatic test
        private ObservableCollection<ITestParameter> _fpvTestParameterCollection;
        public ObservableCollection<ITestParameter> FpvTestParameterCollection
        {
            get { return _fpvTestParameterCollection; }
            set { _fpvTestParameterCollection = value; OnPropertyChanged(); }
        }

        // Test command for maunal test
        private ObservableCollection<ITestParameter> _fpvCommandParameterCollection;
        public ObservableCollection<ITestParameter> FpvCommandParameterCollection
        {
            get { return _fpvCommandParameterCollection; }
            set { _fpvCommandParameterCollection = value; OnPropertyChanged(); }
        }

        // Selected command for automatic test
        private ITestParameter _selectedFpvTestParameter;
        public ITestParameter SelectedFpvTestParameter
        {
            get { return _selectedFpvTestParameter; }
            set { _selectedFpvTestParameter = value; OnPropertyChanged(); WriteTestCommandOnPropertyChanged(); }
        }

        // Selected command for manual test
        private ITestParameter _selectedFpvCommandParameter;
        public ITestParameter SelectedFpvCommandParameter
        {
            get { return _selectedFpvCommandParameter; }
            set { _selectedFpvCommandParameter = value; OnPropertyChanged(); WriteCommandOnPropertyChanged(); }
        }

        /// <summary>
        /// Writes a test command to the serial port when a property is changed. 
        /// The change occurs when the user clicks on one of the icons for automatic test.
        /// </summary>
        private async void WriteTestCommandOnPropertyChanged()
        {
            if (_serialPort?.IsOpen != true)
            {
                OnCommandActionExceptionHandler("Serial port disconnected");
                return;
            }

            if (SelectedFpvTestParameter?.Id > 0)
            {
                if (SelectedFpvTestParameter.Id == 67) // Calibration
                {
                    await CalibrationAsync();
                }
                else
                {
                    _serialPort.WriteLine(GenerateTestCommand(1, SelectedFpvTestParameter.Id, 1));
                }

                // Restart selected item from list
                SelectedFpvTestParameter = null;
            }

           
        }

        /// <summary>
        /// Writes a test command to the serial port when a property is changed. 
        /// The change occurs when the user clicks on one of the icons for manual test.
        /// </summary>
        private async void WriteCommandOnPropertyChanged()
        {
            try
            {
                if (_serialPort?.IsOpen != true)
                {
                    OnCommandActionExceptionHandler("Serial port disconnected");
                    return;
                }

                if (SelectedFpvCommandParameter?.Id > 0)
                {
                    if (SelectedFpvCommandParameter.Id == 48) // Automatic test
                    {
                        InitTestParameters();
                        _serialPort.WriteLine(GenerateTestCommand(2, 49, 1));
                        await Task.Delay(1000);
                        _serialPort.WriteLine(GenerateTestCommand(2, SelectedFpvCommandParameter.Id, 1));
                    }
                    else if (SelectedFpvCommandParameter.Id == 29 || SelectedFpvCommandParameter.Id == 30 || SelectedFpvCommandParameter.Id == 31) // Buzzer, Left fan, Right fan
                    {
                        _serialPort.WriteLine(GenerateTestCommand(2, SelectedFpvCommandParameter.Id, 1));
                        await Task.Delay(1000);
                        _serialPort.WriteLine(GenerateTestCommand(2, SelectedFpvCommandParameter.Id, 0));
                    }

                    // Restart selected item from list
                    SelectedFpvCommandParameter = null;
                }
            }
            catch (Exception ex)
            {
                OnCommandActionExceptionHandler(ex);
            }
        }

        public interface ITestParameter
        {
            int Id { get; set; }
            string Name { get; set; }
            int TestErrorCode { get; set; }
            PathGeometry PathData { get; set; }
            SolidColorBrush PathFill { get; set; }
        }
        public class FpvTestParameter : ObservableObject, ITestParameter
        {
            private int _id;
            public int Id
            {
                get { return _id; }
                set { _id = value; OnPropertyChanged(); }
            }

            private string _name;
            public string Name
            {
                get { return _name; }
                set { _name = value; OnPropertyChanged(); }
            }

            private int _testErrorCode;
            public int TestErrorCode
            {
                get { return _testErrorCode; }
                set { _testErrorCode = value; OnPropertyChanged(); }
            }

            private PathGeometry _pathData;
            public PathGeometry PathData
            {
                get { return _pathData; }
                set { _pathData = value; OnPropertyChanged(); }
            }

            private SolidColorBrush _pathFill;
            public SolidColorBrush PathFill
            {
                get { return _pathFill; }
                set { _pathFill = value; OnPropertyChanged(); }
            }

            public FpvTestParameter(int id, string name, int testErrorCode, PathGeometry pathData, SolidColorBrush pathFill)
            {
                Id = id;
                Name = name;
                TestErrorCode = testErrorCode;
                PathData = pathData;
                PathFill = pathFill;
            }
        }

        public abstract class TestParametersProvider
        {
            public abstract ObservableCollection<ITestParameter> GetAllTestParameters();
        }

        public class FpvTestParametersProvider : TestParametersProvider
        {
            public override ObservableCollection<ITestParameter> GetAllTestParameters()
            {
                var testParameters = new ObservableCollection<ITestParameter>();
                testParameters.Add(new FpvTestParameter(67, "Calib", 0, (PathGeometry)_resourceDictionary["CalibrationIcon"], _defaultColorBrush));
                testParameters.Add(new FpvTestParameter(1, "IMU", 0, (PathGeometry)_resourceDictionary["TemperatureIcon"], _defaultColorBrush));
                testParameters.Add(new FpvTestParameter(2, "Atmel", 0, (PathGeometry)_resourceDictionary["TemperatureIcon"], _defaultColorBrush));
                testParameters.Add(new FpvTestParameter(3, "Rtc", 0, (PathGeometry)_resourceDictionary["VoltageIcon"], _defaultColorBrush));
                testParameters.Add(new FpvTestParameter(4, "WiFi", 0, (PathGeometry)_resourceDictionary["WifiIcon"], _defaultColorBrush));
                testParameters.Add(new FpvTestParameter(5, "Atmel", 0, (PathGeometry)_resourceDictionary["VoltageIcon"], _defaultColorBrush));
                testParameters.Add(new FpvTestParameter(6, "SD", 0, (PathGeometry)_resourceDictionary["MicroSdCardIcon"], _defaultColorBrush));
                testParameters.Add(new FpvTestParameter(7, "Displays", 0, (PathGeometry)_resourceDictionary["DisplayIcon"], _defaultColorBrush));
                testParameters.Add(new FpvTestParameter(9, "Vrx", 0, (PathGeometry)_resourceDictionary["ConnectionIcon"], _defaultColorBrush));
                testParameters.Add(new FpvTestParameter(10, "AV", 0, (PathGeometry)_resourceDictionary["ConnectionIcon"], _defaultColorBrush));
                testParameters.Add(new FpvTestParameter(11, "HDMI", 0, (PathGeometry)_resourceDictionary["Hdmi1Icon"], _defaultColorBrush));
                testParameters.Add(new FpvTestParameter(12, "HDMI SFP", 0, (PathGeometry)_resourceDictionary["Hdmi2Icon"], _defaultColorBrush));
                testParameters.Add(new FpvTestParameter(13, "Imu", 0, (PathGeometry)_resourceDictionary["VoltageIcon"], _defaultColorBrush));
                return testParameters;
            }
        }

        public class FpvCommandParametersProvider : TestParametersProvider
        {
            public override ObservableCollection<ITestParameter> GetAllTestParameters()
            {
                var testParameters = new ObservableCollection<ITestParameter>();
                testParameters.Add(new FpvTestParameter(48, "Automatic", 0, (PathGeometry)_resourceDictionary["AutoRefreshIcon"], _defaultColorBrush));
                testParameters.Add(new FpvTestParameter(29, "Buzzer", 0, (PathGeometry)_resourceDictionary["VolumeIcon"], _defaultColorBrush));
                testParameters.Add(new FpvTestParameter(30, "Left fan", 0, (PathGeometry)_resourceDictionary["FanIcon"], _defaultColorBrush));
                testParameters.Add(new FpvTestParameter(31, "Right fan", 0, (PathGeometry)_resourceDictionary["FanIcon"], _defaultColorBrush));
                return testParameters;
            }
        }

        /// <summary>
        /// Initialize the automatic test parameter that will be checked for the correctness
        /// </summary>
        private void InitTestParameters()
        {
            TestParametersProvider provider = new FpvTestParametersProvider();
            FpvTestParameterCollection = new ObservableCollection<ITestParameter>();
            FpvTestParameterCollection = provider.GetAllTestParameters();
        }

        /// <summary>
        /// Initialize the manual test parameter
        /// </summary>
        private void InitCommandParameters()
        {
            TestParametersProvider provider = new FpvCommandParametersProvider();
            FpvCommandParameterCollection = new ObservableCollection<ITestParameter>();
            FpvCommandParameterCollection = provider.GetAllTestParameters();
        }

        /// <summary>
        /// Updates the test parameter based on the target ID and result value.
        /// </summary>
        /// <param name="targetId">The ID of the test parameter to update.</param>
        /// <param name="result">The result value used to update the parameter.</param>
        private void UpdateTestParameter(int targetId, int result)
        {
            // Find the parameter to update based on the targetId
            var parameterToUpdate = FpvTestParameterCollection.FirstOrDefault(parameter => parameter.Id == targetId);

            if (parameterToUpdate != null)
            {
                // Update the parameter based on its ID

                // IMU temperature
                if (parameterToUpdate.Id == 1)
                {
                    if (result > 0)
                    {
                        parameterToUpdate.PathFill = _successColorBrush;
                        parameterToUpdate.TestErrorCode = 1; // Success
                    }
                    else
                    {
                        parameterToUpdate.PathFill = _failureColorBrush;
                        parameterToUpdate.TestErrorCode = 2; // Failure
                    }
                }
                // Atmel temperature
                else if (parameterToUpdate.Id == 2)
                {
                    if (result > 0)
                    {
                        parameterToUpdate.PathFill = _successColorBrush;
                        parameterToUpdate.TestErrorCode = 1; // Success
                    }
                    else
                    {
                        parameterToUpdate.PathFill = _failureColorBrush;
                        parameterToUpdate.TestErrorCode = 2; // Failure
                    }
                }
                // Atmel voltage
                else if (parameterToUpdate.Id == 5)
                {
                    if (result > 11000)
                    {
                        parameterToUpdate.PathFill = _successColorBrush;
                        parameterToUpdate.TestErrorCode = 1; // Success
                    }
                    else
                    {
                        parameterToUpdate.PathFill = _failureColorBrush;
                        parameterToUpdate.TestErrorCode = 2; // Failure
                    }
                }
                // Other parameters
                else
                {
                    if (result > 0)
                    {
                        parameterToUpdate.PathFill = _successColorBrush;
                        parameterToUpdate.TestErrorCode = 1; // Success
                    }
                    else
                    {
                        parameterToUpdate.PathFill = _failureColorBrush;
                        parameterToUpdate.TestErrorCode = 2; // Failure
                    }
                }
            }
        }
        #endregion

        #region CONSTRUCTOR
        public FpvOneProTestViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
            Initialization = InitializeAsync();

            NavigateHomeCommand = new RelayCommand(o =>
            {
                // Handler for serial port
                if (_serialPort?.IsOpen == true)
                    _serialPort.Close();

                // Handler for navigate to home view
                NavigationService.NavigateTo<HomeViewModel>();
            });

            SerialPortCalibrationCommand = new AsyncRelayCommand(CalibrationAsync, (ex) => OnCommandActionExceptionHandler(ex));

            SerialPortTryToConnectCommand = new AsyncRelayCommand(ConnectToSerialPortAsync, (ex) => OnCommandActionExceptionHandler(ex));

            SerialPortSendMessageCommand = new RelayCommand(o =>
            {
                if (_serialPort?.IsOpen == true)
                {
                    // Write to serial port command
                    _serialPort.WriteLine(SerialPortCommand.Replace("\n", ""));

                    // Set this for effect when user click enter to clear input textbox
                    SerialPortCommand = "";
                }
                else
                {
                    OnCommandActionExceptionHandler("Serial port disconnected");
                }
            });
        }
        public Task Initialization { get; private set; }
        private async Task InitializeAsync()
        {
            try
            {
                // Test parameters for goggle
                InitTestParameters();
                // Test commands for goggle
                InitCommandParameters();

                // Try to connect to serial port
                await ConnectToSerialPortAsync();
            }
            catch (Exception ex)
            {
                OnCommandActionExceptionHandler(ex);
            }
        }
        private void OnCommandActionExceptionHandler(Exception ex)
        {
            ReceivedMessages.Clear();
            //Exception Handler
            ReceivedMessages.Add(ex.Message);
        }
        private void OnCommandActionExceptionHandler(string message)
        {
            ReceivedMessages.Clear();
            //Exception Handler
            ReceivedMessages.Add(message);
        }
        #endregion
    }
}
