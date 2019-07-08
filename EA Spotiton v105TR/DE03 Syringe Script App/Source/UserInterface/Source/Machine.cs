using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using EA.PixyControl.ClassLibrary;
using Aurigin;

// PKv5.3.3 Added parameters for high speed Hop 

namespace EA.PixyControl

{
	public class ProjectFiles
	{

        public const string MachineDevelopmentSystemFlag = @"..\data\MachineDevelopmentSystemFlag.xml";
        public const string MachineConfigurationFile  = @"..\data\MachineParameters.xml";
        public const string MachineConfigurationFileDev = @"..\data\MachineParametersDev.xml";      // PKv5.5.5
        public const string SyringeConfigurationFile  = @"..\data\syringe.xml";  
		public const string MotionConfigurationFile = @"..\data\MotionParameters.xml";
        public const string MotionConfigurationFileDev = @"..\data\MotionParametersDev.xml";       // PKv5.5.0
        public const string MainSequenceFile = @"..\data\ProcessSequence.xml";
//		public const string PiezoSequenceFile = @".\PiezoSequences.xml"; // kc060524
	}

    public class MachineDevelopmentSystemFlag                           // PKv5.5.5   Allow this to run on the Phoenix development system.
    {
        public bool RunOnPHXDevSystem;

        static public MachineDevelopmentSystemFlag GetMachineDevelopmentSystemFlagFromFile(string fileName)
        {
            try
            {
                // deserialize the move parameters from XML file
                FileStream workFileStream = new FileStream(fileName, FileMode.Open);
                XmlSerializer Serializer = new XmlSerializer(typeof(MachineDevelopmentSystemFlag));

                MachineDevelopmentSystemFlag mdsf = (MachineDevelopmentSystemFlag)Serializer.Deserialize(workFileStream);       // Serialize from the file,  does this create a new object on the HEAP ??
                                                                                                                                // Will i leak memory.
                workFileStream.Close();
                return mdsf;
            }

            catch
            {
                // If the file does not exist report the error and just set the development system flag to false.
                System.Windows.Forms.MessageBox.Show("Error loading " + fileName);
                MachineDevelopmentSystemFlag mdsf = new MachineDevelopmentSystemFlag();
                mdsf.RunOnPHXDevSystem = false;
                return mdsf;
            }
        }
    }

    public class MachineConfigurationData
	{
		public short				FiringControllerComPort;
        public short                FiringControllerComPort2;   //2019-01-15   
        public string				WorklistDirectory;
		public string				WorklistExtension;
		public string				LogFileDirectory;
		public string				LogFileExtension;
		public string				DefaultLogFile;
        public string               VideoFileDirectory;     // Location of recorded video files from the experiments on Spotiton    
        public int                  NumberOfTips;           // total number of tips on the machine               

