using System;
using System.Collections;
using System.Data;


namespace EA.PixyControl.ClassLibrary
{
    public class VariableManager
    {
        public enum MathOperator
        {
            Add,
            Subtract,
            Multiply,
            Divide
        }

        public enum ComparisonOperator
        {
            EqualTo,
            NotEqualTo,
            GreaterThan,
            GreaterThanOrEqualTo,
            LessThan,
            LessThanOrEqualTo
        }

        public enum TextType
        {
            UnknownString,
            IntValue,
            DblValue,
            BlnValue,
            VariableName,
            SingleCoordinateInPoint,
            PointInMatrix,
            SingleCoordinateInPointInMatrix
        }


        // store variables in a hash table so we can index them by name
        // initialize capacity to a prime number greater than the number we expect
        private Hashtable mUserVariables = new Hashtable(53);

        public VariableManager()
        {
            mUserVariables.Clear();
        }

        public VariableManager(VariableManager VM)
        {
            mUserVariables = VM.CopyVariables();
        }

        #region Text Interpretation

        public static MathOperator MathOperatorFromText(string Txt)
        {
            Txt = Txt.Trim();

            if (Txt == "+") return VariableManager.MathOperator.Add;
            if (Txt == "-") return VariableManager.MathOperator.Subtract;
            if (Txt == "*") return VariableManager.MathOperator.Multiply;
            if (Txt == "/") return VariableManager.MathOperator.Divide;

            throw new Exception("Invalid text where math operator was expected: " + Txt);
        }

        public static ComparisonOperator ComparisonOperatorFromText(string Txt)
        {
            Txt = Txt.Trim();

            if (Txt == "=") return VariableManager.ComparisonOperator.EqualTo;
            if (Txt == "<>") return VariableManager.ComparisonOperator.NotEqualTo;
            if (Txt == "<") return VariableManager.ComparisonOperator.LessThan;
            if (Txt == "<=") return VariableManager.ComparisonOperator.LessThanOrEqualTo;
            if (Txt == ">") return VariableManager.ComparisonOperator.GreaterThan;
            if (Txt == ">=") return VariableManager.ComparisonOperator.GreaterThanOrEqualTo;

            throw new Exception("Invalid text where comparison operator was expected: " + Txt);
        }

        public object GetValueFromText(string Txt)
        {
            Txt = PreConditionText(Txt);

            switch (GetTextType(Txt))
            {
                case VariableManager.TextType.BlnValue:
                    return GetBooleanFromText(Txt);

                case VariableManager.TextType.DblValue:
                    return GetDoubleFromText(Txt);

                case VariableManager.TextType.IntValue:
                    return GetIntFromText(Txt);

                case VariableManager.TextType.PointInMatrix:
                    return GetCoordinateFromText(Txt);

                case VariableManager.TextType.SingleCoordinateInPoint:
                    return GetDoubleFromText(Txt);

                case VariableManager.TextType.SingleCoordinateInPointInMatrix:
                    return GetDoubleFromText(Txt);

                case VariableManager.TextType.VariableName:
                    return mUserVariables[Txt];

                default:
                    throw new Exception("Unable to interpret this text : '" + Txt + "'");
            }
        }

        public int GetIntFromText(string Txt)
        {
            Txt = PreConditionText(Txt);

            switch (GetTextType(Txt))
            {
                case VariableManager.TextType.BlnValue:
                    throw new Exception("Cannot convert from boolean to integer : '" + Txt + "'");

                case VariableManager.TextType.DblValue:
                    throw new Exception("Cannot convert from double to integer : '" + Txt + "'");

                case VariableManager.TextType.IntValue:
                    return System.Convert.ToInt32(Txt);

                case VariableManager.TextType.PointInMatrix:
                    throw new Exception("Cannot convert from point in matrix to integer : '" + Txt + "'");

                case VariableManager.TextType.SingleCoordinateInPoint:
                    throw new Exception("Cannot convert from double to integer : '" + Txt + "'");

                case VariableManager.TextType.SingleCoordinateInPointInMatrix:
                    throw new Exception("Cannot convert from double to integer : '" + Txt + "'");

                case VariableManager.TextType.VariableName:
                    object UserVar = mUserVariables[Txt];
                    if (!(UserVar is System.Int32)) throw new Exception("User variable is not an integer: '" + Txt + "'");
                    return (int)UserVar;

                default:
                    throw new Exception("Unable to interpret this text : '" + Txt + "'");
            }
        }

