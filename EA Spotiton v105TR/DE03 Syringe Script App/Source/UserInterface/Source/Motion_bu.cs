//#define PIXY_3_AXIS       // Symbols will be defined in project properties,  3 axis operation for PIXY
//#define SPOTITON_4_AXIS   // Symbols will be defined in project properties,  4 axis operation for SPOTITON
// PKv4.0,2015-04-27,      Added many more motion parameters to the xml file.  Homing speeds and default speeds.
// PKv5.2.7 ,  MoveDouble to speed up on the fly dispense:   2016-06-15
// PKv5.3.2,  2016-08-31  Speeding up the move to Nitrogen
// PKv5.3.3,  2016-09-12, High Speed HOP.
// PKv5.3.3.1,  2016-09-23, Bug fix


using System;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using EA.PixyControl.ClassLibrary;
using EA.PixyControl;
using System.IO.Ports;  // PKv3.0
using System.Collections.Generic;
using System.Threading;
using Aurigin;    // For the IO Control


namespace EA.PixyControl
{

	// things we need to know about each axis
	public class AxisParameters
	{
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string	AxisName;

		// servo parameters
		public double	DistResolution_mmPerEncCount;       // Lead Screw Pitch / Encoder Counts per revolution.

		// homing
		public bool		HomeInReverseDirection;  
        public double   IfHomeToIndexXtraMveB4Index_mm;  // Added to help move away from limit before starting to search for index 
		public double	HomingMoveToLimitSpeed_mmpersec;  
		public double	HomingFindHomeSpeed_mmpersec;
		public int		HomeTimeout_sec;
		public bool		LimitSwitchInverted;	// PK  True if the limit bits are on and then switch off.

		// motion parameters                           //pkV4.0,2015-04-27,  These were update extensively
        public double   Vel_Max_Limit_mmpersec;        // Used by MoveSafely command,  Can be updated by SetMaxVelocity() routine.
        public double   Vel_Default_mmpersec;          // Read from file: Used only by  SetDefaultMotionParameters( )
        public int      Vel_Initial_counts;            // Read from file: Used only by  SetDefaultMotionParameters( ),  VI command to lexium motor.
        public double   Accel_Max_Limit_mmpersec2;     // Updated by SetMaxAccel(),  written to motion controller at that time too.
        public double   Decel_Max_Limit_mmpersec2;     // Updated by SetMaxAccel(),  written to motion controller at that time too
        public double   Accel_Default_mmpersec2;      // Read from file: Used only by  SetDefaultMotionParameters( )   
        public double   Decel_Default_mmpersec2;      // Read from file: Used only by  SetDefaultMotionParameters( )
		public double	MinPosition_mm;
		public double	MaxPosition_mm;
        public double   HomePosition_mm;
        public int      HomeA_counts;
        public int      HomeD_counts;
        public int      HomeVI_counts;
        public int      HomeVM_counts;

        public bool     HomeToIndex;                // Set to true if you want to use zindex homing and verification check
        public bool     HomeToIndexInNegativeDirection;

        public int      IndexToLimitExpectedValueSteps;
        public int      IndexToLimitToleranceSteps;   

	}

	// this class gets serialized to XML
	public class MotionConfigurationData
	{
		public int				ComPort;				// how we talk to the device
        public int              ComPortGrid;            // Grid Axis will use a 2nd com port.  PKv4.0,2015-03-02
		public double			DefaultSpeed_pct;		// default speed for moves - 0.0 to 100.0
		public double			SafeZ;					// a safe height for moves, with pipette robot
        public double           SafeZGrid;              // a safe height for moves, with gridRobot                
		public double			ZTolerance_mm;			// if two z's differ by this or less we'll consider them the same

		[XmlArrayItem("AXIS", typeof(AxisParameters))]
		public AxisParameters[]	AxisData;				// much information about each axis
	}


	// info returned by the motion system    // TODO Need to study how errors are returned.  Do these default to false
	public class ServoStatus
	{
		public bool		MoveDone;			//set when move done (trap. pos mode), when goal vel. has been reached (vel mode) or when not servoing
		public bool		CheckSumError;		//checksum error in received command or some other comm error.
		public bool		Overcurrent;		//set on overcurrent condition (sticky bit)
		public bool		PowerOn;			//set when motor power is on
		public bool		PositionError;		//set on excess pos. error (sticky bit)
		public bool		AtNegativeLimit;	//value of limit 1 input (negative)
		public bool		AtPositiveLimit;	//value of limit 2 input (positive)
		public bool		HomeInProgress;		//set while searching for home, cleared when home found

		public bool	ErrorOccurred()
		{
			return (PositionError || Overcurrent || CheckSumError);
		}
	}


	// more info it can give you
	public class ServoAuxStatus
	{
		public bool		EncoderIndex;				//value of the encoder index signal
		public bool		PositionCounterOverflow;	//set when 32 bit position counter wraps around (sticky bit)
		public bool		ServoOn;					//set when position servo is operating
		public bool		AccelerationDone;			//set when acceleration portion of a move is done
		public bool		SlewDone;					//set when slew portion of a move is done
		public bool		ServoOverrun;				//set if servo takes longer than the specified servo period to execute
		public bool		PathMotionInProgress;		// path mode is enabled

		public bool ErrorOccurred()
		{
			return (PositionCounterOverflow || ServoOverrun);
		}
	}
	

	// object wrapper for a boolean
	// something that the motion controller can check while it's waiting for a move to complete
	public class SoftwareStopButton
	{
		public bool	StopNow = false;
	}


	// low level control of motion
	// there is one instance of this class and it talks directly to the motion system
	// accessed by the user motion control class and directly by a user sometimes.
	// talks to the device in terms of encoder counts and servo ticks
	// talks its motion control class in terms of mm
	// axis numbers are zero-based: X=0, Y=1, Z=2 (currently no checking on that since we just have one user object and we trust it)
	public class ServoControl
	{
		#region constants and enums

		public const int			BAUD_RATE = 19200;
        public const int            NUM_AXES = 6;           
        public const int			X_AXIS = 0;
        public const int			Y_AXIS = 1;
        public const int            Z_AXIS = 2;

        public const int            X2_AXIS = 3;
        public const int            Y2_AXIS = 4;
        public const int            Z2_AXIS = 5;

        public const int			AXISMASK_X = 1;
		public const int			AXISMASK_Y = 2;
        public const int            AXISMASK_Z = 4;
        public const int            AXISMASK_X2 = 8;
        public const int            AXISMASK_Y2 = 16;
        public const int            AXISMASK_Z2 = 32;

        public const int			AXISMASK_ALL = AXISMASK_X | AXISMASK_Y | AXISMASK_Z;
        public const int            AXISMASK_ALL_GRID = AXISMASK_X2 | AXISMASK_Y2 | AXISMASK_Z2;  
		public const int			DEFAULT_MOVE_TIMEOUT_SEC = 100;  // 100 seconds

        public const int            PIPETTE_ROBOT = 1;
        public const int            GRID_ROBOT = 2;

        public const int            COMM_ISSUE_DELAY = 10;      // add delay to attempt to solve comm issue.


        // PKv3.0
        private static int port = 0;
        private static List<SerialPort> ports = new List<SerialPort>();    // New list of serial ports
        private static List<ManualResetEvent> mutexes = new List<ManualResetEvent>();

        private const int COMMAND_TIMEOUT = 5000;  // Up to how long to wait when a command is supposed to respond with data msec

        //  PKv5.5.5.1   Commented out the following 3 lines and replaced values.   Attempting to eliminate the communications glitch.
        //        private const int COMMAND_DELAY_BUG_BEFORE = 10;  // Extra Delay added BEFORE every command is sent (every Command).  msec, PKv4.0,2015-04-15,  from 1 to 10
        //        private const int COMMAND_DELAY_BUG = 10;  // Extra Delay added after every command is sent (every Command). 
        //        private const int COMMAND_WAIT_FOR_CR = 50;  // Up to how long  to wait for the CR after each command  msec

        private const int COMMAND_DELAY_BUG_BEFORE = 15;    // PKv5.5.5.1  Changed from above values
        private const int COMMAND_DELAY_BUG = 15;           // PKv5.5.5.1  Changed from above values
        private const int COMMAND_WAIT_FOR_CR = 100;        // PKv5.5.5.1  Changed from above values

        private static bool initialized = false;

        private static string lastCommand;          // PKv5.5.5  Lets keep track of the last command sent to motors.

		public enum MOVE_RESULT
		{
			MOVE_SUCCESS = 0,
			MOVE_ERROR = 1,
			MOVE_TIMEOUT = 2
		}

		private enum HOME_METHOD
		{
			HOME_TO_INDEX = 0,
			HOME_TO_NEGATIVE_LIMIT = 1,
			HOME_TO_POSITIVE_LIMIT = 2
		}

		#endregion

		#region members

		// want to leave homing method configurable for setup and testing but not for the user
		// once the teachpoints are determined you could really create a mess by homing to a different place
		// note: existing EA code indicates that Limit1 = negative limit, Limit2 = positive limit

		//													X						Y							Z                  
        private HOME_METHOD[] HomingMethod = { HOME_METHOD.HOME_TO_INDEX, HOME_METHOD.HOME_TO_INDEX, HOME_METHOD.HOME_TO_INDEX };

		private AxisParameters[]	AxisParams = new AxisParameters[NUM_AXES];

        private static double[] Last_Commanded_Pos_mm = { -999.999, -999.999, -999.999, -999.999, -999.999, -999.999 };   // PKv3.0

        private bool useLastCommanded_test = false;               //  PKv5.3.2

        private int[] AxisMasks = { AXISMASK_X, AXISMASK_Y, AXISMASK_Z, AXISMASK_X2, AXISMASK_Y2, AXISMASK_Z2 };


        private int					comPort;
        private int                 comPortGrid;
		private bool				moveInProgress = false;
		private bool				unhandledMotionError = false;
		private SoftwareStopButton	stopRequested;
//		private bool				initialized = false;

		#endregion

		#region properties

		public bool MoveStarted
		{
			get {return moveInProgress;}
		}

		public bool MotionError
		{
			get {return unhandledMotionError;}
		}

		public bool Initialized
		{
			get {return initialized;}
		}

		
		#endregion
		
		#region public methods
	
		// initialize with a com port to talk to and a file where you can find the parameters
		// don't move anythng yet
		public ServoControl(MotionConfigurationData MotionConfigData, SoftwareStopButton StopButton)
		{
			comPort					= MotionConfigData.ComPort;
            comPortGrid             = MotionConfigData.ComPortGrid;
			stopRequested			= StopButton;
			initialized				= false;
			moveInProgress			= false;
			unhandledMotionError	= false;

			// point to the motion parameter objects
			for (int Axis = 0; Axis < NUM_AXES; ++Axis)
			{
				AxisParams[Axis] = MotionConfigData.AxisData[Axis];
			}
		}

        // PKv3.0  Added Direct  comm routines to the motion controller adapted from the DE03 communications routines
        // Still a decent bit of cleanup needed.
        # region Low Level RS232 comm routines.

        // Don't think this is used anymore.

        //public static int SendCommandNoCR(string command)
        //{

        //    // clear out any old data
        //    if (ClearResponseBuffer() != 0) return 1;
        //    ports[0].Write(command);
        //    //			Comm.Write(port, command);
        //    return 0;
        //}

        // Send a command to an individually addressed axis
        // PKv5.5.5  Keep track of the last command sent to the motors.

        public static int SendCommand(string address,string command)
        {
            int axisInt = Convert.ToInt32(address);         // 1 based axis
            int portIndex = 0;
            if (axisInt > 3) portIndex = 1;

            // clear out any old data
            if (ClearResponseBuffer(portIndex) != 0) return 1;
            if (COMMAND_DELAY_BUG_BEFORE > 0) Thread.Sleep(COMMAND_DELAY_BUG_BEFORE);
            ports[portIndex].Write(address + command + "\x000A");    // Control J terminator
 //           Console.WriteLine("To Motor =>[{0}]{1}", address, command);
            Thread.Sleep(COMMAND_DELAY_BUG);

            lastCommand = string.Format("Command = {0}, Port (0=Tip,1=Grid) = {1}, Axis Address (1-X,3-Z)= {2}", command, portIndex, address);

            string resp = GetResponse(portIndex,COMMAND_WAIT_FOR_CR, true);   // This should just be null followed by "CR" so suppress error.
            return 0;
        }