		[XmlArrayItem("TIP", typeof(MachineCoordinate))]
		public MachineCoordinate[]	Tips;					// first tip should have offset 0,0,0
		public WashBowl				WashPoint;				// tip 1 centered at top of wash well 
		public CoordinateMatrix		Microtiter;				// tip 1 centered at top of plate well
		public MachineCoordinate	DownCameraOffset;		// x,y,z offset from tip 1
		public MachineCoordinate	DispenseDeck;			// tip 1 touching deck origin
        [XmlArrayItem("TIP", typeof(MachineCoordinate))]
        public MachineCoordinate[]  SideCamInspectPointTips;
        public MachineCoordinate	SideCamInspectPoint;	// tip 1 in the side camera
		public MachineCoordinate	SafePoint;				// A nice out of the way spot.
        public MachineCoordinate    SafePointGrid;           // Now for the grid robot  // PKv4.0,2015-05-06
		public MachineCoordinate	Reservoir;
        public MachineCoordinate    WashPointUPTip1;        // position just above the wash station - tip1
        public MachineCoordinate    WashPointDOWNTip1;      // position for washing inside the bowl
        public MachineCoordinate    AspiratePointUPTip1;    // position just above the sample well - tip1
        public MachineCoordinate    AspiratePointDOWNTip1;  // position in the sample well - tip1
        public MachineCoordinate    StainPointDOWNTip1;     // position in the sample well - tip1
        [XmlArrayItem("TIP", typeof(MachineCoordinate))]
        public MachineCoordinate[]  BackCameraPointTips;    // position for spoting on the grid - tip1
        [XmlArrayItem("TIP", typeof(MachineCoordinate))]
        public MachineCoordinate[]  CalculatedVideoCenterPoint;// calibrated position for the center of the video camera
        [XmlArrayItem("TIP", typeof(MachineCoordinate))]
        public MachineCoordinate[]  GridDropLocation;       // Positions in the bowl for dropping of the grids
        [XmlArrayItem("TIP", typeof(MachineCoordinate))]
        public MachineCoordinate[]  GridDropLocationBent;       // Positions in the bowl for dropping of the grids
        public MachineCoordinate    InLiquidEthaneBowl;     // Position of the grid robot when in the liquid ethane bowl when plunge freezing
        public MachineCoordinate    InLiquidEthaneBowlBent;     // Position of the grid robot when in the liquid ethane bowl when plunge freezing
        public MachineCoordinate    GridToCameraPoint;      // position of Grid robot that puts the grid in front of the camera
        public MachineCoordinate    GridToCameraPointBent;  // position of Grid robot that puts the grid in front of the camera with bent tweezers.
        public MachineCoordinate    AboveLiquidNitrogen;    // Only Z value used,  Offset for the liquid N2 level when moving the grid from ethane to drop location
        public double               Z2HighestPoint;         // Z2 axis point all the way at the top, away from the camera and the tips
        public int                  SyringeMask;            // Syringe Mask
        public int                  DelayAfterms;           // Delay after aspiration by syringe in ms
        public int                  SyringePullSpeed;       // Syringe pulling speed           
        public int                  PrimeVolume;            // Syringe volume used for priming the tips (ul)
        public int                  SyringePullSpeedPrime;  // Prime setting for syringe
        public int                  SyringePushSpeedPrime;  // Prime setting for syringe
        public int                  DelayAftermsPrime;      // Prime delay ms
        public int[]                DE03TrapSetupLeading;   // PKv5.2.4   Made an array
        public int[]                DE03TrapSetupDwell;
        public int[]                DE03TrapSetupTrailing;
        public int                  DE03TrapSetupTrapDrops;
        public int                  DE03TrapSetupTrapFreq;
        public int                  DE03TrapSetupStrobeDuration;
        public int                  DE03TrapSetupTriggerSetting;
        public int                  DE03TrapSetupTriggerDelay;
        public int                  DE03TrapSetupTriggerPeriod;
        public int                  DE03TrapSetupStrobeDelay;
        public int[]                TrapAmp;
        // PKv5.2.4   Cosine burst for "on-the-fly" dispense
        public int[]                DE03CosSetupFreq;
        public int[]                DE03CosSetupAmp;
        public int                  DE03CosSetupDrops;    // Will be used for inspection.  Will be overwritten when on-the-fly on the grid.
        public int[]                NumberDropsToSpot;      // number of drops the piezo will despense when we click on the video feed
        public double []            AspirateVolume;         // sample aspiration volume for each piezo ,  2019-01-16,  made array, changed to double

        public long                 SideCameraID;
        public long                 BackCameraID;
        public double               VideoToMotionScaler_X;  //scale for convertion of pixel to real world mm for axis motion
        public double               VideoToMotionScaler_Y;  //scale for convertion of pixel to real world mm for axis motion
        public double               PlungeAxisWithEthaneUp;
        public double               PlungeAxisWithEthaneDown;
        public double               PlungeAxisSpeedPct;

        // new parameters for the Theta harmonic drive
        public int                  ThetaAccelCount;
        public int                  ThetaDecelCount;
        public int                  ThetaMaxVelCount;
        public int                  ThetaRotate_0;
        public int                  ThetaRotate_90;
        public int                  WashTime;               // duration of tip washing in ms
        public MachineCoordinate    TipWipingPointStart;    // position of the tip robot for the beginning of the wiping 
        public MachineCoordinate    TipWipingPointStop;     // position of the tip robot for the end of the wiping


        //		public short			
        public bool DIOPresent;
        public bool MotionPresent;
        public bool SyringePresent;
        public bool DE03Present;
        public bool DE03_2_Present = true;          // 2019-01-16,    
        public bool[] TipVideoCentered;

        public bool ClearTipsB4Plunge;      // PKv5.1  Optionally move tips back in x axis before plunging
        public bool UseBentTweezerPoints;   // PKv5.1  Will use a 2nd set of points for the grip robot tweezer positions

