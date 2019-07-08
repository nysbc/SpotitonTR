using System;
using System.Text;

namespace EA.PixyControl.ClassLibrary
{
    public class Variable_Define : ProcessAction
    {
        #region members

        private string variableName;
        private string variableType;
        private string variableValue;

        [ProcessActionArgument(typeof(string), true)]
        public string VariableName
        {
            get { return variableName; }
            set { variableName = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string VariableType
        {
            get { return variableType; }
            set { variableType = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string VariableValue
        {
            get { return variableValue; }
            set { variableValue = value; }
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
            variableName = "";
            variableType = "";
            variableValue = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            if (SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg) == false) return false;

            if (VM.VariableDefined(VariableName))
            {
                ErrorMsg = "Variable '" + VariableName + " ' is already defined";
                return false;
            }

            try
            {
                // try setting it
                switch (this.VariableType.ToUpper())
                {
                    case ("INT"):
                        VM.DefineVariable(this.VariableName, VM.GetIntFromText(this.VariableValue));
                        break;
                    case ("BOOL"):
                        VM.DefineVariable(this.VariableName, VM.GetBooleanFromText(this.VariableValue));
                        break;
                    case ("DOUBLE"):
                        VM.DefineVariable(this.VariableName, VM.GetDoubleFromText(this.VariableValue));
                        break;
                    default:
                        throw new Exception("Unsupported variable type in " + this.Name + " command - must be int, bool, or double.");
                }

                // then remove it
                VM.RemoveVariable(this.VariableName);
            }
            catch (Exception Ex)
            {
                ErrorMsg = Ex.Message;
                return false;
            }

            return true;
        }

        public Variable_Define() : base("Define Variable", "Declare a new variable", 0, true, SequenceFile.CommandNames.DefineVariable) { Clear(); }

    }

    public class Variable_DefinePoint : ProcessAction
    {
        #region members

        private string variableName;
        private string x;
        private string y;
        private string z;

        [ProcessActionArgument(typeof(string), true)]
        public string VariableName
        {
            get { return variableName; }
            set { variableName = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string X
        {
            get { return x; }
            set { x = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string Y
        {
            get { return y; }
            set { y = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string Z
        {
            get { return z; }
            set { z = value; }
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
            variableName = "";
            x = "";
            y = "";
            z = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            if (SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg) == false) return false;

            if (VM.VariableDefined(VariableName))
            {
                ErrorMsg = "Variable '" + VariableName + " ' is already defined";
                return false;
            }

            try
            {
                // try defining it
                MachineCoordinate Pt = new MachineCoordinate();
                Pt.Name = this.VariableName;
                Pt.X = VM.GetDoubleFromText(this.X);
                Pt.Y = VM.GetDoubleFromText(this.Y);
                Pt.Z = VM.GetDoubleFromText(this.Z);

                VM.DefineVariable(this.VariableName, Pt);

                // then remove it
                VM.RemoveVariable(this.VariableName);

            }
            catch (Exception Ex)
            {
                ErrorMsg = Ex.Message;
                return false;
            }

            return true;
        }

        public Variable_DefinePoint() : base("Define Point", "Declare a new machine point variable", 0, true, SequenceFile.CommandNames.DefinePoint) { Clear(); }

    }

    public class Variable_DefineGrid : ProcessAction
    {
        #region members

        private string variableName;
        private string x;
        private string y;
        private string z;
        private string xSteps;
        private string xIncrement;
        private string ySteps;
        private string yIncrement;

        [ProcessActionArgument(typeof(string), true)]
        public string VariableName
        {
            get { return variableName; }
            set { variableName = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string X
        {
            get { return x; }
            set { x = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string Y
        {
            get { return y; }
            set { y = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string Z
        {
            get { return z; }
            set { z = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string XSteps
        {
            get { return xSteps; }
            set { xSteps = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string XIncrement
        {
            get { return xIncrement; }
            set { xIncrement = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string YSteps
        {
            get { return ySteps; }
            set { ySteps = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string YIncrement
        {
            get { return yIncrement; }
            set { yIncrement = value; }
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
            variableName = "";
            x = "";
            y = "";
            z = "";
            xSteps = "";
            xIncrement = "";
            ySteps = "";
            yIncrement = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            if (SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg) == false) return false;

            if (VM.VariableDefined(VariableName))
            {
                ErrorMsg = "Variable '" + VariableName + " ' is already defined";
                return false;
            }

            try
            {
                // try to add it
                CoordinateMatrix Grd = new CoordinateMatrix();

                Grd.Name = this.VariableName;
                Grd.X = VM.GetDoubleFromText(this.X);
                Grd.Y = VM.GetDoubleFromText(this.Y);
                Grd.Z = VM.GetDoubleFromText(this.Z);
                Grd.XSteps = VM.GetIntFromText(this.XSteps);
                Grd.YSteps = VM.GetIntFromText(this.YSteps);
                Grd.XStepSize = VM.GetDoubleFromText(this.XIncrement);
                Grd.YStepSize = VM.GetDoubleFromText(this.YIncrement);

                VM.DefineVariable(this.VariableName, Grd);

                // then remove it
                VM.RemoveVariable(this.VariableName);
            }
            catch (Exception Ex)
            {
                ErrorMsg = Ex.Message;
                return false;
            }

            return true;
        }

        public Variable_DefineGrid() : base("Define Grid", "Declare a new array of machine points", 0, true, SequenceFile.CommandNames.DefineGrid) { Clear(); }

    }

    public class Variable_Set : ProcessAction
    {
        private string variableName;
        private string variableValue;

        [ProcessActionArgument(typeof(string), true)]
        public string VariableName
        {
            get { return variableName; }
            set { variableName = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string VariableValue
        {
            get { return variableValue; }
            set { variableValue = value; }
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
            variableValue = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            if (SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg) == false) return false;

            if (VM.VariableDefined(this.VariableName) == false)
            {
                ErrorMsg = "Variable '" + this.VariableName + "' is not defined";
                return false;
            }

            System.Type T = VM.VariableType(this.VariableName);

            if (T == typeof(double))
            {
                try
                {
                    double d = VM.GetDoubleFromText(this.VariableValue);
                }
                catch (Exception Ex)
                {
                    ErrorMsg = "Value '" + VariableValue + "' cannot be converted to a double\n" + Ex.Message;
                    return false;
                }
            }
            else if (T == typeof(int))
            {
                try
                {
                    int i = VM.GetIntFromText(this.VariableValue);
                }
                catch (Exception Ex)
                {
                    ErrorMsg = "Value '" + VariableValue + "' cannot be converted to an integer\n" + Ex.Message;
                    return false;
                }
            }
            else if (T == typeof(bool))
            {
                try
                {
                    bool b = VM.GetBooleanFromText(this.VariableValue);
                }
                catch (Exception Ex)
                {
                    ErrorMsg = "Value '" + VariableValue + "' cannot be converted to a boolean\n" + Ex.Message;
                    return false;
                }
            }
            else
            {
                ErrorMsg = "Can only set value for integer, double, and boolean variables with Set Variable command";
                return false;
            }

            return true;
        }

        public Variable_Set() : base("Variable Set", "Assign a new value to a variable", 0, true, SequenceFile.CommandNames.SetVariable) { Clear(); }

    }

    public class Variable_MathOperation : ProcessAction
    {
        private string resultVariable;
        private string lhs;
        private string mathOperator;
        private string rhs;

        [ProcessActionArgument(typeof(string), true)]
        public string ResultVariable
        {
            get { return resultVariable; }
            set { resultVariable = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string LHS
        {
            get { return lhs; }
            set { lhs = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string MathOperator
        {
            get { return mathOperator; }
            set { mathOperator = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string RHS
        {
            get { return rhs; }
            set { rhs = value; }
        }

        public override void GetFromFileText(string FileText)
        {
            int OpenParen = FileText.IndexOf("(");
            int CloseParen = FileText.LastIndexOf(")");
            int EqualPos = FileText.IndexOf("=", OpenParen + 1);
            int OperatorPos = 0;
            string TestString = null;

            // Variable_Math(Z = x + 1)

            if (OpenParen < 0) throw new Exception("Error in " + this.Name + " command text - no opening parentheses.\n\n" + FileText);
            if (CloseParen != FileText.Length - 1) throw new Exception("Error in " + this.Name + "command text - unexpected text after closing parentheses.\n\n" + FileText);
            if (EqualPos < 0) throw new Exception("Error in " + this.Name + " command text - no equal sign found inside parentheses.\n\n" + FileText);

            this.resultVariable = FileText.Substring(OpenParen + 1, EqualPos - OpenParen - 1).Trim();

            OperatorPos = SequenceFile.FindFirstMathOperator(FileText, EqualPos + 1, CloseParen - 1, ref  this.mathOperator);
            if (OperatorPos < 0) throw new Exception("Error in " + this.Name + " command text - no math operator found inside parentheses.\n\n" + FileText);

            int TestOperatorPos = SequenceFile.FindFirstMathOperator(FileText, OperatorPos + 1, CloseParen - 1, ref TestString);
            if (TestOperatorPos > 0) throw new Exception("Error in " + this.Name + "command text - only one math operator allowed inside parentheses.\n\n" + FileText);

            this.lhs = FileText.Substring(EqualPos + 1, OperatorPos - EqualPos - 1).Trim();
            this.rhs = FileText.Substring(OperatorPos + this.mathOperator.Length, CloseParen - OperatorPos - this.mathOperator.Length).Trim();
        }

        public override string[] WriteToFileText()
        {
            //  Variable_Math(Z = x + 1)
            StringBuilder sb = new StringBuilder();

            sb.Append(this.NameInCommandFile);
            sb.Append("(");
            sb.Append(this.ResultVariable);
            sb.Append(" = ");
            sb.Append(this.LHS);
            sb.Append(" ");
            sb.Append(this.MathOperator);
            sb.Append(" ");
            sb.Append(this.RHS);
            sb.Append(")");

            string[] ret = new string[1];
            ret[0] = sb.ToString();

            return ret;
        }

        public override void Clear()
        {
            resultVariable = "";
            lhs = "";
            mathOperator = "";
            rhs = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            if (SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg) == false) return false;

            try
            {
                if (VM.VariableDefined(this.ResultVariable) == false)
                {
                    throw new Exception("Result variable '" + this.ResultVariable + "' is not defined");
                }

                // result is a user variable, get the value so we can set it back
                System.Type T = VM.VariableType(this.ResultVariable);
                object result = this.ResultVariable;

                if (T == typeof(int))
                {
                    // get the initial value
                    int i = VM.GetIntFromText(this.ResultVariable);

                    // try the operation
                    VM.MathOperation(this.LHS, this.RHS, ref result, VariableManager.MathOperatorFromText(this.MathOperator));

                    // change it back
                    VM.SetVariable(this.resultVariable, i);
                }
                else if (T == typeof(double))
                {
                    // get the initial value
                    double d = VM.GetDoubleFromText(this.ResultVariable);

                    // try the operation
                    VM.MathOperation(this.LHS, this.RHS, ref result, VariableManager.MathOperatorFromText(this.MathOperator));

                    // change it back
                    VM.SetVariable(this.resultVariable, d);
                }
                else if (T == typeof(bool))
                {
                    // get the initial value
                    bool b = VM.GetBooleanFromText(this.ResultVariable);

                    // try the operation
                    VM.MathOperation(this.LHS, this.RHS, ref result, VariableManager.MathOperatorFromText(this.MathOperator));

                    // change it back
                    VM.SetVariable(this.resultVariable, b);
                }
                else
                {
                    throw new Exception("Result variable must be an integer, double, or boolean for math operations");
                }
            }
            catch (Exception Ex)
            {
                ErrorMsg = Ex.Message;
                return false;
            }

            return true;
        }

        public Variable_MathOperation() : base("Variable_Math", "Perform math operation on variables", 0, true, SequenceFile.CommandNames.VariableMath) { Clear(); }

    }

    public class Variable_CompareOperation : ProcessAction
    {
        private string resultVariable;
        private string lhs;
        private string compareOperator;
        private string rhs;

        [ProcessActionArgument(typeof(string), true)]
        public string ResultVariable
        {
            get { return resultVariable; }
            set { resultVariable = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string LHS
        {
            get { return lhs; }
            set { lhs = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string CompareOperator
        {
            get { return compareOperator; }
            set { compareOperator = value; }
        }

        [ProcessActionArgument(typeof(string), true)]
        public string RHS
        {
            get { return rhs; }
            set { rhs = value; }
        }

        public override void GetFromFileText(string FileText)
        {
            int OpenParen = FileText.IndexOf("(");
            int CloseParen = FileText.LastIndexOf(")");
            int EqualPos = FileText.IndexOf("=", OpenParen + 1);
            int OperatorPos = 0;

            // Variable_Compare(IsMore = x >= 1)

            if (OpenParen < 0) throw new Exception("Error in " + this.Name + " command text - no opening parentheses.\n\n" + FileText);
            if (CloseParen != FileText.Length - 1) throw new Exception("Error in " + this.Name + "command text - unexpected text after closing parentheses.\n\n" + FileText);
            if (EqualPos < 0) throw new Exception("Error in " + this.Name + "command text - no equal sign found inside parentheses.\n\n" + FileText);

            this.resultVariable = FileText.Substring(OpenParen + 1, EqualPos - OpenParen - 1).Trim();

            OperatorPos = SequenceFile.FindFirstComparisonOperator(FileText, EqualPos + 1, CloseParen - 1, ref this.compareOperator);
            if (OperatorPos < 0) throw new Exception("Error in " + this.Name + "command text - no comparison operator found inside parentheses.\n\n" + FileText);

            this.lhs = FileText.Substring(EqualPos + 1, OperatorPos - EqualPos - 1).Trim();
            this.rhs = FileText.Substring(OperatorPos + this.CompareOperator.Length, CloseParen - OperatorPos - this.CompareOperator.Length).Trim();
        }

        public override string[] WriteToFileText()
        {
            // Variable_Compare(IsMore = x >= 1)
            StringBuilder sb = new StringBuilder();

            sb.Append(this.NameInCommandFile);
            sb.Append("(");
            sb.Append(this.ResultVariable);
            sb.Append(" = ");
            sb.Append(this.LHS);
            sb.Append(" ");
            sb.Append(this.CompareOperator);
            sb.Append(" ");
            sb.Append(this.RHS);
            sb.Append(")");

            string[] ret = new string[1];
            ret[0] = sb.ToString();

            return ret;
        }

        public override void Clear()
        {
            resultVariable = "";
            lhs = "";
            compareOperator = "";
            rhs = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            if (SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg) == false) return false;

            try
            {
                if (VM.VariableDefined(this.ResultVariable) == false)
                {
                    throw new Exception("Result variable '" + this.ResultVariable + "' is not defined");
                }

                // result is a user variable, get the value so we can set it back
                System.Type T = VM.VariableType(this.ResultVariable);
                object result = this.ResultVariable;

                if (T == typeof(bool))
                {
                    // get the initial value
                    bool b = VM.GetBooleanFromText(this.ResultVariable);

                    // try the operation
                    VM.ComparisonOperation(this.LHS, this.RHS, ref result, VariableManager.ComparisonOperatorFromText(this.CompareOperator));

                    // change it back
                    VM.SetVariable(this.resultVariable, b);
                }
                else
                {
                    throw new Exception("Result variable must be a boolean for compare operations");
                }
            }
            catch (Exception Ex)
            {
                ErrorMsg = Ex.Message;
                return false;
            }

            return true;
        }

        public Variable_CompareOperation() : base("Variable Compare", "Perform comparison operation on variables", 0, true, SequenceFile.CommandNames.VariableCompare) { Clear(); }

    }

    public class Variable_DefineImport : ProcessAction        // [LEA COMMAND DEF]
    {
        private string fileName;
        private bool overrideFlag;

        [ProcessActionArgument(typeof(string), true,"A valid file name may include underscore and space")]
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        //[ProcessActionArgument(typeof(bool), false,"When TRUE, it will override previous definition of variables.")]
        //public bool OverrideFlag
        //{
        //    get { return overrideFlag; }
        //    set { overrideFlag = value; }
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
            fileName = "";
            //overrideFlag = false;
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
            
        }

        public Variable_DefineImport()
            : base( "Define Import", "Import variables from a file.", 
                    0, false, 
                    SequenceFile.CommandNames.DefineImport) 
        { 
            Clear(); 
        }
    }
}