        public double GetDoubleFromText(string Txt)
        {
            string VarName;
            int CoordInd, XInd, YInd;
            object UserVar;

            Txt = PreConditionText(Txt);

            switch (GetTextType(Txt))
            {
                case VariableManager.TextType.BlnValue:
                    throw new Exception("Cannot convert from boolean to double : '" + Txt + "'");

                case VariableManager.TextType.DblValue:
                    return System.Convert.ToDouble(Txt);

                case VariableManager.TextType.IntValue:
                    return System.Convert.ToDouble(Txt);

                case VariableManager.TextType.PointInMatrix:
                    throw new Exception("Cannot convert from point in matrix to double : '" + Txt + "'");

                case VariableManager.TextType.SingleCoordinateInPoint:
                    ParseText_SingleCoordinateInPoint(Txt, out VarName, out CoordInd);
                    UserVar = mUserVariables[VarName];
                    if (!(UserVar is MachineCoordinate)) throw new Exception("Variable '" + Txt + "' is not a valid point");
                    return ((MachineCoordinate)UserVar)[CoordInd];

                case VariableManager.TextType.SingleCoordinateInPointInMatrix:
                    ParseText_SingleCoordinateInPointInMatrix(Txt, out VarName, out XInd, out YInd, out CoordInd);
                    UserVar = mUserVariables[VarName];
                    if (!(UserVar is CoordinateMatrix)) throw new Exception("Variable '" + Txt + "' is not a valid point matrix");
                    return ((CoordinateMatrix)UserVar)[XInd, YInd][CoordInd];

                case VariableManager.TextType.VariableName:
                    UserVar = mUserVariables[Txt];
                    if (UserVar is System.Int32) return System.Convert.ToDouble((int)UserVar);
                    if (UserVar is System.Double) return (double)UserVar;
                    throw new Exception("User variable is not a double or integer: '" + Txt + "'");

                default:
                    throw new Exception("Unable to interpret this text : '" + Txt + "'");
            }
        }

        public bool GetBooleanFromText(string Txt)
        {
            Txt = PreConditionText(Txt);

			if (Txt == "") Txt = "FALSE";

            switch (GetTextType(Txt))
            {
                case VariableManager.TextType.BlnValue:
                    if (Txt == "TRUE") return true;
                    if (Txt == "FALSE") return false;
                    throw new Exception("Error converting text to boolean value : '" + Txt + "'");

                case VariableManager.TextType.DblValue:
                    throw new Exception("Cannot convert from double to boolean : '" + Txt + "'");

                case VariableManager.TextType.IntValue:
                    throw new Exception("Cannot convert from int to boolean : '" + Txt + "'");

                case VariableManager.TextType.PointInMatrix:
                    throw new Exception("Cannot convert from point in matrix to boolean : '" + Txt + "'");

                case VariableManager.TextType.SingleCoordinateInPoint:
                    throw new Exception("Cannot convert from double to boolean : '" + Txt + "'");

                case VariableManager.TextType.SingleCoordinateInPointInMatrix:
                    throw new Exception("Cannot convert from double to boolean : '" + Txt + "'");

                case VariableManager.TextType.VariableName:
                    object UserVar = mUserVariables[Txt];
                    if (!(UserVar is System.Boolean)) throw new Exception("User variable is not a boolean : '" + Txt + "'");
                    return (bool)UserVar;

                default:
                    throw new Exception("Unable to interpret this text : '" + Txt + "'");
            }
        }

        public MachineCoordinate GetCoordinateFromText(string Txt)
        {
            string VarName;
            int XInd, YInd;
            object UserVar;

            Txt = PreConditionText(Txt);

            switch (GetTextType(Txt))
            {
                case VariableManager.TextType.BlnValue:
                    throw new Exception("Cannot convert from boolean to point: '" + Txt + "'");

                case VariableManager.TextType.DblValue:
                    throw new Exception("Cannot convert from double to point : '" + Txt + "'");

                case VariableManager.TextType.IntValue:
                    throw new Exception("Cannot convert from int to point : '" + Txt + "'");

                case VariableManager.TextType.PointInMatrix:
                    ParseText_PointInMatrix(Txt, out VarName, out XInd, out YInd);
                    UserVar = mUserVariables[VarName];
                    if (!(UserVar is CoordinateMatrix)) throw new Exception("Variable '" + Txt + "' is not a valid point matrix");
                    return ((CoordinateMatrix)UserVar)[XInd, YInd];

                case VariableManager.TextType.SingleCoordinateInPoint:
                    throw new Exception("Cannot convert from double to point : '" + Txt + "'");

                case VariableManager.TextType.SingleCoordinateInPointInMatrix:
                    throw new Exception("Cannot convert from double to point : '" + Txt + "'");

                case VariableManager.TextType.VariableName:
                    UserVar = mUserVariables[Txt];
                    if (!(UserVar is MachineCoordinate)) throw new Exception("User variable is not a point : '" + Txt + "'");
                    return (MachineCoordinate)UserVar;

                default:
                    throw new Exception("Unable to interpret this text : '" + Txt + "'");
            }
        }


