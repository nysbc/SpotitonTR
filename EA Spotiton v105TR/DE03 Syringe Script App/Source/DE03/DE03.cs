using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO.Ports;
using System.Collections.Generic;

// Testing DE03 firmware version 00.008
// Marked PKv.008  in areas with compatibility issues versus DE03 version 00.006

// PKv4.6.0

namespace Aurigin
{

    public class DE03
    {
        private const int COMMAND_TIMEOUT = 5000;
        private const int COMMAND_DELAY_BUG = 0;  // Extra Delay added after every command.

        private const int VERSION = 6;  // PKv.008   Set to VERSION = 6 for DE03 firmware version 00.006
                                        //Set to VERSION = 8 for DE03 firmware version 00.008

        private static bool initialized = false;
        private static int port = 0;

        private static List<SerialPort> ports = new List<SerialPort>();    // New list of serial ports
        private static List<ManualResetEvent> mutexes = new List<ManualResetEvent>();

        public const bool useSecondDE03 = true;


        #region properties  

        // Tells whether the DE03 has been initialized or not
        public static bool Initialized
        {
            get { return initialized; }
        }
        #endregion

        #region public methods	  (New ones for DE03 Development)

        public static int InitTipControl(int desiredPort)
        {
            // first the com port
            if (InitComPort(desiredPort) != 0) return 1;

            port = desiredPort;

            initialized = true;

            return 0;
        }

        public static int InitializeBoard()
        {
            return InitializeBoard(1);
        }

        public static int InitializeBoard(int channel)
        {
            return InitializeBoard(channel, false);
        }

        public static int InitializeBoard(int channel, bool useDevice2)
        {
            DE03.SendCommand(useDevice2, "E0");   // Make sure the echo is off.
            Thread.Sleep(50);
            DE03.SendCommand(useDevice2, "E0");   // Twice is required on DE03 powerup for some reason.
            Thread.Sleep(50);
            if (DE03.SendCommand(useDevice2, "Z1", "0", 100) != 0) return 1;   // HIGH Voltage On
            string commandStr = "CH" + channel.ToString();
            if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Turn on the Channel
            return 0;
        }

        // Will check to see if the DE03 is still firing (useful when in external trigger mode and waiting for triggers).
        // PKv4.2.6,  Added this method

        // PKv4.6.0  Overload to accomodate 2nd DE03   bool useDevice2

        public static bool Firing()
        {
            return Firing(false);
        }

        public static bool Firing(bool useDevice2)
        {
            DE03.SendCommand(useDevice2, "G?");
            string response = DE03.GetResponse(useDevice2, 100);
            if (response == "1")
            {
                return true;
            }
            else
            {
                if (response != "0")
                {
                    Console.WriteLine("DE03.cs - Unexpected response to G? command {0}", response);
                }
                return false;
            }
        }

        // Revision R2.02 - Now uses tipNo

        // PKv4.6.0  Overload to accomodate 2nd DE03,  bool useDevice2

        public static int TrapSetup(int tipNo, int leading, int dwell, int trailing, int trapDrops, int trapFreq, int trapAmp,
            int strobeDelay, int strobeDuration, int triggerSetting, int triggerDelay, int triggerPeriod)
        {
            return TrapSetup(false, tipNo, leading, dwell, trailing, trapDrops, trapFreq, trapAmp,
                strobeDelay, strobeDuration, triggerSetting, triggerDelay, triggerPeriod);
        }

        public static int TrapSetup(bool useDevice2, int tipNo, int leading, int dwell, int trailing, int trapDrops, int trapFreq, int trapAmp,
            int strobeDelay, int strobeDuration, int triggerSetting, int triggerDelay, int triggerPeriod)
        {
            string commandStr = "CH" + tipNo.ToString();
            if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Turn on the Channel
            return TrapSetup(useDevice2, leading, dwell, trailing, trapDrops, trapFreq, trapAmp,
             strobeDelay, strobeDuration, triggerSetting, triggerDelay, triggerPeriod);

        }

