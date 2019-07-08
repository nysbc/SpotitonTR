using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Threading;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows.Forms;

using NationalInstruments.DAQmx;

namespace Aurigin
{
    //PKv4.0,2015-03-05  Modified the software to run with only 6501 DIO module, 16 inputs and 8 outputs.
    // The new DIO Iolator board maps ot the NI 6501 IO board as such:
    // Port 1,  Inputs 1 to 8
    // Port 0,  Inputs 9 to 16,     15 and 16 are jumpers.
    // Port 2,  Outputs 1 to 8.

    public class IO
    {
        private const int COMMAND_TIMEOUT = 5000;

		private static bool initializedInputs = false;
        private static bool initializedOutputs = false;
        private static List<string> portsInput = new List<string>();    // keep a list of port info
        private static List<string> portsOutput = new List<string>();    // keep a list of port info

        private static int[] outvalues = { 0, 0, 0 };

        #region Output Definitions  (These are really not used for much)
        //public const int OutSolenoid1 = -1;
        //public const int OutSolenoid2 = -2;
        //public const int OutSolenoid2 = -2;               
       

        #endregion

        #region Input Definitions
        public const int InModulePresent1 = 15;             
        public const int InModulePresent2 = -16;
        #endregion

        #region properties
        // Tells whether the IO has been initialized or not
		public static bool InitializedInputs
		{
            get { return initializedInputs; }
		}
		#endregion

		#region public methods	  

        public static int InitInputAndOutputs()
        {
            try
            {

                Console.WriteLine("\nInitializing Natiional Instrument IO Modules");

                if (InitializeInputs() != 0) return -1;
                Console.WriteLine("    Input module found");
                if ((ReadInput(IO.InModulePresent1) && ReadInput(IO.InModulePresent2)) == false)
                {
                    string message = "DIO Input Module Present Sensors Not in expected state";
                    message += String.Format("\nInput state check should be  (-ve is on):  {0},  {1}", IO.InModulePresent1, IO.InModulePresent2);
                    MessageBox.Show(message);
                    return -1;
                }
                Console.WriteLine("    Input module verified.  Checked Sensor State (-ve is on): {0},  {1}", IO.InModulePresent1, IO.InModulePresent2);

                if (InitializeOutputs() != 0) return -1;
                Console.WriteLine("    Output Initialization Complete");
                Console.WriteLine("    Initialize Successful");
                return 0;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("   Error initializing NI Dio " + ex.ToString(), "InitInputAndOutputs: Error ");
                Console.WriteLine("    Module Initialization Failed");
                return 1;
            }
        }



        public static int InitializeInputs()
        {
            if (initializedInputs) return 0; // already initialized

            portsInput.AddRange(DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.DIPort, PhysicalChannelAccess.External));
            if (portsInput.Count != 3)
            {
                string message = "DIO - Error initializing Inputs";
                message = message + "\nPorts Found =" + portsInput.Count;
                message = message + "\nPorts Found should be 3 with output module only";
                message = message + "\nPorts Found should be 6 with input and output module";

                MessageBox.Show(message);
  
                return -1;
            }
            initializedInputs = true;
            return 0;
        }

        public static int InitializeOutputs()
        {
            if (initializedOutputs) return 0; // already initialized

            portsOutput.AddRange(DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.DOPort, PhysicalChannelAccess.External));
            if (portsOutput.Count != 3)  
            {
                string message = "DIO - Error initializing Outputs";
                message = message + "\nPorts Found =" + portsOutput.Count;
                message = message + "\nPorts Found should be 3 with output module only";
                message = message + "\nPorts Found should be 6 with input and output module";

                MessageBox.Show(message);
                return -1;
            }

            initializedOutputs = true;

            //  Set them all high (off state) to start with.

 //           IO.Write(0, 255);
 //           IO.Write(1, 255);
            IO.Write(2, 255);

  //          outvalues[0] = 255;
  //          outvalues[1] = 255;
            outvalues[2] = 255;

