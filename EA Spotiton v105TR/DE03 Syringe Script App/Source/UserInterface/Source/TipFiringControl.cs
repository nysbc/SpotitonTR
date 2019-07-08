using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using Aurigin;


namespace EA.PixyControl
{
    public class UserFiringControl
    {
        private short comPort;

        #region public methods

        public UserFiringControl(short ComPort)
        {
            comPort = ComPort;
        }


        public int InitTipControl()
        {
            Console.WriteLine("\nInitializing the DE03");
            string serialPortName = string.Format("COM{0}", comPort);
            Console.WriteLine("    Serial Port: {0}", serialPortName);
            // first the com port
            if (DE03.InitTipControl(comPort) != 0) return 1;
            Console.WriteLine("    DE03 Found");

            if (DE03.InitializeBoard(1) != 0) return 1;

            Console.WriteLine("    Initialize Successful");

            // Initialize the dispense
            // TODO Turn on high voltage, etc...

            //			initialized = true;

            return 0;
        }
    }

        public class UserFiringControl2                     //2019-01-15
        {
            private short comPort;

            #region public methods

            public UserFiringControl2(short ComPort)
            {
                comPort = ComPort;
            }


            public int InitTipControl()
            {
                Console.WriteLine("\nInitializing the SECOND DE03");
                string serialPortName = string.Format("COM{0}", comPort);
                Console.WriteLine("    Serial Port: {0}", serialPortName);
                // first the com port
                if (DE03.InitTipControl(comPort) != 0) return 1;
                Console.WriteLine("    OMG....SECOND DE03 Found !!!!");

                if (DE03.InitializeBoard(2, DE03.useSecondDE03) != 0) return 1;

                Console.WriteLine("    OMG  SECOND DE03 Initialize Successful");

                return 0;
            }

            #endregion
        }

        #endregion
    }