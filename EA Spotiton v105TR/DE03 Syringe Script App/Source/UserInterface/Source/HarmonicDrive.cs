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

// 

namespace EA.PixyControl
{

    public class HarmonicDrive
    {
        private const int COMMAND_TIMEOUT = 5000;
        private const int COMMAND_DELAY_BUG = 0;  // Extra Delay added after every command.
        private const int DEFAULT_MOVE_TIMEOUT_SEC = 30;
        private const int MIN_POSITION_COUNT = -120000;   // Quarter turn is 100,000 counts,  home is about at 0.
        private const int MAX_POSITION_COUNT = 20000;
        private const int HOME_SENS_POSITION_COUNT = -35000;  // This is where the home sensor is.

        private static bool initialized = false;    // RS232 is initialized
        private static int port = 0;

        private static bool homed = false;  // Whether the axis has been homed.

        private static List<SerialPort> ports = new List<SerialPort>();    // New list of serial ports
        private static List<ManualResetEvent> mutexes = new List<ManualResetEvent>();


        #region properties

        // Tells whether it had been initialized or not
        public static bool Initialized
        {
            get { return initialized; }
        }
        #endregion

        public static bool Homed
        {
            get { return homed; }
        }

        #region public methods,  Main User Interface

        public static int InitRS232(int desiredPort)
        {
            // first the com port
            if (InitComPort(desiredPort) != 0) return 1;    // Call the private one.

            port = desiredPort;

            initialized = true;

            return 0;
        }

        // 

        public static int SetParameters(int maxVelocity, int accel, int decel)
        {
            string cmd;
            HarmonicDrive.SendCommand("s r0x24 21", "ok", 100);  // Position mode
            HarmonicDrive.SendCommand("s r0xc8 0", "ok", 100);  // Set trapezoid profile move
            cmd = "s r0xcb " + maxVelocity.ToString();
            HarmonicDrive.SendCommand(cmd, "ok", 100);
            cmd = "s r0xcc " + accel.ToString();
            HarmonicDrive.SendCommand(cmd, "ok", 100);
            cmd = "s r0xcd " + decel.ToString();
            HarmonicDrive.SendCommand(cmd, "ok", 100);
            return 0;
        }

        public static int Home(bool useCounterClockwise)
        {
 
            if (HarmonicDrive.SendCommand("s r0x24 21", "ok", 100)!=0) return 1;  // Position mode
            if (HarmonicDrive.SendCommand("s r0xc8 0", "ok", 100)!=0) return 1;  // Set trapezoid profile move,  256 for relative move.
            if (HarmonicDrive.SendCommand("s r0xc3 50000", "ok", 100) != 0) return 1;  // Fast velocity count
            if (HarmonicDrive.SendCommand("s r0xc4 10000", "ok", 100) != 0) return 1;  // Fast velocity count
            if (HarmonicDrive.SendCommand("s r0xc5 5000", "ok", 100) != 0) return 1;  // Accel / Decel velocity count
            if (useCounterClockwise)
            {
                if (HarmonicDrive.SendCommand("s r0xc6 10000", "ok", 100) != 0) return 1;  // Extra offset for homing
                if (HarmonicDrive.SendCommand("s r0xc2 514", "ok", 100) != 0) return 1;  // Home to the switch, in negative direction
                if (HarmonicDrive.SendCommand("t 2", "ok", 100) != 0) return 1;  // Start homing
                if (WaitForMoveFinish(DEFAULT_MOVE_TIMEOUT_SEC) !=0) return 1;
            }
            if (HarmonicDrive.SendCommand("s r0xc6 0", "ok", 100) != 0) return 1;  // No offset
            if (HarmonicDrive.SendCommand("s r0xc2 562", "ok", 100) != 0) return 1;  // Home to the switch, in negative direction
            if (HarmonicDrive.SendCommand("t 2", "ok", 100) != 0) return 1;  // Start homing

            if (WaitForMoveFinish(DEFAULT_MOVE_TIMEOUT_SEC) != 0) return 1;

            // Assume everything went well and set the count value

            if (HarmonicDrive.SendCommand("s r0x32 " + HOME_SENS_POSITION_COUNT.ToString(), "ok", 100) != 0) return 1;  // Set the offset

            homed = true;

            Move(0, true);  // Move to 0

            return 0;
        }

        public static int CheckRs232Init()
        {
            if (!HarmonicDrive.Initialized)
            {
                MessageBox.Show("HarmonicDrive.CheckRS232Init: ERROR !  RS232 not initialized.");
                return -1;
            }
            else
            {
                return 0;
            }
        }

        public static int Move(int position_count,bool waitForComplete)
        {
            string cmd;

            if (!HarmonicDrive.Homed)
            {
                MessageBox.Show("HarmonicDrive.Move: Error ! Axis is not homed.");
                return -1;
            }

            cmd = "s r0xca " + position_count.ToString();
            HarmonicDrive.SendCommand(cmd, "ok", 100);
            HarmonicDrive.SendCommand("t 1", "ok", 100);
            if (waitForComplete)
            {
                return WaitForMoveFinish(DEFAULT_MOVE_TIMEOUT_SEC);
            }
            return 0;
        }

        public static int GetPosition()
        {
            int position;
            string resp;

            if (HarmonicDrive.SendCommand("g r0x32")!=0) return -1;
            resp = HarmonicDrive.GetResponse(100);

            try
            {
                position = Convert.ToInt32(resp.Substring(2));
            }
            catch
            {
                MessageBox.Show("HarmonicDrive.GetPosition, Error reading position" + resp);
                return 0;
            }
            return position;
        }

        // return 1 - if it is moving
        // return 0 - when it is done (hopefully).
        // return -1 - if an error occurs.