        private void ParseText_PointInMatrix(string Txt, out string UserVar, out int XIndex, out int YIndex)
        {
            int Pos, Pos2;

            // get the variable name
            Pos = Txt.IndexOf("[");
            UserVar = Txt.Substring(0, Pos);

            // get the X index - may be from a variable too
            Pos2 = Txt.IndexOf(",", Pos + 1);
            if (Pos2 < Pos) throw new Exception("Invalid expression: '" + Txt + "' - missing comma in indexer.");
            XIndex = GetIntFromText(Txt.Substring(Pos + 1, Pos2 - Pos - 1));

            // get the Y index - may be from a variable too
            Pos = Pos2;
            Pos2 = Txt.IndexOf("]", Pos + 1);
            if (Pos2 < Pos) throw new Exception("Invalid expression: '" + Txt + "' - closing bracket in indexer.");
            YIndex = GetIntFromText(Txt.Substring(Pos + 1, Pos2 - Pos - 1));
        }

        private void ParseText_SingleCoordinateInPoint(string Txt, out string UserVar, out int CoordIndex)
        {
            int DotPos = Txt.LastIndexOf(".");

            // WashPoint.X
            if (DotPos != (Txt.Length - 2)) throw new Exception("Invalid expression: '" + Txt + "' - dot must be followed by just one character: X, Y, or Z.");

            switch (Txt[Txt.Length - 1])
            {
                case 'X':
                    CoordIndex = 0;
                    break;
                case 'Y':
                    CoordIndex = 1;
                    break;
                case 'Z':
                    CoordIndex = 2;
                    break;
                default:
                    throw new Exception("Invalid expression: '" + Txt + "' - dot must be followed by just one character: X, Y, or Z.");
            }

            UserVar = Txt.Substring(0, DotPos);
        }

        private void ParseText_SingleCoordinateInPointInMatrix(string Txt, out string UserVar, out int XIndex, out int YIndex, out int CoordIndex)
        {
            int Pos = Txt.LastIndexOf(".");
            int Pos2;

            // Microtiter[0,1].X
            if (Pos != (Txt.Length - 2)) throw new Exception("Invalid expression: '" + Txt + "' - dot must be followed by just one character: X, Y, or Z.");

            switch (Txt[Txt.Length - 1])
            {
                case 'X':
                    CoordIndex = 0;
                    break;
                case 'Y':
                    CoordIndex = 1;
                    break;
                case 'Z':
                    CoordIndex = 2;
                    break;
                default:
                    throw new Exception("Invalid expression: '" + Txt + "' - dot must be followed by just one character: X, Y, or Z.");
            }

            // get the variable name
            Pos = Txt.IndexOf("[");
            UserVar = Txt.Substring(0, Pos);

            // get the X index - may be from a variable too
            Pos2 = Txt.IndexOf(",", Pos + 1);
            if (Pos2 < Pos) throw new Exception("Invalid expression: '" + Txt + "' - missing comma in indexer.");
            XIndex = GetIntFromText(Txt.Substring(Pos + 1, Pos2 - Pos - 1));

            // get the Y index - may be from a variable too
            Pos = Pos2;
            Pos2 = Txt.IndexOf("]", Pos + 1);
            if (Pos2 < Pos) throw new Exception("Invalid expression: '" + Txt + "' - closing bracket in indexer.");
            YIndex = GetIntFromText(Txt.Substring(Pos + 1, Pos2 - Pos - 1));
        }


        private string PreConditionText(string Txt)
        {
            return (Txt.ToUpper().Trim());

        }


