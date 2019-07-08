using System;


namespace EA.PixyControl.ClassLibrary
{
    public class Process_InitializeMotion : ProcessAction
    {
        public override void GetFromFileText(string FileText)
        {
            SequenceFile.GetProcessActionFromFileText((ProcessAction)this, FileText);
        }

        public override string[] WriteToFileText()
        {
            return SequenceFile.GetFileTextFromProcessAction((ProcessAction)this);
        }

        public override void Clear()
        { }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_InitializeMotion() : base("Motion Initialize", "Enable and home motion system", 0, true, SequenceFile.CommandNames.MotionInitialize) { Clear(); }
    }
    
    public class Process_HomeAxis : ProcessAction
    {
        #region members

        private string axisNumber;

        [ProcessActionArgument(typeof(int), true, "0, 1, or 2")]
        public string AxisNumber
        {
            get { return axisNumber; }
            set { axisNumber = value; }
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
            axisNumber = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_HomeAxis() : base("Home Axis", "Move single axis to home position", 0, true, SequenceFile.CommandNames.HomeAxis) { Clear(); }

    }

    public class Process_ServoEnable : ProcessAction
    {
        #region members

        private string axisNumber;

        [ProcessActionArgument(typeof(int), true, "0, 1, 2, or 3")]
        public string AxisNumber
        {
            get { return axisNumber; }
            set { axisNumber = value; }
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
            axisNumber = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_ServoEnable() : base("Servo Enable", "Enable Servo of Axis", 0, true, SequenceFile.CommandNames.ServoEnable) { Clear(); }

    }

    public class Process_ServoDisable : ProcessAction
    {
        #region members

        private string axisNumber;

        [ProcessActionArgument(typeof(int), true, "0, 1, 2 or 3")]
        public string AxisNumber
        {
            get { return axisNumber; }
            set { axisNumber = value; }
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
            axisNumber = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_ServoDisable() : base("Servo Disable", "Disable Servo of Axis", 0, true, SequenceFile.CommandNames.ServoDisable) { Clear(); }

    }
    
    public class Process_MoveRelative : ProcessAction
    {
        #region members

        private string xOffset;
        private string yOffset;
        private string zOffset;
        private string speed_pct;
        private string useGridRobot;

