using System;


namespace EA.PixyControl.ClassLibrary
{
    // Revision R2.03
    public class User_GetIntegersFromFile : ProcessAction
    {
        private string filePath;
        private string int1;
        private string int2;
        private string int3;
        private string int4;
        private string int5;
        private string int6;
 
        [ProcessActionArgument(typeof(string), true,"Full Path Name of File")]
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string Int1
        {
            get { return int1; }
            set { int1 = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string Int2
        {
            get { return int2; }
            set { int2 = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string Int3
        {
            get { return int3; }
            set { int3 = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string Int4
        {
            get { return int4; }
            set { int4 = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string Int5
        {
            get { return int5; }
            set { int5 = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string Int6
        {
            get { return int6; }
            set { int6 = value; }
        }

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
            int1 = "";int2 = "";int3 = "";int4 = "";int5 = "";int6 = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public User_GetIntegersFromFile() : base("Get Integers From File", "Get integer values from file", 0, true, SequenceFile.CommandNames.GetIntegersFromFile) { Clear(); }
    }

    // Revision R2.03
    public class User_PutIntegersToFile : ProcessAction
    {
        private string filePath;
        private string int1;
        private string int2;
        private string int3;
        private string int4;
        private string int5;
        private string int6;

        [ProcessActionArgument(typeof(string), true, "Full Path Name of File")]
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string Int1
        {
            get { return int1; }
            set { int1 = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string Int2
        {
            get { return int2; }
            set { int2 = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string Int3
        {
            get { return int3; }
            set { int3 = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string Int4
        {
            get { return int4; }
            set { int4 = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string Int5
        {
            get { return int5; }
            set { int5 = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string Int6
        {
            get { return int6; }
            set { int6 = value; }
        }

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
            int1 = ""; int2 = ""; int3 = ""; int4 = ""; int5 = ""; int6 = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public User_PutIntegersToFile() : base("Put Integers To File", "Put integer values to file", 0, true, SequenceFile.CommandNames.PutIntegersToFile) { Clear(); }
    }

    // Revision R2.03
    public class User_GetStepFromFile : ProcessAction
    {
        private string filePath;
        private string stepNo;
        private string code;
        private string row;
        private string col;
        private string vol1_ul;
        private string vol1_syringe_speed;
        private string vol1_delay;
        private string vol2_ul;
        private string vol2_syringe_speed;
        private string vol2_delay;


        [ProcessActionArgument(typeof(string), true, "Full Path Name of File")]
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        [ProcessActionArgument(typeof(string), true, "Step Number You Wish To Retreive from the file")]
        public string StepNo
        {
            get { return stepNo; }
            set { stepNo = value; }
        }

        [ProcessActionArgument(typeof(int), true,"Will return -1 when step number not found")]
        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        [ProcessActionArgument(typeof(int), true,"Returns 0 Based Row Number,  Row number in file - 1")]
        public string Row
        {
            get { return row; }
            set { row = value; }
        }

        [ProcessActionArgument(typeof(int), true,"Returns 0 Based Col Number,  Col number in file - 1")]
        public string Col
        {
            get { return col; }
            set { col = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string Vol1_ul
        {
            get { return vol1_ul; }
            set { vol1_ul = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string Vol1_syringe_speed
        {
            get { return vol1_syringe_speed; }
            set { vol1_syringe_speed = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string Vol1_delay
        {
            get { return vol1_delay; }
            set { vol1_delay = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string Vol2_ul
        {
            get { return vol2_ul; }
            set { vol2_ul = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string Vol2_syringe_speed
        {
            get { return vol2_syringe_speed; }
            set { vol2_syringe_speed = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string Vol2_delay
        {
            get { return vol2_delay; }
            set { vol2_delay = value; }
        }

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
            code = ""; stepNo = ""; row = ""; col = "";
            vol1_ul="";vol1_syringe_speed="";vol1_delay="";
            vol2_ul="";vol2_syringe_speed="";vol2_delay="";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public User_GetStepFromFile() : base("Get Step From File", "Get step parameters from a file", 0, true, SequenceFile.CommandNames.GetStepFromFile) { Clear(); }
    }
}