            return 0;
        }


        // This is a utility function that will just Read and print out all the inputs to the console

        public static int ReadAllInputs()
        {
            if (!initializedInputs) return -1; // Not initialized yet
           
            try
            {
  //              for (int i = 3; i < portsInput.Count; i++)
                for (int i = 0; i < 2; i++)
                {
                    using (Task digitalReadTask = new Task())
                    {
                        digitalReadTask.DIChannels.CreateChannel(
                            portsInput[i],
                            "port"+i.ToString(),
                            ChannelLineGrouping.OneChannelForAllLines);

                        DigitalSingleChannelReader reader = new DigitalSingleChannelReader(digitalReadTask.Stream);
                        UInt32 data = reader.ReadSingleSamplePortUInt32();

                        //Update the Data Read box
                        string hex = String.Format("0x{0:X}", data);
                        Console.WriteLine("Reading Port {0} = {1} , {2}", i,data, hex);
                    }
                }
                return 0;
            }
            catch (DaqException ex)
            {
                MessageBox.Show(ex.Message);
            }

            return 0;

        }

        // Write Output numbered 1 to 8  (corresponds to all 24 outputs on the 6501)
        // If you use a negative number it will invert the signal
        // returns 0 if write was OK,   otherwise error code.

        public static int SetOutput(int outputNumber)
        {
            if (!initializedOutputs) return -1; // Not initialized yet
            int absOutputNumber = Math.Abs(outputNumber);    //  positive
 
   //         int group = 0;
            int group = 2;

            //if (absOutputNumber > 16)
            //    group = 2;
            //else if (absOutputNumber > 8)
            //    group = 1;

            int bit = (int)Math.Pow(2, ((absOutputNumber - 1) % 8));

            bool outputIsOn = false;
            if ((bit & outvalues[group]) > 0)        //  Is the bit already set
            {
               outputIsOn = true;
            }

            // Want to turn on the output
            if (outputNumber > 0)
            {
                if (outputIsOn)
                    return 0;   // Don't have to do anything
                else
                {
                    outvalues[group] += bit;
                    IO.Write(group, outvalues[group]);
                }
            }
            else   // Want to turn off the output
            {
                if (!outputIsOn)
                    return 0;   // Don't have to do anything
                else
                {
                    outvalues[group] -= bit;
                    IO.Write(group, outvalues[group]);
                }
            }

            return 0;
        }

        // Read Output numbered 1 to 8  (corresponds to all 8 outputs on the 6501)
        // If you use a negative number it will invert the signal returned to you.
        // returns 0 if write was OK,   otherwise error code.

        public static bool ReadOutput(int outputNumber)
        {
            if (!initializedOutputs) return false; // Not initialized yet
            int absOutputNumber = Math.Abs(outputNumber);    //  positive

            //int group = 0;

            //if (absOutputNumber > 16)
            //    group = 2;
            //else if (absOutputNumber > 8)
            //    group = 1;

            int group = 2;

            int bit = (int)Math.Pow(2, ((absOutputNumber - 1) % 8));

            int alt_bit_calc = 1 << (absOutputNumber - 1);    // much nicer !


            bool outputIsOn = false;
            if ((bit & outvalues[group]) > 0)        //  Is the bit already set
            {
                outputIsOn = true;
            }

            // Want to turn on the output
            if (outputNumber > 0)
            {
                if (outputIsOn)
                    return true; 
                else
                {
                    return false;
                }
            }
            else   // Inverted case
            {
                if (!outputIsOn)
                    return true;  
                else
                {
                    return false;
                }
            }
        }

        // Will return 0 if input is in correct state before timeout
        // will return -1 if input never acheives correct state

        public static int WaitInput(int inputNumber, int timeOut_ms)
        {
            string message= string.Format("Waiting for input {0}",inputNumber);

            if (ReadInput(inputNumber)) return 0;
            int st = System.Environment.TickCount;
            do
            {
                if (ReadInput(inputNumber)) return 0;
                System.Threading.Thread.Sleep(1);       // Goto sleep for 1 ms,  Update 18 Sept 2012 from 5ms.
                if ((System.Environment.TickCount - st) > timeOut_ms)
                {
                    DialogResult result = MessageBox.Show(message, "Sensor TimeOut", MessageBoxButtons.AbortRetryIgnore);
                    if (result == DialogResult.Abort) return -1;
                    if (result == DialogResult.Ignore) return 0;
                    st = System.Environment.TickCount;
                }

            } while (true);
        }

        // Read Input numbered 1 to 16  (corresponds to all 24 inputs on the 6501)
        // If you use a negative number it will invert the signal
        // returns true if the input is on.

        public static bool ReadInput(int inputNumber)
        {
            if (!initializedInputs) return false; // Not initialized yet
            
            int absInputNumber = Math.Abs(inputNumber);    //  positive
            bool readResult = false;
  //          int group = 0+3;

  //          if (absInputNumber > 16)
  //              group = 2+3;
  //          else if (absInputNumber > 8) 
  //              group = 1+3;

            int group = 1;

            if (absInputNumber > 8) 
                group = 0;

            int bit = (int) Math.Pow(2,((absInputNumber-1) % 8));

            try
            {
                    using (Task digitalReadTask = new Task())
                    {
                        digitalReadTask.DIChannels.CreateChannel(
                            portsInput[group],
                            "port" + group.ToString(),
                            ChannelLineGrouping.OneChannelForAllLines);

                        DigitalSingleChannelReader reader = new DigitalSingleChannelReader(digitalReadTask.Stream);
                        UInt32 data = reader.ReadSingleSamplePortUInt32();

                        //Update the Data Read box
 //                      string hex = String.Format("0x{0:X}", data);
 //                      Console.WriteLine("Reading Port {0} = {1} , {2}", group, data, hex);

                        if ((bit & data) > 0)        //  Is the bit set
                        {
                            readResult = true;
                        }

                        if (inputNumber < 0)
                        {
  //                         Console.WriteLine("Input {0} is {1}", inputNumber, !readResult);
                            return !readResult;
                        }
                        else
                        {
  //                        Console.WriteLine("Input {0} is {1}", inputNumber, readResult);
                            return readResult;
                        }

                    }
 
            }
            catch (DaqException ex)
            {
                MessageBox.Show(ex.Message);
            }

            return false;

        }

        // Will write 8 bit number out the port.
        // port 0,1,2

        private static int Write(int portNumber, int outValue)
        {
            if (!initializedOutputs) return -1; // Not initialized yet

            try
            {
                using (Task digitalWriteTask = new Task())
                {
                    digitalWriteTask.DOChannels.CreateChannel(
                        portsOutput[portNumber],
                        "port" + portNumber.ToString(),
                        ChannelLineGrouping.OneChannelForAllLines);

                    DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalWriteTask.Stream);
                    writer.WriteSingleSamplePort(true, (UInt32)outValue);

                }


            }
            catch (DaqException ex)
            {
                MessageBox.Show(ex.Message);
                return -1;
            }

            return 0;

        }

        #endregion
    }
}
