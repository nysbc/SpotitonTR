using System;
using System.IO;
using System.Collections;
using System.Data;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;


namespace EA.PixyControl.ClassLibrary
{
    // define an event that is raised by the sequence file class each time we come to a new command
	// this lets the GUI update its display and cancel execution
	public delegate void NewCommandEventHandler(object sender, NewCommandEventArgs ev);
	public delegate void SequenceCompleteEventHandler(object sender, EventArgs ev);
	public delegate void VariableChangedEventHandler(object sender, EventArgs ev);

	public class NewCommandEventArgs : System.ComponentModel.CancelEventArgs
	{
		string	message = "";
		int		stepNumber;
			
		public NewCommandEventArgs(int StepNum) : base()
		{
			stepNumber = StepNum;
		}

		public string Message
		{
			get {return message;}
			set {message = value;}
		}

		public int StepNumber
		{
			get {return stepNumber;}
			set {stepNumber = value;}
		}
	}

	public class SequenceFile
	{
		#region members and properties
        //////////////////////////////////////////////////////////////////////////////////
        //
        //
        //////////////////////////////////////////////////////////////////////////////////

		private ProcessActionOpList operationList = null;
		private string				logFile = "";
		private ArrayList			cmdSequence = null;
		private VariableManager		varMgr = null;
		private string				fileName = null;

		public VariableManager VariableMgr
		{
			get {return varMgr;}
		}

		public ProcessActionOpList OperationList
		{
			get{return operationList;}
			set{operationList = value;}
		}

		public string LogFile
		{
			get{return logFile;}
			set{logFile = value;}
		}

		public ArrayList CommandSequence
		{
			get{return cmdSequence;}
			//set{cmdSequence = value;}
		}

		public string FileName
		{
			get{return fileName;}
		}
        
		#endregion members and properties



		#region contained classes
        //////////////////////////////////////////////////////////////////////////////////
        //
        //
        //////////////////////////////////////////////////////////////////////////////////

        private class ArgumentString
		{
			public string ArgumentName;
			public string ArgumentValue;
		}

		public class CommandNames                   // add new commands here
		{
			public const string CommentLine = "'";
			public const string Prime               = "PRIME";
			public const string Aspirate            = "ASPIRATE";
			public const string Dispense            = "DISPENSE";
			public const string SyringeSetValve     = "SYRINGE_SET_VALVE";
			public const string SyringeMove         = "SYRINGE_MOVE";
			public const string SyringeEmpty        = "SYRINGE_EMPTY";
			public const string WashTips            = "WASH_TIPS";
			public const string ControllerEnable    = "CONTROLLER_ENABLE";
			public const string InspectTips         = "INSPECT_TIPS";
			public const string PiezoDispense       = "PIEZO_DISPENSE";
			public const string MotionInitialize    = "MOTION_INITIALIZE";
			public const string HomeAxis            = "HOME_AXIS";
            public const string ServoEnable         = "SERVO_ENABLE";
            public const string ServoDisable        = "SERVO_DISABLE";
			public const string MoveRelative        = "MOVE_RELATIVE";
			public const string MoveToSafeHeight    = "MOVE_TO_SAFE_HEIGHT";
			public const string MoveToPoint         = "MOVE_TO_POINT";
			public const string MoveAbovePoint      = "MOVE_ABOVE_POINT";
			public const string DefineVariable      = "DEFINE_VARIABLE";
			public const string DefinePoint         = "DEFINE_POINT";
			public const string DefineGrid          = "DEFINE_GRID";
			public const string SetVariable         = "VARIABLE_SET";
			public const string VariableMath        = "VARIABLE_MATH";
			public const string VariableCompare     = "VARIABLE_COMPARE";
			public const string GetBooleanFromUser  = "GET_BOOLEAN_FROM_USER";
			public const string GetIntegerFromUser  = "GET_INTEGER_FROM_USER";
			public const string GetDoubleFromUser   = "GET_DOUBLE_FROM_USER";
            public const string WriteToConsole      = "WRITE_TO_CONSOLE";
			public const string Pause               = "PAUSE";
            public const string TimeStamp           = "TIMESTAMP";
			public const string ForStart            = "FOR";
			public const string ForEnd              = "NEXT";
			public const string WhileStart          = "WHILE";
			public const string WhileEnd            = "LOOP";
			public const string ExitLoop            = "EXITLOOP";
			public const string IfCmd               = "IF";
			public const string ElseCmd             = "ELSE";
			public const string EndIfCmd            = "ENDIF";
			public const string ExitSequence        = "EXITSEQUENCE";

            // Add new commands

            public const string SetMaxVelocity      = "SET_MAX_VELOCITY";  // PKv3.0  new command for lexium motors
            public const string SetAccel            = "SET_ACCEL";         // PKv3.0  new command for lexium motors
            public const string SetDefaultMotionParameters = "SET_DEFAULT_MOTION_PARAMETERS";         // PKv4.0,2015-04-28

            public const string DefineImport        = "DEFINE_IMPORT";   // [LEA COMMAND DEF]
			public const string DE03CosSetup        = "DE03_COS_SETUP";   // [PK COMMAND DEF]
			public const string DE03StartWaveform	= "DE03_START_WAVEFORM";   // [PK COMMAND DEF]
			public const string DE03StopWaveform	= "DE03_STOP_WAVEFORM";   // [PK COMMAND DEF]
			public const string DE03TrapSetup        = "DE03_TRAP_SETUP";   // [PK COMMAND DEF]
            public const string IOSetOutput          = "IO_SET_OUTPUT";   // [PK COMMAND DEF]
            public const string IOWaitInput          = "IO_WAIT_INPUT";   // [PK COMMAND DEF]

            public const string PlungeAxisMove         = "PLUNGE_AXIS_MOVE";   // [PK COMMAND DEF]
            public const string SuperPlunge            = "SUPER_PLUNGE";   // [PK COMMAND DEF]

            public const string GetIntegersFromFile = "GET_INTEGERS_FROM_FILE";   // [PK COMMAND DEF]
            public const string PutIntegersToFile = "PUT_INTEGERS_TO_FILE";   // [PK COMMAND DEF]
            public const string GetStepFromFile = "GET_STEP_FROM_FILE";   // [PK COMMAND DEF]

            public const string RotateMove = "ROTATE_MOVE";   // [PK COMMAND DEF]           // PKv4.0,2015-04-13
            public const string RotateSetup = "ROTATE_SETUP";   // [PK COMMAND DEF]

		}
	
		public class CommandStackItem
		{
			public int				CommandNumber = 0;
			public ProcessAction	Command = null;

			public CommandStackItem(ProcessAction PA, int CommandNum)
			{
				Command = PA;
				CommandNumber = CommandNum;
			}
		}

		#endregion contained classes


				
		#region text processing functions
        //////////////////////////////////////////////////////////////////////////////////
        //
        //
        //////////////////////////////////////////////////////////////////////////////////
		
		private static void ParseFunctionCall(string FullText, ref string FunctionName, ref ArrayList Args)
		{
			Args = new ArrayList();
			
			int				OpenParen = FullText.IndexOf("(");
			int				CloseParen = FullText.LastIndexOf(")");
			ArgumentString	NewArg = null;
			bool			InBracket = false;
			StringBuilder	CurrentParam = new StringBuilder();

			// some items have no parameters
			if (OpenParen <0 )
			{
				FunctionName = FullText;
				return;
			}

			if (CloseParen < 0) throw new Exception("Error in command text - no closing parenteses found:\n\n" + FullText);
			
			// get the function/class name
			FunctionName = FullText.Substring(0, OpenParen-1).Trim();
			
			// we had an open parentheses - must have a closing one at the end
			if (CloseParen != FullText.Length-1) throw new Exception("Error in command text - unexpected text after closing parentheses:\n\n" + FullText);

			// process the arguments letter by letter
			for(int i=OpenParen+1; i<CloseParen; ++i)
			{
				switch(FullText[i])
				{
					case ',':
						if (InBracket)
						{
							CurrentParam.Append(FullText[i]);
						}
						else
						{
							NewArg = ParseArgument(CurrentParam.ToString());
							Args.Add(NewArg);
							CurrentParam = new StringBuilder();
						}
						break;
					case '[':
						InBracket = true;
						CurrentParam.Append(FullText[i]);
						break;
					case ']':
						InBracket = false;
						CurrentParam.Append(FullText[i]);
						break;
					default:
						CurrentParam.Append(FullText[i]);
						break;
				}
			}

			if (CurrentParam.ToString() != "")
			{
				NewArg = ParseArgument(CurrentParam.ToString());
				Args.Add(NewArg);
			}
		}
        
		public static ProcessAction [] GetProcessActionList()
		{
			ProcessAction[] Cmds = {new Process_CommentLine(), 
                                    new Process_WriteToConsole(),
									new Process_Prime(),
									new Process_Aspirate(),
									new Process_Dispense(),
									new Process_SetSyringeValvePosition(),
									new Process_SyringeMove(),
									new Process_SyringeEmpty(),
									new Process_Wash(),
									new Process_EnableController(),
									new Process_InspectTipFiring(),
									new Process_PiezoDispense(),
									new Process_InitializeMotion(),
									new Process_HomeAxis(),
                                    new Process_ServoDisable(),     // PK New Command
                                    new Process_ServoEnable(),      // PK New Command
									new Process_MoveRelative(),
									new Process_MoveToSafeHeight(),
									new Process_MoveToPoint(),
									new Process_MoveAbovePoint(),
									new Variable_Define(),
									new Variable_DefinePoint(),
									new Variable_DefineGrid(),
									new Variable_Set(),
									new Variable_MathOperation(),
									new Variable_CompareOperation(),
									new User_GetBoolean(),
									new User_GetInteger(),
									new User_GetDouble(),
									new Process_Pause(),
                                    new Process_TimeStamp(),
									new Loop_ForStart(),
									new Loop_ForEnd(),
									new Loop_WhileStart(),
									new Loop_WhileEnd(),
									new Loop_ExitLoop(),
									new Process_If(),
									new Process_Else(),
									new Process_EndIf(),
									new Process_ExitSequence(),
								    // add new commands here 
                                    new Process_SetMaxVelocity(),   // PKv3.0
                                    new Process_SetAccel(),

                                    new Process_SetDefaultMotionParameters(),   //PKv4.0,2015-04-28

                                    new Variable_DefineImport(),            // [LEA COMMAND DEF]
									new Process_DE03CosSetup(),			    // PK New Command
									new Process_DE03StartWaveform(),		// PK New Command
									new Process_DE03StopWaveform(),			// PK New Command
									new Process_DE03TrapSetup(),			// PK New Command

                                    new Process_IOSetOutput(),			    // PK New Command
                                    new Process_IOWaitInput(),			    // PK New Command

                                    new Process_PlungeAxisMove(),            // PK New command
                                    new Process_SuperPlunge(),            // PK New command

                                    new User_GetIntegersFromFile(),          // PK New command
                                    new User_PutIntegersToFile(),           // PK New command
                                    new User_GetStepFromFile(),           // PK New command
                                    
                                    new Process_RotateMove(),               // PKv4.0,2015-04-13
                                    new Process_RotateSetup()
			};
			return Cmds;
		}

