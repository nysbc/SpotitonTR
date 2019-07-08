using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.IO.Ports;
using System.Collections.Generic;
using System.Threading;

// PKv3.00 , Corrected ValveOutputPositionIsOnRight bug.

namespace Aurigin
{
	public enum SyringeValvePosition
	{
		InputPosition = 0,
		OutputPosition = 1,
		BypassPosition = 2
	}

	public class SyringeStatus
	{
		public bool Idle;
		public bool InitializationError;
		public bool InvalidCommand;
		public bool InvalidOperand;
		public bool InvalidCommandSeq;
		public bool DeviceNotInitialized;
		public bool PlungerOverload;
		public bool ValveOverload;
		public bool PlungerMoveNotAllowed;
		public bool CommandOverflow;
		
		public bool	NoError
		{
			get
			{
				// PK23May added the ! not
				return !(InitializationError || 
					InvalidCommand || 
					InvalidOperand ||
					InvalidCommandSeq ||
					DeviceNotInitialized ||
					PlungerOverload ||
					ValveOverload ||
					PlungerMoveNotAllowed ||
					CommandOverflow);
			}
		}

		public override string ToString()
		{
			StringBuilder status = new StringBuilder("Status\n", 300);
			
			if (NoError) 
			{
				status.Append("  No error");
				return status.ToString();
			}

			if (InvalidCommand)				status.Append("  Invalid command");
			if (InvalidOperand)				status.Append("  Invalid command operand");
			if (InvalidCommandSeq)			status.Append("  Invalid command sequence");
			if (DeviceNotInitialized)		status.Append("  Need to initialize pump");
			if (PlungerOverload)			status.Append("  Plunger overload");
			if (ValveOverload)				status.Append("  Valve overload");
			if (PlungerMoveNotAllowed)		status.Append("  Plunger move not allowed");
			if (CommandOverflow)			status.Append("  Command buffer overflow");
			
			return status.ToString();
		}
	}

	public class SyringeParameters
	{
		#region members
		public const int	MinInitForceCodeValue = 0;
		public const int	MaxInitForceCodeValue = 3;
		public const int	MinSpeedCodeValue = 1;
		public const int	MaxSpeedCodeValue = 40;
		
		private string		pumpName;
		private byte		pumpAddress;
		private int			initializationForceCode;			// how hard it drives the pumps upwards to find the top at startup
		private int			stepsPerStroke;						// 3000
		private int			syringeVolume_uL;
		private bool		valveOutputPositionIsOnRight;		// when viewed from front of pump
		private int		    refillSpeedCode;					// How fast it will pull from the resevoir when priming
		private int			pumpCommPort;						// Each pump can be at a different comm port
		#endregion members

		#region properties
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string PumpName
		{
			get{return pumpName;}
			set{pumpName = value;}
		}

		public int InitializationForceCode_0to3	// 0 = full force, 3 = slow enough for 25 uL syringe
		{
			get {return initializationForceCode;}
			set 
			{
				if (value < MinInitForceCodeValue)
				{
					initializationForceCode = MinInitForceCodeValue;
				}
				else if (value > MaxInitForceCodeValue)
				{
					initializationForceCode = MaxInitForceCodeValue;
				}
				else
				{
					initializationForceCode = value;
				}
			}
		}

		public int StepsPerStroke
		{
			get {return stepsPerStroke;}
			set {stepsPerStroke = value;}
		}

		public int SyringeVolume_uL
		{
			get {return syringeVolume_uL;}
			set {syringeVolume_uL = value;}
		}

		public bool ValveOutputPositionIsOnRight
		{
			get {return valveOutputPositionIsOnRight;}
			set {valveOutputPositionIsOnRight = value;}
		}

		public byte PumpAddress
		{
			get {return pumpAddress;}
			set {pumpAddress = value;}
		}

		public int PumpCommPort
		{
			get {return pumpCommPort;}
			set {pumpCommPort = value;}
		}