        // PKv5.2.1   Allow better control of plunge axis speed / setting different values throughout the program.   Used for normal operation NON-OTF.
        public double PlungeAxisMaxSpeed_Plunge;  // 
        public double PlungeAxisAccel_Plunge;     // Accel and decel during the plunge.
        public double PlungeAxisMaxSpeed_General; // Speed during other moves.
        public double PlungeAxisAccel_General;    // Accel and decel during other moves.

        // PKv5.2.5   On-the-fly OTF processing parameters,  Note - accel, decel and speed of plunge are included for OTF (independent of NON - OTF) 
        public double OTFStartAbove_mm;         // How far above the dispense target the motion will start (z axis relative mm)
        public double OTFDispOffset_mm;         // Small offset to compensate for fly by motion, like dropping a bomb (z axis relative mm).
        public double OTFZStopView_mm;          // Not used PKv5.2.6,  was used in PKv5.2.5
        public double OTFTargetZOffset_mm;      // PKv5.2.6,  Will dispense this ammount above GridToCam and then stop at GridToCam
        public double OTFAccelPlunge_mm_ss;      // Plunge Acceleration mm / (sec-sec).
        public double OTFDecelPlunge_mm_ss;      // Plunge decel mm / (sec-sec).
        public double OTFSpeedPlunge_mm_s;       // Plunge speed mm / sec.
        public double OTFTrapDispSpeed_mm_s;     // Trap Only OTF -  Slow speed of OTF dispense eg 50mm/sec
        public double OTFTrapStartOffset_mm;     //  How long of line it draws (eg 2.5mm) 
        public double OTFTrapDropSpacing_mm;     //  Distance between spots (eg 0.1mm , will calculate 25 drops)


        // 2019-01-24  Time Resolved Upgrade

        public double OTFTRDistBetweenTriggers_mm= 0.25;  //  How far apart each trigger will be (0.250mm)
        public double OTFTRDistBetweenTips_mm = 4.5;    //  How far apart tip 2 is from tip 1.

        public int AspriateDelayAfterBypass = 1000;

        // PKv5.3.3   High Speed Hop to Nitrogen Parameters added,  speeds and accelerations will be independent and only used during the hop.

        public bool HOPEnable;                  // Will enable the high speed HOP method.
        public double HOPZ2Accel_mm_ss;         // Z2 axis accel,decel and speed during hop
        public double HOPZ2Decel_mm_ss;         // 
        public double HOPZ2Speed_mm_s;          // 
        public double HOPY2Accel_mm_ss;         // Y2 axis accel, decel and speed during hop
        public double HOPY2Decel_mm_ss;         // 
        public double HOPY2Speed_mm_s;          // 

        public double Y2AccelGen_mm_ss;         // General Y2 accel, decel and speed setting after the hop (if we wish to slow it down for more general moves).
                                                // Similar to the PlungeAxisMaxSpeed_General
        public double Y2DecelGen_mm_ss;         // 
        public double Y2SpeedGen_mm_s;          // 

        public int Trigger_CW;                  //  50 to 255 (Had to increase the width of the pulse for the trigger output to
                                                //  get the hopping to work.   Previously this was hardcoded at 50.

        public double HOPZ2CornerCut_mm;        // How close to finishing the ZUp move during the hop move before the Y axis starts moving (typically 0.5mm)
        public double HOPY2CornerCut_mm;        // How close to finishing the Y move during the hop move before the Z axis starts moving down (typically 1.0mm)

        public int HOPSettleTime_ms;            // Might try 50,  100 is the default that the machine is currently running.  After hopping
                                                //  the hop method will set this back to 100.

        // PKv5.5.0
        public bool PKDevSystemEnable;          // Enable operation on MDGES development system (3 axis).  Will bypass things like the grid robot moves, Harmonic drive, etc.. 

        public bool InsDropUseHighSpeed;        // TRUE = Use high speed image acquisition system for drople inspection,  FALSE = Do it the old way.

        public int InsDropBurstSize;            // Number of drops in each burst
        public int InsDropTotalBursts;          // Total number of bursts to inspect.
        public int InsDropDE03SetupBursts;      // Number of bursts the DE03 will look for,  For experimentation with triggered acquisition you can set this to <InsDropTotalBursts
                                                //   and veriry you miss the correct number of frame acquisitions.   // TODO Don't think this is used or working properly.,  Leave set to 1000
        public int InsDrop1stTriggerDelay_usec;    // Set to 1 to see how the droplet stream starts.
        public int InsDropTriggerDelayInc_usec; // How much to increment the delay after trigger between each burst
        public int InsDropDelayBTWTriggers_ms;  // How long between each trigger.

