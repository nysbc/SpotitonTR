using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using EA.PixyControl.ClassLibrary;

// pkV4.0,2015-05-06 Added ability to copy onto the clipboard when btnGetPosition_Click so you can update points in txt files easier.
// PKv5.1,2016-04-27 Added some safety features to prevent crashing when operator accidentally hits move command.  Put non-numeric values
//                     in the move to values so the operator has to input a valid value and then hit move command.

namespace EA.PixyControl
{
	/// <summary>
	/// Summary description for MotionControlForm.
	/// </summary>
	public class MotionControlForm : System.Windows.Forms.Form
	{
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.TextBox txtXPosition;
		private System.Windows.Forms.TextBox txtYPosition;
		private System.Windows.Forms.TextBox txtZPosition;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtZDesired;
		private System.Windows.Forms.TextBox txtYDesired;
		private System.Windows.Forms.TextBox txtXDesired;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button btnMoveX;
		private System.Windows.Forms.Button btnMoveY;
		private System.Windows.Forms.Button btnMoveZ;
		private System.Windows.Forms.CheckBox chkXHomed;
		private System.Windows.Forms.CheckBox chkYHomed;
		private System.Windows.Forms.CheckBox chkZHomed;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button btnMoveAll;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TrackBar trkSpeedX;
		private System.Windows.Forms.Label lblSpeedX;
		private System.Windows.Forms.TrackBar trkSpeedY;
        private System.Windows.Forms.TrackBar trkSpeedZ;
		private System.Windows.Forms.Label lblSpeedY;
		private System.Windows.Forms.Label lblSpeedZ;
		private System.Windows.Forms.Label lblSpeedAll;
		private System.Windows.Forms.Button btnStop;

        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.Button btnSetDefaultsX;
		private System.Windows.Forms.Button btnSetDefaultY;
		private System.Windows.Forms.Button btnSetDefaultZ;
		private System.Windows.Forms.Button btnHomeZ;
		private System.Windows.Forms.Button btnHomeY;
        private System.Windows.Forms.Button btnHomeX;
		private System.Windows.Forms.CheckBox chkMoveAtZSafe;
		private System.Windows.Forms.Button btnInitializeAll;
		private UserMotionControl	motionControl;
        private MachineConfigurationData machineParameters;
		private System.Windows.Forms.Button btnGetPosition;
		private System.Windows.Forms.Button btnMinusX;
		private System.Windows.Forms.Button btnPlusX;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox txtJogmm;
		private System.Windows.Forms.Button btnMinusY;
		private System.Windows.Forms.Button btnPlusY;
		private System.Windows.Forms.Button btnMinusZ;
        private System.Windows.Forms.Button btnPlusZ;
        private Button btnSendCommand;
        private TextBox tbCommand;
        private TextBox tbReply;
        private Label label11;
        private ComboBox cbAxis;
        private Label label12;
        private Button btnHomeT;
        private CheckBox chkTHomed;
        private GroupBox groupBox1;
        private Button btnEnableT;
        private GroupBox groupBox2;
        private TextBox txtTPosition;
        private TextBox txtTDesired;
        private Button btnMoveT;
        private GroupBox groupBox3;
        private Button btnPlusT;
        private Button btnMinusT;
        private GroupBox groupBox4;
        private TrackBar trkSpeedAll;
        private Button btnGetPositionT;
        private Label lblSpeedT;
        private TrackBar trkSpeedT;
        private TextBox txtJogT;
        private Label label1;
        private TrackBar trkSpeedAllGrid;
        private Button btnEnableX2;
        private Button button1;
        private TrackBar trkSpeedY2;
        private Button button2;
        private TextBox txtX2Position;
        private Button button3;
        private TextBox txtY2Position;
        private Button button4;
        private TextBox txtZ2Position;
        private Button button5;
        private TextBox txtX2Desired;
        private Button button6;
        private TextBox txtY2Desired;
        private Button btnGetPositionGrid;
        private TextBox txtZ2Desired;
        private CheckBox chkMoveAtZSafeGrid;
        private Button btnMoveX2;
        private Button btnMoveY2;
        private Button btnMoveZ2;
        private Button btnHomeX2;
        private Button btnEnableZ2;
        private Button btnHomeY2;
        private Button btnEnableY2;
        private Button btnHomeZ2;
        private Button btnMoveAllGrid;
        private CheckBox chkX2Homed;
        private CheckBox chkY2Homed;
        private Label lblSpeedAllGrid;
        private CheckBox chkZ2Homed;
        private Label lblSpeedZ2;
        private Label lblSpeedX2;
        private Label lblSpeedY2;
        private TrackBar trkSpeedX2;
        private TrackBar trkSpeedZ2;
        private GroupBox groupBox5;
        private TextBox txtHDVelocity;
        private TextBox txtHDDecel;
        private TextBox txtHDAccel;
        private Label label10;
        private Label label3;
        private Label label2;
        private Button btnHDSetParam;
        private Button btnInitHDComm;
        private CheckBox chkTCCWHome;
        private Label lblZLimits;
        private Label lblYLimits;
        private Label lblXLimits;
        private Label label14;
        private Label label13;
        private Label lblZ2Limits;
        private Label lblY2Limits;
        private Label lblX2Limits;
        private GroupBox groupBox6;
        private Label label15;
        private Label label16;
        private Button buttonUpdateBackCameraTip3;
        private Button buttonUpdateBackCameraTip2;
        private Button buttonUpdateBackCameraTip1;
        private Button buttonUpdateSideCameraTip3;
        private Button buttonUpdateSideCameraTip2;
        private Button buttonUpdateSideCameraTip1;
        private GroupBox groupBox7;
        private Label label17;
        private Button buttonUpdateBackCameraGrid;
        private Button btnToggleTriggerOutZ2;
        private Button btnToggleTriggerOutX1;
        private CheckBox cbTestTemp;