		public int RefillSpeedCode
		{
			get {return refillSpeedCode;}
			set {refillSpeedCode = value;}
		}


		#endregion properties
	}

	// this class gets serialized to XML
	public class SyringeConfigurationData
	{
		//public short	ComPort;						//  how we talk to the pumps
		public int		TimeoutForValveMove_ms;			//  how long to wait for valves to switch
		public int		TimeoutForSyringeMove_ms;		// 	how long to wait on a plunger move
		public int		TimeoutForCommandReponse_ms;	//  how long to wait for pump to answer
		public int		DelayBetweenCommands_ms;		//  give the com port some time
		public bool		MultiplexPump;					//	indicate whether to multiple pump
		public int		TotalSyringePump;				//	indicate the total number of syringe pump

		// could have multiple pumps with different parameters
		[XmlArrayItem("PUMP_PARAMETERS", typeof(SyringeParameters))]
		public SyringeParameters[]	SyringeParams;		// much information about each axis
	}

	public class UserSyringeControl
	{
		#region constants
		// hex command items for talking to the pumps
		private const byte COMMAND_START		= 0x2f;		// for cmd & answer
		private const byte BASE_PUMP_ADDRESS	= 0x31;		// first available pump address
		private const byte COMMAND_END			= 0x0d;		
		private const byte COMPUTER_ADDRESS		= 0x30;
		private const byte END_RESPONSE			= 0x03;
		private const byte LINE_SYNC_CHAR		= 0xff;
		private const byte LINE_FEED_CHAR		= 0x0a;
		private const byte STATUS_IDLE_BIT		=(1 << 5);

		private const byte STATUS_ERROR_INITIALIZING		= 0x01;		// failed to initialize
		private const byte STATUS_ERROR_INVALID_COMMAND		= 0x02;		// bad command sent
		private const byte STATUS_ERROR_INVALID_OPERAND		= 0x03;		// bad parameter in a command
		private const byte STATUS_ERROR_INVALID_CMD_SEQ		= 0x04;		// problem in command
		private const byte STATUS_ERROR_NOT_INITIALIZED		= 0x07;		// need to initialize the pump
		private const byte STATUS_ERROR_PLUNGER_OVERLOAD	= 0x09;		// lost steps, need to reinitialize
		private const byte STATUS_ERROR_VALVE_OVERLOAD		= 0x0a;		// lost too many steps, need to reinitialize
		private const byte STATUS_ERROR_MOVE_NOT_ALLOWED	= 0x0b;		// can't move plunger when valve is in bypass position
		private const byte STATUS_ERROR_CMD_OVERFLOW		= 0x0f;		// too many characters in the command queue, how about executing some?

		public const int MIN_SPEED_CODE = 1;
		public const int MAX_SPEED_CODE = 40;
		#endregion constants

        private static List<SerialPort> ports = new List<SerialPort>();    // New list of serial ports
        private static List<ManualResetEvent> mutexes = new List<ManualResetEvent>();

		#region members
		private SyringeConfigurationData pumpConfiguration;
		private int[] port = new int[31];
		private bool initialized = false;
		private string configurationFile;

		#endregion members
			
		#region public methods

		// constructor
		public UserSyringeControl(string ConfigurationFile)
		{
			configurationFile = ConfigurationFile;
			UpdateSettingsFromFile();
			initialized = false;
//			Console.WriteLine("Syringe Pump Multiplexing set to " + pumpConfiguration.MultiplexPump.ToString());
		}

		public int UpdateSettingsFromFile()
		{
			// deserialize the syringe parameters from XML file
			try
			{
				FileStream ConfigFile = new FileStream(configurationFile, FileMode.Open);
				XmlSerializer Serializer = new XmlSerializer(typeof(SyringeConfigurationData));
				pumpConfiguration = (SyringeConfigurationData)Serializer.Deserialize(ConfigFile);
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("Error getting syringe settings from file '" + configurationFile + "'");
				return 1;
			}

			return 0;
		}

