using System;


namespace EA.PixyControl.ClassLibrary
{
    public class User_GetBoolean : ProcessAction
    {
        private string variableName;
        private string prompt;
        private string valueIfUserCancels;

        [ProcessActionArgument(typeof(string), true)]
        public string VariableName
        {
            get { return variableName; }
            set { variableName = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string Prompt
        {
            get { return prompt; }
            set { prompt = value; }
        }

        [ProcessActionArgument(typeof(bool), false)]
        public string ValueIfUserCancels
        {
            get { return valueIfUserCancels; }
            set { valueIfUserCancels = value; }
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
            variableName = "";
            prompt = "";
            valueIfUserCancels = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public User_GetBoolean() : base("Get Boolean From User", "Get true or false value from user", 0, true, SequenceFile.CommandNames.GetBooleanFromUser) { Clear(); }
    }

    public class User_GetInteger : ProcessAction
    {
        private string variableName;
        private string prompt;
        private string minValue;
        private string maxValue;
        private string valueIfUserCancels;
        private string defaultValue;

        [ProcessActionArgument(typeof(string), true)]
        public string VariableName
        {
            get { return variableName; }
            set { variableName = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string Prompt
        {
            get { return prompt; }
            set { prompt = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string MinValue
        {
            get { return minValue; }
            set { minValue = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string MaxValue
        {
            get { return maxValue; }
            set { maxValue = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string ValueIfUserCancels
        {
            get { return valueIfUserCancels; }
            set { valueIfUserCancels = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string DefaultValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
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
            variableName = "";
            prompt = "";
            minValue = "";
            maxValue = "";
            valueIfUserCancels = "";
            defaultValue = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public User_GetInteger() : base("Get Integer From User", "Get integer value from user", 0, true, SequenceFile.CommandNames.GetIntegerFromUser) { Clear(); }
    }

    public class User_GetDouble : ProcessAction
    {
        private string variableName;
        private string prompt;
        private string minValue;
        private string maxValue;
        private string valueIfUserCancels;

        [ProcessActionArgument(typeof(string), true)]
        public string VariableName
        {
            get { return variableName; }
            set { variableName = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string Prompt
        {
            get { return prompt; }
            set { prompt = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string MinValue
        {
            get { return minValue; }
            set { minValue = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string MaxValue
        {
            get { return maxValue; }
            set { maxValue = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string ValueIfUserCancels
        {
            get { return valueIfUserCancels; }
            set { valueIfUserCancels = value; }
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
            variableName = "";
            prompt = "";
            minValue = "";
            maxValue = "";
            valueIfUserCancels = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public User_GetDouble() : base("Get Double From User", "Get double value from user", 0, true, SequenceFile.CommandNames.GetDoubleFromUser) { Clear(); }


    }

}