        public VariableManager.TextType GetTextType(string Txt)
        {
            Txt = PreConditionText(Txt);

            // is it a user variable that we know?
            if (IsUserVariable(Txt)) return VariableManager.TextType.VariableName;

            // is it true or false?
            if ((Txt == "TRUE") || (Txt == "FALSE")) return VariableManager.TextType.BlnValue;

            // is it a single coordinate within a point "Wash.X" or a matrix "Microtiter[0,0].X"?
            if ((Txt.IndexOf(".X") >= 0) || (Txt.IndexOf(".Y") >= 0) || (Txt.IndexOf(".Z") >= 0))
            {
                return ((Txt.IndexOf("[") >= 0) ? VariableManager.TextType.SingleCoordinateInPointInMatrix : VariableManager.TextType.SingleCoordinateInPoint);
            }

            // is it a matrix point?
            if ((Txt.IndexOf("[") >= 0) && (Txt.IndexOf("]") >= 0)) return VariableManager.TextType.PointInMatrix;

            // is it a number?
            try
            {
                double d = System.Convert.ToDouble(Txt);
                return ((Txt.IndexOf(".") >= 0) ? VariableManager.TextType.DblValue : VariableManager.TextType.IntValue);
            }
            catch { }

            // if we haven't returned yet, who knows what this is
            return VariableManager.TextType.UnknownString;
        }

        public bool IsUserVariable(string s)
        {
            if (s == "") return false;

            return VariableDefined(s);

        }

        public static bool IsNumber(string s)
        {
            try
            {
                double d = System.Convert.ToDouble(s);
                return true;
            }
            catch
            {
                return false;
            }
        }


        #endregion

        #region Variable Management

        public void DefineVariable(string VN, int Value) { AddVariableToTable(VN, Value); }
        public void DefineVariable(string VN, double Value) { AddVariableToTable(VN, Value); }
        public void DefineVariable(string VN, bool Value) { AddVariableToTable(VN, Value); }
        public void DefineVariable(string VN, CoordinateMatrix Value) { AddVariableToTable(VN, Value); }
        public void DefineVariable(string VN, MachineCoordinate Value) { AddVariableToTable(VN, Value); }
        private void AddVariableToTable(string VN, object Value)
        {
            string BadChars = " -+*/=^.()[]',:;";

            VN = PreConditionText(VN);
            if (VN == "") throw new Exception("Must assign a variable name.");
            if (VariableDefined(VN)) throw new Exception("Variable " + VN + " is already defined.");

            if (IsNumber(VN.Substring(0, 1))) throw new Exception("Invalid variable name: '" + VN + "' - cannot start with a number");
            if (VN.IndexOfAny(BadChars.ToCharArray()) >= 0) throw new Exception("Invalid variable name: '" + VN + "' - cannot contain these characters: " + BadChars);

            mUserVariables.Add(VN, Value);
        }

        public void SetVariable(string VN, int Value)
        {
            VN = VN.ToUpper();

            object V = mUserVariables[VN];

            if (V == null) throw new Exception("Variable " + VN + " is not defined.");

            if (!IsValidNumericType(V)) throw new Exception("Variable " + VN + " is not numeric.");

            if (V is System.Int32) mUserVariables[VN] = Value;
            else mUserVariables[VN] = (double)Value;

        }

        public void SetVariable(string VN, double Value)
        {
            VN = VN.ToUpper();

            object V = mUserVariables[VN];

            if (V == null) throw new Exception("Variable " + VN + " is not defined.");

            if (!IsValidNumericType(V)) throw new Exception("Variable " + VN + " is not numeric.");

            if (V is System.Int32) mUserVariables[VN] = (int)Value;
            else mUserVariables[VN] = Value;

        }

        public void SetVariable(string VN, bool Value)
        {
            VN = VN.ToUpper();

            object V = mUserVariables[VN];

            if (V == null) throw new Exception("Variable " + VN + " is not defined.");

            if (!(V is System.Boolean)) throw new Exception("Variable " + VN + " is not boolean.");

            mUserVariables[VN] = Value;
        }

        public void SetVariable(string VN, MachineCoordinate Value)
        {
            VN = VN.ToUpper();

            object V = mUserVariables[VN];

            if (V == null) throw new Exception("Variable " + VN + " is not defined.");

            if (!(V is MachineCoordinate)) throw new Exception("Variable " + VN + " is not point.");

            mUserVariables[VN] = Value;
        }

        public void SetVariable(string VN, CoordinateMatrix Value)
        {
            VN = VN.ToUpper();

            object V = mUserVariables[VN];

            if (V == null) throw new Exception("Variable " + VN + " is not defined.");

            if (!(V is CoordinateMatrix)) throw new Exception("Variable " + VN + " is not matrix.");

            mUserVariables[VN] = Value;
        }

