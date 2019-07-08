using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.IO;
using System.Diagnostics;


namespace Aurigin
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class TestDE03 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox tbCosFreq;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button btnCosWaveSetup;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox tbLeading;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox tbDwell;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox tbCosNoInBurst;
		private System.Windows.Forms.TextBox tbCosAmp;
		private System.Windows.Forms.TextBox tbTrailing;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox tbTrapFreq;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox tbTrapDrops;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox tbTrapAmp;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.TextBox tbCommand;
		private System.Windows.Forms.Button btnSendCommand;
		private System.Windows.Forms.TextBox tbReply;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Button btnExit;
		private System.Windows.Forms.Button btnTrapSetup;
		private System.Windows.Forms.TextBox tbPort;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Button btnInitComm;
		private System.Windows.Forms.Button btnInitDisp;
		private System.Windows.Forms.TextBox tbChannel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.TextBox tbStrobeDuration;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.TextBox tbStrobeDelay;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Button btnConvert;
		private System.Windows.Forms.TextBox tbDE02Amp;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.Button btnInitSyringe;
		private System.Windows.Forms.Button btnLaunchSyringeUtil;
		private System.Windows.Forms.GroupBox groupBox7;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.TextBox tbTriggerDelay;
		private System.Windows.Forms.ComboBox cbTriggerSetting;
		private System.Windows.Forms.TextBox tbTriggerPeriod;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.TextBox tbCosNumOfBursts;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public TestDE03()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tbCosFreq = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label18 = new System.Windows.Forms.Label();
			this.tbCosNumOfBursts = new System.Windows.Forms.TextBox();
			this.btnCosWaveSetup = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.tbCosAmp = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.tbCosNoInBurst = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.tbTrapAmp = new System.Windows.Forms.TextBox();
			this.btnTrapSetup = new System.Windows.Forms.Button();
			this.label10 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.tbTrapFreq = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.tbTrailing = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.tbDwell = new System.Windows.Forms.TextBox();
			this.tbLeading = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.tbTrapDrops = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.tbChannel = new System.Windows.Forms.TextBox();
			this.btnInitDisp = new System.Windows.Forms.Button();
			this.label13 = new System.Windows.Forms.Label();
			this.tbPort = new System.Windows.Forms.TextBox();
			this.btnInitComm = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.btnStart = new System.Windows.Forms.Button();
			this.label12 = new System.Windows.Forms.Label();
			this.tbReply = new System.Windows.Forms.TextBox();
			this.btnSendCommand = new System.Windows.Forms.Button();
			this.tbCommand = new System.Windows.Forms.TextBox();
			this.btnExit = new System.Windows.Forms.Button();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.label14 = new System.Windows.Forms.Label();
			this.tbStrobeDelay = new System.Windows.Forms.TextBox();
			this.tbStrobeDuration = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.label15 = new System.Windows.Forms.Label();
			this.btnConvert = new System.Windows.Forms.Button();
			this.tbDE02Amp = new System.Windows.Forms.TextBox();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.btnLaunchSyringeUtil = new System.Windows.Forms.Button();
			this.btnInitSyringe = new System.Windows.Forms.Button();
			this.groupBox7 = new System.Windows.Forms.GroupBox();
			this.label17 = new System.Windows.Forms.Label();
			this.tbTriggerPeriod = new System.Windows.Forms.TextBox();
			this.cbTriggerSetting = new System.Windows.Forms.ComboBox();
			this.label16 = new System.Windows.Forms.Label();
			this.tbTriggerDelay = new System.Windows.Forms.TextBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.groupBox7.SuspendLayout();
			this.SuspendLayout();
			// 
			// tbCosFreq
			// 
			this.tbCosFreq.Location = new System.Drawing.Point(8, 24);
			this.tbCosFreq.Name = "tbCosFreq";
			this.tbCosFreq.Size = new System.Drawing.Size(64, 20);
			this.tbCosFreq.TabIndex = 0;
			this.tbCosFreq.Text = "12750";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label18);
			this.groupBox1.Controls.Add(this.tbCosNumOfBursts);
			this.groupBox1.Controls.Add(this.btnCosWaveSetup);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.tbCosAmp);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.tbCosNoInBurst);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.tbCosFreq);
			this.groupBox1.Location = new System.Drawing.Point(16, 24);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(192, 248);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Cosine Wave Drive Mode";
			// 
			// label18
			// 
			this.label18.Location = new System.Drawing.Point(79, 95);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(88, 13);
			this.label18.TabIndex = 12;
			this.label18.Text = "Num of Bursts";
			// 
			// tbCosNumOfBursts
			// 
			this.tbCosNumOfBursts.Location = new System.Drawing.Point(8, 92);
			this.tbCosNumOfBursts.Name = "tbCosNumOfBursts";
			this.tbCosNumOfBursts.Size = new System.Drawing.Size(64, 20);
			this.tbCosNumOfBursts.TabIndex = 11;
			this.tbCosNumOfBursts.Text = "1";
			// 
			// btnCosWaveSetup
			// 
			this.btnCosWaveSetup.Enabled = false;
			this.btnCosWaveSetup.Location = new System.Drawing.Point(56, 208);
			this.btnCosWaveSetup.Name = "btnCosWaveSetup";
			this.btnCosWaveSetup.TabIndex = 10;
			this.btnCosWaveSetup.Text = "Setup";
			this.btnCosWaveSetup.Click += new System.EventHandler(this.btnCosWaveSetup_Click);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(80, 130);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(96, 16);
			this.label5.TabIndex = 9;
			this.label5.Text = "Amp (0 to 2047)";
			// 
			// tbCosAmp
			// 
			this.tbCosAmp.Location = new System.Drawing.Point(8, 125);
			this.tbCosAmp.Name = "tbCosAmp";
			this.tbCosAmp.Size = new System.Drawing.Size(48, 20);
			this.tbCosAmp.TabIndex = 8;
			this.tbCosAmp.Text = "562";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(80, 54);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(88, 29);
			this.label4.TabIndex = 7;
			this.label4.Text = "Num In burst (per trigger)";
			// 
			// tbCosNoInBurst
			// 
			this.tbCosNoInBurst.Location = new System.Drawing.Point(8, 56);
			this.tbCosNoInBurst.Name = "tbCosNoInBurst";
			this.tbCosNoInBurst.Size = new System.Drawing.Size(64, 20);
			this.tbCosNoInBurst.TabIndex = 6;
			this.tbCosNoInBurst.Text = "10000000";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(80, 27);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(98, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Desired Cos Freq";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.tbTrapAmp);
			this.groupBox2.Controls.Add(this.btnTrapSetup);
			this.groupBox2.Controls.Add(this.label10);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.Controls.Add(this.tbTrapFreq);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.tbTrailing);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.tbDwell);
			this.groupBox2.Controls.Add(this.tbLeading);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.tbTrapDrops);
			this.groupBox2.Controls.Add(this.label11);
			this.groupBox2.Location = new System.Drawing.Point(224, 24);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(208, 248);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Trapezoid Drive Mode";
			// 
			// tbTrapAmp
			// 
			this.tbTrapAmp.Location = new System.Drawing.Point(16, 165);
			this.tbTrapAmp.Name = "tbTrapAmp";
			this.tbTrapAmp.Size = new System.Drawing.Size(48, 20);
			this.tbTrapAmp.TabIndex = 20;
			this.tbTrapAmp.Text = "700";
			// 
			// btnTrapSetup
			// 
			this.btnTrapSetup.Enabled = false;
			this.btnTrapSetup.Location = new System.Drawing.Point(48, 208);
			this.btnTrapSetup.Name = "btnTrapSetup";
			this.btnTrapSetup.TabIndex = 19;
			this.btnTrapSetup.Text = "Setup";
			this.btnTrapSetup.Click += new System.EventHandler(this.btnTrapSetup_Click);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(96, 136);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(96, 16);
			this.label10.TabIndex = 18;
			this.label10.Text = "No of Drops (RC)";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(72, 112);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(88, 16);
			this.label9.TabIndex = 17;
			this.label9.Text = "Frequency (hz)";
			// 
			// tbTrapFreq
			// 
			this.tbTrapFreq.Location = new System.Drawing.Point(16, 107);
			this.tbTrapFreq.Name = "tbTrapFreq";
			this.tbTrapFreq.Size = new System.Drawing.Size(48, 20);
			this.tbTrapFreq.TabIndex = 16;
			this.tbTrapFreq.Text = "300";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(72, 80);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(74, 16);
			this.label8.TabIndex = 15;
			this.label8.Text = "Trailing";
			// 
			// tbTrailing
			// 
			this.tbTrailing.Location = new System.Drawing.Point(16, 77);
			this.tbTrailing.Name = "tbTrailing";
			this.tbTrailing.Size = new System.Drawing.Size(48, 20);
			this.tbTrailing.TabIndex = 14;
			this.tbTrailing.Text = "10";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(72, 52);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(74, 16);
			this.label7.TabIndex = 13;
			this.label7.Text = "Dwell";
			// 
			// tbDwell
			// 
			this.tbDwell.Location = new System.Drawing.Point(16, 51);
			this.tbDwell.Name = "tbDwell";
			this.tbDwell.Size = new System.Drawing.Size(48, 20);
			this.tbDwell.TabIndex = 12;
			this.tbDwell.Text = "15";
			// 
			// tbLeading
			// 
			this.tbLeading.Location = new System.Drawing.Point(16, 24);
			this.tbLeading.Name = "tbLeading";
			this.tbLeading.Size = new System.Drawing.Size(48, 20);
			this.tbLeading.TabIndex = 11;
			this.tbLeading.Text = "3";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(72, 27);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(74, 16);
			this.label6.TabIndex = 11;
			this.label6.Text = "Leading";
			// 
			// tbTrapDrops
			// 
			this.tbTrapDrops.Location = new System.Drawing.Point(16, 136);
			this.tbTrapDrops.Name = "tbTrapDrops";
			this.tbTrapDrops.Size = new System.Drawing.Size(64, 20);
			this.tbTrapDrops.TabIndex = 11;
			this.tbTrapDrops.Text = "10000000";
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(72, 168);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(88, 16);
			this.label11.TabIndex = 11;
			this.label11.Text = "Amp (0 to 4095)";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.label2);
			this.groupBox3.Controls.Add(this.tbChannel);
			this.groupBox3.Controls.Add(this.btnInitDisp);
			this.groupBox3.Controls.Add(this.label13);
			this.groupBox3.Controls.Add(this.tbPort);
			this.groupBox3.Controls.Add(this.btnInitComm);
			this.groupBox3.Controls.Add(this.btnStop);
			this.groupBox3.Controls.Add(this.btnStart);
			this.groupBox3.Controls.Add(this.label12);
			this.groupBox3.Controls.Add(this.tbReply);
			this.groupBox3.Controls.Add(this.btnSendCommand);
			this.groupBox3.Controls.Add(this.tbCommand);
			this.groupBox3.Location = new System.Drawing.Point(16, 400);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(416, 160);
			this.groupBox3.TabIndex = 3;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Commands";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(184, 128);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(104, 16);
			this.label2.TabIndex = 11;
			this.label2.Text = "Channel (1 to 4)";
			// 
			// tbChannel
			// 
			this.tbChannel.Location = new System.Drawing.Point(144, 128);
			this.tbChannel.Name = "tbChannel";
			this.tbChannel.Size = new System.Drawing.Size(24, 20);
			this.tbChannel.TabIndex = 10;
			this.tbChannel.Text = "1";
			// 
			// btnInitDisp
			// 
			this.btnInitDisp.Enabled = false;
			this.btnInitDisp.Location = new System.Drawing.Point(16, 128);
			this.btnInitDisp.Name = "btnInitDisp";
			this.btnInitDisp.Size = new System.Drawing.Size(104, 23);
			this.btnInitDisp.TabIndex = 9;
			this.btnInitDisp.Text = "Initialize Dispense";
			this.btnInitDisp.Click += new System.EventHandler(this.btnInitDisp_Click);
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(184, 99);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(24, 16);
			this.label13.TabIndex = 8;
			this.label13.Text = "port";
			// 
			// tbPort
			// 
			this.tbPort.Location = new System.Drawing.Point(144, 96);
			this.tbPort.Name = "tbPort";
			this.tbPort.Size = new System.Drawing.Size(24, 20);
			this.tbPort.TabIndex = 7;
			this.tbPort.Text = "3";
			// 
			// btnInitComm
			// 
			this.btnInitComm.Location = new System.Drawing.Point(16, 96);
			this.btnInitComm.Name = "btnInitComm";
			this.btnInitComm.Size = new System.Drawing.Size(104, 23);
			this.btnInitComm.TabIndex = 6;
			this.btnInitComm.Text = "Initialize Comm";
			this.btnInitComm.Click += new System.EventHandler(this.btnInitComm_Click);
			// 
			// btnStop
			// 
			this.btnStop.Enabled = false;
			this.btnStop.Location = new System.Drawing.Point(88, 24);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(56, 23);
			this.btnStop.TabIndex = 5;
			this.btnStop.Text = "Stop";
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// btnStart
			// 
			this.btnStart.Enabled = false;
			this.btnStart.Location = new System.Drawing.Point(18, 24);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(54, 23);
			this.btnStart.TabIndex = 4;
			this.btnStart.Text = "Start";
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(224, 64);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(40, 16);
			this.label12.TabIndex = 3;
			this.label12.Text = "Reply";
			// 
			// tbReply
			// 
			this.tbReply.Location = new System.Drawing.Point(272, 62);
			this.tbReply.Name = "tbReply";
			this.tbReply.ReadOnly = true;
			this.tbReply.Size = new System.Drawing.Size(80, 20);
			this.tbReply.TabIndex = 2;
			this.tbReply.Text = "";
			// 
			// btnSendCommand
			// 
			this.btnSendCommand.Enabled = false;
			this.btnSendCommand.Location = new System.Drawing.Point(16, 62);
			this.btnSendCommand.Name = "btnSendCommand";
			this.btnSendCommand.Size = new System.Drawing.Size(104, 23);
			this.btnSendCommand.TabIndex = 1;
			this.btnSendCommand.Text = "Send Command";
			this.btnSendCommand.Click += new System.EventHandler(this.btnSendCommand_Click);
			// 
			// tbCommand
			// 
			this.tbCommand.Location = new System.Drawing.Point(136, 62);
			this.tbCommand.Name = "tbCommand";
			this.tbCommand.Size = new System.Drawing.Size(56, 20);
			this.tbCommand.TabIndex = 0;
			this.tbCommand.Text = "";
			// 
			// btnExit
			// 
			this.btnExit.Location = new System.Drawing.Point(480, 528);
			this.btnExit.Name = "btnExit";
			this.btnExit.TabIndex = 4;
			this.btnExit.Text = "Exit";
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.label14);
			this.groupBox4.Controls.Add(this.tbStrobeDelay);
			this.groupBox4.Controls.Add(this.tbStrobeDuration);
			this.groupBox4.Controls.Add(this.label3);
			this.groupBox4.Location = new System.Drawing.Point(16, 280);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(192, 88);
			this.groupBox4.TabIndex = 5;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Strobe (Used for Cos and Trap)";
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(60, 57);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(88, 16);
			this.label14.TabIndex = 23;
			this.label14.Text = "Delay (usec)";
			// 
			// tbStrobeDelay
			// 
			this.tbStrobeDelay.Location = new System.Drawing.Point(8, 56);
			this.tbStrobeDelay.Name = "tbStrobeDelay";
			this.tbStrobeDelay.Size = new System.Drawing.Size(32, 20);
			this.tbStrobeDelay.TabIndex = 22;
			this.tbStrobeDelay.Text = "0";
			// 
			// tbStrobeDuration
			// 
			this.tbStrobeDuration.Location = new System.Drawing.Point(8, 24);
			this.tbStrobeDuration.Name = "tbStrobeDuration";
			this.tbStrobeDuration.Size = new System.Drawing.Size(32, 20);
			this.tbStrobeDuration.TabIndex = 21;
			this.tbStrobeDuration.Text = "1";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(60, 28);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(88, 16);
			this.label3.TabIndex = 21;
			this.label3.Text = "Duration (usec)";
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.label15);
			this.groupBox5.Controls.Add(this.btnConvert);
			this.groupBox5.Controls.Add(this.tbDE02Amp);
			this.groupBox5.Location = new System.Drawing.Point(448, 128);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(176, 88);
			this.groupBox5.TabIndex = 6;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Convert DE02 Amp to DE03";
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(56, 28);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(112, 16);
			this.label15.TabIndex = 24;
			this.label15.Text = "DE02 Amp (0 to 255)";
			// 
			// btnConvert
			// 
			this.btnConvert.Location = new System.Drawing.Point(48, 56);
			this.btnConvert.Name = "btnConvert";
			this.btnConvert.Size = new System.Drawing.Size(88, 23);
			this.btnConvert.TabIndex = 23;
			this.btnConvert.Text = "<-  Convert";
			this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
			// 
			// tbDE02Amp
			// 
			this.tbDE02Amp.Location = new System.Drawing.Point(16, 24);
			this.tbDE02Amp.Name = "tbDE02Amp";
			this.tbDE02Amp.Size = new System.Drawing.Size(32, 20);
			this.tbDE02Amp.TabIndex = 22;
			this.tbDE02Amp.Text = "255";
			// 
			// groupBox6
			// 
			this.groupBox6.Controls.Add(this.btnLaunchSyringeUtil);
			this.groupBox6.Controls.Add(this.btnInitSyringe);
			this.groupBox6.Location = new System.Drawing.Point(448, 16);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(176, 104);
			this.groupBox6.TabIndex = 7;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Syringe";
			// 
			// btnLaunchSyringeUtil
			// 
			this.btnLaunchSyringeUtil.Location = new System.Drawing.Point(48, 64);
			this.btnLaunchSyringeUtil.Name = "btnLaunchSyringeUtil";
			this.btnLaunchSyringeUtil.Size = new System.Drawing.Size(96, 23);
			this.btnLaunchSyringeUtil.TabIndex = 1;
			this.btnLaunchSyringeUtil.Text = "Launch Utilities";
			this.btnLaunchSyringeUtil.Click += new System.EventHandler(this.btnLaunchSyringeUtil_Click);
			// 
			// btnInitSyringe
			// 
			this.btnInitSyringe.Location = new System.Drawing.Point(56, 24);
			this.btnInitSyringe.Name = "btnInitSyringe";
			this.btnInitSyringe.TabIndex = 0;
			this.btnInitSyringe.Text = "Initialize";
			this.btnInitSyringe.Click += new System.EventHandler(this.btnInitSyringe_Click);
			// 
			// groupBox7
			// 
			this.groupBox7.Controls.Add(this.label17);
			this.groupBox7.Controls.Add(this.tbTriggerPeriod);
			this.groupBox7.Controls.Add(this.cbTriggerSetting);
			this.groupBox7.Controls.Add(this.label16);
			this.groupBox7.Controls.Add(this.tbTriggerDelay);
			this.groupBox7.Location = new System.Drawing.Point(224, 280);
			this.groupBox7.Name = "groupBox7";
			this.groupBox7.Size = new System.Drawing.Size(208, 120);
			this.groupBox7.TabIndex = 8;
			this.groupBox7.TabStop = false;
			this.groupBox7.Text = "External Trigger Setup";
			// 
			// label17
			// 
			this.label17.Location = new System.Drawing.Point(72, 88);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(96, 16);
			this.label17.TabIndex = 26;
			this.label17.Text = "trigger period";
			// 
			// tbTriggerPeriod
			// 
			this.tbTriggerPeriod.Location = new System.Drawing.Point(8, 88);
			this.tbTriggerPeriod.Name = "tbTriggerPeriod";
			this.tbTriggerPeriod.Size = new System.Drawing.Size(56, 20);
			this.tbTriggerPeriod.TabIndex = 25;
			this.tbTriggerPeriod.Text = "1";
			// 
			// cbTriggerSetting
			// 
			this.cbTriggerSetting.Items.AddRange(new object[] {
																  "Off",
																  "Rising Edge",
																  "Falling Edge"});
			this.cbTriggerSetting.Location = new System.Drawing.Point(8, 24);
			this.cbTriggerSetting.Name = "cbTriggerSetting";
			this.cbTriggerSetting.Size = new System.Drawing.Size(120, 21);
			this.cbTriggerSetting.TabIndex = 24;
			this.cbTriggerSetting.Text = "Off";
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(72, 58);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(128, 16);
			this.label16.TabIndex = 23;
			this.label16.Text = "(usec) between triggers";
			// 
			// tbTriggerDelay
			// 
			this.tbTriggerDelay.Location = new System.Drawing.Point(8, 56);
			this.tbTriggerDelay.Name = "tbTriggerDelay";
			this.tbTriggerDelay.Size = new System.Drawing.Size(56, 20);
			this.tbTriggerDelay.TabIndex = 22;
			this.tbTriggerDelay.Text = "10000";
			// 
			// TestDE03
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(648, 573);
			this.Controls.Add(this.groupBox7);
			this.Controls.Add(this.groupBox6);
			this.Controls.Add(this.groupBox5);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.btnExit);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "TestDE03";
			this.Text = "Test DE03";
			this.Load += new System.EventHandler(this.TestDE02_Load);
			this.Closed += new System.EventHandler(this.TestDE02_Closed);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.groupBox6.ResumeLayout(false);
			this.groupBox7.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
