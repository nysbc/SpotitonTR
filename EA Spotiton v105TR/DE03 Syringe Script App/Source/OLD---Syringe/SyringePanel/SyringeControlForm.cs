using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace EA.PixyControl
{
	/// <summary>
	/// Summary description for SyringeControlForm.
	/// </summary>
	public class SyringeControlForm : System.Windows.Forms.Form
	{
		private static int				formCount;
		
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
			
			this.trkVolume.Minimum = 1;
			this.trkVolume.Maximum = (int)syringeControl.SyringeVolume_uL();
			this.trkVolume.Value = 1;
			this.lblVolume.Text = "1 uL";

			this.trkSpeed.Minimum = UserSyringeControl.MIN_SPEED_CODE;
			this.trkSpeed.Maximum = UserSyringeControl.MAX_SPEED_CODE;
			this.trkSpeed.Value = 15;
			this.lblSpeedCode.Text = "15";

			this.tmrUpdate.Enabled = true;

			base.Show();
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
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
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trkVolume)).BeginInit();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trkSpeed)).BeginInit();
			this.SuspendLayout();
			// 
			// btnInitialize
			// 
			this.btnInitialize.Location = new System.Drawing.Point(16, 16);
			this.btnInitialize.Name = "btnInitialize";
			this.btnInitialize.Size = new System.Drawing.Size(128, 24);
			this.btnInitialize.TabIndex = 0;
			this.btnInitialize.Text = "Initialize Pump";
			this.btnInitialize.Click += new System.EventHandler(this.btnInitialize_Click);
			// 
			// btnEmpty
			// 
			this.btnEmpty.Location = new System.Drawing.Point(152, 16);
			this.btnEmpty.Name = "btnEmpty";
			this.btnEmpty.Size = new System.Drawing.Size(128, 24);
			this.btnEmpty.TabIndex = 1;
			this.btnEmpty.Text = "Empty Syringe";
			this.btnEmpty.Click += new System.EventHandler(this.btnEmpty_Click);
			// 
			// btnAspirate
			// 
			this.btnAspirate.Location = new System.Drawing.Point(16, 120);
			this.btnAspirate.Name = "btnAspirate";
			this.btnAspirate.Size = new System.Drawing.Size(128, 24);
			this.btnAspirate.TabIndex = 2;
			this.btnAspirate.Text = "Aspirate";
			this.btnAspirate.Click += new System.EventHandler(this.btnAspirate_Click);
			// 
			// btnDispense
			// 
			this.btnDispense.Location = new System.Drawing.Point(152, 120);
			this.btnDispense.Name = "btnDispense";
			this.btnDispense.Size = new System.Drawing.Size(128, 24);
			this.btnDispense.TabIndex = 3;
			this.btnDispense.Text = "Dispense";
			this.btnDispense.Click += new System.EventHandler(this.btnDispense_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.rdoBypass);
			this.groupBox1.Controls.Add(this.rdoOutput);
			this.groupBox1.Controls.Add(this.rdoInput);
			this.groupBox1.Location = new System.Drawing.Point(304, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(128, 112);
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
			this.rdoBypass.CheckedChanged += new System.EventHandler(this.rdoBypass_CheckedChanged);
			// 
			// rdoOutput
			// 
			this.rdoOutput.Location = new System.Drawing.Point(16, 48);
			this.rdoOutput.Name = "rdoOutput";
			this.rdoOutput.Size = new System.Drawing.Size(64, 24);
			this.rdoOutput.TabIndex = 1;
			this.rdoOutput.Text = "Output";
			this.rdoOutput.CheckedChanged += new System.EventHandler(this.rdoOutput_CheckedChanged);
			// 
			// rdoInput
			// 
			this.rdoInput.Location = new System.Drawing.Point(16, 24);
			this.rdoInput.Name = "rdoInput";
			this.rdoInput.Size = new System.Drawing.Size(64, 24);
			this.rdoInput.TabIndex = 0;
			this.rdoInput.Text = "Input";
			this.rdoInput.CheckedChanged += new System.EventHandler(this.rdoInput_CheckedChanged);
			// 
			// trkVolume
			// 
			this.trkVolume.Location = new System.Drawing.Point(80, 48);
			this.trkVolume.Name = "trkVolume";
			this.trkVolume.Size = new System.Drawing.Size(152, 45);
			this.trkVolume.TabIndex = 5;
			this.trkVolume.TickFrequency = 10;
			this.trkVolume.Scroll += new System.EventHandler(this.trkVolume_Scroll);
			// 
			// lblVolume
			// 
			this.lblVolume.Location = new System.Drawing.Point(232, 56);
			this.lblVolume.Name = "lblVolume";
			this.lblVolume.Size = new System.Drawing.Size(56, 24);
			this.lblVolume.TabIndex = 6;
			this.lblVolume.Text = "(uL)";
			// 
			// groupBox2
			// 
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
			this.groupBox2.Location = new System.Drawing.Point(16, 160);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(416, 136);
			this.groupBox2.TabIndex = 7;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Status";
			// 
			// chkIdle
			// 
			this.chkIdle.Enabled = false;
			this.chkIdle.Location = new System.Drawing.Point(16, 24);
			this.chkIdle.Name = "chkIdle";
			this.chkIdle.Size = new System.Drawing.Size(120, 24);
			this.chkIdle.TabIndex = 10;
			this.chkIdle.Text = "Idle";
			// 
			// chkCmdOverflow
			// 
			this.chkCmdOverflow.Enabled = false;
			this.chkCmdOverflow.Location = new System.Drawing.Point(288, 72);
			this.chkCmdOverflow.Name = "chkCmdOverflow";
			this.chkCmdOverflow.Size = new System.Drawing.Size(120, 24);
			this.chkCmdOverflow.TabIndex = 9;
			this.chkCmdOverflow.Text = "Cmd Overflow";
			// 
			// chkMoveNotAllowed
			// 
			this.chkMoveNotAllowed.Enabled = false;
			this.chkMoveNotAllowed.Location = new System.Drawing.Point(288, 48);
			this.chkMoveNotAllowed.Name = "chkMoveNotAllowed";
			this.chkMoveNotAllowed.Size = new System.Drawing.Size(120, 24);
			this.chkMoveNotAllowed.TabIndex = 8;
			this.chkMoveNotAllowed.Text = "Move Not Allowed";
			// 
			// chkValveOverload
			// 
			this.chkValveOverload.Enabled = false;
			this.chkValveOverload.Location = new System.Drawing.Point(288, 24);
			this.chkValveOverload.Name = "chkValveOverload";
			this.chkValveOverload.Size = new System.Drawing.Size(120, 24);
			this.chkValveOverload.TabIndex = 7;
			this.chkValveOverload.Text = "Valve Overload";
			// 
			// chkPlungerOverload
			// 
			this.chkPlungerOverload.Enabled = false;
			this.chkPlungerOverload.Location = new System.Drawing.Point(144, 96);
			this.chkPlungerOverload.Name = "chkPlungerOverload";
			this.chkPlungerOverload.Size = new System.Drawing.Size(120, 24);
			this.chkPlungerOverload.TabIndex = 6;
			this.chkPlungerOverload.Text = "Plunger Overload";
			// 
			// chkNeedInit
			// 
			this.chkNeedInit.Enabled = false;
			this.chkNeedInit.Location = new System.Drawing.Point(144, 72);
			this.chkNeedInit.Name = "chkNeedInit";
			this.chkNeedInit.Size = new System.Drawing.Size(120, 24);
			this.chkNeedInit.TabIndex = 5;
			this.chkNeedInit.Text = "Need to Initialize";
			// 
			// chkInvalidCmdSeq
			// 
			this.chkInvalidCmdSeq.Enabled = false;
			this.chkInvalidCmdSeq.Location = new System.Drawing.Point(144, 48);
			this.chkInvalidCmdSeq.Name = "chkInvalidCmdSeq";
			this.chkInvalidCmdSeq.Size = new System.Drawing.Size(120, 24);
			this.chkInvalidCmdSeq.TabIndex = 4;
			this.chkInvalidCmdSeq.Text = "Invalid Cmd Seq";
			// 
			// chkInvalidOperand
			// 
			this.chkInvalidOperand.Enabled = false;
			this.chkInvalidOperand.Location = new System.Drawing.Point(144, 24);
			this.chkInvalidOperand.Name = "chkInvalidOperand";
			this.chkInvalidOperand.Size = new System.Drawing.Size(120, 24);
			this.chkInvalidOperand.TabIndex = 3;
			this.chkInvalidOperand.Text = "Invalid Operand";
			// 
			// chkInvalidCommand
			// 
			this.chkInvalidCommand.Enabled = false;
			this.chkInvalidCommand.Location = new System.Drawing.Point(16, 96);
			this.chkInvalidCommand.Name = "chkInvalidCommand";
			this.chkInvalidCommand.Size = new System.Drawing.Size(120, 24);
			this.chkInvalidCommand.TabIndex = 2;
			this.chkInvalidCommand.Text = "Invalid Cmd";
			// 
			// chkInit
			// 
			this.chkInit.Enabled = false;
			this.chkInit.Location = new System.Drawing.Point(16, 72);
			this.chkInit.Name = "chkInit";
			this.chkInit.Size = new System.Drawing.Size(120, 24);
			this.chkInit.TabIndex = 1;
			this.chkInit.Text = "Init Error";
			// 
			// chkNoError
			// 
			this.chkNoError.Enabled = false;
			this.chkNoError.Location = new System.Drawing.Point(16, 48);
			this.chkNoError.Name = "chkNoError";
			this.chkNoError.Size = new System.Drawing.Size(120, 24);
			this.chkNoError.TabIndex = 0;
			this.chkNoError.Text = "No Error";
			// 
			// tmrUpdate
			// 
			this.tmrUpdate.Interval = 250;
			this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
			// 
			// lblSpeedCode
			// 
			this.lblSpeedCode.Location = new System.Drawing.Point(232, 88);
			this.lblSpeedCode.Name = "lblSpeedCode";
			this.lblSpeedCode.Size = new System.Drawing.Size(56, 24);
			this.lblSpeedCode.TabIndex = 9;
			this.lblSpeedCode.Text = "(Code)";
			// 
			// trkSpeed
			// 
			this.trkSpeed.Location = new System.Drawing.Point(80, 80);
			this.trkSpeed.Name = "trkSpeed";
			this.trkSpeed.Size = new System.Drawing.Size(152, 45);
			this.trkSpeed.TabIndex = 8;
			this.trkSpeed.TickFrequency = 10;
			this.trkSpeed.Scroll += new System.EventHandler(this.trkSpeed_Scroll);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 56);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 16);
			this.label1.TabIndex = 10;
			this.label1.Text = "Volume";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 88);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 16);
			this.label2.TabIndex = 11;
			this.label2.Text = "Speed Code";
			// 
			// SyringeControlForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(448, 310);
			this.Controls.Add(this.trkSpeed);
			this.Controls.Add(this.trkVolume);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lblSpeedCode);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.lblVolume);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnDispense);
			this.Controls.Add(this.btnAspirate);
			this.Controls.Add(this.btnEmpty);
			this.Controls.Add(this.btnInitialize);
			this.Name = "SyringeControlForm";
			this.Text = "Syringe Control";
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.trkVolume)).EndInit();
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.trkSpeed)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void tmrUpdate_Tick(object sender, System.EventArgs e)
		{
			SyringeStatus Status = syringeControl.PumpStatus();

			this.chkIdle.Checked				= Status.Idle;
			this.chkCmdOverflow.Checked			= Status.CommandOverflow;
			this.chkInit.Checked				= Status.InitializationError;
			this.chkInvalidCmdSeq.Checked		= Status.InvalidCommandSeq;
			this.chkInvalidCommand.Checked		= Status.InvalidCommand;
			this.chkInvalidOperand.Checked		= Status.InvalidOperand;
			this.chkMoveNotAllowed.Checked		= Status.PlungerMoveNotAllowed;
			this.chkNeedInit.Checked			= Status.DeviceNotInitialized;
			this.chkNoError.Checked				= Status.NoError;
			this.chkPlungerOverload.Checked		= Status.PlungerOverload;
			this.chkValveOverload.Checked		= Status.ValveOverload;
		}

		private void btnInitialize_Click(object sender, System.EventArgs e)
		{
			if (syringeControl.InitializePump() != 0)
			{
				System.Windows.Forms.MessageBox.Show("Failed to initialize syringe pump", "SYRINGE ERROR");
			}
		}

		private void btnEmpty_Click(object sender, System.EventArgs e)
		{
			if (syringeControl.EmptySyringe(GetSpeedCode(), false) != 0)
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
			return (double) this.trkVolume.Value;
		}

		private void trkVolume_Scroll(object sender, System.EventArgs e)
		{
			this.lblVolume.Text = string.Format("{0:D} uL", trkVolume.Value);
		}

		private void trkSpeed_Scroll(object sender, System.EventArgs e)
		{
			this.lblSpeedCode.Text = string.Format("{0:D}", trkSpeed.Value);
		}

		private void btnAspirate_Click(object sender, System.EventArgs e)
		{
			if (syringeControl.Aspirate(GetVolume(), GetSpeedCode(), false) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Failed to execute syringe command", "SYRINGE ERROR");
			}
		}

		private void btnDispense_Click(object sender, System.EventArgs e)
		{
			if (syringeControl.Dispense(GetVolume(), GetSpeedCode(), false) != 0)
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
			if (syringeControl.SetValvePosition(Position) != 0)
			{
				System.Windows.Forms.MessageBox.Show("Failed to execute syringe command", "SYRINGE ERROR");
			}
		}
	}
}