        public void RemoveVariable(string VN)
        {
            VN = VN.ToUpper();

            if (!VariableDefined(VN)) return;

            mUserVariables.Remove(VN);
        }

        public void Clear()
        {
            mUserVariables.Clear();
        }

        public bool VariableDefined(string VN)
        {
            return (mUserVariables[VN.ToUpper()] != null);
        }

        public System.Type VariableType(string VN)
        {
            object V = mUserVariables[VN.ToUpper()];

            if (V == null) return null;

            return V.GetType();
        }

        private bool IsValidNumericType(object Obj)
        {
            if (Obj is System.Int32) return true;
            if (Obj is System.Double) return true;
            return false;
        }

        public System.Data.DataTable ListVariables()
        {
            try
            {
                System.Data.DataTable VarList = new DataTable();
                DataRow NewRow;

                // don't allow the variables to be changed while enumerating    THIS IS NEW 03-28-07
                lock (this)
                {
                    IDictionaryEnumerator Enumerator = mUserVariables.GetEnumerator();

                    VarList.Columns.Add("Name").ReadOnly = true;
                    VarList.Columns.Add("Type").ReadOnly = true;
                    VarList.Columns.Add("Value").ReadOnly = true;

                    while (Enumerator.MoveNext())
                    {
                        NewRow = VarList.NewRow();
                        NewRow["Name"] = Enumerator.Key.ToString();

                        // 03-03-07 is the exception here?
                        if (Enumerator.Value != null)
                        {
                            NewRow["Type"] = Enumerator.Value.GetType().ToString();
                            NewRow["Value"] = Enumerator.Value.ToString();
                        }
                        else
                        {
                            NewRow["Type"] = "";
                            NewRow["Value"] = "";
                        }

                        VarList.Rows.Add(NewRow);
                    }
                }
                return VarList;
            }
            catch
            {
                return null;
            }
        }

        protected Hashtable CopyVariables()
        {
            Hashtable Out = new Hashtable(53);

            // don't allow changes while we're enumerating    THIS IS NEW
            lock (mUserVariables)
            {

                IDictionaryEnumerator Enumerator = mUserVariables.GetEnumerator();

                while (Enumerator.MoveNext())
                {
                    Out.Add(Enumerator.Key, Enumerator.Value);
                }

                return Out;
            }
        }

        #endregion

        #region Math Functions

        public void MathOperation(object lhs, object rhs, ref object result, MathOperator Operator)
        {
            object LHSObj = lhs;
            object RHSObj = rhs;
            object ResultObj = result;

            // convert any variable names to the real numbers
            if (LHSObj is System.String) LHSObj = GetValueFromText((string)lhs); //mUserVariables[((string)lhs).ToUpper()];
            if (RHSObj is System.String) RHSObj = GetValueFromText((string)rhs); //mUserVariables[((string)rhs).ToUpper()];

            // if the result is a string, it must be a variable name
            if (ResultObj is System.String)
            {
                result = ((string)result).ToUpper();
                ResultObj = mUserVariables[(string)result];
            }

            // are the variables defined
            if (LHSObj == null) throw new Exception("LHS parameter '" + lhs.ToString() + "' is not defined");
            if (RHSObj == null) throw new Exception("RHS parameter '" + rhs.ToString() + "' is not defined");
            if (ResultObj == null) throw new Exception("Result parameter '" + result.ToString() + "' is not defined");

            // are they valid types
            if (!IsValidNumericType(LHSObj)) throw new Exception("LHS parameter '" + lhs.ToString() + "' is not a valid numeric type");
            if (!IsValidNumericType(RHSObj)) throw new Exception("RHS parameter '" + rhs.ToString() + "' is not a valid numeric type");
            if (!IsValidNumericType(ResultObj)) throw new Exception("Result parameter '" + result.ToString() + "' is not a valid numeric type");

            // do the operation
            // pass on the original result object so that we know what type it was
            // if it's a user variable we need the UserVariableName object to assign it 
            switch (Operator)
            {
                case VariableManager.MathOperator.Add:
                    Add(LHSObj, RHSObj, ref result);
                    break;

                case VariableManager.MathOperator.Subtract:
                    Subtract(LHSObj, RHSObj, ref result);
                    break;

                case VariableManager.MathOperator.Multiply:
                    Multiply(LHSObj, RHSObj, ref result);
                    break;

                case VariableManager.MathOperator.Divide:
                    Divide(LHSObj, RHSObj, ref result);
                    break;
                default:
                    throw new Exception("Unknown math operator");
            }
        }