		public int InitPumpControl()
		{
            Console.WriteLine("\nInitializing Syringe Pumps ....");
			// first initialize the com port
            if (!initialized)
            {
  
                if (InitComPort() != 0) return 1;

                Console.WriteLine("    Communication Port Initialized");

                for (int i = 0; i < pumpConfiguration.TotalSyringePump; i++)
                {
                    // then the device
                    if (InitializePump(i) != 0) return 1;
                    Console.WriteLine("    Initialized Syringe Pump: "+i.ToString());
                }
            }
            else
            {
                Console.WriteLine("    Already Initialized");
            }

            Console.WriteLine("    Initialization Successful");

			initialized = true;
			return 0;
		}

		public int InitializePump(int index)
		{
			byte pumpAddress = pumpConfiguration.SyringeParams[index].PumpAddress;

			if (InitializePlungerAndValve(pumpAddress) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Error initializing pump: " + pumpConfiguration.SyringeParams[index].PumpName, "SYRINGE ERROR");
				return 1;
			}
			
			return WaitForPumpMoveAndCheckStatus(pumpAddress, pumpConfiguration.TimeoutForSyringeMove_ms);
		}

		public int SyringeVolume_uL(int index)
		{
			int Volume = this.pumpConfiguration.SyringeParams[index].SyringeVolume_uL;
			return Volume;
		}

		public int SyringeRefillSpeed(int index)
		{
			int SpeedCode = this.pumpConfiguration.SyringeParams[index].RefillSpeedCode;
			return SpeedCode;
		}


		public int WaitForPumpMoveAndCheckStatus(byte pumpAddress, int timeout_ms)
		{
			SyringeStatus status;
			int index = pumpAddress - 1;
			
			if (WaitForPumpToBeIdle(pumpAddress, timeout_ms) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Timed out waiting for syringe pump: " + pumpConfiguration.SyringeParams[index].PumpName, "SYRINGE ERROR");
				return 1;
			}

			if (GetPumpStatus(pumpAddress, out status) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Error getting status for syringe pump: " + pumpConfiguration.SyringeParams[index].PumpName, "SYRINGE ERROR");
				return 1;
			}

			if (!status.NoError)
			{
				System.Windows.Forms.MessageBox.Show(pumpConfiguration.SyringeParams[index].PumpName + "\n" + status.ToString(), "SYRINGE ERROR");
				return 1;
			}

			return 0;
		}

		public SyringeStatus PumpStatus(int index)
		{
			SyringeStatus status = new SyringeStatus();
			
			if (CheckForInitialize() != 0) return status;
		
			byte PumpAddress = pumpConfiguration.SyringeParams[index].PumpAddress;

			GetPumpStatus(PumpAddress, out status);

			return status;
		}

		public int EmptySyringe(int index, int speedCode, bool waitForCompletion)
		{
			if (CheckForInitialize() != 0) return 1;
		
			byte pumpAddress = pumpConfiguration.SyringeParams[index].PumpAddress;

			// set the move speed
			if (this.SetPlungerSpeed(pumpAddress, speedCode) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Error setting speed for syringe pump: " + pumpConfiguration.SyringeParams[index].PumpName, "SYRINGE ERROR");
				return 1;
			}

			// give it some time between commands
			System.Threading.Thread.Sleep(pumpConfiguration.DelayBetweenCommands_ms);

			// move to the top - position zero
			if (this.MovePlungerToAbsolutePosition(pumpAddress, 0) != 0) return 1;
			
			if (!waitForCompletion) return 0;

			// wait for the pump to be idle
			return WaitForPumpMoveAndCheckStatus(pumpAddress, pumpConfiguration.TimeoutForSyringeMove_ms);
		}

