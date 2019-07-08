using System;


namespace EA.PixyControl.ClassLibrary
{
 
 
    public class Process_DE03CosSetup : ProcessAction
    {
        #region members

        private string tip;
        private string desiredFreq;					
        private string inBurstNo;						
        private string cosAmplitude;
        private string strobeDuration;
        private string strobeDelay;
        private string numOfBursts;
        private string triggerSetting;
        private string triggerDelay;
        private string triggerPeriod;

        [ProcessActionArgument(typeof(int), true, "1 to 4")]
        public string Tip
        {
            get { return tip; }
            set { tip = value; }
        }

        [ProcessActionArgument(typeof(int), true, "10 to 100000")]
        public string DesiredFreq
        {
            get { return desiredFreq; }
            set { desiredFreq = value; }
        }

        [ProcessActionArgument(typeof(int), true, "1 TO 10,000,000")]
        public string InBurstNo
        {
            get { return inBurstNo; }
            set { inBurstNo = value; }
        }

        [ProcessActionArgument(typeof(int), true, "0 TO 2047")]
        public string CosAmplitude
        {
            get { return cosAmplitude; }
            set { cosAmplitude = value; }
        }

        [ProcessActionArgument(typeof(int), true, "1 TO 10")]
        public string StrobeDuration
        {
            get { return strobeDuration; }
            set { strobeDuration = value; }
        }

        [ProcessActionArgument(typeof(int), true,"0 TO 10")]
        public string StrobeDelay
        {
            get { return strobeDelay; }
            set { strobeDelay = value; }
        }

        [ProcessActionArgument(typeof(int), true, "1 TO 10,000,000")]
        public string NumOfBursts
        {
            get { return numOfBursts; }
            set { numOfBursts = value; }
        }

		[ProcessActionArgument(typeof(int), false, "0 to 2")]
		public string TriggerSetting
		{
			get { return triggerSetting; }
			set { triggerSetting = value; }
		}

		[ProcessActionArgument(typeof(int), false, "1 TO 10,000,000")]
		public string TriggerDelay
		{
			get { return triggerDelay; }
			set { triggerDelay = value; }
		}

		[ProcessActionArgument(typeof(int), false, "1 TO 10,000,000")]
		public string TriggerPeriod
		{
			get { return triggerPeriod; }
			set { triggerPeriod = value; }
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
			desiredFreq = "";					
			inBurstNo= "";						
			cosAmplitude= "";
			strobeDuration= "";
			strobeDelay= "";
			numOfBursts= "";
			triggerSetting= "";
			triggerDelay= "";
			triggerPeriod= "";
        }

        public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
        {
            return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
        }

        public Process_DE03CosSetup() : base("DE03 Cos Setup", "Setup Cos wave in DE03", ProcessAction.IMG_DISPENSE, true, SequenceFile.CommandNames.DE03CosSetup) { Clear(); }
    }

	public class Process_DE03TrapSetup : ProcessAction
	{
		#region members

		private string tip;
		private string leading;					
		private string dwell;						
		private string trailing;
		private string trapDrops;					
		private string trapFreq;						
		private string trapAmp;
		private string strobeDuration;
		private string strobeDelay;
		private string triggerSetting;
		private string triggerDelay;
		private string triggerPeriod;

		[ProcessActionArgument(typeof(int), true, "1 to 4")]
		public string Tip
		{
			get { return tip; }
			set { tip = value; }
		}

		[ProcessActionArgument(typeof(int), true, "leading edge usec")]
		public string Leading
		{
			get { return leading; }
			set { leading = value; }
		}

		[ProcessActionArgument(typeof(int), true, "dwell usec")]
		public string Dwell
		{
			get { return dwell; }
			set { dwell = value; }
		}

		[ProcessActionArgument(typeof(int), true, "trailing edge usec")]
		public string Trailing
		{
			get { return trailing; }
			set { trailing = value; }
		}

		[ProcessActionArgument(typeof(int), true, "Number of drops")]
		public string TrapDrops
		{
			get { return trapDrops; }
			set { trapDrops = value; }
		}

		[ProcessActionArgument(typeof(int), true, "Frequency of drops")]
		public string TrapFreq
		{
			get { return trapFreq; }
			set { trapFreq = value; }
		}


		[ProcessActionArgument(typeof(int), true, "Amplitude 0 TO 4095")]
		public string TrapAmp
		{
			get { return trapAmp; }
			set { trapAmp = value; }
		}

		[ProcessActionArgument(typeof(int), true, "1 TO 10 usec")]
		public string StrobeDuration
		{
			get { return strobeDuration; }
			set { strobeDuration = value; }
		}

		[ProcessActionArgument(typeof(int), true,"0 TO 300 usec")]
		public string StrobeDelay
		{
			get { return strobeDelay; }
			set { strobeDelay = value; }
		}

		[ProcessActionArgument(typeof(int), false, "0 to 2")]
		public string TriggerSetting
		{
			get { return triggerSetting; }
			set { triggerSetting = value; }
		}

		[ProcessActionArgument(typeof(int), false, "1 TO 10,000,000")]
		public string TriggerDelay
		{
			get { return triggerDelay; }
			set { triggerDelay = value; }
		}

		[ProcessActionArgument(typeof(int), false, "1 TO 500")]
		public string TriggerPeriod
		{
			get { return triggerPeriod; }
			set { triggerPeriod = value; }
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
			leading = "";					
			dwell= "";						
			trailing= "";
			trapDrops= "";					
			trapFreq= "";						
			trapAmp= "";
			strobeDuration= "";
			strobeDelay= "";
			triggerSetting= "";
			triggerDelay= "";
			triggerPeriod= "";
		}

		public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
		{
			return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
		}

		public Process_DE03TrapSetup() : base("DE03 Trap Setup", "Setup Trapezoid wave in DE03", ProcessAction.IMG_DISPENSE, true, SequenceFile.CommandNames.DE03TrapSetup) { Clear(); }
	}

public class Process_DE03StartWaveform : ProcessAction
{
	#region members

	private string waitForCompletion;
	private string timeout_ms;					

	[ProcessActionArgument(typeof(bool), true)]
	public string WaitForCompletion
	{
		get { return waitForCompletion; }
		set { waitForCompletion = value; }
	}

	[ProcessActionArgument(typeof(int), true)]
	public string Timeout_ms
	{
		get { return timeout_ms; }
		set { timeout_ms = value; }
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
		waitForCompletion="";
		timeout_ms="";					
	}

	public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
	{
		return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
	}

	public Process_DE03StartWaveform() : base("DE03 Start Waveform", "Start outputting waveform", ProcessAction.IMG_DISPENSE, true, SequenceFile.CommandNames.DE03StartWaveform) { Clear(); }
}

	public class Process_DE03StopWaveform : ProcessAction
	{
		#region members



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
		}

		public override bool ParametersOK(VariableManager VM, out string ErrorMsg)
		{
			return SequenceFile.ProcessActionStringParametersOK(this, VM, out ErrorMsg);
		}

		public Process_DE03StopWaveform() : base("DE03 Stop Waveform", "Stop outputting waveform", ProcessAction.IMG_DISPENSE, true, SequenceFile.CommandNames.DE03StopWaveform) { Clear(); }
	}
}