        private void Add(object lhs, object rhs, ref object result)
        {
            bool IsUserVariable = (result is System.String);
            object ResultObj = result;
            int IntResult = 0;
            double DblResult = 0.0;

            if (result is System.String) ResultObj = mUserVariables[result];

            // calculate the sum as an integer or double
            if (ResultObj is System.Int32)
            {
                // add to create an integer
                if (lhs is System.Int32)
                {
                    IntResult = (System.Int32)((System.Int32)lhs + ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
                else
                {
                    IntResult = (System.Int32)((System.Double)lhs + ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
            }
            else if (ResultObj is System.Double)
            {
                // add to create a double
                if (lhs is System.Int32)
                {
                    DblResult = (System.Double)((System.Int32)lhs + ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
                else
                {
                    DblResult = (System.Double)((System.Double)lhs + ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
            }
            else
            {
                throw new Exception("Add operation: result variable has invalid data type");
            }

            // assign result to the output variable
            if (IsUserVariable)
            {
                if (ResultObj is System.Int32) mUserVariables[result] = IntResult;
                else mUserVariables[result] = DblResult;
            }
            else
            {
                if (ResultObj is System.Int32) result = IntResult;
                else result = DblResult;
            }
        }


        private void Subtract(object lhs, object rhs, ref object result)
        {
            bool IsUserVariable = (result is System.String);
            object ResultObj = result;
            int IntResult = 0;
            double DblResult = 0.0;

            if (IsUserVariable) ResultObj = mUserVariables[result];

            // calculate the difference as an integer or double
            if (ResultObj is System.Int32)
            {
                // subtract to create an integer
                if (lhs is System.Int32)
                {
                    IntResult = (System.Int32)((System.Int32)lhs - ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
                else
                {
                    IntResult = (System.Int32)((System.Double)lhs - ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
            }
            else if (ResultObj is System.Double)
            {
                // subtract to create a double
                if (lhs is System.Int32)
                {
                    DblResult = (System.Double)((System.Int32)lhs - ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
                else
                {
                    DblResult = (System.Double)((System.Double)lhs - ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
            }
            else
            {
                throw new Exception("Subtract operation: result variable has invalid data type");
            }

            // assign result to the output variable
            if (IsUserVariable)
            {
                if (ResultObj is System.Int32) mUserVariables[result] = IntResult;
                else mUserVariables[result] = DblResult;
            }
            else
            {
                if (ResultObj is System.Int32) result = IntResult;
                else result = DblResult;
            }
        }


        private void Multiply(object lhs, object rhs, ref object result)
        {
            bool IsUserVariable = (result is System.String);
            object ResultObj = result;
            int IntResult = 0;
            double DblResult = 0.0;

            if (IsUserVariable) ResultObj = mUserVariables[result];

            // calculate the product as an integer or double
            if (ResultObj is System.Int32)
            {
                // add to create an integer
                if (lhs is System.Int32)
                {
                    IntResult = (System.Int32)((System.Int32)lhs * ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
                else
                {
                    IntResult = (System.Int32)((System.Double)lhs * ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
            }
            else if (ResultObj is System.Double)
            {
                // add to create a double
                if (lhs is System.Int32)
                {
                    DblResult = (System.Double)((System.Int32)lhs * ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
                else
                {
                    DblResult = (System.Double)((System.Double)lhs * ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
            }
            else
            {
                throw new Exception("Multiply operation: result variable has invalid data type");
            }

            // assign result to the output variable
            if (IsUserVariable)
            {
                if (ResultObj is System.Int32) mUserVariables[result] = IntResult;
                else mUserVariables[result] = DblResult;
            }
            else
            {
                if (ResultObj is System.Int32) result = IntResult;
                else result = DblResult;
            }
        }


        private void Divide(object lhs, object rhs, ref object result)
        {
            bool IsUserVariable = (result is System.String);
            object ResultObj = result;
            int IntResult = 0;
            double DblResult = 0.0;

            if (IsUserVariable) ResultObj = mUserVariables[result];

            // calculate the quotient as an integer or double
            if (ResultObj is System.Int32)
            {
                // divide to create an integer
                if (lhs is System.Int32)
                {
                    IntResult = (System.Int32)((System.Int32)lhs / ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
                else
                {
                    IntResult = (System.Int32)((System.Double)lhs / ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
            }
            else if (ResultObj is System.Double)
            {
                // add to create a double
                if (lhs is System.Int32)
                {
                    DblResult = (System.Double)((System.Int32)lhs / ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
                else
                {
                    DblResult = (System.Double)((System.Double)lhs / ((rhs is System.Int32) ? ((System.Int32)rhs) : ((System.Double)rhs)));
                }
            }
            else
            {
                throw new Exception("Divide operation: result variable has invalid data type");
            }

            // assign result to the output variable
            if (IsUserVariable)
            {
                if (ResultObj is System.Int32) mUserVariables[result] = IntResult;
                else mUserVariables[result] = DblResult;
            }
            else
            {
                if (ResultObj is System.Int32) result = IntResult;
                else result = DblResult;
            }
        }


        public void ComparisonOperation(object lhs, object rhs, ref object result, ComparisonOperator Operator)
        {
            object LHSObj = lhs;
            object RHSObj = rhs;
            object ResultObj = result;

            // convert any variable names to the real numbers
            if (LHSObj is string) LHSObj = GetValueFromText((string)lhs);
            if (RHSObj is string) RHSObj = GetValueFromText((string)rhs);

            // if the result is a string, it must be a variable name
            if (ResultObj is string)
            {
                result = ((string)result).ToUpper();
                ResultObj = mUserVariables[(string)result];
            }

            // are the variables defined
            if (LHSObj == null) throw new Exception("LHS parameter '" + lhs.ToString() + "' is not defined");
            if (RHSObj == null) throw new Exception("RHS parameter '" + rhs.ToString() + "' is not defined");
            if (ResultObj == null) throw new Exception("Result parameter '" + result.ToString() + "' is not defined");


            // check the types - return must be a boolean for comparison operators
            if (!(ResultObj is System.Boolean)) throw new Exception("Result parameter '" + result.ToString() + "' must be a boolean");

            // do the operation
            switch (Operator)
            {
                case VariableManager.ComparisonOperator.EqualTo:
                    EqualTo(LHSObj, RHSObj, ref result);
                    break;

                case VariableManager.ComparisonOperator.NotEqualTo:
                    NotEqualTo(LHSObj, RHSObj, ref result);
                    break;

                case VariableManager.ComparisonOperator.GreaterThan:
                    GreaterThan(LHSObj, RHSObj, ref result);
                    break;

                case VariableManager.ComparisonOperator.GreaterThanOrEqualTo:
                    // arguments must both be numeric
                    GreaterThanOrEqualTo(LHSObj, RHSObj, ref result);
                    break;

                case VariableManager.ComparisonOperator.LessThan:
                    // arguments must both be numeric
                    LessThan(LHSObj, RHSObj, ref result);
                    break;

                case VariableManager.ComparisonOperator.LessThanOrEqualTo:
                    // arguments must both be numeric
                    LessThanOrEqualTo(LHSObj, RHSObj, ref result);
                    break;

                default:
                    throw new Exception("Unknown comparison operator");
            }
        }


        private void EqualTo(object lhs, object rhs, ref object result)
        {
            bool IsUserVariable = (result is System.String);
            object ResultObj = result;
            bool IsTrue;

            if (IsUserVariable) ResultObj = mUserVariables[result];

            if (IsValidNumericType(lhs) && IsValidNumericType(rhs))
            {
                if (lhs is System.Int32)
                {
                    IsTrue = ((rhs is System.Int32) ? ((int)lhs == (int)rhs) : ((int)lhs == (double)rhs));
                }
                else
                {
                    IsTrue = ((rhs is System.Int32) ? ((double)lhs == (int)rhs) : ((double)lhs == (double)rhs));
                }
            }
            else if (lhs is System.Boolean && rhs is System.Boolean)
            {
                IsTrue = (bool)lhs == (bool)rhs;
            }
            else
            {
                throw new Exception("EqualTo operation: operands must both be numeric or boolean");
            }

            // assign result to the output variable
            if (IsUserVariable)
            {
                mUserVariables[result] = IsTrue;
            }
            else
            {
                result = IsTrue;
            }
        }


        private void NotEqualTo(object lhs, object rhs, ref object result)
        {
            bool IsUserVariable = (result is System.String);
            object ResultObj = result;
            bool IsTrue;

            if (IsUserVariable) ResultObj = mUserVariables[result];

            if (IsValidNumericType(lhs) && IsValidNumericType(rhs))
            {
                if (lhs is System.Int32)
                {
                    IsTrue = ((rhs is System.Int32) ? ((int)lhs != (int)rhs) : ((int)lhs != (double)rhs));
                }
                else
                {
                    IsTrue = ((rhs is System.Int32) ? ((double)lhs != (int)rhs) : ((double)lhs != (double)rhs));
                }
            }
            else if (lhs is System.Boolean && rhs is System.Boolean)
            {
                IsTrue = (bool)lhs != (bool)rhs;
            }
            else
            {
                throw new Exception("NotEqualTo operation: operands must both be numeric or boolean");
            }

            // assign result to the output variable
            if (IsUserVariable)
            {
                mUserVariables[result] = IsTrue;
            }
            else
            {
                result = IsTrue;
            }
        }


        private void LessThan(object lhs, object rhs, ref object result)
        {
            bool IsUserVariable = (result is System.String);
            object ResultObj = result;
            bool IsTrue;

            if (IsUserVariable) ResultObj = mUserVariables[result];

            if (IsValidNumericType(lhs) && IsValidNumericType(rhs))
            {
                if (lhs is System.Int32)
                {
                    IsTrue = ((rhs is System.Int32) ? ((int)lhs < (int)rhs) : ((int)lhs < (double)rhs));
                }
                else
                {
                    IsTrue = ((rhs is System.Int32) ? ((double)lhs < (int)rhs) : ((double)lhs < (double)rhs));
                }
            }
            else
            {
                throw new Exception("LessThan operation: operands must both be numeric");
            }

            // assign result to the output variable
            if (IsUserVariable)
            {
                mUserVariables[result] = IsTrue;
            }
            else
            {
                result = IsTrue;
            }
        }


        private void LessThanOrEqualTo(object lhs, object rhs, ref object result)
        {
            bool IsUserVariable = (result is System.String);
            object ResultObj = result;
            bool IsTrue;

            if (IsUserVariable) ResultObj = mUserVariables[result];

            if (IsValidNumericType(lhs) && IsValidNumericType(rhs))
            {
                if (lhs is System.Int32)
                {
                    IsTrue = ((rhs is System.Int32) ? ((int)lhs <= (int)rhs) : ((int)lhs <= (double)rhs));
                }
                else
                {
                    IsTrue = ((rhs is System.Int32) ? ((double)lhs <= (int)rhs) : ((double)lhs <= (double)rhs));
                }
            }
            else
            {
                throw new Exception("LessThanOrEqualTo operation: operands must both be numeric");
            }

            // assign result to the output variable
            if (IsUserVariable)
            {
                mUserVariables[result] = IsTrue;
            }
            else
            {
                result = IsTrue;
            }
        }


        private void GreaterThan(object lhs, object rhs, ref object result)
        {
            bool IsUserVariable = (result is System.String);
            object ResultObj = result;
            bool IsTrue;

            if (IsUserVariable) ResultObj = mUserVariables[result];

            if (IsValidNumericType(lhs) && IsValidNumericType(rhs))
            {
                if (lhs is System.Int32)
                {
                    IsTrue = ((rhs is System.Int32) ? ((int)lhs > (int)rhs) : ((int)lhs > (double)rhs));
                }
                else
                {
                    IsTrue = ((rhs is System.Int32) ? ((double)lhs > (int)rhs) : ((double)lhs > (double)rhs));
                }
            }
            else
            {
                throw new Exception("GreaterThan operation: operands must both be numeric");
            }

            // assign result to the output variable
            if (IsUserVariable)
            {
                mUserVariables[result] = IsTrue;
            }
            else
            {
                result = IsTrue;
            }
        }


        private void GreaterThanOrEqualTo(object lhs, object rhs, ref object result)
        {
            bool IsUserVariable = (result is System.String);
            object ResultObj = result;
            bool IsTrue;

            if (IsUserVariable) ResultObj = mUserVariables[result];

            if (IsValidNumericType(lhs) && IsValidNumericType(rhs))
            {
                if (lhs is System.Int32)
                {
                    IsTrue = ((rhs is System.Int32) ? ((int)lhs >= (int)rhs) : ((int)lhs >= (double)rhs));
                }
                else
                {
                    IsTrue = ((rhs is System.Int32) ? ((double)lhs >= (int)rhs) : ((double)lhs >= (double)rhs));
                }
            }
            else
            {
                throw new Exception("GreaterThanOrEqualTo operation: operands must both be numeric");
            }

            // assign result to the output variable
            if (IsUserVariable)
            {
                mUserVariables[result] = IsTrue;
            }
            else
            {
                result = IsTrue;
            }
        }


        #endregion

    }

}