        public static int TrapSetup(int leading, int dwell, int trailing, int trapDrops, int trapFreq, int trapAmp,
            int strobeDelay, int strobeDuration,
            int triggerSetting, int triggerDelay, int triggerPeriod)
        {
            return TrapSetup(false, leading, dwell, trailing, trapDrops, trapFreq, trapAmp,
             strobeDelay, strobeDuration,
             triggerSetting, triggerDelay, triggerPeriod);
        }

        public static int TrapSetup(bool useDevice2, int leading, int dwell, int trailing, int trapDrops, int trapFreq, int trapAmp,
            int strobeDelay, int strobeDuration,
            int triggerSetting, int triggerDelay, int triggerPeriod)
        {

            if (DE03.SendCommand(useDevice2, "BRFF", "0", 100) != 0) return 1;   // Set the bias ramp time to 255usec,  PKv4.6.1

            int bufferLength = leading + dwell + trailing + 1;
            if ((strobeDelay + strobeDuration) > bufferLength)
            {
                bufferLength = strobeDelay + strobeDuration + 1;
            }

            int[] waveVolt = new int[bufferLength];

            waveVolt[0] = 4095;
            int step = trapAmp / leading;   // Use integer math , truncate error is small
            for (int i = 1; i <= leading; i++)
            {
                waveVolt[i] = waveVolt[i - 1] - step;
            }
            for (int i = (leading + 1); i <= (leading + dwell); i++)
            {
                waveVolt[i] = waveVolt[i - 1];
            }
            step = trapAmp / trailing;
            for (int i = (leading + dwell + 1); i < (leading + dwell + trailing); i++)
            {
                waveVolt[i] = waveVolt[i - 1] + step;
            }
            for (int i = (leading + dwell + trailing); i < bufferLength; i++)
            {
                waveVolt[i] = 4095;
            }

            DE03.SendCommand(useDevice2, "WU" + bufferLength.ToString(), "0", 100);   // Don't wait for reply
            Console.WriteLine("DE03 Trap Setup - Downloading wave to DE03");
            //			System.Threading.Thread.Sleep(50);				
            //			DE03.SendCommandNoCR("   ");   // Bug in code requires 3 spaces.
            //			System.Threading.Thread.Sleep(10);				
            bool strobeOn = false;
            string tempString;
            for (int i = 0; i < bufferLength; i++)
            {
                strobeOn = false;
                if (i >= strobeDelay)
                {
                    if (i < (strobeDelay + strobeDuration))
                    {
                        strobeOn = true;
                    }
                }
                tempString = Convert.ToString(waveVolt[i], 16);
                tempString = tempString.PadLeft(3, '0');
                if (strobeOn)
                {
                    tempString = "1" + tempString;
                }
                else
                {
                    tempString = "0" + tempString;
                }
                tempString = tempString.ToUpper();
                DE03.SendCommandNoCR(useDevice2, tempString);
                Console.Write(tempString);
                System.Threading.Thread.Sleep(1);
            }
            DE03.SendCommand(useDevice2, "");  // CR to wrap things up
            Console.WriteLine("");

            string resp = GetResponse(useDevice2, 1000);			// DE03 responds with Checksum....  TODO Should make sure it got the right one.

            Console.WriteLine("Checksum from DE03 = {0}", resp);
            string commandStr = "RC" + trapDrops.ToString();
            if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Number of drops

            if (triggerSetting == 0)
            {
                commandStr = "TE0";
                if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;
                int trapDelay = (1000000 / trapFreq) - bufferLength;
                if (trapDelay < 0) { trapDelay = 100; }
                commandStr = "RP" + trapDelay.ToString();
                if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Delay Between drops
            }
            else
            {
                commandStr = "TE" + triggerSetting.ToString();
                if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Trigger Enable (TE1 or TE2)
                commandStr = "RP" + triggerDelay.ToString();
                if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Delay after wave before next trigger can be used
                commandStr = "TP" + triggerPeriod.ToString();
                if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Trigger period
            }


            if (DE03.SendCommand(useDevice2, "WR1", "0", 100) != 0) return 1;   // One Wave

            // In case i want to send this out to file.
            //  outputFile = @"C:\DE03 C# Example App\temp_h.txt";
            //	DE03.WriteToFile(outputFile,outString);

            return 0;
        }

