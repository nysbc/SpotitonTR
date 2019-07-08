using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

// PKv4.2.4,  Smarter SyringeControlForm defaults (first thing we usually like to do is prime):
//     Volume setting to 100%,  
//     Read speed code from XML file  , (let the user change this).

//     Remembers where the valve was last set and displays this info

//     Got rid of a few seldom used buttons and added a button to read / update the status


namespace Aurigin
{
	/// <summary>
	/// Summary description for SyringeControlForm.
	/// </summary>
	public class SyringeControlForm : System.Windows.Forms.Form
	{
		private static int formCount;
		
		private UserSyringeControl	syringeControl;
		private System.Windows.Forms.Button btnInitialize;
		private System.Windows.Forms.Button btnEmpty;
		private System.Windows.Forms.Button btnAspirate;
		private System.Windows.Forms.Button btnDispense;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton rdoInput;
		private System.Windows.Forms.RadioButton rdoOutput;
		private System.Windows.Forms.RadioButton rdoBypass;
		private System.Windows.Forms.TrackBar trkVolume;
		private System.Windows.Forms.Label lblVolume;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.CheckBox chkNoError;
		private System.Windows.Forms.CheckBox chkInit;
		private System.Windows.Forms.CheckBox chkInvalidCommand;
		private System.Windows.Forms.CheckBox chkInvalidOperand;
		private System.Windows.Forms.CheckBox chkInvalidCmdSeq;
		private System.Windows.Forms.CheckBox chkNeedInit;
		private System.Windows.Forms.CheckBox chkPlungerOverload;
		private System.Windows.Forms.CheckBox chkValveOverload;
		private System.Windows.Forms.CheckBox chkMoveNotAllowed;
		private System.Windows.Forms.CheckBox chkCmdOverflow;
		private System.Windows.Forms.Timer tmrUpdate;
		private System.Windows.Forms.CheckBox chkIdle;
		private System.Windows.Forms.Label lblSpeedCode;
		private System.Windows.Forms.TrackBar trkSpeed;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox comboBoxSyringe;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btnPrime;
		private System.Windows.Forms.NumericUpDown numUpDownLoop;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Button btnSetPrecise;
		private System.Windows.Forms.TextBox txtPreciseVol;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
        private Button btnReadValvePos;
        private Button btnUpdateStatus;
        private System.ComponentModel.IContainer components;

		public SyringeControlForm()
		{
			++formCount;

			// Required for Windows Form Designer support
			InitializeComponent();
		}

		public static int FormCount
		{
			get{return formCount;}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );

			formCount--;
		}