        public int InsDropCamFlashDelay;        // Set to 1
        public int InsDropCamFlashDuration;     // Set to 100,  Actual flash duration is controlled by the high speed trigger board.
        public int InsDropCamDelayAfterTrip;    // 10 to 10000 usec.. PKv5.5.0.2 (extra addition to the control the start of the inspection.


        public double InsDropVolumePerDrop_nl;  // Used to estimate fluid consumed.

        public int InsDropEndOfStreamDropsFmEnd;    // How far from the end of the stream will we start inspecting. (ie  a 30 drop burst you may want to start looking at 28).  

        // PKv5.5.1
        // Will use SyringePushSpeedPrime for flushing setting  // 14 = 67um tip,  15 = 32um or 40um tip,   16 = 24um tip    higher numbers are slower

        public int usAmp;                       // Amplitude 1800    (Used USWashAmp in StainItOn,    usAmp in this routine.
        public int usFreqSweepCycles;           // 2,  will sweep through these frequencies twice,   USFreqSweepCycles in StainItOn,  usFreqSweepCycles in this routine.
        public int USFreq1;                     // Frequencies to pulse the tip 40,000,  0 means skip
        public int USCount1;                    // Number of pulses  30,000  which would take 750msec
        public int USTime1_ms;                  // 1000 Total time (including off time),   will include off time (250msec)
        public int USFreq2;                     // 60000
        public int USCount2;                    // 45000
        public int USTime2_ms;                  // 1000
        public int USFreq3;                     // 80000
        public int USCount3;                    // 60000
        public int USTime3_ms;                  // 1000
        
        // PKv5.5.3
        public int InsGridCamFlashDelay;        // Set to 1
        public int InsGridCamFlashDuration;     // Set to 100,  Actual flash duration is controlled by the high speed trigger board.
        public int InsGridCamDelayAfterTrip;    // 10 to 10000 usec..

        // PKv5.5.4
        public int OTFSlowDownPercent;          // 1 to 99 ,  percent of the way to grid inspection point before starting the slow down.
        public int OTFSlowDownSpeed;            // Target Speed as the grid passes the inspection point mm / sec.  Try 20mm / sec.

        // PKv5.6
        public string RemoteVideoFileDirectory = @"C:\FlyByImagesFromLowerGridCamera";

        public double OTFLowerGridCamZ;         // Z mm of the lower grid camera,  in robot coordinates

    }

    public class ThisRobot
	{
		public const int TIP_COUNT		= 1;
		public const int SYRINGE_COUNT	= 1;
		
		public UserMotionControl			MotionControl;
		public UserSyringeControl			SyringeControl;
		public UserFiringControl			FiringControl;
        public UserFiringControl2            FiringControl2;            
        //		public UserOpticControl				OpticControl; // kc060529
        private string						configurationFile;
		private MachineConfigurationData	machineParameters;
        public  int                         ActiveTip = 0;    // SPOTITON v0.75 tips 0,1,2

 


        public MachineConfigurationData MachineParameters
		{
			get {return machineParameters;}
		}
	
		#region public methods
        public int getActiveTip()
        { return ActiveTip; }

        public void setActiveTip(int inputTip)
        {
            ActiveTip = inputTip;
        }
		public ThisRobot(string ConfigurationFile)
		{
			configurationFile = ConfigurationFile;                  // Store away the configuration file name.

			// load points, etc. from xml file
			UpdateSettingsFromFile();

            if (machineParameters.PKDevSystemEnable)                // PK5.5.0   Allow homing of tip robot.
            {
                MotionControl = new UserMotionControl((object)this, ProjectFiles.MotionConfigurationFileDev);
            }
            else
            {
                MotionControl = new UserMotionControl((object)this, ProjectFiles.MotionConfigurationFile);
            }

			FiringControl  = new UserFiringControl(machineParameters.FiringControllerComPort);
            FiringControl2 = new UserFiringControl2(machineParameters.FiringControllerComPort2);     //2019-01-15

        }