        // PKv4.6.0  Overload to accomodate 2nd DE03,  bool useDevice2

        public static int CosSetup(int tipNo, int desiredFreq, int inBurstNo, int cosAmplitude, int strobeDuration, int strobeDelay,
            int numOfBursts, int triggerSetting, int triggerDelay, int triggerPeriod)
        {
            return CosSetup(false, tipNo, desiredFreq, inBurstNo, cosAmplitude, strobeDuration, strobeDelay,
                 numOfBursts, triggerSetting, triggerDelay, triggerPeriod);
        }

        // Revision R2.02,  now uses tip No
        public static int CosSetup(bool useDevice2, int tipNo, int desiredFreq, int inBurstNo, int cosAmplitude, int strobeDuration, int strobeDelay,
            int numOfBursts, int triggerSetting, int triggerDelay, int triggerPeriod)
        {
            string commandStr = "CH" + tipNo.ToString();
            if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Turn on the Channel
            Thread.Sleep(50);
            return CosSetup(useDevice2, desiredFreq, inBurstNo, cosAmplitude, strobeDuration, strobeDelay,
             numOfBursts, triggerSetting, triggerDelay, triggerPeriod);
        }

        public static int CosSetup(int desiredFreq, int inBurstNo, int cosAmplitude,
                int strobeDuration, int strobeDelay,
                int numOfBursts, int triggerSetting, int triggerDelay, int triggerPeriod)
        {
            return CosSetup(false, desiredFreq, inBurstNo, cosAmplitude,
                 strobeDuration, strobeDelay, numOfBursts, triggerSetting, triggerDelay, triggerPeriod);
        }

        public static int CosSetup(bool useDevice2, int desiredFreq, int inBurstNo, int cosAmplitude,
            int strobeDuration, int strobeDelay,
            int numOfBursts, int triggerSetting, int triggerDelay, int triggerPeriod)

        {
            if (DE03.SendCommand(useDevice2, "BRFF", "0", 100) != 0) return 1;   // Set the bias ramp time to 255usec,  PKv4.6.1

            if (DE03.SendCommand(useDevice2, "NS1", "0", 100) != 0) return 1;   // Number Cycles 1, when producing the wave

            string commandStr = "FS" + desiredFreq.ToString();
            if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Frequency

            commandStr = "WR" + inBurstNo.ToString();
            if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Number of cycles in each burst

            commandStr = "A" + Convert.ToString(cosAmplitude, 16);
            if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Amplitude

            commandStr = "FD" + strobeDuration.ToString();  //PKv.008
            if (VERSION > 6) commandStr = "BD" + strobeDuration.ToString();
            if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Strobe Duration

            commandStr = "FP" + strobeDelay.ToString();   //PKv.008
            if (VERSION > 6) commandStr = "BP" + strobeDelay.ToString();
            if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Strobe Position

            Thread.Sleep(10);    // 10msec delay before the PW command.
            if (DE03.SendCommand(useDevice2, "PW", "0", 100) != 0) return 1;   // Produce Wave

            Thread.Sleep(10);    // 10msec delay after the PW command.
            commandStr = "RC" + numOfBursts.ToString();
            if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Number of times to repeat the burst

            commandStr = "TE" + triggerSetting.ToString();   // 0,1,2
            if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // External Trigger Enable (TE0,TE1,TE2)

            commandStr = "RP" + triggerDelay.ToString();
            if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Delay after burst before next trigger or burst

            commandStr = "TP" + triggerPeriod.ToString();
            if (DE03.SendCommand(useDevice2, commandStr, "0", 100) != 0) return 1;   // Trigger period usually 1  (2 means trigger every 2nd trigger, 3 every third etc..)

            return 0;
        }

        // PKv4.6.0  Overload to accomodate 2nd DE03   bool useDevice2

        public static int StartWaveform(bool waitForCompletion, int timeout_ms)
        {
            return StartWaveform(false, waitForCompletion, timeout_ms);
        }

