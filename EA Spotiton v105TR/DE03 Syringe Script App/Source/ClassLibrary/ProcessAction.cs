using System;


namespace EA.PixyControl.ClassLibrary
{
    // custom attribute for process action parameters
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class ProcessActionArgumentAttribute : Attribute
    {
        private System.Type variableType;
        private bool isRequired;
        private string description;

        public System.Type TargetVariableType
        {
            get { return variableType; }
        }

        public bool IsRequired
        {
            get { return isRequired; }
        }

        public string Description
        {
            get { return description; }
        }

        public ProcessActionArgumentAttribute(System.Type TargetVariableType, bool VariableIsRequired, string Descrip)
        {
            variableType = TargetVariableType;
            isRequired = VariableIsRequired;
            description = Descrip;
        }

        public ProcessActionArgumentAttribute(System.Type TargetVariableType, bool VariableIsRequired) : this(TargetVariableType, VariableIsRequired, "") { }

    }

    // delegate for a function that you do in response to a ProcessAction command
    public delegate int ProcessActionOp(ProcessAction Cmd);

    // abstract class for any process you can put in the command sequence
    public abstract class ProcessAction
    {
        public const int IMG_ASPIRATE = 0;                  // Add new commands (type of command)
        public const int IMG_DISPENSE = 1;                  // not sure what this is used for.
        public const int IMG_INSPECT = 2;
        public const int IMG_MOVE = 3;
        public const int IMG_PAUSE = 4;
        public const int IMG_PRIME = 5;
        public const int IMG_READ = 6;
        public const int IMG_WASH = 7;
        public const int IMG_LOOPEND = 8;
        public const int IMG_LOOPSTART = 9;
        public const int IMG_IO = 10;
        public const int IMG_TIMESTAMP = 11;

        #region members

        private string name;
        private string description;
        private bool enabled;
        private int imageIndex;
        private bool canBeDisabled;
        private string nameInCommandFile;

        #endregion

        #region properties

        public string Name						// what we call this process
        {
            get { return name; }
            set { name = value; }
        }

        public string NameInCommandFile
        {
            get { return nameInCommandFile; }
        }

        public string Description 				// some explanatory text to display
        {
            get { return description; }
            set { description = value; }
        }

        [ProcessActionArgument(typeof(bool), false)]
        public bool Enabled 					// do this step?
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public int ImageIndex
        {
            get { return imageIndex; }
            set { imageIndex = value; }
        }

        public bool CanBeDisabled
        {
            get { return canBeDisabled; }
        }

        #endregion

        public abstract void GetFromFileText(string FileText);
        public abstract string[] WriteToFileText();
        public abstract void Clear();
        public abstract bool ParametersOK(VariableManager VM, out string ErrorMsg);

        #region constructors

        public ProcessAction() : this("", "", 0, true, "") { }

        public ProcessAction(string ProcessName, string ProcessDescription, int ImgIndex, bool CanBeDisabled, string CmdFileName)
        {
            name = ProcessName;
            description = ProcessDescription;
            enabled = true;
            imageIndex = ImgIndex;
            canBeDisabled = CanBeDisabled;
            nameInCommandFile = CmdFileName;
        }

        #endregion
    }

}