		public void Show(UserSyringeControl SyringeControl)
		{
			syringeControl = SyringeControl;
			comboBoxSyringe.Items.Clear();
			for (int i=1; i<=syringeControl.TotalSyringe(); i++)
			{
				comboBoxSyringe.Items.Add(i.ToString());
			}
			comboBoxSyringe.SelectedIndex = 0;
			int index = this.comboBoxSyringe.SelectedIndex;

			this.trkVolume.Minimum = 0;
			this.trkVolume.Maximum = (int)syringeControl.SyringeVolume_uL(index);
            this.trkVolume.Value = (int)syringeControl.SyringeVolume_uL(index);                 // PKv4.2.4  Default to highest speed
            this.lblVolume.Text = this.trkVolume.Value.ToString() + ".0";

            //			this.trkVolume.Value = 1;
            //			this.lblVolume.Text = "1.0";

            this.trkSpeed.Minimum = UserSyringeControl.MIN_SPEED_CODE;
			this.trkSpeed.Maximum = UserSyringeControl.MAX_SPEED_CODE;
            this.trkSpeed.Value = (int)syringeControl.SyringeDefaultSpeed(index);
            this.lblSpeedCode.Text = this.trkSpeed.Value.ToString();

 //           this.trkSpeed.Value = 17;
 //			this.lblSpeedCode.Text = "17";

            // TODO  Test with tmrUpdate.Enabled = false
            // Add optional button to allow scanning for status

            this.tmrUpdate.Enabled = false;

			//base.Show();
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyringeControlForm));
            this.btnInitialize = new System.Windows.Forms.Button();
            this.btnEmpty = new System.Windows.Forms.Button();
            this.btnAspirate = new System.Windows.Forms.Button();
            this.btnDispense = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rdoBypass = new System.Windows.Forms.RadioButton();
            this.rdoOutput = new System.Windows.Forms.RadioButton();
            this.rdoInput = new System.Windows.Forms.RadioButton();
            this.trkVolume = new System.Windows.Forms.TrackBar();
            this.lblVolume = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkIdle = new System.Windows.Forms.CheckBox();
            this.chkCmdOverflow = new System.Windows.Forms.CheckBox();
            this.chkMoveNotAllowed = new System.Windows.Forms.CheckBox();
            this.chkValveOverload = new System.Windows.Forms.CheckBox();
            this.chkPlungerOverload = new System.Windows.Forms.CheckBox();
            this.chkNeedInit = new System.Windows.Forms.CheckBox();
            this.chkInvalidCmdSeq = new System.Windows.Forms.CheckBox();
            this.chkInvalidOperand = new System.Windows.Forms.CheckBox();
            this.chkInvalidCommand = new System.Windows.Forms.CheckBox();
            this.chkInit = new System.Windows.Forms.CheckBox();
            this.chkNoError = new System.Windows.Forms.CheckBox();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.lblSpeedCode = new System.Windows.Forms.Label();
            this.trkSpeed = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxSyringe = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnPrime = new System.Windows.Forms.Button();
            this.numUpDownLoop = new System.Windows.Forms.NumericUpDown();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnSetPrecise = new System.Windows.Forms.Button();
            this.txtPreciseVol = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnUpdateStatus = new System.Windows.Forms.Button();
            this.btnReadValvePos = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkVolume)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUpDownLoop)).BeginInit();
            this.SuspendLayout();
            // 
            // btnInitialize
            // 
            this.btnInitialize.Location = new System.Drawing.Point(16, 41);
            this.btnInitialize.Name = "btnInitialize";
            this.btnInitialize.Size = new System.Drawing.Size(128, 24);
            this.btnInitialize.TabIndex = 0;
            this.btnInitialize.Text = "Initialize Pump";
            this.btnInitialize.Click += new System.EventHandler(this.btnInitialize_Click);
            // 
            // btnEmpty
            // 
            this.btnEmpty.Location = new System.Drawing.Point(152, 41);
            this.btnEmpty.Name = "btnEmpty";
            this.btnEmpty.Size = new System.Drawing.Size(128, 24);
            this.btnEmpty.TabIndex = 1;
            this.btnEmpty.Text = "Empty Syringe";
            this.toolTip1.SetToolTip(this.btnEmpty, "Completely empty the syringe");
            this.btnEmpty.Click += new System.EventHandler(this.btnEmpty_Click);
            // 
            // btnAspirate
            // 
            this.btnAspirate.Location = new System.Drawing.Point(16, 145);
            this.btnAspirate.Name = "btnAspirate";
            this.btnAspirate.Size = new System.Drawing.Size(128, 24);
            this.btnAspirate.TabIndex = 2;
            this.btnAspirate.Text = "Aspirate";
            this.toolTip1.SetToolTip(this.btnAspirate, "Pull fluid (Volume) into the Syringe");
            this.btnAspirate.Click += new System.EventHandler(this.btnAspirate_Click);
            // 
            // btnDispense
            // 
            this.btnDispense.Location = new System.Drawing.Point(152, 145);
            this.btnDispense.Name = "btnDispense";
            this.btnDispense.Size = new System.Drawing.Size(128, 24);
            this.btnDispense.TabIndex = 3;
            this.btnDispense.Text = "Dispense";
            this.toolTip1.SetToolTip(this.btnDispense, "Push Fluid  (Volume) Out of the Syringe");
            this.btnDispense.Click += new System.EventHandler(this.btnDispense_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnReadValvePos);
            this.groupBox1.Controls.Add(this.rdoBypass);
            this.groupBox1.Controls.Add(this.rdoOutput);
            this.groupBox1.Controls.Add(this.rdoInput);
            this.groupBox1.Location = new System.Drawing.Point(312, 78);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(136, 140);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Valve";
            // 
            // rdoBypass
            // 
            this.rdoBypass.Location = new System.Drawing.Point(16, 72);
            this.rdoBypass.Name = "rdoBypass";
            this.rdoBypass.Size = new System.Drawing.Size(64, 24);
            this.rdoBypass.TabIndex = 2;
            this.rdoBypass.Text = "Bypass";
            this.toolTip1.SetToolTip(this.rdoBypass, " Input to output bypassing the syringe");
            this.rdoBypass.CheckedChanged += new System.EventHandler(this.rdoBypass_CheckedChanged);
            // 
            // rdoOutput
            // 
            this.rdoOutput.Location = new System.Drawing.Point(16, 48);
            this.rdoOutput.Name = "rdoOutput";
            this.rdoOutput.Size = new System.Drawing.Size(104, 24);
            this.rdoOutput.TabIndex = 1;
            this.rdoOutput.Text = "Output (Tip)";
            this.toolTip1.SetToolTip(this.rdoOutput, "Syringe To Output");
            this.rdoOutput.CheckedChanged += new System.EventHandler(this.rdoOutput_CheckedChanged);
            // 
            // rdoInput
            // 
            this.rdoInput.Location = new System.Drawing.Point(16, 24);
            this.rdoInput.Name = "rdoInput";
            this.rdoInput.Size = new System.Drawing.Size(112, 24);
            this.rdoInput.TabIndex = 0;
            this.rdoInput.Text = "Input (Reservoir)";
            this.toolTip1.SetToolTip(this.rdoInput, "Syringe To Input");
            this.rdoInput.CheckedChanged += new System.EventHandler(this.rdoInput_CheckedChanged);
            // 
            // trkVolume
            // 
            this.trkVolume.Location = new System.Drawing.Point(80, 73);
            this.trkVolume.Name = "trkVolume";
            this.trkVolume.Size = new System.Drawing.Size(152, 45);
            this.trkVolume.TabIndex = 5;
            this.trkVolume.TickFrequency = 10;
            this.trkVolume.Scroll += new System.EventHandler(this.trkVolume_Scroll);
            // 
            // lblVolume
            // 
            this.lblVolume.Location = new System.Drawing.Point(264, 81);
            this.lblVolume.Name = "lblVolume";
            this.lblVolume.Size = new System.Drawing.Size(56, 24);
            this.lblVolume.TabIndex = 6;
            this.lblVolume.Text = "1.0";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnUpdateStatus);
            this.groupBox2.Controls.Add(this.chkIdle);
            this.groupBox2.Controls.Add(this.chkCmdOverflow);
            this.groupBox2.Controls.Add(this.chkMoveNotAllowed);
            this.groupBox2.Controls.Add(this.chkValveOverload);
            this.groupBox2.Controls.Add(this.chkPlungerOverload);
            this.groupBox2.Controls.Add(this.chkNeedInit);
            this.groupBox2.Controls.Add(this.chkInvalidCmdSeq);
            this.groupBox2.Controls.Add(this.chkInvalidOperand);
            this.groupBox2.Controls.Add(this.chkInvalidCommand);
            this.groupBox2.Controls.Add(this.chkInit);
            this.groupBox2.Controls.Add(this.chkNoError);
            this.groupBox2.Location = new System.Drawing.Point(16, 216);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(432, 131);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Status";
            // 
            // chkIdle
            // 
            this.chkIdle.Enabled = false;
            this.chkIdle.Location = new System.Drawing.Point(16, 20);
            this.chkIdle.Name = "chkIdle";
            this.chkIdle.Size = new System.Drawing.Size(120, 24);
            this.chkIdle.TabIndex = 10;
            this.chkIdle.Text = "Idle";
            // 
            // chkCmdOverflow
            // 
            this.chkCmdOverflow.Enabled = false;
            this.chkCmdOverflow.Location = new System.Drawing.Point(288, 68);
            this.chkCmdOverflow.Name = "chkCmdOverflow";
            this.chkCmdOverflow.Size = new System.Drawing.Size(120, 24);
            this.chkCmdOverflow.TabIndex = 9;
            this.chkCmdOverflow.Text = "Cmd Overflow";
            // 
            // chkMoveNotAllowed
            // 
            this.chkMoveNotAllowed.Enabled = false;
            this.chkMoveNotAllowed.Location = new System.Drawing.Point(288, 44);
            this.chkMoveNotAllowed.Name = "chkMoveNotAllowed";
            this.chkMoveNotAllowed.Size = new System.Drawing.Size(120, 24);
            this.chkMoveNotAllowed.TabIndex = 8;
            this.chkMoveNotAllowed.Text = "Move Not Allowed";
            // 
            // chkValveOverload
            // 
            this.chkValveOverload.Enabled = false;
            this.chkValveOverload.Location = new System.Drawing.Point(288, 20);
            this.chkValveOverload.Name = "chkValveOverload";
            this.chkValveOverload.Size = new System.Drawing.Size(120, 24);
            this.chkValveOverload.TabIndex = 7;
            this.chkValveOverload.Text = "Valve Overload";
            // 
            // chkPlungerOverload
            // 
            this.chkPlungerOverload.Enabled = false;
            this.chkPlungerOverload.Location = new System.Drawing.Point(144, 92);
            this.chkPlungerOverload.Name = "chkPlungerOverload";
            this.chkPlungerOverload.Size = new System.Drawing.Size(120, 24);
            this.chkPlungerOverload.TabIndex = 6;
            this.chkPlungerOverload.Text = "Plunger Overload";
            // 
            // chkNeedInit
            // 
            this.chkNeedInit.Enabled = false;
            this.chkNeedInit.Location = new System.Drawing.Point(144, 68);
            this.chkNeedInit.Name = "chkNeedInit";
            this.chkNeedInit.Size = new System.Drawing.Size(120, 24);
            this.chkNeedInit.TabIndex = 5;
            this.chkNeedInit.Text = "Need to Initialize";
            // 
            // chkInvalidCmdSeq
            // 
            this.chkInvalidCmdSeq.Enabled = false;
            this.chkInvalidCmdSeq.Location = new System.Drawing.Point(144, 44);
            this.chkInvalidCmdSeq.Name = "chkInvalidCmdSeq";
            this.chkInvalidCmdSeq.Size = new System.Drawing.Size(120, 24);
            this.chkInvalidCmdSeq.TabIndex = 4;
            this.chkInvalidCmdSeq.Text = "Invalid Cmd Seq";
            // 
            // chkInvalidOperand
            // 
            this.chkInvalidOperand.Enabled = false;
            this.chkInvalidOperand.Location = new System.Drawing.Point(144, 20);
            this.chkInvalidOperand.Name = "chkInvalidOperand";
            this.chkInvalidOperand.Size = new System.Drawing.Size(120, 24);
            this.chkInvalidOperand.TabIndex = 3;
            this.chkInvalidOperand.Text = "Invalid Operand";
            // 
            // chkInvalidCommand
            // 
            this.chkInvalidCommand.Enabled = false;
            this.chkInvalidCommand.Location = new System.Drawing.Point(16, 92);
            this.chkInvalidCommand.Name = "chkInvalidCommand";
            this.chkInvalidCommand.Size = new System.Drawing.Size(120, 24);
            this.chkInvalidCommand.TabIndex = 2;
            this.chkInvalidCommand.Text = "Invalid Cmd";
            // 
            // chkInit
            // 
            this.chkInit.Enabled = false;
            this.chkInit.Location = new System.Drawing.Point(16, 68);
            this.chkInit.Name = "chkInit";
            this.chkInit.Size = new System.Drawing.Size(120, 24);
            this.chkInit.TabIndex = 1;
            this.chkInit.Text = "Init Error";
            // 
            // chkNoError
            // 
            this.chkNoError.Enabled = false;
            this.chkNoError.Location = new System.Drawing.Point(16, 44);
            this.chkNoError.Name = "chkNoError";
            this.chkNoError.Size = new System.Drawing.Size(120, 24);
            this.chkNoError.TabIndex = 0;
            this.chkNoError.Text = "No Error";
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Interval = 1000;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // lblSpeedCode
            // 
            this.lblSpeedCode.Location = new System.Drawing.Point(232, 113);
            this.lblSpeedCode.Name = "lblSpeedCode";
            this.lblSpeedCode.Size = new System.Drawing.Size(56, 24);
            this.lblSpeedCode.TabIndex = 9;
            this.lblSpeedCode.Text = "(Code)";
            // 
            // trkSpeed
            // 
            this.trkSpeed.Location = new System.Drawing.Point(80, 105);
            this.trkSpeed.Name = "trkSpeed";
            this.trkSpeed.Size = new System.Drawing.Size(152, 45);
            this.trkSpeed.TabIndex = 8;
            this.trkSpeed.TickFrequency = 10;
            this.trkSpeed.Scroll += new System.EventHandler(this.trkSpeed_Scroll);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 81);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 16);
            this.label1.TabIndex = 10;
            this.label1.Text = "Volume";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 113);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 16);
            this.label2.TabIndex = 11;
            this.label2.Text = "Speed Code";
            // 
            // comboBoxSyringe
            // 
            this.comboBoxSyringe.Items.AddRange(new object[] {
            "Syringe 1",
            "Syringe 2",
            "Syringe 3",
            "Syringe 4"});
            this.comboBoxSyringe.Location = new System.Drawing.Point(64, 8);
            this.comboBoxSyringe.Name = "comboBoxSyringe";
            this.comboBoxSyringe.Size = new System.Drawing.Size(120, 21);
            this.comboBoxSyringe.TabIndex = 12;
            this.comboBoxSyringe.Text = "Syringe 1";
            this.comboBoxSyringe.SelectedIndexChanged += new System.EventHandler(this.comboBoxSyringe_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(8, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 24);
            this.label3.TabIndex = 13;
            this.label3.Text = "Syringe";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnPrime
            // 
            this.btnPrime.Location = new System.Drawing.Point(296, 41);
            this.btnPrime.Name = "btnPrime";
            this.btnPrime.Size = new System.Drawing.Size(72, 24);
            this.btnPrime.TabIndex = 16;
            this.btnPrime.Text = "Prime ->";
            this.toolTip1.SetToolTip(this.btnPrime, "Use Volume and Speed Code to Prime N Times");
            this.btnPrime.Click += new System.EventHandler(this.btnPrime_Click);
            // 
            // numUpDownLoop
            // 
            this.numUpDownLoop.AllowDrop = true;
            this.numUpDownLoop.Location = new System.Drawing.Point(376, 41);
            this.numUpDownLoop.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numUpDownLoop.Name = "numUpDownLoop";
            this.numUpDownLoop.Size = new System.Drawing.Size(48, 20);
            this.numUpDownLoop.TabIndex = 18;
            this.numUpDownLoop.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // btnSetPrecise
            // 
            this.btnSetPrecise.Location = new System.Drawing.Point(16, 177);
            this.btnSetPrecise.Name = "btnSetPrecise";
            this.btnSetPrecise.Size = new System.Drawing.Size(128, 24);
            this.btnSetPrecise.TabIndex = 20;
            this.btnSetPrecise.Text = "Set Precise Vol";
            this.toolTip1.SetToolTip(this.btnSetPrecise, "Set a precise volume on the slider bar (For testing very small volumes)");
            this.btnSetPrecise.Click += new System.EventHandler(this.btnSetPrecise_Click);
            // 
            // txtPreciseVol
            // 
            this.txtPreciseVol.Location = new System.Drawing.Point(192, 177);
            this.txtPreciseVol.Name = "txtPreciseVol";
            this.txtPreciseVol.Size = new System.Drawing.Size(40, 20);
            this.txtPreciseVol.TabIndex = 21;
            this.txtPreciseVol.Text = "0.0";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(240, 179);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 16);
            this.label4.TabIndex = 22;
            this.label4.Text = "ul";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(232, 81);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(24, 16);
            this.label5.TabIndex = 23;
            this.label5.Text = "ul";
            // 
            // btnUpdateStatus
            // 
            this.btnUpdateStatus.Location = new System.Drawing.Point(290, 98);
            this.btnUpdateStatus.Name = "btnUpdateStatus";
            this.btnUpdateStatus.Size = new System.Drawing.Size(96, 24);
            this.btnUpdateStatus.TabIndex = 24;
            this.btnUpdateStatus.Text = "Read Status";
            this.toolTip1.SetToolTip(this.btnUpdateStatus, "Set a precise volume on the slider bar (For testing very small volumes)");
            this.btnUpdateStatus.Click += new System.EventHandler(this.btnUpdateStatus_Click);
            // 
            // btnReadValvePos
            // 
            this.btnReadValvePos.Location = new System.Drawing.Point(16, 108);
            this.btnReadValvePos.Name = "btnReadValvePos";
            this.btnReadValvePos.Size = new System.Drawing.Size(96, 24);
            this.btnReadValvePos.TabIndex = 25;
            this.btnReadValvePos.Text = "Read Valve Pos";
            this.toolTip1.SetToolTip(this.btnReadValvePos, "Set a precise volume on the slider bar (For testing very small volumes)");
            this.btnReadValvePos.Click += new System.EventHandler(this.btnReadValvePos_Click);
            // 
            // SyringeControlForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(464, 361);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtPreciseVol);
            this.Controls.Add(this.btnSetPrecise);
            this.Controls.Add(this.numUpDownLoop);
            this.Controls.Add(this.btnPrime);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBoxSyringe);
            this.Controls.Add(this.btnDispense);
            this.Controls.Add(this.btnAspirate);
            this.Controls.Add(this.trkSpeed);
            this.Controls.Add(this.trkVolume);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblSpeedCode);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.lblVolume);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnEmpty);
            this.Controls.Add(this.btnInitialize);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SyringeControlForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Syringe Control";
            this.toolTip1.SetToolTip(this, "Will set a precise real value volume");
            this.Load += new System.EventHandler(this.SyringeControlForm_Load);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trkVolume)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUpDownLoop)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		static SyringeStatus _status = new SyringeStatus();
		private void tmrUpdate_Tick(object sender, System.EventArgs e)
		{
			if (syringeControl != null)
			{
				int index = this.comboBoxSyringe.SelectedIndex;
				SyringeStatus status = syringeControl.PumpStatus(index);

				if (status != _status)
				{
					_status = status;
					this.chkIdle.Checked				= status.Idle;
					this.chkCmdOverflow.Checked			= status.CommandOverflow;
					this.chkInit.Checked				= status.InitializationError;
					this.chkInvalidCmdSeq.Checked		= status.InvalidCommandSeq;
					this.chkInvalidCommand.Checked		= status.InvalidCommand;
					this.chkInvalidOperand.Checked		= status.InvalidOperand;
					this.chkMoveNotAllowed.Checked		= status.PlungerMoveNotAllowed;
					this.chkNeedInit.Checked			= status.DeviceNotInitialized;
					this.chkNoError.Checked				= status.NoError;
					this.chkPlungerOverload.Checked		= status.PlungerOverload;
					this.chkValveOverload.Checked		= status.ValveOverload;
				}
			}
		}

		private void btnInitialize_Click(object sender, System.EventArgs e)
		{
			int index = this.comboBoxSyringe.SelectedIndex;
			if (syringeControl.InitializePump(index) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Failed to initialize syringe pump", "SYRINGE ERROR");
			}
		}

		private void btnEmpty_Click(object sender, System.EventArgs e)
		{
			int index = this.comboBoxSyringe.SelectedIndex;
			if (syringeControl.EmptySyringe(index, GetSpeedCode(), false) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Failed to execute syringe command", "SYRINGE ERROR");
			}
		}

		private int GetSpeedCode()
		{
			return this.trkSpeed.Value;
		}

		private double GetVolume()
		{
//			return (double) this.trkVolume.Value;
			return Convert.ToDouble(this.lblVolume.Text);   //14Feb2008
		}

		private void trkVolume_Scroll(object sender, System.EventArgs e)
		{
			this.lblVolume.Text = string.Format("{0:F1}", (double) trkVolume.Value);
		}

		private void trkSpeed_Scroll(object sender, System.EventArgs e)
		{
			this.lblSpeedCode.Text = string.Format("{0:D}", trkSpeed.Value);
		}

		private void btnAspirate_Click(object sender, System.EventArgs e)
		{
			int index = this.comboBoxSyringe.SelectedIndex;
			int timeout = System.Environment.TickCount;
			if (syringeControl.Aspirate(index, GetVolume(), GetSpeedCode(), true) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Failed to execute syringe command", "SYRINGE ERROR");
			}
			timeout = System.Environment.TickCount - timeout;
		}

		private void btnDispense_Click(object sender, System.EventArgs e)
		{
			int index = this.comboBoxSyringe.SelectedIndex;
			if (syringeControl.Dispense(index, GetVolume(), GetSpeedCode(), false) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Failed to execute syringe command", "SYRINGE ERROR");
			}
		}

		private void rdoInput_CheckedChanged(object sender, System.EventArgs e)
		{
			if (rdoInput.Checked) SetValvePosition(SyringeValvePosition.InputPosition);
		}

		private void rdoOutput_CheckedChanged(object sender, System.EventArgs e)
		{
			if (rdoOutput.Checked) SetValvePosition(SyringeValvePosition.OutputPosition);
		}

		private void rdoBypass_CheckedChanged(object sender, System.EventArgs e)
		{
			if (rdoBypass.Checked) SetValvePosition(SyringeValvePosition.BypassPosition);
		}

		private void SetValvePosition(SyringeValvePosition Position)
		{
			int index = this.comboBoxSyringe.SelectedIndex;
			if (syringeControl.SetValvePosition(index, Position) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Failed to execute syringe command", "SYRINGE ERROR");
			}
		}

		private void SyringeControlForm_Load(object sender, System.EventArgs e)
		{
//			AtSyringe.Control.InitPumpControl();    // If not initialized then initialize the pump.
			Show(AtSyringe.Control);
            this.btnReadValvePos_Click(sender, e);
		}

        private void btnPrime_Click(object sender, System.EventArgs e)
        {
            int index = this.comboBoxSyringe.SelectedIndex;
            double vol = 100.0;

            // Empty
            SetValvePosition(SyringeValvePosition.OutputPosition);
            if (syringeControl.EmptySyringe(index, GetSpeedCode(), true) != 0)
            {
                System.Windows.Forms.MessageBox.Show("Failed to execute syringe command", "SYRINGE ERROR");
            }

            if (index == (syringeControl.TotalSyringe() - 1))
            {
                vol = syringeControl.SyringeVolume_uL(index);
            }

            int refillSpeedCode = syringeControl.SyringeRefillSpeed(index);

            for (int i = 0; i < Convert.ToInt16(this.numUpDownLoop.Value); i++)
            {
                // Input

                //                SetValvePosition(SyringeValvePosition.InputPosition);  added following line.
                rdoInput.Checked = true;  // this should generate the interupt to change valve state if necessary.  PKv4.2.4
                System.Threading.Thread.Sleep(50);   // Give it a bit of time.

                if (syringeControl.Aspirate(index, GetVolume(), refillSpeedCode, true) != 0)
                {
                    System.Windows.Forms.MessageBox.Show("Failed to execute syringe command", "SYRINGE ERROR");
                }

                rdoOutput.Checked = true;   // this should generate the interupt to change valve state if necessary.
                System.Threading.Thread.Sleep(250);   // Give it a bit of time.   // PKv4.5.0  More time after refill.

                if (syringeControl.Dispense(index, GetVolume(), GetSpeedCode(), true) != 0)
                {
                    System.Windows.Forms.MessageBox.Show("Failed to execute syringe command", "SYRINGE ERROR");
                }
            }

            syringeControl.EmptySyringe(index, GetSpeedCode(), true);

        }

        private void btnPurge_Click(object sender, System.EventArgs e)
		{
			int index = this.comboBoxSyringe.SelectedIndex;
			int slow_speed = GetSpeedCode();

			// Output
			SetValvePosition(SyringeValvePosition.OutputPosition);
			if (syringeControl.Aspirate(index, GetVolume(), slow_speed, true) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Failed to execute syringe command", "SYRINGE ERROR");
			}

			// Input
			SetValvePosition(SyringeValvePosition.InputPosition);
			if (syringeControl.Aspirate(index, GetVolume(), slow_speed, true) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Failed to execute syringe command", "SYRINGE ERROR");
			}
			
			// Output
			SetValvePosition(SyringeValvePosition.OutputPosition);
			syringeControl.EmptySyringe(index, slow_speed, true);
			rdoOutput.Checked=true;   // pk14Feb2008
		}

		private void comboBoxSyringe_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			this.trkVolume.Maximum = (int)syringeControl.SyringeVolume_uL(comboBoxSyringe.SelectedIndex);
		}

		private void btnSetPrecise_Click(object sender, System.EventArgs e)
		{
			double temp=0.0;
			try
			{
				temp = Convert.ToDouble(txtPreciseVol.Text);
			}
			catch
			{
				MessageBox.Show("Invalide Number Format, use decimal point");
				txtPreciseVol.Text = "0.0";
				return;
				
				// Future Warning value//
			}
			if (temp<=this.trkVolume.Maximum && temp>=trkVolume.Minimum)
			{
				trkVolume.Value = (int) temp;
				this.lblVolume.Text = string.Format("{0:F3}", temp);
			}
			else
			{
				MessageBox.Show("Out of range");
				txtPreciseVol.Text = "0.0";
			}
		}

        private void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            tmrUpdate_Tick(sender, e);   
        }

        private void btnReadValvePos_Click(object sender, EventArgs e)
        {
            SyringeValvePosition valvePosition = syringeControl.GetValvePosition(comboBoxSyringe.SelectedIndex);
            if (valvePosition ==  SyringeValvePosition.OutputPosition) { rdoOutput.Checked=true; }
            if (valvePosition == SyringeValvePosition.InputPosition) { rdoInput.Checked = true; }
            if (valvePosition == SyringeValvePosition.BypassPosition) { rdoBypass.Checked = true; }
        }
    }
}