		public int Aspirate(int index, double volume_uL, int speedCode, bool waitForCompletion)
		{	
			if (CheckForInitialize() != 0) return 1;

			byte pumpAddress = pumpConfiguration.SyringeParams[index].PumpAddress;

			// set the move speed
			if (this.SetPlungerSpeed(pumpAddress, speedCode) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Error setting speed for syringe pump: " + pumpConfiguration.SyringeParams[index].PumpName, "SYRINGE ERROR");
				return 1;
			}

			// give it some time between commands
			System.Threading.Thread.Sleep(pumpConfiguration.DelayBetweenCommands_ms);

			// start the move
			if (this.AspirateRelative(pumpAddress, VolumeToSteps(index, volume_uL)) != 0) return 1;

			if (!waitForCompletion) return 0;

			// wait for the pump to be idle
			return WaitForPumpMoveAndCheckStatus(pumpAddress, pumpConfiguration.TimeoutForSyringeMove_ms);
		}

		public int Dispense(int index, double volume_uL, int speedCode, bool waitForCompletion)
		{
			if (CheckForInitialize() != 0) return 1;

			byte pumpAddress = pumpConfiguration.SyringeParams[index].PumpAddress;

			// set the move speed
			if (this.SetPlungerSpeed(pumpAddress, speedCode) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Error setting speed for syringe pump: " + pumpConfiguration.SyringeParams[index].PumpName, "SYRINGE ERROR");
				return 1;
			}

			// give it some time between commands
			System.Threading.Thread.Sleep(pumpConfiguration.DelayBetweenCommands_ms);

			// start the move
			if (this.DispenseRelative(pumpAddress, VolumeToSteps(index, volume_uL)) != 0) return 1;	

			if (!waitForCompletion) return 0;

			// wait for the pump to be idle
			return WaitForPumpMoveAndCheckStatus(pumpAddress, pumpConfiguration.TimeoutForSyringeMove_ms);
		}

		public int SetValvePosition(int index, SyringeValvePosition position)
		{
			if (CheckForInitialize() != 0) return 1;

			byte pumpAddress = pumpConfiguration.SyringeParams[index].PumpAddress;
			int  Result = 0;

			switch (position)
			{
				case SyringeValvePosition.BypassPosition:
					Result = this.ValvesToBypass(pumpAddress);
					break;
				case SyringeValvePosition.InputPosition:
					Result = this.ValvesToInput(pumpAddress);
					break;
				case SyringeValvePosition.OutputPosition:
					Result = this.ValvesToOutput(pumpAddress);
					break;
			}

			if (Result != 0)
			{
				System.Windows.Forms.MessageBox.Show("Error setting valve position for syringe pump: " + pumpConfiguration.SyringeParams[index].PumpName, "SYRINGE ERROR");
				return 1;
			}

			// wait for the pump to be idle
			return WaitForPumpMoveAndCheckStatus(pumpAddress, pumpConfiguration.TimeoutForValveMove_ms);
		}

		public int SetValvePosition(int index, SyringeValvePosition position, bool waitForCompletion)
		{
			if (CheckForInitialize() != 0) return 1;

			byte pumpAddress = pumpConfiguration.SyringeParams[index].PumpAddress;
			int  Result = 0;

			switch (position)
			{
				case SyringeValvePosition.BypassPosition:
					Result = this.ValvesToBypass(pumpAddress);
					break;
				case SyringeValvePosition.InputPosition:
					Result = this.ValvesToInput(pumpAddress);
					break;
				case SyringeValvePosition.OutputPosition:
					Result = this.ValvesToOutput(pumpAddress);
					break;
			}

			if (Result != 0)
			{
				System.Windows.Forms.MessageBox.Show("Error setting valve position for syringe pump: " + pumpConfiguration.SyringeParams[index].PumpName, "SYRINGE ERROR");
				return 1;
			}

			if (!waitForCompletion)	
			{
				return 0;
			}

			// wait for the pump to be idle
			return WaitForPumpMoveAndCheckStatus(pumpAddress, pumpConfiguration.TimeoutForValveMove_ms);
		}