//		[STAThread]
//		static void Main() 
//		{
//			Application.Run(new TestDE03());
//		}

		private const int COMMAND_DELAY_BUG = 10;

		private void btnCosWaveSetup_Click(object sender, System.EventArgs e)
		{
			int cosDesiredFreq = 0;
			int cosNoInBurst = 0;
			int cosAmplitude = 0;
			int cosNumOfBursts = 1;
			int strobeDelay = 0;
			int strobeDuration = 1;

			try
			{
				cosDesiredFreq = Convert.ToInt32(tbCosFreq.Text);
				cosNoInBurst = Convert.ToInt32(tbCosNoInBurst.Text);
				cosNumOfBursts = Convert.ToInt32(tbCosNumOfBursts.Text);
				cosAmplitude = Convert.ToInt32(tbCosAmp.Text);
				strobeDelay = Convert.ToInt32(tbStrobeDelay.Text);
				strobeDuration = Convert.ToInt32(tbStrobeDuration.Text);
			}
			catch
			{
				MessageBox.Show("Illegal parameters entered,  try again");
				return;
			}

			int triggerDelay = 0;
			int triggerPeriod = 1;

			try
			{
				triggerDelay = Convert.ToInt32(tbTriggerDelay.Text);
				triggerPeriod = Convert.ToInt32(tbTriggerPeriod.Text);
			}
			catch
			{
				MessageBox.Show("Illegal trigger parameters,  try again");
				return;
			}


			if (DE03.SendCommand("NS1","0",100)!=0) return;   // Number Cycles 1

//			if (DE03.SendCommand("RC1","0",100)!=0) return;   // Repeat Count
//			if (DE03.SendCommand("RP100","0",100)!=0) return;  // Repeat Period

			string commandStr = "FS"+cosDesiredFreq.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // Frequency

			commandStr = "WR"+cosNoInBurst.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // Number of cycles in each burst

			commandStr = "A"+Convert.ToString(cosAmplitude,16);
			if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // Amplitude
			
			commandStr = "FD"+strobeDuration.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // Strobe Duration

			commandStr = "FP"+strobeDelay.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // Strobe Position

			Thread.Sleep(10);    // 10msec delay before the PW command.
			if (DE03.SendCommand("PW","0",100)!=0) return;   // Produce Wave

			Thread.Sleep(10);    // 10msec delay after the PW command.
			commandStr = "RC"+cosNumOfBursts.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // Number of cycles in each burst

			commandStr = "TE"+cbTriggerSetting.SelectedIndex.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // External Trigger Enable (TE0,TE1,TE2)

			commandStr = "RP"+triggerDelay.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // Delay after burst before next trigger or burst

			commandStr = "TP"+triggerPeriod.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // Trigger period


		}

		private void TestDE02_Load(object sender, System.EventArgs e)
		{
		}

		private void btnTrapSetup_Click(object sender, System.EventArgs e)
		{

			int leading,dwell,trailing,trapAmp,trapDrops,trapFreq,strobeDelay,strobeDuration;
			try
			{
				leading=Convert.ToInt32(tbLeading.Text);
				dwell=Convert.ToInt32(tbDwell.Text);
				trailing=Convert.ToInt32(tbTrailing.Text);
				trapDrops=Convert.ToInt32(tbTrapDrops.Text);
				trapFreq=Convert.ToInt32(tbTrapFreq.Text);
				trapAmp = Convert.ToInt32(tbTrapAmp.Text);
				strobeDelay = Convert.ToInt32(tbStrobeDelay.Text);
				strobeDuration = Convert.ToInt32(tbStrobeDuration.Text);

			}
			catch
			{
				MessageBox.Show("Illegal Trapezoid Parameter");
				return;
			}

			int triggerDelay = 0;
			int triggerPeriod = 1;

			try
			{
				triggerDelay = Convert.ToInt32(tbTriggerDelay.Text);
				triggerPeriod = Convert.ToInt32(tbTriggerPeriod.Text);
			}
			catch
			{
				MessageBox.Show("Illegal trigger parameters,  try again");
				return;
			}


			int bufferLength = leading+dwell+trailing+1;
			if ((strobeDelay + strobeDuration) > bufferLength)
			{
				bufferLength = strobeDelay + strobeDuration + 1;
			}

			int [] waveVolt = new int[bufferLength];

			waveVolt[0]=4095;
			int step = trapAmp / leading;   // Use integer math for truncate error is small
			for (int i=1;i<=leading;i++)
			{
				waveVolt[i] = waveVolt[i-1]-step;
			}
			for (int i=(leading+1);i<=(leading+dwell);i++)
			{
				waveVolt[i] = waveVolt[i-1];
			}
			step = trapAmp / trailing;
			for (int i=(leading+dwell+1);i<(leading+dwell+trailing);i++)
			{
				waveVolt[i] = waveVolt[i-1]+step;
			}
			for (int i=(leading+dwell+trailing);i<bufferLength;i++)
			{
				waveVolt[i]=4095;
			}

			DE03.SendCommand("WU"+bufferLength.ToString());   // Don't wait for reply
			System.Threading.Thread.Sleep(50);				
			DE03.SendCommandNoCR("   ");   // Bug in code requires 3 spaces.
			System.Threading.Thread.Sleep(10);				
			bool strobeOn=false;
			string tempString;
			for (int i=0;i<bufferLength;i++)  
			{
				strobeOn=false;
				if (i>=strobeDelay)
				{
					if (i<(strobeDelay+strobeDuration))
					{
						strobeOn=true;
					}
				}
				tempString = Convert.ToString(waveVolt[i],16);
				tempString = tempString.PadLeft(3,'0');
				if (strobeOn)
				{
					tempString = "1"+tempString;
				}
				else
				{
					tempString = "0"+tempString;
				}
				tempString = tempString.ToUpper();
				DE03.SendCommandNoCR(tempString);
				System.Threading.Thread.Sleep(1);
			}
			DE03.SendCommand("");  // CR to wrap things up
									// DE03 responds with Checksum... TODO check this.


			string commandStr = "RC"+trapDrops.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // Number of drops

			if (cbTriggerSetting.SelectedIndex == 0)
			{
				commandStr = "TE0";
				if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // Trigger Enable (TE1 or TE2)
				int trapDelay = (1000000/trapFreq)- bufferLength;  
				if (trapDelay < 0) {trapDelay = 100;}
				commandStr = "RP"+trapDelay.ToString();
				if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // Delay Between drops
			}
			else
			{
				commandStr = "TE"+cbTriggerSetting.SelectedIndex.ToString();
				if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // Trigger Enable (TE1 or TE2)
				commandStr = "RP"+triggerDelay.ToString();
				if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // Delay after wave before next trigger can be used
				commandStr = "TP"+triggerPeriod.ToString();
				if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // Trigger period
			}


			if (DE03.SendCommand("WR1","0",100)!=0) return;   // One Wave

			// In case i want to send this out to file.
			//  outputFile = @"C:\DE03 C# Example App\temp_h.txt";
			//	DE03.WriteToFile(outputFile,outString);

			return;

		}

		private void btnExit_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void btnSendCommand_Click(object sender, System.EventArgs e)
		{
			int ret = DE03.SendCommand(tbCommand.Text);
			Thread.Sleep(50);
			tbReply.Text = DE03.ReadComm();
		}

		private void btnInitComm_Click(object sender, System.EventArgs e)
		{
			int port = Convert.ToInt32(tbPort.Text);
			// Seems to only work for port <=9.
			int ret = DE03.InitTipControl(port);
			if (ret != 0)
			{
				MessageBox.Show("Communication Initiatlization Failed");
				return;
			}

			btnInitDisp.Enabled = true;
		}

		private void TestDE02_Closed(object sender, System.EventArgs e)
		{
			Application.Exit();
		}

		private void btnStart_Click(object sender, System.EventArgs e)
		{
			int ret = DE03.SendCommand("G");
		}

		private void btnStop_Click(object sender, System.EventArgs e)
		{
			int ret = DE03.SendCommand("S");
		}

		private void btnInitDisp_Click(object sender, System.EventArgs e)
		{
			int channel=1;
			DE03.SendCommand("E0");   // Make sure the echo is off.
			Thread.Sleep(50); 
			DE03.SendCommand("E0");   // Twice is required on DE03 powerup for some reason.
			Thread.Sleep(50); 

			if (DE03.SendCommand("Z1","0",100)!=0) return;   // HIGH Voltage On

			try
			{
				channel=Convert.ToInt32(tbChannel.Text);
			}
			catch
			{
				MessageBox.Show("Bad Channel (1 to 4), will use channel 1");
				channel =1;
				tbChannel.Text = "1";
			}

			if ((channel<1)|(channel>4))
			{
				MessageBox.Show("Bad Channel (1 to 4), will use channel 1");
				channel =1;
				tbChannel.Text = "1";
			}

			string commandStr = "CH"+channel.ToString();
			if (DE03.SendCommand(commandStr,"0",100)!=0) return;   // Turn on the Channel
			
			btnCosWaveSetup.Enabled = true;
			btnTrapSetup.Enabled = true;
			btnStart.Enabled = true;
			btnStop.Enabled = true;
			btnSendCommand.Enabled = true;
		}

		private void btnConvert_Click(object sender, System.EventArgs e)
		{
			int de02Amp = 255;
			try
			{
				de02Amp = Convert.ToInt32(tbDE02Amp.Text);
			}
			catch
			{
				MessageBox.Show("Wrong");
				return;
			}
			if ((de02Amp<0)|(de02Amp>255))
			{
				de02Amp = 255;
				tbDE02Amp.Text = "255";
			}
			int cosAmp = Convert.ToInt32(2047 * (de02Amp/255.0));
			tbCosAmp.Text = cosAmp.ToString();
			int trapAmp = Convert.ToInt32(4095 * (de02Amp/255.0));
			tbTrapAmp.Text = trapAmp.ToString();

		}

		private void btnInitSyringe_Click(object sender, System.EventArgs e)
		{
			int errorCode = AtSyringe.Control.InitPumpControl();
		}

		private void btnLaunchSyringeUtil_Click(object sender, System.EventArgs e)
		{
			Thread thread= new Thread(new ThreadStart(SyringeUtilThread));
			thread.Start();
		}

		private void SyringeUtilThread()
		{
			try
			{
				Aurigin.SyringeControlForm syringeForm = new SyringeControlForm();
				syringeForm.ShowDialog();
			}
			finally{}

		}

	}
}