		// there must be a better way to do this...
		public static ProcessAction CreateCommandOfSameType(ProcessAction PA)
		{
			ProcessAction NewPA = null;

            if (PA is Process_CommentLine) NewPA = new Process_CommentLine();
            else if (PA is Process_Prime) NewPA = new Process_Prime();
            else if (PA is Process_Aspirate) NewPA = new Process_Aspirate();
            else if (PA is Process_Dispense) NewPA = new Process_Dispense();
            else if (PA is Process_SetSyringeValvePosition) NewPA = new Process_SetSyringeValvePosition();
            else if (PA is Process_SyringeMove) NewPA = new Process_SyringeMove();
            else if (PA is Process_SyringeEmpty) NewPA = new Process_SyringeEmpty();
            else if (PA is Process_Wash) NewPA = new Process_Wash();
            else if (PA is Process_EnableController) NewPA = new Process_EnableController();
            else if (PA is Process_InspectTipFiring) NewPA = new Process_InspectTipFiring();
            else if (PA is Process_PiezoDispense) NewPA = new Process_PiezoDispense();
            else if (PA is Process_InitializeMotion) NewPA = new Process_InitializeMotion();
            else if (PA is Process_HomeAxis) NewPA = new Process_HomeAxis();
            else if (PA is Process_ServoEnable) NewPA = new Process_ServoEnable();          // PK new command
            else if (PA is Process_ServoDisable) NewPA = new Process_ServoDisable();        // PK new command
            else if (PA is Process_MoveRelative) NewPA = new Process_MoveRelative();
            else if (PA is Process_MoveToSafeHeight) NewPA = new Process_MoveToSafeHeight();
            else if (PA is Process_MoveToPoint) NewPA = new Process_MoveToPoint();
            else if (PA is Process_MoveAbovePoint) NewPA = new Process_MoveAbovePoint();
            else if (PA is Variable_Define) NewPA = new Variable_Define();
            else if (PA is Variable_DefinePoint) NewPA = new Variable_DefinePoint();
            else if (PA is Variable_DefineGrid) NewPA = new Variable_DefineGrid();
            else if (PA is Variable_Set) NewPA = new Variable_Set();
            else if (PA is Variable_MathOperation) NewPA = new Variable_MathOperation();
            else if (PA is Variable_CompareOperation) NewPA = new Variable_CompareOperation();
            else if (PA is User_GetBoolean) NewPA = new User_GetBoolean();
            else if (PA is User_GetInteger) NewPA = new User_GetInteger();
            else if (PA is User_GetDouble) NewPA = new User_GetDouble();
            else if (PA is Process_Pause) NewPA = new Process_Pause();
            else if (PA is Process_WriteToConsole) NewPA = new Process_WriteToConsole();
            else if (PA is Process_TimeStamp) NewPA = new Process_TimeStamp();
            else if (PA is Loop_ForStart) NewPA = new Loop_ForStart();
            else if (PA is Loop_ForEnd) NewPA = new Loop_ForEnd();
            else if (PA is Loop_WhileStart) NewPA = new Loop_WhileStart();
            else if (PA is Loop_WhileEnd) NewPA = new Loop_WhileEnd();
            else if (PA is Loop_ExitLoop) NewPA = new Loop_ExitLoop();
            else if (PA is Process_If) NewPA = new Process_If();
            else if (PA is Process_Else) NewPA = new Process_Else();
            else if (PA is Process_EndIf) NewPA = new Process_EndIf();
            else if (PA is Process_ExitSequence) NewPA = new Process_ExitSequence();
            // add new commands here
            else if (PA is Process_SetMaxVelocity) NewPA = new Process_SetMaxVelocity();   // PKv3.0
            else if (PA is Process_SetAccel) NewPA = new Process_SetAccel();   // PKv3.0   
            else if (PA is Process_SetDefaultMotionParameters) NewPA = new Process_SetDefaultMotionParameters();   //PKv4.0,2015-04-28

            else if (PA is Variable_DefineImport) NewPA = new Variable_DefineImport();   // [LEA COMMAND DEF]
			else if (PA is Process_DE03CosSetup) NewPA = new Process_DE03CosSetup();     // PK new command
			else if (PA is Process_DE03StartWaveform) NewPA = new Process_DE03StartWaveform();     // PK new command
			else if (PA is Process_DE03StopWaveform) NewPA = new Process_DE03StopWaveform();     // PK new command
			else if (PA is Process_DE03TrapSetup) NewPA = new Process_DE03TrapSetup();     // PK new command
            else if (PA is Process_IOSetOutput) NewPA = new Process_IOSetOutput();     // PK new command
            else if (PA is Process_IOWaitInput) NewPA = new Process_IOWaitInput();     // PK new command
            else if (PA is Process_PlungeAxisMove) NewPA = new Process_PlungeAxisMove();     // PK new command
            else if (PA is Process_SuperPlunge) NewPA = new Process_SuperPlunge();     // PK new command
            else if (PA is User_GetIntegersFromFile) NewPA = new User_GetIntegersFromFile();     // PK new command
            else if (PA is User_PutIntegersToFile) NewPA = new User_PutIntegersToFile();     // PK new command
            else if (PA is User_GetStepFromFile) NewPA = new User_GetStepFromFile();     // PK new command
            else if (PA is Process_RotateMove) NewPA = new Process_RotateMove();     // PK new command  ,  pkV4.0,2015-04-13
            else if (PA is Process_RotateSetup) NewPA = new Process_RotateSetup();     // PK new command
			return NewPA;
		}
		
		public static ProcessAction GetFromProcessActionTypeFileText(string FullText)
		{
			if (FullText.Substring(0,1) == SequenceFile.CommandNames.CommentLine)	return new Process_CommentLine();
			
			if (FullText.IndexOf(SequenceFile.CommandNames.Prime.ToUpper()) == 0) return new Process_Prime();
			
			if (FullText.IndexOf(SequenceFile.CommandNames.Aspirate.ToUpper()) == 0) return new Process_Aspirate();
			
			if (FullText.IndexOf(SequenceFile.CommandNames.Dispense.ToUpper()) == 0) return new Process_Dispense();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.SyringeSetValve.ToUpper()) == 0) return new Process_SetSyringeValvePosition();
			
			if (FullText.IndexOf(SequenceFile.CommandNames.SyringeMove.ToUpper()) == 0) return new Process_SyringeMove();
			