        public static int StartWaveform(bool useDevice2, bool waitForCompletion, int timeout_ms)
        {
            if (VERSION > 6)
            {
                if (DE03.SendCommand(useDevice2, "G", "0", 100) != 0) return 1;          //PKv.008  
            }
            else
            {
                if (DE03.SendCommand(useDevice2, "G", "1", 100) != 0) return 1;        //PKv.008  
            }

            // TODO - Make sure this waitForCompletion works.   //PKv.008  still need to test
            if (waitForCompletion)
            {
                string respStr = GetResponse(useDevice2, timeout_ms);
                if (respStr != "0")
                {
                    string message = @"DE03.StartWaveform waiting for completion \n Expecting 0, reply=" + respStr;
                    MessageBox.Show(message, "DE03 Controller", MessageBoxButtons.OK);
                    return 1;
                }
            }
            return 0;
        }

        // PKv4.6.0  Overload to accomodate 2nd DE03   bool useDevice2

        public static int StopWaveform()
        {
            return StopWaveform(false);
        }

        public static int StopWaveform(bool useDevice2)
        {
            if (DE03.SendCommand(useDevice2, "S", "0", 300) != 0)       //PKv.008 (longer bias ramp is default in version .008 longer timeout OK for version .006 too. 
            {
                MessageBox.Show("DE03.StopWaveform: S command did not receive 0 reply", "DE03 Controller", MessageBoxButtons.OK);
                return 1;
            }
            return 0;
        }

        // PKv4.6.0   Added overload for 2nd DE03

        public static int SendCommandNoCR(string command)
        {
            return SendCommandNoCR(false, command);
        }


        public static int SendCommandNoCR(bool useDevice2, string command)
        {

            int portIndex = 0;
            if (useDevice2) portIndex = 1;

            // clear out any old data
            if (ClearResponseBuffer() != 0) return 1;
            ports[portIndex].Write(command);
            return 0;
        }

        // PKv4.6.0   Added overload for 2nd DE03

        public static int SendCommand(string command)
        {
            return SendCommand(false, command);
        }

        public static int SendCommand(bool useDevice2, string command)
        {

            int portIndex = 0;
            if (useDevice2) portIndex = 1;

            // clear out any old data
            if (ClearResponseBuffer() != 0) return 1;       // TODO need to update this to work with only one of two ports.

            ports[portIndex].Write(command + "\r");
            Thread.Sleep(COMMAND_DELAY_BUG);
            return 0;
        }

        public static int SendCommand(byte[] buffer)
        {
            return SendCommand(false, buffer);
        }

        /// <summary>
        /// Send Command
        /// </summary>
        /// <param name="command">bytes</param>
        /// <returns></returns>

        public static int SendCommand(bool useDevice2, byte[] buffer)
        {

            int portIndex = 0;
            if (useDevice2) portIndex = 1;

            // clear out any old data
            if (ClearResponseBuffer() != 0) return 1;
            ports[portIndex].Write(buffer, 0, buffer.Length);
            Thread.Sleep(COMMAND_DELAY_BUG);
            return 0;
        }

        // PKv4.6.0   Added overload for 2nd DE03

        public static int SendCommand(string command, string expectedReply, int timeout_ms)
        {
            return SendCommand(false, command, expectedReply, timeout_ms);
        }

        //		Overload.
        //		expectedReply string will look for and match an exact expected reply string.
        public static int SendCommand(bool useDevice2, string command, string expectedReply, int timeout_ms)
        {

            int portIndex = 0;
            if (useDevice2) portIndex = 1;

            // clear out any old data
            if (ClearResponseBuffer() != 0) return 1;

            ports[portIndex].Write(command + "\r");

            string resp = GetResponse(useDevice2, timeout_ms);
            if (resp.IndexOf(expectedReply) != 0)
            {
                MessageBox.Show("Unexpected Reply from piezo controller: " + resp);
                return 1;
            }
            Thread.Sleep(COMMAND_DELAY_BUG);
            return 0;
        }

