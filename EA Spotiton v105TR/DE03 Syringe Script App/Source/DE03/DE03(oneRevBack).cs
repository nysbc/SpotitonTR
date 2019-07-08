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


		#region properties  
  
		// Tells whether the DE03 has been initialized or not
		public static bool Initialized
		{
			get{return initialized;}
		}
		#endregion

		#region public methods	  (New ones for DE03 Development)

		public static int CosWaveCalculationA(int cosDesiredFreq,int maxBufferSize,out double cosClosestFreq,out int repeatCycles)
		{
			cosClosestFreq=0.0;
			repeatCycles =0;

			double desiredPeriod = 1000000.0/cosDesiredFreq;  // in usec
			int maxCycles = (int) Math.Floor(maxBufferSize / desiredPeriod); 
			double fractionalPeriod = desiredPeriod - Math.Floor(desiredPeriod);
			if (fractionalPeriod == 0.0)
			{
				cosClosestFreq=cosDesiredFreq;
				repeatCycles =1;
				return 0;
			}

			double newFractionalPeriod=0.0;
			double bestFractionalPeriod=0.0;

			double minError = 0.0;
			int minErrorIndex_i = 0;
			int minErrorIndex_j = 0;
			if (fractionalPeriod<0.5)
				minError = Math.Abs(fractionalPeriod);
			else
				minError = Math.Abs(1-fractionalPeriod);
				

			double error =0.0;
			for (int i=2;i<=maxCycles;i++)
			{
				for (int j=1;j<i;j++)
				{
					newFractionalPeriod = Convert.ToDouble(j)/i;
					error = Math.Abs(newFractionalPeriod-fractionalPeriod);
					if (error < minError)
					{
						bestFractionalPeriod = newFractionalPeriod;
						minErrorIndex_i = i;
						minErrorIndex_j = j;
						minError = error;
					}
				}
			}

			double bestPeriod = 0.0;
			if (minErrorIndex_i == 0)
			{
				if (fractionalPeriod<0.5)
					bestPeriod = Math.Floor(desiredPeriod);
				else
					bestPeriod = Math.Floor(desiredPeriod)+1;

				cosClosestFreq = 1000000.0/bestPeriod;   
				repeatCycles=1;
				return 0;
			}
			
			bestPeriod = Math.Floor(desiredPeriod) + bestFractionalPeriod;

			cosClosestFreq = (1000000.0 * minErrorIndex_i)/Convert.ToInt32(minErrorIndex_i * bestPeriod);
			repeatCycles=minErrorIndex_i;
			return 0;
		}

		public static int WriteToFile(string outFilePath,double[] buffer,int lengthToWrite )
		{
			string line;
			if (File.Exists(outFilePath)) 
			{
				Console.WriteLine("Overwriting File: {0}", outFilePath);
				File.Delete(outFilePath);
			}
			StreamWriter sr = File.CreateText(outFilePath);
			for (int i=0;i<=lengthToWrite;i++)
			{
				line = Convert.ToString(buffer[i]);
				sr.WriteLine(line);
			}
			sr.WriteLine(" ");
			sr.Close();
			return 0;

//			for (int i=0;i<16;i++)
//			{
//				line =Convert.ToString(byteArray[i*16]);
//				for (int j=1;j<16;j++)
//				{
//					line += delimStr+Convert.ToString(byteArray[i*16+j]);
//				}
//				sr.WriteLine(line);
//			}
//			sr.WriteLine(sourceFile);
//			for (int i=0;i<256;i++)
//			{
//				line = Convert.ToString(byteArray[i]);
//				sr.WriteLine(line);
//			}
		}

		public static int WriteToFile(string outFilePath,string aString)
		{
			string line;
			if (File.Exists(outFilePath)) 
			{
				Console.WriteLine("Overwriting File: {0}", outFilePath);
				File.Delete(outFilePath);
			}
			StreamWriter sr = File.CreateText(outFilePath);
			int lengthToWrite = Convert.ToInt32(aString.Length / 4);
			int intEq;
			for (int i=0;i<lengthToWrite;i++)
			{
				line = aString.Substring(i*4,4);
				intEq = Convert.ToInt32(line,16);
				sr.WriteLine("Hex ="+line+" int = "+intEq.ToString());
			}
			sr.WriteLine(" ");
			sr.Close();
			return 0;

			//			for (int i=0;i<16;i++)
			//			{
			//				line =Convert.ToString(byteArray[i*16]);
			//				for (int j=1;j<16;j++)
			//				{
			//					line += delimStr+Convert.ToString(byteArray[i*16+j]);
			//				}
			//				sr.WriteLine(line);
			//			}
			//			sr.WriteLine(sourceFile);
			//			for (int i=0;i<256;i++)
			//			{
			//				line = Convert.ToString(byteArray[i]);
			//				sr.WriteLine(line);
			//			}
		}



		public static int CosWaveCalculation(int cosDesiredFreq,int maxBufferSize,out int cosClosestFreq,out int repeatCycles)
		{
			cosClosestFreq=0;
			repeatCycles =0;

			double desiredPeriod = 1000000.0/cosDesiredFreq;  // in usec
			int maxCycles = (int) Math.Floor(maxBufferSize / desiredPeriod); 
			double fractionalPeriod = desiredPeriod - Math.Floor(desiredPeriod);
			if (fractionalPeriod == 0.0)
			{
				cosClosestFreq=cosDesiredFreq;
				repeatCycles =1;
				return 0;
			}
			double minError = 9.9;
			int minErrorIndex = 0;
			double error =0.0;
			for (int i=1;i<=maxCycles;i++)
			{
				error = i*fractionalPeriod-Math.Floor(i*fractionalPeriod);
				if (error >0.5) {error = 1-error;}
				if (error < minError)
				{
					minErrorIndex = i;
					minError = error;
				}
			}

			double tempDouble = (1000000.0 * minErrorIndex)/Convert.ToInt32(minErrorIndex * desiredPeriod);
			cosClosestFreq = Convert.ToInt32(tempDouble);

			if (minErrorIndex == maxCycles)
			{
				int nextHighestFreq = Convert.ToInt32(1000000.0/Math.Floor(desiredPeriod));
				if ((nextHighestFreq-cosDesiredFreq)<Math.Abs(cosClosestFreq-cosDesiredFreq))
				{
					minErrorIndex = 1;
					cosClosestFreq = nextHighestFreq;
				}

			}

			repeatCycles=minErrorIndex;
			return 0;
		}

		public static int InitTipControl(int desiredPort)
		{
			// first the com port
			if (InitComPort(desiredPort) != 0) return 1;

			port = desiredPort;
			
			// then the device
//			if (InitializeBoard() != 0) return 1;

			initialized = true;

			return 0;
		}

		public static int InitializeBoard()
		{
			return InitializeBoard(1);
		}

		public static int InitializeBoard(int channel)
		{
			DE03.SendCommand("E0");   // Make sure the echo is off.
			Thread.Sleep(50); 
			DE03.SendCommand("E0");   // Twice is required on DE03 powerup for some reason.
			Thread.Sleep(50); 
			if (DE03.SendCommand("Z1","0",100)!=0) return 1;   // HIGH Voltage On
			string commandStr = "CH"+channel.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // Turn on the Channel
			return 0;
		}

        // Revision R2.02 - Now uses tipNo

        public static int TrapSetup(int tipNo, int leading, int dwell, int trailing, int trapDrops, int trapFreq, int trapAmp,
            int strobeDelay, int strobeDuration, int triggerSetting, int triggerDelay, int triggerPeriod)
        {
            string commandStr = "CH" + tipNo.ToString();
            if (DE03.SendCommand(commandStr, "0", 100) != 0) return 1;   // Turn on the Channel
            return TrapSetup(leading, dwell, trailing, trapDrops, trapFreq, trapAmp,
             strobeDelay, strobeDuration, triggerSetting, triggerDelay, triggerPeriod);

        }

		public static int TrapSetup(int leading,int dwell,int trailing,int trapDrops,int trapFreq,int trapAmp,
			int strobeDelay,int strobeDuration,
			int triggerSetting, int triggerDelay, int triggerPeriod)
		{

			int bufferLength = leading+dwell+trailing+1;
			if ((strobeDelay + strobeDuration) > bufferLength)
			{
				bufferLength = strobeDelay + strobeDuration + 1;
			}

			int [] waveVolt = new int[bufferLength];

			waveVolt[0]=4095;
			int step = trapAmp / leading;   // Use integer math , truncate error is small
			for (int i=1;i<=leading;i++)
			{
				waveVolt[i] = waveVolt[i-1]-step;
			}
			for (int i=(leading+1);i<=(leading+dwell);i++)
			{
				waveVolt[i] = waveVolt[i-1];
			}
			step = trapAmp / trailing;
			for (int i=(leading+dwell+1);i<(leading+dwell+trailing);i++)
			{
				waveVolt[i] = waveVolt[i-1]+step;
			}
			for (int i=(leading+dwell+trailing);i<bufferLength;i++)
			{
				waveVolt[i]=4095;
			}

			DE03.SendCommand("WU"+bufferLength.ToString(),"0",100);   // Don't wait for reply
            Console.WriteLine("DE03 Trap Setup - Downloading wave to DE03");
//			System.Threading.Thread.Sleep(50);				
//			DE03.SendCommandNoCR("   ");   // Bug in code requires 3 spaces.
//			System.Threading.Thread.Sleep(10);				
			bool strobeOn=false;
			string tempString;
			for (int i=0;i<bufferLength;i++)  
			{
				strobeOn=false;
				if (i>=strobeDelay)
				{
					if (i<(strobeDelay+strobeDuration))
					{
						strobeOn=true;
					}
				}
				tempString = Convert.ToString(waveVolt[i],16);
				tempString = tempString.PadLeft(3,'0');
				if (strobeOn)
				{
					tempString = "1"+tempString;
				}
				else
				{
					tempString = "0"+tempString;
				}
				tempString = tempString.ToUpper();
				DE03.SendCommandNoCR(tempString);
                Console.Write(tempString);
				System.Threading.Thread.Sleep(1);
			}
			DE03.SendCommand("");  // CR to wrap things up
            Console.WriteLine("");

            string resp = GetResponse(1000);			// DE03 responds with Checksum....  TODO Should make sure it got the right one.

            Console.WriteLine("Checksum from DE03 = {0}", resp);
			string commandStr = "RC"+trapDrops.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // Number of drops

			if (triggerSetting == 0)
			{
				commandStr = "TE0";
				if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // Trigger Enable (TE1 or TE2)
				int trapDelay = (1000000/trapFreq)- bufferLength;  
				if (trapDelay < 0) {trapDelay = 100;}
				commandStr = "RP"+trapDelay.ToString();
				if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // Delay Between drops
			}
			else
			{
				commandStr = "TE"+triggerSetting.ToString();
				if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // Trigger Enable (TE1 or TE2)
				commandStr = "RP"+triggerDelay.ToString();
				if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // Delay after wave before next trigger can be used
				commandStr = "TP"+triggerPeriod.ToString();
				if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // Trigger period
			}


			if (DE03.SendCommand("WR1","0",100)!=0) return 1;   // One Wave

			// In case i want to send this out to file.
			//  outputFile = @"C:\DE03 C# Example App\temp_h.txt";
			//	DE03.WriteToFile(outputFile,outString);

			return 0;
		}

        // Revision R2.02,  now uses tip No
        public static int CosSetup(int tipNo, int desiredFreq, int inBurstNo, int cosAmplitude,int strobeDuration, int strobeDelay,
            int numOfBursts, int triggerSetting, int triggerDelay, int triggerPeriod)
        {
            string commandStr = "CH" + tipNo.ToString();
            if (DE03.SendCommand(commandStr, "0", 100) != 0) return 1;   // Turn on the Channel
            Thread.Sleep(50);
            return CosSetup( desiredFreq, inBurstNo, cosAmplitude, strobeDuration, strobeDelay,
			 numOfBursts,  triggerSetting,  triggerDelay,  triggerPeriod);
        }

		public static int CosSetup(int desiredFreq,int inBurstNo,int cosAmplitude,
			int strobeDuration,int strobeDelay,
			int numOfBursts, int triggerSetting, int triggerDelay, int triggerPeriod)

		{
			if (DE03.SendCommand("NS1","0",100)!=0) return 1;   // Number Cycles 1, when producing the wave

			string commandStr = "FS"+desiredFreq.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // Frequency

			commandStr = "WR"+inBurstNo.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // Number of cycles in each burst

			commandStr = "A"+Convert.ToString(cosAmplitude,16);
			if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // Amplitude
			
			commandStr = "FD"+strobeDuration.ToString();  //PKv.008
            if (VERSION > 6)commandStr = "BD" + strobeDuration.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // Strobe Duration

            commandStr = "FP" + strobeDelay.ToString();   //PKv.008
            if (VERSION > 6) commandStr = "BP" + strobeDelay.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // Strobe Position

			Thread.Sleep(10);    // 10msec delay before the PW command.
			if (DE03.SendCommand("PW","0",100)!=0) return 1;   // Produce Wave

			Thread.Sleep(10);    // 10msec delay after the PW command.
			commandStr = "RC"+numOfBursts.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // Number of times to repeat the burst

			commandStr = "TE"+triggerSetting.ToString();   // 0,1,2
			if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // External Trigger Enable (TE0,TE1,TE2)

			commandStr = "RP"+triggerDelay.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // Delay after burst before next trigger or burst

			commandStr = "TP"+triggerPeriod.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return 1;   // Trigger period usually 1

			return 0;
		}

		public static int StartWaveform(bool waitForCompletion, int timeout_ms)
		{
            if (VERSION > 6)
            {
                if (DE03.SendCommand("G", "0", 100) != 0) return 1;          //PKv.008  
            }
            else
            {
                if (DE03.SendCommand("G", "1", 100) != 0) return 1;        //PKv.008  
            }

                    // TODO - Make sure this waitForCompletion works.   //PKv.008  still need to test
			if (waitForCompletion)
			{
				string respStr = GetResponse(timeout_ms);
				if (respStr != "0")
				{
					string message= @"DE03.StartWaveform waiting for completion \n Expecting 0, reply="+respStr;
					MessageBox.Show(message,"DE03 Controller", MessageBoxButtons.OK);
					return 1;
				}
			}
			return 0;
		}

		public static int StopWaveform()
		{
			if (DE03.SendCommand("S","0",300)!=0)       //PKv.008 (longer bias ramp is default in version .008 longer timeout OK for version .006 too. 
			{
				MessageBox.Show("DE03.StopWaveform: S command did not receive 0 reply","DE03 Controller", MessageBoxButtons.OK);
				return 1; 
			}
			return 0;
		}



		#endregion

		#region public methods	  (Old ones that were used for DE02,  have not been updated yet)	


		// This routine will setup DE02 to send out a burst of cosine waves when the 
		// "Go" command is received.

		public static int CosWaveSetup(int dropsPerBurst,int cosFreq,int waveFreq,int waveCount,int[] chAmplitude)
		{
			if (DefaultDE02()!= 0) {return 1;}
			if (SendCommand("MC", "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}
			if (SendCommand("N" + dropsPerBurst.ToString(), "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}
			if (SendCommand("F" + cosFreq.ToString(), "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}
			// TODO - Strobe is mode dependent.  Not sure strobe parameters
			// should be set in MW or MC  for this.			
			if (SendCommand("LD10", "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}	// DE02 Firmware bug, This is defaulting to 150 (10 is more reasonable)	
			if (SendCommand("LO5", "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}	// DE02 Firmware bug, default not 1 in version 8.	10Sept2008 Update
			if (SendCommand("MW", "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}
			if (SendCommand("WC1", "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}
			if (SendCommand("N" + waveCount.ToString(), "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}
			if (SendCommand("F" + waveFreq.ToString(), "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}
			int tempAmp;
			for (int i=1; i<=8; i++)
			{
				tempAmp = chAmplitude[i-1];
				if (SendCommand("A" + i.ToString() + tempAmp.ToString(), "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}
			}
			// Strobe is mode dependent.  Not sure strobe parameters
			// should be set in MW or MC  for this.
			if (SendCommand("LD10", "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}	// DE02 Firmware bug, This is defaulting to 150 (10 is more reasonable)	
			if (SendCommand("LO5", "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}	// DE02 Firmware bug, default not 1 in version 8.	10Sept2008 Update
			return 0;
		}


		// Routine will setup DE02 to send out a burst of trapezoidal waves each time the "GO"
		// Command is received.
		// Routine must be followed ModeTrapUpdate

		public static int TrapSetup(int numberOfPulses,int pulseFreq,int amp,int dwell,int leading,int trailing,int[] chAmplitude)
		{
			if (DefaultDE02()!= 0) {return 1;}
			if (SendCommand("MT", "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}
			if (SendCommand("N" + numberOfPulses.ToString(), "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}
			if (SendCommand("F" + pulseFreq.ToString(), "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}
			if (SendCommand("LD10", "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}	// DE02 Firmware bug, This is defaulting to 150 (10 is more reasonable)	
			if (SendCommand("LO5", "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}	// DE02 Firmware bug, default not 1 in version 8.	10Sept2008 Update
			if (SendCommand("TA"+amp.ToString(), "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}	
			if (SendCommand("TD"+dwell.ToString(), "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}	
			if (SendCommand("TL"+leading.ToString(), "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}		
			if (SendCommand("TT"+trailing.ToString(), "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}		
			if (SendCommand("X0", "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}
			int tempAmp;
			for (int i=1; i<=8; i++)
			{
				tempAmp = chAmplitude[i-1];
				if (SendCommand("A" + i.ToString() + tempAmp.ToString(), "0", COMMAND_TIMEOUT) != 0)	{	return 1;	}
			}
			return 0;
		}

		public static int DefaultDE02()
		{
			if (SendCommand("S") != 0)	{	return 1;	}
			Thread.Sleep(50);
			if (SendCommand("DE") != 0)	{	return 1;	}
			Thread.Sleep(100);
			if (SendCommand("E0") != 0)	{	return 1;	}
			Thread.Sleep(50);
			return 0;
		}

		#region Public Lower Level Communication Routines
	
		/// <summary>
		/// Read Comm port without CR or LF
		/// </summary>
		/// <returns>return string</returns>
//        public static string ReadComm()
//        {
//            string retString = "";
//            try
//            {
//                // Assuming the last character is always a CR.
////				retString = Comm.Read(port, COMMAND_TIMEOUT);
//                ports[0].NewLine = "\r";
//                ports[0].ReadTimeout = COMMAND_TIMEOUT;
//                retString = ports[0].ReadLine();
//                if (retString == null || retString.Length < 1)
//                {
//                    DialogResult status = MessageBox.Show("Comm Port " + port.ToString() + " timeout.", "DE02 Controller", MessageBoxButtons.RetryCancel);
//                    if (status == DialogResult.Cancel)
//                    {
//                        return "";
//                    }
//                }
//            }
//            catch
//            {
//                return "Error reading com port";
//            }
//            return retString;
//        }


		public static int SendCommandNoCR(string command)
		{
		
			// clear out any old data
			if (ClearResponseBuffer() != 0) return 1;
            ports[0].Write(command);
//			Comm.Write(port, command);
			return 0;
		}


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
		public static int SendCommand(byte[] buffer)
		{
			// clear out any old data
			if (ClearResponseBuffer() != 0) return 1;

            ports[0].Write(buffer, 0, buffer.Length);

//			Comm.WriteBytes(port, buffer);
			Thread.Sleep(COMMAND_DELAY_BUG);
			return 0;
		}

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
				MessageBox.Show("Unexpected Reply from piezo controller: " + resp);
				return 1;
			}
			Thread.Sleep(COMMAND_DELAY_BUG);
			return 0;
		}

		#endregion

		#endregion

		#region private methods  Actually some of these are now public....
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
				}
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("Error initializing DE03 com port", "COM PORT ERROR");
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
                System.Windows.Forms.MessageBox.Show("Error clearing DE03 com port buffer", "COM PORT ERROR");
                return 1;
            }

            return 0;
        }
		
        //private static int ClearResponseBuffer()       // Old one...
        //{
        //    try
        //    {
        //        // read out anything that's in there
        //        Comm.ClearReadBuffer(port);
        //    }
        //    catch
        //    {
        //        System.Windows.Forms.MessageBox.Show("Error clearing controller com port buffer", "COM PORT ERROR");
        //        return 1;
        //    } 
        //    return 0;
        //}

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
                // Assuming the last character is always a CR.
                ports[0].NewLine = "\r";                // Added this
                ports[0].ReadTimeout = timeout;
                retString = ports[0].ReadLine();
                retString = retString.Trim('\n');

                if (retString == null || retString.Length < 1)
                {
                    if (!supressError)
                    {
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

        //  Is there any characters in the incoming communcation buffer

        public static bool ResponseWaiting()
        {
            if (ports[0].BytesToRead > 0) return true;
            return false;
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