			if (FullText.IndexOf(SequenceFile.CommandNames.SyringeEmpty.ToUpper()) == 0) return new Process_SyringeEmpty();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.WashTips.ToUpper()) == 0) return new Process_Wash();
			
			if (FullText.IndexOf(SequenceFile.CommandNames.ControllerEnable.ToUpper()) == 0) return new Process_EnableController();
			
			if (FullText.IndexOf(SequenceFile.CommandNames.InspectTips.ToUpper()) == 0) return new Process_InspectTipFiring();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.PiezoDispense.ToUpper()) == 0) return new Process_PiezoDispense();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.MotionInitialize.ToUpper()) == 0) return new Process_InitializeMotion();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.HomeAxis.ToUpper()) == 0) return new Process_HomeAxis();

            if (FullText.IndexOf(SequenceFile.CommandNames.ServoDisable.ToUpper()) == 0) return new Process_ServoDisable();   // PK New Command

            if (FullText.IndexOf(SequenceFile.CommandNames.ServoEnable.ToUpper()) == 0) return new Process_ServoEnable();   // PK New Command
				
			if (FullText.IndexOf(SequenceFile.CommandNames.MoveRelative.ToUpper()) == 0) return new Process_MoveRelative();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.MoveToSafeHeight.ToUpper()) == 0) return new Process_MoveToSafeHeight();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.MoveToPoint.ToUpper()) == 0) return new Process_MoveToPoint();
			
			if (FullText.IndexOf(SequenceFile.CommandNames.MoveAbovePoint.ToUpper()) == 0) return new Process_MoveAbovePoint();

			if (FullText.IndexOf(SequenceFile.CommandNames.DefineVariable.ToUpper()) == 0) return new Variable_Define();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.DefinePoint.ToUpper()) == 0) return new Variable_DefinePoint();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.DefineGrid.ToUpper()) == 0) return new Variable_DefineGrid();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.SetVariable.ToUpper()) == 0) return new Variable_Set();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.VariableMath.ToUpper()) == 0) return new Variable_MathOperation();

			if (FullText.IndexOf(SequenceFile.CommandNames.VariableCompare.ToUpper()) == 0) return new Variable_CompareOperation();
			
			if (FullText.IndexOf(SequenceFile.CommandNames.GetBooleanFromUser.ToUpper()) == 0) return new User_GetBoolean();
			
			if (FullText.IndexOf(SequenceFile.CommandNames.GetIntegerFromUser.ToUpper()) == 0) return new User_GetInteger();
			
			if (FullText.IndexOf(SequenceFile.CommandNames.GetDoubleFromUser.ToUpper()) == 0) return new User_GetDouble();

            if (FullText.IndexOf(SequenceFile.CommandNames.WriteToConsole.ToUpper()) == 0) return new Process_WriteToConsole();
			
			if (FullText.IndexOf(SequenceFile.CommandNames.Pause.ToUpper()) == 0) return new Process_Pause();

            if (FullText.IndexOf(SequenceFile.CommandNames.TimeStamp.ToUpper()) == 0) return new Process_TimeStamp();
			
			if (FullText.IndexOf(SequenceFile.CommandNames.ForStart.ToUpper()) == 0) return new Loop_ForStart();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.ForEnd.ToUpper()) == 0) return new Loop_ForEnd();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.WhileStart.ToUpper()) == 0) return new Loop_WhileStart();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.WhileEnd.ToUpper()) == 0) return new Loop_WhileEnd();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.ExitLoop.ToUpper()) == 0) return new Loop_ExitLoop();
					
			if (FullText.IndexOf(SequenceFile.CommandNames.IfCmd.ToUpper()) == 0) return new Process_If();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.ElseCmd.ToUpper()) == 0) return new Process_Else();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.EndIfCmd.ToUpper()) == 0) return new Process_EndIf();
				
			if (FullText.IndexOf(SequenceFile.CommandNames.ExitSequence.ToUpper()) == 0) return new Process_ExitSequence();

			// add new commands here
            if (FullText.IndexOf(SequenceFile.CommandNames.SetMaxVelocity.ToUpper()) == 0) return new Process_SetMaxVelocity();   // PKv3.0 New Command

            if (FullText.IndexOf(SequenceFile.CommandNames.SetAccel.ToUpper()) == 0) return new Process_SetAccel();   // PKv3.0 New Command

            if (FullText.IndexOf(SequenceFile.CommandNames.SetDefaultMotionParameters.ToUpper()) == 0) return new Process_SetDefaultMotionParameters();   //PKv4.0,2015-04-28

            if (FullText.IndexOf(SequenceFile.CommandNames.DefineImport.ToUpper()) == 0) return new Variable_DefineImport();  // [LEA COMMAND DEF]

			if (FullText.IndexOf(SequenceFile.CommandNames.DE03CosSetup.ToUpper()) == 0) return new Process_DE03CosSetup();   // PK New Command

			if (FullText.IndexOf(SequenceFile.CommandNames.DE03StartWaveform.ToUpper()) == 0) return new Process_DE03StartWaveform();   // PK New Command

			if (FullText.IndexOf(SequenceFile.CommandNames.DE03StopWaveform.ToUpper()) == 0) return new Process_DE03StopWaveform();   // PK New Command

			if (FullText.IndexOf(SequenceFile.CommandNames.DE03TrapSetup.ToUpper()) == 0) return new Process_DE03TrapSetup();   // PK New Command

            if (FullText.IndexOf(SequenceFile.CommandNames.IOSetOutput.ToUpper()) == 0) return new Process_IOSetOutput();   // PK New Command

            if (FullText.IndexOf(SequenceFile.CommandNames.IOWaitInput.ToUpper()) == 0) return new Process_IOWaitInput();   // PK New Command

            if (FullText.IndexOf(SequenceFile.CommandNames.PlungeAxisMove.ToUpper()) == 0) return new Process_PlungeAxisMove();   // PK New Command

            if (FullText.IndexOf(SequenceFile.CommandNames.SuperPlunge.ToUpper()) == 0) return new Process_SuperPlunge();   // PK New Command

            if (FullText.IndexOf(SequenceFile.CommandNames.GetIntegersFromFile.ToUpper()) == 0) return new User_GetIntegersFromFile();   // PK New Command

            if (FullText.IndexOf(SequenceFile.CommandNames.PutIntegersToFile.ToUpper()) == 0) return new User_PutIntegersToFile(); // PK New Command

            if (FullText.IndexOf(SequenceFile.CommandNames.GetStepFromFile.ToUpper()) == 0) return new User_GetStepFromFile(); // PK New Command

            if (FullText.IndexOf(SequenceFile.CommandNames.RotateMove.ToUpper()) == 0) return new Process_RotateMove(); // PKv4.0,2015-04-13

            if (FullText.IndexOf(SequenceFile.CommandNames.RotateSetup.ToUpper()) == 0) return new Process_RotateSetup(); // PKv4.0,2015-04-13

			throw new Exception("Unable to determine command type for text\n\n'" + FullText + "'");		
		}
        		
		// arguments are in the form ParameterName = Value or just Value
		private static ArgumentString ParseArgument(string ArgText)
		{
			int		EqualPos = ArgText.IndexOf("=");

			ArgumentString ArgStr = new ArgumentString();

			if (EqualPos > 0)
			{
				ArgStr.ArgumentName = ArgText.Substring(0, EqualPos).Trim();
				ArgStr.ArgumentValue = ArgText.Substring(EqualPos+1).Trim();
			}
			else
			{
				ArgStr.ArgumentValue = ArgText.Trim();
			}

			return ArgStr;
		}
        
		public static bool ProcessActionStringParametersOK(ProcessAction Obj, VariableManager VM, out string ErrorMsg)
		{
			Type							CommandType = Obj.GetType();
			PropertyInfo[]					Properties = CommandType.GetProperties();
			object[]						Attributes = null;
			ProcessActionArgumentAttribute	Attrib;
			object							ConvertedValue;
			object							PropertyValue = null;
			string							PropertyValueString;
			
			ErrorMsg = "";

			// check all the public properties that have the ProcessActionArgument attributes
			foreach (PropertyInfo PropInfo in Properties)
			{
				// only check the string properties
				if (!(PropInfo.PropertyType == typeof(string))) continue;

				// get the current value
				PropertyValue = PropInfo.GetValue(Obj, null);

				// get the attributes
				Attributes = PropInfo.GetCustomAttributes(typeof(ProcessActionArgumentAttribute), true);
				if (Attributes.Length > 0)
				{
					Attrib = Attributes[0] as ProcessActionArgumentAttribute;

					// can only do this check if paramter has this attribute
					if (Attrib != null)
					{
						// if it's required, make sure it's there
						if ((PropertyValue == null) || ((string)PropertyValue == ""))
						{
							if (Attrib.IsRequired)
							{
								ErrorMsg = "Missing required parameter '" + PropInfo.Name + "' in '" + CommandType.Name + "' command";
								return false;
							}
							else
							{
								continue;
							}
						}

						PropertyValueString = PropertyValue as string;

						try
						{
							// can it be converted to the right type?
							if (Attrib.TargetVariableType == typeof(string)) continue;
							else if (Attrib.TargetVariableType == typeof(int)) ConvertedValue = VM.GetIntFromText(PropertyValueString);
							else if (Attrib.TargetVariableType == typeof(double)) ConvertedValue = VM.GetDoubleFromText(PropertyValueString);
							else if (Attrib.TargetVariableType == typeof(bool)) ConvertedValue = VM.GetBooleanFromText(PropertyValueString);
							else if (Attrib.TargetVariableType == typeof(MachineCoordinate)) ConvertedValue = VM.GetCoordinateFromText(PropertyValueString);
							else throw new Exception("Unsupported target variable type: '" + Attrib.TargetVariableType.ToString() + "'" );
						}
						catch(Exception Ex)
						{
							ErrorMsg =  "Unable to convert '" + PropertyValueString + "' to correct data type for parameter '" + PropInfo.Name + "' in '" + CommandType.Name + "' command\n" + Ex.Message;
							return false;
						}
					}
				}
			}

			return true;
		
		}
        
		public static int FindFirstMathOperator(string Text, int Start, int End, ref string MathOp)
		{
			MathOp = "";

			int Pos = Text.IndexOfAny("+-/*".ToCharArray(), Start, End-Start+1);
			if (Pos<0) return Pos;

			MathOp = Text.Substring(Pos, 1);

			return Pos;
		}
        		
		public static int FindFirstComparisonOperator(string Text, int Start, int End, ref string CompareOp)
		{
			// 1)< 2)> 3)= 4)<> 5)>= 6)<=
			CompareOp = "";

			int Pos = Text.IndexOfAny("=<>".ToCharArray(), Start, End-Start+1);
			if (Pos < 0) return Pos;

			int Pos2 = Text.IndexOfAny(">=".ToCharArray(), Pos+1, 1);
			CompareOp = Text.Substring(Pos, (Pos2>0) ? 2 : 1);

			return Pos;
		}
        							
		// reads in any process action like aspirate or dispense command
		// uses reflection to determine what parameters/properties the process has
		// uses custom attributes to verify that all required parameters are present
		public static void GetProcessActionFromFileText(ProcessAction Obj, string FileText)
		{
			Type			T;
			PropertyInfo []	Properties;
			object[]		Attribs;
			bool []			FoundProperty;
			ArrayList		Args = null;
			ArgumentString	ThisArg = null;
			string			FunctionName = null;
			int				ArgInd, PropertyInd;
			bool			FoundMatch;

			// find out which type of class
			T = Obj.GetType();
			
			// get a list of all the public properties
			// these represent the parameters/variables for the process action
			Properties = T.GetProperties();

			// create an array for keeping track of what's been found
			FoundProperty = new bool [Properties.Length];
			
			// parse the command text into a function name and arguments
			ParseFunctionCall(FileText, ref FunctionName, ref Args);

			// search arguments for matching text items
			for (ArgInd = 0; ArgInd < Args.Count; ++ArgInd)
			{
				ThisArg = (ArgumentString)Args[ArgInd];

				if (ThisArg.ArgumentName == "") throw new Exception("Error in " + Obj.Name + " command - no name specified for argument " + ArgInd + 1);

				FoundMatch = false;
				
				for (PropertyInd = 0; PropertyInd < Properties.Length; ++PropertyInd)
				{
					if (Properties[PropertyInd].Name.ToUpper() == ThisArg.ArgumentName)
					{
						// mark this item as found - complain if an argument is present twice
						if (FoundProperty[PropertyInd]) throw new Exception("Error in " + Obj.Name + " command - two instances of argument " + Properties[PropertyInd].Name + "\n" + FileText);
						FoundProperty[PropertyInd] = true;

						// set the value using reflection
						Properties[PropertyInd].SetValue(Obj, System.Convert.ChangeType(ThisArg.ArgumentValue, Properties[PropertyInd].PropertyType),null);
						FoundMatch = true;
						break;
					}
				}

				// complain if they have an unknown argument - a typo?
				if (!FoundMatch) throw new Exception("Unknown argument in " + Obj.Name + " command - " + ThisArg.ArgumentName + "\n" + FileText);
			}

			// if anything wasn't found, make sure it wasn't required
			for (PropertyInd = 0; PropertyInd < Properties.Length; ++PropertyInd)
			{
				// if we found it, we're not worried
				if (FoundProperty[PropertyInd]) continue;

				// get the custom attributes to see whether it's required
				Attribs = Properties[PropertyInd].GetCustomAttributes(typeof(ProcessActionArgumentAttribute), true);
				
				foreach (Attribute ThisAttrib in Attribs)
				{
					ProcessActionArgumentAttribute CustomAtt = ThisAttrib as ProcessActionArgumentAttribute;

					if (CustomAtt != null)
					{
						if (CustomAtt.IsRequired) throw new Exception("Error in " + Obj.Name + " command - required parameter '" + Properties[PropertyInd].Name + "' not found\n\n" + FileText);
					}
				}
			}
		}
	
		public static string [] GetFileTextFromProcessAction(ProcessAction Cmd)
		{
			StringBuilder	sb = new StringBuilder(Cmd.NameInCommandFile);
			PropertyInfo []	Properties = null;
			int				ParameterCount;
			ArrayList		Strings = new ArrayList();
			string			PropertyValue;

			// some commands have no parameters (if, else, exitloop, etc.)
			sb.Append("(");
			ParameterCount = 0;

			// write any properties that have the ProcessActionParameter attribute
			Properties = Cmd.GetType().GetProperties();

			foreach (PropertyInfo P in Properties)
			{
				// only write enabled value if it's disabled - might not use this
				if (P.Name.ToUpper() == "ENABLED")
				{
					if (!Cmd.Enabled)
					{
						sb.Append("Enabled=false");
						++ParameterCount;
					}
				}
				else if (P.GetCustomAttributes(typeof(ProcessActionArgumentAttribute), false).Length > 0)
				{
					PropertyValue = P.GetValue(Cmd, null).ToString();

					// don't write optional parameters that we don't have a value for
					if (PropertyValue != "")
					{
						if (ParameterCount > 0)
						{
							// putting the parameters on separate lines makes it more readable
							sb.Append(", _");
							Strings.Add(sb.ToString());
							sb = new StringBuilder(" ");
						}

						sb.Append(P.Name);
						sb.Append("=");
						sb.Append(PropertyValue);
						
						++ParameterCount;
					}
				}
			}

			sb.Append(")");
		
			Strings.Add(sb.ToString());

			string [] ret = new string[Strings.Count];

			for(int i=0; i< Strings.Count; ++i)
			{
				ret[i] = (string)Strings[i];
			}

			return ret;
		}
        
		public static string [] GetDebugTextFromProcessAction(ProcessAction Cmd, VariableManager VM)
		{
			StringBuilder	sb = new StringBuilder(Cmd.NameInCommandFile);
			PropertyInfo []	Properties = null;
			int				ParameterCount;
			object			ParameterValue = null;
			ArrayList		Strings = new ArrayList();

			// some commands have no parameters (if, else, exitloop, etc.)
			sb.Append("(");
			ParameterCount = 0;

			// write any properties that have the ProcessActionParameter attribute
			Properties = Cmd.GetType().GetProperties();

			foreach (PropertyInfo P in Properties)
			{
				// only write enabled value if it's disabled - might not use this
				if (P.Name.ToUpper() == "ENABLED")
				{
					if (!Cmd.Enabled)
					{
						sb.Append("Enabled=false");
						++ParameterCount;
					}
				}
				else if (P.GetCustomAttributes(typeof(ProcessActionArgumentAttribute), false).Length > 0)
				{
					ParameterValue = P.GetValue(Cmd, null);

					if (ParameterCount > 0)
					{
						// putting the parameters on separate lines makes it more readable
						sb.Append(", _");
						Strings.Add(sb.ToString());
						sb = new StringBuilder(" ");
					}
					sb.Append(P.Name);
					sb.Append("=");
					sb.Append(ParameterValue.ToString());
			
					if (ParameterValue is string)
					{
						try
						{
							object V = VM.GetValueFromText((string)ParameterValue);
							sb.Append(" {Value=");
							sb.Append(V.ToString());
							sb.Append("}");
						}
						catch{}
					}
		
					++ParameterCount;
				}
			}

			sb.Append(")");
		
			Strings.Add(sb.ToString());

			string [] ret = new string[Strings.Count];

			for(int i=0; i< Strings.Count; ++i)
			{
				ret[i] = (string)Strings[i];
			}

			return ret;
		}
        
		private static string GetContinuedLines(StreamReader Reader, string CurrentLine, ref int LineNumber)
		{
			StringBuilder sb = new StringBuilder();
			
			// append any lines that have the to-be-continued sign
			while ( (CurrentLine.Length >= 2) && 
				(CurrentLine.LastIndexOf(" _") == (CurrentLine.Length - 2)) )
			{
				sb.Append(CurrentLine.Substring(0, CurrentLine.Length-2));
				LineNumber++;
				CurrentLine = Reader.ReadLine().Trim();
			}
			
			sb.Append(CurrentLine);

			return sb.ToString().ToUpper();
		}
        		
		#endregion text processing functions



		#region events
        //////////////////////////////////////////////////////////////////////////////////
        //
        //
        //////////////////////////////////////////////////////////////////////////////////

		public event NewCommandEventHandler NewCommand;
	
		public event SequenceCompleteEventHandler SequenceComplete;

		public event VariableChangedEventHandler VariableChanged;

		public void OnNewCommand(object sender, NewCommandEventArgs ev)
		{
			if (NewCommand != null) NewCommand(sender, ev);
		}

		public void OnSequenceComplete(object sender, EventArgs ev)
		{
			if (SequenceComplete != null) SequenceComplete(sender, ev);
		}

		public void OnVariableChanged(object sender, EventArgs ev)
		{
			if (VariableChanged != null) VariableChanged(sender, ev);
		}

		#endregion events

        // Revision R2.02
        // logAllCommands  variable set to false and timestamping to console window.

		public void ExecuteSequence()
		{
			int					BadCmd, ActionReturnCode, CurrInd=0;
			string				ErrorMsg;
			ProcessAction		Cmd = null;
			ArrayList			CmdStack = new ArrayList();
			ProcessActionOp		AssignedMethod = null;

            bool logAllCommands = false;

			try
			{
				LogToFile("Start execution of command sequence at " + Datalog.Timestamp());
			
				// check the sequence
				if (SequenceFile.SequenceOK(this.cmdSequence, this.varMgr, out BadCmd, out ErrorMsg) == false)
				{
					System.Windows.Forms.MessageBox.Show(ErrorMsg);
					return;
				}
			
				CurrInd = 0;

				while(CurrInd < this.cmdSequence.Count)
				{
                    if (logAllCommands)
                    {
					    LogToFile("");
					    LogToFile("Command # " + (CurrInd+1));
                    }
					
					Cmd = (ProcessAction)this.cmdSequence[CurrInd];

					// if we used the Enable boolean, we'd check it here
					// it might not be safe to allow that with loops and if/else/endif constructions

					// fire the event to update anybody who's watching about the new step - allow them to cancel
					NewCommandEventArgs EventInfo = new NewCommandEventArgs(CurrInd);
					OnNewCommand(null, EventInfo);
					
					// did user cancel execution
					if (EventInfo.Cancel)
					{
						LogToFile("Execution aborted at " + System.DateTime.Now.ToString());
						return;
					}

                    if (Cmd is Process_ExitSequence)
                    {
                        LogToFile(GetDebugTextFromProcessAction(Cmd, varMgr));
                        return;
                    }

                    else if ((Cmd is Variable_Define) || (Cmd is Variable_DefinePoint) || (Cmd is Variable_DefineGrid))
                    {
                        DefineVariable(Cmd, varMgr);
                        OnVariableChanged(null, EventArgs.Empty);
                    }

                        // check any flow control actions - make sure loops and ifs have starts and ends
                    else if ((Cmd is Loop_ForStart) || (Cmd is Loop_WhileStart) || (Cmd is Process_If))
                    {
                        // add the command start to the "stack"
                        CmdStack.Add(new CommandStackItem(Cmd, CurrInd));

                        bool DoIt = false;

                        if (Cmd is Loop_ForStart)
                        {
                            StartForLoop((Loop_ForStart)Cmd, varMgr, ref DoIt);
                            OnVariableChanged(null, EventArgs.Empty);
                        }
                        else if (Cmd is Loop_WhileStart) DoIt = DoWhileLoop((Loop_WhileStart)Cmd, varMgr);
                        else if (Cmd is Process_If) DoIt = DoIf((Process_If)Cmd, varMgr);

                        if (!DoIt)
                        {
                            if (Cmd is Process_If)
                            {
                                // go to the corresponding else or endif
                                if (logAllCommands)
                                    LogToFile(GetDebugTextFromProcessAction(Cmd, varMgr));
                                
                                SkipToElseOrEndIf(this.cmdSequence, CmdStack, ref CurrInd);
                                if (logAllCommands)
                                    LogToFile("Skip to Else or EndIf " + Cmd.Name);
                                continue;
                            }
                            else
                            {
                                // skip to the end of this loop or IF and pop this one off, continue at the step after the end
                                if (logAllCommands)
                                    LogToFile(GetDebugTextFromProcessAction(Cmd, varMgr));
                                SkipToEndOfCurrentStackItem(this.cmdSequence, CmdStack, ref CurrInd);
                                if (logAllCommands)
                                    LogToFile("Skip to end of loop");
                                continue;
                            }
                        }
                    }
                    else if (Cmd is Process_Else)
                    {   // make sure we're in an IF
                        if ((CmdStack.Count < 1) || ((((CommandStackItem)CmdStack[CmdStack.Count - 1]).Command is Process_If) == false)) throw new Exception(Cmd.Name + " command without corresponding If command");
                        // find the ENDIF and pop this one off, continue at the step after the end
                        SkipToEndOfCurrentStackItem(this.cmdSequence, CmdStack, ref CurrInd);

                        if (logAllCommands)
                        {
                            LogToFile(GetDebugTextFromProcessAction(Cmd, varMgr));
                            LogToFile("Skip to EndIF");
                        }

                        continue;
                    }
                    else if (Cmd is Process_EndIf)
                    {   // just pop the IF off the stack and move on
                        if ((CmdStack.Count < 1) || ((((CommandStackItem)CmdStack[CmdStack.Count - 1]).Command is Process_If) == false)) throw new Exception(Cmd.Name + " command without corresponding If command");

                        CmdStack.RemoveAt(CmdStack.Count - 1);
                    }
                    else if (Cmd is Loop_ExitLoop)
                    {
                        int EndOfLoopInd = -1;
                        // ExitLoop exits the current loop - find the next end of loop and continue from there
                        for (int SearchInd = CurrInd + 1; SearchInd < this.cmdSequence.Count; ++SearchInd)
                        {
                            Cmd = this.cmdSequence[SearchInd] as ProcessAction;

                            if ((Cmd is Loop_ForEnd) || (Cmd is Loop_WhileEnd))
                            {
                                EndOfLoopInd = SearchInd;
                                break;
                            }
                        }
                        if (EndOfLoopInd == -1) throw new Exception("Error finding end of loop for ExitLoop command # " + (CurrInd + 1));

                        CurrInd = EndOfLoopInd + 1;
                        if (logAllCommands)
                            LogToFile(GetDebugTextFromProcessAction(Cmd, varMgr));
                        continue;
                    }
                    else if (Cmd is Loop_ForEnd)
                    {
                        if ((CmdStack.Count < 1) || ((((CommandStackItem)CmdStack[CmdStack.Count - 1]).Command is Loop_ForStart) == false)) throw new Exception(Cmd.Name + " command without corresponding For loop start");

                        Loop_ForStart TheStart = ((CommandStackItem)CmdStack[CmdStack.Count - 1]).Command as Loop_ForStart;
                        bool DoIt = false;

                        // increment and check for loop
                        IncrementAndCheckForLoop(TheStart, varMgr, ref DoIt);
                        OnVariableChanged(null, EventArgs.Empty);

                        if (logAllCommands)
                            LogToFile("For loop variable {0} = {1}", ((Loop_ForStart)TheStart).VariableName, varMgr.GetIntFromText(((Loop_ForStart)TheStart).VariableName));

                        if (DoIt)
                        {
                            // if it's not done, continue with the command after the For
                            CurrInd = ((CommandStackItem)CmdStack[CmdStack.Count - 1]).CommandNumber + 1;
                            if (logAllCommands)
                            {
                                LogToFile(GetDebugTextFromProcessAction(Cmd, varMgr));
                                LogToFile("Back to start of For loop");
                            }
                            continue;
                        }
                        else
                        {
                            // if it is done, pop the loop start off the stack
                            if (logAllCommands)
                                LogToFile("Done with for loop");
                            CmdStack.RemoveAt(CmdStack.Count - 1);
                        }
                    }
                    else if (Cmd is Loop_WhileEnd)
                    {
                        if ((CmdStack.Count < 1) || ((((CommandStackItem)CmdStack[CmdStack.Count - 1]).Command is Loop_WhileStart) == false)) throw new Exception(Cmd.Name + " command without corresponding While loop start");

                        Loop_WhileStart TheStart = ((CommandStackItem)CmdStack[CmdStack.Count - 1]).Command as Loop_WhileStart;

                        // check the While loop condition
                        if (DoWhileLoop(TheStart, varMgr))
                        {
                            // if it's not done, continue with the command after the While
                            CurrInd = ((CommandStackItem)CmdStack[CmdStack.Count - 1]).CommandNumber + 1;
                            if (logAllCommands)
                            {
                                LogToFile(GetDebugTextFromProcessAction(Cmd, varMgr));

                                LogToFile("While loop variable {0} = {1}", ((Loop_WhileStart)TheStart).TestVariableName, varMgr.GetValueFromText(((Loop_WhileStart)TheStart).TestVariableName));
                                LogToFile("Back to start of While loop");
                            }
                            continue;
                        }
                        else
                        {
                            // if it is done, pop the loop start off the stack
                            if (logAllCommands)
                            {
                                LogToFile("While loop variable {0} = {1}", ((Loop_WhileStart)TheStart).TestVariableName, varMgr.GetValueFromText(((Loop_WhileStart)TheStart).TestVariableName));
                                LogToFile("Done with While loop");
                            }
                            CmdStack.RemoveAt(CmdStack.Count - 1);
                        }
                    }

                    else if ((Cmd is Variable_CompareOperation) || (Cmd is Variable_MathOperation) || (Cmd is Variable_Set))
                    {
                        VariableOperation(Cmd, varMgr);
                        OnVariableChanged(null, EventArgs.Empty);
                    }

                    else if (Cmd is User_GetBoolean)
                    {
                        GetBoolean((User_GetBoolean)Cmd, varMgr);
                        OnVariableChanged(null, EventArgs.Empty);
                    }

                    else if (Cmd is User_GetDouble)
                    {
                        GetDouble((User_GetDouble)Cmd, varMgr);
                        OnVariableChanged(null, EventArgs.Empty);
                    }

                    else if (Cmd is User_GetInteger)
                    {
                        GetInteger((User_GetInteger)Cmd, varMgr);
                        OnVariableChanged(null, EventArgs.Empty);
                    }

                    else if (Cmd is User_GetIntegersFromFile)
                    {
                        int ret1 = GetIntegersFromFile((User_GetIntegersFromFile)Cmd, varMgr);
                        if (ret1 != 0) throw new Exception("Execution error in method GetIntegersFromFile");
                        OnVariableChanged(null, EventArgs.Empty);
                    }

                    else if (Cmd is User_PutIntegersToFile)
                    {
                        int ret1 = PutIntegersToFile((User_PutIntegersToFile)Cmd, varMgr);
                        if (ret1 != 0) throw new Exception("Execution error in method GetIntegersFromFile");
                        OnVariableChanged(null, EventArgs.Empty);
                    }

                    else if (Cmd is User_GetStepFromFile)
                    {
                        int ret1 = GetStepFromFile((User_GetStepFromFile)Cmd, varMgr);
                        if (ret1 != 0) throw new Exception("Execution error in method GetIntegersFromFile");
                        OnVariableChanged(null, EventArgs.Empty);
                    }

                    else if (Cmd is Process_Pause) DoPause((Process_Pause)Cmd, varMgr);

                    else if (Cmd is Process_WriteToConsole)  DoWriteToConsole((Process_WriteToConsole)Cmd, varMgr);

                    else if (Cmd is Process_TimeStamp)
                    {
                        Process_TimeStamp P = (Process_TimeStamp)Cmd;
   //                     LogToFile(P.UserMessage+System.DateTime.Now.ToString());
                        LogToFile(P.UserMessage + Datalog.Timestamp());
                        Console.WriteLine(P.UserMessage + Datalog.Timestamp());
                    }

					// see if there is method assigned to this command
					// if so, do it
					if (this.operationList != null)
					{
						AssignedMethod = operationList.GetProcessActionOp(Cmd.NameInCommandFile);
						if (AssignedMethod != null) 
						{
							ActionReturnCode = AssignedMethod(Cmd);
							
							if (ActionReturnCode != 0) throw new Exception("Execution error in method " + AssignedMethod.ToString() + ". Returned code " + ActionReturnCode.ToString());
						}
					}

                    if (logAllCommands)
					    LogToFile(GetDebugTextFromProcessAction(Cmd, varMgr));
					
					// next command
					++CurrInd;

				}	// while

				System.Windows.Forms.MessageBox.Show("Test complete");

			} // try
			catch(Exception Ex)
			{
				string CmdName = "";
				
				if ((CurrInd > 0) && (CurrInd<=this.cmdSequence.Count)) CmdName = "(" + ((ProcessAction)this.cmdSequence[CurrInd]).Name + ")";
				
				System.Windows.Forms.MessageBox.Show("Error in sequence at command # " + (CurrInd+1) + " " + CmdName + "\n\n" + Ex.Message, "Execution Error");
				
			}
			finally
			{
				OnSequenceComplete(null, System.EventArgs.Empty);
			}
		}
        		
		// if there is an error, CmdNumber returns which one with a one-based index, otherwise CmdNumber returns zero
		public static bool SequenceOK(ArrayList CommandSequence, VariableManager VMInput, out int CmdNumber, out string ErrorMsg)
		{
			ProcessAction		Cmd = null;
			ArrayList			FlowCmdStack = new ArrayList();
			CommandStackItem	StackItem = null;
			
			// want to pass in system variables
			// but this method will also have to add variables
			// so make a local copy of the variable manager that starts with what is passed in
			VariableManager		VM = new VariableManager(VMInput);

			ErrorMsg = "";
			CmdNumber = 0;
			
			try
			{
				foreach(object Obj in CommandSequence)
				{
					++CmdNumber;
					
					Cmd = Obj as ProcessAction;
		
					// make sure it has all of its parameters
					if (Cmd.ParametersOK(VM, out ErrorMsg) == false) throw new Exception(ErrorMsg);

					// define the variable so it can be checked in subsequent commands
					if ((Cmd is Variable_Define) || (Cmd is Variable_DefinePoint) || (Cmd is Variable_DefineGrid))
					{
						DefineVariable(Cmd, VM);
					}

                    // [LEA COMMAND DEF]
                    // We can add more variable definitions into VM because it is a copy of VMInput
                    // and VMInput is left untouched.
                    if (Cmd is Variable_DefineImport)
                        ReadImportVariables((Variable_DefineImport)Cmd, VM);

					// check any flow control actions - make sure loops and ifs have starts and ends
					if ( (Cmd is Loop_ForStart) || (Cmd is Loop_WhileStart) || (Cmd is Process_If) )
					{
						// add it to the "stack" so we can make sure there is a corresponding end command
						StackItem = new CommandStackItem(Cmd, CmdNumber);
						FlowCmdStack.Add(StackItem);

					}
					else if (Cmd is Process_Else)
					{
						// else must have an if
						if ((FlowCmdStack.Count < 1) || ((((CommandStackItem)FlowCmdStack[FlowCmdStack.Count-1]).Command is Process_If) == false)) throw new Exception(Cmd.Name + " command without corresponding If command");
						
						// don't add an else to the stack, don't remove the if until we reach the end if
					}
					else if (Cmd is Process_EndIf)
					{
						// endif must have an if
						if ((FlowCmdStack.Count < 1) || ((((CommandStackItem)FlowCmdStack[FlowCmdStack.Count-1]).Command is Process_If) == false)) throw new Exception(Cmd.Name + " command without corresponding If command");
						
						// pop the if off the stack
						FlowCmdStack.RemoveAt(FlowCmdStack.Count-1);
					}
					else if (Cmd is Loop_ExitLoop)
					{
						// exit loop must be in a loop, it may not necessarily be at the end of the stack, you can exit from anywhere in the loop
						bool FoundLoopStart = false;
						
						for(int StackInd = FlowCmdStack.Count-1; StackInd >=0; --StackInd)
						{
							StackItem = ((CommandStackItem)FlowCmdStack[StackInd]);

							if ((StackItem.Command is Loop_ForStart) || (StackItem.Command is Loop_WhileStart))
							{
								FoundLoopStart = true;
								break;
							}
						}
						
						if (!FoundLoopStart) throw new Exception(Cmd.Name + " without corresponding loop start");
						// no popping yet
					}
					else if (Cmd is Loop_ForEnd)
					{
						// endif must have an if
						if ((FlowCmdStack.Count < 1) || ((((CommandStackItem)FlowCmdStack[FlowCmdStack.Count-1]).Command is Loop_ForStart) == false)) throw new Exception(Cmd.Name + " command without corresponding For loop start");
						
						// pop the if off the stack
						FlowCmdStack.RemoveAt(FlowCmdStack.Count-1);
					}
					else if (Cmd is Loop_WhileEnd) 
					{
						// endif must have an if
						if ((FlowCmdStack.Count < 1) || ((((CommandStackItem)FlowCmdStack[FlowCmdStack.Count-1]).Command is Loop_WhileStart) == false)) throw new Exception(Cmd.Name + " command without corresponding While loop start");
						
						// pop the if off the stack
						FlowCmdStack.RemoveAt(FlowCmdStack.Count-1);
					}
				}

				// should have popped any flow control cmds off, if not then something wasn't ended properly
				if (FlowCmdStack.Count != 0)
				{
					StackItem = (CommandStackItem)FlowCmdStack[FlowCmdStack.Count-1];
					CmdNumber = StackItem.CommandNumber;

					throw new Exception("No ending found for the " + StackItem.Command.Name + " command");
				}

			}
			catch(Exception Ex)
			{
				string CmdName = "";
				
				if ((CmdNumber > 0) && (CmdNumber<=CommandSequence.Count)) CmdName = "(" + ((ProcessAction)CommandSequence[CmdNumber-1]).Name + ")";
				
				ErrorMsg = "Error in sequence at command # " + CmdNumber.ToString() + " " + CmdName + "\n\n" + Ex.Message;
				
				return false;
			}

			CmdNumber = 0;
			return true;
		}
        		
		public int Read(string FileName, ArrayList CmdSeq, VariableManager VM)
		{
			if (CmdSeq == null) this.cmdSequence = new ArrayList();
			else this.cmdSequence = CmdSeq;

			if (VM == null) this.varMgr = new VariableManager();
			else this.varMgr = VM;

			this.cmdSequence.Clear();
			this.varMgr.Clear();
			this.fileName = FileName;
            
            int LineNumber = 0;
            int NumFound = 0;
            string NewLine = null;
            string FullText = null;
            ProcessAction NewObject = null;

            try
            {
                using (StreamReader Reader = new StreamReader(FileName))
                {
                    while (Reader.Peek() >= 0)
                    {
                        LineNumber++;
                        NewLine = Reader.ReadLine().Trim();

                        if (NewLine == "") continue;

                        // nothing is case-sensitive
                        NewLine = NewLine.ToUpper();

                        // if a line ends with " _" then it's continued - get the whole line
                        FullText = GetContinuedLines(Reader, NewLine, ref LineNumber);

                        // see if we recognize the command
                        NewObject = GetFromProcessActionTypeFileText(FullText);

                        // parse out the arguments
                        NewObject.GetFromFileText(FullText);

                        // add command to the list
                        this.cmdSequence.Add(NewObject);
                        ++NumFound;
                    }
                }
            }
            catch (Exception Ex)
            {
                this.cmdSequence.Clear();
                NumFound = -1;
                System.Windows.Forms.MessageBox.Show("Error in file " + FileName + " at line " + LineNumber + "\n" + Ex.Message);
            }

            return NumFound;
		}

		public int Write(string FileName)
		{
			int					CommandsWritten = 0;
			int					IndentLevel = 0;
			int					i;
			ProcessAction		ThisCmd = null;
			
			try
			{
				using (StreamWriter Writer = new StreamWriter(FileName, false))
				{
					foreach (object Obj in this.cmdSequence)
					{
						ThisCmd = Obj as ProcessAction;
						
						if (ThisCmd is Process_Else) if (IndentLevel>0) --IndentLevel;
						if (ThisCmd is Process_EndIf) if (IndentLevel>0) --IndentLevel;
						if (ThisCmd is Loop_ForEnd) if (IndentLevel>0) --IndentLevel;
						if (ThisCmd is Loop_WhileEnd) if (IndentLevel>0) --IndentLevel;
						
						foreach (string s in ThisCmd.WriteToFileText())
						{
							for(i=0; i<IndentLevel; ++i) Writer.Write("\t");
							Writer.WriteLine(s);
						}
						
						Writer.WriteLine("\n");
						++CommandsWritten;

						if (ThisCmd is Process_If) ++IndentLevel;
						if (ThisCmd is Process_Else) ++IndentLevel;
						if (ThisCmd is Loop_ForStart) ++IndentLevel;
						if (ThisCmd is Loop_WhileStart) ++IndentLevel;
					}
				}			
			}
			catch(Exception Ex)
			{
				System.Windows.Forms.MessageBox.Show("Error writing command sequence to file " + FileName + "\n" + Ex.Message);
				CommandsWritten = -1;
			}

			return CommandsWritten;
		}
				

		#region user input
        //////////////////////////////////////////////////////////////////////////////////
        //
        //
        //////////////////////////////////////////////////////////////////////////////////

		private static void GetBoolean(User_GetBoolean GB, VariableManager VM)
		{
			frmUserInput Frm = new frmUserInput();

			bool UserChoice = Frm.GetBoolean(GB.Prompt, VM.GetBooleanFromText(GB.ValueIfUserCancels));
			
			VM.SetVariable(GB.VariableName, UserChoice);
		}
	
		private static void GetDouble(User_GetDouble GD, VariableManager VM)
		{
			frmUserInput Frm = new frmUserInput();

			double UserChoice = Frm.GetDouble(GD.Prompt, VM.GetDoubleFromText(GD.MinValue), VM.GetDoubleFromText(GD.MaxValue), VM.GetDoubleFromText(GD.ValueIfUserCancels));
			
			VM.SetVariable(GD.VariableName, UserChoice);	
		}
	
		private static void GetInteger(User_GetInteger GI, VariableManager VM)
		{
			frmUserInput Frm = new frmUserInput();
            int defaultValue = -1;
            if (GI.DefaultValue != "") defaultValue = VM.GetIntFromText(GI.DefaultValue);

            int UserChoice = Frm.GetInteger(GI.Prompt, VM.GetIntFromText(GI.MinValue), VM.GetIntFromText(GI.MaxValue), VM.GetIntFromText(GI.ValueIfUserCancels), defaultValue);

			VM.SetVariable(GI.VariableName, UserChoice);	
		}


	
		private static void DoPause(Process_Pause P, VariableManager VM)
		{
			int PauseTime_ms = VM.GetIntFromText(P.PauseTime_ms);

			if (PauseTime_ms <= 0)
			{
				System.Windows.Forms.MessageBox.Show(P.UserMessage, "Pause", System.Windows.Forms.MessageBoxButtons.OK);
			}
			else
			{
				frmPause PauseForm = new frmPause();
				PauseForm.DoPause(P.UserMessage, PauseTime_ms);
			}
		}

        private static void DoWriteToConsole(Process_WriteToConsole P, VariableManager VM)
        {
            Console.WriteLine(P.TextToWrite);
        }


		#endregion user input

        #region FileIO

        private static int GetStepFromFile(User_GetStepFromFile GI, VariableManager VM)
        {
            try
            {
                string[] myLines = File.ReadAllLines(GI.FilePath);          // read all lines in memory
                int desiredStepNo = VM.GetIntFromText(GI.StepNo);
                string desiredStepNoString = desiredStepNo.ToString().Trim();
                if (desiredStepNo == 1)
                {
                    Console.WriteLine("\n\n\nGET STEP  FilePath ={0}", GI.FilePath);
                }
                for (int j = 0; j < myLines.Length; j++)
                {
                    int indexOfComma = myLines[j].IndexOf(",");
                    if (indexOfComma != -1)
                    {
                        if (desiredStepNoString == myLines[j].Substring(0, indexOfComma).Trim())
                        //int fileStep = Convert.ToInt32(myLines[j].Substring(0, indexOfComma));     // Do i need to trim ?
                        //if (fileStep == desiredStepNo)
                        {
 //                         Console.WriteLine("  Step={0}, DataLine={1}", desiredStepNoString,myLines[j]);
                            string[] parsedString = myLines[j].Split(',');
                            if (parsedString.Length<5)
                            {
                                MessageBox.Show("Not enough data found stepNo =" + desiredStepNoString, "ERROR GetStepFromFile");
                                return -1;
                            }

                            Console.Write("\n\nStep={0}", desiredStepNoString);

                            VM.SetVariable(GI.Code, Convert.ToInt32(parsedString[1]));
                            Console.Write(", Code={0}", Convert.ToInt32(parsedString[1]));

                            VM.SetVariable(GI.Row, Convert.ToInt32(parsedString[2])-1);
                            Console.Write(", Row={0}", Convert.ToInt32(parsedString[2]));

                            VM.SetVariable(GI.Col, Convert.ToInt32(parsedString[3])-1);
                            Console.WriteLine(", Col={0}", Convert.ToInt32(parsedString[3]));

                            VM.SetVariable(GI.Vol1_ul, Convert.ToDouble(parsedString[4]));
                            Console.Write(", Vol1={0}", Convert.ToDouble(parsedString[4]));

                            if ((GI.Vol1_syringe_speed != "") && (parsedString.Length > 5))
                            {
                                VM.SetVariable(GI.Vol1_syringe_speed, Convert.ToInt32(parsedString[5]));
                                Console.Write(", Sp1={0}", Convert.ToInt32(parsedString[5]));
                            }

                            if ((GI.Vol1_delay != "") && (parsedString.Length > 6))
                            {
                                VM.SetVariable(GI.Vol1_delay, Convert.ToInt32(parsedString[6]));
                                Console.WriteLine(", Dly1={0}", Convert.ToInt32(parsedString[6]));
                            }

                            if ((GI.Vol2_ul != "") && (parsedString.Length > 7))
                            {
                                VM.SetVariable(GI.Vol2_ul, Convert.ToDouble(parsedString[7]));
                                Console.Write(", Vol2={0}", Convert.ToDouble(parsedString[7]));
                            }

                            if ((GI.Vol2_syringe_speed != "") && (parsedString.Length > 8))
                            {
                                VM.SetVariable(GI.Vol2_syringe_speed, Convert.ToInt32(parsedString[8]));
                                Console.Write(", Sp2={0}", Convert.ToInt32(parsedString[8]));
                            }

                            if ((GI.Vol2_delay != "") && (parsedString.Length > 9))
                            {
                                VM.SetVariable(GI.Vol2_delay, Convert.ToInt32(parsedString[9]));
                                Console.Write(", Dly2={0}", Convert.ToInt32(parsedString[9]));
                            }

                            Console.WriteLine(" ");

                            return 0;
                        }
                    }
                }
                // This must mean the step was not found
                VM.SetVariable(GI.Code, -1);
                Console.WriteLine("Step={0}   Not found in file\n\n", desiredStepNo);
                return 0;
            }
            catch
            {
                MessageBox.Show("FATAL Error reading from file", "ERROR GetStepFromFile");
                return -1;
            }
 
        }

        // This will read integer settings out of a file.
        // R2.03

        private static int GetIntegersFromFile(User_GetIntegersFromFile GI, VariableManager VM)
        {
            Console.WriteLine("\nGET  FilePath ={0}", GI.FilePath);
            string varName = "";
            bool matchFound = false;
            int indexOfEq = -1;
            try
            {
                string[] myLines = File.ReadAllLines(GI.FilePath);          // read all lines in memory
                string[] myfileVar = new string[myLines.Length];
                string[] myfileVarTextVal = new string[myLines.Length];
                for (int j = 0; j < myLines.Length; j++)
                {
                    indexOfEq = myLines[j].IndexOf("=");
                    if (indexOfEq != -1)
                    {
                        myfileVar[j] = (myLines[j].Substring(0, indexOfEq)).Trim();
                        myfileVarTextVal[j] = (myLines[j].Substring(indexOfEq + 1)).Trim();
                    }
                    else
                    {
                        myfileVar[j] = "";
                        myfileVarTextVal[j] = "";
                    }
                }
                for (int i = 0; i < 6; i++)
                {
                    switch (i)
                    {
                        case 0: varName = GI.Int1; break;
                        case 1: varName = GI.Int2; break;
                        case 2: varName = GI.Int3; break;
                        case 3: varName = GI.Int4; break;
                        case 4: varName = GI.Int5; break;
                        case 5: varName = GI.Int6; break;
                    }
                    if (varName == "") break;   // break the for loop if no more variables re
                    matchFound = false;
                    for (int j = 0; j < myLines.Length; j++)
                    {
                        if (varName == myfileVar[j])
                        {
                            matchFound = true;
                            VM.SetVariable(varName, Convert.ToInt32(myfileVarTextVal[j]));
                            Console.WriteLine("   {0}={1}", varName, VM.GetIntFromText(varName));
                            break;
                        }
                    }
 
                    if (!matchFound)
                    {
                        MessageBox.Show("Variable " + varName + " not found in file :\n" + GI.FilePath, "Error GetIntegersFromFile");
                        return -1;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Serious Error while reading from :\n" + GI.FilePath, "Error GetIntegersFromFile");
                return -1;
            }
            return 0;
        }


        // Method will put integer settings back into a file.
        // R2.03

        private static int PutIntegersToFile(User_PutIntegersToFile GI, VariableManager VM)
        {
            Console.WriteLine("\nPUT  FilePath ={0}", GI.FilePath);
            string varName = "";
            List<string> myLines = new List<string>();

            for (int i = 0; i < 6; i++)
            {
                switch (i)
                {
                    case 0: varName = GI.Int1; break;
                    case 1: varName = GI.Int2; break;
                    case 2: varName = GI.Int3; break;
                    case 3: varName = GI.Int4; break;
                    case 4: varName = GI.Int5; break;
                    case 5: varName = GI.Int6; break;
                }
                if (varName == "") break;   // break the for loop if no more variables remain
                myLines.Add(varName + "=" + VM.GetIntFromText(varName));
                Console.WriteLine("   {0}={1}", varName, VM.GetIntFromText(varName));
            }

            try
            {
                File.WriteAllLines(GI.FilePath, myLines);
            }
            catch
            {
                MessageBox.Show("Serious Error while writing to :\n" + GI.FilePath, "Error GetIntegersFromFile");
                return -1;
            }
            return 0;
        }

        #endregion FileIO

        #region loop functions
        //////////////////////////////////////////////////////////////////////////////////
        //
        //
        //////////////////////////////////////////////////////////////////////////////////

		// initialize the counter and test the end condition - set a variable that says whether to do the loop
		public static void StartForLoop(Loop_ForStart Cmd, VariableManager VM, ref bool DoTheLoop)
		{
			VM.SetVariable(Cmd.VariableName, VM.GetIntFromText(Cmd.StartValue));

			DoTheLoop = DoForLoop(Cmd, VM);
		}

		public static bool DoForLoop(Loop_ForStart Cmd, VariableManager VM)
		{
			object	DoTheLoop = new bool();
			int		Increment = 1;
		
			if (Cmd.Increment != "") VM.GetIntFromText(Cmd.Increment);

			if (Increment > 0)
				VM.ComparisonOperation(Cmd.VariableName, Cmd.EndValue, ref DoTheLoop, VariableManager.ComparisonOperator.LessThanOrEqualTo);
			else
				VM.ComparisonOperation(Cmd.VariableName, Cmd.EndValue, ref DoTheLoop, VariableManager.ComparisonOperator.GreaterThanOrEqualTo);

			return (bool)DoTheLoop;
		}

		// increment the counter and test the end condition - set a variable that says whether to do the loop
		public static void IncrementAndCheckForLoop(Loop_ForStart Cmd, VariableManager VM, ref bool DoTheLoop)
		{
			int		Increment = 1;
			if (Cmd.Increment != "") Increment = VM.GetIntFromText(Cmd.Increment);

			object Var = Cmd.VariableName;

			VM.MathOperation(Cmd.VariableName, Increment, ref Var, VariableManager.MathOperator.Add);
			
			DoTheLoop = DoForLoop(Cmd, VM);
		}

		// does the condition = true?
		public static bool DoWhileLoop(Loop_WhileStart Cmd, VariableManager VM)
		{
			object DoTheLoop = new bool();

			VM.ComparisonOperation(Cmd.TestVariableName, Cmd.TestValue, ref DoTheLoop, VariableManager.ComparisonOperatorFromText(Cmd.ComparisonOperator));

			return (bool)DoTheLoop;
		}

		public static bool DoIf(Process_If Cmd, VariableManager VM)
		{
			object DoTheIf = new bool();	
	
			VM.ComparisonOperation(Cmd.TestVariableName, Cmd.TestValue, ref DoTheIf, VariableManager.ComparisonOperatorFromText(Cmd.ComparisonOperator));

			return (bool) DoTheIf;
		}

		public static void SkipToElseOrEndIf(ArrayList CmdSequence, ArrayList CmdStack, ref int CurrInd)
		{
			CommandStackItem	CurrentItem = (CommandStackItem)CmdStack[CmdStack.Count-1];
			CommandStackItem	StackItem = null;
			int					StartingStackCount = CmdStack.Count;
			int					SearchInd;
			ProcessAction		Cmd;
			bool				FoundIt = false;

			if (CmdStack.Count<1) throw new Exception("Error in SkipToElseOrEndIf - no commands in stack");
			if (!(CurrentItem.Command is Process_If)) throw new Exception("Error in SkipToElseOrEndIf: No If command on stack");

			// go through commands adding and removing from stack if necessary until we get to 
			// an else or endif
			for(SearchInd = CurrInd+1; SearchInd < CmdSequence.Count; ++SearchInd)
			{
				Cmd = CmdSequence[SearchInd] as ProcessAction;

				if ( (Cmd is Loop_ForStart) || (Cmd is Loop_WhileStart) || (Cmd is Process_If) )
				{
					// add it to the "stack"
					StackItem = new CommandStackItem(Cmd, SearchInd);
					CmdStack.Add(StackItem);

				}
				else if (Cmd is Process_EndIf)
				{
					// endif must have an if
					if ((CmdStack.Count < 1) || ((((CommandStackItem)CmdStack[CmdStack.Count-1]).Command is Process_If) == false)) throw new Exception(Cmd.Name + " command without corresponding If command");
						
					// pop the if off the stack
					CmdStack.RemoveAt(CmdStack.Count-1);
					
					if (CmdStack.Count < StartingStackCount)
					{
						FoundIt = true;
						break;
					}
				}
				else if (Cmd is Process_Else)
				{
					if (CmdStack.Count == StartingStackCount)
					{
						FoundIt = true;
						break;
					}
				}

				else if (Cmd is Loop_ForEnd)
				{
					// endif must have an if
					if ((CmdStack.Count < 1) || ((((CommandStackItem)CmdStack[CmdStack.Count-1]).Command is Loop_ForStart) == false)) throw new Exception(Cmd.Name + " command without corresponding For loop start");
						
					// pop the if off the stack
					CmdStack.RemoveAt(CmdStack.Count-1);
				}
				else if (Cmd is Loop_WhileEnd) 
				{
					// endif must have an if
					if ((CmdStack.Count < 1) || ((((CommandStackItem)CmdStack[CmdStack.Count-1]).Command is Loop_WhileStart) == false)) throw new Exception(Cmd.Name + " command without corresponding While loop start");
						
					// pop the if off the stack
					CmdStack.RemoveAt(CmdStack.Count-1);
				}
			}

			if (!FoundIt) throw new Exception("Error finding Else or EndIf for If command at step " + (CurrentItem.CommandNumber + 1));

			// pass back the current index, continue at the step after the else or endif
			CurrInd = SearchInd+1;		
		}

		public static void SkipToEndOfCurrentStackItem(ArrayList CmdSequence, ArrayList CmdStack, ref int CurrInd)
		{
			CommandStackItem	CurrentItem = (CommandStackItem)CmdStack[CmdStack.Count-1];
			CommandStackItem	StackItem = null;
			int					StartingStackCount = CmdStack.Count;
			int					SearchInd;
			ProcessAction		Cmd;
			
			if (CmdStack.Count<1) throw new Exception("Error in SkipToEndOfCurrentStackItem - no commands in stack");

			// go through commands adding and removing from stack if necessary until we get to 
			// an ending item that corresponds to our starting item
			for(SearchInd = CurrInd+1; SearchInd < CmdSequence.Count; ++SearchInd)
			{
				Cmd = CmdSequence[SearchInd] as ProcessAction;

				if ( (Cmd is Loop_ForStart) || (Cmd is Loop_WhileStart) || (Cmd is Process_If) )
				{
					// add it to the "stack"
					StackItem = new CommandStackItem(Cmd, SearchInd);
					CmdStack.Add(StackItem);

				}
				else if (Cmd is Process_EndIf)
				{
					// endif must have an if
					if ((CmdStack.Count < 1) || ((((CommandStackItem)CmdStack[CmdStack.Count-1]).Command is Process_If) == false)) throw new Exception(Cmd.Name + " command without corresponding If command");
						
					// pop the if off the stack
					CmdStack.RemoveAt(CmdStack.Count-1);
					if (CmdStack.Count < StartingStackCount) break;
				}
				else if (Cmd is Loop_ForEnd)
				{
					// endif must have an if
					if ((CmdStack.Count < 1) || ((((CommandStackItem)CmdStack[CmdStack.Count-1]).Command is Loop_ForStart) == false)) throw new Exception(Cmd.Name + " command without corresponding For loop start");
						
					// pop the if off the stack
					CmdStack.RemoveAt(CmdStack.Count-1);
					if (CmdStack.Count < StartingStackCount) break;
				}
				else if (Cmd is Loop_WhileEnd) 
				{
					// endif must have an if
					if ((CmdStack.Count < 1) || ((((CommandStackItem)CmdStack[CmdStack.Count-1]).Command is Loop_WhileStart) == false)) throw new Exception(Cmd.Name + " command without corresponding While loop start");
						
					// pop the if off the stack
					CmdStack.RemoveAt(CmdStack.Count-1);
					if (CmdStack.Count < StartingStackCount) break;
				}
			}

			if (CmdStack.Count != StartingStackCount-1) throw new Exception("Error finding end of " + CurrentItem.Command.Name + " command at step " + (CurrentItem.CommandNumber + 1));

			// pass back the current index, continue at the step after the ending command
			CurrInd = SearchInd+1;
		}
		
		#endregion loop functions



		public static void GetUserVariables(ArrayList CmdList, VariableManager VM)
		{
			ProcessAction Cmd = null;

			try
			{
				foreach(object O in CmdList)
				{
					Cmd = O as ProcessAction;
				
					if ((Cmd is Variable_Define) || (Cmd is Variable_DefinePoint) || (Cmd is Variable_DefineGrid))
					{
						DefineVariable(Cmd, VM);
					}
				}
			}
			catch (Exception Ex)
			{
				System.Windows.Forms.MessageBox.Show("Error getting user variables from command sequence\n" + Ex.Message);
			}
		}

        public static void PreloadImportVariables(ArrayList CmdList, VariableManager VM)    // [LEA COMMAND DEF]
        {
            ProcessAction Cmd = null;

            try
            {
                foreach (object obj in CmdList)
                {
                    Cmd = obj as ProcessAction;
                    if(Cmd is Variable_DefineImport)
                        ReadImportVariables( (Variable_DefineImport)Cmd, VM);
                }
            }
            catch (Exception Ex)
            {
                System.Windows.Forms.MessageBox.Show("Error getting user variables from command sequence\n" + Ex.Message);
            }
        }

        public static void ReadImportVariables(Variable_DefineImport importAction, VariableManager vm) // [LEA COMMAND DEF]
        {
            string importFile = importAction.FileName;

            // Lets load all the defines in the external file into
            // a new command list.
            ArrayList importList = new ArrayList();
            SequenceFile tempSeqFile = new SequenceFile();
            VariableManager tempVM = new VariableManager();

            if (tempSeqFile.Read(importFile, importList, tempVM) != -1)
            {
                // Use the new command list to get only those define commands
                // and add them to the global variable list.
                SequenceFile.GetUserVariables(importList, vm);
            }
        }
		
		public static void VariableOperation(ProcessAction PA, VariableManager VM)
		{
			if (PA is Variable_MathOperation)
			{
				Variable_MathOperation Cmd = PA as Variable_MathOperation;
				object OutVar = Cmd.ResultVariable;
				VM.MathOperation(Cmd.LHS, Cmd.RHS, ref OutVar, VariableManager.MathOperatorFromText(Cmd.MathOperator));
			}
			else if (PA is Variable_CompareOperation)
			{
				Variable_CompareOperation Cmd = PA as Variable_CompareOperation;
				object OutVar = Cmd.ResultVariable;
				VM.ComparisonOperation(Cmd.LHS, Cmd.RHS, ref OutVar, VariableManager.ComparisonOperatorFromText(Cmd.CompareOperator));
			}
			else if (PA is Variable_Set)
			{
				Variable_Set Cmd = PA as Variable_Set;

				if (VM.VariableDefined(Cmd.VariableName) == false) throw new Exception("Variable '" + Cmd.VariableName + "' is not defined");
				
				System.Type T = VM.VariableType(Cmd.VariableName);

				if (T == typeof(int))
				{
					VM.SetVariable(Cmd.VariableName, VM.GetIntFromText(Cmd.VariableValue));
				}
				else if (T == typeof(double))
				{
					VM.SetVariable(Cmd.VariableName, VM.GetDoubleFromText(Cmd.VariableValue));
				}
				else if (T == typeof(bool))
				{
					VM.SetVariable(Cmd.VariableName, VM.GetBooleanFromText(Cmd.VariableValue));
				}
				else
				{
					throw new Exception("Invalid variable type for " + Cmd.Name + " command - must be int, double, or bool.");
				}
			}
		}
        		
		public static void DefineVariable(ProcessAction PA, VariableManager VM)
		{
			if (PA is Variable_Define) 
			{
				Variable_Define Cmd = PA as Variable_Define;

				switch (Cmd.VariableType.ToUpper())
				{
					case ("INT"):
						VM.DefineVariable(Cmd.VariableName, VM.GetIntFromText(Cmd.VariableValue));
						break;
					case ("BOOL"):
						VM.DefineVariable(Cmd.VariableName, VM.GetBooleanFromText(Cmd.VariableValue));
						break;
					case ("DOUBLE"):
						VM.DefineVariable(Cmd.VariableName, VM.GetDoubleFromText(Cmd.VariableValue));
						break;
					default:
						throw new Exception("Unsupported variable type in " + Cmd.Name + " command - must be int, bool, or double.");
				}
			}
			else if (PA is Variable_DefinePoint)
			{
				Variable_DefinePoint Cmd = PA as Variable_DefinePoint;

				MachineCoordinate Pt = new MachineCoordinate();
				Pt.Name = Cmd.VariableName;
				Pt.X = VM.GetDoubleFromText(Cmd.X);
				Pt.Y = VM.GetDoubleFromText(Cmd.Y);
				Pt.Z = VM.GetDoubleFromText(Cmd.Z);
				
				VM.DefineVariable(Cmd.VariableName, Pt);
			}
			else if (PA is Variable_DefineGrid)
			{
				Variable_DefineGrid Cmd = PA as Variable_DefineGrid;

				CoordinateMatrix Grd = new CoordinateMatrix();
				Grd.Name = Cmd.VariableName;
				Grd.X = VM.GetDoubleFromText(Cmd.X);
				Grd.Y = VM.GetDoubleFromText(Cmd.Y);
				Grd.Z = VM.GetDoubleFromText(Cmd.Z);
				Grd.XSteps = VM.GetIntFromText(Cmd.XSteps);
				Grd.YSteps = VM.GetIntFromText(Cmd.YSteps);
				Grd.XStepSize = VM.GetDoubleFromText(Cmd.XIncrement);
				Grd.YStepSize = VM.GetDoubleFromText(Cmd.YIncrement);

				VM.DefineVariable(Cmd.VariableName, Grd);
			}
			else
			{
				throw new Exception("Error in DefneVariable - command type " + PA.Name + " cannot be used to define a variable.");
			}
		}
        
		public static void AssignDefaultParameters(ProcessAction Cmd)
		{
			if (Cmd == null) return;

			if (Cmd is Process_Wash)
			{
//				((Process_Wash)Cmd).Tip1Point = SystemVariableNames.TipWashPoint;
				((Process_Wash)Cmd).SyringeMask = "0";
			}
			else if (Cmd is Process_Prime)
			{
//				((Process_Prime)Cmd).Tip1Point = SystemVariableNames.TipWashPoint;
				((Process_Prime)Cmd).SyringeMask = "0";
			}
			else if (Cmd is Process_Aspirate)
			{
//				((Process_Aspirate)Cmd).Tip1Point = SystemVariableNames.MicrotiterPoint + "[0,0]";
				((Process_Aspirate)Cmd).SyringeMask = "0";
			}
			else if (Cmd is Process_Dispense)
			{
				((Process_Dispense)Cmd).SyringeMask = "1";
			}
			else if (Cmd is Process_SetSyringeValvePosition)
			{
				((Process_SetSyringeValvePosition)Cmd).SyringeMask = "0";
				((Process_SetSyringeValvePosition)Cmd).ValvePosition = "2";
			}
			else if (Cmd is Process_SyringeMove)
			{
				((Process_SyringeMove)Cmd).SyringeMask = "0";
				((Process_SyringeMove)Cmd).SyringePosition = "0";
			}
			else if (Cmd is Process_SyringeEmpty)
			{
				((Process_SyringeEmpty)Cmd).SyringeMask = "0";
			}
			else if (Cmd is Process_EnableController)
			{
				((Process_EnableController)Cmd).EnableController = "True";
			}
			else if (Cmd is Process_InspectTipFiring)
			{
				((Process_InspectTipFiring)Cmd).Tip1Point = SystemVariableNames.SideCamInspectPoint;
				((Process_InspectTipFiring)Cmd).TipMask = "1";
			}
			else if (Cmd is Process_PiezoDispense)
			{
				((Process_PiezoDispense)Cmd).Tip = "1";
//				((Process_PiezoDispense)Cmd).Tip1Point = SystemVariableNames.DeckOriginPoint;
				((Process_PiezoDispense)Cmd).ZOffset = "5.0";
			}
			else if (Cmd is Process_InitializeMotion)
			{}
			else if (Cmd is Process_HomeAxis)
			{
				((Process_HomeAxis)Cmd).AxisNumber = "2";
			}
			else if (Cmd is Process_MoveRelative)
			{}
			else if (Cmd is Process_MoveToSafeHeight)
			{}
			else if (Cmd is Process_MoveAbovePoint)
			{
//				((Process_MoveAbovePoint)Cmd).Point = SystemVariableNames.TipWashPoint;
			}
			else if (Cmd is Process_MoveToPoint)
			{
				((Process_MoveToPoint)Cmd).Point = SystemVariableNames.SafePoint;
				((Process_MoveToPoint)Cmd).MoveToSafeHeightFirst = "true";
//				((Process_MoveToPoint)Cmd).ZOffset = "5.0";
			}
			else if (Cmd is Process_MoveAbovePoint)
			{
				((Process_MoveAbovePoint)Cmd).Point = SystemVariableNames.SafePoint;
			}
		}



        #region logfile
        //////////////////////////////////////////////////////////////////////////////////
        //
        //
        //////////////////////////////////////////////////////////////////////////////////

        private void LogToFile(string Msg)
        {
            try
            {
                Datalog log = new Datalog(this.LogFile);
                log.WriteLine(Msg);
                log.Close();
            }
            catch { }
        }

        private void LogToFile(StringBuilder Msg)
        {
            try
            {
                Datalog log = new Datalog(this.LogFile);
                log.WriteLine(Msg);
                log.Close();
            }
            catch { }
        }

        private void LogToFile(string[] Msg)
        {
            try
            {
                Datalog log = new Datalog(this.LogFile);
                log.WriteLine(Msg);
                log.Close();
            }
            catch { }
        }

        private void LogToFile(string Format, params object[] Arg)
        {
            try
            {
                Datalog log = new Datalog(this.LogFile);
                log.WriteLine(Format, Arg);
                log.Close();
            }
            catch { }
        }
        
        #endregion logfile

    }

}
