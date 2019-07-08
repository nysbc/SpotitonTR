using System;


namespace EA.PixyControl.ClassLibrary
{
    public class Process_Prime : ProcessAction
    {
        #region members

        private string syringeMask;				// bitmask for which syringe
        private string volumePerStroke_uL;
        private string numSyringeStrokes;
        private string syringeSpeedPull;
        private string syringeSpeedPush;
        private string delayAfter_ms;
        private string tip1Point;
        private string xOffset;
        private string yOffset;
        private string zOffset;
        private string moveSpeedIn_pct;
        private string moveSpeedOut_pct;

        [ProcessActionArgument(typeof(int), true, "0=Pump1, 1=Pump2")]
        public string SyringeMask				// bitmask for which syringe
        {
            get { return syringeMask; }
            set { syringeMask = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string VolumePerStroke_uL
        {
            get { return volumePerStroke_uL; }
            set { volumePerStroke_uL = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string NumSyringeStrokes
        {
            get { return numSyringeStrokes; }
            set { numSyringeStrokes = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string SyringeSpeedPull
        {
            get { return syringeSpeedPull; }
            set { syringeSpeedPull = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string SyringeSpeedPush
        {
            get { return syringeSpeedPush; }
            set { syringeSpeedPush = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string DelayAfter_ms
        {
            get { return delayAfter_ms; }
            set { delayAfter_ms = value; }
        }

        [ProcessActionArgument(typeof(MachineCoordinate), false)]
        public string Tip1Point
        {
            get { return tip1Point; }
            set { tip1Point = value; }
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
        public string MoveSpeedIn_pct
        {
            get { return moveSpeedIn_pct; }
            set { moveSpeedIn_pct = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string MoveSpeedOut_pct
        {
            get { return moveSpeedOut_pct; }
            set { moveSpeedOut_pct = value; }
        }

        #endregion

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
            syringeMask = "";
            volumePerStroke_uL = "";
            numSyringeStrokes = "";
            syringeSpeedPull = "";
            syringeSpeedPush = "";
            delayAfter_ms = "";
            tip1Point = "";
            xOffset = "";
            yOffset = "";
            zOffset = "";
            moveSpeedIn_pct = "";
            moveSpeedOut_pct = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }


        public Process_Prime() : base("Prime", "Remove air from the system", ProcessAction.IMG_PRIME, true, SequenceFile.CommandNames.Prime) { Clear(); }
    }

    public class Process_Aspirate : ProcessAction
    {

        #region members

        private string syringeMask;		// bitmask for which syringe
        private string volume_uL;
        private string syringeSpeed;
		private string valveToBypass;
		private string aspFromReservoir;
        private string delayAfter_ms;
        private string tip1Point;
        private string xOffset;
        private string yOffset;
        private string zOffset;
        private string moveSpeedIn_pct;
        private string moveSpeedOut_pct;

        [ProcessActionArgument(typeof(int), true, "0=Pump1, 1=Pump2")]
        public string SyringeMask
        {
            get { return syringeMask; }
            set { syringeMask = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string Volume_uL
        {
            get { return volume_uL; }
            set { volume_uL = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string SyringeSpeed
        {
            get { return syringeSpeed; }
            set { syringeSpeed = value; }
        }

		[ProcessActionArgument(typeof(bool), false)]
		public string ValveToBypass
		{
			get { return valveToBypass; }
			set { valveToBypass = value; }
		}

		[ProcessActionArgument(typeof(bool), false)]
		public string AspFromReservoir
		{
			get { return aspFromReservoir; }
			set { aspFromReservoir = value; }
		}

        [ProcessActionArgument(typeof(int), false)]
        public string DelayAfter_ms
        {
            get { return delayAfter_ms; }
            set { delayAfter_ms = value; }
        }

        [ProcessActionArgument(typeof(MachineCoordinate), false)]
        public string Tip1Point
        {
            get { return tip1Point; }
            set { tip1Point = value; }
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
        public string MoveSpeedIn_pct
        {
            get { return moveSpeedIn_pct; }
            set { moveSpeedIn_pct = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string MoveSpeedOut_pct
        {
            get { return moveSpeedOut_pct; }
            set { moveSpeedOut_pct = value; }
        }

        #endregion

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
            syringeMask = "";
            volume_uL = "";
            syringeSpeed = "";
			valveToBypass = "";
			aspFromReservoir = "";
            delayAfter_ms = "";
            tip1Point = "";
            xOffset = "";
            yOffset = "";
            zOffset = "";
            moveSpeedIn_pct = "";
            moveSpeedOut_pct = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }


        public Process_Aspirate() : base("Aspirate", "Pull fluid into tip using syringe", ProcessAction.IMG_ASPIRATE, true, SequenceFile.CommandNames.Aspirate) { Clear(); }

    }

    public class Process_Dispense : ProcessAction
    {

        #region members

        private string syringeMask;		// bitmask for which syringe
        private string volume_uL;
        private string syringeSpeed;
		private string valveToBypass;
        private string delayAfter_ms;
        private string tip1Point;
        private string xOffset;
        private string yOffset;
        private string zOffset;
        private string moveSpeedIn_pct;
        private string moveSpeedOut_pct;

        [ProcessActionArgument(typeof(int), true, "0=Pump1, 1=Pump2")]
        public string SyringeMask
        {
            get { return syringeMask; }
            set { syringeMask = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string Volume_uL
        {
            get { return volume_uL; }
            set { volume_uL = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string SyringeSpeed
        {
            get { return syringeSpeed; }
            set { syringeSpeed = value; }
        }

		[ProcessActionArgument(typeof(bool), false)]
		public string ValveToBypass
		{
			get { return valveToBypass; }
			set { valveToBypass = value; }
		}

        [ProcessActionArgument(typeof(int), false)]
        public string DelayAfter_ms
        {
            get { return delayAfter_ms; }
            set { delayAfter_ms = value; }
        }

        [ProcessActionArgument(typeof(MachineCoordinate), false)]
        public string Tip1Point
        {
            get { return tip1Point; }
            set { tip1Point = value; }
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
        public string MoveSpeedIn_pct
        {
            get { return moveSpeedIn_pct; }
            set { moveSpeedIn_pct = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string MoveSpeedOut_pct
        {
            get { return moveSpeedOut_pct; }
            set { moveSpeedOut_pct = value; }
        }

        #endregion

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
            syringeMask = "";
            volume_uL = "";
            syringeSpeed = "";
			valveToBypass = "";
            delayAfter_ms = "";
            tip1Point = "";
            xOffset = "";
            yOffset = "";
            zOffset = "";
            moveSpeedIn_pct = "";
            moveSpeedOut_pct = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_Dispense() : base("Dispense", "Push fluid out of tip usinge syringe", ProcessAction.IMG_ASPIRATE, true, SequenceFile.CommandNames.Dispense) { Clear(); }
    }
    
    public class Process_SetSyringeValvePosition : ProcessAction
    {
        #region members

        private string syringeMask;
        private string valvePosition;
        private string delayAfter_ms;

        [ProcessActionArgument(typeof(int), true, "0=Pump1, 1=Pump2")]
        public string SyringeMask
        {
            get { return syringeMask; }
            set { syringeMask = value; }
        }

        [ProcessActionArgument(typeof(int), true, "0=Input (Reservoir), 1=Output (tip), 2=Bypass")]
        public string ValvePosition
        {
            get { return valvePosition; }
            set { valvePosition = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string DelayAfter_ms
        {
            get { return delayAfter_ms; }
            set { delayAfter_ms = value; }
        }

        #endregion

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
            syringeMask = "";
            valvePosition = "";
            delayAfter_ms = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_SetSyringeValvePosition() : base("Syringe Valve Position", "Change position of syringe valve", ProcessAction.IMG_ASPIRATE, true, SequenceFile.CommandNames.SyringeSetValve) { Clear(); }
    }

    public class Process_SyringeMove : ProcessAction
    {
        #region members

        private string syringeMask;
        private string syringePosition;
        private string syringeSpeed;
        private string delayAfter_ms;

        [ProcessActionArgument(typeof(int), true, "0=Pump1, 1=Pump2")]
        public string SyringeMask
        {
            get { return syringeMask; }
            set { syringeMask = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string SyringePosition
        {
            get { return syringePosition; }
            set { syringePosition = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string SyringeSpeed
        {
            get { return syringeSpeed; }
            set { syringeSpeed = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string DelayAfter_ms
        {
            get { return delayAfter_ms; }
            set { delayAfter_ms = value; }
        }

        #endregion

        public override void GetFromFileText(string FileText)
        {
            SequenceFile.GetProcessActionFromFileText((ProcessAction)this, FileText);
        }

        public override string[] WriteToFileText()
        {
            return SequenceFile.GetFileTextFromProcessAction((ProcessAction)this);
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public override void Clear()
        {
            syringeMask = "";
            syringePosition = "";
            syringeSpeed = "";
            delayAfter_ms = "";
        }

        public Process_SyringeMove() : base("Syringe Move", "Move syringe to specified position", ProcessAction.IMG_ASPIRATE, true, SequenceFile.CommandNames.SyringeMove) { Clear(); }

    }

    public class Process_SyringeEmpty : ProcessAction
    {
        #region members

        public string syringeMask;
        public string syringeSpeed;
        public string emptyThroughTip;
        public string delayAfter_ms;

        [ProcessActionArgument(typeof(int), true, "0=Pump1, 1=Pump2")]
        public string SyringeMask				// bitmask for which syringe
        {
            get { return syringeMask; }
            set { syringeMask = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string SyringeSpeed
        {
            get { return syringeSpeed; }
            set { syringeSpeed = value; }
        }

        [ProcessActionArgument(typeof(bool), false)]
        public string EmptyThroughTip
        {
            get { return emptyThroughTip; }
            set { emptyThroughTip = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string DelayAfter_ms
        {
            get { return delayAfter_ms; }
            set { delayAfter_ms = value; }
        }

        #endregion

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
            syringeMask = "";
            syringeSpeed = "";
            emptyThroughTip = "";
            delayAfter_ms = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_SyringeEmpty() : base("Syringe Empty", "Empty a syringe", 0, true, SequenceFile.CommandNames.SyringeEmpty) { Clear(); }
    }

    public class Process_Wash : ProcessAction
    {
        #region members

        private string syringeMask;
        private string volumePerStroke_uL;
        private string numSyringeStrokes;
        private string syringeSpeedPull;
        private string syringeSpeedPush;
        private string delayAfter_ms;
        private string tip1Point;
        private string xOffset;
        private string yOffset;
        private string zOffset;
        private string moveSpeedIn_pct;
        private string moveSpeedOut_pct;
        private string doAirAspirateAfter;

        [ProcessActionArgument(typeof(int), true, "0=Pump1, 1=Pump2")]
        public string SyringeMask				// bitmask for which syringe
        {
            get { return syringeMask; }
            set { syringeMask = value; }
        }

        [ProcessActionArgument(typeof(double), true)]
        public string VolumePerStroke_uL
        {
            get { return volumePerStroke_uL; }
            set { volumePerStroke_uL = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string NumSyringeStrokes
        {
            get { return numSyringeStrokes; }
            set { numSyringeStrokes = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string SyringeSpeedPull
        {
            get { return syringeSpeedPull; }
            set { syringeSpeedPull = value; }
        }

        [ProcessActionArgument(typeof(int), true)]
        public string SyringeSpeedPush
        {
            get { return syringeSpeedPush; }
            set { syringeSpeedPush = value; }
        }

        [ProcessActionArgument(typeof(int), false)]
        public string DelayAfter_ms
        {
            get { return delayAfter_ms; }
            set { delayAfter_ms = value; }
        }

        [ProcessActionArgument(typeof(MachineCoordinate), false)]
        public string Tip1Point
        {
            get { return tip1Point; }
            set { tip1Point = value; }
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
        public string MoveSpeedIn_pct
        {
            get { return moveSpeedIn_pct; }
            set { moveSpeedIn_pct = value; }
        }

        [ProcessActionArgument(typeof(double), false)]
        public string MoveSpeedOut_pct
        {
            get { return moveSpeedOut_pct; }
            set { moveSpeedOut_pct = value; }
        }

        [ProcessActionArgument(typeof(bool), false,"Not Tested - DO NOT USE")]
        public string DoAirAspirateAfter
        {
            get { return doAirAspirateAfter; }
            set { doAirAspirateAfter = value; }
        }

        #endregion

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
            syringeMask = "";
            volumePerStroke_uL = "";
            numSyringeStrokes = "";
            syringeSpeedPull = "";
            syringeSpeedPush = "";
            delayAfter_ms = "";
            tip1Point = "";
            xOffset = "";
            yOffset = "";
            zOffset = "";
            moveSpeedIn_pct = "";
            moveSpeedOut_pct = "";
            doAirAspirateAfter = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_Wash() : base("Wash Tips", "Wash tips", ProcessAction.IMG_WASH, true, SequenceFile.CommandNames.WashTips) { Clear(); }

    }
}