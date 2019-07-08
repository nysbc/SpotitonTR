using System;


namespace EA.PixyControl.ClassLibrary
{
 
    public class Process_IOSetOutput : ProcessAction
    {
        #region members

        private string iONumber;      //       Output number  1 to 24
  
        [ProcessActionArgument(typeof(int), true, "1 to 24,  Negative turns off")]
        public string IONumber
        {
            get { return iONumber; }
            set { iONumber = value; }
        }

        #endregion members

        public override void GetFromFileText(string FileText)
        {
            SequenceFile.GetProcessActionFromFileText((ProcessAction)this, FileText);
        }

        public override string[] WriteToFileText()
        {
            return SequenceFile.GetFileTextFromProcessAction((ProcessAction)this);
        }

        public override void Clear()
        {
            iONumber = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_IOSetOutput() : base("Set IO", "Sets or clears and IO Point", ProcessAction.IMG_IO, true, SequenceFile.CommandNames.IOSetOutput) { Clear(); }

    }

    public class Process_IOWaitInput : ProcessAction
    {
        #region members

        private string iONumber;      //       Input number  1 to 24
        private string timeOut_ms;      //      msec to wait for the state to be acheived

        [ProcessActionArgument(typeof(int), true, "1 to 24,  Negative means on (active low)")]
        public string IONumber
        {
            get { return iONumber; }
            set { iONumber = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string TimeOut_ms
        {
            get { return timeOut_ms; }
            set { timeOut_ms = value; }
        }

        #endregion members

        public override void GetFromFileText(string FileText)
        {
            SequenceFile.GetProcessActionFromFileText((ProcessAction)this, FileText);
        }

        public override string[] WriteToFileText()
        {
            return SequenceFile.GetFileTextFromProcessAction((ProcessAction)this);
        }

        public override void Clear()
        {
            iONumber = "";
            timeOut_ms = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_IOWaitInput() : base("Wait IO input", "Waits for input to turn to a certain state", ProcessAction.IMG_IO, true, SequenceFile.CommandNames.IOWaitInput) { Clear(); }

    }

}