        [ProcessActionArgument(typeof(double), true)]
        public string XOffset
        {
            get { return xOffset; }
            set { xOffset = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string YOffset
        {
            get { return yOffset; }
            set { yOffset = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string ZOffset
        {
            get { return zOffset; }
            set { zOffset = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string Speed_pct
        {
            get { return speed_pct; }
            set { speed_pct = value; }
        }

        [ProcessActionArgument(typeof(bool), false)]
        public string UseGridRobot
        {
            get { return useGridRobot; }
            set { useGridRobot = value; }
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
            xOffset = "";
            yOffset = "";
            zOffset = "";
            speed_pct = "";
            useGridRobot = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_MoveRelative() : base("Move Relative", "Move some offset from the current position", 0, true, SequenceFile.CommandNames.MoveRelative) { Clear(); }

    }
    
    public class Process_MoveToSafeHeight : ProcessAction
    {
        #region members

        private string speed_pct;
        private string useGridRobot;

        [ProcessActionArgument(typeof(double), false)]
        public string Speed_pct
        {
            get { return speed_pct; }
            set { speed_pct = value; }
        }

        [ProcessActionArgument(typeof(bool), false)]
        public string UseGridRobot
        {
            get { return useGridRobot; }
            set { useGridRobot = value; }
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
            speed_pct = "";
            useGridRobot = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_MoveToSafeHeight() : base("Move To Safe Height", "Move tips up if below safe height", 0, true, SequenceFile.CommandNames.MoveToSafeHeight) { Clear(); }

    }
    
    // PKv4.0,2015-04-07
    public class Process_MoveToPoint : ProcessAction
    {
        #region members

        private string tool;			// tip 1, etc.
        private string point;			// wash, mt, etc.
        private string xOffset;
        private string yOffset;
        private string zOffset;
        private string speed_pct;
        private string moveToSafeHeightFirst;
        private string useGridRobot;

        [ProcessActionArgument(typeof(MachineCoordinate), true)]
        public string Tool
        {
            get { return tool; }
            set { tool = value; }
        }

        [ProcessActionArgument(typeof(MachineCoordinate), true)]
        public string Point
        {
            get { return point; }
            set { point = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string XOffset
        {
            get { return xOffset; }
            set { xOffset = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string YOffset
        {
            get { return yOffset; }
            set { yOffset = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string ZOffset
        {
            get { return zOffset; }
            set { zOffset = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string Speed_pct
        {
            get { return speed_pct; }
            set { speed_pct = value; }
        }

        [ProcessActionArgument(typeof(bool), true)]
        public string MoveToSafeHeightFirst
        {
            get { return moveToSafeHeightFirst; }
            set { moveToSafeHeightFirst = value; }
        }

        [ProcessActionArgument(typeof(bool), false)]
        public string UseGridRobot
        {
            get { return useGridRobot; }
            set { useGridRobot = value; }
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
            tool = "";
            point = "";
            xOffset = "";
            yOffset = "";
            zOffset = "";
            speed_pct = "";
            moveToSafeHeightFirst = "";
            useGridRobot = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_MoveToPoint() : base("Move To Point", "Move to a selected location", ProcessAction.IMG_MOVE, true, SequenceFile.CommandNames.MoveToPoint) { Clear(); }

    }

    public class Process_PlungeAxisMove : ProcessAction
    {
        #region members

        private string positionZ2;		// position of the axis
        private string zOffset;         // Offset added to the position   
        private string speed_pct;

 

        [ProcessActionArgument(typeof(double), true)]
        public string PositionZ2
        {
            get { return positionZ2; }
            set { positionZ2 = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string ZOffset
        {
            get { return zOffset; }
            set { zOffset = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string Speed_pct
        {
            get { return speed_pct; }
            set { speed_pct = value; }
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
            positionZ2 = "";
            zOffset = "";
            speed_pct = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }
        // TODO  Are we OK with IMG_MOVE ?
        public Process_PlungeAxisMove() : base("Move the plunge axis only", "Move to a selected position", ProcessAction.IMG_MOVE, true, SequenceFile.CommandNames.PlungeAxisMove) { Clear(); }

    }

    public class Process_SuperPlunge : ProcessAction
    {
        #region members

        private string startSolenoid;	// Output to fire (-999) means none
        private string startSensor;     // Input to look for to start plunge plunge servo
        private string endSensor;     // Input to look for to start plunge plunge servo
        private string positionZ2;		// position of the plunge axis moving to.
        private string speed_pct;

        [ProcessActionArgument(typeof(int), true,"Output number that will turn on at start of command, pnuematic ethane output(-999=none)")]
        public string StartSolenoid
        {
            get { return startSolenoid; }
            set { startSolenoid = value; }
        }

       [ProcessActionArgument(typeof(int), true,"Sensor state to look for to trigger servo plunge axis motion (-999=none)")]
        public string StartSensor
        {
            get { return startSensor; }
            set { startSensor = value; }
        }

        [ProcessActionArgument(typeof(int), true, "Sensor state to look for at end of pnuematic ethance axis travel (-999=none)")]
        public string EndSensor
        {
            get { return endSensor; }
            set { endSensor = value; }
        }

        [ProcessActionArgument(typeof(double), false,"Position to move plunge axis")]
        public string PositionZ2
        {
            get { return positionZ2; }
            set { positionZ2 = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string Speed_pct
        {
            get { return speed_pct; }
            set { speed_pct = value; }
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
            startSolenoid = "";
            startSensor = "";
            endSensor = "";
            positionZ2 = "";
            speed_pct = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }
        // TODO  Are we OK with IMG_MOVE ?
        public Process_SuperPlunge() : base("Move the plunge axis synchonized with ethane pnuematic", "Move to a selected position", ProcessAction.IMG_MOVE, true, SequenceFile.CommandNames.SuperPlunge) { Clear(); }

    }


  
    public class Process_SetMaxVelocity : ProcessAction
    {
        #region members

        private string axisNumber;
        private string maxVelocity;	    // mm per sec
        private string useGridRobot;

        [ProcessActionArgument(typeof(int), true, "0, 1, or 2")]
        public string AxisNumber
        {
            get { return axisNumber; }
            set { axisNumber = value; }
        }

        [ProcessActionArgument(typeof(double), true, "mm Per Sec")]
        public string MaxVelocity
        {
            get { return maxVelocity; }
            set { maxVelocity = value; }
        }

        [ProcessActionArgument(typeof(bool), false, "Use the grid robot, default if not included to false")]
        public string UseGridRobot
        {
            get { return useGridRobot; }
            set { useGridRobot = value; }
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
            axisNumber = "";
            maxVelocity = "";
            useGridRobot = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_SetMaxVelocity() : base("Set Max Velocity", "Set max velocity", ProcessAction.IMG_MOVE, true, SequenceFile.CommandNames.SetMaxVelocity) { Clear(); }

    }

    public class Process_SetAccel : ProcessAction
    {
        #region members

        private string axisNumber;
        private string accel;	    // mm  per sec-sec
        private string decel;	    // mm  per sec-sec
        private string useGridRobot;


        [ProcessActionArgument(typeof(int), true, "0, 1, or 2")]
        public string AxisNumber
        {
            get { return axisNumber; }
            set { axisNumber = value; }
        }

        [ProcessActionArgument(typeof(double), true, "mm  Per Sec Sec")]
        public string Accel
        {
            get { return accel; }
            set { accel = value; }
        }

        [ProcessActionArgument(typeof(double), false, "mm  Per Sec Sec, optional if not included set same as accel")]
        public string Decel
        {
            get { return decel; }
            set { decel = value; }
        }

        [ProcessActionArgument(typeof(bool), false, "Use the grid robot, default if not included to false")]
        public string UseGridRobot
        {
            get { return useGridRobot; }
            set { useGridRobot = value; }
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
            accel = "";
            decel = "";
            useGridRobot = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_SetAccel() : base("Set Accel", "Set Accel of an axis", ProcessAction.IMG_MOVE, true, SequenceFile.CommandNames.SetAccel) { Clear(); }

    }

    public class Process_SetDefaultMotionParameters : ProcessAction
    {
        #region members

        private string axisNumber;
        private string useGridRobot;


        [ProcessActionArgument(typeof(int), false, "0, 1, or 2,   if not included all 3 axis will be set to default values")]
        public string AxisNumber
        {
            get { return axisNumber; }
            set { axisNumber = value; }
        }

        [ProcessActionArgument(typeof(bool), false, "Use the grid robot, default if not included is false")]
        public string UseGridRobot
        {
            get { return useGridRobot; }
            set { useGridRobot = value; }
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
            axisNumber = "";
            useGridRobot = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_SetDefaultMotionParameters() : base("Set Default Motion Parameters", "Sets defulat motion parameters from xml file", ProcessAction.IMG_MOVE, true, SequenceFile.CommandNames.SetDefaultMotionParameters) { Clear(); }

    }

       public class Process_MoveAbovePoint : ProcessAction
    {
        #region members

        private string tool;			// tip 1, etc.
        private string point;			// wash, mt, etc.
        private string xOffset;
        private string yOffset;
        private string speed_pct;
        private string useGridRobot;

        [ProcessActionArgument(typeof(MachineCoordinate), true)]
        public string Tool
        {
            get { return tool; }
            set { tool = value; }
        }

        [ProcessActionArgument(typeof(MachineCoordinate), true)]
        public string Point
        {
            get { return point; }
            set { point = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string XOffset
        {
            get { return xOffset; }
            set { xOffset = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string YOffset
        {
            get { return yOffset; }
            set { yOffset = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string Speed_pct
        {
            get { return speed_pct; }
            set { speed_pct = value; }
        }

        [ProcessActionArgument(typeof(bool), false)]
        public string UseGridRobot
        {
            get { return useGridRobot; }
            set { useGridRobot = value; }
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
            tool = "";
            point = "";
            xOffset = "";
            yOffset = "";
            speed_pct = "";
            useGridRobot = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_MoveAbovePoint() : base("Move Above Point", "Move to a selected location", ProcessAction.IMG_MOVE, true, SequenceFile.CommandNames.MoveAbovePoint) { Clear(); }

    }

       public class Process_RotateMove : ProcessAction       //  PKv4.0,2015-04-13,  Added some commands for driving the harmonic drive rotating axis.
       {
           #region members

           private string position_count;	    // position that you wish to move

           [ProcessActionArgument(typeof(int), true, "Move to this position counts, -100,000 is quarter turn clockwise")]
           public string PositionCount
           {
               get { return position_count; }
               set { position_count = value; }
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
               position_count = "";
           }

           public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
           {
               return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
           }

           public Process_RotateMove() : base("Rotate Move", "Move the rotation axis", ProcessAction.IMG_MOVE, true, SequenceFile.CommandNames.RotateMove) { Clear(); }

       }

       public class Process_RotateSetup : ProcessAction
       {
           #region members

           private string accel_count;	    // Acceleration Counts is 10X this Parameter
           private string decel_count;	    // Decel Counts is 10X this Parameter
           private string max_velocity_count;     // Max Velocity Counts is 0.1X this parameter

           [ProcessActionArgument(typeof(int), true, "Accel count is 10x this, 25,000 is reasonable")]
           public string AccelCount
           {
               get { return accel_count; }
               set { accel_count = value; }
           }

           [ProcessActionArgument(typeof(int), true, "DECEL count is 10x this, 25,000 is reasonable")]
           public string DecelCount
           {
               get { return decel_count; }
               set { decel_count = value; }
           }

          [ProcessActionArgument(typeof(int), true, "Max Velocity count is 0.1x this, 1,000,000 is reasonable")]
           public string MaxVelCount
           {
               get { return max_velocity_count; }
               set { max_velocity_count = value; }
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
               accel_count = "";
               decel_count = "";
               max_velocity_count = "";
           }

           public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
           {
               return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
           }

           public Process_RotateSetup() : base("Rotate Setup", "Set Rotate Axis Parameters", ProcessAction.IMG_MOVE, true, SequenceFile.CommandNames.RotateSetup) { Clear(); }

       }
}