        // only one instance of this form is allowed
        // if someone creates a new one, get rid of the old one
        private static MotionControlForm	mSingleInstance = null;
		
		
		public MotionControlForm()
		{
			if (mSingleInstance != null)
			{
				try{mSingleInstance.Dispose();}
				catch{}
			}
			
			mSingleInstance = this;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

            InitMoveToValues();

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
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MotionControlForm));
            this.txtXPosition = new System.Windows.Forms.TextBox();
            this.txtYPosition = new System.Windows.Forms.TextBox();
            this.txtZPosition = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtZDesired = new System.Windows.Forms.TextBox();
            this.txtYDesired = new System.Windows.Forms.TextBox();
            this.txtXDesired = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnMoveX = new System.Windows.Forms.Button();
            this.btnMoveY = new System.Windows.Forms.Button();
            this.btnMoveZ = new System.Windows.Forms.Button();
            this.btnHomeZ = new System.Windows.Forms.Button();
            this.btnHomeY = new System.Windows.Forms.Button();
            this.btnHomeX = new System.Windows.Forms.Button();
            this.chkXHomed = new System.Windows.Forms.CheckBox();
            this.chkYHomed = new System.Windows.Forms.CheckBox();
            this.chkZHomed = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.trkSpeedX = new System.Windows.Forms.TrackBar();
            this.btnMoveAll = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.lblSpeedX = new System.Windows.Forms.Label();
            this.trkSpeedY = new System.Windows.Forms.TrackBar();
            this.trkSpeedZ = new System.Windows.Forms.TrackBar();
            this.lblSpeedY = new System.Windows.Forms.Label();
            this.lblSpeedZ = new System.Windows.Forms.Label();
            this.lblSpeedAll = new System.Windows.Forms.Label();
            this.btnStop = new System.Windows.Forms.Button();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.btnSetDefaultsX = new System.Windows.Forms.Button();
            this.btnSetDefaultY = new System.Windows.Forms.Button();
            this.btnSetDefaultZ = new System.Windows.Forms.Button();
            this.chkMoveAtZSafe = new System.Windows.Forms.CheckBox();
            this.btnInitializeAll = new System.Windows.Forms.Button();
            this.btnGetPosition = new System.Windows.Forms.Button();
            this.btnMinusX = new System.Windows.Forms.Button();
            this.btnPlusX = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtJogmm = new System.Windows.Forms.TextBox();
            this.btnMinusY = new System.Windows.Forms.Button();
            this.btnPlusY = new System.Windows.Forms.Button();
            this.btnMinusZ = new System.Windows.Forms.Button();
            this.btnPlusZ = new System.Windows.Forms.Button();
            this.btnSendCommand = new System.Windows.Forms.Button();
            this.tbCommand = new System.Windows.Forms.TextBox();
            this.tbReply = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.cbAxis = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.btnHomeT = new System.Windows.Forms.Button();
            this.chkTHomed = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnToggleTriggerOutX1 = new System.Windows.Forms.Button();
            this.lblZLimits = new System.Windows.Forms.Label();
            this.lblYLimits = new System.Windows.Forms.Label();
            this.lblXLimits = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.trkSpeedAll = new System.Windows.Forms.TrackBar();
            this.btnEnableT = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtTPosition = new System.Windows.Forms.TextBox();
            this.txtTDesired = new System.Windows.Forms.TextBox();
            this.btnMoveT = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkTCCWHome = new System.Windows.Forms.CheckBox();
            this.btnGetPositionT = new System.Windows.Forms.Button();
            this.lblSpeedT = new System.Windows.Forms.Label();
            this.trkSpeedT = new System.Windows.Forms.TrackBar();
            this.txtJogT = new System.Windows.Forms.TextBox();
            this.btnPlusT = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnMinusT = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnToggleTriggerOutZ2 = new System.Windows.Forms.Button();
            this.lblZ2Limits = new System.Windows.Forms.Label();
            this.lblY2Limits = new System.Windows.Forms.Label();
            this.lblX2Limits = new System.Windows.Forms.Label();
            this.trkSpeedAllGrid = new System.Windows.Forms.TrackBar();
            this.trkSpeedZ2 = new System.Windows.Forms.TrackBar();
            this.btnEnableX2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.trkSpeedY2 = new System.Windows.Forms.TrackBar();
            this.button2 = new System.Windows.Forms.Button();
            this.txtX2Position = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.txtY2Position = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.txtZ2Position = new System.Windows.Forms.TextBox();
            this.button5 = new System.Windows.Forms.Button();
            this.txtX2Desired = new System.Windows.Forms.TextBox();
            this.button6 = new System.Windows.Forms.Button();
            this.txtY2Desired = new System.Windows.Forms.TextBox();
            this.btnGetPositionGrid = new System.Windows.Forms.Button();
            this.txtZ2Desired = new System.Windows.Forms.TextBox();
            this.chkMoveAtZSafeGrid = new System.Windows.Forms.CheckBox();
            this.btnMoveX2 = new System.Windows.Forms.Button();
            this.btnMoveY2 = new System.Windows.Forms.Button();
            this.btnMoveZ2 = new System.Windows.Forms.Button();
            this.btnHomeX2 = new System.Windows.Forms.Button();
            this.btnEnableZ2 = new System.Windows.Forms.Button();
            this.btnHomeY2 = new System.Windows.Forms.Button();
            this.btnEnableY2 = new System.Windows.Forms.Button();
            this.btnHomeZ2 = new System.Windows.Forms.Button();
            this.btnMoveAllGrid = new System.Windows.Forms.Button();
            this.chkX2Homed = new System.Windows.Forms.CheckBox();
            this.chkY2Homed = new System.Windows.Forms.CheckBox();
            this.lblSpeedAllGrid = new System.Windows.Forms.Label();
            this.chkZ2Homed = new System.Windows.Forms.CheckBox();
            this.lblSpeedZ2 = new System.Windows.Forms.Label();
            this.lblSpeedX2 = new System.Windows.Forms.Label();
            this.lblSpeedY2 = new System.Windows.Forms.Label();
            this.trkSpeedX2 = new System.Windows.Forms.TrackBar();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.btnInitHDComm = new System.Windows.Forms.Button();
            this.btnHDSetParam = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtHDDecel = new System.Windows.Forms.TextBox();
            this.txtHDAccel = new System.Windows.Forms.TextBox();
            this.txtHDVelocity = new System.Windows.Forms.TextBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.buttonUpdateBackCameraTip3 = new System.Windows.Forms.Button();
            this.buttonUpdateBackCameraTip2 = new System.Windows.Forms.Button();
            this.buttonUpdateBackCameraTip1 = new System.Windows.Forms.Button();
            this.buttonUpdateSideCameraTip3 = new System.Windows.Forms.Button();
            this.buttonUpdateSideCameraTip2 = new System.Windows.Forms.Button();
            this.buttonUpdateSideCameraTip1 = new System.Windows.Forms.Button();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label17 = new System.Windows.Forms.Label();
            this.buttonUpdateBackCameraGrid = new System.Windows.Forms.Button();
            this.cbTestTemp = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedZ)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedAll)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedT)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedAllGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedZ2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedY2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedX2)).BeginInit();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtXPosition
            // 
            this.txtXPosition.Enabled = false;
            this.txtXPosition.Location = new System.Drawing.Point(257, 44);
            this.txtXPosition.Name = "txtXPosition";
            this.txtXPosition.Size = new System.Drawing.Size(72, 20);
            this.txtXPosition.TabIndex = 0;
            this.txtXPosition.Text = "X Position";
            this.txtXPosition.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtYPosition
            // 
            this.txtYPosition.Enabled = false;
            this.txtYPosition.Location = new System.Drawing.Point(257, 76);
            this.txtYPosition.Name = "txtYPosition";
            this.txtYPosition.Size = new System.Drawing.Size(72, 20);
            this.txtYPosition.TabIndex = 1;
            this.txtYPosition.Text = "Y Position";
            this.txtYPosition.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtZPosition
            // 
            this.txtZPosition.Enabled = false;
            this.txtZPosition.Location = new System.Drawing.Point(257, 108);
            this.txtZPosition.Name = "txtZPosition";
            this.txtZPosition.Size = new System.Drawing.Size(72, 20);
            this.txtZPosition.TabIndex = 2;
            this.txtZPosition.Text = "Z Position";
            this.txtZPosition.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label4.Location = new System.Drawing.Point(257, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 16);
            this.label4.TabIndex = 6;
            this.label4.Text = "Current Position";
            // 
            // txtZDesired
            // 
            this.txtZDesired.Location = new System.Drawing.Point(345, 108);
            this.txtZDesired.Name = "txtZDesired";
            this.txtZDesired.Size = new System.Drawing.Size(72, 20);
            this.txtZDesired.TabIndex = 9;
            this.txtZDesired.Text = "Desired Z";
            this.txtZDesired.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtYDesired
            // 
            this.txtYDesired.Location = new System.Drawing.Point(345, 76);
            this.txtYDesired.Name = "txtYDesired";
            this.txtYDesired.Size = new System.Drawing.Size(72, 20);
            this.txtYDesired.TabIndex = 8;
            this.txtYDesired.Text = "Desired Y";
            this.txtYDesired.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtXDesired
            // 
            this.txtXDesired.Location = new System.Drawing.Point(345, 44);
            this.txtXDesired.Name = "txtXDesired";
            this.txtXDesired.Size = new System.Drawing.Size(72, 20);
            this.txtXDesired.TabIndex = 7;
            this.txtXDesired.Text = "Desired X";
            this.txtXDesired.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label5.Location = new System.Drawing.Point(345, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 16);
            this.label5.TabIndex = 10;
            this.label5.Text = "Desired Position";
            // 
            // btnMoveX
            // 
            this.btnMoveX.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveX.Location = new System.Drawing.Point(433, 44);
            this.btnMoveX.Name = "btnMoveX";
            this.btnMoveX.Size = new System.Drawing.Size(72, 24);
            this.btnMoveX.TabIndex = 11;
            this.btnMoveX.Text = "Move X";
            this.btnMoveX.Click += new System.EventHandler(this.btnMoveX_Click);
            // 
            // btnMoveY
            // 
            this.btnMoveY.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveY.Location = new System.Drawing.Point(433, 76);
            this.btnMoveY.Name = "btnMoveY";
            this.btnMoveY.Size = new System.Drawing.Size(72, 24);
            this.btnMoveY.TabIndex = 12;
            this.btnMoveY.Text = "Move Y";
            this.btnMoveY.Click += new System.EventHandler(this.btnMoveY_Click);
            // 
            // btnMoveZ
            // 
            this.btnMoveZ.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveZ.Location = new System.Drawing.Point(433, 108);
            this.btnMoveZ.Name = "btnMoveZ";
            this.btnMoveZ.Size = new System.Drawing.Size(72, 24);
            this.btnMoveZ.TabIndex = 13;
            this.btnMoveZ.Text = "Move Z";
            this.btnMoveZ.Click += new System.EventHandler(this.btnMoveZ_Click);
            // 
            // btnHomeZ
            // 
            this.btnHomeZ.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnHomeZ.Location = new System.Drawing.Point(49, 108);
            this.btnHomeZ.Name = "btnHomeZ";
            this.btnHomeZ.Size = new System.Drawing.Size(56, 24);
            this.btnHomeZ.TabIndex = 16;
            this.btnHomeZ.Text = "Home Z";
            this.btnHomeZ.Click += new System.EventHandler(this.btnHomeZ_Click);
            // 
            // btnHomeY
            // 
            this.btnHomeY.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnHomeY.Location = new System.Drawing.Point(49, 76);
            this.btnHomeY.Name = "btnHomeY";
            this.btnHomeY.Size = new System.Drawing.Size(56, 24);
            this.btnHomeY.TabIndex = 15;
            this.btnHomeY.Text = "Home Y";
            this.btnHomeY.Click += new System.EventHandler(this.btnHomeY_Click);
            // 
            // btnHomeX
            // 
            this.btnHomeX.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnHomeX.Location = new System.Drawing.Point(49, 44);
            this.btnHomeX.Name = "btnHomeX";
            this.btnHomeX.Size = new System.Drawing.Size(56, 24);
            this.btnHomeX.TabIndex = 14;
            this.btnHomeX.Text = "Home X";
            this.btnHomeX.Click += new System.EventHandler(this.btnHomeX_Click);
            // 
            // chkXHomed
            // 
            this.chkXHomed.Enabled = false;
            this.chkXHomed.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkXHomed.Location = new System.Drawing.Point(17, 52);
            this.chkXHomed.Name = "chkXHomed";
            this.chkXHomed.Size = new System.Drawing.Size(16, 16);
            this.chkXHomed.TabIndex = 17;
            // 
            // chkYHomed
            // 
            this.chkYHomed.Enabled = false;
            this.chkYHomed.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkYHomed.Location = new System.Drawing.Point(17, 84);
            this.chkYHomed.Name = "chkYHomed";
            this.chkYHomed.Size = new System.Drawing.Size(16, 16);
            this.chkYHomed.TabIndex = 18;
            // 
            // chkZHomed
            // 
            this.chkZHomed.Enabled = false;
            this.chkZHomed.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkZHomed.Location = new System.Drawing.Point(17, 116);
            this.chkZHomed.Name = "chkZHomed";
            this.chkZHomed.Size = new System.Drawing.Size(16, 16);
            this.chkZHomed.TabIndex = 19;
            // 
            // label6
            // 
            this.label6.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label6.Location = new System.Drawing.Point(9, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 16);
            this.label6.TabIndex = 20;
            this.label6.Text = "Homed";
            // 
            // trkSpeedX
            // 
            this.trkSpeedX.LargeChange = 10;
            this.trkSpeedX.Location = new System.Drawing.Point(633, 44);
            this.trkSpeedX.Maximum = 100;
            this.trkSpeedX.Minimum = 1;
            this.trkSpeedX.Name = "trkSpeedX";
            this.trkSpeedX.Size = new System.Drawing.Size(96, 45);
            this.trkSpeedX.TabIndex = 21;
            this.trkSpeedX.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trkSpeedX.Value = 1;
            this.trkSpeedX.Scroll += new System.EventHandler(this.trkSpeedX_Scroll);
            // 
            // btnMoveAll
            // 
            this.btnMoveAll.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveAll.Location = new System.Drawing.Point(433, 144);
            this.btnMoveAll.Name = "btnMoveAll";
            this.btnMoveAll.Size = new System.Drawing.Size(72, 24);
            this.btnMoveAll.TabIndex = 22;
            this.btnMoveAll.Text = "Move XYZ";
            this.btnMoveAll.Click += new System.EventHandler(this.btnMoveAll_Click);
            // 
            // label7
            // 
            this.label7.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label7.Location = new System.Drawing.Point(665, 20);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 16);
            this.label7.TabIndex = 23;
            this.label7.Text = "Speed";
            // 
            // lblSpeedX
            // 
            this.lblSpeedX.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblSpeedX.Location = new System.Drawing.Point(737, 44);
            this.lblSpeedX.Name = "lblSpeedX";
            this.lblSpeedX.Size = new System.Drawing.Size(48, 16);
            this.lblSpeedX.TabIndex = 24;
            this.lblSpeedX.Text = "1 %";
            // 
            // trkSpeedY
            // 
            this.trkSpeedY.LargeChange = 10;
            this.trkSpeedY.Location = new System.Drawing.Point(633, 76);
            this.trkSpeedY.Maximum = 100;
            this.trkSpeedY.Minimum = 1;
            this.trkSpeedY.Name = "trkSpeedY";
            this.trkSpeedY.Size = new System.Drawing.Size(96, 45);
            this.trkSpeedY.TabIndex = 25;
            this.trkSpeedY.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trkSpeedY.Value = 1;
            this.trkSpeedY.Scroll += new System.EventHandler(this.trkSpeedY_Scroll);
            // 
            // trkSpeedZ
            // 
            this.trkSpeedZ.LargeChange = 10;
            this.trkSpeedZ.Location = new System.Drawing.Point(633, 108);
            this.trkSpeedZ.Maximum = 100;
            this.trkSpeedZ.Minimum = 1;
            this.trkSpeedZ.Name = "trkSpeedZ";
            this.trkSpeedZ.Size = new System.Drawing.Size(96, 45);
            this.trkSpeedZ.TabIndex = 26;
            this.trkSpeedZ.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trkSpeedZ.Value = 1;
            this.trkSpeedZ.Scroll += new System.EventHandler(this.trkSpeedZ_Scroll);
            // 
            // lblSpeedY
            // 
            this.lblSpeedY.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblSpeedY.Location = new System.Drawing.Point(737, 76);
            this.lblSpeedY.Name = "lblSpeedY";
            this.lblSpeedY.Size = new System.Drawing.Size(48, 16);
            this.lblSpeedY.TabIndex = 28;
            this.lblSpeedY.Text = "1 %";
            // 
            // lblSpeedZ
            // 
            this.lblSpeedZ.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblSpeedZ.Location = new System.Drawing.Point(737, 108);
            this.lblSpeedZ.Name = "lblSpeedZ";
            this.lblSpeedZ.Size = new System.Drawing.Size(48, 16);
            this.lblSpeedZ.TabIndex = 29;
            this.lblSpeedZ.Text = "1 %";
            // 
            // lblSpeedAll
            // 
            this.lblSpeedAll.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblSpeedAll.Location = new System.Drawing.Point(738, 144);
            this.lblSpeedAll.Name = "lblSpeedAll";
            this.lblSpeedAll.Size = new System.Drawing.Size(48, 16);
            this.lblSpeedAll.TabIndex = 30;
            this.lblSpeedAll.Text = "1 %";
            // 
            // btnStop
            // 
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnStop.Location = new System.Drawing.Point(12, 52);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(107, 29);
            this.btnStop.TabIndex = 53;
            this.btnStop.Text = "Stop All Axis";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Interval = 250;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // btnSetDefaultsX
            // 
            this.btnSetDefaultsX.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnSetDefaultsX.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSetDefaultsX.Location = new System.Drawing.Point(113, 44);
            this.btnSetDefaultsX.Name = "btnSetDefaultsX";
            this.btnSetDefaultsX.Size = new System.Drawing.Size(66, 24);
            this.btnSetDefaultsX.TabIndex = 54;
            this.btnSetDefaultsX.Text = "Set Default: V,A,D";
            this.btnSetDefaultsX.Click += new System.EventHandler(this.btnSetDefaultX_Click);
            // 
            // btnSetDefaultY
            // 
            this.btnSetDefaultY.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnSetDefaultY.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSetDefaultY.Location = new System.Drawing.Point(113, 76);
            this.btnSetDefaultY.Name = "btnSetDefaultY";
            this.btnSetDefaultY.Size = new System.Drawing.Size(66, 24);
            this.btnSetDefaultY.TabIndex = 58;
            this.btnSetDefaultY.Text = "Set Default: V,A,D";
            this.btnSetDefaultY.Click += new System.EventHandler(this.btnSetDefaultY_Click);
            // 
            // btnSetDefaultZ
            // 
            this.btnSetDefaultZ.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnSetDefaultZ.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSetDefaultZ.Location = new System.Drawing.Point(113, 108);
            this.btnSetDefaultZ.Name = "btnSetDefaultZ";
            this.btnSetDefaultZ.Size = new System.Drawing.Size(66, 24);
            this.btnSetDefaultZ.TabIndex = 59;
            this.btnSetDefaultZ.Text = "Set Default: V,A,D";
            this.btnSetDefaultZ.Click += new System.EventHandler(this.btnSetDefaultZ_Click);
            // 
            // chkMoveAtZSafe
            // 
            this.chkMoveAtZSafe.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkMoveAtZSafe.Location = new System.Drawing.Point(525, 139);
            this.chkMoveAtZSafe.Name = "chkMoveAtZSafe";
            this.chkMoveAtZSafe.Size = new System.Drawing.Size(100, 36);
            this.chkMoveAtZSafe.TabIndex = 64;
            this.chkMoveAtZSafe.Text = "Move XY at Safe Z or Higher";
            // 
            // btnInitializeAll
            // 
            this.btnInitializeAll.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnInitializeAll.Location = new System.Drawing.Point(12, 20);
            this.btnInitializeAll.Name = "btnInitializeAll";
            this.btnInitializeAll.Size = new System.Drawing.Size(107, 26);
            this.btnInitializeAll.TabIndex = 65;
            this.btnInitializeAll.Text = "Init Comm Ports";
            this.btnInitializeAll.Click += new System.EventHandler(this.btnInitializeAll_Click);
            // 
            // btnGetPosition
            // 
            this.btnGetPosition.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnGetPosition.Location = new System.Drawing.Point(257, 144);
            this.btnGetPosition.Name = "btnGetPosition";
            this.btnGetPosition.Size = new System.Drawing.Size(72, 24);
            this.btnGetPosition.TabIndex = 66;
            this.btnGetPosition.Text = "Get Position";
            this.btnGetPosition.Click += new System.EventHandler(this.btnGetPosition_Click);
            // 
            // btnMinusX
            // 
            this.btnMinusX.Location = new System.Drawing.Point(528, 44);
            this.btnMinusX.Name = "btnMinusX";
            this.btnMinusX.Size = new System.Drawing.Size(39, 24);
            this.btnMinusX.TabIndex = 67;
            this.btnMinusX.Text = "- X";
            this.btnMinusX.Click += new System.EventHandler(this.btnMinusX_Click);
            // 
            // btnPlusX
            // 
            this.btnPlusX.Location = new System.Drawing.Point(577, 44);
            this.btnPlusX.Name = "btnPlusX";
            this.btnPlusX.Size = new System.Drawing.Size(39, 24);
            this.btnPlusX.TabIndex = 68;
            this.btnPlusX.Text = "+ X";
            this.btnPlusX.Click += new System.EventHandler(this.btnPlusX_Click);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(433, 20);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(80, 16);
            this.label8.TabIndex = 69;
            this.label8.Text = "Move Desired";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(571, 20);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 16);
            this.label9.TabIndex = 70;
            this.label9.Text = "Jog (mm)";
            // 
            // txtJogmm
            // 
            this.txtJogmm.Location = new System.Drawing.Point(532, 18);
            this.txtJogmm.Name = "txtJogmm";
            this.txtJogmm.Size = new System.Drawing.Size(32, 20);
            this.txtJogmm.TabIndex = 71;
            this.txtJogmm.Text = "0.1";
            // 
            // btnMinusY
            // 
            this.btnMinusY.Location = new System.Drawing.Point(528, 76);
            this.btnMinusY.Name = "btnMinusY";
            this.btnMinusY.Size = new System.Drawing.Size(41, 23);
            this.btnMinusY.TabIndex = 72;
            this.btnMinusY.Text = "-Y";
            this.btnMinusY.Click += new System.EventHandler(this.btnMinusY_Click);
            // 
            // btnPlusY
            // 
            this.btnPlusY.Location = new System.Drawing.Point(577, 76);
            this.btnPlusY.Name = "btnPlusY";
            this.btnPlusY.Size = new System.Drawing.Size(39, 23);
            this.btnPlusY.TabIndex = 73;
            this.btnPlusY.Text = "+Y";
            this.btnPlusY.Click += new System.EventHandler(this.btnPlusY_Click);
            // 
            // btnMinusZ
            // 
            this.btnMinusZ.Location = new System.Drawing.Point(528, 108);
            this.btnMinusZ.Name = "btnMinusZ";
            this.btnMinusZ.Size = new System.Drawing.Size(41, 23);
            this.btnMinusZ.TabIndex = 74;
            this.btnMinusZ.Text = "-Z";
            this.btnMinusZ.Click += new System.EventHandler(this.btnMinusZ_Click);
            // 
            // btnPlusZ
            // 
            this.btnPlusZ.Location = new System.Drawing.Point(577, 108);
            this.btnPlusZ.Name = "btnPlusZ";
            this.btnPlusZ.Size = new System.Drawing.Size(39, 23);
            this.btnPlusZ.TabIndex = 75;
            this.btnPlusZ.Text = "+Z";
            this.btnPlusZ.Click += new System.EventHandler(this.btnPlusZ_Click);
            // 
            // btnSendCommand
            // 
            this.btnSendCommand.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnSendCommand.Location = new System.Drawing.Point(251, 16);
            this.btnSendCommand.Name = "btnSendCommand";
            this.btnSendCommand.Size = new System.Drawing.Size(91, 27);
            this.btnSendCommand.TabIndex = 97;
            this.btnSendCommand.Text = "Send Comand";
            this.btnSendCommand.Click += new System.EventHandler(this.btnSendCommand_Click);
            // 
            // tbCommand
            // 
            this.tbCommand.Location = new System.Drawing.Point(348, 20);
            this.tbCommand.Name = "tbCommand";
            this.tbCommand.Size = new System.Drawing.Size(99, 20);
            this.tbCommand.TabIndex = 98;
            // 
            // tbReply
            // 
            this.tbReply.Location = new System.Drawing.Point(287, 49);
            this.tbReply.Name = "tbReply";
            this.tbReply.Size = new System.Drawing.Size(160, 20);
            this.tbReply.TabIndex = 99;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(230, 52);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(29, 13);
            this.label11.TabIndex = 100;
            this.label11.Text = "reply";
            // 
            // cbAxis
            // 
            this.cbAxis.FormattingEnabled = true;
            this.cbAxis.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6"});
            this.cbAxis.Location = new System.Drawing.Point(178, 19);
            this.cbAxis.Name = "cbAxis";
            this.cbAxis.Size = new System.Drawing.Size(35, 21);
            this.cbAxis.TabIndex = 101;
            this.cbAxis.Text = "1";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(219, 22);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(26, 13);
            this.label12.TabIndex = 102;
            this.label12.Text = "Axis";
            // 
            // btnHomeT
            // 
            this.btnHomeT.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnHomeT.Location = new System.Drawing.Point(49, 41);
            this.btnHomeT.Name = "btnHomeT";
            this.btnHomeT.Size = new System.Drawing.Size(56, 24);
            this.btnHomeT.TabIndex = 103;
            this.btnHomeT.Text = "Home T";
            this.btnHomeT.Click += new System.EventHandler(this.btnHomeT_Click);
            // 
            // chkTHomed
            // 
            this.chkTHomed.Enabled = false;
            this.chkTHomed.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkTHomed.Location = new System.Drawing.Point(17, 44);
            this.chkTHomed.Name = "chkTHomed";
            this.chkTHomed.Size = new System.Drawing.Size(16, 16);
            this.chkTHomed.TabIndex = 104;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnToggleTriggerOutX1);
            this.groupBox1.Controls.Add(this.lblZLimits);
            this.groupBox1.Controls.Add(this.lblYLimits);
            this.groupBox1.Controls.Add(this.lblXLimits);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.trkSpeedAll);
            this.groupBox1.Controls.Add(this.btnPlusZ);
            this.groupBox1.Controls.Add(this.btnMinusZ);
            this.groupBox1.Controls.Add(this.btnPlusY);
            this.groupBox1.Controls.Add(this.btnMinusY);
            this.groupBox1.Controls.Add(this.txtJogmm);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.btnPlusX);
            this.groupBox1.Controls.Add(this.btnMinusX);
            this.groupBox1.Controls.Add(this.btnGetPosition);
            this.groupBox1.Controls.Add(this.chkMoveAtZSafe);
            this.groupBox1.Controls.Add(this.btnSetDefaultZ);
            this.groupBox1.Controls.Add(this.btnSetDefaultY);
            this.groupBox1.Controls.Add(this.btnMoveAll);
            this.groupBox1.Controls.Add(this.btnSetDefaultsX);
            this.groupBox1.Controls.Add(this.lblSpeedAll);
            this.groupBox1.Controls.Add(this.lblSpeedZ);
            this.groupBox1.Controls.Add(this.lblSpeedY);
            this.groupBox1.Controls.Add(this.trkSpeedZ);
            this.groupBox1.Controls.Add(this.trkSpeedY);
            this.groupBox1.Controls.Add(this.lblSpeedX);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.chkZHomed);
            this.groupBox1.Controls.Add(this.chkYHomed);
            this.groupBox1.Controls.Add(this.chkXHomed);
            this.groupBox1.Controls.Add(this.btnHomeZ);
            this.groupBox1.Controls.Add(this.btnHomeY);
            this.groupBox1.Controls.Add(this.btnHomeX);
            this.groupBox1.Controls.Add(this.btnMoveZ);
            this.groupBox1.Controls.Add(this.btnMoveY);
            this.groupBox1.Controls.Add(this.btnMoveX);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtZDesired);
            this.groupBox1.Controls.Add(this.txtYDesired);
            this.groupBox1.Controls.Add(this.txtXDesired);
            this.groupBox1.Controls.Add(this.txtZPosition);
            this.groupBox1.Controls.Add(this.txtYPosition);
            this.groupBox1.Controls.Add(this.txtXPosition);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.trkSpeedX);
            this.groupBox1.Location = new System.Drawing.Point(17, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(799, 186);
            this.groupBox1.TabIndex = 106;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Pipette XYZ Stage";
            // 
            // btnToggleTriggerOutX1
            // 
            this.btnToggleTriggerOutX1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnToggleTriggerOutX1.Location = new System.Drawing.Point(36, 151);
            this.btnToggleTriggerOutX1.Name = "btnToggleTriggerOutX1";
            this.btnToggleTriggerOutX1.Size = new System.Drawing.Size(124, 24);
            this.btnToggleTriggerOutX1.TabIndex = 118;
            this.btnToggleTriggerOutX1.Text = "Toggle Trigger Out X1";
            this.btnToggleTriggerOutX1.Click += new System.EventHandler(this.btnToggleTriggerOutX1_Click);
            // 
            // lblZLimits
            // 
            this.lblZLimits.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblZLimits.Location = new System.Drawing.Point(185, 113);
            this.lblZLimits.Name = "lblZLimits";
            this.lblZLimits.Size = new System.Drawing.Size(60, 17);
            this.lblZLimits.TabIndex = 81;
            // 
            // lblYLimits
            // 
            this.lblYLimits.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblYLimits.Location = new System.Drawing.Point(185, 80);
            this.lblYLimits.Name = "lblYLimits";
            this.lblYLimits.Size = new System.Drawing.Size(60, 17);
            this.lblYLimits.TabIndex = 80;
            // 
            // lblXLimits
            // 
            this.lblXLimits.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblXLimits.Location = new System.Drawing.Point(185, 45);
            this.lblXLimits.Name = "lblXLimits";
            this.lblXLimits.Size = new System.Drawing.Size(66, 21);
            this.lblXLimits.TabIndex = 79;
            // 
            // label14
            // 
            this.label14.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label14.Location = new System.Drawing.Point(111, 11);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(68, 25);
            this.label14.TabIndex = 78;
            this.label14.Text = "Set Default\r\nVel,Acc,Dec";
            this.label14.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label13
            // 
            this.label13.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label13.Location = new System.Drawing.Point(196, 11);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(36, 32);
            this.label13.TabIndex = 77;
            this.label13.Text = "Motion\r\nLimits";
            // 
            // trkSpeedAll
            // 
            this.trkSpeedAll.LargeChange = 10;
            this.trkSpeedAll.Location = new System.Drawing.Point(633, 139);
            this.trkSpeedAll.Maximum = 100;
            this.trkSpeedAll.Minimum = 1;
            this.trkSpeedAll.Name = "trkSpeedAll";
            this.trkSpeedAll.Size = new System.Drawing.Size(96, 45);
            this.trkSpeedAll.TabIndex = 76;
            this.trkSpeedAll.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trkSpeedAll.Value = 1;
            // 
            // btnEnableT
            // 
            this.btnEnableT.Enabled = false;
            this.btnEnableT.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnEnableT.Location = new System.Drawing.Point(113, 40);
            this.btnEnableT.Name = "btnEnableT";
            this.btnEnableT.Size = new System.Drawing.Size(47, 24);
            this.btnEnableT.TabIndex = 76;
            this.btnEnableT.Text = "Enable";
            this.btnEnableT.Click += new System.EventHandler(this.btnEnableT_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cbTestTemp);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.cbAxis);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.tbReply);
            this.groupBox2.Controls.Add(this.tbCommand);
            this.groupBox2.Controls.Add(this.btnSendCommand);
            this.groupBox2.Controls.Add(this.btnInitializeAll);
            this.groupBox2.Controls.Add(this.btnStop);
            this.groupBox2.Location = new System.Drawing.Point(17, 468);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(468, 101);
            this.groupBox2.TabIndex = 107;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Low Level Comm Debug (Lexium Motors)";
            // 
            // txtTPosition
            // 
            this.txtTPosition.Enabled = false;
            this.txtTPosition.Location = new System.Drawing.Point(255, 44);
            this.txtTPosition.Name = "txtTPosition";
            this.txtTPosition.Size = new System.Drawing.Size(72, 20);
            this.txtTPosition.TabIndex = 108;
            this.txtTPosition.Text = "T Position";
            this.txtTPosition.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtTDesired
            // 
            this.txtTDesired.Location = new System.Drawing.Point(343, 45);
            this.txtTDesired.Name = "txtTDesired";
            this.txtTDesired.Size = new System.Drawing.Size(72, 20);
            this.txtTDesired.TabIndex = 76;
            this.txtTDesired.Text = "Desired T";
            this.txtTDesired.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnMoveT
            // 
            this.btnMoveT.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveT.Location = new System.Drawing.Point(431, 42);
            this.btnMoveT.Name = "btnMoveT";
            this.btnMoveT.Size = new System.Drawing.Size(72, 24);
            this.btnMoveT.TabIndex = 76;
            this.btnMoveT.Text = "Move T";
            this.btnMoveT.Click += new System.EventHandler(this.btnMoveT_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkTCCWHome);
            this.groupBox3.Controls.Add(this.btnGetPositionT);
            this.groupBox3.Controls.Add(this.lblSpeedT);
            this.groupBox3.Controls.Add(this.trkSpeedT);
            this.groupBox3.Controls.Add(this.txtJogT);
            this.groupBox3.Controls.Add(this.btnPlusT);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.btnMinusT);
            this.groupBox3.Controls.Add(this.btnMoveT);
            this.groupBox3.Controls.Add(this.txtTDesired);
            this.groupBox3.Controls.Add(this.txtTPosition);
            this.groupBox3.Controls.Add(this.btnEnableT);
            this.groupBox3.Controls.Add(this.chkTHomed);
            this.groupBox3.Controls.Add(this.btnHomeT);
            this.groupBox3.Location = new System.Drawing.Point(17, 191);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(799, 85);
            this.groupBox3.TabIndex = 109;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Theta Stage";
            // 
            // chkTCCWHome
            // 
            this.chkTCCWHome.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkTCCWHome.Location = new System.Drawing.Point(74, 16);
            this.chkTCCWHome.Name = "chkTCCWHome";
            this.chkTCCWHome.Size = new System.Drawing.Size(139, 18);
            this.chkTCCWHome.TabIndex = 109;
            this.chkTCCWHome.Text = "Home in CCW direction";
            // 
            // btnGetPositionT
            // 
            this.btnGetPositionT.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnGetPositionT.Location = new System.Drawing.Point(257, 14);
            this.btnGetPositionT.Name = "btnGetPositionT";
            this.btnGetPositionT.Size = new System.Drawing.Size(72, 24);
            this.btnGetPositionT.TabIndex = 77;
            this.btnGetPositionT.Text = "Get Position";
            this.btnGetPositionT.Click += new System.EventHandler(this.btnGetPositionT_Click);
            // 
            // lblSpeedT
            // 
            this.lblSpeedT.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblSpeedT.Location = new System.Drawing.Point(735, 41);
            this.lblSpeedT.Name = "lblSpeedT";
            this.lblSpeedT.Size = new System.Drawing.Size(48, 16);
            this.lblSpeedT.TabIndex = 77;
            this.lblSpeedT.Text = "1 %";
            // 
            // trkSpeedT
            // 
            this.trkSpeedT.LargeChange = 10;
            this.trkSpeedT.Location = new System.Drawing.Point(633, 35);
            this.trkSpeedT.Maximum = 100;
            this.trkSpeedT.Minimum = 1;
            this.trkSpeedT.Name = "trkSpeedT";
            this.trkSpeedT.Size = new System.Drawing.Size(96, 45);
            this.trkSpeedT.TabIndex = 77;
            this.trkSpeedT.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trkSpeedT.Value = 1;
            // 
            // txtJogT
            // 
            this.txtJogT.Location = new System.Drawing.Point(515, 14);
            this.txtJogT.Name = "txtJogT";
            this.txtJogT.Size = new System.Drawing.Size(47, 20);
            this.txtJogT.TabIndex = 78;
            this.txtJogT.Text = "10000";
            // 
            // btnPlusT
            // 
            this.btnPlusT.Location = new System.Drawing.Point(577, 42);
            this.btnPlusT.Name = "btnPlusT";
            this.btnPlusT.Size = new System.Drawing.Size(39, 23);
            this.btnPlusT.TabIndex = 76;
            this.btnPlusT.Text = "+T";
            this.btnPlusT.Click += new System.EventHandler(this.btnPlusT_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(574, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 16);
            this.label1.TabIndex = 77;
            this.label1.Text = "Jog (counts)";
            // 
            // btnMinusT
            // 
            this.btnMinusT.Location = new System.Drawing.Point(528, 42);
            this.btnMinusT.Name = "btnMinusT";
            this.btnMinusT.Size = new System.Drawing.Size(41, 23);
            this.btnMinusT.TabIndex = 76;
            this.btnMinusT.Text = "-T";
            this.btnMinusT.Click += new System.EventHandler(this.btnMinusT_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnToggleTriggerOutZ2);
            this.groupBox4.Controls.Add(this.lblZ2Limits);
            this.groupBox4.Controls.Add(this.lblY2Limits);
            this.groupBox4.Controls.Add(this.lblX2Limits);
            this.groupBox4.Controls.Add(this.trkSpeedAllGrid);
            this.groupBox4.Controls.Add(this.trkSpeedZ2);
            this.groupBox4.Controls.Add(this.btnEnableX2);
            this.groupBox4.Controls.Add(this.button1);
            this.groupBox4.Controls.Add(this.trkSpeedY2);
            this.groupBox4.Controls.Add(this.button2);
            this.groupBox4.Controls.Add(this.txtX2Position);
            this.groupBox4.Controls.Add(this.button3);
            this.groupBox4.Controls.Add(this.txtY2Position);
            this.groupBox4.Controls.Add(this.button4);
            this.groupBox4.Controls.Add(this.txtZ2Position);
            this.groupBox4.Controls.Add(this.button5);
            this.groupBox4.Controls.Add(this.txtX2Desired);
            this.groupBox4.Controls.Add(this.button6);
            this.groupBox4.Controls.Add(this.txtY2Desired);
            this.groupBox4.Controls.Add(this.btnGetPositionGrid);
            this.groupBox4.Controls.Add(this.txtZ2Desired);
            this.groupBox4.Controls.Add(this.chkMoveAtZSafeGrid);
            this.groupBox4.Controls.Add(this.btnMoveX2);
            this.groupBox4.Controls.Add(this.btnMoveY2);
            this.groupBox4.Controls.Add(this.btnMoveZ2);
            this.groupBox4.Controls.Add(this.btnHomeX2);
            this.groupBox4.Controls.Add(this.btnEnableZ2);
            this.groupBox4.Controls.Add(this.btnHomeY2);
            this.groupBox4.Controls.Add(this.btnEnableY2);
            this.groupBox4.Controls.Add(this.btnHomeZ2);
            this.groupBox4.Controls.Add(this.btnMoveAllGrid);
            this.groupBox4.Controls.Add(this.chkX2Homed);
            this.groupBox4.Controls.Add(this.chkY2Homed);
            this.groupBox4.Controls.Add(this.lblSpeedAllGrid);
            this.groupBox4.Controls.Add(this.chkZ2Homed);
            this.groupBox4.Controls.Add(this.lblSpeedZ2);
            this.groupBox4.Controls.Add(this.lblSpeedX2);
            this.groupBox4.Controls.Add(this.lblSpeedY2);
            this.groupBox4.Controls.Add(this.trkSpeedX2);
            this.groupBox4.Location = new System.Drawing.Point(17, 282);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(799, 180);
            this.groupBox4.TabIndex = 110;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Grid XYZ Stage";
            // 
            // btnToggleTriggerOutZ2
            // 
            this.btnToggleTriggerOutZ2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnToggleTriggerOutZ2.Location = new System.Drawing.Point(49, 135);
            this.btnToggleTriggerOutZ2.Name = "btnToggleTriggerOutZ2";
            this.btnToggleTriggerOutZ2.Size = new System.Drawing.Size(124, 24);
            this.btnToggleTriggerOutZ2.TabIndex = 117;
            this.btnToggleTriggerOutZ2.Text = "Toggle Trigger Out Z2";
            this.btnToggleTriggerOutZ2.Click += new System.EventHandler(this.btnToggleTriggerOutZ2_Click);
            // 
            // lblZ2Limits
            // 
            this.lblZ2Limits.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblZ2Limits.Location = new System.Drawing.Point(185, 96);
            this.lblZ2Limits.Name = "lblZ2Limits";
            this.lblZ2Limits.Size = new System.Drawing.Size(66, 21);
            this.lblZ2Limits.TabIndex = 116;
            // 
            // lblY2Limits
            // 
            this.lblY2Limits.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblY2Limits.Location = new System.Drawing.Point(185, 63);
            this.lblY2Limits.Name = "lblY2Limits";
            this.lblY2Limits.Size = new System.Drawing.Size(66, 21);
            this.lblY2Limits.TabIndex = 115;
            // 
            // lblX2Limits
            // 
            this.lblX2Limits.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblX2Limits.Location = new System.Drawing.Point(185, 28);
            this.lblX2Limits.Name = "lblX2Limits";
            this.lblX2Limits.Size = new System.Drawing.Size(66, 21);
            this.lblX2Limits.TabIndex = 82;
            // 
            // trkSpeedAllGrid
            // 
            this.trkSpeedAllGrid.LargeChange = 10;
            this.trkSpeedAllGrid.Location = new System.Drawing.Point(633, 127);
            this.trkSpeedAllGrid.Maximum = 100;
            this.trkSpeedAllGrid.Minimum = 1;
            this.trkSpeedAllGrid.Name = "trkSpeedAllGrid";
            this.trkSpeedAllGrid.Size = new System.Drawing.Size(96, 45);
            this.trkSpeedAllGrid.TabIndex = 114;
            this.trkSpeedAllGrid.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trkSpeedAllGrid.Value = 1;
            this.trkSpeedAllGrid.Scroll += new System.EventHandler(this.trkSpeedAllGrid_Scroll);
            // 
            // trkSpeedZ2
            // 
            this.trkSpeedZ2.LargeChange = 10;
            this.trkSpeedZ2.Location = new System.Drawing.Point(633, 89);
            this.trkSpeedZ2.Maximum = 100;
            this.trkSpeedZ2.Minimum = 1;
            this.trkSpeedZ2.Name = "trkSpeedZ2";
            this.trkSpeedZ2.Size = new System.Drawing.Size(96, 45);
            this.trkSpeedZ2.TabIndex = 96;
            this.trkSpeedZ2.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trkSpeedZ2.Value = 1;
            this.trkSpeedZ2.Scroll += new System.EventHandler(this.trkSpeedZ2_Scroll);
            // 
            // btnEnableX2
            // 
            this.btnEnableX2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnEnableX2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEnableX2.Location = new System.Drawing.Point(113, 28);
            this.btnEnableX2.Name = "btnEnableX2";
            this.btnEnableX2.Size = new System.Drawing.Size(66, 24);
            this.btnEnableX2.TabIndex = 100;
            this.btnEnableX2.Text = "Set Default: V,A,D";
            this.btnEnableX2.Click += new System.EventHandler(this.btnSetDefaultX2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(577, 92);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(39, 23);
            this.button1.TabIndex = 113;
            this.button1.Text = "+Z";
            this.button1.Click += new System.EventHandler(this.btnPlusZ2_Click);
            // 
            // trkSpeedY2
            // 
            this.trkSpeedY2.LargeChange = 10;
            this.trkSpeedY2.Location = new System.Drawing.Point(633, 57);
            this.trkSpeedY2.Maximum = 100;
            this.trkSpeedY2.Minimum = 1;
            this.trkSpeedY2.Name = "trkSpeedY2";
            this.trkSpeedY2.Size = new System.Drawing.Size(96, 45);
            this.trkSpeedY2.TabIndex = 92;
            this.trkSpeedY2.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trkSpeedY2.Value = 1;
            this.trkSpeedY2.Scroll += new System.EventHandler(this.trkSpeedY2_Scroll);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(528, 92);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(41, 23);
            this.button2.TabIndex = 112;
            this.button2.Text = "-Z";
            this.button2.Click += new System.EventHandler(this.btnMinusZ2_Click);
            // 
            // txtX2Position
            // 
            this.txtX2Position.Enabled = false;
            this.txtX2Position.Location = new System.Drawing.Point(257, 28);
            this.txtX2Position.Name = "txtX2Position";
            this.txtX2Position.Size = new System.Drawing.Size(72, 20);
            this.txtX2Position.TabIndex = 77;
            this.txtX2Position.Text = "X Position";
            this.txtX2Position.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(577, 60);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(39, 23);
            this.button3.TabIndex = 111;
            this.button3.Text = "+Y";
            this.button3.Click += new System.EventHandler(this.btnPlusY2_Click);
            // 
            // txtY2Position
            // 
            this.txtY2Position.Enabled = false;
            this.txtY2Position.Location = new System.Drawing.Point(257, 60);
            this.txtY2Position.Name = "txtY2Position";
            this.txtY2Position.Size = new System.Drawing.Size(72, 20);
            this.txtY2Position.TabIndex = 78;
            this.txtY2Position.Text = "Y Position";
            this.txtY2Position.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(528, 60);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(41, 23);
            this.button4.TabIndex = 110;
            this.button4.Text = "-Y";
            this.button4.Click += new System.EventHandler(this.btnMinusY2_Click);
            // 
            // txtZ2Position
            // 
            this.txtZ2Position.Enabled = false;
            this.txtZ2Position.Location = new System.Drawing.Point(257, 92);
            this.txtZ2Position.Name = "txtZ2Position";
            this.txtZ2Position.Size = new System.Drawing.Size(72, 20);
            this.txtZ2Position.TabIndex = 79;
            this.txtZ2Position.Text = "Z Position";
            this.txtZ2Position.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(577, 28);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(39, 24);
            this.button5.TabIndex = 109;
            this.button5.Text = "+ X";
            this.button5.Click += new System.EventHandler(this.btnPlusX2_Click);
            // 
            // txtX2Desired
            // 
            this.txtX2Desired.Location = new System.Drawing.Point(345, 28);
            this.txtX2Desired.Name = "txtX2Desired";
            this.txtX2Desired.Size = new System.Drawing.Size(72, 20);
            this.txtX2Desired.TabIndex = 80;
            this.txtX2Desired.Text = "Desired X";
            this.txtX2Desired.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(528, 28);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(39, 24);
            this.button6.TabIndex = 108;
            this.button6.Text = "- X";
            this.button6.Click += new System.EventHandler(this.btnMinusX2_Click);
            // 
            // txtY2Desired
            // 
            this.txtY2Desired.Location = new System.Drawing.Point(345, 60);
            this.txtY2Desired.Name = "txtY2Desired";
            this.txtY2Desired.Size = new System.Drawing.Size(72, 20);
            this.txtY2Desired.TabIndex = 81;
            this.txtY2Desired.Text = "Desired Y";
            this.txtY2Desired.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnGetPositionGrid
            // 
            this.btnGetPositionGrid.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnGetPositionGrid.Location = new System.Drawing.Point(257, 128);
            this.btnGetPositionGrid.Name = "btnGetPositionGrid";
            this.btnGetPositionGrid.Size = new System.Drawing.Size(72, 24);
            this.btnGetPositionGrid.TabIndex = 107;
            this.btnGetPositionGrid.Text = "Get Position";
            this.btnGetPositionGrid.Click += new System.EventHandler(this.btnGetPositionGrid_Click);
            // 
            // txtZ2Desired
            // 
            this.txtZ2Desired.Location = new System.Drawing.Point(345, 92);
            this.txtZ2Desired.Name = "txtZ2Desired";
            this.txtZ2Desired.Size = new System.Drawing.Size(72, 20);
            this.txtZ2Desired.TabIndex = 82;
            this.txtZ2Desired.Text = "Desired Z";
            this.txtZ2Desired.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // chkMoveAtZSafeGrid
            // 
            this.chkMoveAtZSafeGrid.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkMoveAtZSafeGrid.Location = new System.Drawing.Point(525, 123);
            this.chkMoveAtZSafeGrid.Name = "chkMoveAtZSafeGrid";
            this.chkMoveAtZSafeGrid.Size = new System.Drawing.Size(100, 36);
            this.chkMoveAtZSafeGrid.TabIndex = 106;
            this.chkMoveAtZSafeGrid.Text = "Move XY at Safe Z or Higher";
            // 
            // btnMoveX2
            // 
            this.btnMoveX2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveX2.Location = new System.Drawing.Point(433, 28);
            this.btnMoveX2.Name = "btnMoveX2";
            this.btnMoveX2.Size = new System.Drawing.Size(72, 24);
            this.btnMoveX2.TabIndex = 83;
            this.btnMoveX2.Text = "Move X2";
            this.btnMoveX2.Click += new System.EventHandler(this.btnMoveX2_Click);
            // 
            // btnMoveY2
            // 
            this.btnMoveY2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveY2.Location = new System.Drawing.Point(433, 60);
            this.btnMoveY2.Name = "btnMoveY2";
            this.btnMoveY2.Size = new System.Drawing.Size(72, 24);
            this.btnMoveY2.TabIndex = 84;
            this.btnMoveY2.Text = "Move Y2";
            this.btnMoveY2.Click += new System.EventHandler(this.btnMoveY2_Click);
            // 
            // btnMoveZ2
            // 
            this.btnMoveZ2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveZ2.Location = new System.Drawing.Point(433, 92);
            this.btnMoveZ2.Name = "btnMoveZ2";
            this.btnMoveZ2.Size = new System.Drawing.Size(72, 24);
            this.btnMoveZ2.TabIndex = 85;
            this.btnMoveZ2.Text = "Move Z2";
            this.btnMoveZ2.Click += new System.EventHandler(this.btnMoveZ2_Click);
            // 
            // btnHomeX2
            // 
            this.btnHomeX2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnHomeX2.Location = new System.Drawing.Point(49, 28);
            this.btnHomeX2.Name = "btnHomeX2";
            this.btnHomeX2.Size = new System.Drawing.Size(56, 24);
            this.btnHomeX2.TabIndex = 86;
            this.btnHomeX2.Text = "Home X2";
            this.btnHomeX2.Click += new System.EventHandler(this.btnHomeX2_Click);
            // 
            // btnEnableZ2
            // 
            this.btnEnableZ2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnEnableZ2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEnableZ2.Location = new System.Drawing.Point(113, 92);
            this.btnEnableZ2.Name = "btnEnableZ2";
            this.btnEnableZ2.Size = new System.Drawing.Size(66, 24);
            this.btnEnableZ2.TabIndex = 102;
            this.btnEnableZ2.Text = "Set Default: V,A,D";
            this.btnEnableZ2.Click += new System.EventHandler(this.btnSetDefaultZ2_Click);
            // 
            // btnHomeY2
            // 
            this.btnHomeY2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnHomeY2.Location = new System.Drawing.Point(49, 60);
            this.btnHomeY2.Name = "btnHomeY2";
            this.btnHomeY2.Size = new System.Drawing.Size(56, 24);
            this.btnHomeY2.TabIndex = 87;
            this.btnHomeY2.Text = "Home Y2";
            this.btnHomeY2.Click += new System.EventHandler(this.btnHomeY2_Click);
            // 
            // btnEnableY2
            // 
            this.btnEnableY2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnEnableY2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEnableY2.Location = new System.Drawing.Point(113, 60);
            this.btnEnableY2.Name = "btnEnableY2";
            this.btnEnableY2.Size = new System.Drawing.Size(66, 24);
            this.btnEnableY2.TabIndex = 101;
            this.btnEnableY2.Text = "Set Default: V,A,D";
            this.btnEnableY2.Click += new System.EventHandler(this.btnSetDefaultY2_Click);
            // 
            // btnHomeZ2
            // 
            this.btnHomeZ2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnHomeZ2.Location = new System.Drawing.Point(49, 92);
            this.btnHomeZ2.Name = "btnHomeZ2";
            this.btnHomeZ2.Size = new System.Drawing.Size(56, 24);
            this.btnHomeZ2.TabIndex = 88;
            this.btnHomeZ2.Text = "Home Z2";
            this.btnHomeZ2.Click += new System.EventHandler(this.btnHomeZ2_Click);
            // 
            // btnMoveAllGrid
            // 
            this.btnMoveAllGrid.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnMoveAllGrid.Location = new System.Drawing.Point(433, 128);
            this.btnMoveAllGrid.Name = "btnMoveAllGrid";
            this.btnMoveAllGrid.Size = new System.Drawing.Size(72, 24);
            this.btnMoveAllGrid.TabIndex = 93;
            this.btnMoveAllGrid.Text = "Move XYZ";
            // 
            // chkX2Homed
            // 
            this.chkX2Homed.Enabled = false;
            this.chkX2Homed.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkX2Homed.Location = new System.Drawing.Point(17, 36);
            this.chkX2Homed.Name = "chkX2Homed";
            this.chkX2Homed.Size = new System.Drawing.Size(16, 16);
            this.chkX2Homed.TabIndex = 89;
            // 
            // chkY2Homed
            // 
            this.chkY2Homed.Enabled = false;
            this.chkY2Homed.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkY2Homed.Location = new System.Drawing.Point(17, 68);
            this.chkY2Homed.Name = "chkY2Homed";
            this.chkY2Homed.Size = new System.Drawing.Size(16, 16);
            this.chkY2Homed.TabIndex = 90;
            // 
            // lblSpeedAllGrid
            // 
            this.lblSpeedAllGrid.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblSpeedAllGrid.Location = new System.Drawing.Point(735, 128);
            this.lblSpeedAllGrid.Name = "lblSpeedAllGrid";
            this.lblSpeedAllGrid.Size = new System.Drawing.Size(48, 16);
            this.lblSpeedAllGrid.TabIndex = 99;
            this.lblSpeedAllGrid.Text = "1 %";
            // 
            // chkZ2Homed
            // 
            this.chkZ2Homed.Enabled = false;
            this.chkZ2Homed.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chkZ2Homed.Location = new System.Drawing.Point(17, 100);
            this.chkZ2Homed.Name = "chkZ2Homed";
            this.chkZ2Homed.Size = new System.Drawing.Size(16, 16);
            this.chkZ2Homed.TabIndex = 91;
            // 
            // lblSpeedZ2
            // 
            this.lblSpeedZ2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblSpeedZ2.Location = new System.Drawing.Point(737, 92);
            this.lblSpeedZ2.Name = "lblSpeedZ2";
            this.lblSpeedZ2.Size = new System.Drawing.Size(48, 16);
            this.lblSpeedZ2.TabIndex = 98;
            this.lblSpeedZ2.Text = "1 %";
            // 
            // lblSpeedX2
            // 
            this.lblSpeedX2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblSpeedX2.Location = new System.Drawing.Point(737, 28);
            this.lblSpeedX2.Name = "lblSpeedX2";
            this.lblSpeedX2.Size = new System.Drawing.Size(48, 16);
            this.lblSpeedX2.TabIndex = 94;
            this.lblSpeedX2.Text = "1 %";
            // 
            // lblSpeedY2
            // 
            this.lblSpeedY2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblSpeedY2.Location = new System.Drawing.Point(737, 60);
            this.lblSpeedY2.Name = "lblSpeedY2";
            this.lblSpeedY2.Size = new System.Drawing.Size(48, 16);
            this.lblSpeedY2.TabIndex = 97;
            this.lblSpeedY2.Text = "1 %";
            // 
            // trkSpeedX2
            // 
            this.trkSpeedX2.LargeChange = 10;
            this.trkSpeedX2.Location = new System.Drawing.Point(633, 24);
            this.trkSpeedX2.Maximum = 100;
            this.trkSpeedX2.Minimum = 1;
            this.trkSpeedX2.Name = "trkSpeedX2";
            this.trkSpeedX2.Size = new System.Drawing.Size(96, 45);
            this.trkSpeedX2.TabIndex = 95;
            this.trkSpeedX2.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trkSpeedX2.Value = 1;
            this.trkSpeedX2.Scroll += new System.EventHandler(this.trkSpeedX2_Scroll);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.btnInitHDComm);
            this.groupBox5.Controls.Add(this.btnHDSetParam);
            this.groupBox5.Controls.Add(this.label10);
            this.groupBox5.Controls.Add(this.label3);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.txtHDDecel);
            this.groupBox5.Controls.Add(this.txtHDAccel);
            this.groupBox5.Controls.Add(this.txtHDVelocity);
            this.groupBox5.Location = new System.Drawing.Point(491, 468);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(324, 101);
            this.groupBox5.TabIndex = 111;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Harmonic Drive Parameters";
            // 
            // btnInitHDComm
            // 
            this.btnInitHDComm.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnInitHDComm.Location = new System.Drawing.Point(259, 54);
            this.btnInitHDComm.Name = "btnInitHDComm";
            this.btnInitHDComm.Size = new System.Drawing.Size(59, 24);
            this.btnInitHDComm.TabIndex = 116;
            this.btnInitHDComm.Text = "Init Comm";
            this.btnInitHDComm.Click += new System.EventHandler(this.btnInitHDComm_Click);
            // 
            // btnHDSetParam
            // 
            this.btnHDSetParam.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnHDSetParam.Location = new System.Drawing.Point(282, 16);
            this.btnHDSetParam.Name = "btnHDSetParam";
            this.btnHDSetParam.Size = new System.Drawing.Size(32, 24);
            this.btnHDSetParam.TabIndex = 115;
            this.btnHDSetParam.Text = "Set";
            this.btnHDSetParam.Click += new System.EventHandler(this.btnHDSetParam_Click);
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(109, 71);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(167, 15);
            this.label10.TabIndex = 81;
            this.label10.Text = "Decel (counts per sec sec)";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(109, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(167, 15);
            this.label3.TabIndex = 80;
            this.label3.Text = "Accel (counts per sec sec)";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(109, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(167, 15);
            this.label2.TabIndex = 77;
            this.label2.Text = "Max Velocity (counts per sec)";
            // 
            // txtHDDecel
            // 
            this.txtHDDecel.Location = new System.Drawing.Point(16, 71);
            this.txtHDDecel.Name = "txtHDDecel";
            this.txtHDDecel.Size = new System.Drawing.Size(70, 20);
            this.txtHDDecel.TabIndex = 79;
            this.txtHDDecel.Text = "25000";
            this.txtHDDecel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtHDAccel
            // 
            this.txtHDAccel.Location = new System.Drawing.Point(16, 45);
            this.txtHDAccel.Name = "txtHDAccel";
            this.txtHDAccel.Size = new System.Drawing.Size(70, 20);
            this.txtHDAccel.TabIndex = 78;
            this.txtHDAccel.Text = "25000";
            this.txtHDAccel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtHDVelocity
            // 
            this.txtHDVelocity.Location = new System.Drawing.Point(16, 20);
            this.txtHDVelocity.Name = "txtHDVelocity";
            this.txtHDVelocity.Size = new System.Drawing.Size(70, 20);
            this.txtHDVelocity.TabIndex = 77;
            this.txtHDVelocity.Text = "1000000";
            this.txtHDVelocity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label15);
            this.groupBox6.Controls.Add(this.label16);
            this.groupBox6.Controls.Add(this.buttonUpdateBackCameraTip3);
            this.groupBox6.Controls.Add(this.buttonUpdateBackCameraTip2);
            this.groupBox6.Controls.Add(this.buttonUpdateBackCameraTip1);
            this.groupBox6.Controls.Add(this.buttonUpdateSideCameraTip3);
            this.groupBox6.Controls.Add(this.buttonUpdateSideCameraTip2);
            this.groupBox6.Controls.Add(this.buttonUpdateSideCameraTip1);
            this.groupBox6.Location = new System.Drawing.Point(822, 2);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(166, 186);
            this.groupBox6.TabIndex = 112;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Update Tip Positions";
            // 
            // label15
            // 
            this.label15.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label15.Location = new System.Drawing.Point(95, 25);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(66, 17);
            this.label15.TabIndex = 82;
            this.label15.Text = "Grid Camera";
            // 
            // label16
            // 
            this.label16.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label16.Location = new System.Drawing.Point(3, 25);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(81, 18);
            this.label16.TabIndex = 113;
            this.label16.Text = "Nozzle  Camera";
            // 
            // buttonUpdateBackCameraTip3
            // 
            this.buttonUpdateBackCameraTip3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonUpdateBackCameraTip3.Location = new System.Drawing.Point(97, 109);
            this.buttonUpdateBackCameraTip3.Name = "buttonUpdateBackCameraTip3";
            this.buttonUpdateBackCameraTip3.Size = new System.Drawing.Size(56, 24);
            this.buttonUpdateBackCameraTip3.TabIndex = 87;
            this.buttonUpdateBackCameraTip3.Text = "Tip 3";
            this.buttonUpdateBackCameraTip3.Click += new System.EventHandler(this.buttonUpdateBackCameraTip3_Click);
            // 
            // buttonUpdateBackCameraTip2
            // 
            this.buttonUpdateBackCameraTip2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonUpdateBackCameraTip2.Location = new System.Drawing.Point(97, 77);
            this.buttonUpdateBackCameraTip2.Name = "buttonUpdateBackCameraTip2";
            this.buttonUpdateBackCameraTip2.Size = new System.Drawing.Size(56, 24);
            this.buttonUpdateBackCameraTip2.TabIndex = 86;
            this.buttonUpdateBackCameraTip2.Text = "Tip 2";
            this.buttonUpdateBackCameraTip2.Click += new System.EventHandler(this.buttonUpdateBackCameraTip2_Click);
            // 
            // buttonUpdateBackCameraTip1
            // 
            this.buttonUpdateBackCameraTip1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonUpdateBackCameraTip1.Location = new System.Drawing.Point(97, 45);
            this.buttonUpdateBackCameraTip1.Name = "buttonUpdateBackCameraTip1";
            this.buttonUpdateBackCameraTip1.Size = new System.Drawing.Size(56, 24);
            this.buttonUpdateBackCameraTip1.TabIndex = 85;
            this.buttonUpdateBackCameraTip1.Text = "Tip 1";
            this.buttonUpdateBackCameraTip1.Click += new System.EventHandler(this.buttonUpdateBackCameraTip1_Click);
            // 
            // buttonUpdateSideCameraTip3
            // 
            this.buttonUpdateSideCameraTip3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonUpdateSideCameraTip3.Location = new System.Drawing.Point(16, 108);
            this.buttonUpdateSideCameraTip3.Name = "buttonUpdateSideCameraTip3";
            this.buttonUpdateSideCameraTip3.Size = new System.Drawing.Size(56, 24);
            this.buttonUpdateSideCameraTip3.TabIndex = 84;
            this.buttonUpdateSideCameraTip3.Text = "Tip 3";
            this.buttonUpdateSideCameraTip3.Click += new System.EventHandler(this.buttonUpdateSideCameraTip3_Click);
            // 
            // buttonUpdateSideCameraTip2
            // 
            this.buttonUpdateSideCameraTip2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonUpdateSideCameraTip2.Location = new System.Drawing.Point(16, 76);
            this.buttonUpdateSideCameraTip2.Name = "buttonUpdateSideCameraTip2";
            this.buttonUpdateSideCameraTip2.Size = new System.Drawing.Size(56, 24);
            this.buttonUpdateSideCameraTip2.TabIndex = 83;
            this.buttonUpdateSideCameraTip2.Text = "Tip 2";
            this.buttonUpdateSideCameraTip2.Click += new System.EventHandler(this.buttonUpdateSideCameraTip2_Click);
            // 
            // buttonUpdateSideCameraTip1
            // 
            this.buttonUpdateSideCameraTip1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonUpdateSideCameraTip1.Location = new System.Drawing.Point(16, 44);
            this.buttonUpdateSideCameraTip1.Name = "buttonUpdateSideCameraTip1";
            this.buttonUpdateSideCameraTip1.Size = new System.Drawing.Size(56, 24);
            this.buttonUpdateSideCameraTip1.TabIndex = 82;
            this.buttonUpdateSideCameraTip1.Text = "Tip 1";
            this.buttonUpdateSideCameraTip1.Click += new System.EventHandler(this.buttonUpdateSideCameraTip1_Click);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.label17);
            this.groupBox7.Controls.Add(this.buttonUpdateBackCameraGrid);
            this.groupBox7.Location = new System.Drawing.Point(822, 282);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(166, 180);
            this.groupBox7.TabIndex = 113;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Update Grid Position";
            // 
            // label17
            // 
            this.label17.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label17.Location = new System.Drawing.Point(15, 28);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(59, 25);
            this.label17.TabIndex = 115;
            this.label17.Text = "In front of camera";
            // 
            // buttonUpdateBackCameraGrid
            // 
            this.buttonUpdateBackCameraGrid.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonUpdateBackCameraGrid.Location = new System.Drawing.Point(16, 59);
            this.buttonUpdateBackCameraGrid.Name = "buttonUpdateBackCameraGrid";
            this.buttonUpdateBackCameraGrid.Size = new System.Drawing.Size(56, 24);
            this.buttonUpdateBackCameraGrid.TabIndex = 114;
            this.buttonUpdateBackCameraGrid.Text = "Grid";
            this.buttonUpdateBackCameraGrid.Click += new System.EventHandler(this.buttonUpdateBackCameraGrid_Click);
            // 
            // cbTestTemp
            // 
            this.cbTestTemp.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbTestTemp.Location = new System.Drawing.Point(131, 73);
            this.cbTestTemp.Name = "cbTestTemp";
            this.cbTestTemp.Size = new System.Drawing.Size(337, 16);
            this.cbTestTemp.TabIndex = 119;
            this.cbTestTemp.Text = "v5.5.5.2 Test (Check ONLY if you know what you are doing)";
            // 
            // MotionControlForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1002, 575);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MotionControlForm";
            this.Text = "Motion Control";
            this.Load += new System.EventHandler(this.MotionControlForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedZ)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedAll)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedT)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedAllGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedZ2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedY2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeedX2)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion


		public void Show(UserMotionControl MotionControl, MachineConfigurationData MachineParameters)
		{

            machineParameters = MachineParameters;
            motionControl = MotionControl;
			
			int		DefaultSpeed_pct = (int) motionControl.DefaultSpeed_pct;
			string	SpeedString = string.Format("{0:D}%",DefaultSpeed_pct);
			
//			this.lblZSafe.Text = string.Format("Safe Z: {0:F3}",motionControl.ZSafe);

			this.trkSpeedX.Value = DefaultSpeed_pct;
			this.lblSpeedX.Text = SpeedString;
			this.trkSpeedY.Value = DefaultSpeed_pct;
			this.lblSpeedY.Text = SpeedString;
			this.trkSpeedZ.Value = DefaultSpeed_pct;
			this.lblSpeedZ.Text = SpeedString;
			this.trkSpeedAll.Value = DefaultSpeed_pct;
			this.lblSpeedAll.Text = SpeedString;
            this.trkSpeedX2.Value = DefaultSpeed_pct;
            this.lblSpeedX2.Text = SpeedString;
            this.trkSpeedY2.Value = DefaultSpeed_pct;
            this.lblSpeedY2.Text = SpeedString;
            this.trkSpeedZ2.Value = DefaultSpeed_pct;
            this.lblSpeedZ2.Text = SpeedString;
            this.trkSpeedAllGrid.Value = DefaultSpeed_pct;
            this.lblSpeedAllGrid.Text = SpeedString;

			base.Show();
		}

        private void InitMoveToValues()
        {
            this.txtXDesired.Text = "x.xxx";
            this.txtYDesired.Text = "y.yyy";
            this.txtZDesired.Text = "z.zzz";
            this.txtTDesired.Text = "t.ttt";
            this.txtX2Desired.Text = "x.xxx";
            this.txtY2Desired.Text = "y.yyy";
            this.txtZ2Desired.Text = "z.zzz";
        }

        // Will stop all axis.

		private void btnStop_Click(object sender, System.EventArgs e)
		{
            // Used to be the following
			if (motionControl.StopAllMotion() != 0) System.Windows.Forms.MessageBox.Show("Stop Failed", "MOTION ERROR");
		}

		private void tmrUpdate_Tick(object sender, System.EventArgs e)
		{
			MachineCoordinate	Position;
			ServoStatus			Status;
			
			if (!motionControl.HardwareInitialized) return;

			return;  // temp PK

			motionControl.GetCurrentPosition(out Position);

			this.txtXPosition.Text = string.Format("{0:F3}", Position.X);
			this.txtYPosition.Text = string.Format("{0:F3}", Position.Y);
			this.txtZPosition.Text = string.Format("{0:F3}", Position.Z);

            updateAxisStatus();
			
		}

        private void updateAxisStatus()
        {

            ServoStatus Status;

            this.chkXHomed.Checked = motionControl.AxisHomed(ServoControl.X_AXIS);
            this.chkYHomed.Checked = motionControl.AxisHomed(ServoControl.Y_AXIS);
            this.chkZHomed.Checked = motionControl.AxisHomed(ServoControl.Z_AXIS);

            Status = motionControl.GetAxisStatus(ServoControl.X_AXIS);
            Status = motionControl.GetAxisStatus(ServoControl.Y_AXIS);
            Status = motionControl.GetAxisStatus(ServoControl.Z_AXIS);

        }


        private void updateAxisGridStatus()
        {

            ServoStatus Status;

            this.chkX2Homed.Checked = motionControl.AxisHomed(ServoControl.X2_AXIS);
            this.chkY2Homed.Checked = motionControl.AxisHomed(ServoControl.Y2_AXIS);
            this.chkZ2Homed.Checked = motionControl.AxisHomed(ServoControl.Z2_AXIS);

            Status = motionControl.GetAxisStatus(ServoControl.X2_AXIS);
            Status = motionControl.GetAxisStatus(ServoControl.Y2_AXIS);
            Status = motionControl.GetAxisStatus(ServoControl.Z2_AXIS);

        }

        private void btnSetDefaultX_Click(object sender, System.EventArgs e)
        {
            SetDefaults(ServoControl.X_AXIS);
        }

        private void btnSetDefaultY_Click(object sender, System.EventArgs e)
        {
            SetDefaults(ServoControl.Y_AXIS);
        }

        private void btnSetDefaultZ_Click(object sender, System.EventArgs e)
        {
            SetDefaults(ServoControl.Z_AXIS);
        }

        private void btnSetDefaultX2_Click(object sender, System.EventArgs e)
        {
            SetDefaults(ServoControl.X2_AXIS);
        }

        private void btnSetDefaultY2_Click(object sender, System.EventArgs e)
        {
            SetDefaults(ServoControl.Y2_AXIS);
        }

        private void btnSetDefaultZ2_Click(object sender, System.EventArgs e)
        {
            SetDefaults(ServoControl.Z2_AXIS);
        }

        private void SetDefaults(int Axis)
        {
            bool useGridRobot = false;
            if (Axis > ServoControl.Z_AXIS)
            {
                useGridRobot = true;
                Axis = Axis - 3;
            }
            if (motionControl.SetDefaultMotionParameters((object)this, Axis, useGridRobot) != 0) System.Windows.Forms.MessageBox.Show("Set Default Motion Parameters Failed", "MOTION ERROR");
            if (motionControl.ClearError((object)this, Axis) != 0) System.Windows.Forms.MessageBox.Show("Clearing Error Error !", "MOTION ERROR");
        }



		private void btnEnableX_Click(object sender, System.EventArgs e)
		{
			EnableAxis(ServoControl.X_AXIS, true);
		}

		private void btnEnableY_Click(object sender, System.EventArgs e)
		{
			EnableAxis(ServoControl.Y_AXIS, true);
		}

		private void btnEnableZ_Click(object sender, System.EventArgs e)
		{
			EnableAxis(ServoControl.Z_AXIS, true);
		}

        private void btnEnableZ2_Click(object sender, System.EventArgs e)
        {
 //           EnableAxis(ServoControl.Z2_AXIS, true);
        }

		private void EnableAxis(int Axis, bool Enable)
		{
			if (motionControl.EnableAxis((object)this, Axis, Enable)  != 0) System.Windows.Forms.MessageBox.Show("Enable / Disable Failed", "MOTION ERROR");
		}

 

		private void btnHomeX_Click(object sender, System.EventArgs e)
		{
			HomeAxis(ServoControl.X_AXIS);
		}

		private void btnHomeY_Click(object sender, System.EventArgs e)
		{
			HomeAxis(ServoControl.Y_AXIS);
		}

		private void btnHomeZ_Click(object sender, System.EventArgs e)
		{
			HomeAxis(ServoControl.Z_AXIS);
		}

        private void btnHomeX2_Click(object sender, EventArgs e)
        {
            HomeAxis(ServoControl.X2_AXIS);
        }

        private void btnHomeY2_Click(object sender, EventArgs e)
        {
            HomeAxis(ServoControl.Y2_AXIS);
        }

        private void btnHomeZ2_Click(object sender, EventArgs e)
        {
            HomeAxis(ServoControl.Z2_AXIS);
        }

        // PKv5.2.5 ..  Added for allowing a quick test of the trigger output from the motor.

        private void btnToggleTriggerOutZ2_Click(object sender, EventArgs e)
        {
            motionControl.ToggleTriggerOutput(sender, 2, true);
        }

        private void btnToggleTriggerOutX1_Click(object sender, EventArgs e)
        {
            motionControl.ToggleTriggerOutput(sender, 0, false);
        }

        private void HomeAxis(int Axis)
		{
            if (motionControl.HomeAxis((object)this, Axis) != 0)
            {
                System.Windows.Forms.MessageBox.Show("Home Failed", "MOTION ERROR");
                return;
            }
            if (Axis <= ServoControl.Z_AXIS)
            {
                if (motionControl.SetDefaultMotionParameters((object)this, Axis, false) != 0) return;
                this.updateAxisStatus();
            }
            else
            {
                this.updateAxisGridStatus();
                if (motionControl.SetDefaultMotionParameters((object)this, Axis-3, true) != 0) return;
            }

            if (motionControl.ClearError((object)this, Axis) != 0) System.Windows.Forms.MessageBox.Show("Clearing Error Error !", "MOTION ERROR");
 
            return;
		}

		private void trkSpeedX_Scroll(object sender, System.EventArgs e)
		{
			this.lblSpeedX.Text = string.Format("{0:D}%", this.trkSpeedX.Value);
		}

		private void trkSpeedY_Scroll(object sender, System.EventArgs e)
		{
			this.lblSpeedY.Text = string.Format("{0:D}%", this.trkSpeedY.Value);
		}

		private void trkSpeedZ_Scroll(object sender, System.EventArgs e)
		{
			this.lblSpeedZ.Text = string.Format("{0:D}%", this.trkSpeedZ.Value);
		}

		private void trkSpeedAll_Scroll(object sender, System.EventArgs e)
		{
			this.lblSpeedAll.Text = string.Format("{0:D}%", this.trkSpeedAll.Value);
		}

        private void trkSpeedX2_Scroll(object sender, System.EventArgs e)
        {
            this.lblSpeedX2.Text = string.Format("{0:D}%", this.trkSpeedX2.Value);
        }

        private void trkSpeedY2_Scroll(object sender, System.EventArgs e)
        {
            this.lblSpeedY2.Text = string.Format("{0:D}%", this.trkSpeedY2.Value);
        }

        private void trkSpeedZ2_Scroll(object sender, System.EventArgs e)
        {
            this.lblSpeedZ2.Text = string.Format("{0:D}%", this.trkSpeedZ2.Value);
        }

        private void trkSpeedAllGrid_Scroll(object sender, System.EventArgs e)
        {
            this.lblSpeedAllGrid.Text = string.Format("{0:D}%", this.trkSpeedAllGrid.Value);
        }

		private void btnMoveX_Click(object sender, System.EventArgs e)
		{
			bool	Invalid;
			int		SpeedPct;


			MachineCoordinate	DesiredPosition = new MachineCoordinate();

			motionControl.GetCurrentPosition(out DesiredPosition);
			DesiredPosition.X = GetXCoordinate(out Invalid);
			SpeedPct = this.trkSpeedX.Value;

			if (Invalid) return;

			MachineMove(DesiredPosition, SpeedPct, false);  
			getAndUpdatePosition();
		}

		private void btnMoveY_Click(object sender, System.EventArgs e)
		{
			bool	Invalid;
			int		SpeedPct;
	
			double	YAxisLoc_mm;		
			YAxisLoc_mm = GetYCoordinate(out Invalid);  

			MachineCoordinate	DesiredPosition = new MachineCoordinate();

			motionControl.GetCurrentPosition(out DesiredPosition);
			DesiredPosition.Y = GetYCoordinate(out Invalid);
			SpeedPct = this.trkSpeedY.Value;

			if (Invalid) return;

			MachineMove(DesiredPosition, SpeedPct, false);
			getAndUpdatePosition();
		}

		private void btnMoveZ_Click(object sender, System.EventArgs e)
		{
			bool	Invalid;
			int		SpeedPct;

			double	ZAxisLoc_mm;		
			ZAxisLoc_mm = GetZCoordinate(out Invalid);  

			MachineCoordinate	DesiredPosition = new MachineCoordinate();

			motionControl.GetCurrentPosition(out DesiredPosition);
			DesiredPosition.Z = GetZCoordinate(out Invalid);
			SpeedPct = this.trkSpeedZ.Value;

			if (Invalid) return;

			//For now lets just move the one axis (Debug purposes only)  // PK Hack
//			motionControl.MoveZUnsafe((object) this,  ZAxisLoc_mm,  (double) SpeedPct,  true);

			MachineMove(DesiredPosition, SpeedPct, true);
			getAndUpdatePosition();
		}

        // PKv4.0,2015-04-06
        private void btnMoveX2_Click(object sender, EventArgs e)
        {
            bool Invalid;
            int SpeedPct;

            bool useGridRobot = true;

            MachineCoordinate DesiredPosition = new MachineCoordinate();

            motionControl.GetCurrentPosition(out DesiredPosition, useGridRobot);
            DesiredPosition.X = GetX2Coordinate(out Invalid);
            SpeedPct = this.trkSpeedX2.Value;

            if (Invalid) return;

            MachineMove(DesiredPosition, SpeedPct, false, useGridRobot);
            getAndUpdatePositionGrid();

        }

        private void btnMoveY2_Click(object sender, EventArgs e)
        {
            bool Invalid;
            int SpeedPct;

            bool useGridRobot = true;

            MachineCoordinate DesiredPosition = new MachineCoordinate();

            motionControl.GetCurrentPosition(out DesiredPosition, useGridRobot);
            DesiredPosition.Y = GetY2Coordinate(out Invalid);
            SpeedPct = this.trkSpeedY2.Value;

            if (Invalid) return;

            MachineMove(DesiredPosition, SpeedPct, false, useGridRobot);
            getAndUpdatePositionGrid();

        }

        private void btnMoveZ2_Click(object sender, System.EventArgs e)
        {
            bool Invalid;
            int SpeedPct;

            bool useGridRobot = true;

            MachineCoordinate DesiredPosition = new MachineCoordinate();

            motionControl.GetCurrentPosition(out DesiredPosition, useGridRobot);
            DesiredPosition.Z = GetZ2Coordinate(out Invalid);
            SpeedPct = this.trkSpeedZ2.Value;

            if (Invalid) return;

            MachineMove(DesiredPosition, SpeedPct, false, useGridRobot);
            getAndUpdatePositionGrid();
            return;
  
            
            
            
            
            /*  
            bool Invalid;
            int SpeedPct;

            double DesiredZ2Pos_mm;
            DesiredZ2Pos_mm = GetZ2Coordinate(out Invalid);     // Gets desired z2 location from text box
            double CurrentZ2Pos_mm;
            motionControl.GetCurrentPosition(ServoControl.Z2_AXIS, out CurrentZ2Pos_mm);
 
            SpeedPct = this.trkSpeedZ2.Value;

            if (Invalid) return;

            if (motionControl.MoveZ2Only((object)this, DesiredZ2Pos_mm, (double)SpeedPct, true) != 0) System.Windows.Forms.MessageBox.Show("Move Failed", "MOTION ERROR");

            getAndUpdatePosition();
            */
        }

		private void btnMoveAll_Click(object sender, System.EventArgs e)
		{
			bool	Invalid;
			int		SpeedPct;
	
			MachineCoordinate	DesiredPosition = new MachineCoordinate();
			
			DesiredPosition.X = GetXCoordinate(out Invalid);
			if (Invalid) return;

			DesiredPosition.Y = GetYCoordinate(out Invalid);
			if (Invalid) return;
			
			DesiredPosition.Z = GetZCoordinate(out Invalid);
			if (Invalid) return;

			SpeedPct = this.trkSpeedAll.Value;

			MachineMove(DesiredPosition, SpeedPct, false);
			getAndUpdatePosition();
		}

		private double GetXCoordinate(out bool Invalid)
		{
			double	Coordinate = 0.0;

			Invalid = false;

			try
			{
				Coordinate = System.Convert.ToDouble(this.txtXDesired.Text);
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("Invalid X coordinate", "ERROR");
				Invalid = true;
			}

			return Coordinate;
		}

        private double GetX2Coordinate(out bool Invalid)
        {
            double Coordinate = 0.0;

            Invalid = false;

            try
            {
                Coordinate = System.Convert.ToDouble(this.txtX2Desired.Text);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Invalid X2 coordinate", "ERROR");
                Invalid = true;
            }

            return Coordinate;
        }

		
		private double GetYCoordinate(out bool Invalid)
		{
			double	Coordinate = 0.0;

			Invalid = false;

			try
			{
				Coordinate = System.Convert.ToDouble(this.txtYDesired.Text);
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("Invalid Y coordinate", "ERROR");
				Invalid = true;
			}

			return Coordinate;
		}

        private double GetY2Coordinate(out bool Invalid)
        {
            double Coordinate = 0.0;

            Invalid = false;

            try
            {
                Coordinate = System.Convert.ToDouble(this.txtY2Desired.Text);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Invalid Y2 coordinate", "ERROR");
                Invalid = true;
            }

            return Coordinate;
        }

		private double GetZCoordinate(out bool Invalid)
		{
			double	Coordinate = 0.0;

			Invalid = false;

			try
			{
				Coordinate = System.Convert.ToDouble(this.txtZDesired.Text);
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("Invalid Z coordinate", "ERROR");
				Invalid = true;
			}

			return Coordinate;
		}

        private double GetZ2Coordinate(out bool Invalid)
        {
            double Coordinate = 0.0;

            Invalid = false;

            try
            {
                Coordinate = System.Convert.ToDouble(this.txtZ2Desired.Text);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Invalid Z2 coordinate", "ERROR");
                Invalid = true;
            }

            return Coordinate;
        }

        private int GetTCoordinate(out bool Invalid)
        {
            int Coordinate = 0;

            Invalid = false;

            try
            {
                Coordinate = System.Convert.ToInt32(this.txtTDesired.Text);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Invalid T coordinate", "ERROR");
                Invalid = true;
            }

            return Coordinate;
        }

  
		private double GetJogValue(out bool Invalid)
		{
			double	JogValue = 0.1;

			Invalid = false;

			try
			{
				JogValue = System.Convert.ToDouble(this.txtJogmm.Text);
			}
			catch
			{
				System.Windows.Forms.MessageBox.Show("Invalid Jog Value", "ERROR");
				this.txtJogmm.Text = "0.1";
				Invalid = true;
			}

			if ((JogValue>100)||(JogValue<0))
			{
				System.Windows.Forms.MessageBox.Show("Invalid Jog Value (Max =100, min=0)", "ERROR");
				this.txtJogmm.Text = "0.1";
				Invalid = true;
			}
			return JogValue;
		}

        private int GetJogValueT(out bool Invalid)
        {
            int jogValue = 0;

            Invalid = false;

            try
            {
                jogValue = System.Convert.ToInt32(this.txtJogT.Text);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Invalid Jog Value", "ERROR");
                this.txtJogT.Text = "0";
                Invalid = true;
            }

            if ((jogValue > 20000) || (jogValue < 0))
            {
                System.Windows.Forms.MessageBox.Show("Invalid Jog Value (Max =20000, min=0)", "ERROR");
                this.txtJogT.Text = "0";
                Invalid = true;
            }
            return jogValue;
        }

        //PKv4.0,2015-04-16  Updated for grid robot.

        private void MachineMove(MachineCoordinate Location, int Speed_pct, bool ZMoveOnly, bool useGridRobot)
        {
            if (useGridRobot)
            {
                MachineCoordinate Tool = new MachineCoordinate();

                // moving to a safe z before a z move would just look wrong
                if (this.chkMoveAtZSafeGrid.Checked && !ZMoveOnly)
                {
                    // if z is below z-safe, first move to a safe height
                    if (motionControl.MoveZToSafeHeight((object)this, true) != 0)
                    {
                        System.Windows.Forms.MessageBox.Show("Error moving to safe height", "MOTION ERROR");
                        return;
                    }
                }

                //27Feb2008 - Change to wait for move to complete.
                if (motionControl.MoveSafely((object)this, Tool, Location, 0.0, (double)Speed_pct, true, true,useGridRobot,false) != 0) System.Windows.Forms.MessageBox.Show("Move Failed", "MOTION ERROR");
            }
            else
            {
                MachineCoordinate Tool = new MachineCoordinate();

                // moving to a safe z before a z move would just look wrong
                if (this.chkMoveAtZSafe.Checked && !ZMoveOnly)
                {
                    // if z is below z-safe, first move to a safe height
                    if (motionControl.MoveZToSafeHeight((object)this, true) != 0)
                    {
                        System.Windows.Forms.MessageBox.Show("Error moving to safe height", "MOTION ERROR");
                        return;
                    }
                }

                //27Feb2008 - Change to wait for move to complete.
                if (motionControl.MoveSafely((object)this, Tool, Location, 0.0, (double)Speed_pct, true, true) != 0) System.Windows.Forms.MessageBox.Show("Move Failed", "MOTION ERROR");
            }
        }


		private void MachineMove(MachineCoordinate Location, int Speed_pct, bool ZMoveOnly)
		{
            MachineMove(Location, Speed_pct, ZMoveOnly, false);

		}

		private void btnDisableX_Click(object sender, System.EventArgs e)
		{
			EnableAxis(ServoControl.X_AXIS, false);
		}

		private void btnDisableY_Click(object sender, System.EventArgs e)
		{
			EnableAxis(ServoControl.Y_AXIS, false);
		}

		private void btnDisableZ_Click(object sender, System.EventArgs e)
		{
			EnableAxis(ServoControl.Z_AXIS, false);
		}

		private void btnInitializeAll_Click(object sender, System.EventArgs e)
		{
			this.tmrUpdate.Enabled = false;  // PK Disable the timer while we reinitialize the 
			// Initialize motion (skip homing for now PKv3.0);
			if (motionControl.InitializeMotion((object)this,true) != 0) System.Windows.Forms.MessageBox.Show("Initialize Failed", "MOTION ERROR");
			this.tmrUpdate.Enabled = true;  // PK reinitialize the timer
		}

		private void btnGetPosition_Click(object sender, System.EventArgs e)
		{

			getAndUpdatePosition();

			this.chkXHomed.Checked = motionControl.AxisHomed(ServoControl.X_AXIS);
			this.chkYHomed.Checked = motionControl.AxisHomed(ServoControl.Y_AXIS);
			this.chkZHomed.Checked = motionControl.AxisHomed(ServoControl.Z_AXIS);

            this.lblXLimits.Text = motionControl.GetLimitString(ServoControl.X_AXIS);
            this.lblYLimits.Text = motionControl.GetLimitString(ServoControl.Y_AXIS);
            this.lblZLimits.Text = motionControl.GetLimitString(ServoControl.Z_AXIS);
		
		}


        // pkV4.0,2015-05-06,  copy to clipboard too.
 
		private void getAndUpdatePosition()
		{
			MachineCoordinate	Position;

			motionControl.GetCurrentPosition(out Position);

			this.txtXPosition.Text = string.Format("{0:F3}", Position.X);
			this.txtYPosition.Text = string.Format("{0:F3}", Position.Y);
			this.txtZPosition.Text = string.Format("{0:F3}", Position.Z);

            string x = string.Format("{0:0.00}", Position.X);
            string y = string.Format("{0:0.00}", Position.Y);
            string z = string.Format("{0:0.00}", Position.Z);

            string tempString = string.Format("    <X>{0}</X>\r\n    <Y>{1}</Y>\r\n    <Z>{2}</Z>", x, y, z);
            Clipboard.SetText(tempString);

            this.txtXDesired.Text = "x.xxx";
            this.txtYDesired.Text = "y.yyy";
            this.txtZDesired.Text = "z.zzz";

		}

        private void btnGetPositionGrid_Click(object sender, EventArgs e)
        {
            getAndUpdatePositionGrid();

            this.chkX2Homed.Checked = motionControl.AxisHomed(ServoControl.X2_AXIS);
            this.chkY2Homed.Checked = motionControl.AxisHomed(ServoControl.Y2_AXIS);
            this.chkZ2Homed.Checked = motionControl.AxisHomed(ServoControl.Z2_AXIS);

            this.lblX2Limits.Text = motionControl.GetLimitString(ServoControl.X2_AXIS);
            this.lblY2Limits.Text = motionControl.GetLimitString(ServoControl.Y2_AXIS);
            this.lblZ2Limits.Text = motionControl.GetLimitString(ServoControl.Z2_AXIS);

        }

        private void getAndUpdatePositionGrid()
        {
            MachineCoordinate Position;

            motionControl.GetCurrentPosition(out Position,true);

            this.txtX2Position.Text = string.Format("{0:F3}", Position.X);
            this.txtY2Position.Text = string.Format("{0:F3}", Position.Y);
            this.txtZ2Position.Text = string.Format("{0:F3}", Position.Z);

            string x = string.Format("{0:0.00}", Position.X);
            string y = string.Format("{0:0.00}", Position.Y);
            string z = string.Format("{0:0.00}", Position.Z);

            string tempString = string.Format("    <X>{0}</X>\r\n    <Y>{1}</Y>\r\n    <Z>{2}</Z>", x,y,z);
            Clipboard.SetText(tempString);

            this.txtX2Desired.Text = "x.xxx";
            this.txtY2Desired.Text = "y.yyy";
            this.txtZ2Desired.Text = "z.zzz";

        }

        private void getAndUpdatePositionT()
        {
            int position = HarmonicDrive.GetPosition();
            txtTPosition.Text = position.ToString();
            this.txtTDesired.Text = "t.ttt";
        }


		private void btnMinusX_Click(object sender, System.EventArgs e)
		{
			bool	Invalid;
			int		SpeedPct;


			MachineCoordinate	DesiredPosition = new MachineCoordinate();

			motionControl.GetCurrentPosition(out DesiredPosition);
			DesiredPosition.X = DesiredPosition.X - GetJogValue(out Invalid);
			SpeedPct = this.trkSpeedX.Value;

			if (Invalid) return;

			MachineMove(DesiredPosition, SpeedPct, false);     
			getAndUpdatePosition();
		
		}

		private void btnPlusX_Click(object sender, System.EventArgs e)
		{
			bool	Invalid;
			int		SpeedPct;


			MachineCoordinate	DesiredPosition = new MachineCoordinate();

			motionControl.GetCurrentPosition(out DesiredPosition);
			DesiredPosition.X = DesiredPosition.X + GetJogValue(out Invalid);
			SpeedPct = this.trkSpeedX.Value;

			if (Invalid) return;

			MachineMove(DesiredPosition, SpeedPct, false);   
			getAndUpdatePosition();

		}

        private void btnMinusX2_Click(object sender, System.EventArgs e)
        {
            bool Invalid;
            int SpeedPct;

            bool useGridRobot = true;

            MachineCoordinate DesiredPosition = new MachineCoordinate();

            motionControl.GetCurrentPosition(out DesiredPosition,useGridRobot);
            DesiredPosition.X = DesiredPosition.X - GetJogValue(out Invalid);
            SpeedPct = this.trkSpeedX2.Value;

            if (Invalid) return;

            MachineMove(DesiredPosition, SpeedPct, false,useGridRobot);
            getAndUpdatePositionGrid();

        }

        private void btnPlusX2_Click(object sender, System.EventArgs e)
        {
            bool Invalid;
            int SpeedPct;

            bool useGridRobot = true;


            MachineCoordinate DesiredPosition = new MachineCoordinate();

            motionControl.GetCurrentPosition(out DesiredPosition, useGridRobot);
            DesiredPosition.X = DesiredPosition.X + GetJogValue(out Invalid);
            SpeedPct = this.trkSpeedX2.Value;

            if (Invalid) return;

            MachineMove(DesiredPosition, SpeedPct, false, useGridRobot);
            getAndUpdatePositionGrid();

        }

		private void btnMinusY_Click(object sender, System.EventArgs e)
		{
			bool	Invalid;
			int		SpeedPct;


			MachineCoordinate	DesiredPosition = new MachineCoordinate();

			motionControl.GetCurrentPosition(out DesiredPosition);
			DesiredPosition.Y = DesiredPosition.Y - GetJogValue(out Invalid);
			SpeedPct = this.trkSpeedY.Value;

			if (Invalid) return;

			MachineMove(DesiredPosition, SpeedPct, false);     

			getAndUpdatePosition();
		}

		private void btnPlusY_Click(object sender, System.EventArgs e)
		{
			bool	Invalid;
			int		SpeedPct;


			MachineCoordinate	DesiredPosition = new MachineCoordinate();

			motionControl.GetCurrentPosition(out DesiredPosition);
			DesiredPosition.Y = DesiredPosition.Y + GetJogValue(out Invalid);
			SpeedPct = this.trkSpeedY.Value;

			if (Invalid) return;

			MachineMove(DesiredPosition, SpeedPct, false);    
			getAndUpdatePosition();
	
		}

        private void btnMinusY2_Click(object sender, System.EventArgs e)
        {
            bool Invalid;
            int SpeedPct;

            bool useGridRobot = true;

            MachineCoordinate DesiredPosition = new MachineCoordinate();

            motionControl.GetCurrentPosition(out DesiredPosition, useGridRobot);
            DesiredPosition.Y = DesiredPosition.Y - GetJogValue(out Invalid);
            SpeedPct = this.trkSpeedY2.Value;

            if (Invalid) return;

            MachineMove(DesiredPosition, SpeedPct, false, useGridRobot);
            getAndUpdatePositionGrid();

        }

        private void btnPlusY2_Click(object sender, System.EventArgs e)
        {
            bool Invalid;
            int SpeedPct;

            bool useGridRobot = true;


            MachineCoordinate DesiredPosition = new MachineCoordinate();

            motionControl.GetCurrentPosition(out DesiredPosition, useGridRobot);
            DesiredPosition.Y = DesiredPosition.Y + GetJogValue(out Invalid);
            SpeedPct = this.trkSpeedY2.Value;

            if (Invalid) return;

            MachineMove(DesiredPosition, SpeedPct, false, useGridRobot);
            getAndUpdatePositionGrid();

        }

		private void btnMinusZ_Click(object sender, System.EventArgs e)
		{
			bool	Invalid;
			int		SpeedPct;


			MachineCoordinate	DesiredPosition = new MachineCoordinate();

			motionControl.GetCurrentPosition(out DesiredPosition);
			DesiredPosition.Z = DesiredPosition.Z - GetJogValue(out Invalid);
			SpeedPct = this.trkSpeedZ.Value;

			if (Invalid) return;

			MachineMove(DesiredPosition, SpeedPct, false);   
			getAndUpdatePosition();
		
		}

		private void btnPlusZ_Click(object sender, System.EventArgs e)
		{
			bool	Invalid;
			int		SpeedPct;


			MachineCoordinate	DesiredPosition = new MachineCoordinate();

			motionControl.GetCurrentPosition(out DesiredPosition);
			DesiredPosition.Z = DesiredPosition.Z + GetJogValue(out Invalid);
			SpeedPct = this.trkSpeedZ.Value;

			if (Invalid) return;

			MachineMove(DesiredPosition, SpeedPct, false);  
			getAndUpdatePosition();
		
		}

        private void btnMinusZ2_Click(object sender, System.EventArgs e)
        {
            bool Invalid;
            int SpeedPct;

            bool useGridRobot = true;

            MachineCoordinate DesiredPosition = new MachineCoordinate();

            motionControl.GetCurrentPosition(out DesiredPosition, useGridRobot);
            DesiredPosition.Z = DesiredPosition.Z - GetJogValue(out Invalid);
            SpeedPct = this.trkSpeedZ2.Value;

            if (Invalid) return;

            MachineMove(DesiredPosition, SpeedPct, false, useGridRobot);
            getAndUpdatePositionGrid();

        }

        private void btnPlusZ2_Click(object sender, System.EventArgs e)
        {
            bool Invalid;
            int SpeedPct;

            bool useGridRobot = true;


            MachineCoordinate DesiredPosition = new MachineCoordinate();

            motionControl.GetCurrentPosition(out DesiredPosition, useGridRobot);
            DesiredPosition.Z = DesiredPosition.Z + GetJogValue(out Invalid);
            SpeedPct = this.trkSpeedZ2.Value;

            if (Invalid) return;

            MachineMove(DesiredPosition, SpeedPct, false, useGridRobot);
            getAndUpdatePositionGrid();

        }

        private void btnMinusT_Click(object sender, EventArgs e)
        {
            bool Invalid;
            int position = HarmonicDrive.GetPosition();
            position -= GetJogValueT(out Invalid);
            if (!Invalid)
            {
                if (HarmonicDrive.Move(position, true) != 0) return;
                getAndUpdatePositionT();
            }
        }

        private void btnPlusT_Click(object sender, EventArgs e)
        {
            bool Invalid;
            int position = HarmonicDrive.GetPosition();
            position += GetJogValueT(out Invalid);
            if (!Invalid)
            {
                if (HarmonicDrive.Move(position, true) != 0) return;
                getAndUpdatePositionT();
            }
        }

        private void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            this.updateAxisStatus();
        }

        private void btnSendCommand_Click(object sender, EventArgs e)
        {

            int axisInt = Convert.ToInt32(cbAxis.Text);         // 1 based axis
            int portIndex = 0;
            if (axisInt > 3) portIndex = 1;

            tbReply.Text = "";  // PK4.0,2015-04-16 
  
            // Clear out and display anything that happened to be in the buffer.
            while (ServoControl.ResponseWaiting(portIndex))
            {
                tbReply.Text = ServoControl.GetResponse(portIndex,50, true);
                Console.WriteLine("[FM <- Servo] {0}", tbReply.Text);
            }
            tbReply.Text = "";  // PK4.0,2015-04-16   Clear out the box
            Console.WriteLine("[TO -> Servo] {0}", tbCommand.Text);

            if (!cbTestTemp.Checked)
            {
                int ret = ServoControl.SendCommand(cbAxis.Text, tbCommand.Text);
                System.Threading.Thread.Sleep(50);   // Give it some time
                while (ServoControl.ResponseWaiting(portIndex))
                {
                    tbReply.Text = ServoControl.GetResponse(portIndex, 50, true);
                    Console.WriteLine("[FM <- Servo] {0}", tbReply.Text);
                }

            }
            else
            {
                // You only want to use this when you are sending query commands or else you are screwed !
                // PKv5.5.5.2
                string retString = ServoControl.SendCommand(cbAxis.Text, tbCommand.Text, true, 100);
                tbReply.Text = retString;
                Console.WriteLine("[FM <- Servo] {0}", tbReply.Text);
            }
        }

        private void btnHomeT_Click(object sender, EventArgs e)
        {
            if (HarmonicDrive.Home(chkTCCWHome.Checked) == 0)
                chkTHomed.Checked = true;
            getAndUpdatePositionT();
        }

        private void btnMoveT_Click(object sender, EventArgs e)
        {
            bool Invalid;
 //            int SpeedPct;
 //            SpeedPct = this.trkSpeedT.Value;

            int position_count = GetTCoordinate(out Invalid);
            if (Invalid) return;

            HarmonicDrive.Move(position_count,true);

            getAndUpdatePositionT();
        }

        private void btnHDSetParam_Click(object sender, EventArgs e)
        {

            int maxVelocityCnt=0, accCnt=0, decelCnt=0;
            bool Invalid = false;

            try
            {
                maxVelocityCnt = System.Convert.ToInt32(this.txtHDVelocity.Text);
                accCnt = System.Convert.ToInt32(this.txtHDAccel.Text);
                decelCnt = System.Convert.ToInt32(this.txtHDDecel.Text);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Invalid T coordinate", "ERROR");
                Invalid = true;
            }

            if (!Invalid)
            {
                HarmonicDrive.SetParameters(maxVelocityCnt, accCnt, decelCnt);
            }
        }

        private void btnInitHDComm_Click(object sender, EventArgs e)
        {
            HarmonicDrive.InitRS232(7);
        }

        private void btnGetPositionT_Click(object sender, EventArgs e)
        {
            getAndUpdatePositionT();
        }

        private void btnEnableT_Click(object sender, EventArgs e)
        {

        }

        private void MotionControlForm_Load(object sender, EventArgs e)
        {
 

        }

        private void buttonUpdateSideCameraTip1_Click(object sender, EventArgs e)
        {
            MachineCoordinate currentPosition = new MachineCoordinate();
            motionControl.GetCurrentPosition(out currentPosition);
            machineParameters.SideCamInspectPointTips[0] = currentPosition;
        }

        private void buttonUpdateSideCameraTip2_Click(object sender, EventArgs e)
        {
            MachineCoordinate currentPosition = new MachineCoordinate();
            motionControl.GetCurrentPosition(out currentPosition);
            machineParameters.SideCamInspectPointTips[1] = currentPosition;
        }

        private void buttonUpdateSideCameraTip3_Click(object sender, EventArgs e)
        {
            MachineCoordinate currentPosition = new MachineCoordinate();
            motionControl.GetCurrentPosition(out currentPosition);
            machineParameters.SideCamInspectPointTips[2] = currentPosition;
        }

        private void buttonUpdateBackCameraTip1_Click(object sender, EventArgs e)
        {
            MachineCoordinate currentPosition = new MachineCoordinate();
            motionControl.GetCurrentPosition(out currentPosition);
            machineParameters.BackCameraPointTips[0] = currentPosition;
        }

        private void buttonUpdateBackCameraTip2_Click(object sender, EventArgs e)
        {
            MachineCoordinate currentPosition = new MachineCoordinate();
            motionControl.GetCurrentPosition(out currentPosition);
            machineParameters.BackCameraPointTips[1] = currentPosition;
        }

        private void buttonUpdateBackCameraTip3_Click(object sender, EventArgs e)
        {
            MachineCoordinate currentPosition = new MachineCoordinate();
            motionControl.GetCurrentPosition(out currentPosition);
            machineParameters.BackCameraPointTips[2] = currentPosition;
        }

        private void buttonUpdateBackCameraGrid_Click(object sender, EventArgs e)
        {
            MachineCoordinate currentPosition = new MachineCoordinate();
            motionControl.GetCurrentPosition(out currentPosition,true);
            if (machineParameters.UseBentTweezerPoints)
                machineParameters.GridToCameraPointBent = currentPosition;
            else
                machineParameters.GridToCameraPoint = currentPosition;
        }
        
	}
}
