using System;
using System.Text;

namespace EA.PixyControl.ClassLibrary
{
    public class Process_ExitSequence : ProcessAction
    {
        public override void GetFromFileText(string FileText)
        {
            if (FileText.Length != this.NameInCommandFile.Length) throw new Exception("Error in " + this.Name + " command - unexpected text after " + this.NameInCommandFile);
        }

        public override string[] WriteToFileText()
        {
            string[] ret = new string[1];

            ret[0] = this.NameInCommandFile;

            return ret;
        }

        public override void Clear() { }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_ExitSequence() : base("Exit Sequence", "End of For loop", 0, true, SequenceFile.CommandNames.ExitSequence) { Clear(); }
    }

    public class Process_Pause : ProcessAction
    {
        #region members

        private string userMessage;		// what to tell the user
        private string pauseTime_ms;		// set to zero to wait for user to click OK

        [ProcessActionArgument(typeof(string), true,"Message to display in dialog box or use 'none' to not pop up dialog box")]
        public string UserMessage
        {
            get { return userMessage; }
            set { userMessage = value; }
        }

        [ProcessActionArgument(typeof(int), true,"Pause time in ms  or Set to 0 to wait for user input")]
        public string PauseTime_ms
        {
            get { return pauseTime_ms; }
            set { pauseTime_ms = value; }
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
            userMessage = "";
            pauseTime_ms = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_Pause() : base("Pause", "Pause sequence execution", ProcessAction.IMG_PAUSE, true, SequenceFile.CommandNames.Pause) { Clear(); }

    }

    public class Process_TimeStamp : ProcessAction
    {
        #region members

        private string userMessage;		    // what will appear in log file with timestamp

        [ProcessActionArgument(typeof(string), true)]
        public string UserMessage
        {
            get { return userMessage; }
            set { userMessage = value; }
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
            userMessage = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_TimeStamp() : base("TimeStamp", "Log Time Stamp", ProcessAction.IMG_TIMESTAMP, true, SequenceFile.CommandNames.TimeStamp) { Clear(); }

    }

    public class Loop_ForStart : ProcessAction
    {
        #region members

        private string variableName;
        private string startValue;
        private string endValue;
        private string increment;