		public int GetValvePosition(int index)
		{
			if (CheckForInitialize() != 0) return 1;

			byte pumpAddress = pumpConfiguration.SyringeParams[index].PumpAddress;

			// send command
			SendCommand(pumpAddress, "?6");

			// wait for an answer
			string response = "";
			if (WaitForPumpResponse(pumpConfiguration.TimeoutForCommandReponse_ms, 3, out response, pumpAddress.ToString()) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Timed out waiting for response from syringe pump", "SYRINGE PUMP ERROR");
				return -1;
			}

			char[] ch = new char[3] {'i', 'o', 'b'};
			return response.IndexOfAny(ch);
		}
		#endregion public methods

		#region private methods
		private int CheckForInitialize()
		{
			if (!initialized)
			{
				System.Windows.Forms.MessageBox.Show("Syringe pump not initialized.", "SYRINGE ERROR");
				return 1;
			}

			return 0;
		}
	
		#region Cavro commands

		// move valve to output position and move syringe to top (empty)
		private int InitializePlungerAndValve(byte pumpAddress)
		{
			string command; 

			int index = pumpAddress - 1;
			if (pumpConfiguration.SyringeParams[index].ValveOutputPositionIsOnRight)
			{
				command = "Z" + pumpConfiguration.SyringeParams[index].InitializationForceCode_0to3 + "R";  // Changed from Y PKv3.00
			}
			else
			{
				command = "Y" + pumpConfiguration.SyringeParams[index].InitializationForceCode_0to3 + "R";  // Changed from Z PKv3.00
			}

			return this.SendCommand(pumpAddress, command);
		}


		// move plunger to some position
		private int MovePlungerToAbsolutePosition(byte pumpAddress, int position_steps)
		{
			return this.SendCommand(pumpAddress, "A" + position_steps.ToString() + "R");
		}


		// aspirate some number of steps relative to wherever we are now
		private int AspirateRelative(byte pumpAddress, int steps)
		{
			return this.SendCommand(pumpAddress, "P" + steps.ToString() + "R");
		}


		// dispense some number of steps relative to wherever we are now
		private int DispenseRelative(byte pumpAddress, int steps)
		{
			return this.SendCommand(pumpAddress, "D" + steps.ToString() + "R");
		}


		// position valve to the reservoir
		private int ValvesToInput(byte pumpAddress)
		{
			return this.SendCommand(pumpAddress, "IR");		// pk-2011-05-12,  reservoir in the right
		}

		// position valve for flush
		private int ValvesToOutput(byte pumpAddress)
		{
			return this.SendCommand(pumpAddress, "OR");      // pk-2011-05-12,  tip on the right         
		}

		// position valve for dispensing - connect tip to backpressure source
		private int ValvesToBypass(byte pumpAddress)
		{
			return this.SendCommand(pumpAddress, "BR");
		}

		// use speed codes, larger is slower	
		private int SetPlungerSpeed(byte pumpAddress, int speedCode)
		{
			return this.SendCommand(pumpAddress, "S" + speedCode.ToString() + "R");
		}

		private int StopPlunger(byte pumpAddress)
		{
			return this.SendCommand(pumpAddress, "T");
		}
		#endregion cavro commands

		#region com port operations

        public static void Close()
        {
            foreach (SerialPort port in ports)
            {
                port.Close();
            }
            ports.Clear();
            mutexes.Clear();
        }