		public int InitializeRobot()
		{

            if (machineParameters.DIOPresent)
            {
                while (IO.InitInputAndOutputs() != 0)
                {
                    if (System.Windows.Forms.MessageBox.Show("Failed to initialize IO Modules. Would you like to retry?", "IO Initialize Error", System.Windows.Forms.MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes) break;
                }
            }

            // just download parameters - don't home until asked
            if (machineParameters.MotionPresent)
            {
                while (MotionControl.InitializeMotion((object)this, false) != 0)
                {
                    if (System.Windows.Forms.MessageBox.Show("Failed to initialize motion system. Would you like to retry?", "MOTION INITIALIZE ERROR", System.Windows.Forms.MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes) break;
                }
            }

            if (machineParameters.SyringePresent)
            {
                while (AtSyringe.Control.InitPumpControl() != 0)
                {
                    if (System.Windows.Forms.MessageBox.Show("Failed to initialize syringe pump. Would you like to retry?", "SYRINGE INITIALIZE ERROR", System.Windows.Forms.MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes) break;
                }
            }

            // initialize the DE03 controller
            if (machineParameters.DE03Present)
            {
                while (FiringControl.InitTipControl() != 0)
                {
                    if (System.Windows.Forms.MessageBox.Show("Failed to initialize DE03. Would you like to retry?", "DE03 INITIALIZE ERROR", System.Windows.Forms.MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes) break;
                }

                while (FiringControl2.InitTipControl() != 0)
                {
                    if (System.Windows.Forms.MessageBox.Show("Failed to initialize 2nd DE03. Would you like to retry?", "DE03 INITIALIZE ERROR", System.Windows.Forms.MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes) break;
                }

            }

//            if (MessageBox.Show("Try to initialize NI DIO ?", "User Input", MessageBoxButtons.YesNo) == DialogResult.Yes)
//            {

//            }


			return 0;		
	
		}

		public int UpdateSettingsFromFile()
		{
			try
			{
				// deserialize the move parameters from XML file
				FileStream				ConfigFile = new FileStream(this.configurationFile, FileMode.Open);
				XmlSerializer			Serializer = new XmlSerializer(typeof(MachineConfigurationData));
				
				machineParameters = (MachineConfigurationData)Serializer.Deserialize(ConfigFile);
				ConfigFile.Close();
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("Error getting machine settings from file '" + this.configurationFile + "'");
				return 1;
			}

			return 0;
		}
        public int WriteSettingsToFile()
        {
            try
            {
                // deserialize the move parameters from XML file
                TextWriter ConfigFile = new StreamWriter(this.configurationFile);
                XmlSerializer Serializer = new XmlSerializer(typeof(MachineConfigurationData));

                Serializer.Serialize(ConfigFile, machineParameters);
                ConfigFile.Close();
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error writing machine settings from file '" + this.configurationFile + "'");
                return 1;
            }

            return 0;
        }

		#endregion public methods

		#region private methods
		
		#endregion private methods
	}

	
	public class WashBowl : MachineCoordinate
	{
		public double	MaxDepth;

		public WashBowl(string Name, double Xcoord, double Ycoord, double Zcoord) : base(Name, Xcoord, Ycoord, Zcoord)
		{
			MaxDepth = 0.0;
		}

		public WashBowl():this("", 0.0, 0.0, 0.0)
		{}
	}


	/*
	public class MicrotiterPlate : MachineCoordinate
	{
		// coordinate is for well A1
		public int		Rows;
		public int		Columns;
		public double	RowPitch;
		public double	ColPitch;
		public bool		RowIncrementsAlongXAxis;	// true if A1, B1, C1 have different X coordinates, same Y

		public MicrotiterPlate():this("", 0.0, 0.0, 0.0)
		{}

		public MicrotiterPlate(string Name, double Xcoord, double Ycoord, double Zcoord) : base(Name, Xcoord, Ycoord, Zcoord)
		{
			Rows = 0;
			Columns = 0;
			RowPitch = 0.0;
			ColPitch = 0.0;
		}

		
		public int GetPickLocation(int Row, int Col, out MachineCoordinate Loc)
		{
			Loc = new MachineCoordinate("Pick from row " + Row.ToString() + ", col " + Col.ToString(), this.X, this.Y, this.Z);

			if ((Row<1) || (Row>Rows)) return 1;
			if ((Col<1) || (Col>Columns)) return 1;

			if (RowIncrementsAlongXAxis)
			{
				Loc.X += (Row-1) * RowPitch;
				Loc.Y += (Col-1) * ColPitch;
			}
			else
			{
				Loc.X += (Col-1) * ColPitch;
				Loc.Y += (Row-1) * RowPitch;
			}
		
			return 0;
		}
	}
	*/

}