using System;


namespace EA.PixyControl.ClassLibrary
{
    public class Process_EnableController : ProcessAction
    {
        private string enableController;

        [ProcessActionArgument(typeof(bool), true)]
        public string EnableController
        {
            get { return enableController; }
            set { enableController = value; }
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
            enableController = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_EnableController() : base("Controller Enable", "Enable or disable piezo controller", ProcessAction.IMG_WASH, true, SequenceFile.CommandNames.ControllerEnable) { Clear(); }

    }

    public class Process_InspectTipFiring : ProcessAction
    {
        #region members

        private string tipMask;
        private string tip1Point;
        private string xOffset;
        private string yOffset;
        private string zOffset;

        [ProcessActionArgument(typeof(int), true,"INSPECT TIP NOT IMPLEMENTED - DO NOT USE")]
        public string TipMask
        {
            get { return tipMask; }
            set { tipMask = value; }
        }

        [ProcessActionArgument(typeof(MachineCoordinate), true)]
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
            tipMask = "";
            tip1Point = "";
            xOffset = "";
            yOffset = "";
            zOffset = "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_InspectTipFiring() : base("Inspect Tips", "Watch tips fire using camera and strobe", ProcessAction.IMG_INSPECT, true, SequenceFile.CommandNames.InspectTips) { Clear(); }

    }

	public class Process_PiezoDispense : ProcessAction
	{
		#region members

		private string tip;
		private string piezoAmplitude;					// 0 to 255.
		private string piezoFreq;						// 0 to 65335
		private string dropsPerBurst;
		private string numBursts;
		private string freqOfBursts;
		private string tip1Point;
		private string xOffset;
		private string yOffset;
		private string zOffset;
		private string moveHeightAboveSurface;

		[ProcessActionArgument(typeof(int), true, "1-based tip number")]
		public string Tip
		{
			get { return tip; }
			set { tip = value; }
		}

		[ProcessActionArgument(typeof(int), false, "0 to 255")]
		public string PiezoAmplitude
		{
			get { return piezoAmplitude; }
			set { piezoAmplitude = value; }
		}

		[ProcessActionArgument(typeof(int), false, "0 TO 65335")]
		public string PiezoFreq
		{
			get { return piezoFreq; }
			set { piezoFreq = value; }
		}

		[ProcessActionArgument(typeof(int), true)]
		public string DropsPerBurst
		{
			get { return dropsPerBurst; }
			set { dropsPerBurst = value; }
		}

		[ProcessActionArgument(typeof(int), true)]
		public string NumBursts
		{
			get { return numBursts; }
			set { numBursts = value; }
		}

		[ProcessActionArgument(typeof(int), true)]
		public string FreqOfBursts
		{
			get { return freqOfBursts; }
			set { freqOfBursts = value; }
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

		[ProcessActionArgument(typeof(double), false, "Height above Tip1Point when dispensing")]
		public string ZOffset
		{
			get { return zOffset; }
			set { zOffset = value; }
		}

		[ProcessActionArgument(typeof(double), false, "Height above Tip1Point when moving  mm" )]
		public string MoveHeightAboveSurface
		{
			get { return moveHeightAboveSurface; }
			set { moveHeightAboveSurface = value; }
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
			tip = "";
			piezoAmplitude = "";
			piezoFreq = "";
			dropsPerBurst = "";
			numBursts = "";
			freqOfBursts = "";
			tip1Point = "";
			xOffset = "";
			yOffset = "";
			zOffset = "";
			moveHeightAboveSurface = "";
		}

		public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
		{
			return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
		}

		public Process_PiezoDispense() : base("Piezo Dispense", "Dispense using piezo tips", ProcessAction.IMG_DISPENSE, true, SequenceFile.CommandNames.PiezoDispense) { Clear(); }

	}

}