        [ProcessActionArgument(typeof(string), true)]
        public string VariableName
        {
            get { return variableName; }
            set { variableName = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string StartValue
        {
            get { return startValue; }
            set { startValue = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string EndValue
        {
            get { return endValue; }
            set { endValue = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string Increment
        {
            get { return increment; }
            set { increment = value; }
        }

        #endregion members


        public override void GetFromFileText(string FileText)
        {
            // For(i = 1 To 3 Step 2)
            int OpenParen = FileText.IndexOf("(");
            if (OpenParen < 0) throw new Exception("Error in " + this.Name + " command text - no opening parentheses.\n\n" + FileText);

            int CloseParen = FileText.LastIndexOf(")");
            if (CloseParen != FileText.Length - 1) throw new Exception("Error in " + this.Name + "command text - unexpected text after closing parentheses.\n\n" + FileText);

            int EqualPos = FileText.IndexOf("=", OpenParen + 1);
            if (EqualPos < 0) throw new Exception("Error in " + this.Name + "command text - no equal sign found inside parentheses.\n\n" + FileText);

            int ToPos = FileText.IndexOf("TO", EqualPos + 1);
            if (EqualPos < 0) throw new Exception("Error in " + this.Name + "command text - no 'To' found inside parentheses.\n\n" + FileText);

            int StepPos = FileText.IndexOf("STEP", ToPos + 1);
            // Step is optional

            this.VariableName = FileText.Substring(OpenParen + 1, EqualPos - OpenParen - 1).Trim();
            this.StartValue = FileText.Substring(EqualPos + 1, ToPos - EqualPos - 1).Trim();

            if (StepPos > 0)
            {
                this.EndValue = FileText.Substring(ToPos + 2, StepPos - ToPos - 2).Trim();
                this.Increment = FileText.Substring(StepPos + 4, CloseParen - StepPos - 4).Trim();
            }
            else
            {
                this.EndValue = FileText.Substring(ToPos + 2, CloseParen - ToPos - 2).Trim();
                this.Increment = "";
            }
        }

        public override string[] WriteToFileText()
        {
            // For(i = 1 To 3 Step 2)

            StringBuilder sb = new StringBuilder();

            sb.Append(this.NameInCommandFile);
            sb.Append("(");
            sb.Append(this.VariableName);
            sb.Append(" = ");
            sb.Append(this.StartValue);
            sb.Append(" To ");
            sb.Append(this.EndValue);

            if (this.Increment != "")
            {
                sb.Append(" Step ");
                sb.Append(this.Increment);
            }
            sb.Append(")");

            string[] ret = new string[1];
            ret[0] = sb.ToString();

            return ret;
        }

        public override void Clear()
        {
            variableName = "";
            startValue = "";
            endValue = "";
            increment = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            if (SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg) == false) return false;

            try
            {
                // is the index variable defined
                if (VM.VariableDefined(this.VariableName) == false) throw new Exception("Variable '" + this.VariableName + "' is not defined");

                // is start value an int
                int i = VM.GetIntFromText(this.StartValue);

                // is end value an int
                i = VM.GetIntFromText(this.EndValue);

                // if we have a step, is it an int
                if (this.Increment != "")
                {
                    i = VM.GetIntFromText(this.Increment);
                    if (i == 0) throw new Exception("For loop Step value cannot be zero");
                }
            }
            catch (Exception Ex)
            {
                ErrorMsg = Ex.Message;
                return false;
            }

            return true;
        }

        public Loop_ForStart() : base("For", "Start For loop", 0, true, SequenceFile.CommandNames.ForStart) { Clear(); }

    }

    public class Loop_ForEnd : ProcessAction
    {

        public override void GetFromFileText(string FileText)
        {
            if (FileText.Length != this.NameInCommandFile.Length) throw new Exception("Error in " + this.Name + " command - unexpected text after " + this.NameInCommandFile);
        }

        public override string[] WriteToFileText()
        {
            string[] ret = new string[1];

            ret[0] = this.NameInCommandFile;

            return ret;
        }

        public override void Clear() { }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Loop_ForEnd() : base("Next", "End of For loop", 0, true, SequenceFile.CommandNames.ForEnd) { Clear(); }
    }

    public class Loop_WhileStart : ProcessAction
    {
        private string testVariableName;
        private string comparisonOperator;
        private string testValue;

        [ProcessActionArgument(typeof(string), true)]
        public string TestVariableName
        {
            get { return testVariableName; }
            set { testVariableName = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string ComparisonOperator
        {
            get { return comparisonOperator; }
            set { comparisonOperator = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string TestValue
        {
            get { return testValue; }
            set { testValue = value; }
        }


        public override void GetFromFileText(string FileText)
        {
            // while (Quit <> true)
            int OpenParen = FileText.IndexOf("(");
            if (OpenParen < 0) throw new Exception("Error in " + this.Name + " command text - no opening parentheses.\n\n" + FileText);

            int CloseParen = FileText.LastIndexOf(")");
            if (CloseParen != FileText.Length - 1) throw new Exception("Error in " + this.Name + "command text - unexpected text after closing parentheses.\n\n" + FileText);

            int OperatorPos = SequenceFile.FindFirstComparisonOperator(FileText, OpenParen + 1, CloseParen - 1, ref this.comparisonOperator);
            if (OperatorPos < 0) throw new Exception("Error in " + this.Name + " command text - no comparison operator inside parentheses.\n\n" + FileText);

            this.TestVariableName = FileText.Substring(OpenParen + 1, OperatorPos - OpenParen - 1).Trim();
            this.TestValue = FileText.Substring(OperatorPos + this.ComparisonOperator.Length, CloseParen - OperatorPos - this.ComparisonOperator.Length).Trim();
        }

        public override string[] WriteToFileText()
        {
            // while (Quit <> true)
            StringBuilder sb = new StringBuilder();

            sb.Append(this.NameInCommandFile);
            sb.Append("(");
            sb.Append(this.TestVariableName);
            sb.Append(" ");
            sb.Append(this.ComparisonOperator);
            sb.Append(" ");
            sb.Append(this.TestValue);
            sb.Append(")");

            string[] ret = new string[1];
            ret[0] = sb.ToString();

            return ret;
        }

        public override void Clear()
        {
            testVariableName = "";
            comparisonOperator = "";
            testValue = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            if (SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg) == false) return false;

            try
            {
                if (VM.VariableDefined(this.TestVariableName) == false) throw new Exception("Variable '" + this.TestVariableName + "' is not defined");

                object result = new bool();

                VM.ComparisonOperation(this.TestVariableName, this.TestValue, ref result, VariableManager.ComparisonOperatorFromText(this.ComparisonOperator));

            }
            catch (Exception Ex)
            {
                ErrorMsg = Ex.Message;
                return false;
            }

            return true;
        }

        public Loop_WhileStart() : base("While", "Start While loop", 0, true, SequenceFile.CommandNames.WhileStart) { Clear(); }
    }

    public class Loop_WhileEnd : ProcessAction
    {
        public override void GetFromFileText(string FileText)
        {
            if (FileText.Length != this.NameInCommandFile.Length) throw new Exception("Error in " + this.Name + " command - unexpected text after " + this.NameInCommandFile);
        }

        public override string[] WriteToFileText()
        {
            string[] ret = new string[1];

            ret[0] = this.NameInCommandFile;

            return ret;
        }

        public override void Clear()
        { }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Loop_WhileEnd() : base("Loop", "End of While loop", 0, true, SequenceFile.CommandNames.WhileEnd) { Clear(); }

    }

    public class Loop_ExitLoop : ProcessAction
    {
        public override void GetFromFileText(string FileText)
        {
            if (FileText.Length != this.NameInCommandFile.Length) throw new Exception("Error in " + this.Name + " command - unexpected text after " + this.NameInCommandFile);
        }

        public override string[] WriteToFileText()
        {
            string[] ret = new string[1];

            ret[0] = this.NameInCommandFile;

            return ret;
        }

        public override void Clear() { }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Loop_ExitLoop() : base("Exit Loop", "Exit the current loop", 0, true, SequenceFile.CommandNames.ExitLoop) { Clear(); }

    }

    /*
	
    public class Process_Label : ProcessAction
    {
        private string labelName;

        [ProcessActionArgument(typeof(string), true)]
        public string LabelName
        {
            get{return labelName;}
            set{labelName = value;}
        }

        public override void GetFromFileText(string FileText)
        {
            // LABEL Line #1
            if (FileText.Length <= this.NameInCommandFile.Length) throw new Exception("Error in " + this.Name + " command - no label name specified");
		
            this.LabelName = FileText.Substring(this.NameInCommandFile.Length).Trim();
        }

        public override string [] WriteToFileText()
        {
            // while (Quit <> true)
            StringBuilder sb = new StringBuilder();
			
            string [] ret = new string[1];
			
            sb.Append(this.NameInCommandFile);
            sb.Append(" ");
            sb.Append(this.LabelName);
			
            ret[0] = sb.ToString();

            return ret;
        }

        public override void Clear()
        {
            labelName="";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_Label () : base ("Label", "Reference for a point in the sequence", 0, false, SequenceFile.CommandNames.Label){Clear();}

    }

    public class Process_GotoLabel : ProcessAction
    {
        private string labelName;

        [ProcessActionArgument(typeof(string), true)]
        public string LabelName
        {
            get{return labelName;}
            set{labelName = value;}
        }

        public override void GetFromFileText(string FileText)
        {
            // Goto Line #1
            if (FileText.Length <= this.NameInCommandFile.Length) throw new Exception("Error in " + this.Name + " command - no label name specified");
		
            this.LabelName = FileText.Substring(this.NameInCommandFile.Length).Trim();
        }

        public override string [] WriteToFileText()
        {
            StringBuilder sb = new StringBuilder();
			
            sb.Append(this.NameInCommandFile);
            sb.Append(" ");
            sb.Append(this.LabelName);
			
            string [] ret = new string [1];
            ret [0] = sb.ToString();

            return ret;
        }

        public override void Clear()
        {
            labelName="";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_GotoLabel () : base ("Go To", "Go to a specified point in the sequence", 0, true, SequenceFile.CommandNames.GoTo){Clear();}

    }

    */

    public class Process_If : ProcessAction
    {
        private string testVariableName;
        private string comparisonOperator;
        private string testValue;

        [ProcessActionArgument(typeof(string), true)]
        public string TestVariableName
        {
            get { return testVariableName; }
            set { testVariableName = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string ComparisonOperator
        {
            get { return comparisonOperator; }
            set { comparisonOperator = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string TestValue
        {
            get { return testValue; }
            set { testValue = value; }
        }

        public override void GetFromFileText(string FileText)
        {
            // if (i <> 3)
            int OpenParen = FileText.IndexOf("(");
            if (OpenParen < 0) throw new Exception("Error in " + this.Name + " command text - no opening parentheses.\n\n" + FileText);

            int CloseParen = FileText.LastIndexOf(")");
            if (CloseParen != FileText.Length - 1) throw new Exception("Error in " + this.Name + "command text - unexpected text after closing parentheses.\n\n" + FileText);

            int OperatorPos = SequenceFile.FindFirstComparisonOperator(FileText, OpenParen + 1, CloseParen - 1, ref this.comparisonOperator);
            if (OperatorPos < 0) throw new Exception("Error in " + this.Name + " command text - no comparison operator inside parentheses.\n\n" + FileText);

            this.TestVariableName = FileText.Substring(OpenParen + 1, OperatorPos - OpenParen - 1).Trim();
            this.TestValue = FileText.Substring(OperatorPos + this.ComparisonOperator.Length, CloseParen - OperatorPos - this.ComparisonOperator.Length).Trim(); ;
        }

        public override string[] WriteToFileText()
        {
            // if (i <> 3)
            StringBuilder sb = new StringBuilder();


            sb.Append(this.NameInCommandFile);
            sb.Append("(");
            sb.Append(this.TestVariableName);
            sb.Append(" ");
            sb.Append(this.ComparisonOperator);
            sb.Append(" ");
            sb.Append(this.TestValue);
            sb.Append(")");

            string[] ret = new string[1];
            ret[0] = sb.ToString();

            return ret;
        }

        public override void Clear()
        {
            testVariableName = "";
            comparisonOperator = "";
            testValue = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            if (SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg) == false) return false;

            try
            {
                if (VM.VariableDefined(this.TestVariableName) == false) throw new Exception("Variable '" + this.TestVariableName + "' is not defined");

                object result = new bool();

                VM.ComparisonOperation(this.TestVariableName, this.TestValue, ref result, VariableManager.ComparisonOperatorFromText(this.ComparisonOperator));

            }
            catch (Exception Ex)
            {
                ErrorMsg = Ex.Message;
                return false;
            }

            return true;
        }

        public Process_If() : base("If", "Test the value of a variable", 0, true, SequenceFile.CommandNames.IfCmd) { Clear(); }
    }

    public class Process_Else : ProcessAction
    {
        public override void GetFromFileText(string FileText)
        {
            if (FileText.Length != this.NameInCommandFile.Length) throw new Exception("Error in " + this.Name + " command - unexpected text after " + this.NameInCommandFile);
        }

        public override string[] WriteToFileText()
        {
            string[] ret = new string[1];

            ret[0] = this.NameInCommandFile;

            return ret;
        }

        public override void Clear() { }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_Else() : base("Else", "Else", 0, true, SequenceFile.CommandNames.ElseCmd) { Clear(); }
    }

    public class Process_EndIf : ProcessAction
    {
        public override void GetFromFileText(string FileText)
        {
            if (FileText.Length != this.NameInCommandFile.Length) throw new Exception("Error in " + this.Name + " command - unexpected text after " + this.NameInCommandFile);
        }

        public override string[] WriteToFileText()
        {
            string[] ret = new string[1];
            ret[0] = this.NameInCommandFile;

            return ret;
        }

        public override void Clear() { }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_EndIf() : base("EndIf", "End of If-Else-EndIf sequence", ProcessAction.IMG_LOOPEND, true, SequenceFile.CommandNames.EndIfCmd) { Clear(); }
    }

    public class Process_CommentLine : ProcessAction
    {
        private string commentText;

        [ProcessActionArgument(typeof(string), false)]
        public string CommentText
        {
            get { return commentText; }
            set { commentText = value; }
        }

        public override void GetFromFileText(string FileText)
        {
            this.CommentText = "";

            // ' no comment at this time
            if (FileText.Length <= this.NameInCommandFile.Length) return;

            this.CommentText = FileText.Substring(this.NameInCommandFile.Length).Trim();
        }

        public override string[] WriteToFileText()
        {
            // ' no comment at this time
            StringBuilder sb = new StringBuilder();

            sb.Append(this.NameInCommandFile);
            sb.Append(" ");
            sb.Append(this.CommentText);

            string[] ret = new string[1];
            ret[0] = sb.ToString();

            return ret;
        }

        public override void Clear()
        {
            commentText = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_CommentLine() : base("Comment", "Any text", 0, true, SequenceFile.CommandNames.CommentLine) { Clear(); }
    }

    public class Process_WriteToConsole : ProcessAction
    {
        private string textToWrite;

        [ProcessActionArgument(typeof(string), false,"Text that will be written to the console window")]
        public string TextToWrite
        {
            get { return textToWrite; }
            set { textToWrite = value; }
        }

        //public override void GetFromFileText(string FileText)
        //{
        //    this.TextToWrite = "";

        //    // ' no text to write at this time
        //    if (FileText.Length <= this.NameInCommandFile.Length) return;

        //    this.TextToWrite = FileText.Substring(this.NameInCommandFile.Length).Trim();
        //}

        //public override string[] WriteToFileText()
        //{
        //    // ' no comment at this time
        //    StringBuilder sb = new StringBuilder();

        //    sb.Append(this.NameInCommandFile);
        //    sb.Append(" ");
        //    sb.Append(this.TextToWrite);

        //    string[] ret = new string[1];
        //    ret[0] = sb.ToString();

        //    return ret;
        //}

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
            TextToWrite = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_WriteToConsole() : base("WriteToConsole", "Write to console", 0, true, SequenceFile.CommandNames.WriteToConsole) { Clear(); }

   
    }
}