		private int InitComPort()
		{
//			Aurigin.Xml profile;

			try
			{
				// Obtain port assignment data
//				profile = new Aurigin.Xml(@"..\data\commPort.xml");

				port[0] = 1;   //Syringe-TODO
				port[1] = 2;   //Syringe-TODO


				if (!pumpConfiguration.MultiplexPump)
				{
					for (int i = 0; i < pumpConfiguration.TotalSyringePump; i++)
                    {

                        #region newway

                        SerialPort serialPort = new SerialPort();
                        ManualResetEvent mutex = new ManualResetEvent(false);
                        serialPort.PortName = string.Format("COM{0}", port[i]);
                        Console.WriteLine("    Serial Port: {0}", serialPort.PortName);
                        serialPort.BaudRate = 9600;
                        serialPort.DataBits = 8;
                        serialPort.StopBits = StopBits.One;
                        serialPort.Parity = Parity.None;
                        serialPort.Open();

                        serialPort.DtrEnable = true;
                        ports.Add(serialPort);
                        mutexes.Add(mutex);

                        #endregion

                        // Set the port to 9600 baud, no parity bit, 8 data bits, 1 stop bit (all standard)
//						port[i-1] = (short)profile.GetValue("SyringePump" + i.ToString(), "Port", 3);	
//						Comm.Open(port[i-1], 9600, 8, Rs232.DataParity.Parity_None, Rs232.DataStopBit.StopBit_1);
//						Comm.Open(pumpConfiguration.SyringeParams[i-1].PumpCommPort, 9600, 8, Rs232.DataParity.Parity_None, Rs232.DataStopBit.StopBit_1);

						// Force the DTR line high, used sometimes to hang up modems
//						Comm.DTR(port[i-1], true);
//						Comm.DTR(pumpConfiguration.SyringeParams[i-1].PumpCommPort, true);

					}
				}
				else
				{
//					port[0] = (short)profile.GetValue("SyringePump1", "Port", 3);	
//					Comm.Open(port[0], 9600, 8, Rs232.DataParity.Parity_None, Rs232.DataStopBit.StopBit_1);
					// Force the DTR line high, used sometimes to hang up modems
//					Comm.DTR(port[0], true);
                    SerialPort serialPort = new SerialPort();
                    ManualResetEvent mutex = new ManualResetEvent(false);
                    serialPort.PortName = string.Format("COM{0}", port[0]);
                    Console.WriteLine("    Serial Port: {0}", serialPort.PortName);
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
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show("Error initializing syringe com port at " + ex.ToString(), "COM PORT ERROR");
				return 1;
			}

			return 0;
		}
		
		private int ClearResponseBuffer()
		{
			try
			{
				// read out anything that's in there
                if (!pumpConfiguration.MultiplexPump)
                {
                    for (int i = 0; i < ports.Count; i++)
                    {
                        ports[i].ReadExisting();
                        //					Comm.ClearReadBuffer(pumpConfiguration.SyringeParams[i-1].PumpCommPort);
                    }
                }
                else
                {
                    ports[0].ReadExisting();
                }
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("Error clearing syringe com port buffer", "COM PORT ERROR");
				return 1;
			}

			return 0;
		}

		private int ClearResponseBuffer(string pumpAddress)
		{
			try
			{
                // TODO - Make sure it's within range 
 
                //// read out anything that's in there
                if (!pumpConfiguration.MultiplexPump)
                {
                    ports[Convert.ToInt16(pumpAddress) - 1].ReadExisting();
                }
                else
                {
                    ports[0].ReadExisting();
                }
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("Error clearing syringe com port buffer", "COM PORT ERROR");
				return 1;
			}

			return 0;
		}

		private string FormCommand(byte pumpAddress, string command)
		{
			StringBuilder cmd = new StringBuilder();
			cmd.Append((char)COMMAND_START);
			cmd.Append(pumpAddress.ToString());
			cmd.Append(command);
			cmd.Append((char)COMMAND_END);
			return cmd.ToString();;
		}

		private byte [] FormByteCommand(byte pumpAddress, string command)
		{
			byte [] ByteCommand = new byte[command.Length + 3];
//			byte temp = 0x30;
			int		i = 0;

			ByteCommand[i++] = COMMAND_START;

			ByteCommand[i++] = (byte)(pumpAddress + (byte)48);	//PK23May - Need to use ascii equivalent address
			
			for (int j=0; j<command.Length; ++j)
			{
				ByteCommand[i++] = (byte)command[j];
			}
			
			ByteCommand[i++] = COMMAND_END;

			return ByteCommand;
		}
		
		private int SendCommand(byte pumpAddress, string command)
		{		
			// clear out any old data
			if (ClearResponseBuffer(pumpAddress.ToString()) != 0) return 1;

			// it won't take a command until it's idle
			if (WaitForPumpToBeIdle(pumpAddress, pumpConfiguration.TimeoutForSyringeMove_ms) != 0) return 1;

			try
			{
				// send the command
				string cmd = FormCommand(pumpAddress, command);
				if (!pumpConfiguration.MultiplexPump)
				{

//					Comm.Write(pumpConfiguration.SyringeParams[Convert.ToInt16(pumpAddress)-1].PumpCommPort, cmd);
                    ports[pumpAddress - 1].Write(cmd);
				}
				else
				{
                    ports[0].Write(cmd);
				}
				System.Threading.Thread.Sleep(50);			//PK24May - Give it a bit of time to get out the buffer.
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("Error sending command to syringe com port", "COM PORT ERROR");
				return 1;
			}

			return 0;
		}

		// wait until the syringe pump says it's done or we get tired of waiting
		// returns zero if it's idle, 1 otherwise
		private int WaitForPumpToBeIdle(byte pumpAddress, int waitTime_ms)
		{
			SyringeStatus		status;
			System.DateTime		StartTime = DateTime.Now;
			System.TimeSpan		MaxTime = new TimeSpan(0, 0, 0, 0, waitTime_ms);
			
			do 
			{
				// find out how it's doing
				if (GetPumpStatus(pumpAddress, out status) != 0) return 1;

				// if it's idle, return now
				if (status.Idle) 
				{
					return 0;
				}

				System.Threading.Thread.Sleep(pumpConfiguration.DelayBetweenCommands_ms);

			} while ((System.DateTime.Now - StartTime) < MaxTime);  // check for move timeout

			// last chance - maybe we missed it with the poll delay
			if (GetPumpStatus(pumpAddress, out status) != 0) return 1;

			return (status.Idle ? 0 : 1);
		}
		
		// can send this command any time
		// can't use the send command function because it calls this function first to see if it's okay to send a command
		private int GetPumpStatus(byte pumpAddress, out SyringeStatus status)
		{
			byte statusByte;
			status = new SyringeStatus();

			// clear out any old data
			if (ClearResponseBuffer(pumpAddress.ToString()) != 0) return 1;

			try
			{
				// send the command
				string cmd = FormCommand(pumpAddress, "Q");
				if (!pumpConfiguration.MultiplexPump)
				{
                    ports[pumpAddress-1].Write(cmd);
	//				Comm.Write(pumpConfiguration.SyringeParams[Convert.ToInt16(pumpAddress)-1].PumpCommPort, cmd);
				}
				else
				{
                    ports[0].Write(cmd);
	//				Comm.Write(port[0], cmd);
				}
				System.Threading.Thread.Sleep(50);			//PK24May - Required for some reason
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("Error sending command to syringe com port", "COM PORT ERROR");
				return 1;
			}

			// wait for an answer
			string response = "";
			if (WaitForPumpResponse(pumpConfiguration.TimeoutForCommandReponse_ms, 3, out response, pumpAddress.ToString()) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Timed out waiting for response from syringe pump", "SYRINGE PUMP ERROR");
				return 1;
			}

			// 1 Start Answer Block / (2Fh)
			StringBuilder ans = new StringBuilder();
			ans.Append((char)COMMAND_START);
			if (response.IndexOf(ans.ToString()) != 0)
			{
				System.Windows.Forms.MessageBox.Show("First character in response from syringe pump is incorrct", "SYRINGE PUMP ERROR");
				return 1;
			}
			
			// 2 Master Address (30h)
			ans = new StringBuilder();
			ans.Append((char)COMPUTER_ADDRESS);
			if (response.IndexOf(ans.ToString()) < 0)
			{
				System.Windows.Forms.MessageBox.Show("First character in response from syringe pump is incorrct", "SYRINGE PUMP ERROR");
				return 1;
			}

			// 3 Status Character
			char[] bytes = response.ToCharArray();
			statusByte = (byte)bytes[2];

			// n Response (if applicable)
			// n + 1 <ETX> (03h embedded in the answer block)
			// n + 2 Carriage return
			// n + 3 Line Feed (0Ah)
			// n + 4 Line Turn Around Character (FFh)

			// if this bit is high, the pump is idle
			status.Idle					= ((STATUS_IDLE_BIT & statusByte) > 0);		
			status.InvalidCommand		= ((statusByte & STATUS_ERROR_INVALID_COMMAND) == STATUS_ERROR_INVALID_COMMAND);
			status.InvalidOperand		= ((statusByte & STATUS_ERROR_INVALID_OPERAND) == STATUS_ERROR_INVALID_OPERAND);
			status.InvalidCommandSeq	= ((statusByte & STATUS_ERROR_INVALID_CMD_SEQ) == STATUS_ERROR_INVALID_CMD_SEQ);
			status.DeviceNotInitialized	= ((statusByte & STATUS_ERROR_NOT_INITIALIZED) == STATUS_ERROR_NOT_INITIALIZED);
			status.PlungerOverload		= ((statusByte & STATUS_ERROR_PLUNGER_OVERLOAD) == STATUS_ERROR_PLUNGER_OVERLOAD);
			status.ValveOverload		= ((statusByte & STATUS_ERROR_VALVE_OVERLOAD) == STATUS_ERROR_VALVE_OVERLOAD);
			status.PlungerMoveNotAllowed= ((statusByte & STATUS_ERROR_MOVE_NOT_ALLOWED) == STATUS_ERROR_MOVE_NOT_ALLOWED);
			status.CommandOverflow		= ((statusByte & STATUS_ERROR_CMD_OVERFLOW) == STATUS_ERROR_CMD_OVERFLOW);
			
			return 0;
		}
		
		// Syringe-TODO - This is very odd that it waits for a response from any pump.   This one not used much...
        //private int WaitForPumpResponse(int waitTime_ms, int minimumCharacters, out string response)
        //{
        //    response = "";
        //    try
        //    {
        //        for (int i = 0; i < pumpConfiguration.TotalSyringePump; i++)
        //        {
        //            response = Comm.Read(pumpConfiguration.SyringeParams[i].PumpCommPort, waitTime_ms);
        //            if (response == null || response.Length <= 0)
        //            {
        //                return 1; // timeout
        //            }
        //            response = "";
        //        }
        //    }
        //    catch
        //    {
        //        System.Windows.Forms.MessageBox.Show("Syringe com port error", "COM PORT ERROR");
        //        return 1;
        //    }
        //    return 0;
        //}

		private int WaitForPumpResponse(int waitTime_ms, int minimumCharacters, out string response, string address)
		{
			response = "";
			try
			{
				if (!pumpConfiguration.MultiplexPump)
				{
//					response = Comm.Read(port[Convert.ToInt16(address)-1], waitTime_ms);
//					response = Comm.Read(pumpConfiguration.SyringeParams[Convert.ToInt16(address)-1].PumpCommPort, waitTime_ms);
                    ports[Convert.ToInt16(address) - 1].ReadTimeout = waitTime_ms;
                    response = ports[Convert.ToInt16(address) - 1].ReadLine();
				}
				else
				{
                    ports[0].ReadTimeout = waitTime_ms;
                    response = ports[0].ReadLine();
	//				response = Comm.Read(port[0], waitTime_ms);
				}

				if (response == null || response.Length <= 0)
				{
					return 1; // timeout
				} 
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("Syringe com port error", "COM PORT ERROR");
				return 1;
			}
			return 0;
		}

		public int VolumeToSteps(int index, double volume_uL)
		{
			return (int) (pumpConfiguration.SyringeParams[index].StepsPerStroke * volume_uL / pumpConfiguration.SyringeParams[index].SyringeVolume_uL);
		}

		public int TotalSyringe()
		{
			return pumpConfiguration.TotalSyringePump;
		}
		#endregion com port operations
	
		#endregion private methods
	}

}