        // TODO need to update this to work with one of the other DE03.  Right now it seems to clear out both !!
        // PKv4.6.0   TODO need to take care of 2nd DE03


        public static int ClearResponseBuffer()
        {
            try
            {
                // read out anything that's in there
                for (int i = 0; i < ports.Count; i++)
                {
                    ports[i].ReadExisting();
                    //					Comm.ClearReadBuffer(pumpConfiguration.SyringeParams[i-1].PumpCommPort);
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error clearing DE03 com port buffer", "COM PORT ERROR");
                return 1;
            }

            return 0;
        }

        // PKv4.6.0   Added overload for 2nd DE03

        public static string GetResponse()
        {
            return GetResponse(false);
        }

        public static string GetResponse(bool useDevice2)
        {

            int portIndex = 0;
            if (useDevice2) portIndex = 1;

            string retString = "";
            try
            {
                // Assuming the last character is always a CR (\r).  Trim out LF \n anyhow.

                ports[portIndex].NewLine = "\r";                // Added this
                ports[portIndex].ReadTimeout = 100;
                retString = ports[portIndex].ReadLine();
                retString = retString.Trim('\n');
            }
            catch
            {
                return "Error reading com port";
            }
            return retString;
        }



        // Default is to not supress error
        // PKv4.6.0   Added overload for 2nd DE03

        public static string GetResponse(int timeout)
        {
            return GetResponse(false, timeout, false);
        }

        public static string GetResponse(bool useDevice2, int timeout)
        {
            return GetResponse(useDevice2, timeout, false);
        }

        public static string GetResponse(int timeout, bool supressError)
        {
            return GetResponse(false, timeout, supressError);
        }

        public static string GetResponse(bool useDevice2, int timeout, bool supressError)
        {

            int portIndex = 0;
            if (useDevice2) portIndex = 1;
            string retString = "";
            try
            {
                // Assuming the last character is always a CR.
                ports[portIndex].NewLine = "\r";                // Added this
                ports[portIndex].ReadTimeout = timeout;
                retString = ports[portIndex].ReadLine();
                retString = retString.Trim('\n');

                if (retString == null || retString.Length < 1)
                {
                    if (!supressError)
                    {
                        // PKv4.6.0  TODO  Might make this er
                        DialogResult status = MessageBox.Show("Comm Port " + port.ToString() + " timeout 1.", "DE03 Controller", MessageBoxButtons.OK);
                    }
                }
            }
            catch
            {
                if (!supressError)
                {
                    DialogResult status = MessageBox.Show("Comm Port " + port.ToString() + " timeout 2", "DE03 Controller", MessageBoxButtons.OK);
                }
            }
            return retString;
        }

        // PKv4.6.0   Added overload for 2nd DE03

        //  Is there any characters in the incoming communcation buffer
        public static bool ResponseWaiting(bool useDevice2)
        {
            int portIndex = 0;
            if (useDevice2) portIndex = 1;
            if (ports[portIndex].BytesToRead > 0) return true;
            return false;
        }


        public static bool ResponseWaiting()
        {
            return ResponseWaiting(false);
        }

        #endregion

        #region private methods  Actually some of these are now public....
        private static int InitComPort(int desiredPort)
        {
            try
            {
                {
                    SerialPort serialPort = new SerialPort();
                    ManualResetEvent mutex = new ManualResetEvent(false);
                    serialPort.PortName = string.Format("COM{0}", desiredPort);
                    serialPort.BaudRate = 9600;
                    serialPort.DataBits = 8;
                    serialPort.StopBits = StopBits.One;
                    serialPort.Parity = Parity.None;
                    serialPort.Open();

                    serialPort.DtrEnable = true;
                    ports.Add(serialPort);
                    mutexes.Add(mutex);
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error initializing DE03 com port", "COM PORT ERROR");
                return 1;
            }

            return 0;
        }

        private static int CheckForInitialize()
        {
            if (!initialized)
            {
                System.Windows.Forms.MessageBox.Show("Tip firing controller pump not initialized.", "CONTROLLER ERROR");
                return 1;
            }

            return 0;
        }

        #endregion
    }
}