        private static int InitComPort(int desiredPort)
        {
            try
            {
                {
                    string comPortName = "COM" + desiredPort.ToString();
                    Console.WriteLine("   Intializing Motor Serial Port: {0}", comPortName);

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
                System.Windows.Forms.MessageBox.Show("Error initializing Servo com port", "COM PORT ERROR");
                return 1;
            }

            return 0;
        }

        public static int ClearResponseBuffer(int portIndex)        // PKv4.0,2015-03-02   Ability to clear one of the other ports.
        {                                                           // portIndex 0 will be the pipette,  portIndex 1 will be the Grid robot.
          try
            {
                ports[portIndex].ReadExisting();
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error clearing Servo com port buffer", "COM PORT ERROR");
                return 1;
            }
            return 0;
        }

        public static int ClearResponseBuffer()                 // The old version.
        {
            try
            {
                // read out anything that's in there
                for (int i = 0; i < ports.Count; i++)
                {
                    ports[i].ReadExisting();
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error clearing Servo com port buffer", "COM PORT ERROR");
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

                ports[0].NewLine = "\n";                // used to be "\r"
                ports[0].ReadTimeout = 100;
                retString = ports[0].ReadLine();
                retString = retString.Trim('\r');   // used to be "\n"
            }
            catch
            {
                return "Error reading com port";
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
                // Assuming the last character is always a LF.
                ports[0].NewLine = "\n";                // used to be "\r"
                ports[0].ReadTimeout = timeout;
                retString = ports[0].ReadLine();    
    //          retString = ports[0].ReadExisting();
                retString = retString.Trim('\r');
 //             Console.WriteLine("From Motor <={0}", retString);

                if (retString == null || retString.Length < 1)
                {
                    if (!supressError)
                    {
                        DialogResult status = MessageBox.Show("Comm Port " + port.ToString() + " timeout 1.", "Servo Controller", MessageBoxButtons.OK);
                    }
                }
            }
            catch
            {
                if (!supressError)
                {
                    DialogResult status = MessageBox.Show("Comm Port " + port.ToString() + " timeout 2", "Servo Controller", MessageBoxButtons.OK);
                }
                retString = ports[0].ReadExisting();  // Might help the recovery by clearing it out
            }
            return retString;
        }

        //  Is there any characters in the incoming communcation buffer

        public static bool ResponseWaiting()
        {
            if (ports[0].BytesToRead > 0) return true;
            return false;
        }

        public static string GetResponse(int portIndex,int timeout)
        {
            return GetResponse(portIndex, timeout, false);
        }

        // PKv5.5.5  Added some better error catching and a way to abort the application.

        public static string GetResponse(int portIndex, int timeout, bool supressError)
        {
            string retString = "";
            try
            {
                // Assuming the last character is always a LF.
                ports[portIndex].NewLine = "\n";                // used to be "\r"
                ports[portIndex].ReadTimeout = timeout;
                retString = ports[portIndex].ReadLine();
                //          retString = ports[0].ReadExisting();
                retString = retString.Trim('\r');
                //             Console.WriteLine("From Motor <={0}", retString);

                if (retString == null || retString.Length < 1)
                {
                    if (!supressError)
                    {
                        DialogResult status = MessageBox.Show("Comm Port " + ports[portIndex].PortName + " timeout 1.", "Servo Controller", MessageBoxButtons.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!supressError)
                {
                    string errorMessage = string.Format("***Serious Error****\nLast Command={0}\n\nException={1}\n\nPortIndex ={2}, timeout 2\n\n\nPlease  terminate the application, No will ignore (Dangerous!)", lastCommand, ex.ToString(), portIndex);
                    DialogResult status = MessageBox.Show(errorMessage, "Aw Snap !", MessageBoxButtons.YesNo);
                    if (status!=DialogResult.No)
                    {
                        // Harsh -- Just shuts down the whole application,  Endless loop trying to close down the application.
                        while (true)
                        {
                            System.Windows.Forms.Application.Exit();
                            Thread.Sleep(1000);
                        }
                    }
                }
                retString = ports[portIndex].ReadExisting();  // Might help the recovery by clearing it out
            }
            return retString;
        }

        //  Is there any characters in the incoming communcation buffer

        public static bool ResponseWaiting(int portIndex)
        {
            if (ports[portIndex].BytesToRead > 0) return true;
            return false;
        }

        # endregion


		public int InitializeMotion(bool SkipHoming)
		{
			// make sure the axis controllers are with us
			// PK - This also resets the motion controllers and closes a comm port if
			// it was open

            if (InitComPort(comPort)!=0) return 1;
            if (InitComPort(comPortGrid) != 0) return 1;

			if (!AllAxesFound()) return 1;

			// set the servo parameters,  Not doing anything for now.
			if (SetAllAxisParameters() != 0) return 1;

			// home all axes
			if (!SkipHoming)
			{
				if (HomeAllAxes() != 0) return 1;	
			}

			// set the flag so we can move
			initialized = true;

			return 0;	
		}


		// find the position we're going to call zero
		public int HomeAllAxes()
		{
			if (HomeAxis(Z_AXIS) != 0) return 1;
			if (HomeAxis(X_AXIS) != 0) return 1;
			if (HomeAxis(Y_AXIS) != 0) return 1;


			// homing all successfully = recovering from motion error
			this.unhandledMotionError = false;

			return 0;
		}

        // pass in a zero-based axis  (0 to 5).
        // find the desired limit and call it home
        // reset the counter so that home is zero counts
        // Add more parameters to AxisParams[Axis].  xml file to better control homing parameter:  .HomeA_counts, .HomeD_counts, .HomeVI_counts, .HomeVM_counts
        // PKv4.0,2015-05-07 Added option to home to index and verification of tolerance.

        public int HomeAxis(int Axis)
        {
            byte AxisAddress = (byte)(Axis + 1);
            int[] axisMask = { AXISMASK_X, AXISMASK_Y, AXISMASK_Z, AXISMASK_X2, AXISMASK_Y2, AXISMASK_Z2 };

            if (ClearError(Axis) != 0) return -1;       //PKv4.0,2015-04-28  Added to try and get rid of start up issues right after motors power on.
 
            if (SendCommand(AxisAddress.ToString(), "EE 1") != 0) return -1;
            if (SendCommand(AxisAddress.ToString(), "A " + AxisParams[Axis].HomeA_counts.ToString()) != 0) return -1;        
            if (SendCommand(AxisAddress.ToString(), "D " + AxisParams[Axis].HomeD_counts.ToString()) != 0) return -1;   

            if (SendCommand(AxisAddress.ToString(), "MS 200") != 0) return -1;

            if (SendCommand(AxisAddress.ToString(), "IS 2,1,0") != 0) return -1;
            if (SendCommand(AxisAddress.ToString(), "VI 5") != 0) return -1;   // Alwasy set VI low temporarily so VI<VM  
            if (SendCommand(AxisAddress.ToString(), "VM " + AxisParams[Axis].HomeVM_counts.ToString()) != 0) return -1;
            if (SendCommand(AxisAddress.ToString(), "VI " + AxisParams[Axis].HomeVI_counts.ToString()) != 0) return -1;  

            if (AxisParams[Axis].HomeInReverseDirection)
            {
                if (SendCommand(AxisAddress.ToString(), "HM 3") != 0) return -1;
            }
            else
            {
                if (SendCommand(AxisAddress.ToString(), "HM 1") != 0) return -1;
            }
            moveInProgress = true;

            if (WaitForMoveToFinish(axisMask[Axis], ServoControl.DEFAULT_MOVE_TIMEOUT_SEC) != 0) return -1;
            if (SetAxisPosition_mm(Axis, AxisParams[Axis].HomePosition_mm) != 0) return -1;

           //PKv4.0,2015-05-07 Added option to home to index and verification of tolerance.

            if (AxisParams[Axis].HomeToIndex)
            {
                if (SendCommand(AxisAddress.ToString(), "VI 5") != 0) return -1;   // Alwasy set VI low temporarily so VI<VM
                if (SendCommand(AxisAddress.ToString(), "VM 500") != 0) return -1;
                if (SendCommand(AxisAddress.ToString(), "VI 100") != 0) return -1;

                if (AxisParams[Axis].HomeToIndexInNegativeDirection)
                {
                    if (SendCommand(AxisAddress.ToString(), "HI 1") != 0) return -1;
                }
                else
                {
                    if (SendCommand(AxisAddress.ToString(), "HI 3") != 0) return -1;
                }

                moveInProgress = true;
                if (WaitForMoveToFinish(axisMask[Axis], ServoControl.DEFAULT_MOVE_TIMEOUT_SEC) != 0) return -1;

                double axisPosition = GetAxisPosition_mm(Axis);
                double foundIndexSteps = Math.Abs((AxisParams[Axis].HomePosition_mm - axisPosition) / AxisParams[Axis].DistResolution_mmPerEncCount);

                string logMessage = string.Format("Axis={0}, Index Loc in mm={1},  Found Index To Limit in Steps ={2:0} , ", Axis, axisPosition,foundIndexSteps);

                if (Math.Abs(foundIndexSteps - AxisParams[Axis].IndexToLimitExpectedValueSteps) > AxisParams[Axis].IndexToLimitToleranceSteps)
                {
                    MessageBox.Show("Home Index found outside of tolerance\n" + logMessage, "Motion.cs HomeAxis:  Error");
                    Datalog log = new Datalog(@"C:\DE03 Syringe Script App\LogFiles\HomingLog.txt");
                    log.WriteLine(Datalog.Timestamp());
                    log.WriteLine("*****HOME ERROR******");
                    log.WriteLine(logMessage);
                    log.Close();
                    Console.WriteLine(logMessage);
                    return -1;
                }

                if (SetAxisPosition_mm(Axis, AxisParams[Axis].HomePosition_mm) != 0) return -1;          

                Datalog log2 = new Datalog(@"C:\DE03 Syringe Script App\LogFiles\HomingLog.txt");
                log2.WriteLine(Datalog.Timestamp());
                log2.WriteLine(logMessage);
                log2.Close();
                Console.WriteLine(logMessage);
            }

  
 //             These now get set with our SetDefaultMotionParameters method:
 //             if (SendCommand(AxisAddress.ToString(), "VI 40") != 0) return -1;        //PKv4.0,2015-04-21,try 40 to 4 ?
 //              if (SendCommand(AxisAddress.ToString(), "VM 1000") != 0) return -1;      //4000 to 1000

            return 0;
        }


		// pass in a zero-based axis
		// find the desired limit and call it home
		// reset the counter so that home is zero counts
        // TODO - Add more parameters to AxisParams[Axis].  xml file to better control homing parameter:  .HomeA_cnts, .HomeD_cnts, .HomeVI_cnts, .HomeVM_cnts

		public int OLDHomeAxis(int Axis)
		{
			byte	AxisAddress = (byte)(Axis+1);
            int[] axisMask = { AXISMASK_X, AXISMASK_Y, AXISMASK_Z, AXISMASK_X2, AXISMASK_Y2, AXISMASK_Z2 };

            // PKv4.0,2015-02-27, Changed parameters to test Z plunge (Z2)
            // PKv4.0,2015-04-06, Added parameters for homing different directions.
            if (Axis == 5)   // Plunge axis homing stuff (Special)
            {
                if (SendCommand(AxisAddress.ToString(), "EE 1") != 0) return -1;                
                if (SendCommand(AxisAddress.ToString(), "A 5000") != 0) return -1;   //2015-04-21, Changed from 40K to 5000     
                if (SendCommand(AxisAddress.ToString(), "D 5000") != 0) return -1;   //2015-04-21, Changed from 40K to 5000

                if (SendCommand(AxisAddress.ToString(), "MS 200") != 0) return -1;

                if (SendCommand(AxisAddress.ToString(), "IS 2,1,0") != 0) return -1;
                if (SendCommand(AxisAddress.ToString(), "VI 40") != 0) return -1;     // Changed from 250 to 40
                if (SendCommand(AxisAddress.ToString(), "VM 1000") != 0) return -1;

                if (AxisParams[Axis].HomeInReverseDirection)
                {
                    if (SendCommand(AxisAddress.ToString(), "HM 3") != 0) return -1;
                }
                else
                {
                    if (SendCommand(AxisAddress.ToString(), "HM 1") != 0) return -1;
                }
                moveInProgress = true;

                if (WaitForMoveToFinish(axisMask[Axis], ServoControl.DEFAULT_MOVE_TIMEOUT_SEC) != 0) return -1;
                if (SetAxisPosition_mm(Axis, AxisParams[Axis].HomePosition_mm) != 0) return 1;
                if (SendCommand(AxisAddress.ToString(), "VI 40") != 0) return -1;        //PKv4.0,2015-04-21,try 40 to 4 ?
                if (SendCommand(AxisAddress.ToString(), "VM 1000") != 0) return -1;      //4000 to 1000
            }
            if ((Axis == 0) || (Axis == 1) || (Axis == 2) || (Axis == 3) || (Axis == 4)) // X,Y,Z,X2,Y2,
            {
                if (SendCommand(AxisAddress.ToString(), "EE 1") != 0) return -1;
                if (SendCommand(AxisAddress.ToString(), "A 400000") != 0) return -1;        
                if (SendCommand(AxisAddress.ToString(), "D 400000") != 0) return -1;

                if (SendCommand(AxisAddress.ToString(), "MS 200") != 0) return -1;

                if (SendCommand(AxisAddress.ToString(), "IS 2,1,0") != 0) return -1;
                if (SendCommand(AxisAddress.ToString(), "VI 500") != 0) return -1;      
                if (SendCommand(AxisAddress.ToString(), "VM 10000") != 0) return -1;

                if (AxisParams[Axis].HomeInReverseDirection)
                {
                    if (SendCommand(AxisAddress.ToString(), "HM 3") != 0) return -1;
                }
                else
                {
                    if (SendCommand(AxisAddress.ToString(), "HM 1") != 0) return -1;
                }
                moveInProgress = true;

                if (WaitForMoveToFinish(axisMask[Axis], ServoControl.DEFAULT_MOVE_TIMEOUT_SEC) != 0) return -1;
                if (SetAxisPosition_mm(Axis, AxisParams[Axis].HomePosition_mm) != 0) return -1;
                if (SendCommand(AxisAddress.ToString(), "VI 40") != 0) return -1;
                if (SendCommand(AxisAddress.ToString(), "VM 40000") != 0) return -1;
            }
            return 0;
            }

        // PKv5.3.3 // Added an override to allow us to use a programmable pulse width.

        public int ArmTriggerOut(int Axis, double relativePosition_mm, bool useGridRobot)
        {
            return ArmTriggerOut(Axis, relativePosition_mm, useGridRobot, 50);
        }

        // PKv5.2.5 Added for producing a trigger for on-the-fly dispesne.

        public int ArmTriggerOut(int Axis, double relativePosition_mm, bool useGridRobot, int pulseWidth)
        {
            int AxisAddress = Axis + 1;
            int arrayIndex = Axis;
            if (useGridRobot)
            {
                AxisAddress = AxisAddress + 3;        // Add 3 to the axis.
                arrayIndex = arrayIndex + 3;
            }

            int relativePosition_encCounts = Convert.ToInt32(relativePosition_mm / AxisParams[arrayIndex].DistResolution_mmPerEncCount);
            string command = string.Format("CW={0}", pulseWidth);                    //  Use the passed in pulse width.
            if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;        //   PKv5.3.3.1,   Bug fix for version PKv5.3.3

            //          string command = string.Format("TR={0},0,1", relativePosition_encCounts); // Only one trigger output
            command = string.Format("TR={0},0,0", relativePosition_encCounts); // Triggers repeat ( for trapezoid mode)
            Console.WriteLine("  Arming for OTF trigger ... command to motor is: {0}", command);
            if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;        //  Set relative position to trip.
            if (SendCommand(AxisAddress.ToString(), "OS=3,28,0") != 0) return 1;    //  Output of the motor in trip mode.
            if (SendCommand(AxisAddress.ToString(), "TE=0") != 0) return 1;         //  Toggle Trip Enable
            if (SendCommand(AxisAddress.ToString(), "TE=16") != 0) return 1;        //  Toggle Trip Enable  (Should be good to go !!)
            return 0;
        }

        // PKv5.2.5 Added for creating one trigger pulse (for debug).

        public int ToggleTriggerOut(int Axis, bool useGridRobot)
        {
            int AxisAddress = Axis + 1;
            int arrayIndex = Axis;
            if (useGridRobot)
            {
                AxisAddress = AxisAddress + 3;        // Add 3 to the axis.
                arrayIndex = arrayIndex + 3;
            }

            if (SendCommand(AxisAddress.ToString(), "OS=3,16,0") != 0) return 1;        //  Set pulse width.
            if (SendCommand(AxisAddress.ToString(), "O3=1") != 0) return 1;    //  Output of the motor in trip mode.
            Thread.Sleep(10);
            if (SendCommand(AxisAddress.ToString(), "O3=0") != 0) return 1;         //  Toggle Trip Enable
            Thread.Sleep(10);
            return 0;
        }



        // PKv4.0,2015-04-15
        // pass in 0 based axis. (0 to 2)

        public int SetAccel(int Axis, double accel_mm_per_s_s, double decel_mm_per_s_s, bool useGridRobot)
        {
            int AxisAddress = Axis + 1;
            int  arrayIndex = Axis;
            if (useGridRobot) 
            {
                AxisAddress = AxisAddress + 3;        // Add 3 to the axis.
                arrayIndex = arrayIndex + 3;
            }

            int accelEncCounts = Convert.ToInt32(accel_mm_per_s_s / Math.Abs(AxisParams[arrayIndex].DistResolution_mmPerEncCount)); // PKv3.00
            int decelEncCounts = Convert.ToInt32(decel_mm_per_s_s / Math.Abs(AxisParams[arrayIndex].DistResolution_mmPerEncCount)); // PKv3.00

            string command = "A " + accelEncCounts.ToString();

            Console.WriteLine("Setting Accel and Decel Encoder Counts = {0}, {1}", accelEncCounts, decelEncCounts);
            Console.WriteLine("Axis 1 to 6 is: {0}",AxisAddress);
            if (SendCommand(AxisAddress.ToString(), command)!= 0) return 1;
            command = "D " + decelEncCounts.ToString();                                 //2015-04-28,  Bug Fix
            if (SendCommand(AxisAddress.ToString(), command)!= 0) return 1;
            return 0;
        }

        // PKv5.3.3  Writing to the motors.
        // Axis 0 to 2;

        public int SetVelocity(int Axis, double velocity_mm_s, bool useGridRobot)
        {
            int AxisAddress = Axis + 1;
            int arrayIndex = Axis;
            if (useGridRobot)
            {
                AxisAddress = AxisAddress + 3;        // Add 3 to the axis.
                arrayIndex = arrayIndex + 3;
            }

            int velocity = ConvertToServoVelocity(velocity_mm_s, Math.Abs(AxisParams[arrayIndex].DistResolution_mmPerEncCount));
            string command = "VM " + velocity.ToString();
            if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;
            return 0;
        }

        // 0 base Axis,  0 to 2
        // PKv4.0,2015-05-08
        // This routine will temporarily set VM to VI+1   (VM gets updated for every move)

        public int SetVI(int Axis, int VI_counts, bool useGridRobot)
        {
            int AxisAddress = Axis + 1;
            int arrayIndex = Axis;
            if (useGridRobot)
            {
                AxisAddress = AxisAddress + 3;        // Add 3 to the axis.
                arrayIndex = arrayIndex + 3;
            }

            if (SendCommand(AxisAddress.ToString(), "VI 1") != 0) return 1;   // Set a very low value temporarily avoid any issue with VM being less than VI

            string command = "VM " + (VI_counts+10).ToString();     //
            if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;


            command = "VI " + VI_counts.ToString();
            if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;

            Console.WriteLine("Setting VI = {0}", VI_counts);
            Console.WriteLine("Axis 1 to 6 is: {0}", AxisAddress);
            return 0;
        }

       // Pass in 0 based Axis (0 to 5);

       public int ClearError(int Axis)

       {
            int portIndex = 0;
            if (Axis >= 3) portIndex = 1;   // Grid robot


           if (SendCommand((Axis + 1).ToString(), "PR ER")!=0) return -1;
           string resp = GetResponse(portIndex,COMMAND_TIMEOUT);
           if (resp != "0")
           {
               Console.WriteLine("ServoControl: ClearError for 0 based axis {0} = {1}",Axis,resp);
           }
           // Now clear the error.
            if (SendCommand((Axis + 1).ToString(), "ER 0")!=0) return -1;
           return 0;
       }
 
		
        // PKv4.0,2015-04-07 Should work OK for 6 axis machine.
		// Move is the one and only move command - nothing else tells the servos to go
		// the client should set the axis mask so that only the desired axes move
		// axis movements start at approximately the same time
		// two overloads to make the limits optional
		public int Move(double[] Position_mm, double [] Velocity_mmpersec, int AxisMask, bool WaitForFinish)
		{

            // Check soft limits
            for (int Axis = 0; Axis < NUM_AXES; ++Axis)
            {
                if (ThisAxisSelected(AxisMask, Axis))
                {
                    if ((Position_mm[Axis] > AxisParams[Axis].MaxPosition_mm) || (Position_mm[Axis] < AxisParams[Axis].MinPosition_mm))
                    {
                        string message = String.Format("Attempt to move Axis {0}  Past soft limit.  Command position {1}", Axis, Position_mm[Axis]);
                        MessageBox.Show(message,"Motion Error",MessageBoxButtons.OK);
                        return -1;
                    }
                }
            }
           return Move(Position_mm, Velocity_mmpersec, AxisMask, WaitForFinish,0);   // Ignore Limits
		}


		public int Move(double[] Position_mm, double [] Velocity_mmpersec, int AxisMask, bool WaitForFinish, byte MotionLimitMask)
		{
			byte	AxisAddress;
            int movingAxisMask=0;       // Might want to make this a static global. 
			
			if (!Initialized)
			{
				System.Windows.Forms.MessageBox.Show("ERROR: Motion control not initialized", "MOTION ERROR");
				return 1;
			}

			// if we had a motion error that hasn't been handled, return
			if (this.unhandledMotionError)
			{
				System.Windows.Forms.MessageBox.Show("Must re-initialize to recover from motion error", "MOTION ERROR");
				return 1;
			}

			// we'll only command one move (may be multi-axis) at a time
			// wait for any previous move to finish before starting a new one
			// if previous move failed just return - make client call ErrorRecovery if they want to 
			// PK - Need to make sure that all axis are finished moving.
//			if (WaitForMoveToFinish(AxisMask, DEFAULT_MOVE_TIMEOUT_SEC)!= MOVE_RESULT.MOVE_SUCCESS) return 1;

			if (WaitForMoveToFinish(ServoControl.AXISMASK_ALL, DEFAULT_MOVE_TIMEOUT_SEC)!= MOVE_RESULT.MOVE_SUCCESS) return 1;
			
			// start any axes that need to be moved
			for (int Axis = 0; Axis < NUM_AXES; ++Axis)
			{
                if (ThisAxisSelected(AxisMask, Axis) & (Last_Commanded_Pos_mm[Axis] != Position_mm[Axis]))
                {
                    AxisAddress = (byte)(Axis + 1);

                    // chance to abort
                    if (stopRequested.StopNow)
                    {
                        StopAllMotion();
                        System.Windows.Forms.MessageBox.Show("User requested stop", "STOP BUTTON");
                        return 1;
                    }

                    try
                    {
                        // Set the velocity
                        int velocity = ConvertToServoVelocity(Velocity_mmpersec[Axis], Math.Abs(AxisParams[Axis].DistResolution_mmPerEncCount));  // PKv3.00
                        string command = "VM " + velocity.ToString();
                        if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;

                        // Start the move
                        int pos = ConvertToServoPosition(Position_mm[Axis], AxisParams[Axis].DistResolution_mmPerEncCount);
                        command = "MA " + pos.ToString();
                        if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;

                        Last_Commanded_Pos_mm[Axis] = Position_mm[Axis];

                        if (COMM_ISSUE_DELAY > 0) Thread.Sleep(COMM_ISSUE_DELAY);  // PKv4.0,2015-04-15

                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("Error communicating with motion system", "MOTION ERROR");
                        return 1;
                    }

                    // set the flag for moving
                    this.moveInProgress = true;
                    movingAxisMask = movingAxisMask+AxisMasks[Axis];

                }
			}

			// chance to abort
			if (stopRequested.StopNow)
			{
				StopAllMotion();
				System.Windows.Forms.MessageBox.Show("User requested stop", "STOP BUTTON");
				return 1;
			}
			
			if (WaitForFinish)
			{
                if (WaitForMoveToFinish(movingAxisMask, DEFAULT_MOVE_TIMEOUT_SEC) != MOVE_RESULT.MOVE_SUCCESS) return 1;
			}

			return 0;
		}

        //  PKv5.2.7
        //  Allows a single axis to perform a double move.
        //  Axis is 0 based axis,  0 to 2
        //  Will always wait for finish.
        //  Delay_ms = number of msec to delay between moves (0 means as fast as possible, virtually no dleay.).

        public int MoveDouble(double Position1_mm, double Position2_mm, int Delay_ms, double Velocity1_mmpersec, double Velocity2_mmpersec, int Axis, bool useGridRobot)
        {
            byte AxisAddress;

            if (!Initialized)
            {
                System.Windows.Forms.MessageBox.Show("ERROR: Motion control not initialized", "MOTION ERROR");
                return 1;
            }

            // if we had a motion error that hasn't been handled, return
            if (this.unhandledMotionError)
            {
                System.Windows.Forms.MessageBox.Show("Must re-initialize to recover from motion error", "MOTION ERROR");
                return 1;
            }

            //  Make sure all other axis are finished moving.
            if (WaitForMoveToFinish(ServoControl.AXISMASK_ALL, DEFAULT_MOVE_TIMEOUT_SEC) != MOVE_RESULT.MOVE_SUCCESS) return 1;

            AxisAddress = (byte)(Axis + 1);
            int tempAxisIndex = Axis;       // Bug Fix !!!  PKv5.2.7.1

            if (useGridRobot)
            {
                AxisAddress = (byte)(AxisAddress + 3);
                tempAxisIndex = tempAxisIndex + 3;    
            }

            try
            {

                if (SendCommand(AxisAddress.ToString(), "CP 1") != 0) return 1;         // PKv5.3.3,  Delete any old program 1.

                if (SendCommand(AxisAddress.ToString(), "PG 1") != 0) return 1;

                // Move to the 1st position at existing acc and decel
                int velocity = ConvertToServoVelocity(Velocity1_mmpersec, Math.Abs(AxisParams[tempAxisIndex].DistResolution_mmPerEncCount));
                string command = "VM " + velocity.ToString();
                if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;
                int pos = ConvertToServoPosition(Position1_mm, AxisParams[tempAxisIndex].DistResolution_mmPerEncCount);
                command = "MA " + pos.ToString();
                if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;

                if (SendCommand(AxisAddress.ToString(), "H") != 0) return 1;   // Finish the move

                if (Delay_ms > 0)
                {
                    command = "H " + Delay_ms.ToString();
                    if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;
                }

                // Set the velocity
                velocity = ConvertToServoVelocity(Velocity2_mmpersec, Math.Abs(AxisParams[tempAxisIndex].DistResolution_mmPerEncCount));
                command = "VM " + velocity.ToString();
                if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;

                // Move to the 2nd position
                pos = ConvertToServoPosition(Position2_mm, AxisParams[tempAxisIndex].DistResolution_mmPerEncCount);
                command = "MA " + pos.ToString();
                if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;

                if (SendCommand(AxisAddress.ToString(), "H") != 0) return 1;   // Finish the move

                if (SendCommand(AxisAddress.ToString(), "E") != 0) return 1;   // End the program

                if (SendCommand(AxisAddress.ToString(), "PG") != 0) return 1;   // Exit Program Mode

                if (SendCommand(AxisAddress.ToString(), "EX 1") != 0) return 1;   // Execute the program

                int portIndex = 0;
                if (AxisAddress > 3) portIndex = 1;
                string resp = "";
                bool programRunning = true;
                while (programRunning)
                {
                    if (SendCommand(AxisAddress.ToString(), "PR BY") != 0) return 1;   // Is the proram still running
                    resp = GetResponse(portIndex, 10, true);  // Supress error,  10msec wait.   It seems to send nothing when running a program.
                    if (resp == "0") programRunning = false;  // Done running the program
                    Thread.Sleep(10);
                    // TODO Need to add a timeout here in case it runs the program forever
                }

                Last_Commanded_Pos_mm[tempAxisIndex] = Position2_mm;

            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error communicating with motion system", "MOTION ERROR");
                return 1;
            }


            return 0;
        }

        // A much faster function to check for the move to finish.
        // Developed specially for plunge axis critical timing
        // 18Sep2012 Update

        public bool IsMoveDone(int axis)
        {
            // ask the current axis for a status
            ServoStatus AxisStatus = GetAxisStatus(axis);

            // if someone isn't done, we're not done
            return AxisStatus.MoveDone;
        }


		// AxisMask is a bitmask with the axes we want to look at - others are ignored
		public MOVE_RESULT WaitForMoveToFinish(int AxisMask, int Timeout_sec)
		{
			ServoStatus				AxisStatus;
			System.DateTime			StartTime = DateTime.Now;
			System.TimeSpan			MaxTime = new TimeSpan(0, 0, 0, Timeout_sec, 0);
			MOVE_RESULT				Result;
			bool					AllDone;
	
			// if we never commanded a move then there's nothing to wait for
			// we can do this because we only start moves in one function and we set the flag to true at that time
			if (!moveInProgress) return 0;

			do
			{	
				// see if we need to stop early
				if (stopRequested.StopNow)
				{
					StopAllMotion();
					System.Windows.Forms.MessageBox.Show("User requested stop", "STOP BUTTON");
					return MOVE_RESULT.MOVE_ERROR;
				}

				// see if everyone is done moving
				AllDone = true;
				
				// check all axes we care about to see if they're done
				for (int Axis = 0; Axis < NUM_AXES; ++Axis)
				{
					if (ThisAxisSelected(AxisMask, Axis))
					{
     
						// ask the current axis for a status
						AxisStatus = GetAxisStatus(Axis);

                        if (COMM_ISSUE_DELAY > 0) Thread.Sleep(COMM_ISSUE_DELAY);    // PKv4.0,2015-04-15
		
						// if someone isn't done, we're not done
						if (!AxisStatus.MoveDone) AllDone = false;
					}
				}

				if (AllDone) break;
				
				// give everyone else a chance at the processor
				System.Threading.Thread.Sleep(1);

			} while ((System.DateTime.Now - StartTime) < MaxTime);  // check for move timeout

			// get the final word on how the move turned out
//			Result = GetMoveResult(AxisMask);      // This seems a little redundant if the move is all done.  //PKv4.0,2015-04-15
                                                   // If we add more sophisticated error handling.

            if (AllDone) //PKv4.0,2015-04-15
            {
                Result = MOVE_RESULT.MOVE_SUCCESS;
            }
            else
            {
                Result = MOVE_RESULT.MOVE_TIMEOUT;
            }

			// if it didn't work then set a flag so that we recover before the next move
			unhandledMotionError = (Result != MOVE_RESULT.MOVE_SUCCESS);
			
			// the move is done unless it never finished
//			if (unhandledMotionError) StopAllMotion();    //      PKv4.0,2015-05-08     Took out the stop.

			// we're not moving now
			this.moveInProgress = false;

			return Result;
		}


		public int StopAllMotion()
		{
			bool Error = false;
			byte AxisAddress;

			for (int Axis = 0; Axis < NUM_AXES; ++Axis)
			{
				AxisAddress = (byte)(Axis+1);

				try
				{
					// Send it a soft stop
                    ServoControl.SendCommand(AxisAddress.ToString(), "\x0005");   // CNTL+E);
				}
				catch
				{
					return 1;
				}
			}

			if (Error) return 1;
			
			// if it worked, we're not moving
			this.moveInProgress = false;
		
			return 0;
		}


		public int MotionErrorRecovery()
		{
			// download parameters and home all axes
			if (InitializeMotion(false) != 0) return 1;

			// clear the flag so we can move
			this.unhandledMotionError = false;

			return 0;
		}

        // Axis is 0 based
        // PKv5.5.5  TODO this needs to be fixed, 
        // Following commented out for version PKv5.5.5  

        /*   

		public double GetAxisPosition_mm(int Axis)          // PKv4.0,2015-03-20  
		{
            if (useLastCommanded_test) return Last_Commanded_Pos_mm[Axis];     //   PKv5.3.2

            int portIndex = 0;
            if (Axis >= 3) portIndex = 1;   // Grid robot

			try
			{
                if (SendCommand((Axis + 1).ToString(), "PR C2")!=0) return -1;
                string resp = GetResponse(portIndex,COMMAND_TIMEOUT);
                Convert.ToInt32(resp);
				return ConvertToMM(Convert.ToInt32(resp), AxisParams[Axis].DistResolution_mmPerEncCount);
			}
			catch
			{
				return 0.000;
			}
		}

        */


        public double GetAxisPosition_mm(int Axis)    // Added for version PKv5.5.5    ,  Got rid of the ability to ignore error.   
        {
            if (useLastCommanded_test) return Last_Commanded_Pos_mm[Axis];     //   PKv5.3.2

            int portIndex = 0;
            if (Axis >= 3) portIndex = 1;   // Grid robot

            if (SendCommand((Axis + 1).ToString(), "PR C2") != 0) return -1;
            string resp = GetResponse(portIndex, COMMAND_TIMEOUT);
             Convert.ToInt32(resp);
             return ConvertToMM(Convert.ToInt32(resp), AxisParams[Axis].DistResolution_mmPerEncCount);
        }


        public int SetAxisPosition_mm(int Axis, double Position_mm)   // PKv4.0,2015-04-07
        {

            try
            {
  
                string command="C2="+ConvertToServoPosition(Position_mm, AxisParams[Axis].DistResolution_mmPerEncCount).ToString(); //PKv3.00 Changed to C2
                int ret = SendCommand((Axis + 1).ToString(), command);
                if (ret == 0)
                {
                    Last_Commanded_Pos_mm[Axis] = Position_mm;  // pkv4.0,2015-04-21,   Need to update this too !
                }
                return ret;
            }
            catch
            {
                return -1;          // Error
            }
        }


		public int AxisEnable(int Axis, bool Enable)
		{
            Console.WriteLine("AxisEnable not supported with Lexium motors");   //pkV4.0,2015-04-28
            return 0;
		}


		// pass a zero-based axis number
		public ServoAuxStatus GetAxisAuxStatus(int Axis)
		{

            Console.WriteLine("ServoAuxStatus() not supported with Lexium motors");  //pkV4.0,2015-04-28
			ServoAuxStatus	Status = new ServoAuxStatus();

            return Status;

	
		}

        //pkv3.0
        // Updated to only pass back Status.MoveDone, Status.CheckSumError (comm error) & Status.PositionError 
        // Routine will throw up a dialog box with a larger description of the error if one occurs

		// pass a zero-based axis number,  axis
		public ServoStatus GetAxisStatus(int Axis)
		{
			byte			AxisAddress = (byte)(Axis+1);
			ServoStatus		Status = new ServoStatus();
            int portIndex = 0;
            if (AxisAddress > 3) portIndex = 1;

			try
            {
                #region MoveDone? // See if the move is done
                string resp;

                Status.CheckSumError = true;   // If it returns early due to a strange comm error.
                Status.PositionError = false;
                Status.MoveDone = false;

                if (SendCommand(AxisAddress.ToString(), "PR MV") != 0) return Status;
                resp = GetResponse(portIndex,COMMAND_TIMEOUT);
                if (resp == "1")
                    Status.MoveDone = false;   // Still moving
                else
                {
                    if (resp == "0")   // Done moving
                        Status.MoveDone = true;
                    else
                    {
                        Status.MoveDone = false;  // Some strange unexpected response
                        string message = string.Format("Axis (1to6) {0},Response to PR MV => {1}", AxisAddress, resp);
                        MessageBox.Show(message, "Motion.cs, ServoStatus");    // Show the response
                        return Status;   
                    }
                }
                #endregion


                #region Check for error flag
                if (SendCommand(AxisAddress.ToString(), "PR EF") != 0) return Status;
                resp = GetResponse(portIndex,COMMAND_TIMEOUT);
                if (resp == "1")
                {
                    Status.PositionError = true;
                    SendCommand(AxisAddress.ToString(), "PR ER");
                    resp = GetResponse(portIndex, COMMAND_TIMEOUT);
                    string message = string.Format("Axis {0},Error Number = {1}",AxisAddress,resp);
                    MessageBox.Show(message,"Motion.cs, ServoStatus");    // Show the error number in a message box
                }
                else
                {
                    if (resp == "0")  // No error flag.  All is good
                        Status.PositionError = false;
                    else
                    {
                        Status.MoveDone = false;   // Some unexpected response
                        string message = string.Format("Axis {0},Response to PR EF = {1}", AxisAddress, resp);
                        MessageBox.Show(message, "Motion.cs, ServoStatus");    // Show the response
                    }
                }
                #endregion

			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("Error communicating with motion system", "MOTION ERROR");
				return Status;
			}

            /*
			Status.MoveDone			= ((StatusByte & MotionDLL.MOVE_DONE)   > 0);
			Status.CheckSumError	= ((StatusByte & MotionDLL.CKSUM_ERROR) > 0);
			Status.Overcurrent		= ((StatusByte & MotionDLL.OVERCURRENT) > 0);
			Status.PowerOn			= ((StatusByte & MotionDLL.POWER_ON)	> 0);
			Status.PositionError	= ((StatusByte & MotionDLL.POS_ERR)		> 0);
			if (!AxisParams[Axis].LimitSwitchInverted)			// PK
			{
				Status.AtNegativeLimit	= ((StatusByte & MotionDLL.LIMIT1)	> 0);
				Status.AtPositiveLimit	= ((StatusByte & MotionDLL.LIMIT2)	> 0);
			}
			else
			{
				Status.AtNegativeLimit	= ((StatusByte & MotionDLL.LIMIT1)	== 0);
				Status.AtPositiveLimit	= ((StatusByte & MotionDLL.LIMIT2)	== 0);
			}
			Status.HomeInProgress	= ((StatusByte & MotionDLL.HOME_IN_PROG)> 0);
            */

            Status.CheckSumError = false;

			return Status;
		}

        // PKv5.3.2

        public int SetLastPositionTest(bool useLastPositionTest)
        {
            useLastCommanded_test = useLastPositionTest;
            return 0;
        }

        public bool GetLastPositionTest()
        {
            return useLastCommanded_test;
        }

        // PKv5.3.3

        public int HopZYZ(double aboveDistance_mm, double yOffset_mm, double downDistance_mm, double zRounding_mm, double yRounding_mm, int settling_ms, bool useGridRobot)
        {

            int AxisZ = 2;  // Zero based,  2 means Z  (0 to 2)
            int AxisY = 1;  // Zero based,  1 means Y  (0 to 2)

            int tempAdder = 0;
            if (useGridRobot) tempAdder = 3;

            byte AxisAddressZ = (byte)(AxisZ + 1 + tempAdder);   // Not zero based,  starts at 1-6
            byte AxisAddressY = (byte)(AxisY + 1 + tempAdder);   // Not zero based,  starts at 1-6

            int AxisIndexZ = AxisAddressZ - 1;        // Zero based, (0 to 5)
            int AxisIndexY = AxisAddressY - 1;        // Zero based, (0 to 5)

            if (!Initialized)
            {
                System.Windows.Forms.MessageBox.Show("ERROR: Motion control not initialized", "MOTION ERROR");
                return 1;
            }

            // if we had a motion error that hasn't been handled, return
            if (this.unhandledMotionError)
            {
                System.Windows.Forms.MessageBox.Show("Must re-initialize to recover from motion error", "MOTION ERROR");
                return 1;
            }

            //  Make sure all other axis are finished moving. 
            if (WaitForMoveToFinish(ServoControl.AXISMASK_ALL, DEFAULT_MOVE_TIMEOUT_SEC) != MOVE_RESULT.MOVE_SUCCESS) return 1;


            try
            {

                int st = System.Environment.TickCount;      // Just curious how long it takes to downoad and setup all these moves. 

                string command = "MT=" + settling_ms.ToString();                        // Override the default settling time
                if (SendCommand(AxisAddressZ.ToString(), command) != 0) return 1;       // Send to Z and Y axis.
                if (SendCommand(AxisAddressY.ToString(), command) != 0) return 1;

                // Arm an outputs of the z axis for triggering the Y axis move.
                //               if (ArmTriggerOut(AxisZ, aboveDistance_mm - zRounding_mm, useGridRobot)!=0) return -1;
                if (SendCommand(AxisAddressZ.ToString(), "OS=3,16,0") != 0) return 1;       //  Configure Output 3 Active Low
                if (SendCommand(AxisAddressZ.ToString(), "O3=0") != 0) return 1;            //  Make sure its off.


                // Arm an output of Y axis for triggering the Z axis move.
                //  if (ArmTriggerOut(AxisY, yOffset_mm - yRounding_mm, useGridRobot) != 0) return -1;

                if (SendCommand(AxisAddressY.ToString(), "OS=3,16,0") != 0) return 1;       //  Configure Output 3 Active Low
                if (SendCommand(AxisAddressY.ToString(), "O3=0") != 0) return 1;            //  Make sure its off.


                #region Setup the Z axis move

                if (SendCommand(AxisAddressZ.ToString(), "CP 1") != 0) return 1;    // Clear the program.

                if (SendCommand(AxisAddressZ.ToString(), "PG 1") != 0) return 1;
                int relativeEncCount = (int)Math.Truncate(aboveDistance_mm / AxisParams[AxisIndexZ].DistResolution_mmPerEncCount);
                command = "MR " + relativeEncCount.ToString();
                if (SendCommand(AxisAddressZ.ToString(), command) != 0) return 1;

                if (SendCommand(AxisAddressZ.ToString(), "LB Ga") != 0) return 1;

                // TODO - Safer to just use:  GetAxisPosition_mm  instead of Last_Commanded_Pos

                double tripZ_mm = Last_Commanded_Pos_mm[AxisIndexZ] + aboveDistance_mm - zRounding_mm;
                int tripZ_EncCount = (int)Math.Truncate(tripZ_mm / AxisParams[AxisIndexZ].DistResolution_mmPerEncCount);

                command = string.Format("BR Ga, C2 >{0}", tripZ_EncCount);
                if (SendCommand(AxisAddressZ.ToString(), command) != 0) return 1;

                //  if (SendCommand(AxisAddressZ.ToString(), "BR Gb,C2>100000") != 0) return 1;

                if (SendCommand(AxisAddressZ.ToString(), "O3=1") != 0) return 1;            //  Turn On the output

                if (SendCommand(AxisAddressZ.ToString(), "H") != 0) return 1;   // Finish the move

                // 2nd part of the move,  moving down

                if (SendCommand(AxisAddressZ.ToString(), "LB Gb") != 0) return 1;

                if (SendCommand(AxisAddressZ.ToString(), "BR Gb,I3=1") != 0) return 1;   // If I3 is not on then loop  

                relativeEncCount = (int)Math.Truncate(-1 * downDistance_mm / AxisParams[AxisIndexZ].DistResolution_mmPerEncCount);
                command = "MR " + relativeEncCount.ToString();
                if (SendCommand(AxisAddressZ.ToString(), command) != 0) return 1;

                if (SendCommand(AxisAddressZ.ToString(), "H") != 0) return 1;   // Finish the move

                if (SendCommand(AxisAddressZ.ToString(), "O3=0") != 0) return 1;            //  Turn it back off

                //

                if (SendCommand(AxisAddressZ.ToString(), "E") != 0) return 1;   // End the program

                if (SendCommand(AxisAddressZ.ToString(), "PG") != 0) return 1;   // Exit Program Mode

                #endregion

                #region Setup the Y axis move

                if (SendCommand(AxisAddressY.ToString(), "CP 1") != 0) return 1;    // Clear the program

                if (SendCommand(AxisAddressY.ToString(), "PG 1") != 0) return 1;

                if (SendCommand(AxisAddressY.ToString(), "LB Ga") != 0) return 1;

                if (SendCommand(AxisAddressY.ToString(), "BR Ga,I3=1") != 0) return 1;   // If I3 is not on then loop  

                relativeEncCount = (int)Math.Truncate(yOffset_mm / AxisParams[AxisIndexY].DistResolution_mmPerEncCount);
                command = "MR " + relativeEncCount.ToString();
                if (SendCommand(AxisAddressY.ToString(), command) != 0) return 1;

                // TODO - Safer to just use:  GetAxisPosition_mm  instead of Last_Commanded_Pos

                double tripY_mm = Last_Commanded_Pos_mm[AxisIndexY] + yOffset_mm - yRounding_mm;
                int tripY_EncCount = (int)Math.Truncate(tripY_mm / AxisParams[AxisIndexY].DistResolution_mmPerEncCount);

                if (SendCommand(AxisAddressY.ToString(), "LB Gb") != 0) return 1;

                command = string.Format("BR Gb, C2 >{0}", tripY_EncCount);
                if (SendCommand(AxisAddressY.ToString(), command) != 0) return 1;

                if (SendCommand(AxisAddressY.ToString(), "O3=1") != 0) return 1;            //  Turn on the output.

                if (SendCommand(AxisAddressY.ToString(), "H") != 0) return 1;   // Finish the move

                if (SendCommand(AxisAddressY.ToString(), "H 2000") != 0) return 1;            // Hold the output on for 2 sec,  
                                                                                              // Bet the Z axis cant miss that !!

                if (SendCommand(AxisAddressY.ToString(), "O3=0") != 0) return 1;            //  Turn it back off


                if (SendCommand(AxisAddressY.ToString(), "E") != 0) return 1;   // End the program

                if (SendCommand(AxisAddressY.ToString(), "PG") != 0) return 1;   // Exit Program Mode

                #endregion

                int et = System.Environment.TickCount - st;
                Console.WriteLine("       Motion.cs: HopZYZ Download Program time {0} msec", et);   // PKv4.2.9.1  Help understand downloading a program delay.

                if (SendCommand(AxisAddressY.ToString(), "EX 1") != 0) return 1;   // Execute the Y axis program

                if (SendCommand(AxisAddressZ.ToString(), "EX 1") != 0) return 1;   // Execute the Z axis program

                int portIndex = 0;
                if (AxisAddressZ > 3) portIndex = 1;
                string resp = "";
                bool programRunning = true;
                st = System.Environment.TickCount;

                while (programRunning)
                {
                    if (SendCommand(AxisAddressY.ToString(), "PR BY") != 0) return 1;   // Is the proram still running
                    resp = GetResponse(portIndex, 10, true);  // Supress error,  10msec wait.   It seems to send nothing when running a program.
                    if (resp == "0") programRunning = false;  // Done running the program
                    Thread.Sleep(10);

                    et = System.Environment.TickCount - st;
                    if (et > 20000)         // Greater than 20 seconds.
                    {
                        //
                        Console.WriteLine("       Motion.cs: HopZYZ Program Time out");
                        MessageBox.Show("     Motion.cs: HopZYZ Program Time out");
                        StopAllMotion();        // This halts programs too by sending the escape key
                        return -1;
                    }

                }

                Last_Commanded_Pos_mm[AxisIndexZ] = Last_Commanded_Pos_mm[AxisIndexZ] + aboveDistance_mm - downDistance_mm;
                Last_Commanded_Pos_mm[AxisIndexY] = Last_Commanded_Pos_mm[AxisIndexY] + yOffset_mm;

                command = "MT=100";     // TODO might want to  have a more universal place to set the the default settling time.
                if (SendCommand(AxisAddressZ.ToString(), command) != 0) return 1;       // Send to Z and Y axis.
                if (SendCommand(AxisAddressY.ToString(), command) != 0) return 1;


            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error communicating with motion system", "MOTION ERROR");
                return 1;
            }


            return 0;
        }

        //  PKv5.5.4 
        //  Routine which can move to a final position, changing speeds after a certain points.
        //  Moves towards Position1 at Velocity1, when it passes Position2 it changes to Velocity2, when it passes Position3 it changes to Velocity3.

        public int MoveComplexProfile(double Pos1_mm, double Pos2_mm, double Pos3_mm, double Vel1_mmpersec, double Vel2_mmpersec, double Vel3_mmpersec, int Axis, bool useGridRobot)
        {
            byte AxisAddress;

            if (!Initialized)
            {
                System.Windows.Forms.MessageBox.Show("ERROR: Motion control not initialized", "MOTION ERROR");
                return 1;
            }

            // if we had a motion error that hasn't been handled, return
            if (this.unhandledMotionError)
            {
                System.Windows.Forms.MessageBox.Show("Must re-initialize to recover from motion error", "MOTION ERROR");
                return 1;
            }

            //  Make sure all other axis are finished moving.
            if (WaitForMoveToFinish(ServoControl.AXISMASK_ALL, DEFAULT_MOVE_TIMEOUT_SEC) != MOVE_RESULT.MOVE_SUCCESS) return 1;

            AxisAddress = (byte)(Axis + 1);
            int tempAxisIndex = Axis;           // PKv4.2.7.1  fix for 6 axis machine.

            if (useGridRobot)
            {
                AxisAddress = (byte)(AxisAddress + 3);
                tempAxisIndex = tempAxisIndex + 3;
            }

            try
            {

                int st = System.Environment.TickCount;

                if (SendCommand(AxisAddress.ToString(), "CP 1") != 0) return 1;        // Clear the program

                if (SendCommand(AxisAddress.ToString(), "PG 1") != 0) return 1;        // Write the next program.  

                // Move to the 1st position at existing speed, acc and decel
                int vel1 = ConvertToServoVelocity(Vel1_mmpersec, Math.Abs(AxisParams[tempAxisIndex].DistResolution_mmPerEncCount));
                string command = "VM " + vel1.ToString();
                if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;
                int pos2 = ConvertToServoPosition(Pos2_mm, AxisParams[tempAxisIndex].DistResolution_mmPerEncCount);
                command = "MA " + pos2.ToString() + ",0,1";  // Not stopping at Position 2
                if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;

                if (SendCommand(AxisAddress.ToString(), "H") != 0) return 1;   // Finish the move

                int pos3 = ConvertToServoPosition(Pos3_mm, AxisParams[tempAxisIndex].DistResolution_mmPerEncCount);

                //
                int vel2 = ConvertToServoVelocity(Vel2_mmpersec, Math.Abs(AxisParams[tempAxisIndex].DistResolution_mmPerEncCount));
                command = "VM " + vel2.ToString();
                if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;

                command = "MA " + pos3.ToString() + ",0,1";      // Not stopping at Position 3
                if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;

                if (SendCommand(AxisAddress.ToString(), "H") != 0) return 1;   // Finish the move

                int pos1 = ConvertToServoPosition(Pos1_mm, AxisParams[tempAxisIndex].DistResolution_mmPerEncCount);

                int vel3 = ConvertToServoVelocity(Vel3_mmpersec, Math.Abs(AxisParams[tempAxisIndex].DistResolution_mmPerEncCount));
                command = "VM " + vel3.ToString();
                if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;

                command = "MA " + pos1.ToString();          // Will stop at Position 1
                if (SendCommand(AxisAddress.ToString(), command) != 0) return 1;

                if (SendCommand(AxisAddress.ToString(), "H") != 0) return 1;   // Finish the move

                if (SendCommand(AxisAddress.ToString(), "E") != 0) return 1;   // End the program

                if (SendCommand(AxisAddress.ToString(), "PG") != 0) return 1;   // Exit Program Mode

                int et = System.Environment.TickCount - st;
                Console.WriteLine("       Motion.cs: MoveComplext Download Program time {0} msec", et);   // PKv4.2.9.1  Help understand downloading a program delay.

                if (SendCommand(AxisAddress.ToString(), "EX 1") != 0) return 1;   // Execute the program

                int portIndex = 0;
                if (AxisAddress > 3) portIndex = 1;
                string resp = "";
                bool programRunning = true;
                while (programRunning)
                {
                    if (SendCommand(AxisAddress.ToString(), "PR BY") != 0) return 1;   // Is the proram still running
                    resp = GetResponse(portIndex, 10, true);  // Supress error,  10msec wait.   It seems to send nothing when running a program.
                    if (resp == "0") programRunning = false;  // Done running the program
                    Thread.Sleep(10);
                    // TODO - Need to add a timeout here in case it runs the program forever
                    // TODO - Might want to have a pass parameter with the option to not begin polling until after a fixed delay.  Lets all critical moves finish
                    //  so the microprocessor is not handling communication AND critical moves
                }

                Last_Commanded_Pos_mm[tempAxisIndex] = Pos1_mm;

            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error communicating with motion system", "MOTION ERROR");
                return 1;
            }
            return 0;
        }


        #endregion



        #region private methods

        // Can we talks to motion hardware and are all axis found ?
        // PKv3.0

        private bool AllAxesFound()
		{
		
			// make sure all the axis can talk,  should look for a reply.
			try
			{

                ServoControl.SendCommand("1", "ER=0"); // 0 out any errors
                ServoControl.SendCommand("2", "ER=0"); // 0 out any errors
                ServoControl.SendCommand("3", "ER=0"); // 0 out any errors

                ServoControl.SendCommand("4", "ER=0"); // 0 out any errors
                ServoControl.SendCommand("5", "ER=0"); // 0 out any errors
                ServoControl.SendCommand("6", "ER=0"); // 0 out any errors

                //PKv4.0,2015-03-02  All the following seems totally uneccessary and was commented out.

                //ServoControl.SendCommandNoCR("\x000A"+"1");  // Send one time to activate party mode.
                //Thread.Sleep(50);
                //ServoControl.SendCommandNoCR("\x000A" + "2");  // Send one time to activate party mode.
                //Thread.Sleep(50);
                //ServoControl.SendCommandNoCR("\x000A" + "3");  // Send one time to activate party mode.
                //Thread.Sleep(50);
                //ServoControl.SendCommandNoCR("\x000A");  // One more with no address what the heck !.

			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("Error communicating with motion system", "MOTION ERROR");
				return false;
			}

			return true;
		}


		private int SetAllAxisParameters()
		{
			try
			{
                // Not sure there is anything we need to do here.

			}

			catch
			{
				System.Windows.Forms.MessageBox.Show("Error communicating with motion system", "MOTION ERROR");
				return 1;
			}

			return 0;
		}


		private MOVE_RESULT GetMoveResult(int AxisMask)
		{
			ServoStatus				AxisStatus;
			ServoAuxStatus			AxisAuxStatus;   // Not used any more

			// see if there were any errors on an axis we care about - also check one more time to see if axes are done
			bool AllDone = true;

			for (int Axis = 0; Axis < NUM_AXES; ++Axis)
			{
				if (ThisAxisSelected(AxisMask, Axis))
				{
					// ask the current axis for a status update
					AxisStatus = GetAxisStatus(Axis);
//					AxisAuxStatus = GetAxisAuxStatus(Axis);

					if (AxisStatus.MoveDone == false) AllDone = false;
//					if (AxisStatus.ErrorOccurred() || AxisAuxStatus.ErrorOccurred()) return MOVE_RESULT.MOVE_ERROR;
				}
			}

			// did we finish
			if (!AllDone) return MOVE_RESULT.MOVE_TIMEOUT;
			
			// no error and axes are done - it must have worked
			return MOVE_RESULT.MOVE_SUCCESS;			
		}


		private double MaxServoPosition_mm(int Axis)
		{
			// use this to move to the end of travel
			// PK - Lets try max value of the PIC servo counter (think it's around 2million)
            return (2000000000) * Math.Abs(AxisParams[Axis].DistResolution_mmPerEncCount);  // PKv3.00
		}


		private double MinServoPosition_mm(int Axis)
		{
			// use this to move to the end of travel
			// PK - Lets try close to the min value of the PIC servo counter (think it's around 2billion)
            return (-2000000000) * Math.Abs(AxisParams[Axis].DistResolution_mmPerEncCount);  // PKv3.00
		}


		private int ConvertToServoPosition(double Position_mm, double mmPerEncoderCount)
		{
			return (int)(Position_mm / mmPerEncoderCount);
		}


		private double ConvertToMM(int ServoPosition_counts, double mmPerEncoderCount)
		{
			return ServoPosition_counts * mmPerEncoderCount;
		}

		
		#region explanation from servo documentation JR KERR
		/*
		The position, velocity and acceleration are programmed as 32 bit quantities in units of encoder counts
		and servo ticks. For example, a velocity of one revolution per second of a motor with a 500 line
		encoder (2000 counts/rev) at a servo tick time of 0.512 msec. would correspond to a velocity of
		1.0240 counts/tick. Velocities and accelerations use the lower 16 bits as a fractional component so
		that the actual programmed velocity would be 1.024 x 2^16 or 67,109. An acceleration of 4 rev/sec/sec
		(which would bring us up to the desired speed in 1/4 sec) would be 0.0021 counts/tick/tick; with the
		lower 16 bits the fractional component, this would be programmed as 0.0021 x 2^16 or 137. Position
		is programmed as a signed 32 bit quantity with no fractional component.
		*/

        /* pkV3.0  Updated to just return encoder counts per second.  */
		#endregion

        private int ConvertToServoVelocity(double Velocity_mmpersec, double mmPerEncoderCount)
        {
            return (int)(Velocity_mmpersec / mmPerEncoderCount);
        }

        private int ConvertToServoAcceleration(double Acceleration_mmpersec2, double mmPerEncoderCount)
        {
            return (int)(Acceleration_mmpersec2 / mmPerEncoderCount);
        }

        /*
		private int ConvertToServoVelocity(double Velocity_mmpersec, double mmPerEncoderCount, double secPerServoTick)
		{
			return (int)((Velocity_mmpersec / mmPerEncoderCount * secPerServoTick) * (int)Math.Pow(2.0, 16.0));
		}

		private int ConvertToServoAcceleration(double Acceleration_mmpersec2, double mmPerEncoderCount, double secPerServoTick)
		{
			return (int)((Acceleration_mmpersec2 / mmPerEncoderCount * secPerServoTick * secPerServoTick) * (int)Math.Pow(2.0, 16.0));
		}
        */
		

		private bool ThisAxisSelected(int AxisMask, int Axis)
		{
			return ((AxisMask & AxisMasks[Axis]) > 0);
		}


		// first move to a limit switch
		// then we'll we know which way to go to find the index pulse
		// reverse direction and find the index pulse
		private int HomeToIndex(int Axis)
		{
            Console.WriteLine("HomeToIndex() not supported with Lexium motors YET");  //pkV4.0,2015-04-28
            return 0;
		}


		// used for homing only
		private int MoveToLimit(int Axis, bool MoveToPositiveLimit)
		{
            Console.WriteLine("MoveToLimit() not supported with Lexium motors YET");  //pkV4.0,2015-04-28
            return 0;
		}
		

		private int HomeToLimit(int Axis, bool MoveToPositiveLimit)
		{
            Console.WriteLine("HomeToLimit() not supported with Lexium motors YET");  //pkV4.0,2015-04-28
            return 0;
		}

		#endregion
	
	}
	
	// user-level control of motion
	// units are mm and seconds, positions are all relative to home
	public class UserMotionControl
	{
		#region members

		private bool[]						axisHomed = new bool [ServoControl.NUM_AXES];	// have we homed each axis?
		private bool						allowManualControl = false;						// do we let the user drive things
		private ServoControl				servoController;								// the device that does the work
		private MotionConfigurationData		motionParameters;								// lots of info about moving
		private object						master;											// the object that owns motion control - can allow or disallow manual control
		public SoftwareStopButton			stopButton = new SoftwareStopButton();			// lets you stop mid-move
		private string						configurationFile;								// xml file with all the settings
		private bool						hardwareInitialized = false;					// general motion parameters downloaded to hardware
		private bool						stepMode;										// enable if you want to wait for user ok before each move
		private const double				MAX_XY_ERROR_MM = .100;                         // Checks in routines read encoders after move to make sure this is not exceeded:  XY only.

        #endregion

		#region properties
		
		
		// motion parameters downloaded
		public bool HardwareInitialized 
		{
			get {return hardwareInitialized;}
		}

		public bool StepMode
		{
			get {return stepMode;}
			set {stepMode = value;}
		}

		public double ZSafe
		{
			get {return this.motionParameters.SafeZ;}
		}

        public double ZSafeGrid
        {
            get { return this.motionParameters.SafeZGrid; }
        }

		public double DefaultSpeed_pct
		{
			get {return this.motionParameters.DefaultSpeed_pct;}
		}

		public bool ManualControlEnabled
		{
			get {return allowManualControl;}
		}

		#endregion

		#region public methods


		public UserMotionControl(object MotionMaster, string ParameterFilePath)
		{
			// initialize member variables
			master = MotionMaster;
			allowManualControl = false;	  // PK Changed to false to not allow manual control until intiated by master.
			stopButton.StopNow = false;
			configurationFile = ParameterFilePath;
			hardwareInitialized = false;

			// deserialize the xml file with the settings
			UpdateSettingsFromFile();

			// create the object that talks to the hardware
			servoController = new ServoControl(motionParameters, stopButton);
		}

		
		public void EnableManualControl(object Caller, bool Enabled)
		{
			// only the object master can do this
			if (HandleManualCommands(Caller) != 0) return;
			
			allowManualControl = Enabled;
		}

        public string GetLimitString(int Axis)
        {
            string tempStr = string.Format("[{0:0.0}, {1:0.0}]",
               motionParameters.AxisData[Axis].MinPosition_mm,
                motionParameters.AxisData[Axis].MaxPosition_mm);

            return tempStr;
 
        }


		public bool AxisHomed(int Axis)
		{
			if (HandleInvalidAxis(Axis) != 0) return false;
			return axisHomed[Axis];
		}


		public ServoStatus GetAxisStatus(int Axis)
		{
			if (HandleInvalidAxis(Axis) != 0)
			{
				return new ServoStatus();
			}
	
			return servoController.GetAxisStatus(Axis);
		}


		// stop all motion - can be called by anyone any time

		public int StopAllMotion()
		{
			bool Error = false;

			// stops if it's in a WaitForMoveToFinish loop
			stopButton.StopNow = true;

			// stops if it's started a move but isn't waiting for the finish
			if (servoController.StopAllMotion() != 0) Error = true;

			// make sure we've stopped
			if (servoController.WaitForMoveToFinish(ServoControl.AXISMASK_ALL, ServoControl.DEFAULT_MOVE_TIMEOUT_SEC) != 0) Error = true;

			// let motion go again
			stopButton.StopNow = false;
			
			return (Error ? 1 : 0);
		}


		public int InitializeMotion(object Caller, bool SkipHoming)  // PK Added Skip homing flag
		{
            Console.WriteLine("\nInitializing Motion Control ...");
			if (HandleManualCommands(Caller) != 0) return 1;
			
			if (servoController.InitializeMotion(true) != 0) return 1; // PKV3.0 did not set the homing flags (skip homing flag),  just initializes the RS232 ports.

            Console.WriteLine("   All Servo Boards Found, communication established");

            hardwareInitialized = true;
												
			if (!SkipHoming)
			{
                if (HomeAllAxes(Caller) == 0)
                {
                    Console.WriteLine("    All Axis Homed Successfully");
                }
                else
                {
                    hardwareInitialized = false;
                    return 1;			// PK Added this home call at user level.
                }
			}

            Console.WriteLine("   Initialize Successful");
			
			return 0;
		}

		// enable or disable the motor for a zero-based axis number   Not used.
		public int EnableAxis(object Caller, int Axis, bool Enable)
		{
			if (HandleManualCommands(Caller) != 0) return 1;
			if (HandleInvalidAxis(Axis) != 0) return 1;
			if (HandleHardwareNotInitialized() != 0) return 1;

			return servoController.AxisEnable(Axis, Enable);
		}

		
		// home just one axis at a time. 
        // 0 based axis from 0 to 5
		public int HomeAxis(object Caller, int Axis)
		{
			if (HandleManualCommands(Caller) != 0) return 1;
			if (HandleInvalidAxis(Axis) != 0) return 1;
			if (HandleHardwareNotInitialized() != 0) return 1;

            axisHomed[Axis] = false;                                //PKv4.0,2015-05-08  In case it does not complete the home properly.
			if (servoController.HomeAxis(Axis) != 0) return 1;
			
			axisHomed[Axis] = true;
			return 0;
		}

        // 0 based Axis in this case goes from 0 to 2,  useGridRobot selects the proper axis.
        // just sets the motionParameter.AxisData[] to MaxVelocity passed in.  .
        // Does not write to motion controller.  Will be used for all subsequent move commands.
        // PKv5.3.3 Added an override option that allows writing to the motors.

        public int SetMaxVelocity(object Caller, int Axis, double MaxVelocity, bool useGridRobot)
        {
            return SetMaxVelocity(Caller, Axis, MaxVelocity, useGridRobot,false);
        }

        public int SetMaxVelocity(object Caller, int Axis, double MaxVelocity, bool useGridRobot, bool writeToMotor)
		{
            //if (HandleManualCommands(Caller) != 0) return 1;
            //if (HandleInvalidAxis(Axis) != 0) return 1;
            //if (HandleHardwareNotInitialized() != 0) return 1;

            Console.WriteLine("Grid Robot = {0}, SetMaxVelocity: 0 based Axis = {1}, Max Velocity (mm/sec)= {2},  ", useGridRobot, Axis, MaxVelocity);
            int baseIndex = 0;
            if (useGridRobot) baseIndex = 3;
            motionParameters.AxisData[baseIndex + Axis].Vel_Max_Limit_mmpersec = MaxVelocity;
            if (!writeToMotor) return 0;
            return servoController.SetVelocity(Axis, MaxVelocity, useGridRobot);
        }

        // Will get the current value of the max velocity of any axis.
        // 0 based axis from 0 to 2.

        public double GetMaxVelocity(object Caller, int Axis, bool useGridRobot)
        {
            //if (HandleManualCommands(Caller) != 0) return 1;
            //if (HandleInvalidAxis(Axis) != 0) return 1;
            //if (HandleHardwareNotInitialized() != 0) return 1;

            int baseIndex = 0;
            if (useGridRobot) baseIndex = 3;
            return motionParameters.AxisData[baseIndex + Axis].Vel_Max_Limit_mmpersec;
        }

        // 0 based axis (0 to 2).   
        // Routine updates the motionParameters.AxisData and writes to the motor controller.

        public int SetAccel(object Caller, int Axis, double Accel, double Decel, bool useGridRobot)
        {
            //if (HandleManualCommands(Caller) != 0) return 1;
            //if (HandleInvalidAxis(Axis) != 0) return 1;
            //if (HandleHardwareNotInitialized() != 0) return 1;

            Console.WriteLine("SetAccel: Axis = {0}, Accel = {1}, Decel = {2}, Grid = {3}", Axis, Accel,Decel,useGridRobot);
            int baseIndex = 0;
            if (useGridRobot) baseIndex = 3;
            motionParameters.AxisData[baseIndex + Axis].Accel_Max_Limit_mmpersec2 = Accel;
            motionParameters.AxisData[baseIndex + Axis].Accel_Max_Limit_mmpersec2 = Decel;
 
            return servoController.SetAccel(Axis, Accel, Decel, useGridRobot);


        }

        // PKv5.2.5
        // 0 based axis (0 to 2).   
        // Routine calculates a trigger output point for OTF dispense and sets the motor to trigger
        // relativePosition_mm can be positive or negative depending on the direction of motion.

        // PKv5.3.0 Added an override that allows us to experiment with the pulse width of the trigger out.

        public int ArmTriggerOutput(object Caller, int Axis, double relativePosition_mm, bool useGridRobot,int pulseWidth)
        {
            return servoController.ArmTriggerOut((int)Axis, relativePosition_mm, useGridRobot, pulseWidth);
        }


        public int ArmTriggerOutput(object Caller, int Axis, double relativePosition_mm, bool useGridRobot)
        {
            return servoController.ArmTriggerOut((int) Axis, relativePosition_mm, useGridRobot,50);
        }


        public int ToggleTriggerOutput(object Caller, int Axis, bool useGridRobot)
        {
            return servoController.ToggleTriggerOut(Axis, useGridRobot);
        }

        // PKv4.0,2015-04-27
        // pass in a zero-based Axis (0 to 2) and whether you want to useGridRobot.  
        // Sets Maximum Velocity, Accel and Decel from AxisParams[Axis].  xml default values.  For one axis at a time.

        public int SetDefaultMotionParameters(object Caller,int Axis,bool useGridRobot)
        {
            int baseIndex = 0;
            if (useGridRobot) baseIndex = 3;
            int ret = SetAccel(Caller, Axis, motionParameters.AxisData[baseIndex + Axis].Accel_Default_mmpersec2, motionParameters.AxisData[baseIndex + Axis].Decel_Default_mmpersec2, useGridRobot);
            if (ret!=0) return ret;

            ret = servoController.SetVI(Axis, motionParameters.AxisData[baseIndex + Axis].Vel_Initial_counts, useGridRobot);   // PKv4.0,2015-05-07
            if (ret != 0) return ret;

            ret= SetMaxVelocity(Caller, Axis, motionParameters.AxisData[baseIndex + Axis].Vel_Default_mmpersec, useGridRobot);
            return ret;
        }

        // Overload which will set all 3 axis motion parameters to their xml default values.  

        public int SetDefaultMotionParameters(object Caller, bool useGridRobot)
        {
            for (int axis = 0; axis <= 2; axis++)
            {
                if (SetDefaultMotionParameters(Caller, axis, useGridRobot) != 0) return -1;
            }
            return 0;
        }


        // PKv4.0,2015-04-27
        // pass in a zero-based axis (0 to 5).
        // Will clear error # by reading .

        public int ClearError(object Caller, int Axis)
        {
            return servoController.ClearError(Axis);
        }



		// find and move to home position for all axes
		// homes z then x then y
		public int HomeAllAxes(object Caller)
		{
			if (HandleManualCommands(Caller) != 0) return 1;
			if (HandleHardwareNotInitialized() != 0) return 1;

            if (MessageBox.Show("OK to home all axis ? \nMake sure if using plunge axis it is not too high (past limit sensor)", "Motion:HomeAllAxes", MessageBoxButtons.OKCancel) != DialogResult.OK) return 1;

			if (servoController.HomeAllAxes() != 0) return 1;

			for (int Axis = 0; Axis < ServoControl.NUM_AXES; ++Axis)
			{
				axisHomed[Axis] = true;
			}

			return 0;
		}

        //PKv4.0,2015-04-20,Updated for UseGridRobot.

		// moves a tool above a specified point
		// the z of the speicified point is changed to the safe Z
		public int MoveAbove(object Caller, MachineCoordinate Tool, MachineCoordinate DeckPoint, double Speed_pct, bool CheckForStepMode, bool WaitForCompletion)
		{
            return MoveAbove(Caller, Tool, DeckPoint, Speed_pct, CheckForStepMode, WaitForCompletion, false);
		}

        // moves a tool above a specified point
        // the z of the speicified point is changed to the safe Z
        public int MoveAbove(object Caller, MachineCoordinate Tool, MachineCoordinate DeckPoint, double Speed_pct, bool CheckForStepMode, bool WaitForCompletion, bool UseGridRobot)
        {
            MachineCoordinate NewDeckPoint = new MachineCoordinate(DeckPoint);
            if (UseGridRobot)
            {
                NewDeckPoint.Z = motionParameters.SafeZGrid;
            }
            else
            {
                NewDeckPoint.Z = motionParameters.SafeZ;
            }

            return MoveSafely(Caller, Tool, NewDeckPoint, 0.0, Speed_pct, CheckForStepMode, WaitForCompletion,UseGridRobot,false);
        }

		
		// if tips are below ZSafe then move them up to ZSafe
		// if tips are already at a safe height then leave them there
		public int MoveZToSafeHeight(object Caller, bool CheckForStepMode)
		{
			return MoveZToSafeHeight(Caller, CheckForStepMode, motionParameters.DefaultSpeed_pct);
		}

        public int MoveZToSafeHeight(object Caller, bool CheckForStepMode, double Speed_pct)
        {
            return MoveZToSafeHeight(Caller, CheckForStepMode, motionParameters.DefaultSpeed_pct,false);
        }

        // PKv4.0,2015-04-20  Updated to use the grid robot
        // TODO it would be nice to have seperate safe z heights for each robot.

		public int MoveZToSafeHeight(object Caller, bool CheckForStepMode, double Speed_pct, bool useGridRobot)
		{
			MachineCoordinate	Position = new MachineCoordinate();
			MachineCoordinate	Tool = new MachineCoordinate("Reference tool", 0.000, 0.000, 0.000);

			if (GetCurrentPosition(out Position,useGridRobot) != 0) return 1;

            if (useGridRobot)
            {
                if (Position.Z < motionParameters.SafeZGrid)
                {
                    Position.Z = motionParameters.SafeZGrid;
                    return MoveSafely(Caller, Tool, Position, 0.000, Speed_pct, CheckForStepMode, true, useGridRobot,false);
                }
            }
            else
            {
                if (Position.Z < motionParameters.SafeZ)
                {
                    Position.Z = motionParameters.SafeZ;
                    return MoveSafely(Caller, Tool, Position, 0.000, Speed_pct, CheckForStepMode, true, useGridRobot,false);
                }
            }

			return 0;
		}
		
		// return the current robot coordinates - anyone can do this any time
		// it will be incorrect if we haven't homed

		public int GetCurrentPosition(out MachineCoordinate Position)
		{
            GetCurrentPosition(out Position, false);
			Position = new MachineCoordinate();

			if (HandleHardwareNotInitialized() != 0) return 1;	

			Position.X = servoController.GetAxisPosition_mm(ServoControl.X_AXIS);
			Position.Y = servoController.GetAxisPosition_mm(ServoControl.Y_AXIS);
			Position.Z = servoController.GetAxisPosition_mm(ServoControl.Z_AXIS);
		
			return 0;
		}

        // Overide for grid tobor
        // PKv4.0,2015-04-16


        public int GetCurrentPosition(out MachineCoordinate Position, bool useGridRobot)
        {
            Position = new MachineCoordinate();

            if (HandleHardwareNotInitialized() != 0) return 1;

            if (useGridRobot)
            {

                Position.X = servoController.GetAxisPosition_mm(ServoControl.X2_AXIS);
                Position.Y = servoController.GetAxisPosition_mm(ServoControl.Y2_AXIS);
                Position.Z = servoController.GetAxisPosition_mm(ServoControl.Z2_AXIS);
            }
            else
            {

                Position.X = servoController.GetAxisPosition_mm(ServoControl.X_AXIS);
                Position.Y = servoController.GetAxisPosition_mm(ServoControl.Y_AXIS);
                Position.Z = servoController.GetAxisPosition_mm(ServoControl.Z_AXIS);
            }

            return 0;
        }



        // Override to help support the plunge axis

        public int GetCurrentPosition(int axis,out double position_mm)
        {
            position_mm = 0.0;

            if (HandleHardwareNotInitialized() != 0) return 1;

            position_mm = servoController.GetAxisPosition_mm(axis);
 
            return 0;
        }

        //  Can be used to set the encoder position.
        //  Useful if you home to a positive value.

        public int SetCurrentPosition(int axis, double position_mm)
        {

            if (HandleHardwareNotInitialized() != 0) return 1;
            return servoController.SetAxisPosition_mm(axis, position_mm);

        }


		public int WaitForEndOfMotion(int Timeout_sec)
		{
			return ((servoController.WaitForMoveToFinish(ServoControl.AXISMASK_ALL, Timeout_sec) == ServoControl.MOVE_RESULT.MOVE_SUCCESS) ? 0 : 1);
		}

		// Overload the function to check for position error by reading the encoder.
		// This will just make me feel better in the DoDispense routines and inspection routines which
		// utilize this WaitForEndOfMotion...

		public int WaitForEndOfMotion(int Timeout_sec,MachineCoordinate Tool, MachineCoordinate DeckPoint)
		{
			MachineCoordinate	DesiredLocation = new MachineCoordinate();
			int ret = ((servoController.WaitForMoveToFinish(ServoControl.AXISMASK_ALL, Timeout_sec) == ServoControl.MOVE_RESULT.MOVE_SUCCESS) ? 0 : 1);
			DesiredLocation = DeckPoint - Tool;
			MachineCoordinate RealCurrentLocation = new MachineCoordinate();
			GetCurrentPosition(out RealCurrentLocation);
			if (Math.Abs(RealCurrentLocation.X-DesiredLocation.X)>MAX_XY_ERROR_MM || Math.Abs(RealCurrentLocation.Y-DesiredLocation.Y)>MAX_XY_ERROR_MM)
			{
				string errorMessage = "Warning MoveSafely: **** Desired X,Y ("+DesiredLocation.X.ToString()+","+DesiredLocation.Y.ToString()+
					")  Actual X,Y ("+RealCurrentLocation.X.ToString()+","+RealCurrentLocation.Y.ToString()+")";
				Debug.WriteLine(errorMessage);
				MessageBox.Show(errorMessage);
			}
			return ret;
		}

        // PKv5.2.7

        public int MoveDouble(double Position1_mm, double Position2_mm, int Delay_ms, double Velocity1_mmpersec, double Velocity2_mmpersec, int Axis, bool useGridRobot)
        {
            int ret = servoController.MoveDouble(Position1_mm, Position2_mm, Delay_ms, Velocity1_mmpersec, Velocity2_mmpersec, Axis, useGridRobot);
            if (ret == 0)
            {
                return SetMaxVelocity(this, Axis, Velocity2_mmpersec, useGridRobot);    // Update the max velocity to the last one.
            }
            else
            {
                return ret;
            }
        }

        //  PKv5.5.4
        //  Routine which can move to a final position, changing speeds after a certain points.
        //  Moves towards Position1 (end point) at Velocity1, when it passes Position2 it changes to Velocity2, when it passes Position3 it changes to Velocity3.

        public int MoveComplexProfile(double Pos1_mm, double Pos2_mm, double Pos3_mm, double Vel1_mmpersec, double Vel2_mmpersec, double Vel3_mmpersec, int Axis, bool useGridRobot)
        {
            int ret = servoController.MoveComplexProfile(Pos1_mm, Pos2_mm, Pos3_mm, Vel1_mmpersec, Vel2_mmpersec, Vel3_mmpersec, Axis, useGridRobot);
            if (ret == 0)
            {
                return SetMaxVelocity(this, Axis, Vel3_mmpersec, useGridRobot);    // Update the max velocity to the last one.
            }
            else
            {
                return ret;
            }
        }

        public int MoveSafely(object Caller, MachineCoordinate Tool, MachineCoordinate DeckPoint, double Gap_mm, double Speed_pct, bool CheckForStepMode, bool WaitForCompletion)
        {
            return MoveSafely(Caller, Tool, DeckPoint, Gap_mm, Speed_pct, CheckForStepMode, WaitForCompletion,false,false);
        }


        // PKv4.0,2015-04-07, Added an overload to work with the grid robot.
		// moves a tool (tip or camera) to a specified coordinate + some z gap
		// if the desired point is lower than the current position: moves XY first, then Z down (e.g. moving into a well)
		// if the desired point is higher than the current position: moves Z up first, then XY
		// if the z's are the same (within some tolerance) move all axes together
		public int MoveSafely(object Caller, MachineCoordinate Tool, MachineCoordinate DeckPoint, double Gap_mm, double Speed_pct, bool CheckForStepMode, bool WaitForCompletion,bool useGridRobot, bool moveAllAxis)
		{
			MachineCoordinate	CurrentLocation = new MachineCoordinate();
			MachineCoordinate	DesiredLocation = new MachineCoordinate();
			double []			Position_mm = new double [ServoControl.NUM_AXES];
			double []			Velocity_mmpersec = new double [ServoControl.NUM_AXES];
			double				SpeedPctChecked = LimitValue(Speed_pct, 0.1, 100.0);
			int					AxisMask;
            int                 indexStart=0;

            if (useGridRobot) indexStart = 3;


			if (HandleManualCommands(Caller) != 0) return 1;
			if (HandleHardwareNotInitialized() != 0) return 1;	
			if (HandleNotHomed(Caller,useGridRobot) != 0) return 1;             // PKv5.5.0
			if (HandleMotionError() != 0) return 1;



			// find out our z height
			if (GetCurrentPosition(out CurrentLocation,useGridRobot) != 0) return 0;

			// subtract off the tool offset to get the robot axis coordinates we need
			// for the reference tool (tip 1) the offset is 0,0,0 so we just use the deckpoint coordinates
			DesiredLocation = DeckPoint - Tool;
			// go back up by the offset amount
			DesiredLocation.Z += Gap_mm;

            Position_mm[indexStart+ServoControl.X_AXIS] = DesiredLocation.X;
            Position_mm[indexStart + ServoControl.Y_AXIS] = DesiredLocation.Y;
            Position_mm[indexStart + ServoControl.Z_AXIS] = DesiredLocation.Z;

            Velocity_mmpersec[indexStart + ServoControl.X_AXIS] = motionParameters.AxisData[indexStart + ServoControl.X_AXIS].Vel_Max_Limit_mmpersec * SpeedPctChecked / 100.0;
            Velocity_mmpersec[indexStart + ServoControl.Y_AXIS] = motionParameters.AxisData[indexStart + ServoControl.Y_AXIS].Vel_Max_Limit_mmpersec * SpeedPctChecked / 100.0;
            Velocity_mmpersec[indexStart + ServoControl.Z_AXIS] = motionParameters.AxisData[indexStart + ServoControl.Z_AXIS].Vel_Max_Limit_mmpersec * SpeedPctChecked / 100.0;
			
			if (System.Math.Abs(DesiredLocation.Z - CurrentLocation.Z) <= motionParameters.ZTolerance_mm || moveAllAxis)  
			{
				// basically the same z (that tolerance better be tight enough)
				// move all axes - this makes sure that you can do the move asynchronously if you want
				AxisMask = ServoControl.AXISMASK_ALL;
                if (useGridRobot)
                    AxisMask = ServoControl.AXISMASK_ALL_GRID;
                else
                    AxisMask = ServoControl.AXISMASK_ALL;
				if ((CheckForStepMode) && (HandleStepMode() != 0)) return 1;
				if (servoController.Move(Position_mm, Velocity_mmpersec, AxisMask, true) != 0) return 1;
				
			}
			else if (DesiredLocation.Z < CurrentLocation.Z) 
			{
				// we want to move down (maybe into a well), move xy first
				// tell it to wait for the finish but it would do this anyway - ServoControl waits for current move to finish before starting a new one
                if (useGridRobot)
                    AxisMask = ServoControl.AXISMASK_X2 | ServoControl.AXISMASK_Y2;
                else
				    AxisMask = ServoControl.AXISMASK_X | ServoControl.AXISMASK_Y;
				if ((CheckForStepMode) && (HandleStepMode() != 0)) return 1;
				if (servoController.Move(Position_mm, Velocity_mmpersec, AxisMask, true) != 0) return 1;

				// then z
                if (useGridRobot)
                    AxisMask = ServoControl.AXISMASK_Z2;
                else
				    AxisMask = ServoControl.AXISMASK_Z;
				if ((CheckForStepMode) && (HandleStepMode() != 0)) return 1;
				if (servoController.Move(Position_mm, Velocity_mmpersec, AxisMask, WaitForCompletion) != 0) return 1;

			}
			else{
				// we want to move up (maybe out of a well), move z first
				// tell it to wait for the finish but it would do this anyway - ServoControl waits for current move to finish before starting a new one
                // First Z
                if (useGridRobot)
                    AxisMask = ServoControl.AXISMASK_Z2;
                else
                    AxisMask = ServoControl.AXISMASK_Z;
				if ((CheckForStepMode) && (HandleStepMode() != 0)) return 1;
				if (servoController.Move(Position_mm, Velocity_mmpersec, AxisMask, true) != 0) return 1;

				// then xy
                if (useGridRobot)
                    AxisMask = ServoControl.AXISMASK_X2 | ServoControl.AXISMASK_Y2;
                else
                    AxisMask = ServoControl.AXISMASK_X | ServoControl.AXISMASK_Y;
				if ((CheckForStepMode) && (HandleStepMode() != 0)) return 1;
				if (servoController.Move(Position_mm, Velocity_mmpersec, AxisMask, WaitForCompletion) != 0) return 1;
			}

			// Lets check to see how far from the desired postition we are in X and Y ....
			if (WaitForCompletion)
			{
				MachineCoordinate RealCurrentLocation = new MachineCoordinate();
				GetCurrentPosition(out RealCurrentLocation,useGridRobot);

				if (Math.Abs(RealCurrentLocation.X-DesiredLocation.X)>MAX_XY_ERROR_MM || Math.Abs(RealCurrentLocation.Y-DesiredLocation.Y)>MAX_XY_ERROR_MM)
				{
					string errorMessage = "Warning MoveSafely: **** Desired X,Y ("+DesiredLocation.X.ToString()+","+DesiredLocation.Y.ToString()+
						")  Actual X,Y ("+RealCurrentLocation.X.ToString()+","+RealCurrentLocation.Y.ToString()+")";
					Debug.WriteLine(errorMessage);
					MessageBox.Show(errorMessage);
				}
			}

			return 0;
		}

        // PKv5.0  Will move all 3 axis towards target location.

        public int MoveUnSafely(object Caller, MachineCoordinate Tool, MachineCoordinate DeckPoint, double Gap_mm, double Speed_pct, bool CheckForStepMode, bool WaitForCompletion, bool useGridRobot, bool moveAllAxis)
        {
            MachineCoordinate CurrentLocation = new MachineCoordinate();
            MachineCoordinate DesiredLocation = new MachineCoordinate();
            double[] Position_mm = new double[ServoControl.NUM_AXES];
            double[] Velocity_mmpersec = new double[ServoControl.NUM_AXES];
            double SpeedPctChecked = LimitValue(Speed_pct, 0.1, 100.0);
            int AxisMask;
            int indexStart = 0;

            if (useGridRobot) indexStart = 3;


            if (HandleManualCommands(Caller) != 0) return 1;
            if (HandleHardwareNotInitialized() != 0) return 1;
            if (HandleNotHomed(Caller) != 0) return 1;              // TEMP2015-04-06, Temp change to debug one robot at a time.
            if (HandleMotionError() != 0) return 1;

            // find out our z height  PKv5.1  Will not querey for starting position.
 //          if (GetCurrentPosition(out CurrentLocation, useGridRobot) != 0) return 0;

            // subtract off the tool offset to get the robot axis coordinates we need
            // for the reference tool (tip 1) the offset is 0,0,0 so we just use the deckpoint coordinates
            DesiredLocation = DeckPoint - Tool;
            // go back up by the offset amount
            DesiredLocation.Z += Gap_mm;

            Position_mm[indexStart + ServoControl.X_AXIS] = DesiredLocation.X;
            Position_mm[indexStart + ServoControl.Y_AXIS] = DesiredLocation.Y;
            Position_mm[indexStart + ServoControl.Z_AXIS] = DesiredLocation.Z;

            Velocity_mmpersec[indexStart + ServoControl.X_AXIS] = motionParameters.AxisData[indexStart + ServoControl.X_AXIS].Vel_Max_Limit_mmpersec * SpeedPctChecked / 100.0;
            Velocity_mmpersec[indexStart + ServoControl.Y_AXIS] = motionParameters.AxisData[indexStart + ServoControl.Y_AXIS].Vel_Max_Limit_mmpersec * SpeedPctChecked / 100.0;
            Velocity_mmpersec[indexStart + ServoControl.Z_AXIS] = motionParameters.AxisData[indexStart + ServoControl.Z_AXIS].Vel_Max_Limit_mmpersec * SpeedPctChecked / 100.0;

            AxisMask = ServoControl.AXISMASK_ALL;
            if (useGridRobot)
                AxisMask = ServoControl.AXISMASK_ALL_GRID;
            else
                AxisMask = ServoControl.AXISMASK_ALL;
            if ((CheckForStepMode) && (HandleStepMode() != 0)) return 1;
            if (servoController.Move(Position_mm, Velocity_mmpersec, AxisMask, true) != 0) return 1;

            // Lets check to see how far from the desired postition we are in X and Y ....
            if (WaitForCompletion)
            {
                MachineCoordinate RealCurrentLocation = new MachineCoordinate();
                GetCurrentPosition(out RealCurrentLocation, useGridRobot);

                if (Math.Abs(RealCurrentLocation.X - DesiredLocation.X) > MAX_XY_ERROR_MM || Math.Abs(RealCurrentLocation.Y - DesiredLocation.Y) > MAX_XY_ERROR_MM)
                {
                    string errorMessage = "Warning MoveSafely: **** Desired X,Y (" + DesiredLocation.X.ToString() + "," + DesiredLocation.Y.ToString() +
                        ")  Actual X,Y (" + RealCurrentLocation.X.ToString() + "," + RealCurrentLocation.Y.ToString() + ")";
                    Debug.WriteLine(errorMessage);
                    MessageBox.Show(errorMessage);
                }
            }

            return 0;
        }

        // Hack by Pete
        public int MoveXUnsafe(object Caller, double AxisLoc_mm, double Speed_pct,  bool WaitForCompletion)
		{
			MachineCoordinate	CurrentLocation = new MachineCoordinate();
			MachineCoordinate	DesiredLocation = new MachineCoordinate();
			double []			Position_mm = new double [ServoControl.NUM_AXES];
			double []			Velocity_mmpersec = new double [ServoControl.NUM_AXES];
			double				SpeedPctChecked = LimitValue(Speed_pct, 0.1, 100.0);
			int					AxisMask;

			if (HandleManualCommands(Caller) != 0) return 1;
			if (HandleNotHomed(Caller) != 0) return 1;
			if (HandleHardwareNotInitialized() != 0) return 1;	
			if (HandleMotionError() != 0) return 1;

			// find out our current location.
			if (GetCurrentPosition(out CurrentLocation) != 0) return 0;

			// poke in the value for the X axis.
			DesiredLocation = CurrentLocation;
			// go back up by the offset amount
			DesiredLocation.X = AxisLoc_mm;

			Position_mm[ServoControl.X_AXIS] = DesiredLocation.X;
			Position_mm[ServoControl.Y_AXIS] = DesiredLocation.Y;
			Position_mm[ServoControl.Z_AXIS] = DesiredLocation.Z;

            Velocity_mmpersec[ServoControl.X_AXIS] = motionParameters.AxisData[ServoControl.X_AXIS].Vel_Max_Limit_mmpersec * SpeedPctChecked / 100.0;
            Velocity_mmpersec[ServoControl.Y_AXIS] = motionParameters.AxisData[ServoControl.Y_AXIS].Vel_Max_Limit_mmpersec * SpeedPctChecked / 100.0;
            Velocity_mmpersec[ServoControl.Z_AXIS] = motionParameters.AxisData[ServoControl.Z_AXIS].Vel_Max_Limit_mmpersec * SpeedPctChecked / 100.0;

			// Only move the x axiz
			AxisMask = ServoControl.AXISMASK_X;
			if (servoController.Move(Position_mm, Velocity_mmpersec, AxisMask, true) != 0) return 1;

			return 0;
		}

		public bool NotHomed()
		{
			for (int Axis = 0; Axis < ServoControl.NUM_AXES; ++Axis)
			{
				if (axisHomed[Axis] == false) return true;
			}

			return false;
		}

        // PKv5.5.0  Changed to allow single robot operation for development system.

        public bool NotHomed(bool useGridRobot)
        {
            int stAxisIndex = 0;
            int endAxisIndex = 2;
            if (useGridRobot)
            {
                stAxisIndex = 3;
                endAxisIndex = 5;
            }
            for (int Axis = stAxisIndex; Axis <= endAxisIndex; ++Axis)
            {
                if (axisHomed[Axis] == false) return true;
            }

            return false;
        }

        public bool MotionErrorOccurred()
		{
			return servoController.MotionError;
		}

        // PKv5.3.2 

        public int SetUseLastPositionTest(object Caller, bool useLastPositionTest)
        {
            return servoController.SetLastPositionTest(useLastPositionTest);
        }

        public bool GetUseLastPositionTest(object Caller)
        {
            return servoController.GetLastPositionTest();
        }

        // PKv5.3.3

        public int HopZYZ(double aboveDistance_mm, double yOffset_mm, double downDistance_mm, double zRounding_mm, double yRounding_mm, int settling_ms, bool useGridRobot)
        {
            return servoController.HopZYZ(aboveDistance_mm, yOffset_mm, downDistance_mm, zRounding_mm, yRounding_mm, settling_ms, useGridRobot);
        }


        #endregion

        #region private methods

        private int HandleMotionError()
		{
			if (this.MotionErrorOccurred())
			{
				if (System.Windows.Forms.MessageBox.Show("Motion error occured. Would you like to recover?", "MOTION ERROR", System.Windows.Forms.MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes) return 1;
	
				return servoController.MotionErrorRecovery();
			}

			return 0;
		}


		private int HandleNotHomed(object Caller)
		{
			if (NotHomed())
			{
				if (System.Windows.Forms.MessageBox.Show("Robot not homed. Would you like to home now?", "NOT HOMED", System.Windows.Forms.MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes) return 1;
	
				return HomeAllAxes(Caller);
			}

			return 0;
		}

        // PKv5.5.0

        private int HandleNotHomed(object Caller, bool useGridRobot)
        {
            if (NotHomed(useGridRobot))
            {
                if (System.Windows.Forms.MessageBox.Show("Robot not homed. Would you like to home now?", "NOT HOMED", System.Windows.Forms.MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes) return 1;

                return HomeAllAxes(Caller);
            }

            return 0;
        }


        private int HandleHardwareNotInitialized()
		{
			if (!hardwareInitialized)
			{
				if (System.Windows.Forms.MessageBox.Show("Motion system not initialized. Would you like to initialize now?", "MOTION ERROR", System.Windows.Forms.MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes) return 1;
	
				if (servoController.InitializeMotion(true) != 0) return 1;

				hardwareInitialized = true;
			}

			return 0;
		}


		private int HandleInvalidAxis(int Axis)
		{
			if ((Axis < 0) || (Axis >= ServoControl.NUM_AXES))
			{
				System.Windows.Forms.MessageBox.Show("Invalid axis selected: " + Axis.ToString(), "MOTION ERROR");
				return 1;
			}
			
			return 0;
		}


		// let user okay each move before it is executed
		private int HandleStepMode()
		{
			if (this.stepMode)
			{
				if (System.Windows.Forms.MessageBox.Show("Proceed with move?", "STEP MODE", System.Windows.Forms.MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes) return 1;
	
				return 0;
			}
			return 0;
		}

		
		// if manual moves are disabled and this isn't the master object, then don't allow it
		private int HandleManualCommands(object Caller)
		{
			if ((this.allowManualControl == false) && (!object.ReferenceEquals(Caller, master))) return 1;

			// either we're allowing manual control or this is the master object - allow the move
			return 0;
		}

		
		private double LimitValue(double Value, double Min, double Max)
		{
			if (Value < Min) return Min;
			if (Value > Max) return Max;
			return Value;
		}


		private int UpdateSettingsFromFile()
		{
			for (int Axis = 0; Axis < ServoControl.NUM_AXES; ++Axis)
			{
				axisHomed[Axis] = false;
			}

			try
			{
				// deserialize the move parameters from XML file
				FileStream				ConfigFile = new FileStream(this.configurationFile, FileMode.Open);
				XmlSerializer			Serializer = new XmlSerializer(typeof(MotionConfigurationData));
				
				motionParameters = (MotionConfigurationData)Serializer.Deserialize(ConfigFile);
                int i = 7;

			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("Error getting motion settings from file '" + this.configurationFile + "'");
				return 1;
			}

			return 0;
		}

		
		#endregion

	}

}