        public static int GetMovingStatusBit()
        {
            int registerStatus;
            string resp;
            if (HarmonicDrive.SendCommand("g r0xc9")!=0) return -1;
            resp = HarmonicDrive.GetResponse(100);

            try
            {
                registerStatus = Convert.ToInt32(resp.Substring(2));
            }
            catch
            {
                MessageBox.Show("HarmonicDrive.GetMovingStatusBit Error Reading " + resp);
                return -1;
            }
            if ((registerStatus & (1<<26)) != 0)   // bit 27, is 1 shifted 26 times.
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static int WaitForMoveFinish(int timeout_sec)
        {

            System.DateTime StartTime = DateTime.Now;
            System.TimeSpan MaxTime = new TimeSpan(0, 0, 0, timeout_sec, 0);

            bool allDone = false;

            // Read back the status byte
            // Loop and check bit while waiting for the timeout

            do
            {
                if (GetMovingStatusBit() == 0) allDone = true; 
                if (allDone) break;
                // give everyone else a chance at the processor
                System.Threading.Thread.Sleep(1);

            } while ((System.DateTime.Now - StartTime) < MaxTime);  // check for move timeout

            // Harmonic Drive move timeout.
            if (!allDone)
            {
                MessageBox.Show("HarmonicDrive.WaitForMoveFinish,   Move timeout");
                return 1;
            }

            return 0;
        }

        #endregion


        #region Public Lower Level Communication Routines (some are public)

        // Don't think this one is used
        //public static int SendCommandNoCR(string command)
        //{
        //    if (HarmonicDrive.CheckRs232Init() != 0) return -1;

        //    // clear out any old data
        //    if (ClearResponseBuffer() != 0) return 1;
        //    ports[0].Write(command);
        //    //			Comm.Write(port, command);
        //    return 0;
        //}


        public static int SendCommand(string command)
        {

            // clear out any old data
            if (ClearResponseBuffer() != 0) return 1;
            ports[0].Write(command + "\r");
            Thread.Sleep(COMMAND_DELAY_BUG);
            return 0;
        }

        /// <summary>
        /// Send Command
        /// </summary>
        /// <param name="command">bytes</param>
        /// <returns></returns>
        //public static int SendCommand(byte[] buffer)
        //{
        //    // clear out any old data
        //    if (ClearResponseBuffer() != 0) return 1;

        //    ports[0].Write(buffer, 0, buffer.Length);

        //    //			Comm.WriteBytes(port, buffer);
        //    Thread.Sleep(COMMAND_DELAY_BUG);
        //    return 0;
        //}

        //		Overload.
        //		expectedReply string will look for and match an exact expected reply string.
        public static int SendCommand(string command, string expectedReply, int timeout_ms)
        {

            // clear out any old data
            if (ClearResponseBuffer() != 0) return 1;

            ports[0].Write(command + "\r");

            string resp = GetResponse(timeout_ms);
            if (resp.IndexOf(expectedReply) != 0)
            {
                MessageBox.Show("Unexpected Reply from harmonic drive controller: " + resp);
                return 1;
            }
            Thread.Sleep(COMMAND_DELAY_BUG);
            return 0;
        }

        private static int InitComPort(int desiredPort)
        {
            try
            {
                {
                    //					Aurigin.Xml profile = new Aurigin.Xml(@"..\data\commPort.xml");
                    //					port = (short)profile.GetValue("Pipette", "Port", 4);

                    // Set the port to 9600 baud, no parity bit, 8 data bits, 1 stop bit (all standard)
                    //					Comm.Open(desiredPort, 9600, 8, Rs232.DataParity.Parity_None, Rs232.DataStopBit.StopBit_1);
                    // Force the DTR line high, used sometimes 
                    //					Comm.DTR(desiredPort, true);

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

                    Console.WriteLine("   Intializing Harmonic Drive Serial Port: {0}", serialPort.PortName);
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error initializing Harmonic Drive com port", "COM PORT ERROR");
                return 1;
            }

            return 0;
        }

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
                System.Windows.Forms.MessageBox.Show("Error clearing Harmonic Drive com port buffer", "COM PORT ERROR");
                return 1;
            }

            return 0;
        }

        public static string GetResponse()
        {
            string retString = "";
            try
            {
                // Assuming the last character is always a CR (\r).  Trim out LF \n anyhow.

                ports[0].NewLine = "\r";                // Added this
                ports[0].ReadTimeout = 100;
                retString = ports[0].ReadLine();
                retString = retString.Trim('\n');
            }
            catch
            {
                return "HarmonicDrive.GetResponse:  Error reading com port";
            }
            return retString;
        }

        // Default is to not supress error

        public static string GetResponse(int timeout)
        {
            return GetResponse(timeout, false);
        }

        public static string GetResponse(int timeout, bool supressError)
        {
            string retString = "";
            try
            {
                // Assuming the last character is always a CR.
                ports[0].NewLine = "\r";                // Added this
                ports[0].ReadTimeout = timeout;
                retString = ports[0].ReadLine();
                retString = retString.Trim('\n');

                if (retString == null || retString.Length < 1)
                {
                    if (!supressError)
                    {
                        DialogResult status = MessageBox.Show("Comm Port " + port.ToString() + " timeout 1.", "Harmonic Drive Controller", MessageBoxButtons.OK);
                    }
                }
            }
            catch
            {
                if (!supressError)
                {
                    DialogResult status = MessageBox.Show("Comm Port " + port.ToString() + " timeout 2", "Harmonic Drive Controller", MessageBoxButtons.OK);
                }
            }
            return retString;
        }

        //  Is there any characters in the incoming communcation buffer

        public static bool ResponseWaiting()
        {
            if (ports[0].BytesToRead > 0) return true;
            return false;
        }

        #endregion
    }
}