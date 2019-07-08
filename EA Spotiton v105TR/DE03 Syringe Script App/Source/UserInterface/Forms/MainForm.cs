using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using EA.PixyControl;
using EA.PixyControl.ClassLibrary;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using Aurigin;

// Version 3.00
//   1) Major revision for Misumi actuators and Schnieder Electric Motors
// Version 4.0
//   1) Major revision for Spotiton system
//
//
// PKv5.2.7 ,  MoveDouble to speed up on the fly dispense with view,   2016-06-15
//     DoOTFPlunge() was updated as well as methods added to the Motion.cs
//     AboutMenu() was updated too.
//
// PKv5.2.8 ,  ,  7/11/2016 sent to Venkat
//      1) Archive experiment settings button added to main panel.
//      Allows user to save all the critical settings used for an experiment into a timestamped log.
//      File path will use the same path that videos are saved in.
//      Added 7/12/2016,       textBoxArchiveNote.Text on front panel and into file.
//      
//      2) Stain panel running with two tips only 7/19/2016,  send to Venkat for debug.  Received word that it is running / released.
//      - Left click will move and allow you to queue targets as before.
//      - Right click while in stain mode to test fire the tip (it will not move first).
//      - F1, F2, F3 keys work as before to change to different tips.
//      - Experiment Mode checkbox is now used as the stain ENABLE check box, check this when using stain tab and doing stain stuff.
//      - btnSTNRunStainQueue_Click was added to keep the logic seperate from the cryomode.  This will run through the queue.
//      
// PKv5.2.9
//      1) Coding for 3 tip operation in stain mode.
//      Assume tips are always loaded tip1-sample1, tip2-sample2, tip3-stain
//      Mode3.1 will dispense tip1-sample1 then tip3-stain on sample1 followed by  tip2-sample2 then tip3-stain on stample2
//      Mode3.2 will dispense tip1-sample1, tip2-sample2 then tip3-stain on sample1 then tip3-stain on sample2.
//          Incrementing the target queue for each sample dispensed. 
//
// PKv5.3.1
//      Added mixing functionality,  Only uses tip1.
//      Inspection tab updated to allow setup and start of in-tip mixing.
//      Methods updated to turn off mixer before firing the tips and then turn the mixer back on after firing tips.
//          btnFire_Click - On Inspection tab was updated.
//          checkBoxClickToSpot - Cyro mode,  On-the-fly and point-to-point  
//      Updated the Aspirate tab button methods to appropriately turn mixing on and off.
//          eg Prime - gets rid of the sample so it turns off the mixing
//
//       Not coded up yet for staining.

// PKv5.3.2
//      Speed up the transfer from Ethane to Liquid Nitrogen.
//      Motion.cs and MainForm.cs
//
//
// PKv5.3.3, 2016-09-12
//      High speed hop to liquid nitrogen.
//      Motion.cs, MainForm.cs, Machine.cs

// PKv5.3.3.1,  2016-09-23, Bug fix to PKv5.3.3
//
// PKv5.4.0,   2016-09-27 to , Starting work on high speed camera (3130).  Mainly concentrated on faster video acquisition.  
//  Never released but will try to leave the changes in to build on.    Video.cs ad MainForm.cs are the only changes
//  At this time i thought high speed video acquisition was the way to go. Make a high speed movie of it flying by.

// PKv5.4.1,  2017-01-20,   1/20/2017,  Reccommendations sent to Venkat to improve aspiration
//
// PKv5.5.0, 2017-02-08
// 
//  Abandoning the approach to make a movie of the grid flying by.  Will now just trigger using an external sensor
//  Triggered acquistion is rolling out to the droplet inspection camera and the grid camera (via an extra sensor on the z axis).
//
// PKv5.5.0 to PKv5.5.4
//
//  Series of modifications for high speed cameras.  Installed hardware and software as part of an upgrade the week of 2017-02-20
//
// PKv5.5.5, Start 2017-03-01,  
//    Added capabilities for fast switch back and forth from development system in Phoenix:  MachineParaemtersDev.xml
//    Better error reporting and error recovery from motion errors... 
//
// PKv5.5.5.2,  2017-03-03  
//    Motors are getting intermitten communcation timeout.  Motion.cs was updated to provide a much smarter SendCommand() to the motor.
// 
// PKv_wa,  20167-04-11   This was a special release for Ivan who did not have DIO on his system.   Changes were taken back out for the next version.
//
// PKv5.5.5.3,  2017-05-22  
//    
//    Added The option to "on-the-fly" dispense and wait in front of the camera until the user clicks a dialog box.    
//    numericUpDownPauseOTF.Value=999,  triggers this feature.   
//    For now this capability only works in Cosine mode.
//    Added a test123 button on the debug panel and tested that videos still get captured while the pause button is up.  

// PKv5.5.5.4,  2017-06-25  
//    
//    Investigate whether a 3rd camera can be added.  Key functionality to demonstrate first is how fast back to back triggered
//     acquisitions can occur on two different cameras.    Turned out this was not nearly fast as it needs to be.
//     Added button to the bebug panel that "TestDTA" Test Double Trigger Acquisition.

// PKv5.6  - This will be the release with the dual camera inspection.   The 2nd camera inspection will be done with a seperate computer that
//     will transfer the image onto a directory of the this computer ... which will then save all the images and settings.
//     This elimnates the need for two very fast back to back triggers of version PKv5.5.5.4
//     Not started righting code yet....  Will start with the 2nd computer code first.

// 2019-01-15, 2019-01-16, 2019-01-16 Revisions for Time Resolved upgrade
//  Changed the main folder containing this applicaiton to "EA Spotiton v105TR"
//



namespace EA.PixyControl
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class frmMain : System.Windows.Forms.Form
    {
        internal ThisRobot Pixy;   // Used to be internal
        internal MachineDevelopmentSystemFlag mdsf;             // PKv5.5.5
        private MotionControlForm MotionWindow;
        private DoActionForm DoActionWindow;
        private bool bAbort;
        private bool bPause;
        internal string CurrentDatalogFile;   //pk-13june

        //Ivans edits for camera
        private Video Camera;
        private Point clickedCoordinates;     // last coordinates of click on the video
        private List<Point> QueuedCoordinates = new List<Point>();      //Array of clickedCoordinates, used for queuing
        private int numQueuedCoordinates; //Current index of QueuedCoordinates

        // PKv5.5.2
        private double loadedSampleVolume = 0.0;

        private VariableManager mVM = new VariableManager();
        private ArrayList mCommandList = null;
        private SequenceFile mSeqFile = null;
        private ArrayList mTreeNodes = null;
        private bool mStepMode = false;
        private bool mSequenceChanged = false;
        private GUI_STATE mState = GUI_STATE.IdleNoSequence;
        private bool mUpdateVariables = false;
        private Thread mExecutionThread = null;

        private System.Windows.Forms.MainMenu mnuMain;
        private System.Windows.Forms.MenuItem mnuFile;
        private System.Windows.Forms.MenuItem mnuEdit;
        private System.Windows.Forms.MenuItem mnuTools;
        private System.Windows.Forms.MenuItem mnuCommands;
        private System.Windows.Forms.MenuItem mnuRun;
        private System.Windows.Forms.ImageList imlProcesses;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.MenuItem mnuExit;
        private System.Windows.Forms.MenuItem mnuMotion;
        private System.Windows.Forms.MenuItem mnuController;
        private System.Windows.Forms.MenuItem mnuRunSequence;
        private System.Windows.Forms.MenuItem mnuPauseSequence;
        private System.Windows.Forms.MenuItem mnuStopSequence;
        private System.Windows.Forms.MenuItem mnuSyringe;
        private System.Windows.Forms.MenuItem mnuOpenWorklist;
        private System.Windows.Forms.StatusBar StatBar;
        private System.Windows.Forms.StatusBarPanel pnlLoop;
        private System.Windows.Forms.StatusBarPanel pnlMessage;
        private System.Windows.Forms.MenuItem mnuMachineSettings;
        private System.Windows.Forms.MenuItem mnuSyringeSettings;
        private System.Windows.Forms.MenuItem mnuMotionSettings;
        private System.Windows.Forms.MenuItem mnuOpenLogFile;
        private System.Windows.Forms.MenuItem menuItem4;
        private System.Windows.Forms.MenuItem mnuNewLogFile;
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem mnuBackRight;
        private System.Windows.Forms.MenuItem mnuIspMove;
        private System.Windows.Forms.MenuItem mnuInspMoveTip1;
        private System.Windows.Forms.MenuItem mnuInspMoveTip2;
        private System.Windows.Forms.MenuItem mnuSave;
        private System.Windows.Forms.MenuItem mnuSaveAs;
        private System.Windows.Forms.MenuItem mnuStep;
        private System.Windows.Forms.MenuItem mnuCheckSequence;
        private System.Windows.Forms.MenuItem mnuReload;
        private System.Windows.Forms.MenuItem mnuDryRun;
        private System.Windows.Forms.Timer TimerVariables;
        private System.Windows.Forms.MenuItem mnuStopNow;
        private System.Windows.Forms.ImageList imlButtons;
        private System.Windows.Forms.MenuItem mnuTestCommands;
        private System.Windows.Forms.MenuItem mnuHelp;
        private System.Windows.Forms.MenuItem mnuCmdRef;
        private System.Windows.Forms.MenuItem mnuMoveZUp;
        private System.Windows.Forms.MenuItem mnuHelpAbout;
        private System.Windows.Forms.MenuItem mnuStrobeOn;
        private System.Windows.Forms.MenuItem mnuStrobeOff;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem mnuReloadMachineSettings;
        private MenuItem menuItem3;
        private MenuItem mnuIOControlWindow;
        private MenuItem mnuSetOutputs;
        private MenuItem mnuReadInputs;
        private MenuItem mnuInitInputsAndOutputs;
        private MenuItem mnuOpenAndRunWorklist;
        private MenuItem mnuMovePipetteSafe;
        private MenuItem mnuMoveGridRobotSafe;
        private PictureBox DisplayWindow;
        private RadioButton radioButtonTip1;
        private RadioButton radioButtonTip2;
        private RadioButton radioButtonTip3;
        private TabControl tabControlRobot;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabSpotSample;
        private TabPage tabPage4;
        private TrackBar trackBarPixelClock;
        private TrackBar trackBarFPS;
        private TrackBar trackBarExposure;
        private Label labelMinFPS;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label labelCurrentFPS;
        private Label labelCurrentExposure;
        private Label labelPixelClockMax;
        private Label labelCurrentPixelClock;
        private Label labelMaxFPS;
        private Label labelPixelClockMin;
        private Label labelMinExposure;
        private Label labelMaxExposure;
        private Button btnSwitchCamera;
        private Label label6;
        private Button button3;
        private Button button2;
        private Button button1;
        private TextBox textBoxVideoFolder;
        private Label label1;
        private Label label5;
        private Label labelTip1VideoCentered;
        private Label labelTip2VideoCentered;
        private Label labelTip3VideoCentered;
        private Label labelRecordingVideo;
        private FolderBrowserDialog folderBrowserDialog1;
        private Label label10;
        private Button buttonTipToCam;
        private Button buttonCenterTip;
        private NumericUpDown numericUpDownSpotDropNumber;
        private CheckBox checkBoxClickToSpot;
        private Button buttonAddQueueTargets;
        private CheckBox checkBoxAutomaticPlunge;
        private Button button8;
        private Button button7;
        private Button button6;
        private Label label11;
        private Label label12;
        private Label label13;
        private NumericUpDown numericUpDownWash;
        private NumericUpDown numericUpDownAspirateVolume;
        private NumericUpDown numericUpDownPrimeNumber;
        private Button button9;
        private NumericUpDown numericUpDownTipAmp;
        private Button button10;
        private Button btnGotoSafePosition;
        private Label label7;
        private Button buttonWipeTips;
        private NumericUpDown numericUpDownNumberWipeTips;
        private Button buttonPositionGridToCamera;
        private Label label8;
        private Button buttonPositionGridToSafePoint;
        private Button buttonWriteSettingsToXMLFile;
        private CheckBox checkBoxCryoMode;
        private GroupBox groupBox3;
        private NumericUpDown numericUpDownCosFreq;
        private GroupBox groupBox2;
        private NumericUpDown numericUpDownTrapTrailing;
        private NumericUpDown numericUpDownTrapDwell;
        private NumericUpDown numericUpDownTrapLeading;
        private CheckBox checkBoxInspCosine;
        private NumericUpDown numericUpDownCosAmp;
        private TabPage tabStain;
        private Label lblTipMovement;
        private CheckBox cbOTF_Enable;
        private GroupBox groupBox4;
        private NumericUpDown numericUpDownPauseOTF;
        private Label label15;
        private Button buttonArchiveExperimentSettings;
        private TextBox textBoxArchiveFile;
        private Label label16;
        private Button buttonSTNPositionGridToSafePoint;
        private Button buttonSTNPositionGridToCamera;
        private Button buttonSTNTipToCam;
        private Button buttonSTNCenterTip;
        private NumericUpDown numericUpDownSTNTip3Drops;
        private NumericUpDown numericUpDownSTNTip2Drops;
        private Label label19;
        private NumericUpDown numericUpDownSTNTip1Drops;
        private Button buttonSTNAddQueueTargets;
        private NumericUpDown numericUpDownSTNDelay_ms;
        private Label label20;
        private Label label21;
        private TextBox textBoxArchiveNote;
        private CheckBox checkBoxExperimentMode;
        private Button btnSTNClearQueue;
        private Button btnSTNRunStainQueue;
        private GroupBox groupBox5;
        private Label label22;
        private NumericUpDown numericUpDownSTNDelay2_ms;
        private GroupBox groupBox6;
        private GroupBox groupBox7;
        private ComboBox comboBoxSTNMode;
        private GroupBox groupBox8;
        private NumericUpDown numericUpDownMixDutyCycle;
        private Label label23;
        private Label label18;
        private NumericUpDown numericUpDownMixAmplitude;
        private Label label17;
        private NumericUpDown numericUpDownMixFreq;
        private CheckBox checkBoxMixOn;
        private TabPage tabPage3;
        private GroupBox groupBox10;
        private Label label27;
        private NumericUpDown numericUpDownDebugVideoWidth;
        private Label label26;
        private NumericUpDown numericUpDownDebugJPG;
        private Label label29;
        private NumericUpDown numericUpDownDebugStrobeDelay;
        private Label label28;
        private NumericUpDown numericUpDownDebugStrobeDuration;
        private CheckBox checkBoxDebugEnableStrobe;
        private Button btnUltrasonicWash;
        private Label label32;
        private NumericUpDown numericUpDownUSWashNumberOfCycles;
        private Label label31;
        private Label label30;
        private NumericUpDown numericUpDownUSWashSyringeSpeedCode;
        private CheckBox checkBoxInspEndOfStream;
        private Label lblActiveCam;
        private GroupBox groupBox11;
        private Button btnCamSetGain;
        private Button btnCamReadGain;
        private NumericUpDown numericUpDownCamGain;
        private Button btnCamLoadSettingsFromFile;
        private Button btnCamSaveSettingsToFile;
        private Button btnLoadedSampleVolReset;
        private Label lblLoadedSampleVolume;
        private Button buttonStain;
        private Label label9;
        private NumericUpDown numericUpDownStainTime;
        private Label label34;
        private NumericUpDown numericUpDownOTFISlowSpeed;
        private Label label33;
        private NumericUpDown numericUpDownOTFIPercentWayToTarget;
        private CheckBox cbOTFIEnableSlowDown;
        private CheckBox cbOTFIUseFlashAcquisition;
        private Label label36;
        private Label label35;
        private NumericUpDown numericUpDownOTFIDelayAfterTrip;
        private CheckBox cbDebugAutoStartVideo;
        private CheckBox cbDebugDisableGridDrop;
        private Button btnSaveBitmap;
        private CheckBox cbDebugTrySavingBitmap;
        private SaveFileDialog saveFileBMPDialog;
        private CheckBox cbPrimeWithoutMove;
        private Button btnTest123;
        private Button btnTestDoubleTriggeredAcquision;
        private Button btnTestStartLive;
        private Button btnTestStopLive;
        private GroupBox groupBox9;
        private Button btnDGCTest;
        private Label lblDGCRemotePath;
        private CheckBox cbDGCPromptAndSave;
        private CheckBox cbDGCEnabled;
        private SaveFileDialog saveImagesAndSettingsFileDialog;
        private SaveFileDialog saveFileDialogArchiveOnly;
        private GroupBox groupBox12;
        private Button btn3StackLoadDefaults;
        private Label label24;
        private NumericUpDown numericUpDown3StackDecel;
        private NumericUpDown numericUpDown3StackAccel;
        private NumericUpDown numericUpDown3StackVel;
        private Button btn3StackTestPlunge;
        private CheckBox cbSelectTip1andTip2;
        private Button btnBypassSyringeValves;
        private Label label14;
        private RadioButton rbTimeResTip1and2;
        private RadioButton rbTimeResTip2;
        private RadioButton rbTimeResTip1;
        private Button btnCloseSyringeValves;
        private System.Windows.Forms.MenuItem mnuMachineSettingsEdit;

        private enum GUI_STATE
        {
            IdleNoSequence,
            IdleSequenceLoaded,
            Running,
            Paused
        }

        // PKv5.2.9  Different types of stain operation
        //     which can be selected by the user with the combobox.

        private enum STN_MODE
        { Tips2_1,
            Tips3_1,
            Tips3_2,
            Tips3_future
        }


        private class ToolbarButtons
        {
            public const int Open = 0;
            public const int Save = 1;
            public const int Check = 2;
            public const int Sep0 = 3;
            public const int Sep1 = 4;
            public const int Run = 5;
            public const int Pause = 6;
            public const int Stop = 7;
            public const int Sep3 = 8;
            public const int Sep4 = 9;
            public const int StopNow = 10;
            public const int StepMode = 11;
        }

        public static class Prompt
        {
            public static int ShowDialog(string text, string caption)
            {
                int cryoPositionIndex = 0;
                Form prompt = new Form();
                prompt.Width = 200;
                prompt.Height = 200;
                prompt.Text = caption;
                Label textLabel = new Label() { Left = 10, Top = 20, Text = text, Width = 150 };
                Button confirmation = new Button() { Text = "Ok", Left = 50, Width = 100, Top = 100 };
                RadioButton position1 = new RadioButton() { Text = "Cryo 1", Left = 20, Top = 50 };
                RadioButton position2 = new RadioButton() { Text = "Cryo 2", Left = 20, Top = 70 }; ;
                position1.Checked = true;
                position2.Enabled = false;      // PKv5.3.3         Lets only use 1 position for now.
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(position1);
                prompt.Controls.Add(position2);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.ShowDialog();
                if (position1.Checked)
                {
                    cryoPositionIndex = 0;
                }
                else if (position2.Checked)
                {
                    cryoPositionIndex = 1;
                }

                return cryoPositionIndex;
            }
        }
        #region event handlers

        // begin of Ivan's event handlers

        private void numericUpDownSpotDropNumber_Changed(object sender, EventArgs e)
        {
            NumericUpDown numericUpDownBox = sender as NumericUpDown;
            Pixy.MachineParameters.NumberDropsToSpot[Pixy.ActiveTip] = (int)numericUpDownBox.Value;
        }

        private void numericUpDownTipAmp_Changed(object sender, EventArgs e)
        {
            NumericUpDown numericUpDownBox = sender as NumericUpDown;
            Pixy.MachineParameters.TrapAmp[Pixy.ActiveTip] = (int)numericUpDownBox.Value;
        }

        // PKv5.2.4 Added more event handlers for allowing drop inspection camera in trap and cosine mode

        private void numericUpDownTrapLeading_Changed(object sender, EventArgs e)
        {
            NumericUpDown numericUpDownBox = sender as NumericUpDown;
            Pixy.MachineParameters.DE03TrapSetupLeading[Pixy.ActiveTip] = (int)numericUpDownBox.Value;
        }

        private void numericUpDownTrapDwell_Changed(object sender, EventArgs e)
        {
            NumericUpDown numericUpDownBox = sender as NumericUpDown;
            Pixy.MachineParameters.DE03TrapSetupDwell[Pixy.ActiveTip] = (int)numericUpDownBox.Value;
        }

        private void numericUpDownTrapTrailing_Changed(object sender, EventArgs e)
        {
            NumericUpDown numericUpDownBox = sender as NumericUpDown;
            Pixy.MachineParameters.DE03TrapSetupTrailing[Pixy.ActiveTip] = (int)numericUpDownBox.Value;
        }

        private void numericUpDownCosFreq_Changed(object sender, EventArgs e)
        {
            NumericUpDown numericUpDownBox = sender as NumericUpDown;
            Pixy.MachineParameters.DE03CosSetupFreq[Pixy.ActiveTip] = (int)numericUpDownBox.Value;

        }

        private void numericUpDownCosAmp_Changed(object sender, EventArgs e)
        {
            NumericUpDown numericUpDownBox = sender as NumericUpDown;
            Pixy.MachineParameters.DE03CosSetupAmp[Pixy.ActiveTip] = (int)numericUpDownBox.Value;

        }

        private void numericUpDownAspirateVolume_Changed(object sender, EventArgs e)
        {
            NumericUpDown numericUpDownBox = sender as NumericUpDown;
            Pixy.MachineParameters.AspirateVolume[Pixy.ActiveTip] = (double)numericUpDownBox.Value;     //2019-01-16

        }


        // This event handlers is called when any of the 3 radio buttons change their states
        // the purpose of the radio button is to select the active tip (1, 2 or 3)

        private void radioButtons_CheckedChanged(object sender, System.EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            if (radioButtonTip1.Checked)
            {
                Pixy.setActiveTip(0);
            }
            else if (radioButtonTip2.Checked)
            {
                Pixy.setActiveTip(1);
            }
            else if (radioButtonTip3.Checked)
            {
                Pixy.setActiveTip(2);
            }

            if ((!this.checkBoxCryoMode.Checked) && this.checkBoxExperimentMode.Checked && Camera.getCurrentCameraName() == "Back")
            {
                DoGoToClickPosition(clickedCoordinates);      // This should still work as before PKv5.2.8 will move the tip in front.
            }
            if (Camera.getCurrentCameraName() == "Side")
            {
                //   DoMoveToPointGUI(Pixy.MachineParameters.SideCamInspectPointTips[Pixy.ActiveTip], 0, 0, 0, Pixy.MachineParameters.Tips[Pixy.ActiveTip], 25, false, true);
            }
            this.numericUpDownTipAmp.Value = Pixy.MachineParameters.TrapAmp[Pixy.ActiveTip];
            this.numericUpDownTrapLeading.Value = Pixy.MachineParameters.DE03TrapSetupLeading[Pixy.ActiveTip];  // PKv5.2.4
            this.numericUpDownTrapDwell.Value = Pixy.MachineParameters.DE03TrapSetupDwell[Pixy.ActiveTip];  // PKv5.2.4
            this.numericUpDownTrapTrailing.Value = Pixy.MachineParameters.DE03TrapSetupTrailing[Pixy.ActiveTip];  // PKv5.2.4

            this.numericUpDownCosFreq.Value = Pixy.MachineParameters.DE03CosSetupFreq[Pixy.ActiveTip];  // PKv5.2.4
            this.numericUpDownCosAmp.Value = Pixy.MachineParameters.DE03CosSetupAmp[Pixy.ActiveTip];  // PKv5.2.4


            this.numericUpDownSpotDropNumber.Value = Pixy.MachineParameters.NumberDropsToSpot[Pixy.ActiveTip];
            this.numericUpDownAspirateVolume.Value = (decimal) Pixy.MachineParameters.AspirateVolume[Pixy.ActiveTip];

        }

        // We can also change the active tip by using the F1, F2 or F3 keys to select 
        // the 1, 2 or 3 tips respectively
        private void frmMain_KeyPressHandler(object sender, KeyEventArgs e)
        {
            // depending on the F key pressed it just calls the respective radio button
            // to be clicked
            if (e.KeyCode == Keys.F1)
            {
                radioButtonTip1.PerformClick();
            }
            else if (e.KeyCode == Keys.F2)
            {
                radioButtonTip2.PerformClick();
            }
            else if (e.KeyCode == Keys.F3)
            {
                radioButtonTip3.PerformClick();
            }
        }

        // this event handler tracks the Pixel Clcok of the current camera
        // it also updates the camera settings from the value of the track bar
        private void trackBarPixelClock_Changed(object sender, System.EventArgs e)
        {
            // null event means that we didnt touch the track bar, so we just need to
            // read the camera's settings and update them on the GUI
            if (e == null)
            {
                Int32 value, min, max, freq = 0;
                Camera.getTrackBallValuePixelClock(out value, out min, out max, out freq);
                this.trackBarPixelClock.Minimum = min;
                this.trackBarPixelClock.Maximum = max;
                this.trackBarPixelClock.Value = value;      //PKv5.4.0  rearranged to work with two different cameras on the system

                this.trackBarPixelClock.TickFrequency = freq;
                this.labelCurrentPixelClock.Text = value.ToString();
                this.labelPixelClockMin.Text = min.ToString();
                this.labelPixelClockMax.Text = max.ToString();

                // call the exposure and FPS handlers to update the values and labels on the
                // respective track bars
                trackBarExposure_Changed(sender, null);
                trackBarFPS_Changed(sender, null);
            }

            // non null event means that the trackbar value was changed, so we update the 
            // camera settings with the new value.
            // Since the Exposure and FPS are dependent on the Pixel Clock of the camera
            // we call their respective event handlers with a "null" event to just update 
            // the track bar values
            else if (e != null)
            {
                trackBarPixelClock.Value = (8 * (int)((trackBarPixelClock.Value + 4) / 8));   //  PKv5.5.0.1 // Pixel clock values seem to prefer a multiple of 8
                Camera.setPixelClock(trackBarPixelClock.Value);
                labelCurrentPixelClock.Text = trackBarPixelClock.Value.ToString();
                trackBarExposure_Changed(sender, null);
                trackBarFPS_Changed(sender, null);
            }
        }

        // this event handler tracks and updates the FPS settings of the camera
        private void trackBarFPS_Changed(object sender, System.EventArgs e)
        {
            // null event means we just need to read the settings from the 
            // camera and update them on the GUI labels
            if (e == null)
            {
                double currentFPS, minFPS, maxFPS, incFPS = 0;
                int trackvalue, range_trackvalue = 0;
                Camera.getTrackBallValueFPS(out currentFPS, out minFPS, out maxFPS, out incFPS, out trackvalue, out range_trackvalue);
                this.trackBarFPS.Minimum = 0;
                this.trackBarFPS.Maximum = range_trackvalue;
                this.trackBarFPS.Value = trackvalue;
                //this.trackBarFPS.TickFrequency = incFPS;
                this.labelCurrentFPS.Text = currentFPS.ToString("F2");
                this.labelMinFPS.Text = minFPS.ToString("F2");
                this.labelMaxFPS.Text = maxFPS.ToString("F2");

                // call the Exposure handler, since the exposure is 
                // dependent on the FPS we just need to read the setting for
                // Exposure from the camera and update them
                trackBarExposure_Changed(sender, null);

            }

            // non null event means the track bar changed value.
            // send the new value to the camera and call the Pixel clock and FPS
            // event handlers to just read their respective values from the camera
            // and update them on the gui
            else if (e != null)
            {
                Camera.setFPS(trackBarFPS.Value);
                labelCurrentFPS.Text = Camera.getFPS().ToString("F2");
                trackBarExposure_Changed(sender, null);
                trackBarPixelClock_Changed(sender, null);
            }
        }

        // this event handler deals with the Exposure track bar of the camera
        private void trackBarExposure_Changed(object sender, System.EventArgs e)
        {
            // null event means we just need to read the camera settings and update them
            // on the gui. no need to call the other event handlers since Exposure setting
            // does not have any dependent variables
            if (e == null)
            {
                double currentExp, minExp, maxExp, incExp = 0;
                int trackvalue, range_trackvalue = 0;
                Camera.getTrackBallValueExposure(out currentExp, out minExp, out maxExp, out incExp, out trackvalue, out range_trackvalue);
                this.trackBarExposure.Minimum = 0;
                this.trackBarExposure.Maximum = range_trackvalue;
                this.trackBarExposure.Value = trackvalue;
                this.labelCurrentExposure.Text = currentExp.ToString("F2");
                this.labelMinExposure.Text = minExp.ToString("F2");
                this.labelMaxExposure.Text = maxExp.ToString("F2");
            }
            // trackbar value changed. send the new value to the camera and update 
            // the PixelClock and FPS values from the camera
            else if (e != null)
            {
                Camera.setExposure(trackBarExposure.Value);
                labelCurrentExposure.Text = Camera.getExposure().ToString("F2");
                trackBarFPS_Changed(sender, null);
                trackBarPixelClock_Changed(sender, null);
            }
        }
        // end of Ivan's event handlers


        // look for the flag that says to update the display of variables
        // timer only runs while a sequence is running
        private void TimerVariables_Tick(object sender, System.EventArgs e)
        {
            if (this.mVM != null)
            {
                if (this.mUpdateVariables)
                {
                    this.mUpdateVariables = false;

                    System.Data.DataTable DT = mVM.ListVariables();

                    if (DT != null)
                    {
                        // can return null if it needs to be updated again
                        //	this.dgVariables.DataSource = DT;

                    }
                    else
                    {
                        // try again next tick
                        this.mUpdateVariables = true;
                    }
                }
            }
        }

        // give user a chance to save changes and cancel the exit
        private void frmMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;

            DialogResult Rslt;

            if (this.mSequenceChanged)
            {
                Rslt = MessageBox.Show("Would you like to save your changes?", "Save Changes", System.Windows.Forms.MessageBoxButtons.YesNoCancel);

                if (Rslt == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (Rslt == DialogResult.Yes) SaveSequenceToFile();
            }

            if ((mExecutionThread != null) && (mExecutionThread.IsAlive)) mExecutionThread.Suspend();

            if (MessageBox.Show("Are you sure you want to exit?", "Exit?", System.Windows.Forms.MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
            {
                if (Pixy.MotionControl.HardwareInitialized)
                {
                    if (!Pixy.MachineParameters.PKDevSystemEnable)      //2019-01-18  Only move the grid robot when not on development system
                    {
                        bool UseGridRobot = true;
                        Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                                      Pixy.MachineParameters.SafePointGrid, 0.0, 75, true, true, UseGridRobot, false);

                    }
                    // go to safe position and then close
                    DoMoveToPointOrderedGUI(Pixy.MachineParameters.SafePoint, 0, 0, 0,
                                                Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                                100, true, Pixy.MachineParameters.ThetaRotate_0, 1, 3, 2, false);

                    Pixy.MotionControl.WaitForEndOfMotion(20);
                }
                e.Cancel = false;
            }
            else e.Cancel = true;
            //e.Cancel = (MessageBox.Show("Are you sure you want to exit?", "Exit?", System.Windows.Forms.MessageBoxButtons.YesNoCancel) != DialogResult.Yes);

            if ((mExecutionThread != null) && (mExecutionThread.IsAlive))
            {
                if (e.Cancel) mExecutionThread.Resume();
                else
                {

                    StopNow();
                }
            }
        }


        // event handler for each new command that's executed from the sequence file
        // - highlight the item in the treeview
        // - can pause or abort
        private void mSeqFile_NewCommand(object sender, NewCommandEventArgs ev)
        {
            if (mCommandList == null) return;

            // update the current command display
            //	SelectCmdNode(ev.StepNumber);

            // update status bar text
            this.pnlLoop.Text = "Step " + (ev.StepNumber + 1) + ": " + ((ProcessAction)mCommandList[ev.StepNumber]).Name;

            if ((this.bPause) || (this.mStepMode))
            {
                ev.Cancel = (MessageBox.Show("Click OK to continue", "Pause or Step Mode", System.Windows.Forms.MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK);
                this.bPause = false;
            }
        }


        // event handler for knowing when sequence file execution is finished
        private void mSeqFile_SequenceComplete(object sender, EventArgs ev)
        {
            SetState(PixyControl.frmMain.GUI_STATE.IdleSequenceLoaded);
            //this.lbRunInfo2.Text = "";
            this.pnlLoop.Text = "";
            this.mExecutionThread = null;

            // Try to ZUP where ever we are at the end of the run..   24Aug2012   Take out
            //			Pixy.MotionControl.MoveZToSafeHeight(Pixy, true);

            // Let the user know that the run has completed...
            string message = "Run Completed: " + Datalog.Timestamp();

            if (!bAbort) // KC-060612 
            {
                MessageBox.Show("The run has completed", "Thank You");
            }
            else   // Run was aborted
            {
                message += ",  Run was aborted";
                MessageBox.Show("The run was aborted", "Thank You");
            }

            UserDatalog(message);
        }


        private void mSeqFile_VariableChanged(object sender, EventArgs ev)
        {
            mUpdateVariables = true;
        }


        // event handler for when the user edits a command parameter in the grid
        /*	private void Viewer_ParameterChanged(object sender, ControlLibrary.ParameterChangedEventArgs ev)
            {
                // no changes while running
                if (mState != GUI_STATE.IdleSequenceLoaded)
                {	// change it back
                    UpdateViewer();
                }

    //			TreeNode Curr = this.tvCommands.SelectedNode;

            //	if (Curr == null) return;
            //	if (Curr.Tag == null) return;

                DataTable		DT = ev.NewInfo;
            //	ProcessAction	Cmd = (ProcessAction)Curr.Tag;
                string			PropName;
            //	Type			T = Cmd.GetType();
                PropertyInfo	Prop = null;

                foreach (DataRow DR in DT.Rows)
                {
                    PropName = (string)(DR["Parameter"]);
                    Prop = T.GetProperty(PropName);

                    if (Prop != null)
                    {
                        if (Prop.GetCustomAttributes(typeof(ProcessActionArgumentAttribute) , false) != null)
                        {
                            Prop.SetValue(Cmd, System.Convert.ChangeType(DR["Value"], Prop.PropertyType), null);
                        }
                    }
                }

                mSequenceChanged = true;
                SetTitleText();

                UpdateViewer();
            }*/


        // if the user clicks on a command in the tree view, update the Viewer display of command parameters
        private void tvCommands_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            //	UpdateViewer();
        }

        #endregion event handlers

        #region display stuff

        private void SetState(GUI_STATE NewState)
        {
            mState = NewState;

            switch (mState)
            {
                case (GUI_STATE.IdleNoSequence):
                    this.mnuFile.Enabled = true;
                    //this.toolBar1.Buttons[ToolbarButtons.Open].Enabled = true;
                    //	this.toolBar1.Buttons[ToolbarButtons.Save].Enabled = false;
                    this.mnuPauseSequence.Enabled = false;
                    //this.toolBar1.Buttons[ToolbarButtons.Pause].Enabled = false;
                    this.mnuStopSequence.Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.Stop].Enabled = false;
                    this.mnuStopNow.Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.StopNow].Enabled = false;
                    this.mnuStep.Enabled = false;
                    this.mnuCheckSequence.Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.Check].Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.Check].Enabled = false;
                    this.mnuDryRun.Enabled = false;
                    this.mnuRunSequence.Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.Run].Enabled = false;
                    this.TimerVariables.Enabled = false;
                    //		this.lbRunInfo1.Text = "Idle";
                    this.mnuCommands.Enabled = true;
                    this.mnuTools.Enabled = true;
                    this.mnuEdit.Enabled = true;
                    //	this.cboAction.Enabled = true;
                    //	this.btnAdd.Enabled = true;
                    break;
                case (GUI_STATE.IdleSequenceLoaded):
                    this.mnuFile.Enabled = true;
                    //	this.toolBar1.Buttons[ToolbarButtons.Open].Enabled = true;
                    //	this.toolBar1.Buttons[ToolbarButtons.Save].Enabled = true;
                    this.mnuPauseSequence.Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.Pause].Enabled = false;
                    this.mnuStopSequence.Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.Stop].Enabled = false;
                    this.mnuStopNow.Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.StopNow].Enabled = false;
                    this.mnuStep.Enabled = true;
                    this.mnuDryRun.Enabled = true;
                    this.mnuCheckSequence.Enabled = true;
                    //	this.toolBar1.Buttons[ToolbarButtons.Check].Enabled = true;
                    //	this.toolBar1.Buttons[ToolbarButtons.Check].Enabled = true;
                    this.mnuRunSequence.Enabled = true;
                    //	this.toolBar1.Buttons[ToolbarButtons.Run].Enabled = true;
                    this.TimerVariables.Enabled = false;
                    //	this.lbRunInfo1.Text = "Idle - Ready";
                    this.mnuCommands.Enabled = true;
                    this.mnuTools.Enabled = true;
                    this.mnuEdit.Enabled = true;
                    //	this.cboAction.Enabled = true;
                    //	this.btnAdd.Enabled = true;
                    break;
                case (GUI_STATE.Running):
                    this.mnuFile.Enabled = false;
                    //		this.toolBar1.Buttons[ToolbarButtons.Open].Enabled = false;
                    //		this.toolBar1.Buttons[ToolbarButtons.Save].Enabled = false;
                    this.mnuPauseSequence.Enabled = true;
                    //		this.toolBar1.Buttons[ToolbarButtons.Pause].Enabled = true;
                    //	this.toolBar1.Buttons[ToolbarButtons.Pause].Enabled = true;
                    this.mnuStopSequence.Enabled = true;
                    //	this.toolBar1.Buttons[ToolbarButtons.Stop].Enabled = true;
                    this.mnuStopNow.Enabled = true;
                    //	this.toolBar1.Buttons[ToolbarButtons.StopNow].Enabled = true;
                    this.mnuStep.Enabled = true;
                    this.mnuCheckSequence.Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.Check].Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.Check].Enabled = false;
                    this.mnuDryRun.Enabled = false;
                    this.mnuRunSequence.Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.Run].Enabled = false;
                    this.TimerVariables.Enabled = true;
                    //	this.lbRunInfo1.Text = "Running";
                    this.mnuCommands.Enabled = false;
                    this.mnuTools.Enabled = false;
                    this.mnuEdit.Enabled = false;
                    //	this.cboAction.Enabled = false;
                    //	this.btnAdd.Enabled = false;
                    break;
                case (GUI_STATE.Paused):
                    this.mnuFile.Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.Open].Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.Save].Enabled = false;
                    this.mnuPauseSequence.Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.Pause].Enabled = false;
                    this.mnuStopSequence.Enabled = true;
                    //	this.toolBar1.Buttons[ToolbarButtons.Stop].Enabled = true;
                    this.mnuStopNow.Enabled = true;
                    //	this.toolBar1.Buttons[ToolbarButtons.StopNow].Enabled = true;
                    this.mnuStep.Enabled = true;
                    this.mnuCheckSequence.Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.Check].Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.Check].Enabled = false;
                    this.mnuDryRun.Enabled = false;
                    this.mnuRunSequence.Enabled = false;
                    //	this.toolBar1.Buttons[ToolbarButtons.Run].Enabled = false;
                    this.TimerVariables.Enabled = false;
                    //	this.lbRunInfo1.Text = "Paused";
                    this.mnuCommands.Enabled = false;
                    this.mnuTools.Enabled = false;
                    this.mnuEdit.Enabled = false;
                    //	this.cboAction.Enabled = false;
                    //	this.btnAdd.Enabled = false;

                    break;
                default:
                    MessageBox.Show("Unknown GUI state");
                    break;
            }
        }

        // updates the form title with the sequence file name
        private void SetTitleText()
        {
            this.Text = "EA Table Top Instrument";

            if ((this.mSeqFile != null) && (this.mSeqFile.FileName != null))
            {
                this.Text = "EA Table Top Instrument - " + this.mSeqFile.FileName + (this.mSequenceChanged ? " *" : "");
            }
        }






        private void ClearStatusBar()
        {
            // clear both panels
            this.pnlLoop.Text = "";
            this.pnlMessage.Text = "";
        }

        private void ShowStatusMessage(string Message)
        {
            pnlMessage.Text = Message;
        }





        #endregion display stuff

        // Clean up any resources being used.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }


        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.mnuMain = new System.Windows.Forms.MainMenu(this.components);
            this.mnuFile = new System.Windows.Forms.MenuItem();
            this.mnuOpenWorklist = new System.Windows.Forms.MenuItem();
            this.mnuSave = new System.Windows.Forms.MenuItem();
            this.mnuSaveAs = new System.Windows.Forms.MenuItem();
            this.mnuOpenAndRunWorklist = new System.Windows.Forms.MenuItem();
            this.mnuReload = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.mnuOpenLogFile = new System.Windows.Forms.MenuItem();
            this.mnuNewLogFile = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.mnuExit = new System.Windows.Forms.MenuItem();
            this.mnuEdit = new System.Windows.Forms.MenuItem();
            this.mnuMachineSettings = new System.Windows.Forms.MenuItem();
            this.mnuMachineSettingsEdit = new System.Windows.Forms.MenuItem();
            this.mnuReloadMachineSettings = new System.Windows.Forms.MenuItem();
            this.mnuSyringeSettings = new System.Windows.Forms.MenuItem();
            this.mnuMotionSettings = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.mnuTools = new System.Windows.Forms.MenuItem();
            this.mnuMotion = new System.Windows.Forms.MenuItem();
            this.mnuController = new System.Windows.Forms.MenuItem();
            this.mnuSyringe = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.mnuIOControlWindow = new System.Windows.Forms.MenuItem();
            this.mnuSetOutputs = new System.Windows.Forms.MenuItem();
            this.mnuReadInputs = new System.Windows.Forms.MenuItem();
            this.mnuInitInputsAndOutputs = new System.Windows.Forms.MenuItem();
            this.mnuCommands = new System.Windows.Forms.MenuItem();
            this.mnuTestCommands = new System.Windows.Forms.MenuItem();
            this.mnuBackRight = new System.Windows.Forms.MenuItem();
            this.mnuMovePipetteSafe = new System.Windows.Forms.MenuItem();
            this.mnuMoveGridRobotSafe = new System.Windows.Forms.MenuItem();
            this.mnuIspMove = new System.Windows.Forms.MenuItem();
            this.mnuInspMoveTip1 = new System.Windows.Forms.MenuItem();
            this.mnuInspMoveTip2 = new System.Windows.Forms.MenuItem();
            this.mnuMoveZUp = new System.Windows.Forms.MenuItem();
            this.mnuStrobeOn = new System.Windows.Forms.MenuItem();
            this.mnuStrobeOff = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.mnuRun = new System.Windows.Forms.MenuItem();
            this.mnuCheckSequence = new System.Windows.Forms.MenuItem();
            this.mnuRunSequence = new System.Windows.Forms.MenuItem();
            this.mnuPauseSequence = new System.Windows.Forms.MenuItem();
            this.mnuStopSequence = new System.Windows.Forms.MenuItem();
            this.mnuStep = new System.Windows.Forms.MenuItem();
            this.mnuDryRun = new System.Windows.Forms.MenuItem();
            this.mnuStopNow = new System.Windows.Forms.MenuItem();
            this.mnuHelp = new System.Windows.Forms.MenuItem();
            this.mnuCmdRef = new System.Windows.Forms.MenuItem();
            this.mnuHelpAbout = new System.Windows.Forms.MenuItem();
            this.imlProcesses = new System.Windows.Forms.ImageList(this.components);
            this.StatBar = new System.Windows.Forms.StatusBar();
            this.pnlLoop = new System.Windows.Forms.StatusBarPanel();
            this.pnlMessage = new System.Windows.Forms.StatusBarPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblActiveCam = new System.Windows.Forms.Label();
            this.btnSwitchCamera = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.checkBoxDebugEnableStrobe = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.textBoxVideoFolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.TimerVariables = new System.Windows.Forms.Timer(this.components);
            this.imlButtons = new System.Windows.Forms.ImageList(this.components);
            this.DisplayWindow = new System.Windows.Forms.PictureBox();
            this.radioButtonTip1 = new System.Windows.Forms.RadioButton();
            this.radioButtonTip2 = new System.Windows.Forms.RadioButton();
            this.radioButtonTip3 = new System.Windows.Forms.RadioButton();
            this.tabControlRobot = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.checkBoxInspEndOfStream = new System.Windows.Forms.CheckBox();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.numericUpDownMixDutyCycle = new System.Windows.Forms.NumericUpDown();
            this.label23 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.numericUpDownMixAmplitude = new System.Windows.Forms.NumericUpDown();
            this.label17 = new System.Windows.Forms.Label();
            this.numericUpDownMixFreq = new System.Windows.Forms.NumericUpDown();
            this.checkBoxMixOn = new System.Windows.Forms.CheckBox();
            this.checkBoxInspCosine = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.numericUpDownCosAmp = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownCosFreq = new System.Windows.Forms.NumericUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.numericUpDownTrapTrailing = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownTrapDwell = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownTrapLeading = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownTipAmp = new System.Windows.Forms.NumericUpDown();
            this.button10 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnBypassSyringeValves = new System.Windows.Forms.Button();
            this.cbPrimeWithoutMove = new System.Windows.Forms.CheckBox();
            this.btnLoadedSampleVolReset = new System.Windows.Forms.Button();
            this.lblLoadedSampleVolume = new System.Windows.Forms.Label();
            this.btnUltrasonicWash = new System.Windows.Forms.Button();
            this.label32 = new System.Windows.Forms.Label();
            this.numericUpDownUSWashNumberOfCycles = new System.Windows.Forms.NumericUpDown();
            this.label31 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.numericUpDownUSWashSyringeSpeedCode = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.buttonWipeTips = new System.Windows.Forms.Button();
            this.numericUpDownNumberWipeTips = new System.Windows.Forms.NumericUpDown();
            this.button8 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.numericUpDownWash = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownAspirateVolume = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownPrimeNumber = new System.Windows.Forms.NumericUpDown();
            this.tabSpotSample = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label14 = new System.Windows.Forms.Label();
            this.rbTimeResTip1and2 = new System.Windows.Forms.RadioButton();
            this.rbTimeResTip2 = new System.Windows.Forms.RadioButton();
            this.rbTimeResTip1 = new System.Windows.Forms.RadioButton();
            this.label36 = new System.Windows.Forms.Label();
            this.label35 = new System.Windows.Forms.Label();
            this.numericUpDownOTFIDelayAfterTrip = new System.Windows.Forms.NumericUpDown();
            this.label34 = new System.Windows.Forms.Label();
            this.numericUpDownOTFISlowSpeed = new System.Windows.Forms.NumericUpDown();
            this.label33 = new System.Windows.Forms.Label();
            this.numericUpDownOTFIPercentWayToTarget = new System.Windows.Forms.NumericUpDown();
            this.cbOTFIEnableSlowDown = new System.Windows.Forms.CheckBox();
            this.cbOTFIUseFlashAcquisition = new System.Windows.Forms.CheckBox();
            this.numericUpDownPauseOTF = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
            this.cbOTF_Enable = new System.Windows.Forms.CheckBox();
            this.lblTipMovement = new System.Windows.Forms.Label();
            this.checkBoxAutomaticPlunge = new System.Windows.Forms.CheckBox();
            this.checkBoxCryoMode = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.buttonPositionGridToSafePoint = new System.Windows.Forms.Button();
            this.buttonPositionGridToCamera = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.buttonTipToCam = new System.Windows.Forms.Button();
            this.buttonCenterTip = new System.Windows.Forms.Button();
            this.numericUpDownSpotDropNumber = new System.Windows.Forms.NumericUpDown();
            this.checkBoxClickToSpot = new System.Windows.Forms.CheckBox();
            this.buttonAddQueueTargets = new System.Windows.Forms.Button();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.btnCamLoadSettingsFromFile = new System.Windows.Forms.Button();
            this.btnCamSaveSettingsToFile = new System.Windows.Forms.Button();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.btnCamSetGain = new System.Windows.Forms.Button();
            this.btnCamReadGain = new System.Windows.Forms.Button();
            this.numericUpDownCamGain = new System.Windows.Forms.NumericUpDown();
            this.labelMaxExposure = new System.Windows.Forms.Label();
            this.labelMaxFPS = new System.Windows.Forms.Label();
            this.labelMinExposure = new System.Windows.Forms.Label();
            this.labelCurrentExposure = new System.Windows.Forms.Label();
            this.labelPixelClockMin = new System.Windows.Forms.Label();
            this.labelPixelClockMax = new System.Windows.Forms.Label();
            this.trackBarExposure = new System.Windows.Forms.TrackBar();
            this.labelMinFPS = new System.Windows.Forms.Label();
            this.labelCurrentFPS = new System.Windows.Forms.Label();
            this.labelCurrentPixelClock = new System.Windows.Forms.Label();
            this.trackBarPixelClock = new System.Windows.Forms.TrackBar();
            this.trackBarFPS = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tabStain = new System.Windows.Forms.TabPage();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.comboBoxSTNMode = new System.Windows.Forms.ComboBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.buttonSTNTipToCam = new System.Windows.Forms.Button();
            this.buttonSTNCenterTip = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.buttonSTNPositionGridToSafePoint = new System.Windows.Forms.Button();
            this.buttonSTNPositionGridToCamera = new System.Windows.Forms.Button();
            this.label22 = new System.Windows.Forms.Label();
            this.numericUpDownSTNDelay2_ms = new System.Windows.Forms.NumericUpDown();
            this.btnSTNRunStainQueue = new System.Windows.Forms.Button();
            this.btnSTNClearQueue = new System.Windows.Forms.Button();
            this.checkBoxExperimentMode = new System.Windows.Forms.CheckBox();
            this.numericUpDownSTNDelay_ms = new System.Windows.Forms.NumericUpDown();
            this.label20 = new System.Windows.Forms.Label();
            this.numericUpDownSTNTip3Drops = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownSTNTip2Drops = new System.Windows.Forms.NumericUpDown();
            this.label19 = new System.Windows.Forms.Label();
            this.numericUpDownSTNTip1Drops = new System.Windows.Forms.NumericUpDown();
            this.buttonSTNAddQueueTargets = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox12 = new System.Windows.Forms.GroupBox();
            this.btn3StackLoadDefaults = new System.Windows.Forms.Button();
            this.label24 = new System.Windows.Forms.Label();
            this.numericUpDown3StackDecel = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown3StackAccel = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown3StackVel = new System.Windows.Forms.NumericUpDown();
            this.btn3StackTestPlunge = new System.Windows.Forms.Button();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.btnDGCTest = new System.Windows.Forms.Button();
            this.lblDGCRemotePath = new System.Windows.Forms.Label();
            this.cbDGCPromptAndSave = new System.Windows.Forms.CheckBox();
            this.cbDGCEnabled = new System.Windows.Forms.CheckBox();
            this.btnTestStopLive = new System.Windows.Forms.Button();
            this.btnTestStartLive = new System.Windows.Forms.Button();
            this.btnTestDoubleTriggeredAcquision = new System.Windows.Forms.Button();
            this.btnTest123 = new System.Windows.Forms.Button();
            this.btnSaveBitmap = new System.Windows.Forms.Button();
            this.cbDebugTrySavingBitmap = new System.Windows.Forms.CheckBox();
            this.cbDebugAutoStartVideo = new System.Windows.Forms.CheckBox();
            this.cbDebugDisableGridDrop = new System.Windows.Forms.CheckBox();
            this.buttonStain = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.numericUpDownStainTime = new System.Windows.Forms.NumericUpDown();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.label29 = new System.Windows.Forms.Label();
            this.numericUpDownDebugStrobeDelay = new System.Windows.Forms.NumericUpDown();
            this.label28 = new System.Windows.Forms.Label();
            this.numericUpDownDebugStrobeDuration = new System.Windows.Forms.NumericUpDown();
            this.label27 = new System.Windows.Forms.Label();
            this.numericUpDownDebugVideoWidth = new System.Windows.Forms.NumericUpDown();
            this.label26 = new System.Windows.Forms.Label();
            this.numericUpDownDebugJPG = new System.Windows.Forms.NumericUpDown();
            this.labelTip1VideoCentered = new System.Windows.Forms.Label();
            this.labelTip2VideoCentered = new System.Windows.Forms.Label();
            this.labelTip3VideoCentered = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.labelRecordingVideo = new System.Windows.Forms.Label();
            this.btnGotoSafePosition = new System.Windows.Forms.Button();
            this.buttonWriteSettingsToXMLFile = new System.Windows.Forms.Button();
            this.buttonArchiveExperimentSettings = new System.Windows.Forms.Button();
            this.textBoxArchiveFile = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.textBoxArchiveNote = new System.Windows.Forms.TextBox();
            this.saveFileBMPDialog = new System.Windows.Forms.SaveFileDialog();
            this.saveImagesAndSettingsFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.saveFileDialogArchiveOnly = new System.Windows.Forms.SaveFileDialog();
            this.cbSelectTip1andTip2 = new System.Windows.Forms.CheckBox();
            this.btnCloseSyringeValves = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pnlLoop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlMessage)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DisplayWindow)).BeginInit();
            this.tabControlRobot.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMixDutyCycle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMixAmplitude)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMixFreq)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCosAmp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCosFreq)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTrapTrailing)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTrapDwell)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTrapLeading)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTipAmp)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownUSWashNumberOfCycles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownUSWashSyringeSpeedCode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumberWipeTips)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWash)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAspirateVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPrimeNumber)).BeginInit();
            this.tabSpotSample.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOTFIDelayAfterTrip)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOTFISlowSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOTFIPercentWayToTarget)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPauseOTF)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSpotDropNumber)).BeginInit();
            this.tabPage4.SuspendLayout();
            this.groupBox11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCamGain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarExposure)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPixelClock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFPS)).BeginInit();
            this.tabStain.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSTNDelay2_ms)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSTNDelay_ms)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSTNTip3Drops)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSTNTip2Drops)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSTNTip1Drops)).BeginInit();
            this.tabPage3.SuspendLayout();
            this.groupBox12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3StackDecel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3StackAccel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3StackVel)).BeginInit();
            this.groupBox9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStainTime)).BeginInit();
            this.groupBox10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDebugStrobeDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDebugStrobeDuration)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDebugVideoWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDebugJPG)).BeginInit();
            this.SuspendLayout();
            // 
            // mnuMain
            // 
            this.mnuMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuFile,
            this.mnuEdit,
            this.mnuTools,
            this.mnuCommands,
            this.mnuRun,
            this.mnuHelp});
            // 
            // mnuFile
            // 
            this.mnuFile.Index = 0;
            this.mnuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuOpenWorklist,
            this.mnuSave,
            this.mnuSaveAs,
            this.mnuOpenAndRunWorklist,
            this.mnuReload,
            this.menuItem4,
            this.mnuOpenLogFile,
            this.mnuNewLogFile,
            this.menuItem6,
            this.mnuExit});
            this.mnuFile.Text = "&File";
            // 
            // mnuOpenWorklist
            // 
            this.mnuOpenWorklist.Index = 0;
            this.mnuOpenWorklist.Text = "&Open";
            this.mnuOpenWorklist.Click += new System.EventHandler(this.mnuOpenWorklist_Click);
            // 
            // mnuSave
            // 
            this.mnuSave.Index = 1;
            this.mnuSave.Text = "&Save";
            this.mnuSave.Click += new System.EventHandler(this.mnuSave_Click);
            // 
            // mnuSaveAs
            // 
            this.mnuSaveAs.Index = 2;
            this.mnuSaveAs.Text = "Save &As";
            this.mnuSaveAs.Click += new System.EventHandler(this.mnuSaveAs_Click);
            // 
            // mnuOpenAndRunWorklist
            // 
            this.mnuOpenAndRunWorklist.Index = 3;
            this.mnuOpenAndRunWorklist.Shortcut = System.Windows.Forms.Shortcut.Ctrl1;
            this.mnuOpenAndRunWorklist.Text = "&1 Open and Run";
            this.mnuOpenAndRunWorklist.Click += new System.EventHandler(this.mnuOpenAndRunWorklist_Click);
            // 
            // mnuReload
            // 
            this.mnuReload.Index = 4;
            this.mnuReload.Text = "Reload";
            this.mnuReload.Click += new System.EventHandler(this.mnuReload_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 5;
            this.menuItem4.Text = "-";
            // 
            // mnuOpenLogFile
            // 
            this.mnuOpenLogFile.Index = 6;
            this.mnuOpenLogFile.Text = "Open Existing Log File (Append)";
            this.mnuOpenLogFile.Click += new System.EventHandler(this.mnuOpenLogFile_Click);
            // 
            // mnuNewLogFile
            // 
            this.mnuNewLogFile.Index = 7;
            this.mnuNewLogFile.Text = "Create New Log File";
            this.mnuNewLogFile.Click += new System.EventHandler(this.mnuNewLogFile_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 8;
            this.menuItem6.Text = "-";
            // 
            // mnuExit
            // 
            this.mnuExit.Index = 9;
            this.mnuExit.Text = "E&xit";
            this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // mnuEdit
            // 
            this.mnuEdit.Index = 1;
            this.mnuEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuMachineSettings,
            this.mnuSyringeSettings,
            this.mnuMotionSettings,
            this.menuItem2});
            this.mnuEdit.Text = "&Edit";
            // 
            // mnuMachineSettings
            // 
            this.mnuMachineSettings.Index = 0;
            this.mnuMachineSettings.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuMachineSettingsEdit,
            this.mnuReloadMachineSettings});
            this.mnuMachineSettings.Text = "Machine Settings (System Points)";
            this.mnuMachineSettings.Click += new System.EventHandler(this.mnuMachineSettings_Click);
            // 
            // mnuMachineSettingsEdit
            // 
            this.mnuMachineSettingsEdit.Index = 0;
            this.mnuMachineSettingsEdit.Text = "Edit";
            this.mnuMachineSettingsEdit.Click += new System.EventHandler(this.menuItem3_Click);
            // 
            // mnuReloadMachineSettings
            // 
            this.mnuReloadMachineSettings.Index = 1;
            this.mnuReloadMachineSettings.Text = "ReLoad";
            this.mnuReloadMachineSettings.Click += new System.EventHandler(this.mnuReloadMachineSettings_Click);
            // 
            // mnuSyringeSettings
            // 
            this.mnuSyringeSettings.Index = 1;
            this.mnuSyringeSettings.Text = "Syringe Settings";
            this.mnuSyringeSettings.Click += new System.EventHandler(this.mnuSyringeSettings_Click);
            // 
            // mnuMotionSettings
            // 
            this.mnuMotionSettings.Index = 2;
            this.mnuMotionSettings.Text = "Motion Settings";
            this.mnuMotionSettings.Click += new System.EventHandler(this.mnuMotionSettings_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 3;
            this.menuItem2.Text = "";
            // 
            // mnuTools
            // 
            this.mnuTools.Index = 2;
            this.mnuTools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuMotion,
            this.mnuController,
            this.mnuSyringe,
            this.menuItem1});
            this.mnuTools.Text = "&Tools";
            // 
            // mnuMotion
            // 
            this.mnuMotion.Index = 0;
            this.mnuMotion.Shortcut = System.Windows.Forms.Shortcut.CtrlM;
            this.mnuMotion.Text = "&Motion Control";
            this.mnuMotion.Click += new System.EventHandler(this.mnuMotion_Click);
            // 
            // mnuController
            // 
            this.mnuController.Index = 1;
            this.mnuController.Text = "&Piezo Firing Control";
            this.mnuController.Click += new System.EventHandler(this.mnuController_Click);
            // 
            // mnuSyringe
            // 
            this.mnuSyringe.Index = 2;
            this.mnuSyringe.Text = "&Syringe Control";
            this.mnuSyringe.Click += new System.EventHandler(this.mnuSyringe_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 3;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuIOControlWindow,
            this.mnuSetOutputs,
            this.mnuReadInputs,
            this.mnuInitInputsAndOutputs});
            this.menuItem1.Text = "IO Control";
            // 
            // mnuIOControlWindow
            // 
            this.mnuIOControlWindow.Index = 0;
            this.mnuIOControlWindow.Text = "IO Control Window";
            this.mnuIOControlWindow.Click += new System.EventHandler(this.mnuIOControlWindow_Click);
            // 
            // mnuSetOutputs
            // 
            this.mnuSetOutputs.Index = 1;
            this.mnuSetOutputs.Text = "Set Outputs (Console Utility)";
            this.mnuSetOutputs.Click += new System.EventHandler(this.mnuSetOutputs_Click);
            // 
            // mnuReadInputs
            // 
            this.mnuReadInputs.Index = 2;
            this.mnuReadInputs.Text = "Read Inputs (Console Utility)";
            this.mnuReadInputs.Click += new System.EventHandler(this.mnuReadInputs_Click);
            // 
            // mnuInitInputsAndOutputs
            // 
            this.mnuInitInputsAndOutputs.Index = 3;
            this.mnuInitInputsAndOutputs.Text = "Init Inputs and Outputs";
            this.mnuInitInputsAndOutputs.Click += new System.EventHandler(this.mnuInitInputsAndOutputs_Click);
            // 
            // mnuCommands
            // 
            this.mnuCommands.Index = 3;
            this.mnuCommands.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuTestCommands,
            this.mnuBackRight,
            this.mnuIspMove,
            this.menuItem3});
            this.mnuCommands.Text = "&Commands";
            // 
            // mnuTestCommands
            // 
            this.mnuTestCommands.Enabled = false;
            this.mnuTestCommands.Index = 0;
            this.mnuTestCommands.Text = "&Test Commands";
            this.mnuTestCommands.Click += new System.EventHandler(this.mnuTestCommands_Click);
            // 
            // mnuBackRight
            // 
            this.mnuBackRight.Index = 1;
            this.mnuBackRight.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuMovePipetteSafe,
            this.mnuMoveGridRobotSafe});
            this.mnuBackRight.Text = "Move To Safe";
            this.mnuBackRight.Click += new System.EventHandler(this.mnuBackRight_Click);
            // 
            // mnuMovePipetteSafe
            // 
            this.mnuMovePipetteSafe.Index = 0;
            this.mnuMovePipetteSafe.Text = "Pipette";
            this.mnuMovePipetteSafe.Click += new System.EventHandler(this.mnuMovePipetteSafe_Click);
            // 
            // mnuMoveGridRobotSafe
            // 
            this.mnuMoveGridRobotSafe.Index = 1;
            this.mnuMoveGridRobotSafe.Text = "GridRobot";
            this.mnuMoveGridRobotSafe.Click += new System.EventHandler(this.mnuMoveGridRobotSafe_Click);
            // 
            // mnuIspMove
            // 
            this.mnuIspMove.Index = 2;
            this.mnuIspMove.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuInspMoveTip1,
            this.mnuInspMoveTip2,
            this.mnuMoveZUp,
            this.mnuStrobeOn,
            this.mnuStrobeOff});
            this.mnuIspMove.Text = "Move To Inspection";
            // 
            // mnuInspMoveTip1
            // 
            this.mnuInspMoveTip1.Index = 0;
            this.mnuInspMoveTip1.Text = "Tip 1";
            this.mnuInspMoveTip1.Click += new System.EventHandler(this.mnuInspMoveTip1_Click);
            // 
            // mnuInspMoveTip2
            // 
            this.mnuInspMoveTip2.Enabled = false;
            this.mnuInspMoveTip2.Index = 1;
            this.mnuInspMoveTip2.Text = "Tip 2";
            this.mnuInspMoveTip2.Click += new System.EventHandler(this.mnuInspMoveTip2_Click);
            // 
            // mnuMoveZUp
            // 
            this.mnuMoveZUp.Index = 2;
            this.mnuMoveZUp.Text = "ZUp";
            this.mnuMoveZUp.Click += new System.EventHandler(this.mnuMoveZUp_Click);
            // 
            // mnuStrobeOn
            // 
            this.mnuStrobeOn.Enabled = false;
            this.mnuStrobeOn.Index = 3;
            this.mnuStrobeOn.Text = "StrobeOn";
            this.mnuStrobeOn.Click += new System.EventHandler(this.mnuStrobeOn_Click);
            // 
            // mnuStrobeOff
            // 
            this.mnuStrobeOff.Enabled = false;
            this.mnuStrobeOff.Index = 4;
            this.mnuStrobeOff.Text = "StrobeOff";
            this.mnuStrobeOff.Click += new System.EventHandler(this.mnuStrobeOff_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Enabled = false;
            this.menuItem3.Index = 3;
            this.menuItem3.Text = "Test Code";
            this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click_1);
            // 
            // mnuRun
            // 
            this.mnuRun.Index = 4;
            this.mnuRun.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuCheckSequence,
            this.mnuRunSequence,
            this.mnuPauseSequence,
            this.mnuStopSequence,
            this.mnuStep,
            this.mnuDryRun,
            this.mnuStopNow});
            this.mnuRun.Text = "&Recipe";
            // 
            // mnuCheckSequence
            // 
            this.mnuCheckSequence.Index = 0;
            this.mnuCheckSequence.Text = "Check Sequence";
            this.mnuCheckSequence.Click += new System.EventHandler(this.mnuCheckSequence_Click);
            // 
            // mnuRunSequence
            // 
            this.mnuRunSequence.Index = 1;
            this.mnuRunSequence.Text = "Run";
            this.mnuRunSequence.Click += new System.EventHandler(this.mnuRunSequence_Click);
            // 
            // mnuPauseSequence
            // 
            this.mnuPauseSequence.Index = 2;
            this.mnuPauseSequence.Text = "Pause";
            this.mnuPauseSequence.Click += new System.EventHandler(this.mnuPauseSequence_Click);
            // 
            // mnuStopSequence
            // 
            this.mnuStopSequence.Index = 3;
            this.mnuStopSequence.Text = "Stop";
            this.mnuStopSequence.Click += new System.EventHandler(this.mnuStopSequence_Click);
            // 
            // mnuStep
            // 
            this.mnuStep.Index = 4;
            this.mnuStep.Text = "Step";
            this.mnuStep.Click += new System.EventHandler(this.mnuStep_Click);
            // 
            // mnuDryRun
            // 
            this.mnuDryRun.Index = 5;
            this.mnuDryRun.Text = "Dry Run";
            this.mnuDryRun.Click += new System.EventHandler(this.mnuDryRun_Click);
            // 
            // mnuStopNow
            // 
            this.mnuStopNow.Index = 6;
            this.mnuStopNow.Text = "Stop Now";
            this.mnuStopNow.Click += new System.EventHandler(this.mnuStopNow_Click);
            // 
            // mnuHelp
            // 
            this.mnuHelp.Index = 5;
            this.mnuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuCmdRef,
            this.mnuHelpAbout});
            this.mnuHelp.Text = "&Help";
            // 
            // mnuCmdRef
            // 
            this.mnuCmdRef.Index = 0;
            this.mnuCmdRef.Text = "Command Reference";
            this.mnuCmdRef.Click += new System.EventHandler(this.mnuCmdRef_Click);
            // 
            // mnuHelpAbout
            // 
            this.mnuHelpAbout.Index = 1;
            this.mnuHelpAbout.Text = "About";
            this.mnuHelpAbout.Click += new System.EventHandler(this.mnuHelpAbout_Click);
            // 
            // imlProcesses
            // 
            this.imlProcesses.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imlProcesses.ImageStream")));
            this.imlProcesses.TransparentColor = System.Drawing.Color.Transparent;
            this.imlProcesses.Images.SetKeyName(0, "");
            this.imlProcesses.Images.SetKeyName(1, "");
            this.imlProcesses.Images.SetKeyName(2, "");
            this.imlProcesses.Images.SetKeyName(3, "");
            this.imlProcesses.Images.SetKeyName(4, "");
            this.imlProcesses.Images.SetKeyName(5, "");
            this.imlProcesses.Images.SetKeyName(6, "");
            this.imlProcesses.Images.SetKeyName(7, "");
            this.imlProcesses.Images.SetKeyName(8, "");
            this.imlProcesses.Images.SetKeyName(9, "");
            // 
            // StatBar
            // 
            this.StatBar.Location = new System.Drawing.Point(0, 628);
            this.StatBar.Name = "StatBar";
            this.StatBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.pnlLoop,
            this.pnlMessage});
            this.StatBar.ShowPanels = true;
            this.StatBar.Size = new System.Drawing.Size(1051, 24);
            this.StatBar.TabIndex = 6;
            // 
            // pnlLoop
            // 
            this.pnlLoop.MinWidth = 75;
            this.pnlLoop.Name = "pnlLoop";
            this.pnlLoop.Width = 175;
            // 
            // pnlMessage
            // 
            this.pnlMessage.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            this.pnlMessage.Name = "pnlMessage";
            this.pnlMessage.Width = 859;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lblActiveCam);
            this.groupBox1.Controls.Add(this.btnSwitchCamera);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.checkBoxDebugEnableStrobe);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.textBoxVideoFolder);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Location = new System.Drawing.Point(8, 498);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(752, 80);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Video Settings";
            // 
            // lblActiveCam
            // 
            this.lblActiveCam.AutoSize = true;
            this.lblActiveCam.Location = new System.Drawing.Point(533, 50);
            this.lblActiveCam.Name = "lblActiveCam";
            this.lblActiveCam.Size = new System.Drawing.Size(32, 13);
            this.lblActiveCam.TabIndex = 28;
            this.lblActiveCam.Text = "(Grid)";
            // 
            // btnSwitchCamera
            // 
            this.btnSwitchCamera.Location = new System.Drawing.Point(427, 44);
            this.btnSwitchCamera.Name = "btnSwitchCamera";
            this.btnSwitchCamera.Size = new System.Drawing.Size(97, 26);
            this.btnSwitchCamera.TabIndex = 27;
            this.btnSwitchCamera.Text = "Switch Camera";
            this.btnSwitchCamera.UseVisualStyleBackColor = true;
            this.btnSwitchCamera.Click += new System.EventHandler(this.btnSwitchCamera_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(311, 51);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(112, 13);
            this.label6.TabIndex = 26;
            this.label6.Text = "Change Camera View:";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(176, 43);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(97, 26);
            this.button3.TabIndex = 24;
            this.button3.Text = "Stop";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.btnStopRecording_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(73, 43);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(97, 26);
            this.button2.TabIndex = 23;
            this.button2.Text = "Start";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btnStartStopVideoRecording_Click);
            // 
            // checkBoxDebugEnableStrobe
            // 
            this.checkBoxDebugEnableStrobe.AutoSize = true;
            this.checkBoxDebugEnableStrobe.Location = new System.Drawing.Point(588, 50);
            this.checkBoxDebugEnableStrobe.Name = "checkBoxDebugEnableStrobe";
            this.checkBoxDebugEnableStrobe.Size = new System.Drawing.Size(154, 17);
            this.checkBoxDebugEnableStrobe.TabIndex = 25;
            this.checkBoxDebugEnableStrobe.Text = "Turn On Strobe When Live";
            this.checkBoxDebugEnableStrobe.UseVisualStyleBackColor = true;
            this.checkBoxDebugEnableStrobe.CheckedChanged += new System.EventHandler(this.checkBoxDebugEnableStrobe_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(649, 17);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(97, 26);
            this.button1.TabIndex = 21;
            this.button1.Text = "Change";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnChooseVideoFolder_Click);
            // 
            // textBoxVideoFolder
            // 
            this.textBoxVideoFolder.Location = new System.Drawing.Point(51, 20);
            this.textBoxVideoFolder.Name = "textBoxVideoFolder";
            this.textBoxVideoFolder.Size = new System.Drawing.Size(592, 20);
            this.textBoxVideoFolder.TabIndex = 21;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Recording:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 13);
            this.label5.TabIndex = 22;
            this.label5.Text = "Folder:";
            // 
            // TimerVariables
            // 
            this.TimerVariables.Interval = 300;
            this.TimerVariables.Tick += new System.EventHandler(this.TimerVariables_Tick);
            // 
            // imlButtons
            // 
            this.imlButtons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imlButtons.ImageStream")));
            this.imlButtons.TransparentColor = System.Drawing.Color.Transparent;
            this.imlButtons.Images.SetKeyName(0, "");
            this.imlButtons.Images.SetKeyName(1, "");
            this.imlButtons.Images.SetKeyName(2, "");
            this.imlButtons.Images.SetKeyName(3, "");
            this.imlButtons.Images.SetKeyName(4, "");
            this.imlButtons.Images.SetKeyName(5, "");
            this.imlButtons.Images.SetKeyName(6, "");
            this.imlButtons.Images.SetKeyName(7, "");
            // 
            // DisplayWindow
            // 
            this.DisplayWindow.Location = new System.Drawing.Point(8, 12);
            this.DisplayWindow.Name = "DisplayWindow";
            this.DisplayWindow.Size = new System.Drawing.Size(752, 480);
            this.DisplayWindow.TabIndex = 16;
            this.DisplayWindow.TabStop = false;
            this.DisplayWindow.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseClick);
            // 
            // radioButtonTip1
            // 
            this.radioButtonTip1.AutoSize = true;
            this.radioButtonTip1.Checked = true;
            this.radioButtonTip1.Location = new System.Drawing.Point(835, 417);
            this.radioButtonTip1.Name = "radioButtonTip1";
            this.radioButtonTip1.Size = new System.Drawing.Size(49, 17);
            this.radioButtonTip1.TabIndex = 17;
            this.radioButtonTip1.TabStop = true;
            this.radioButtonTip1.Text = "Tip 1";
            this.radioButtonTip1.UseVisualStyleBackColor = true;
            // 
            // radioButtonTip2
            // 
            this.radioButtonTip2.AutoSize = true;
            this.radioButtonTip2.Location = new System.Drawing.Point(835, 440);
            this.radioButtonTip2.Name = "radioButtonTip2";
            this.radioButtonTip2.Size = new System.Drawing.Size(49, 17);
            this.radioButtonTip2.TabIndex = 18;
            this.radioButtonTip2.Text = "Tip 2";
            this.radioButtonTip2.UseVisualStyleBackColor = true;
            // 
            // radioButtonTip3
            // 
            this.radioButtonTip3.AutoSize = true;
            this.radioButtonTip3.Enabled = false;
            this.radioButtonTip3.Location = new System.Drawing.Point(835, 463);
            this.radioButtonTip3.Name = "radioButtonTip3";
            this.radioButtonTip3.Size = new System.Drawing.Size(49, 17);
            this.radioButtonTip3.TabIndex = 19;
            this.radioButtonTip3.Text = "Tip 3";
            this.radioButtonTip3.UseVisualStyleBackColor = true;
            // 
            // tabControlRobot
            // 
            this.tabControlRobot.Controls.Add(this.tabPage1);
            this.tabControlRobot.Controls.Add(this.tabPage2);
            this.tabControlRobot.Controls.Add(this.tabSpotSample);
            this.tabControlRobot.Controls.Add(this.tabPage4);
            this.tabControlRobot.Controls.Add(this.tabStain);
            this.tabControlRobot.Controls.Add(this.tabPage3);
            this.tabControlRobot.Location = new System.Drawing.Point(766, 12);
            this.tabControlRobot.Name = "tabControlRobot";
            this.tabControlRobot.SelectedIndex = 0;
            this.tabControlRobot.Size = new System.Drawing.Size(285, 376);
            this.tabControlRobot.TabIndex = 20;
            this.tabControlRobot.SelectedIndexChanged += new System.EventHandler(this.tabControlRobot_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.checkBoxInspEndOfStream);
            this.tabPage1.Controls.Add(this.groupBox8);
            this.tabPage1.Controls.Add(this.checkBoxInspCosine);
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.button10);
            this.tabPage1.Controls.Add(this.button9);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(277, 350);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Inspect";
            // 
            // checkBoxInspEndOfStream
            // 
            this.checkBoxInspEndOfStream.AutoSize = true;
            this.checkBoxInspEndOfStream.Location = new System.Drawing.Point(23, 161);
            this.checkBoxInspEndOfStream.Name = "checkBoxInspEndOfStream";
            this.checkBoxInspEndOfStream.Size = new System.Drawing.Size(118, 17);
            this.checkBoxInspEndOfStream.TabIndex = 36;
            this.checkBoxInspEndOfStream.Text = "Insp End Of Stream";
            this.checkBoxInspEndOfStream.UseVisualStyleBackColor = true;
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.numericUpDownMixDutyCycle);
            this.groupBox8.Controls.Add(this.label23);
            this.groupBox8.Controls.Add(this.label18);
            this.groupBox8.Controls.Add(this.numericUpDownMixAmplitude);
            this.groupBox8.Controls.Add(this.label17);
            this.groupBox8.Controls.Add(this.numericUpDownMixFreq);
            this.groupBox8.Controls.Add(this.checkBoxMixOn);
            this.groupBox8.Location = new System.Drawing.Point(9, 186);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(260, 69);
            this.groupBox8.TabIndex = 35;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "In Tip Mixing";
            // 
            // numericUpDownMixDutyCycle
            // 
            this.numericUpDownMixDutyCycle.DecimalPlaces = 1;
            this.numericUpDownMixDutyCycle.Location = new System.Drawing.Point(201, 39);
            this.numericUpDownMixDutyCycle.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            65536});
            this.numericUpDownMixDutyCycle.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownMixDutyCycle.Name = "numericUpDownMixDutyCycle";
            this.numericUpDownMixDutyCycle.Size = new System.Drawing.Size(42, 20);
            this.numericUpDownMixDutyCycle.TabIndex = 42;
            this.numericUpDownMixDutyCycle.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(190, 19);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(58, 13);
            this.label23.TabIndex = 41;
            this.label23.Text = "Duty Cycle";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(126, 19);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(53, 13);
            this.label18.TabIndex = 40;
            this.label18.Text = "Amplitude";
            // 
            // numericUpDownMixAmplitude
            // 
            this.numericUpDownMixAmplitude.Location = new System.Drawing.Point(129, 39);
            this.numericUpDownMixAmplitude.Maximum = new decimal(new int[] {
            2047,
            0,
            0,
            0});
            this.numericUpDownMixAmplitude.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownMixAmplitude.Name = "numericUpDownMixAmplitude";
            this.numericUpDownMixAmplitude.Size = new System.Drawing.Size(59, 20);
            this.numericUpDownMixAmplitude.TabIndex = 39;
            this.numericUpDownMixAmplitude.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(60, 19);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(57, 13);
            this.label17.TabIndex = 38;
            this.label17.Text = "Frequency";
            // 
            // numericUpDownMixFreq
            // 
            this.numericUpDownMixFreq.Location = new System.Drawing.Point(58, 39);
            this.numericUpDownMixFreq.Maximum = new decimal(new int[] {
            80000,
            0,
            0,
            0});
            this.numericUpDownMixFreq.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMixFreq.Name = "numericUpDownMixFreq";
            this.numericUpDownMixFreq.Size = new System.Drawing.Size(65, 20);
            this.numericUpDownMixFreq.TabIndex = 37;
            this.numericUpDownMixFreq.Value = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            // 
            // checkBoxMixOn
            // 
            this.checkBoxMixOn.AutoSize = true;
            this.checkBoxMixOn.Location = new System.Drawing.Point(12, 26);
            this.checkBoxMixOn.Name = "checkBoxMixOn";
            this.checkBoxMixOn.Size = new System.Drawing.Size(40, 17);
            this.checkBoxMixOn.TabIndex = 36;
            this.checkBoxMixOn.Text = "On";
            this.checkBoxMixOn.UseVisualStyleBackColor = true;
            this.checkBoxMixOn.CheckedChanged += new System.EventHandler(this.checkBoxMixOn_CheckedChanged);
            // 
            // checkBoxInspCosine
            // 
            this.checkBoxInspCosine.AutoSize = true;
            this.checkBoxInspCosine.Checked = true;
            this.checkBoxInspCosine.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxInspCosine.Location = new System.Drawing.Point(23, 138);
            this.checkBoxInspCosine.Name = "checkBoxInspCosine";
            this.checkBoxInspCosine.Size = new System.Drawing.Size(103, 17);
            this.checkBoxInspCosine.TabIndex = 34;
            this.checkBoxInspCosine.Text = "Fire With Cosine";
            this.checkBoxInspCosine.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.numericUpDownCosAmp);
            this.groupBox3.Controls.Add(this.numericUpDownCosFreq);
            this.groupBox3.Location = new System.Drawing.Point(7, 79);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(262, 53);
            this.groupBox3.TabIndex = 33;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Cosine (Frequency, Amplitude)";
            // 
            // numericUpDownCosAmp
            // 
            this.numericUpDownCosAmp.Location = new System.Drawing.Point(140, 19);
            this.numericUpDownCosAmp.Maximum = new decimal(new int[] {
            2047,
            0,
            0,
            0});
            this.numericUpDownCosAmp.Name = "numericUpDownCosAmp";
            this.numericUpDownCosAmp.Size = new System.Drawing.Size(65, 20);
            this.numericUpDownCosAmp.TabIndex = 36;
            this.numericUpDownCosAmp.ValueChanged += new System.EventHandler(this.numericUpDownCosAmp_Changed);
            // 
            // numericUpDownCosFreq
            // 
            this.numericUpDownCosFreq.Location = new System.Drawing.Point(6, 19);
            this.numericUpDownCosFreq.Maximum = new decimal(new int[] {
            80000,
            0,
            0,
            0});
            this.numericUpDownCosFreq.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownCosFreq.Name = "numericUpDownCosFreq";
            this.numericUpDownCosFreq.Size = new System.Drawing.Size(80, 20);
            this.numericUpDownCosFreq.TabIndex = 36;
            this.numericUpDownCosFreq.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownCosFreq.ValueChanged += new System.EventHandler(this.numericUpDownCosFreq_Changed);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.numericUpDownTrapTrailing);
            this.groupBox2.Controls.Add(this.numericUpDownTrapDwell);
            this.groupBox2.Controls.Add(this.numericUpDownTrapLeading);
            this.groupBox2.Controls.Add(this.numericUpDownTipAmp);
            this.groupBox2.Location = new System.Drawing.Point(3, 14);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(266, 56);
            this.groupBox2.TabIndex = 32;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Trapezoid (Leading, Dwell, Trailing,    Amplitude)";
            // 
            // numericUpDownTrapTrailing
            // 
            this.numericUpDownTrapTrailing.Location = new System.Drawing.Point(113, 23);
            this.numericUpDownTrapTrailing.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTrapTrailing.Name = "numericUpDownTrapTrailing";
            this.numericUpDownTrapTrailing.Size = new System.Drawing.Size(46, 20);
            this.numericUpDownTrapTrailing.TabIndex = 35;
            this.numericUpDownTrapTrailing.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTrapTrailing.ValueChanged += new System.EventHandler(this.numericUpDownTrapTrailing_Changed);
            // 
            // numericUpDownTrapDwell
            // 
            this.numericUpDownTrapDwell.Location = new System.Drawing.Point(60, 23);
            this.numericUpDownTrapDwell.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTrapDwell.Name = "numericUpDownTrapDwell";
            this.numericUpDownTrapDwell.Size = new System.Drawing.Size(46, 20);
            this.numericUpDownTrapDwell.TabIndex = 34;
            this.numericUpDownTrapDwell.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTrapDwell.ValueChanged += new System.EventHandler(this.numericUpDownTrapDwell_Changed);
            // 
            // numericUpDownTrapLeading
            // 
            this.numericUpDownTrapLeading.Location = new System.Drawing.Point(6, 23);
            this.numericUpDownTrapLeading.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTrapLeading.Name = "numericUpDownTrapLeading";
            this.numericUpDownTrapLeading.Size = new System.Drawing.Size(46, 20);
            this.numericUpDownTrapLeading.TabIndex = 33;
            this.numericUpDownTrapLeading.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTrapLeading.ValueChanged += new System.EventHandler(this.numericUpDownTrapLeading_Changed);
            // 
            // numericUpDownTipAmp
            // 
            this.numericUpDownTipAmp.Location = new System.Drawing.Point(184, 23);
            this.numericUpDownTipAmp.Maximum = new decimal(new int[] {
            4095,
            0,
            0,
            0});
            this.numericUpDownTipAmp.Name = "numericUpDownTipAmp";
            this.numericUpDownTipAmp.Size = new System.Drawing.Size(65, 20);
            this.numericUpDownTipAmp.TabIndex = 29;
            this.numericUpDownTipAmp.ValueChanged += new System.EventHandler(this.numericUpDownTipAmp_Changed);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(29, 281);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(210, 26);
            this.button10.TabIndex = 25;
            this.button10.Text = "Go to Camera";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.btnTipToSideCamera_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(180, 148);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(77, 22);
            this.button9.TabIndex = 31;
            this.button9.Text = "Fire";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.btnFire_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.btnCloseSyringeValves);
            this.tabPage2.Controls.Add(this.btnBypassSyringeValves);
            this.tabPage2.Controls.Add(this.cbPrimeWithoutMove);
            this.tabPage2.Controls.Add(this.btnLoadedSampleVolReset);
            this.tabPage2.Controls.Add(this.lblLoadedSampleVolume);
            this.tabPage2.Controls.Add(this.btnUltrasonicWash);
            this.tabPage2.Controls.Add(this.label32);
            this.tabPage2.Controls.Add(this.numericUpDownUSWashNumberOfCycles);
            this.tabPage2.Controls.Add(this.label31);
            this.tabPage2.Controls.Add(this.label30);
            this.tabPage2.Controls.Add(this.numericUpDownUSWashSyringeSpeedCode);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.buttonWipeTips);
            this.tabPage2.Controls.Add(this.numericUpDownNumberWipeTips);
            this.tabPage2.Controls.Add(this.button8);
            this.tabPage2.Controls.Add(this.button7);
            this.tabPage2.Controls.Add(this.button6);
            this.tabPage2.Controls.Add(this.label11);
            this.tabPage2.Controls.Add(this.label12);
            this.tabPage2.Controls.Add(this.label13);
            this.tabPage2.Controls.Add(this.numericUpDownWash);
            this.tabPage2.Controls.Add(this.numericUpDownAspirateVolume);
            this.tabPage2.Controls.Add(this.numericUpDownPrimeNumber);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(277, 350);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Aspirate";
            // 
            // btnBypassSyringeValves
            // 
            this.btnBypassSyringeValves.Location = new System.Drawing.Point(204, 38);
            this.btnBypassSyringeValves.Name = "btnBypassSyringeValves";
            this.btnBypassSyringeValves.Size = new System.Drawing.Size(61, 22);
            this.btnBypassSyringeValves.TabIndex = 46;
            this.btnBypassSyringeValves.Text = "Bypass";
            this.btnBypassSyringeValves.UseVisualStyleBackColor = true;
            this.btnBypassSyringeValves.Click += new System.EventHandler(this.btnBypassSyringeValves_Click);
            // 
            // cbPrimeWithoutMove
            // 
            this.cbPrimeWithoutMove.AutoSize = true;
            this.cbPrimeWithoutMove.Location = new System.Drawing.Point(133, 86);
            this.cbPrimeWithoutMove.Name = "cbPrimeWithoutMove";
            this.cbPrimeWithoutMove.Size = new System.Drawing.Size(122, 17);
            this.cbPrimeWithoutMove.TabIndex = 45;
            this.cbPrimeWithoutMove.Text = "Prime Without Move";
            this.cbPrimeWithoutMove.UseVisualStyleBackColor = true;
            // 
            // btnLoadedSampleVolReset
            // 
            this.btnLoadedSampleVolReset.Location = new System.Drawing.Point(196, 9);
            this.btnLoadedSampleVolReset.Name = "btnLoadedSampleVolReset";
            this.btnLoadedSampleVolReset.Size = new System.Drawing.Size(65, 22);
            this.btnLoadedSampleVolReset.TabIndex = 44;
            this.btnLoadedSampleVolReset.Text = "<-- Reset";
            this.btnLoadedSampleVolReset.UseVisualStyleBackColor = true;
            this.btnLoadedSampleVolReset.Click += new System.EventHandler(this.btnLoadedSampleVolReset_Click);
            // 
            // lblLoadedSampleVolume
            // 
            this.lblLoadedSampleVolume.AutoSize = true;
            this.lblLoadedSampleVolume.Location = new System.Drawing.Point(7, 14);
            this.lblLoadedSampleVolume.Name = "lblLoadedSampleVolume";
            this.lblLoadedSampleVolume.Size = new System.Drawing.Size(166, 13);
            this.lblLoadedSampleVolume.TabIndex = 43;
            this.lblLoadedSampleVolume.Text = "Loaded Sample Volume:  0.000 ul";
            // 
            // btnUltrasonicWash
            // 
            this.btnUltrasonicWash.Location = new System.Drawing.Point(107, 115);
            this.btnUltrasonicWash.Name = "btnUltrasonicWash";
            this.btnUltrasonicWash.Size = new System.Drawing.Size(148, 22);
            this.btnUltrasonicWash.TabIndex = 42;
            this.btnUltrasonicWash.Text = "Ultrasonic Wash";
            this.btnUltrasonicWash.UseVisualStyleBackColor = true;
            this.btnUltrasonicWash.Click += new System.EventHandler(this.btnUltasonicWash_Click);
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(10, 99);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(108, 13);
            this.label32.TabIndex = 41;
            this.label32.Text = "No. Ultrasonic Cycles";
            // 
            // numericUpDownUSWashNumberOfCycles
            // 
            this.numericUpDownUSWashNumberOfCycles.Location = new System.Drawing.Point(13, 116);
            this.numericUpDownUSWashNumberOfCycles.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownUSWashNumberOfCycles.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownUSWashNumberOfCycles.Name = "numericUpDownUSWashNumberOfCycles";
            this.numericUpDownUSWashNumberOfCycles.Size = new System.Drawing.Size(78, 20);
            this.numericUpDownUSWashNumberOfCycles.TabIndex = 40;
            this.numericUpDownUSWashNumberOfCycles.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(113, 305);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(135, 13);
            this.label31.TabIndex = 39;
            this.label31.Text = "Prime and Ultrasonic Wash";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(112, 291);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(104, 13);
            this.label30.TabIndex = 38;
            this.label30.Text = "Syringe Speed Code";
            // 
            // numericUpDownUSWashSyringeSpeedCode
            // 
            this.numericUpDownUSWashSyringeSpeedCode.Location = new System.Drawing.Point(46, 298);
            this.numericUpDownUSWashSyringeSpeedCode.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDownUSWashSyringeSpeedCode.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownUSWashSyringeSpeedCode.Name = "numericUpDownUSWashSyringeSpeedCode";
            this.numericUpDownUSWashSyringeSpeedCode.Size = new System.Drawing.Size(43, 20);
            this.numericUpDownUSWashSyringeSpeedCode.TabIndex = 37;
            this.numericUpDownUSWashSyringeSpeedCode.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 232);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(75, 13);
            this.label7.TabIndex = 33;
            this.label7.Text = "No. Tip Wipes";
            // 
            // buttonWipeTips
            // 
            this.buttonWipeTips.Enabled = false;
            this.buttonWipeTips.Location = new System.Drawing.Point(113, 247);
            this.buttonWipeTips.Name = "buttonWipeTips";
            this.buttonWipeTips.Size = new System.Drawing.Size(148, 22);
            this.buttonWipeTips.TabIndex = 32;
            this.buttonWipeTips.Text = "Wipe Tips";
            this.buttonWipeTips.UseVisualStyleBackColor = true;
            this.buttonWipeTips.Click += new System.EventHandler(this.buttonWipeTips_Click);
            // 
            // numericUpDownNumberWipeTips
            // 
            this.numericUpDownNumberWipeTips.Location = new System.Drawing.Point(11, 249);
            this.numericUpDownNumberWipeTips.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownNumberWipeTips.Name = "numericUpDownNumberWipeTips";
            this.numericUpDownNumberWipeTips.Size = new System.Drawing.Size(78, 20);
            this.numericUpDownNumberWipeTips.TabIndex = 31;
            this.numericUpDownNumberWipeTips.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // button8
            // 
            this.button8.Enabled = false;
            this.button8.Location = new System.Drawing.Point(111, 201);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(148, 22);
            this.button8.TabIndex = 30;
            this.button8.Text = "Wash Tips (Rinse Outside)";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.buttonWashTip_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(111, 155);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(148, 22);
            this.button7.TabIndex = 29;
            this.button7.Text = "Aspirate";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.btnAspirate_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(99, 53);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(99, 22);
            this.button6.TabIndex = 28;
            this.button6.Text = "Prime";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.btnPrime_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(8, 189);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(89, 13);
            this.label11.TabIndex = 25;
            this.label11.Text = "No. Wash Cycles";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(8, 141);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(59, 13);
            this.label12.TabIndex = 26;
            this.label12.Text = "Volume (ul)";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(8, 38);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(87, 13);
            this.label13.TabIndex = 27;
            this.label13.Text = "No. Prime Cycles";
            // 
            // numericUpDownWash
            // 
            this.numericUpDownWash.Location = new System.Drawing.Point(11, 203);
            this.numericUpDownWash.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownWash.Name = "numericUpDownWash";
            this.numericUpDownWash.Size = new System.Drawing.Size(78, 20);
            this.numericUpDownWash.TabIndex = 8;
            this.numericUpDownWash.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // numericUpDownAspirateVolume
            // 
            this.numericUpDownAspirateVolume.Location = new System.Drawing.Point(11, 157);
            this.numericUpDownAspirateVolume.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDownAspirateVolume.Name = "numericUpDownAspirateVolume";
            this.numericUpDownAspirateVolume.Size = new System.Drawing.Size(78, 20);
            this.numericUpDownAspirateVolume.TabIndex = 7;
            // 
            // numericUpDownPrimeNumber
            // 
            this.numericUpDownPrimeNumber.Location = new System.Drawing.Point(11, 55);
            this.numericUpDownPrimeNumber.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDownPrimeNumber.Name = "numericUpDownPrimeNumber";
            this.numericUpDownPrimeNumber.Size = new System.Drawing.Size(78, 20);
            this.numericUpDownPrimeNumber.TabIndex = 6;
            this.numericUpDownPrimeNumber.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // tabSpotSample
            // 
            this.tabSpotSample.BackColor = System.Drawing.SystemColors.Control;
            this.tabSpotSample.Controls.Add(this.groupBox4);
            this.tabSpotSample.Controls.Add(this.lblTipMovement);
            this.tabSpotSample.Controls.Add(this.checkBoxAutomaticPlunge);
            this.tabSpotSample.Controls.Add(this.checkBoxCryoMode);
            this.tabSpotSample.Controls.Add(this.label8);
            this.tabSpotSample.Controls.Add(this.buttonPositionGridToSafePoint);
            this.tabSpotSample.Controls.Add(this.buttonPositionGridToCamera);
            this.tabSpotSample.Controls.Add(this.label10);
            this.tabSpotSample.Controls.Add(this.buttonTipToCam);
            this.tabSpotSample.Controls.Add(this.buttonCenterTip);
            this.tabSpotSample.Controls.Add(this.numericUpDownSpotDropNumber);
            this.tabSpotSample.Controls.Add(this.checkBoxClickToSpot);
            this.tabSpotSample.Controls.Add(this.buttonAddQueueTargets);
            this.tabSpotSample.Location = new System.Drawing.Point(4, 22);
            this.tabSpotSample.Name = "tabSpotSample";
            this.tabSpotSample.Padding = new System.Windows.Forms.Padding(3);
            this.tabSpotSample.Size = new System.Drawing.Size(277, 350);
            this.tabSpotSample.TabIndex = 2;
            this.tabSpotSample.Text = "Cryo";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label14);
            this.groupBox4.Controls.Add(this.rbTimeResTip1and2);
            this.groupBox4.Controls.Add(this.rbTimeResTip2);
            this.groupBox4.Controls.Add(this.rbTimeResTip1);
            this.groupBox4.Controls.Add(this.label36);
            this.groupBox4.Controls.Add(this.label35);
            this.groupBox4.Controls.Add(this.numericUpDownOTFIDelayAfterTrip);
            this.groupBox4.Controls.Add(this.label34);
            this.groupBox4.Controls.Add(this.numericUpDownOTFISlowSpeed);
            this.groupBox4.Controls.Add(this.label33);
            this.groupBox4.Controls.Add(this.numericUpDownOTFIPercentWayToTarget);
            this.groupBox4.Controls.Add(this.cbOTFIEnableSlowDown);
            this.groupBox4.Controls.Add(this.cbOTFIUseFlashAcquisition);
            this.groupBox4.Controls.Add(this.numericUpDownPauseOTF);
            this.groupBox4.Controls.Add(this.label15);
            this.groupBox4.Controls.Add(this.cbOTF_Enable);
            this.groupBox4.Location = new System.Drawing.Point(10, 129);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(259, 164);
            this.groupBox4.TabIndex = 18;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "On-The-Fly (OTF)";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(3, 117);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(124, 13);
            this.label14.TabIndex = 19;
            this.label14.Text = "Time Resolve Print With:";
            // 
            // rbTimeResTip1and2
            // 
            this.rbTimeResTip1and2.AutoSize = true;
            this.rbTimeResTip1and2.Location = new System.Drawing.Point(132, 135);
            this.rbTimeResTip1and2.Name = "rbTimeResTip1and2";
            this.rbTimeResTip1and2.Size = new System.Drawing.Size(91, 17);
            this.rbTimeResTip1and2.TabIndex = 30;
            this.rbTimeResTip1and2.Text = "Tip1 and Tip2";
            this.rbTimeResTip1and2.UseVisualStyleBackColor = true;
            // 
            // rbTimeResTip2
            // 
            this.rbTimeResTip2.AutoSize = true;
            this.rbTimeResTip2.Location = new System.Drawing.Point(71, 135);
            this.rbTimeResTip2.Name = "rbTimeResTip2";
            this.rbTimeResTip2.Size = new System.Drawing.Size(46, 17);
            this.rbTimeResTip2.TabIndex = 29;
            this.rbTimeResTip2.Text = "Tip2";
            this.rbTimeResTip2.UseVisualStyleBackColor = true;
            // 
            // rbTimeResTip1
            // 
            this.rbTimeResTip1.AutoSize = true;
            this.rbTimeResTip1.Checked = true;
            this.rbTimeResTip1.Location = new System.Drawing.Point(10, 135);
            this.rbTimeResTip1.Name = "rbTimeResTip1";
            this.rbTimeResTip1.Size = new System.Drawing.Size(46, 17);
            this.rbTimeResTip1.TabIndex = 28;
            this.rbTimeResTip1.TabStop = true;
            this.rbTimeResTip1.Text = "Tip1";
            this.rbTimeResTip1.UseVisualStyleBackColor = true;
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(199, 57);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(45, 13);
            this.label36.TabIndex = 27;
            this.label36.Text = "after trip";
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(199, 42);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(58, 13);
            this.label35.TabIndex = 26;
            this.label35.Text = "usec delay";
            // 
            // numericUpDownOTFIDelayAfterTrip
            // 
            this.numericUpDownOTFIDelayAfterTrip.Location = new System.Drawing.Point(139, 48);
            this.numericUpDownOTFIDelayAfterTrip.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownOTFIDelayAfterTrip.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownOTFIDelayAfterTrip.Name = "numericUpDownOTFIDelayAfterTrip";
            this.numericUpDownOTFIDelayAfterTrip.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownOTFIDelayAfterTrip.TabIndex = 25;
            this.numericUpDownOTFIDelayAfterTrip.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(202, 96);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(48, 13);
            this.label34.TabIndex = 24;
            this.label34.Text = "mm /sec";
            // 
            // numericUpDownOTFISlowSpeed
            // 
            this.numericUpDownOTFISlowSpeed.Location = new System.Drawing.Point(144, 94);
            this.numericUpDownOTFISlowSpeed.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownOTFISlowSpeed.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownOTFISlowSpeed.Name = "numericUpDownOTFISlowSpeed";
            this.numericUpDownOTFISlowSpeed.Size = new System.Drawing.Size(52, 20);
            this.numericUpDownOTFISlowSpeed.TabIndex = 23;
            this.numericUpDownOTFISlowSpeed.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(57, 96);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(72, 13);
            this.label33.TabIndex = 19;
            this.label33.Text = "% way to cam";
            // 
            // numericUpDownOTFIPercentWayToTarget
            // 
            this.numericUpDownOTFIPercentWayToTarget.Location = new System.Drawing.Point(18, 94);
            this.numericUpDownOTFIPercentWayToTarget.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numericUpDownOTFIPercentWayToTarget.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownOTFIPercentWayToTarget.Name = "numericUpDownOTFIPercentWayToTarget";
            this.numericUpDownOTFIPercentWayToTarget.Size = new System.Drawing.Size(37, 20);
            this.numericUpDownOTFIPercentWayToTarget.TabIndex = 22;
            this.numericUpDownOTFIPercentWayToTarget.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // cbOTFIEnableSlowDown
            // 
            this.cbOTFIEnableSlowDown.AutoSize = true;
            this.cbOTFIEnableSlowDown.Enabled = false;
            this.cbOTFIEnableSlowDown.Location = new System.Drawing.Point(9, 72);
            this.cbOTFIEnableSlowDown.Name = "cbOTFIEnableSlowDown";
            this.cbOTFIEnableSlowDown.Size = new System.Drawing.Size(192, 17);
            this.cbOTFIEnableSlowDown.TabIndex = 21;
            this.cbOTFIEnableSlowDown.Text = "Slow down as passing grid camera:";
            this.cbOTFIEnableSlowDown.UseVisualStyleBackColor = true;
            // 
            // cbOTFIUseFlashAcquisition
            // 
            this.cbOTFIUseFlashAcquisition.AutoSize = true;
            this.cbOTFIUseFlashAcquisition.Checked = true;
            this.cbOTFIUseFlashAcquisition.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbOTFIUseFlashAcquisition.Location = new System.Drawing.Point(9, 49);
            this.cbOTFIUseFlashAcquisition.Name = "cbOTFIUseFlashAcquisition";
            this.cbOTFIUseFlashAcquisition.Size = new System.Drawing.Size(133, 17);
            this.cbOTFIUseFlashAcquisition.TabIndex = 20;
            this.cbOTFIUseFlashAcquisition.Text = "Triggered / Flash Insp,";
            this.cbOTFIUseFlashAcquisition.UseVisualStyleBackColor = true;
            // 
            // numericUpDownPauseOTF
            // 
            this.numericUpDownPauseOTF.Location = new System.Drawing.Point(139, 22);
            this.numericUpDownPauseOTF.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownPauseOTF.Name = "numericUpDownPauseOTF";
            this.numericUpDownPauseOTF.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownPauseOTF.TabIndex = 19;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(123, 8);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(134, 13);
            this.label15.TabIndex = 17;
            this.label15.Text = "Insp Pause B4 Plunge (ms)";
            // 
            // cbOTF_Enable
            // 
            this.cbOTF_Enable.AutoSize = true;
            this.cbOTF_Enable.Checked = true;
            this.cbOTF_Enable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbOTF_Enable.Location = new System.Drawing.Point(9, 24);
            this.cbOTF_Enable.Name = "cbOTF_Enable";
            this.cbOTF_Enable.Size = new System.Drawing.Size(83, 17);
            this.cbOTF_Enable.TabIndex = 14;
            this.cbOTF_Enable.Text = "Enable OTF";
            this.cbOTF_Enable.UseVisualStyleBackColor = true;
            // 
            // lblTipMovement
            // 
            this.lblTipMovement.AutoSize = true;
            this.lblTipMovement.Location = new System.Drawing.Point(10, 299);
            this.lblTipMovement.Name = "lblTipMovement";
            this.lblTipMovement.Size = new System.Drawing.Size(83, 13);
            this.lblTipMovement.TabIndex = 16;
            this.lblTipMovement.Text = "Tip Movements:";
            // 
            // checkBoxAutomaticPlunge
            // 
            this.checkBoxAutomaticPlunge.AutoSize = true;
            this.checkBoxAutomaticPlunge.Location = new System.Drawing.Point(121, 85);
            this.checkBoxAutomaticPlunge.Name = "checkBoxAutomaticPlunge";
            this.checkBoxAutomaticPlunge.Size = new System.Drawing.Size(109, 17);
            this.checkBoxAutomaticPlunge.TabIndex = 11;
            this.checkBoxAutomaticPlunge.Text = "Automatic Plunge";
            this.checkBoxAutomaticPlunge.UseVisualStyleBackColor = true;
            this.checkBoxAutomaticPlunge.CheckedChanged += new System.EventHandler(this.checkBoxAutomaticPlunge_CheckedChanged);
            // 
            // checkBoxCryoMode
            // 
            this.checkBoxCryoMode.AutoSize = true;
            this.checkBoxCryoMode.Checked = true;
            this.checkBoxCryoMode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCryoMode.Location = new System.Drawing.Point(189, 6);
            this.checkBoxCryoMode.Name = "checkBoxCryoMode";
            this.checkBoxCryoMode.Size = new System.Drawing.Size(83, 17);
            this.checkBoxCryoMode.TabIndex = 9;
            this.checkBoxCryoMode.Text = "Cryo Enable";
            this.checkBoxCryoMode.UseVisualStyleBackColor = true;
            this.checkBoxCryoMode.CheckedChanged += new System.EventHandler(this.checkBoxCryoMode_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 7);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(87, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "Grid Movements:";
            // 
            // buttonPositionGridToSafePoint
            // 
            this.buttonPositionGridToSafePoint.Location = new System.Drawing.Point(142, 26);
            this.buttonPositionGridToSafePoint.Name = "buttonPositionGridToSafePoint";
            this.buttonPositionGridToSafePoint.Size = new System.Drawing.Size(94, 26);
            this.buttonPositionGridToSafePoint.TabIndex = 7;
            this.buttonPositionGridToSafePoint.Text = "Away";
            this.buttonPositionGridToSafePoint.UseVisualStyleBackColor = true;
            this.buttonPositionGridToSafePoint.Click += new System.EventHandler(this.buttonPositionGridToSafePoint_Click);
            // 
            // buttonPositionGridToCamera
            // 
            this.buttonPositionGridToCamera.Location = new System.Drawing.Point(9, 26);
            this.buttonPositionGridToCamera.Name = "buttonPositionGridToCamera";
            this.buttonPositionGridToCamera.Size = new System.Drawing.Size(111, 26);
            this.buttonPositionGridToCamera.TabIndex = 6;
            this.buttonPositionGridToCamera.Text = "To Camera";
            this.buttonPositionGridToCamera.UseVisualStyleBackColor = true;
            this.buttonPositionGridToCamera.Click += new System.EventHandler(this.buttonPositionGrid_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(9, 55);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(87, 13);
            this.label10.TabIndex = 5;
            this.label10.Text = "Number of Drops";
            // 
            // buttonTipToCam
            // 
            this.buttonTipToCam.Location = new System.Drawing.Point(39, 318);
            this.buttonTipToCam.Name = "buttonTipToCam";
            this.buttonTipToCam.Size = new System.Drawing.Size(88, 26);
            this.buttonTipToCam.TabIndex = 4;
            this.buttonTipToCam.Text = "Tip To Camera";
            this.buttonTipToCam.UseVisualStyleBackColor = true;
            this.buttonTipToCam.Click += new System.EventHandler(this.btn_clickGotoBackCamera);
            // 
            // buttonCenterTip
            // 
            this.buttonCenterTip.Location = new System.Drawing.Point(154, 318);
            this.buttonCenterTip.Name = "buttonCenterTip";
            this.buttonCenterTip.Size = new System.Drawing.Size(77, 26);
            this.buttonCenterTip.TabIndex = 3;
            this.buttonCenterTip.Text = "Center Tip";
            this.buttonCenterTip.UseVisualStyleBackColor = true;
            this.buttonCenterTip.Click += new System.EventHandler(this.btnCenterTip_Click);
            // 
            // numericUpDownSpotDropNumber
            // 
            this.numericUpDownSpotDropNumber.Location = new System.Drawing.Point(13, 73);
            this.numericUpDownSpotDropNumber.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownSpotDropNumber.Name = "numericUpDownSpotDropNumber";
            this.numericUpDownSpotDropNumber.Size = new System.Drawing.Size(78, 20);
            this.numericUpDownSpotDropNumber.TabIndex = 1;
            this.numericUpDownSpotDropNumber.ValueChanged += new System.EventHandler(this.numericUpDownSpotDropNumber_Changed);
            // 
            // checkBoxClickToSpot
            // 
            this.checkBoxClickToSpot.AutoSize = true;
            this.checkBoxClickToSpot.Location = new System.Drawing.Point(121, 59);
            this.checkBoxClickToSpot.Name = "checkBoxClickToSpot";
            this.checkBoxClickToSpot.Size = new System.Drawing.Size(108, 17);
            this.checkBoxClickToSpot.TabIndex = 0;
            this.checkBoxClickToSpot.Text = "Spot All in Queue";
            this.checkBoxClickToSpot.UseVisualStyleBackColor = true;
            this.checkBoxClickToSpot.Click += new System.EventHandler(this.checkBoxClickToSpot_Click);
            // 
            // buttonAddQueueTargets
            // 
            this.buttonAddQueueTargets.Location = new System.Drawing.Point(9, 98);
            this.buttonAddQueueTargets.Name = "buttonAddQueueTargets";
            this.buttonAddQueueTargets.Size = new System.Drawing.Size(90, 23);
            this.buttonAddQueueTargets.TabIndex = 10;
            this.buttonAddQueueTargets.Text = "Queue Target";
            this.buttonAddQueueTargets.Click += new System.EventHandler(this.addQueueTargets_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage4.Controls.Add(this.btnCamLoadSettingsFromFile);
            this.tabPage4.Controls.Add(this.btnCamSaveSettingsToFile);
            this.tabPage4.Controls.Add(this.groupBox11);
            this.tabPage4.Controls.Add(this.labelMaxExposure);
            this.tabPage4.Controls.Add(this.labelMaxFPS);
            this.tabPage4.Controls.Add(this.labelMinExposure);
            this.tabPage4.Controls.Add(this.labelCurrentExposure);
            this.tabPage4.Controls.Add(this.labelPixelClockMin);
            this.tabPage4.Controls.Add(this.labelPixelClockMax);
            this.tabPage4.Controls.Add(this.trackBarExposure);
            this.tabPage4.Controls.Add(this.labelMinFPS);
            this.tabPage4.Controls.Add(this.labelCurrentFPS);
            this.tabPage4.Controls.Add(this.labelCurrentPixelClock);
            this.tabPage4.Controls.Add(this.trackBarPixelClock);
            this.tabPage4.Controls.Add(this.trackBarFPS);
            this.tabPage4.Controls.Add(this.label2);
            this.tabPage4.Controls.Add(this.label3);
            this.tabPage4.Controls.Add(this.label4);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(277, 350);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Camera";
            // 
            // btnCamLoadSettingsFromFile
            // 
            this.btnCamLoadSettingsFromFile.Location = new System.Drawing.Point(186, 294);
            this.btnCamLoadSettingsFromFile.Name = "btnCamLoadSettingsFromFile";
            this.btnCamLoadSettingsFromFile.Size = new System.Drawing.Size(85, 22);
            this.btnCamLoadSettingsFromFile.TabIndex = 46;
            this.btnCamLoadSettingsFromFile.Text = "Load Settings";
            this.btnCamLoadSettingsFromFile.UseVisualStyleBackColor = true;
            this.btnCamLoadSettingsFromFile.Click += new System.EventHandler(this.btnCamLoadSettingsFromFile_Click);
            // 
            // btnCamSaveSettingsToFile
            // 
            this.btnCamSaveSettingsToFile.Location = new System.Drawing.Point(186, 266);
            this.btnCamSaveSettingsToFile.Name = "btnCamSaveSettingsToFile";
            this.btnCamSaveSettingsToFile.Size = new System.Drawing.Size(85, 22);
            this.btnCamSaveSettingsToFile.TabIndex = 45;
            this.btnCamSaveSettingsToFile.Text = "Save Settings";
            this.btnCamSaveSettingsToFile.UseVisualStyleBackColor = true;
            this.btnCamSaveSettingsToFile.Click += new System.EventHandler(this.btnCamSaveSettingsToFile_Click);
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.btnCamSetGain);
            this.groupBox11.Controls.Add(this.btnCamReadGain);
            this.groupBox11.Controls.Add(this.numericUpDownCamGain);
            this.groupBox11.Location = new System.Drawing.Point(8, 266);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(170, 52);
            this.groupBox11.TabIndex = 33;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "Gain";
            // 
            // btnCamSetGain
            // 
            this.btnCamSetGain.Location = new System.Drawing.Point(117, 17);
            this.btnCamSetGain.Name = "btnCamSetGain";
            this.btnCamSetGain.Size = new System.Drawing.Size(45, 22);
            this.btnCamSetGain.TabIndex = 44;
            this.btnCamSetGain.Text = "Set";
            this.btnCamSetGain.UseVisualStyleBackColor = true;
            this.btnCamSetGain.Click += new System.EventHandler(this.btnCamSetGain_Click);
            // 
            // btnCamReadGain
            // 
            this.btnCamReadGain.Location = new System.Drawing.Point(66, 17);
            this.btnCamReadGain.Name = "btnCamReadGain";
            this.btnCamReadGain.Size = new System.Drawing.Size(45, 22);
            this.btnCamReadGain.TabIndex = 43;
            this.btnCamReadGain.Text = "Read";
            this.btnCamReadGain.UseVisualStyleBackColor = true;
            this.btnCamReadGain.Click += new System.EventHandler(this.btnCamReadGain_Click);
            // 
            // numericUpDownCamGain
            // 
            this.numericUpDownCamGain.Location = new System.Drawing.Point(9, 19);
            this.numericUpDownCamGain.Name = "numericUpDownCamGain";
            this.numericUpDownCamGain.Size = new System.Drawing.Size(50, 20);
            this.numericUpDownCamGain.TabIndex = 37;
            this.numericUpDownCamGain.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // labelMaxExposure
            // 
            this.labelMaxExposure.AutoSize = true;
            this.labelMaxExposure.Location = new System.Drawing.Point(231, 215);
            this.labelMaxExposure.Name = "labelMaxExposure";
            this.labelMaxExposure.Size = new System.Drawing.Size(13, 13);
            this.labelMaxExposure.TabIndex = 32;
            this.labelMaxExposure.Text = "0";
            // 
            // labelMaxFPS
            // 
            this.labelMaxFPS.AutoSize = true;
            this.labelMaxFPS.Location = new System.Drawing.Point(231, 119);
            this.labelMaxFPS.Name = "labelMaxFPS";
            this.labelMaxFPS.Size = new System.Drawing.Size(13, 13);
            this.labelMaxFPS.TabIndex = 29;
            this.labelMaxFPS.Text = "0";
            // 
            // labelMinExposure
            // 
            this.labelMinExposure.AutoSize = true;
            this.labelMinExposure.Location = new System.Drawing.Point(2, 215);
            this.labelMinExposure.Name = "labelMinExposure";
            this.labelMinExposure.Size = new System.Drawing.Size(13, 13);
            this.labelMinExposure.TabIndex = 31;
            this.labelMinExposure.Text = "0";
            // 
            // labelCurrentExposure
            // 
            this.labelCurrentExposure.AutoSize = true;
            this.labelCurrentExposure.Location = new System.Drawing.Point(117, 238);
            this.labelCurrentExposure.Name = "labelCurrentExposure";
            this.labelCurrentExposure.Size = new System.Drawing.Size(35, 13);
            this.labelCurrentExposure.TabIndex = 26;
            this.labelCurrentExposure.Text = "label6";
            // 
            // labelPixelClockMin
            // 
            this.labelPixelClockMin.AutoSize = true;
            this.labelPixelClockMin.Location = new System.Drawing.Point(2, 40);
            this.labelPixelClockMin.Name = "labelPixelClockMin";
            this.labelPixelClockMin.Size = new System.Drawing.Size(13, 13);
            this.labelPixelClockMin.TabIndex = 30;
            this.labelPixelClockMin.Text = "0";
            // 
            // labelPixelClockMax
            // 
            this.labelPixelClockMax.AutoSize = true;
            this.labelPixelClockMax.Location = new System.Drawing.Point(231, 40);
            this.labelPixelClockMax.Name = "labelPixelClockMax";
            this.labelPixelClockMax.Size = new System.Drawing.Size(13, 13);
            this.labelPixelClockMax.TabIndex = 27;
            this.labelPixelClockMax.Text = "0";
            // 
            // trackBarExposure
            // 
            this.trackBarExposure.Location = new System.Drawing.Point(28, 199);
            this.trackBarExposure.Name = "trackBarExposure";
            this.trackBarExposure.Size = new System.Drawing.Size(210, 45);
            this.trackBarExposure.TabIndex = 23;
            // 
            // labelMinFPS
            // 
            this.labelMinFPS.AutoSize = true;
            this.labelMinFPS.Location = new System.Drawing.Point(2, 119);
            this.labelMinFPS.Name = "labelMinFPS";
            this.labelMinFPS.Size = new System.Drawing.Size(13, 13);
            this.labelMinFPS.TabIndex = 21;
            this.labelMinFPS.Text = "0";
            // 
            // labelCurrentFPS
            // 
            this.labelCurrentFPS.AutoSize = true;
            this.labelCurrentFPS.Location = new System.Drawing.Point(117, 147);
            this.labelCurrentFPS.Name = "labelCurrentFPS";
            this.labelCurrentFPS.Size = new System.Drawing.Size(35, 13);
            this.labelCurrentFPS.TabIndex = 25;
            this.labelCurrentFPS.Text = "label5";
            // 
            // labelCurrentPixelClock
            // 
            this.labelCurrentPixelClock.AutoSize = true;
            this.labelCurrentPixelClock.Location = new System.Drawing.Point(117, 63);
            this.labelCurrentPixelClock.Name = "labelCurrentPixelClock";
            this.labelCurrentPixelClock.Size = new System.Drawing.Size(35, 13);
            this.labelCurrentPixelClock.TabIndex = 28;
            this.labelCurrentPixelClock.Text = "label8";
            // 
            // trackBarPixelClock
            // 
            this.trackBarPixelClock.Location = new System.Drawing.Point(28, 24);
            this.trackBarPixelClock.Maximum = 100;
            this.trackBarPixelClock.Name = "trackBarPixelClock";
            this.trackBarPixelClock.Size = new System.Drawing.Size(210, 45);
            this.trackBarPixelClock.TabIndex = 21;
            // 
            // trackBarFPS
            // 
            this.trackBarFPS.Location = new System.Drawing.Point(28, 103);
            this.trackBarFPS.Name = "trackBarFPS";
            this.trackBarFPS.Size = new System.Drawing.Size(210, 45);
            this.trackBarFPS.TabIndex = 22;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(88, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 22;
            this.label2.Text = "Pixel Clock (MHz)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(81, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 13);
            this.label3.TabIndex = 23;
            this.label3.Text = "Frames per second";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(97, 180);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 13);
            this.label4.TabIndex = 24;
            this.label4.Text = "Exposure (ms)";
            // 
            // tabStain
            // 
            this.tabStain.BackColor = System.Drawing.SystemColors.Control;
            this.tabStain.Controls.Add(this.groupBox7);
            this.tabStain.Controls.Add(this.groupBox6);
            this.tabStain.Controls.Add(this.groupBox5);
            this.tabStain.Controls.Add(this.label22);
            this.tabStain.Controls.Add(this.numericUpDownSTNDelay2_ms);
            this.tabStain.Controls.Add(this.btnSTNRunStainQueue);
            this.tabStain.Controls.Add(this.btnSTNClearQueue);
            this.tabStain.Controls.Add(this.checkBoxExperimentMode);
            this.tabStain.Controls.Add(this.numericUpDownSTNDelay_ms);
            this.tabStain.Controls.Add(this.label20);
            this.tabStain.Controls.Add(this.numericUpDownSTNTip3Drops);
            this.tabStain.Controls.Add(this.numericUpDownSTNTip2Drops);
            this.tabStain.Controls.Add(this.label19);
            this.tabStain.Controls.Add(this.numericUpDownSTNTip1Drops);
            this.tabStain.Controls.Add(this.buttonSTNAddQueueTargets);
            this.tabStain.Location = new System.Drawing.Point(4, 22);
            this.tabStain.Name = "tabStain";
            this.tabStain.Size = new System.Drawing.Size(277, 350);
            this.tabStain.TabIndex = 4;
            this.tabStain.Text = "Stain";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.comboBoxSTNMode);
            this.groupBox7.Location = new System.Drawing.Point(115, 75);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(154, 52);
            this.groupBox7.TabIndex = 36;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Stain Mode";
            // 
            // comboBoxSTNMode
            // 
            this.comboBoxSTNMode.FormattingEnabled = true;
            this.comboBoxSTNMode.Items.AddRange(new object[] {
            "2 Tip, Mode 2.1",
            "3 Tip, Mode 3.1",
            "3 Tip, Mode 3.2",
            "3 Tip, Mode Future"});
            this.comboBoxSTNMode.Location = new System.Drawing.Point(14, 19);
            this.comboBoxSTNMode.Name = "comboBoxSTNMode";
            this.comboBoxSTNMode.Size = new System.Drawing.Size(134, 21);
            this.comboBoxSTNMode.TabIndex = 37;
            this.comboBoxSTNMode.Text = "2 Tip, Mode 2.1";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.buttonSTNTipToCam);
            this.groupBox6.Controls.Add(this.buttonSTNCenterTip);
            this.groupBox6.Location = new System.Drawing.Point(12, 269);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(192, 52);
            this.groupBox6.TabIndex = 35;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Tip Movements";
            // 
            // buttonSTNTipToCam
            // 
            this.buttonSTNTipToCam.Location = new System.Drawing.Point(8, 17);
            this.buttonSTNTipToCam.Name = "buttonSTNTipToCam";
            this.buttonSTNTipToCam.Size = new System.Drawing.Size(85, 26);
            this.buttonSTNTipToCam.TabIndex = 10;
            this.buttonSTNTipToCam.Text = "To Camera";
            this.buttonSTNTipToCam.UseVisualStyleBackColor = true;
            this.buttonSTNTipToCam.Click += new System.EventHandler(this.btn_clickGotoBackCamera);
            // 
            // buttonSTNCenterTip
            // 
            this.buttonSTNCenterTip.Location = new System.Drawing.Point(103, 17);
            this.buttonSTNCenterTip.Name = "buttonSTNCenterTip";
            this.buttonSTNCenterTip.Size = new System.Drawing.Size(77, 26);
            this.buttonSTNCenterTip.TabIndex = 9;
            this.buttonSTNCenterTip.Text = "Center Tip";
            this.buttonSTNCenterTip.UseVisualStyleBackColor = true;
            this.buttonSTNCenterTip.Click += new System.EventHandler(this.btnCenterTip_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.buttonSTNPositionGridToSafePoint);
            this.groupBox5.Controls.Add(this.buttonSTNPositionGridToCamera);
            this.groupBox5.Location = new System.Drawing.Point(8, 8);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(184, 56);
            this.groupBox5.TabIndex = 34;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Grid Movements";
            // 
            // buttonSTNPositionGridToSafePoint
            // 
            this.buttonSTNPositionGridToSafePoint.Location = new System.Drawing.Point(104, 18);
            this.buttonSTNPositionGridToSafePoint.Name = "buttonSTNPositionGridToSafePoint";
            this.buttonSTNPositionGridToSafePoint.Size = new System.Drawing.Size(68, 26);
            this.buttonSTNPositionGridToSafePoint.TabIndex = 12;
            this.buttonSTNPositionGridToSafePoint.Text = "Away";
            this.buttonSTNPositionGridToSafePoint.UseVisualStyleBackColor = true;
            this.buttonSTNPositionGridToSafePoint.Click += new System.EventHandler(this.buttonPositionGridToSafePoint_Click);
            // 
            // buttonSTNPositionGridToCamera
            // 
            this.buttonSTNPositionGridToCamera.Location = new System.Drawing.Point(10, 18);
            this.buttonSTNPositionGridToCamera.Name = "buttonSTNPositionGridToCamera";
            this.buttonSTNPositionGridToCamera.Size = new System.Drawing.Size(75, 26);
            this.buttonSTNPositionGridToCamera.TabIndex = 11;
            this.buttonSTNPositionGridToCamera.Text = "To Camera";
            this.buttonSTNPositionGridToCamera.UseVisualStyleBackColor = true;
            this.buttonSTNPositionGridToCamera.Click += new System.EventHandler(this.buttonPositionGrid_Click);
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(93, 219);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(62, 13);
            this.label22.TabIndex = 31;
            this.label22.Text = "Delay2 (ms)";
            // 
            // numericUpDownSTNDelay2_ms
            // 
            this.numericUpDownSTNDelay2_ms.Location = new System.Drawing.Point(97, 236);
            this.numericUpDownSTNDelay2_ms.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownSTNDelay2_ms.Name = "numericUpDownSTNDelay2_ms";
            this.numericUpDownSTNDelay2_ms.Size = new System.Drawing.Size(59, 20);
            this.numericUpDownSTNDelay2_ms.TabIndex = 30;
            this.numericUpDownSTNDelay2_ms.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnSTNRunStainQueue
            // 
            this.btnSTNRunStainQueue.Location = new System.Drawing.Point(10, 133);
            this.btnSTNRunStainQueue.Name = "btnSTNRunStainQueue";
            this.btnSTNRunStainQueue.Size = new System.Drawing.Size(95, 23);
            this.btnSTNRunStainQueue.TabIndex = 29;
            this.btnSTNRunStainQueue.Text = "Run Queue";
            this.btnSTNRunStainQueue.Click += new System.EventHandler(this.btnSTNRunStainQueue_Click);
            // 
            // btnSTNClearQueue
            // 
            this.btnSTNClearQueue.Location = new System.Drawing.Point(10, 104);
            this.btnSTNClearQueue.Name = "btnSTNClearQueue";
            this.btnSTNClearQueue.Size = new System.Drawing.Size(95, 23);
            this.btnSTNClearQueue.TabIndex = 28;
            this.btnSTNClearQueue.Text = "Clear Queue";
            this.btnSTNClearQueue.Click += new System.EventHandler(this.btnSTNClearQueue_Click);
            // 
            // checkBoxExperimentMode
            // 
            this.checkBoxExperimentMode.AutoSize = true;
            this.checkBoxExperimentMode.Location = new System.Drawing.Point(194, 3);
            this.checkBoxExperimentMode.Name = "checkBoxExperimentMode";
            this.checkBoxExperimentMode.Size = new System.Drawing.Size(86, 17);
            this.checkBoxExperimentMode.TabIndex = 27;
            this.checkBoxExperimentMode.Text = "Stain Enable";
            this.checkBoxExperimentMode.UseVisualStyleBackColor = true;
            this.checkBoxExperimentMode.CheckedChanged += new System.EventHandler(this.checkBoxExperimentMode_CheckedChanged);
            // 
            // numericUpDownSTNDelay_ms
            // 
            this.numericUpDownSTNDelay_ms.Location = new System.Drawing.Point(21, 236);
            this.numericUpDownSTNDelay_ms.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownSTNDelay_ms.Name = "numericUpDownSTNDelay_ms";
            this.numericUpDownSTNDelay_ms.Size = new System.Drawing.Size(59, 20);
            this.numericUpDownSTNDelay_ms.TabIndex = 25;
            this.numericUpDownSTNDelay_ms.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(17, 219);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(62, 13);
            this.label20.TabIndex = 24;
            this.label20.Text = "Delay1 (ms)";
            // 
            // numericUpDownSTNTip3Drops
            // 
            this.numericUpDownSTNTip3Drops.Location = new System.Drawing.Point(175, 190);
            this.numericUpDownSTNTip3Drops.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownSTNTip3Drops.Name = "numericUpDownSTNTip3Drops";
            this.numericUpDownSTNTip3Drops.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownSTNTip3Drops.TabIndex = 23;
            this.numericUpDownSTNTip3Drops.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // numericUpDownSTNTip2Drops
            // 
            this.numericUpDownSTNTip2Drops.Location = new System.Drawing.Point(100, 190);
            this.numericUpDownSTNTip2Drops.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownSTNTip2Drops.Name = "numericUpDownSTNTip2Drops";
            this.numericUpDownSTNTip2Drops.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownSTNTip2Drops.TabIndex = 22;
            this.numericUpDownSTNTip2Drops.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(15, 174);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(165, 13);
            this.label19.TabIndex = 21;
            this.label19.Text = "Number of Drops (Tip1,Tip2,Tip3)";
            // 
            // numericUpDownSTNTip1Drops
            // 
            this.numericUpDownSTNTip1Drops.Location = new System.Drawing.Point(19, 190);
            this.numericUpDownSTNTip1Drops.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDownSTNTip1Drops.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownSTNTip1Drops.Name = "numericUpDownSTNTip1Drops";
            this.numericUpDownSTNTip1Drops.Size = new System.Drawing.Size(59, 20);
            this.numericUpDownSTNTip1Drops.TabIndex = 20;
            this.numericUpDownSTNTip1Drops.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // buttonSTNAddQueueTargets
            // 
            this.buttonSTNAddQueueTargets.Location = new System.Drawing.Point(10, 75);
            this.buttonSTNAddQueueTargets.Name = "buttonSTNAddQueueTargets";
            this.buttonSTNAddQueueTargets.Size = new System.Drawing.Size(95, 23);
            this.buttonSTNAddQueueTargets.TabIndex = 19;
            this.buttonSTNAddQueueTargets.Text = "Queue Target";
            this.buttonSTNAddQueueTargets.Click += new System.EventHandler(this.addQueueTargets_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox12);
            this.tabPage3.Controls.Add(this.groupBox9);
            this.tabPage3.Controls.Add(this.btnTestStopLive);
            this.tabPage3.Controls.Add(this.btnTestStartLive);
            this.tabPage3.Controls.Add(this.btnTestDoubleTriggeredAcquision);
            this.tabPage3.Controls.Add(this.btnTest123);
            this.tabPage3.Controls.Add(this.btnSaveBitmap);
            this.tabPage3.Controls.Add(this.cbDebugTrySavingBitmap);
            this.tabPage3.Controls.Add(this.cbDebugAutoStartVideo);
            this.tabPage3.Controls.Add(this.cbDebugDisableGridDrop);
            this.tabPage3.Controls.Add(this.buttonStain);
            this.tabPage3.Controls.Add(this.label9);
            this.tabPage3.Controls.Add(this.numericUpDownStainTime);
            this.tabPage3.Controls.Add(this.groupBox10);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(277, 350);
            this.tabPage3.TabIndex = 5;
            this.tabPage3.Text = "Debug";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox12
            // 
            this.groupBox12.Controls.Add(this.btn3StackLoadDefaults);
            this.groupBox12.Controls.Add(this.label24);
            this.groupBox12.Controls.Add(this.numericUpDown3StackDecel);
            this.groupBox12.Controls.Add(this.numericUpDown3StackAccel);
            this.groupBox12.Controls.Add(this.numericUpDown3StackVel);
            this.groupBox12.Controls.Add(this.btn3StackTestPlunge);
            this.groupBox12.Location = new System.Drawing.Point(7, 91);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Size = new System.Drawing.Size(261, 74);
            this.groupBox12.TabIndex = 50;
            this.groupBox12.TabStop = false;
            this.groupBox12.Text = "Test Plunge";
            // 
            // btn3StackLoadDefaults
            // 
            this.btn3StackLoadDefaults.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn3StackLoadDefaults.Location = new System.Drawing.Point(192, 14);
            this.btn3StackLoadDefaults.Name = "btn3StackLoadDefaults";
            this.btn3StackLoadDefaults.Size = new System.Drawing.Size(64, 19);
            this.btn3StackLoadDefaults.TabIndex = 51;
            this.btn3StackLoadDefaults.Text = "Load Defaults";
            this.btn3StackLoadDefaults.UseVisualStyleBackColor = true;
            this.btn3StackLoadDefaults.Click += new System.EventHandler(this.btn3StackLoadDefaults_Click);
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(6, 42);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(161, 13);
            this.label24.TabIndex = 50;
            this.label24.Text = "vel, acc, decel  (mm/s  .. mm/ss)";
            // 
            // numericUpDown3StackDecel
            // 
            this.numericUpDown3StackDecel.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown3StackDecel.Location = new System.Drawing.Point(124, 16);
            this.numericUpDown3StackDecel.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
            this.numericUpDown3StackDecel.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown3StackDecel.Name = "numericUpDown3StackDecel";
            this.numericUpDown3StackDecel.Size = new System.Drawing.Size(58, 20);
            this.numericUpDown3StackDecel.TabIndex = 3;
            this.numericUpDown3StackDecel.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            // 
            // numericUpDown3StackAccel
            // 
            this.numericUpDown3StackAccel.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown3StackAccel.Location = new System.Drawing.Point(60, 16);
            this.numericUpDown3StackAccel.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
            this.numericUpDown3StackAccel.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown3StackAccel.Name = "numericUpDown3StackAccel";
            this.numericUpDown3StackAccel.Size = new System.Drawing.Size(58, 20);
            this.numericUpDown3StackAccel.TabIndex = 2;
            this.numericUpDown3StackAccel.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            // 
            // numericUpDown3StackVel
            // 
            this.numericUpDown3StackVel.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDown3StackVel.Location = new System.Drawing.Point(5, 16);
            this.numericUpDown3StackVel.Maximum = new decimal(new int[] {
            3000,
            0,
            0,
            0});
            this.numericUpDown3StackVel.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.numericUpDown3StackVel.Name = "numericUpDown3StackVel";
            this.numericUpDown3StackVel.Size = new System.Drawing.Size(47, 20);
            this.numericUpDown3StackVel.TabIndex = 1;
            this.numericUpDown3StackVel.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // btn3StackTestPlunge
            // 
            this.btn3StackTestPlunge.Location = new System.Drawing.Point(200, 35);
            this.btn3StackTestPlunge.Name = "btn3StackTestPlunge";
            this.btn3StackTestPlunge.Size = new System.Drawing.Size(52, 36);
            this.btn3StackTestPlunge.TabIndex = 0;
            this.btn3StackTestPlunge.Text = "Test Plunge";
            this.btn3StackTestPlunge.UseVisualStyleBackColor = true;
            this.btn3StackTestPlunge.Click += new System.EventHandler(this.btn3StackTestPlunge_Click);
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.btnDGCTest);
            this.groupBox9.Controls.Add(this.lblDGCRemotePath);
            this.groupBox9.Controls.Add(this.cbDGCPromptAndSave);
            this.groupBox9.Controls.Add(this.cbDGCEnabled);
            this.groupBox9.Location = new System.Drawing.Point(3, 6);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(266, 83);
            this.groupBox9.TabIndex = 49;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Dual Grid Camera";
            // 
            // btnDGCTest
            // 
            this.btnDGCTest.Location = new System.Drawing.Point(224, 39);
            this.btnDGCTest.Name = "btnDGCTest";
            this.btnDGCTest.Size = new System.Drawing.Size(36, 21);
            this.btnDGCTest.TabIndex = 49;
            this.btnDGCTest.Text = "Test";
            this.btnDGCTest.UseVisualStyleBackColor = true;
            this.btnDGCTest.Click += new System.EventHandler(this.btnDGCTest_Click);
            // 
            // lblDGCRemotePath
            // 
            this.lblDGCRemotePath.AutoSize = true;
            this.lblDGCRemotePath.Location = new System.Drawing.Point(16, 41);
            this.lblDGCRemotePath.Name = "lblDGCRemotePath";
            this.lblDGCRemotePath.Size = new System.Drawing.Size(134, 13);
            this.lblDGCRemotePath.TabIndex = 32;
            this.lblDGCRemotePath.Text = "This is where path appears";
            // 
            // cbDGCPromptAndSave
            // 
            this.cbDGCPromptAndSave.AutoSize = true;
            this.cbDGCPromptAndSave.Checked = true;
            this.cbDGCPromptAndSave.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDGCPromptAndSave.Location = new System.Drawing.Point(12, 62);
            this.cbDGCPromptAndSave.Name = "cbDGCPromptAndSave";
            this.cbDGCPromptAndSave.Size = new System.Drawing.Size(180, 17);
            this.cbDGCPromptAndSave.TabIndex = 48;
            this.cbDGCPromptAndSave.Text = "Save Image Copies and Settings";
            this.cbDGCPromptAndSave.UseVisualStyleBackColor = true;
            // 
            // cbDGCEnabled
            // 
            this.cbDGCEnabled.AutoSize = true;
            this.cbDGCEnabled.Checked = true;
            this.cbDGCEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDGCEnabled.Location = new System.Drawing.Point(12, 22);
            this.cbDGCEnabled.Name = "cbDGCEnabled";
            this.cbDGCEnabled.Size = new System.Drawing.Size(226, 17);
            this.cbDGCEnabled.TabIndex = 47;
            this.cbDGCEnabled.Text = "Check For Image From Lower Grid Camera";
            this.cbDGCEnabled.UseVisualStyleBackColor = true;
            // 
            // btnTestStopLive
            // 
            this.btnTestStopLive.Location = new System.Drawing.Point(141, 297);
            this.btnTestStopLive.Name = "btnTestStopLive";
            this.btnTestStopLive.Size = new System.Drawing.Size(63, 22);
            this.btnTestStopLive.TabIndex = 46;
            this.btnTestStopLive.Text = "StopLive";
            this.btnTestStopLive.UseVisualStyleBackColor = true;
            this.btnTestStopLive.Click += new System.EventHandler(this.btnTestStopLive_Click);
            // 
            // btnTestStartLive
            // 
            this.btnTestStartLive.Location = new System.Drawing.Point(70, 297);
            this.btnTestStartLive.Name = "btnTestStartLive";
            this.btnTestStartLive.Size = new System.Drawing.Size(63, 22);
            this.btnTestStartLive.TabIndex = 45;
            this.btnTestStartLive.Text = "StartLive";
            this.btnTestStartLive.UseVisualStyleBackColor = true;
            this.btnTestStartLive.Click += new System.EventHandler(this.btnTestStartLive_Click);
            // 
            // btnTestDoubleTriggeredAcquision
            // 
            this.btnTestDoubleTriggeredAcquision.Location = new System.Drawing.Point(3, 297);
            this.btnTestDoubleTriggeredAcquision.Name = "btnTestDoubleTriggeredAcquision";
            this.btnTestDoubleTriggeredAcquision.Size = new System.Drawing.Size(61, 22);
            this.btnTestDoubleTriggeredAcquision.TabIndex = 44;
            this.btnTestDoubleTriggeredAcquision.Text = "TestDTA";
            this.btnTestDoubleTriggeredAcquision.UseVisualStyleBackColor = true;
            this.btnTestDoubleTriggeredAcquision.Click += new System.EventHandler(this.btnTestDoubleTriggeredAcquision_Click);
            // 
            // btnTest123
            // 
            this.btnTest123.Location = new System.Drawing.Point(220, 297);
            this.btnTest123.Name = "btnTest123";
            this.btnTest123.Size = new System.Drawing.Size(54, 22);
            this.btnTest123.TabIndex = 43;
            this.btnTest123.Text = "Test123";
            this.btnTest123.UseVisualStyleBackColor = true;
            this.btnTest123.Click += new System.EventHandler(this.btnTest123_Click);
            // 
            // btnSaveBitmap
            // 
            this.btnSaveBitmap.Location = new System.Drawing.Point(194, 269);
            this.btnSaveBitmap.Name = "btnSaveBitmap";
            this.btnSaveBitmap.Size = new System.Drawing.Size(80, 22);
            this.btnSaveBitmap.TabIndex = 42;
            this.btnSaveBitmap.Text = "Save BMP ...";
            this.btnSaveBitmap.UseVisualStyleBackColor = true;
            this.btnSaveBitmap.Click += new System.EventHandler(this.btnSaveBitmap_Click);
            // 
            // cbDebugTrySavingBitmap
            // 
            this.cbDebugTrySavingBitmap.AutoSize = true;
            this.cbDebugTrySavingBitmap.Checked = true;
            this.cbDebugTrySavingBitmap.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDebugTrySavingBitmap.Enabled = false;
            this.cbDebugTrySavingBitmap.Location = new System.Drawing.Point(21, 280);
            this.cbDebugTrySavingBitmap.Name = "cbDebugTrySavingBitmap";
            this.cbDebugTrySavingBitmap.Size = new System.Drawing.Size(166, 17);
            this.cbDebugTrySavingBitmap.TabIndex = 41;
            this.cbDebugTrySavingBitmap.Text = "Try Saving Bitmap Grid Image";
            this.cbDebugTrySavingBitmap.UseVisualStyleBackColor = true;
            // 
            // cbDebugAutoStartVideo
            // 
            this.cbDebugAutoStartVideo.AutoSize = true;
            this.cbDebugAutoStartVideo.Checked = true;
            this.cbDebugAutoStartVideo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDebugAutoStartVideo.Location = new System.Drawing.Point(21, 261);
            this.cbDebugAutoStartVideo.Name = "cbDebugAutoStartVideo";
            this.cbDebugAutoStartVideo.Size = new System.Drawing.Size(97, 17);
            this.cbDebugAutoStartVideo.TabIndex = 40;
            this.cbDebugAutoStartVideo.Text = "AutoStartVideo";
            this.cbDebugAutoStartVideo.UseVisualStyleBackColor = true;
            // 
            // cbDebugDisableGridDrop
            // 
            this.cbDebugDisableGridDrop.AutoSize = true;
            this.cbDebugDisableGridDrop.Location = new System.Drawing.Point(21, 244);
            this.cbDebugDisableGridDrop.Name = "cbDebugDisableGridDrop";
            this.cbDebugDisableGridDrop.Size = new System.Drawing.Size(109, 17);
            this.cbDebugDisableGridDrop.TabIndex = 25;
            this.cbDebugDisableGridDrop.Text = "Disable Grid Drop";
            this.cbDebugDisableGridDrop.UseVisualStyleBackColor = true;
            // 
            // buttonStain
            // 
            this.buttonStain.Location = new System.Drawing.Point(141, 257);
            this.buttonStain.Name = "buttonStain";
            this.buttonStain.Size = new System.Drawing.Size(54, 22);
            this.buttonStain.TabIndex = 39;
            this.buttonStain.Text = "Stain";
            this.buttonStain.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(138, 245);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(85, 13);
            this.label9.TabIndex = 38;
            this.label9.Text = "Stain Time (Sec)";
            // 
            // numericUpDownStainTime
            // 
            this.numericUpDownStainTime.Location = new System.Drawing.Point(227, 242);
            this.numericUpDownStainTime.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.numericUpDownStainTime.Name = "numericUpDownStainTime";
            this.numericUpDownStainTime.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownStainTime.TabIndex = 37;
            this.numericUpDownStainTime.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.label29);
            this.groupBox10.Controls.Add(this.numericUpDownDebugStrobeDelay);
            this.groupBox10.Controls.Add(this.label28);
            this.groupBox10.Controls.Add(this.numericUpDownDebugStrobeDuration);
            this.groupBox10.Controls.Add(this.label27);
            this.groupBox10.Controls.Add(this.numericUpDownDebugVideoWidth);
            this.groupBox10.Controls.Add(this.label26);
            this.groupBox10.Controls.Add(this.numericUpDownDebugJPG);
            this.groupBox10.Location = new System.Drawing.Point(8, 165);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(266, 71);
            this.groupBox10.TabIndex = 26;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Camera / Video Recording Parameters";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label29.Location = new System.Drawing.Point(186, 47);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(73, 9);
            this.label29.TabIndex = 31;
            this.label29.Text = "Strobe Delay (usec)";
            // 
            // numericUpDownDebugStrobeDelay
            // 
            this.numericUpDownDebugStrobeDelay.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownDebugStrobeDelay.Location = new System.Drawing.Point(133, 44);
            this.numericUpDownDebugStrobeDelay.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownDebugStrobeDelay.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numericUpDownDebugStrobeDelay.Name = "numericUpDownDebugStrobeDelay";
            this.numericUpDownDebugStrobeDelay.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownDebugStrobeDelay.TabIndex = 30;
            this.numericUpDownDebugStrobeDelay.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label28.Location = new System.Drawing.Point(186, 28);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(74, 9);
            this.label28.TabIndex = 29;
            this.label28.Text = "Strobe Duration (us)";
            // 
            // numericUpDownDebugStrobeDuration
            // 
            this.numericUpDownDebugStrobeDuration.Location = new System.Drawing.Point(133, 22);
            this.numericUpDownDebugStrobeDuration.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownDebugStrobeDuration.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownDebugStrobeDuration.Name = "numericUpDownDebugStrobeDuration";
            this.numericUpDownDebugStrobeDuration.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownDebugStrobeDuration.TabIndex = 28;
            this.numericUpDownDebugStrobeDuration.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(58, 46);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(70, 13);
            this.label27.TabIndex = 27;
            this.label27.Text = "Width (pixels)";
            // 
            // numericUpDownDebugVideoWidth
            // 
            this.numericUpDownDebugVideoWidth.Enabled = false;
            this.numericUpDownDebugVideoWidth.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numericUpDownDebugVideoWidth.Location = new System.Drawing.Point(4, 44);
            this.numericUpDownDebugVideoWidth.Maximum = new decimal(new int[] {
            800,
            0,
            0,
            0});
            this.numericUpDownDebugVideoWidth.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownDebugVideoWidth.Name = "numericUpDownDebugVideoWidth";
            this.numericUpDownDebugVideoWidth.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownDebugVideoWidth.TabIndex = 26;
            this.numericUpDownDebugVideoWidth.Value = new decimal(new int[] {
            800,
            0,
            0,
            0});
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(57, 24);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(70, 13);
            this.label26.TabIndex = 25;
            this.label26.Text = "JPEG Compn";
            // 
            // numericUpDownDebugJPG
            // 
            this.numericUpDownDebugJPG.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDownDebugJPG.Location = new System.Drawing.Point(4, 22);
            this.numericUpDownDebugJPG.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownDebugJPG.Name = "numericUpDownDebugJPG";
            this.numericUpDownDebugJPG.Size = new System.Drawing.Size(51, 20);
            this.numericUpDownDebugJPG.TabIndex = 21;
            this.numericUpDownDebugJPG.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // labelTip1VideoCentered
            // 
            this.labelTip1VideoCentered.AutoSize = true;
            this.labelTip1VideoCentered.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTip1VideoCentered.Location = new System.Drawing.Point(902, 415);
            this.labelTip1VideoCentered.Name = "labelTip1VideoCentered";
            this.labelTip1VideoCentered.Size = new System.Drawing.Size(84, 20);
            this.labelTip1VideoCentered.TabIndex = 21;
            this.labelTip1VideoCentered.Text = "Not Ready";
            // 
            // labelTip2VideoCentered
            // 
            this.labelTip2VideoCentered.AutoSize = true;
            this.labelTip2VideoCentered.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTip2VideoCentered.Location = new System.Drawing.Point(902, 438);
            this.labelTip2VideoCentered.Name = "labelTip2VideoCentered";
            this.labelTip2VideoCentered.Size = new System.Drawing.Size(84, 20);
            this.labelTip2VideoCentered.TabIndex = 22;
            this.labelTip2VideoCentered.Text = "Not Ready";
            // 
            // labelTip3VideoCentered
            // 
            this.labelTip3VideoCentered.AutoSize = true;
            this.labelTip3VideoCentered.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTip3VideoCentered.Location = new System.Drawing.Point(902, 461);
            this.labelTip3VideoCentered.Name = "labelTip3VideoCentered";
            this.labelTip3VideoCentered.Size = new System.Drawing.Size(84, 20);
            this.labelTip3VideoCentered.TabIndex = 23;
            this.labelTip3VideoCentered.Text = "Not Ready";
            // 
            // labelRecordingVideo
            // 
            this.labelRecordingVideo.AutoSize = true;
            this.labelRecordingVideo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRecordingVideo.Location = new System.Drawing.Point(14, 581);
            this.labelRecordingVideo.Name = "labelRecordingVideo";
            this.labelRecordingVideo.Size = new System.Drawing.Size(50, 20);
            this.labelRecordingVideo.TabIndex = 24;
            this.labelRecordingVideo.Text = "Video";
            // 
            // btnGotoSafePosition
            // 
            this.btnGotoSafePosition.Location = new System.Drawing.Point(808, 486);
            this.btnGotoSafePosition.Name = "btnGotoSafePosition";
            this.btnGotoSafePosition.Size = new System.Drawing.Size(210, 26);
            this.btnGotoSafePosition.TabIndex = 6;
            this.btnGotoSafePosition.Text = "Go to SafePosition";
            this.btnGotoSafePosition.UseVisualStyleBackColor = true;
            this.btnGotoSafePosition.Click += new System.EventHandler(this.btnGotoSafePosition_Click);
            // 
            // buttonWriteSettingsToXMLFile
            // 
            this.buttonWriteSettingsToXMLFile.Enabled = false;
            this.buttonWriteSettingsToXMLFile.Location = new System.Drawing.Point(808, 518);
            this.buttonWriteSettingsToXMLFile.Name = "buttonWriteSettingsToXMLFile";
            this.buttonWriteSettingsToXMLFile.Size = new System.Drawing.Size(210, 26);
            this.buttonWriteSettingsToXMLFile.TabIndex = 25;
            this.buttonWriteSettingsToXMLFile.Text = "Write Settings to XML File";
            this.buttonWriteSettingsToXMLFile.UseVisualStyleBackColor = true;
            this.buttonWriteSettingsToXMLFile.Click += new System.EventHandler(this.buttonWriteSettingsToXMLFile_Click);
            // 
            // buttonArchiveExperimentSettings
            // 
            this.buttonArchiveExperimentSettings.Location = new System.Drawing.Point(808, 547);
            this.buttonArchiveExperimentSettings.Name = "buttonArchiveExperimentSettings";
            this.buttonArchiveExperimentSettings.Size = new System.Drawing.Size(210, 26);
            this.buttonArchiveExperimentSettings.TabIndex = 26;
            this.buttonArchiveExperimentSettings.Text = "Archive Experiment Settings To File";
            this.buttonArchiveExperimentSettings.UseVisualStyleBackColor = true;
            this.buttonArchiveExperimentSettings.Click += new System.EventHandler(this.buttonArchiveExperimentSettings_Click);
            // 
            // textBoxArchiveFile
            // 
            this.textBoxArchiveFile.Location = new System.Drawing.Point(851, 579);
            this.textBoxArchiveFile.Name = "textBoxArchiveFile";
            this.textBoxArchiveFile.Size = new System.Drawing.Size(152, 20);
            this.textBoxArchiveFile.TabIndex = 28;
            this.textBoxArchiveFile.Text = "Experiment1";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(787, 582);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(57, 13);
            this.label16.TabIndex = 28;
            this.label16.Text = "Exp Note1";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(786, 605);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(60, 13);
            this.label21.TabIndex = 29;
            this.label21.Text = "Exp Note 2";
            // 
            // textBoxArchiveNote
            // 
            this.textBoxArchiveNote.Location = new System.Drawing.Point(851, 602);
            this.textBoxArchiveNote.Name = "textBoxArchiveNote";
            this.textBoxArchiveNote.Size = new System.Drawing.Size(152, 20);
            this.textBoxArchiveNote.TabIndex = 30;
            this.textBoxArchiveNote.Text = "Condition A, Grid #";
            // 
            // cbSelectTip1andTip2
            // 
            this.cbSelectTip1andTip2.AutoSize = true;
            this.cbSelectTip1andTip2.Location = new System.Drawing.Point(806, 394);
            this.cbSelectTip1andTip2.Name = "cbSelectTip1andTip2";
            this.cbSelectTip1andTip2.Size = new System.Drawing.Size(162, 17);
            this.cbSelectTip1andTip2.TabIndex = 31;
            this.cbSelectTip1andTip2.Text = "Use tip 1 and 2 for Aspiration";
            this.cbSelectTip1andTip2.UseVisualStyleBackColor = true;
            // 
            // btnCloseSyringeValves
            // 
            this.btnCloseSyringeValves.Location = new System.Drawing.Point(204, 61);
            this.btnCloseSyringeValves.Name = "btnCloseSyringeValves";
            this.btnCloseSyringeValves.Size = new System.Drawing.Size(61, 22);
            this.btnCloseSyringeValves.TabIndex = 47;
            this.btnCloseSyringeValves.Text = "Close";
            this.btnCloseSyringeValves.UseVisualStyleBackColor = true;
            this.btnCloseSyringeValves.Click += new System.EventHandler(this.btnCloseSyringeValves_Click);
            // 
            // frmMain
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1051, 652);
            this.Controls.Add(this.cbSelectTip1andTip2);
            this.Controls.Add(this.textBoxArchiveNote);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.textBoxArchiveFile);
            this.Controls.Add(this.buttonArchiveExperimentSettings);
            this.Controls.Add(this.buttonWriteSettingsToXMLFile);
            this.Controls.Add(this.btnGotoSafePosition);
            this.Controls.Add(this.labelRecordingVideo);
            this.Controls.Add(this.labelTip3VideoCentered);
            this.Controls.Add(this.labelTip2VideoCentered);
            this.Controls.Add(this.labelTip1VideoCentered);
            this.Controls.Add(this.tabControlRobot);
            this.Controls.Add(this.radioButtonTip3);
            this.Controls.Add(this.radioButtonTip2);
            this.Controls.Add(this.radioButtonTip1);
            this.Controls.Add(this.DisplayWindow);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.StatBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.mnuMain;
            this.MinimumSize = new System.Drawing.Size(600, 584);
            this.Name = "frmMain";
            this.Text = "EA Table Top Instrument";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.frmMain_Closing);
            ((System.ComponentModel.ISupportInitialize)(this.pnlLoop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlMessage)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DisplayWindow)).EndInit();
            this.tabControlRobot.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMixDutyCycle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMixAmplitude)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMixFreq)).EndInit();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCosAmp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCosFreq)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTrapTrailing)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTrapDwell)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTrapLeading)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTipAmp)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownUSWashNumberOfCycles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownUSWashSyringeSpeedCode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumberWipeTips)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownWash)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAspirateVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPrimeNumber)).EndInit();
            this.tabSpotSample.ResumeLayout(false);
            this.tabSpotSample.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOTFIDelayAfterTrip)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOTFISlowSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownOTFIPercentWayToTarget)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPauseOTF)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSpotDropNumber)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.groupBox11.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCamGain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarExposure)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPixelClock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFPS)).EndInit();
            this.tabStain.ResumeLayout(false);
            this.tabStain.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSTNDelay2_ms)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSTNDelay_ms)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSTNTip3Drops)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSTNTip2Drops)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSTNTip1Drops)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3StackDecel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3StackAccel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3StackVel)).EndInit();
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStainTime)).EndInit();
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDebugStrobeDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDebugStrobeDuration)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDebugVideoWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDebugJPG)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        public frmMain()
        {

            // Required for Windows Form Designer support
            InitializeComponent();

            // event handler for when the user changes a setting for some command
            //this.Viewer.ParameterChanged += new ControlLibrary.ParameterChangedEventHandler(Viewer_ParameterChanged);

            // populate combo box with actions that can be added to a sequence file
            //AddActionsToComboBox();

            // initialize some gui things
            Clear();

            // Lets see if we are running on the development system

            mdsf = MachineDevelopmentSystemFlag.GetMachineDevelopmentSystemFlagFromFile(ProjectFiles.MachineDevelopmentSystemFlag);        // PKv5.5.5

            aboutMenu();

            if (mdsf.RunOnPHXDevSystem)                 // PKv5.5.5
            {
                Console.WriteLine("Running on the phoenix development system");
                Pixy = new ThisRobot(ProjectFiles.MachineConfigurationFileDev);
            }
            else
            {
                Console.WriteLine("NYSCB installed version");
                Pixy = new ThisRobot(ProjectFiles.MachineConfigurationFile);
            }



            Pixy.InitializeRobot();

            //Ivan's video edits
            Camera = new Video();
            Camera.setDisplayHandle(DisplayWindow.Handle);

            //Ivan's radio button edits
            this.radioButtonTip1.CheckedChanged += new EventHandler(radioButtons_CheckedChanged);
            this.radioButtonTip2.CheckedChanged += new EventHandler(radioButtons_CheckedChanged);
            this.radioButtonTip3.CheckedChanged += new EventHandler(radioButtons_CheckedChanged);

            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(frmMain_KeyPressHandler);
            this.numericUpDownTipAmp.Value = Pixy.MachineParameters.TrapAmp[Pixy.ActiveTip];
            this.numericUpDownSpotDropNumber.Value = Pixy.MachineParameters.NumberDropsToSpot[Pixy.ActiveTip];
            this.numericUpDownAspirateVolume.Value = (decimal) Pixy.MachineParameters.AspirateVolume[Pixy.ActiveTip];

            this.trackBarPixelClock.Scroll += new EventHandler(trackBarPixelClock_Changed);
            this.trackBarFPS.Scroll += new EventHandler(trackBarFPS_Changed);
            this.trackBarExposure.Scroll += new EventHandler(trackBarExposure_Changed);
            this.numericUpDownSpotDropNumber.ValueChanged += new EventHandler(numericUpDownSpotDropNumber_Changed);
            this.numericUpDownTipAmp.ValueChanged += new EventHandler(numericUpDownTipAmp_Changed);
            this.numericUpDownAspirateVolume.ValueChanged += new EventHandler(numericUpDownAspirateVolume_Changed);
            trackBarPixelClock_Changed(this, null);
            trackBarExposure_Changed(this, null);
            trackBarFPS_Changed(this, null);
            Pixy.MachineParameters.TipVideoCentered[0] = true; //need to set at the beginning so if we read from settings as true...lets revert back to false,  2019-01-29
            Pixy.MachineParameters.TipVideoCentered[1] = false; //need to set at the beginning so if we read from settings as true...lets revert back to false
            Pixy.MachineParameters.TipVideoCentered[2] = false; //need to set at the beginning so if we read from settings as true...lets revert back to false

            for (int i = 0; i < Pixy.MachineParameters.NumberOfTips; i++)
            {
                UpdateTipVideoCenterDisplay(i);
            }
            //setup the file dialog and corresponding text box to directory from xml file 
            this.textBoxVideoFolder.Text = Pixy.MachineParameters.VideoFileDirectory;
            this.folderBrowserDialog1.SelectedPath = Pixy.MachineParameters.VideoFileDirectory;

            // PKv5.5.0.1
            btnCamReadGain_Click(this, null);

            IO.SetOutput(-2);


            // Open up the default datalog file (this will create one if it doesn't exist)
            CurrentDatalogFile = Pixy.MachineParameters.LogFileDirectory + Pixy.MachineParameters.DefaultLogFile;
            FileInfo fileinfo = new FileInfo(CurrentDatalogFile);
            Datalog log = new Datalog(CurrentDatalogFile, fileinfo.Exists);
            //		lbLogFileName.Text = "Selected Datalog: " + fileinfo.Name;
            log.Close();

            UserDatalog("Machine Initialized");

            // PKv5.2
          //  cbClearTipsB4Plunge.Checked = Pixy.MachineParameters.ClearTipsB4Plunge;         // Let user see the value of this (Read only)
          //  cbUseBentTweezerPoints.Checked = Pixy.MachineParameters.UseBentTweezerPoints;

            // PKv5.2.4
            this.numericUpDownTrapLeading.Value = Pixy.MachineParameters.DE03TrapSetupLeading[Pixy.ActiveTip];  // PKv5.2.4
            this.numericUpDownTrapDwell.Value = Pixy.MachineParameters.DE03TrapSetupDwell[Pixy.ActiveTip]; ;
            this.numericUpDownTrapTrailing.Value = Pixy.MachineParameters.DE03TrapSetupTrailing[Pixy.ActiveTip];

            this.numericUpDownCosFreq.Value = Pixy.MachineParameters.DE03CosSetupFreq[Pixy.ActiveTip];
            this.numericUpDownCosAmp.Value = Pixy.MachineParameters.DE03CosSetupAmp[Pixy.ActiveTip];

            this.numericUpDownAspirateVolume.Value = (decimal)Pixy.MachineParameters.AspirateVolume[Pixy.ActiveTip];  //2019-01-16

            // PKv5.5.1, PKv5.5.3             // Let the users see this if it is changed in the file
            this.numericUpDownUSWashSyringeSpeedCode.Value = Pixy.MachineParameters.SyringePushSpeedPrime;
            this.numericUpDownOTFIDelayAfterTrip.Value = Pixy.MachineParameters.InsGridCamDelayAfterTrip;

            // PKv5.5.4
            this.numericUpDownOTFIPercentWayToTarget.Value = Pixy.MachineParameters.OTFSlowDownPercent;
            this.numericUpDownOTFISlowSpeed.Value = Pixy.MachineParameters.OTFSlowDownSpeed;

            // PKv5.6
            this.lblDGCRemotePath.Text = Pixy.MachineParameters.RemoteVideoFileDirectory;
            lblTipMovement.Text = "Tip Movements:    Backlite Z offset (mm) = " + Pixy.MachineParameters.OTFTargetZOffset_mm.ToString();

        }


        private void Clear()
        {
            mVM.Clear();
            mCommandList = null;
            mTreeNodes = null;
            mSeqFile = null;

            bPause = false;
            bAbort = false;
            mSequenceChanged = false;
            mUpdateVariables = false;

            //	this.lbRunInfo1.Text = "";
            //	this.lbRunInfo2.Text = "";

            SetState(GUI_STATE.IdleNoSequence);

            ClearStatusBar();

            //	ShowCommandList();
        }



        // The main entry point for the application.
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.DoEvents();
            Application.Run(new frmMain());
        }

        #region open and close tool windows

        private void ShowTestCommandWindow()
        {
            DoActionWindow = new DoActionForm();

            // pass it a list of methods to run for each command
            ProcessActionOpList ActionOpList = new ProcessActionOpList();
            GetActionOperations(ActionOpList);

            // give access to the system variables
            mVM.Clear();
            AddSystemVariables(mVM);
            // and user variables defined in current file
            AddSequenceVariables(mVM);

            AddImportVariables(mVM);    // [LEA COMMAND DEF]

            DoActionWindow.AllowProcessActions(ActionOpList, mVM);
        }


        private void ShowMotionWindow()
        {
            MotionWindow = new MotionControlForm();

            //PK23May - Enable manual control when showing the window
            Pixy.MotionControl.EnableManualControl(Pixy, true);

            MotionWindow.Show(Pixy.MotionControl, Pixy.MachineParameters);
        }


        //		private void ShowSyringeWindow()   //Syringe-TODO
        //		{
        //			SyringeWindow = new SyringeControlForm();
        //			SyringeWindow.Show(Pixy.SyringeControl);
        //		}

        private void ShowTipControlWindow()
        {
            /*
			FiringWindow = new TipControllerForm();
			WaveformParameters t1DiagnosticWaveformParameters = new WaveformParameters();
			WaveformParameters t2DiagnosticWaveformParameters = new WaveformParameters();
			FiringWindow.Show(Pixy.FiringControl,t1DiagnosticWaveformParameters,t2DiagnosticWaveformParameters);
			*/
            //			FiringWindow = new DE02Form();
            //			FiringWindow.Show();     // 20Feb2008 Trying new DE02 control window.
        }

        private void CloseTestCommandWindow()
        {
            try { if (DoActionWindow != null) DoActionWindow.Close(); }
            catch { }
            finally { DoActionWindow = null; }
        }

        private void CloseMotionWindow()
        {
            try { if (MotionWindow != null) MotionWindow.Close(); }
            catch { }
            finally { MotionWindow = null; }
        }

        //		private void CloseSyringeWindow()    // Syringe-TODO
        //		{
        //			try{if (SyringeWindow != null) SyringeWindow.Close();}
        //			catch{}
        //			finally{SyringeWindow = null;}
        //		}

        private void CloseTipControllerWindow()
        {
            //try{if (FiringWindow != null)FiringWindow.Close();}
            //catch{}
            //finally{FiringWindow = null;}
        }

        private void CloseToolWindows()
        {
            CloseTestCommandWindow();
            CloseMotionWindow();
            ////Syringe-TODO
            //			CloseSyringeWindow();
            CloseTipControllerWindow();
        }


        #endregion open and close tool windows

        #region menu stuff


        private void mnuBackRight_Click(object sender, System.EventArgs e)
        {
            if (Pixy.MotionControl.MoveZToSafeHeight(Pixy, true) != 0) return;
            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[0], Pixy.MachineParameters.SafePoint, 0.0, Pixy.MotionControl.DefaultSpeed_pct, true, true);
        }

        private void mnuInspMoveTip1_Click(object sender, System.EventArgs e)
        {
            if (Pixy.MotionControl.MoveZToSafeHeight(Pixy, true) != 0) return;
            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[0], Pixy.MachineParameters.SideCamInspectPoint, 0.0, Pixy.MotionControl.DefaultSpeed_pct, true, true);
        }

        private void mnuInspMoveTip2_Click(object sender, System.EventArgs e)
        {
            if (Pixy.MachineParameters.Tips.Length < 2) return;
            if (Pixy.MotionControl.MoveZToSafeHeight(Pixy, true) != 0) return;
            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[1], Pixy.MachineParameters.SideCamInspectPoint, 0.0, Pixy.MotionControl.DefaultSpeed_pct, true, true);
        }

        private void mnuMoveZUp_Click(object sender, System.EventArgs e)
        {
            if (Pixy.MotionControl.MoveZToSafeHeight(Pixy, true) != 0) return;
        }


        private void mnuStopNow_Click(object sender, System.EventArgs e)
        {
            StopNow();
        }



        private void mnuTestCommands_Click(object sender, System.EventArgs e)
        {
            if ((this.mState != PixyControl.frmMain.GUI_STATE.Running) && (this.mState != PixyControl.frmMain.GUI_STATE.Paused))
            {
                ShowTestCommandWindow();
            }
        }


        private void mnuCheckSequence_Click(object sender, System.EventArgs e)
        {
            CheckSequence();
        }

        private void mnuDryRun_Click(object sender, System.EventArgs e)
        {
            StartSequence(true);
        }

        private void mnuStep_Click(object sender, System.EventArgs e)
        {
            SetStepMode(!mStepMode);
        }

        private void mnuSaveAs_Click(object sender, System.EventArgs e)
        {
            SaveSequenceAs();
        }

        private void mnuSave_Click(object sender, System.EventArgs e)
        {
            SaveSequenceToFile();
        }


        private void mnuExit_Click(object sender, System.EventArgs e)
        {
            Application.Exit();
        }

        private void mnuReload_Click(object sender, System.EventArgs e)
        {
            if (this.mSeqFile == null) return;

            if (this.mSequenceChanged)
            {
                System.Windows.Forms.DialogResult Rslt = MessageBox.Show("You will lose your changes.  Are you sure you want to reload?", "Save Changes", System.Windows.Forms.MessageBoxButtons.YesNoCancel);

                if (Rslt != DialogResult.Yes) return;
            }

            LoadSequenceFile(this.mSeqFile.FileName);

        }

        private void mnuOpenWorklist_Click(object sender, System.EventArgs e)
        {
            OpenNewWorklist();
        }

        private void mnuOpenAndRunWorklist_Click(object sender, EventArgs e)
        {
            if (OpenNewWorklist() != 0) return;
            StartSequence(false);
        }


        private void mnuPauseSequence_Click(object sender, System.EventArgs e)
        {
            bPause = true;
        }

        /*
		private void mnuInspection_Click(object sender, System.EventArgs e)
		{
			mnuInspection.Checked = !mnuInspection.Checked = false;
			bInspection = mnuInspection.Checked;
			
			SetInspectionProcessEnable(bInspection);

			//Update the worklist display ?
		}
		*/

        private void mnuStopSequence_Click(object sender, System.EventArgs e)
        {
            StopSequence();
        }

        private void mnuRunSequence_Click(object sender, System.EventArgs e)
        {
            if (SyringeControlForm.FormCount > 0)    //14Feb2008
            {
                MessageBox.Show("Please close Syringe Control form before running a sequence", "Warning");
            }
            else
            {
                //				DE02.EndStrobe();   // 14Feb2008 In case it's on.
                StartSequence(false);
            }
        }

        private void mnuController_Click(object sender, System.EventArgs e)
        {
            //			ShowTipControlWindow();
            Aurigin.TestDE03 testDE03Form = new TestDE03();
            testDE03Form.Show();
        }

        private void mnuMotion_Click(object sender, System.EventArgs e)
        {
            ShowMotionWindow();
        }


        private void mnuSyringe_Click(object sender, System.EventArgs e)
        {
            if (SyringeControlForm.FormCount == 0)
            {
                Aurigin.SyringeControlForm syringeForm = new SyringeControlForm();
                syringeForm.Show();
            }
            //			ShowSyringeWindow();		
        }


        private void mnuMachineSettings_Click(object sender, System.EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Must restart application for any changes to take effect.", "EDIT CONFIGURATION");
            ShowInNotepad(ProjectFiles.MachineConfigurationFile);
        }


        private void mnuSyringeSettings_Click(object sender, System.EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Must restart application for any changes to take effect.", "EDIT CONFIGURATION");
            ShowInNotepad(ProjectFiles.SyringeConfigurationFile);
        }


        private void mnuMotionSettings_Click(object sender, System.EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Must restart application for any changes to take effect.", "EDIT CONFIGURATION");
            ShowInNotepad(ProjectFiles.MotionConfigurationFile);
        }


        private void mnuProcessSequence_Click(object sender, System.EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Must restart application for any changes to take effect.", "EDIT CONFIGURATION");
            ShowInNotepad(ProjectFiles.MainSequenceFile);
        }

        private void btnAdd_Click(object sender, System.EventArgs e)
        {
            AddSelectedActionToSequence();
        }

        /*
		private void mnuOpenWorklist_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog OpenDlg = new OpenFileDialog();
			StringBuilder  Filter = new StringBuilder("worklists (*");
			Filter.Append(Pixy.MachineParameters.WorklistExtension);
			Filter.Append(")|*");
			Filter.Append(Pixy.MachineParameters.WorklistExtension);
			Filter.Append("|text files (*.txt)|*.txt");
			
			OpenDlg.Filter = Filter.ToString();
			OpenDlg.InitialDirectory = Pixy.MachineParameters.WorklistDirectory;
			OpenDlg.FilterIndex = 1 ;
			OpenDlg.RestoreDirectory = true ;
			OpenDlg.Multiselect = false;

			if(OpenDlg.ShowDialog() == DialogResult.OK)
			{
				// try to load it
				if (CurrentWorklist.LoadFromFile(OpenDlg.FileName) != 0) CurrentWorklist.Clear();
			
				// display the worklist
				DisplayWorklist();
				FileInfo fileInfo = new FileInfo(OpenDlg.FileName);
				lbWorklistFile.Text = "Selected Worklist: "+ fileInfo.Name;
			}
		}
		*/

        private void mnuOpenLogFile_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog OpenDlg = new OpenFileDialog();
            StringBuilder Filter = new StringBuilder("datalog (*");
            Filter.Append(Pixy.MachineParameters.LogFileExtension);
            Filter.Append(")|*");
            Filter.Append(Pixy.MachineParameters.LogFileExtension);
            Filter.Append("|text files (*.txt)|*.txt");

            OpenDlg.Filter = Filter.ToString();
            OpenDlg.InitialDirectory = Pixy.MachineParameters.LogFileDirectory;
            OpenDlg.FilterIndex = 1;
            OpenDlg.RestoreDirectory = true;
            OpenDlg.Multiselect = false;

            if (OpenDlg.ShowDialog() == DialogResult.OK)
            {
                CurrentDatalogFile = OpenDlg.FileName;
                FileInfo fileInfo = new FileInfo(OpenDlg.FileName);
                //lbLogFileName.Text = "Selected Datalog: " + fileInfo.Name;
                UserDatalog("Existing Log File Opened for Append");
            }
        }

        private void mnuNewLogFile_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog OpenDlg = new OpenFileDialog();
            StringBuilder Filter = new StringBuilder("datalog (*");
            Filter.Append(Pixy.MachineParameters.LogFileExtension);
            Filter.Append(")|*");
            Filter.Append(Pixy.MachineParameters.LogFileExtension);
            Filter.Append("|text files (*.txt)|*.txt");

            OpenDlg.Filter = Filter.ToString();
            OpenDlg.InitialDirectory = Pixy.MachineParameters.LogFileDirectory;
            OpenDlg.FilterIndex = 1;
            OpenDlg.RestoreDirectory = true;
            OpenDlg.Multiselect = false;
            OpenDlg.CheckFileExists = false;
            OpenDlg.Title = "Enter a new datalog file name";

            if (OpenDlg.ShowDialog() == DialogResult.OK)
            {
                bool overwrite = false;
                FileInfo fileInfo = new FileInfo(OpenDlg.FileName);
                if (fileInfo.Exists)
                {
                    if (MessageBox.Show("Datalog File Already Exists.\n  OK to overwrite ?", "Datalog", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        overwrite = true;
                }
                if (!fileInfo.Exists || overwrite)
                {
                    CurrentDatalogFile = OpenDlg.FileName;
                    Datalog log = new Datalog(OpenDlg.FileName, false);
                    log.Close();
                    UserDatalog("Brand New datalog created");
                    //lbLogFileName.Text = "Selected Datalog: " + fileInfo.Name;
                }
            }
        }

        #endregion menu stuff

        #region datalog

        // Perform all the overhead with writing something to the datalog.
        public void UserDatalog(string Msg)
        {
            try
            {
                Datalog log = new Datalog(CurrentDatalogFile);
                log.WriteLine(Msg);
                log.Close();
            }
            catch { }
        }


        private void UserDatalog(StringBuilder Msg)
        {
            try
            {
                Datalog log = new Datalog(CurrentDatalogFile);
                log.WriteLine(Msg);
                log.Close();
            }
            catch { }
        }

        private void UserDatalog(string[] Msg)
        {
            try
            {
                Datalog log = new Datalog(CurrentDatalogFile);
                log.WriteLine(Msg);
                log.Close();
            }
            catch { }
        }

        private void UserDatalog(string Format, params object[] Arg)
        {
            try
            {
                Datalog log = new Datalog(CurrentDatalogFile);
                log.WriteLine(Format, Arg);
                log.Close();
            }
            catch { }
        }

        #endregion datalog

        #region Text Conversion Functions

        //////////////////////////////////////////////////////////////////////////////////
        //
        // functions to convert from text in commands to real values for process actions
        // handle null strings since many parameters are optional
        //
        //////////////////////////////////////////////////////////////////////////////////
        //
        // Updated to return {0,0,0} if no PointName is specified.   24 Aug 2012
        // Used to 

        private MachineCoordinate GetPoint(string PointName, string XOffset, string YOffset, string ZOffset, out bool NoPointSpecified)
        {
            MachineCoordinate Pt;

            if (PointName == "")
            {
                // get where we are now
                //				Pixy.MotionControl.GetCurrentPosition(out Pt);      commented out 24Aug2012
                Pt = new MachineCoordinate();   // Will assign 0,0,0   added 24Aug2012
                NoPointSpecified = true;
            }
            else
            {
                // get the location specified
                Pt = mVM.GetCoordinateFromText(PointName);
                NoPointSpecified = false;
            }

            // 03-03-07 - copy coordinate to a new point - don't modify the original one each time this is called
            MachineCoordinate NewPt = new MachineCoordinate(Pt);

            // can have an offset from specified or current position
            if (XOffset != "") NewPt.X = NewPt.X + mVM.GetDoubleFromText(XOffset);
            if (YOffset != "") NewPt.Y = NewPt.Y + mVM.GetDoubleFromText(YOffset);
            if (ZOffset != "") NewPt.Z = NewPt.Z + mVM.GetDoubleFromText(ZOffset);

            //			return Pt;
            return NewPt;
        }

        private double GetMoveSpeed_pct(string MoveSpeed)
        {
            if (MoveSpeed == "")
            {
                return LimitMoveSpeed_pct(Pixy.MotionControl.DefaultSpeed_pct);
            }
            else
            {
                return LimitMoveSpeed_pct(mVM.GetDoubleFromText(MoveSpeed));
            }
        }

        private int LimitSyringeSpeed(int RequestedSpeed)
        {
            if (RequestedSpeed < UserSyringeControl.MIN_SPEED_CODE) return UserSyringeControl.MIN_SPEED_CODE;
            if (RequestedSpeed > UserSyringeControl.MAX_SPEED_CODE) return UserSyringeControl.MAX_SPEED_CODE;
            return RequestedSpeed;
        }

        private int GetSyringePrimePullSpeed(string Speed)
        {
            if (Speed == "")
            {
                //				return LimitSyringeSpeed(Pixy.SyringeControl.PrimePullSpeed());    //Syringe-TODO
                return (0);
            }
            else
            {
                return LimitSyringeSpeed(mVM.GetIntFromText(Speed));
            }
        }

        private int GetSyringePrimePushSpeed(string Speed)
        {
            if (Speed == "")
            {
                //				return LimitSyringeSpeed(Pixy.SyringeControl.PrimePushSpeed());   //Syringe-TODO
                return 0;
            }
            else
            {
                return LimitSyringeSpeed(mVM.GetIntFromText(Speed));
            }
        }

        private double LimitMoveSpeed_pct(double RequestedSpeed)
        {
            if (RequestedSpeed <= 0.0) return 0.01;
            if (RequestedSpeed > 100.0) return 100.0;
            return RequestedSpeed;
        }

        int GetOptionalDelay_ms(string Delay_ms)
        {
            if (Delay_ms == "")
            {
                return 0;
            }
            else
            {
                return mVM.GetIntFromText(Delay_ms);
            }
        }

        private int GetSyringeMask(string Mask)
        {
            if (Mask == "")
            {
                //				return UserSyringeControl.TIP1_PUMP;   //Syringe-TODO
                return 0;
            }
            else
            {
                return LimitSyringeMask(mVM.GetIntFromText(Mask));
            }
        }

        private int LimitSyringeMask(int RequestedMask)
        {
            //			if (RequestedMask < UserSyringeControl.TIP1_PUMP) return UserSyringeControl.TIP1_PUMP;   //Syringe-TODO
            //			if (RequestedMask  > UserSyringeControl.BOTH_PUMP) return UserSyringeControl.BOTH_PUMP;
            return RequestedMask;
        }

        private SyringeValvePosition GetSyringeValvePosition(string Position)
        {
            int RequestedPosition = 0;
            if (Position != "") RequestedPosition = mVM.GetIntFromText(Position);

            switch (RequestedPosition)
            {
                // 03-03-07 corrected these
                case 0:
                    return SyringeValvePosition.InputPosition;
                case 1:
                    return SyringeValvePosition.OutputPosition;
                case 2:
                    return SyringeValvePosition.BypassPosition;

                default:
                    throw new Exception("Invalid valve position: " + Position);
            }
        }

        #endregion Text Conversion Functions

        #region Implementation of ProcessActions

        //////////////////////////////////////////////////////////////////////////////////
        //
        //
        //////////////////////////////////////////////////////////////////////////////////


        private int DoPrime(ProcessAction PA)
        {
            Process_Prime Cmd = PA as Process_Prime;
            if (Cmd == null) throw new Exception("Unexpected command type in DoPrime: " + PA.Name);

            // required parameters
            double PrimeVolume_uL = mVM.GetDoubleFromText(Cmd.VolumePerStroke_uL);
            int NumSyringeStrokes = mVM.GetIntFromText(Cmd.NumSyringeStrokes);
            int SyringeMask = GetSyringeMask(Cmd.SyringeMask);

            // optional ones
            bool NoPointSpecified;
            int DelayAfter_ms = GetOptionalDelay_ms(Cmd.DelayAfter_ms);
            MachineCoordinate Pt = GetPoint(Cmd.Tip1Point, Cmd.XOffset, Cmd.YOffset, Cmd.ZOffset, out NoPointSpecified);
            int SyringePushSpeed = GetSyringePrimePushSpeed(Cmd.SyringeSpeedPush);
            int SyringePullSpeed = GetSyringePrimePullSpeed(Cmd.SyringeSpeedPull);

            // some error checking
            if (NumSyringeStrokes <= 0)
            {  // user might disable the command this way by using a variable - allow it but record why it's skipped
                UserDatalog("{0} syringe strokes requested - bypassing prime", NumSyringeStrokes);
                return 0;
            }
            if (PrimeVolume_uL <= 0.0) throw new Exception("Invalid prime volume: " + PrimeVolume_uL);

            if (!NoPointSpecified)
            {
                // move above the point first
                if (Pixy.MotionControl.MoveAbove(Pixy, Pixy.MachineParameters.Tips[0], Pt, Pixy.MotionControl.DefaultSpeed_pct, true, true) != 0) return 1;

                // move down into the wash (possibly)
                if (Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[0], Pt, 0, GetMoveSpeed_pct(Cmd.MoveSpeedIn_pct), true, true) != 0) return 1;
            }

            //			SyringePullSpeed = 5;   //28Feb2007  

            // empty the syringe if there's anything in it    //Syringe-TODO
            //			if (Pixy.SyringeControl.SetValvePosition(SyringeValvePosition.OutputPosition, SyringeMask) != 0) return 1;
            //			if (Pixy.SyringeControl.EmptySyringe(SyringePushSpeed, true, SyringeMask) != 0) return 1;
            if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.OutputPosition, true) != 0) return 1;
            if (AtSyringe.Control.EmptySyringe(SyringeMask, SyringePushSpeed, true) != 0) return 1;

            // prime to the tip
            for (int Cycle = 0; Cycle < NumSyringeStrokes; ++Cycle)
            {
                // fill the tip        //Syringe-TODO
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.InputPosition, true) != 0) return 1;
                if (AtSyringe.Control.Aspirate(SyringeMask, PrimeVolume_uL, SyringePullSpeed, true) != 0) return 1;

                // flush it out     //Syringe-TODO
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.OutputPosition, true) != 0) return 1;
                if (AtSyringe.Control.EmptySyringe(SyringeMask, SyringePushSpeed, true) != 0) return 1;
                //				if (Pixy.SyringeControl.EmptySyringe(SyringePushSpeed, true, SyringeMask) != 0) return 1;
            }



            // go to the normal position  //Syringe-TODO
            //			if (Pixy.SyringeControl.SetValvePosition(SyringeValvePosition.BypassPosition, SyringeMask) != 0) return 1;

            // delay before moving out
            if (DelayAfter_ms > 0) System.Threading.Thread.Sleep(DelayAfter_ms);

            if (!NoPointSpecified)
            {
                // move out
                if (Pixy.MotionControl.MoveAbove(Pixy, Pixy.MachineParameters.Tips[0], Pt, GetMoveSpeed_pct(Cmd.MoveSpeedOut_pct), true, true) != 0) return 1;
            }

            return 0;
        }

        private int DoAspirate(ProcessAction PA)
        {
            Process_Aspirate Cmd = PA as Process_Aspirate;
            if (Cmd == null) throw new Exception("Unexpected command type in DoAspirate: " + PA.Name);

            // required parameters
            double Volume_uL = mVM.GetDoubleFromText(Cmd.Volume_uL);
            int SyringeMask = GetSyringeMask(Cmd.SyringeMask);

            // optional ones
            bool NoPointSpecified;
            MachineCoordinate Well = GetPoint(Cmd.Tip1Point, Cmd.XOffset, Cmd.YOffset, Cmd.ZOffset, out NoPointSpecified);
            int DelayAfter_ms = GetOptionalDelay_ms(Cmd.DelayAfter_ms);
            int SyringePullSpeed = GetSyringePrimePullSpeed(Cmd.SyringeSpeed);
            bool ValveToBypass = mVM.GetBooleanFromText(Cmd.ValveToBypass);
            bool AspFromReservoir = mVM.GetBooleanFromText(Cmd.AspFromReservoir);


            // some error checking
            if (Volume_uL <= 0.0) throw new Exception("Invalid aspirate volume: " + Volume_uL);

            if (!NoPointSpecified)
            {
                // move above the point first
                if (Pixy.MotionControl.MoveAbove(Pixy, Pixy.MachineParameters.Tips[0], Well, Pixy.MotionControl.DefaultSpeed_pct, true, true) != 0) return 1;

                // move down into the well
                if (Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[0], Well, 0.0, GetMoveSpeed_pct(Cmd.MoveSpeedIn_pct), true, true) != 0) return 1;
            }

            // fill the tip   //Syringe-TODO
            //			if (Pixy.SyringeControl.SetValvePosition(SyringeValvePosition.OutputPosition, SyringeMask) != 0) return 1;
            if (AspFromReservoir)
            {
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.InputPosition, true) != 0) return 1;
            }
            else
            {
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.OutputPosition, true) != 0) return 1;
            }
            //Syringe-TODO
            //			if (Pixy.SyringeControl.Aspirate(Volume_uL, SyringePullSpeed, true, SyringeMask) != 0) return 1;
            if (AtSyringe.Control.Aspirate(SyringeMask, Volume_uL, SyringePullSpeed, true) != 0) return 1;

            // go to the normal valve position
            //if (Pixy.SyringeControl.SetValvePosition(SyringeValvePosition.BypassPosition, SyringeMask) != 0) return 1;
            if (ValveToBypass)
            {
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.BypassPosition, true) != 0) return 1;
            }

            ////Syringe-TODO
            //			if (Pixy.SyringeControl.SetValveToPostAspiratePosition(SyringeMask) != 0) return 1;	// 03-28-07

            // delay before moving out
            if (DelayAfter_ms > 0) System.Threading.Thread.Sleep(DelayAfter_ms);

            if (!NoPointSpecified)
            {
                // move out  //pk24may - Need to move out above the well location.
                if (Pixy.MotionControl.MoveAbove(Pixy, Pixy.MachineParameters.Tips[0], Well, GetMoveSpeed_pct(Cmd.MoveSpeedOut_pct), true, true) != 0) return 1;
            }

            return 0;
        }

        private int DoDispense(ProcessAction PA)
        {
            Process_Dispense Cmd = PA as Process_Dispense;
            if (Cmd == null) throw new Exception("Unexpected command type in DoDispense: " + PA.Name);

            // required parameters
            double Volume_uL = mVM.GetDoubleFromText(Cmd.Volume_uL);
            int SyringeMask = GetSyringeMask(Cmd.SyringeMask);

            // optional ones
            bool NoPointSpecified;
            int DelayAfter_ms = GetOptionalDelay_ms(Cmd.DelayAfter_ms);
            MachineCoordinate Well = GetPoint(Cmd.Tip1Point, Cmd.XOffset, Cmd.YOffset, Cmd.ZOffset, out NoPointSpecified);
            int SyringeSpeed = GetSyringePrimePullSpeed(Cmd.SyringeSpeed);
            bool ValveToBypass = mVM.GetBooleanFromText(Cmd.ValveToBypass);


            // some error checking
            if (Volume_uL <= 0.0) throw new Exception("Invalid aspirate volume: " + Volume_uL);

            if (!NoPointSpecified)
            {
                // move above the point first
                if (Pixy.MotionControl.MoveAbove(Pixy, Pixy.MachineParameters.Tips[0], Well, Pixy.MotionControl.DefaultSpeed_pct, true, true) != 0) return 1;

                // move down into the well
                if (Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[0], Well, 0.0, GetMoveSpeed_pct(Cmd.MoveSpeedIn_pct), true, true) != 0) return 1;
            }

            //		    if (Pixy.SyringeControl.SetValvePosition(SyringeValvePosition.OutputPosition, SyringeMask) != 0) return 1;
            if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.OutputPosition, true) != 0) return 1;

            //Syringe-TODO
            //			if (Pixy.SyringeControl.Aspirate(Volume_uL, SyringePullSpeed, true, SyringeMask) != 0) return 1;
            if (DelayAfter_ms == -1)
            {
                if (AtSyringe.Control.Dispense(SyringeMask, Volume_uL, SyringeSpeed, false) != 0) return 1;   // pk-2011-05-20  Don't wait for finish
            }
            else
            {
                if (AtSyringe.Control.Dispense(SyringeMask, Volume_uL, SyringeSpeed, true) != 0) return 1;
            }

            // go to the normal valve position   //Syringe-TODO
            //			if (Pixy.SyringeControl.SetValvePosition(SyringeValvePosition.BypassPosition, SyringeMask) != 0) return 1;
            if (ValveToBypass)
            {
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.BypassPosition, true) != 0) return 1;
            }
            // delay before moving out
            if (DelayAfter_ms > 0) System.Threading.Thread.Sleep(DelayAfter_ms);

            if (!NoPointSpecified)
            {
                // move out  //pk24may - Need to move out above the well location.
                if (Pixy.MotionControl.MoveAbove(Pixy, Pixy.MachineParameters.Tips[0], Well, GetMoveSpeed_pct(Cmd.MoveSpeedOut_pct), true, true) != 0) return 1;
            }

            return 0;
        }

        private int DoSyringeSetValvePosition(ProcessAction PA)
        {
            Process_SetSyringeValvePosition Cmd = PA as Process_SetSyringeValvePosition;
            if (Cmd == null) throw new Exception("Unexpected command type in DoSyringeSetValvePosition: " + PA.Name);

            ////Syringe-TODO
            //			if (Pixy.SyringeControl.SetValvePosition(GetSyringeValvePosition(Cmd.ValvePosition), GetSyringeMask(Cmd.SyringeMask)) != 0) return 1;
            if (AtSyringe.Control.SetValvePosition(GetSyringeMask(Cmd.SyringeMask), GetSyringeValvePosition(Cmd.ValvePosition), true) != 0) return 1;

            int DelayAfter_ms = GetOptionalDelay_ms(Cmd.DelayAfter_ms);
            if (DelayAfter_ms > 0) System.Threading.Thread.Sleep(DelayAfter_ms);

            return 0;
        }

        private int DoSyringeMove(ProcessAction PA)
        {
            Process_SyringeMove Cmd = PA as Process_SyringeMove;
            if (Cmd == null) throw new Exception("Unexpected command type in DoSyringeMove: " + PA.Name);

            int SyringeSpeed = GetSyringePrimePullSpeed(Cmd.SyringeSpeed);

            //Syringe-TODO
            //			if (Pixy.SyringeControl.SetSyringePosition(mVM.GetIntFromText(Cmd.SyringePosition), SyringeSpeed, true, GetSyringeMask(Cmd.SyringeMask)) != 0) return 1;

            int DelayAfter_ms = GetOptionalDelay_ms(Cmd.DelayAfter_ms);
            if (DelayAfter_ms > 0) System.Threading.Thread.Sleep(DelayAfter_ms);

            return 0;
        }

        private int DoSyringeEmpty(ProcessAction PA)
        {
            Process_SyringeEmpty Cmd = PA as Process_SyringeEmpty;
            if (Cmd == null) throw new Exception("Unexpected command type in DoSyringeEmpty: " + PA.Name);

            int SyringeSpeed = GetSyringePrimePushSpeed(Cmd.SyringeSpeed);
            int SyringeMask = GetSyringeMask(Cmd.SyringeMask);
            bool EmptyThroughTip = mVM.GetBooleanFromText(Cmd.EmptyThroughTip);

            if (EmptyThroughTip)
            {
                //Syringe-TODO
                //				if (Pixy.SyringeControl.SetValvePosition(EA.PixyControl.SyringeValvePosition.OutputPosition, SyringeMask) != 0) return 1;
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.OutputPosition, true) != 0) return 1;
            }
            else
            {
                //Syringe-TODO
                //				if (Pixy.SyringeControl.SetValvePosition(EA.PixyControl.SyringeValvePosition.InputPosition, SyringeMask) != 0) return 1;
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.InputPosition, true) != 0) return 1;
            }
            ////Syringe-TODO			
            //			if (Pixy.SyringeControl.EmptySyringe(SyringeSpeed, true, SyringeMask) != 0) return 1;
            if (AtSyringe.Control.EmptySyringe(SyringeMask, SyringeSpeed, true) != 0) return 1;

            int DelayAfter_ms = GetOptionalDelay_ms(Cmd.DelayAfter_ms);
            if (DelayAfter_ms > 0) System.Threading.Thread.Sleep(DelayAfter_ms);

            return 0;
        }

        private int DoWashTips(ProcessAction PA)
        {
            Process_Wash Cmd = PA as Process_Wash;
            if (Cmd == null) throw new Exception("Unexpected command type in DoWashTips: " + PA.Name);

            // Pixy.MotionControl.WashPumpOn();

            // required parameters
            double WashVolume_uL = mVM.GetDoubleFromText(Cmd.VolumePerStroke_uL);
            int NumSyringeStrokes = mVM.GetIntFromText(Cmd.NumSyringeStrokes);
            int SyringeMask = GetSyringeMask(Cmd.SyringeMask);

            // optional ones
            bool DoAirAspirate = false;
            bool NoPointSpecified;
            MachineCoordinate Pt = GetPoint(Cmd.Tip1Point, Cmd.XOffset, Cmd.YOffset, Cmd.ZOffset, out NoPointSpecified);
            int DelayAfter_ms = GetOptionalDelay_ms(Cmd.DelayAfter_ms);
            int SyringePushSpeed = GetSyringePrimePushSpeed(Cmd.SyringeSpeedPush);
            int SyringePullSpeed = GetSyringePrimePullSpeed(Cmd.SyringeSpeedPull);

            // some error checking
            if (NumSyringeStrokes <= 0)
            {  // user might disable the command this way by using a variable - allow it but record why it's skipped
                UserDatalog("{0} syringe strokes requested - bypassing wash", NumSyringeStrokes);
                return 0;
            }

            if (WashVolume_uL <= 0.0) throw new Exception("Invalid wash volume: " + WashVolume_uL);

            // 03-03-07 this is optional
            if (Cmd.DoAirAspirateAfter != "") DoAirAspirate = mVM.GetBooleanFromText(Cmd.DoAirAspirateAfter);

            // Turn on the wash pump.
            //			PerformaxAnDIO.Output(0,true);

            if (!NoPointSpecified)
            {
                // move above the point first
                if (Pixy.MotionControl.MoveAbove(Pixy, Pixy.MachineParameters.Tips[0], Pt, Pixy.MotionControl.DefaultSpeed_pct, true, true) != 0) return 1;

                // move down into the wash (possibly)
                if (Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[0], Pt, 0, GetMoveSpeed_pct(Cmd.MoveSpeedIn_pct), true, true) != 0) return 1;
            }

            // empty the syringe if there's anything in it  //Syringe-TODO
            //			if (Pixy.SyringeControl.SetValvePosition(SyringeValvePosition.OutputPosition, SyringeMask) != 0) return 1;
            //			if (Pixy.SyringeControl.EmptySyringe(SyringePushSpeed, true, SyringeMask) != 0) return 1;
            if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.OutputPosition, true) != 0) return 1;
            if (AtSyringe.Control.EmptySyringe(SyringeMask, SyringePushSpeed, true) != 0) return 1;

            //SyringePullSpeed = 5;   // PK 28FEB2007

            // do the wash cycles
            for (int Cycle = 0; Cycle < NumSyringeStrokes; ++Cycle)
            {
                // fill the tip  //Syringe-TODO
                //				if (Pixy.SyringeControl.SetValvePosition(SyringeValvePosition.InputPosition, SyringeMask) != 0) return 1;
                //				if (Pixy.SyringeControl.Aspirate(WashVolume_uL, SyringePullSpeed, true, SyringeMask) != 0) return 1;
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.InputPosition, true) != 0) return 1;
                if (AtSyringe.Control.Aspirate(SyringeMask, WashVolume_uL, SyringePullSpeed, true) != 0) return 1;

                // flush it out   //Syringe-TODO
                //				if (Pixy.SyringeControl.SetValvePosition(SyringeValvePosition.OutputPosition, SyringeMask) != 0) return 1;
                //				if (Pixy.SyringeControl.EmptySyringe(SyringePushSpeed, true, SyringeMask) != 0) return 1;
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.OutputPosition, true) != 0) return 1;
                if (AtSyringe.Control.EmptySyringe(SyringeMask, SyringePushSpeed, true) != 0) return 1;
            }

            // Turn off the wash pump...
            //			PerformaxAnDIO.Output(0,false);


            //			Pixy.MotionControl.WashPumpoff();

            // go to the normal position  //Syringe-TODO
            //			if (Pixy.SyringeControl.SetValvePosition(SyringeValvePosition.BypassPosition, SyringeMask) != 0) return 1;
            if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.BypassPosition, true) != 0) return 1;

            // delay before moving out
            if (DelayAfter_ms > 0) System.Threading.Thread.Sleep(DelayAfter_ms);

            //31July2006-pk  Optional Air Aspirate
            if (DoAirAspirate)
            {
                // move up a little (5mm hardcoded)...
                if (Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[0], Pt, 5.0, GetMoveSpeed_pct(Cmd.MoveSpeedOut_pct), true, true) != 0) return 1;
                //Syringe-TODO
                //				if (Pixy.SyringeControl.AirAspirate(SyringeMask) !=0 ) return 1;

                // move down into the wash (will knock the glob of liquid)
                // if (Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[0], Pixy.MachineParameters.TipWash, -Settings.DepthInWash, Settings.ZSpeedIn_pct, true, true) != 0) return 1;

                // go to the normal position (again)  //Syringe-TODO
                //				if (Pixy.SyringeControl.SetValvePosition(SyringeValvePosition.BypassPosition, SyringeMask) != 0) return 1;

                // delay before moving out (again)
                if (DelayAfter_ms > 0) System.Threading.Thread.Sleep(DelayAfter_ms);
            }

            // move out
            if (!NoPointSpecified)
            {
                if (Pixy.MotionControl.MoveAbove(Pixy, Pixy.MachineParameters.Tips[0], Pt, GetMoveSpeed_pct(Cmd.MoveSpeedOut_pct), true, true) != 0) return 1;
            }

            return 0;
        }

        private int DoControllerEnable(ProcessAction PA)
        {
            Process_EnableController Cmd = PA as Process_EnableController;
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            // not sure how the controller works - do we want user to have ability to enable & disable?
            // is this how you do it?
            if (mVM.GetBooleanFromText(Cmd.EnableController))
            {
                //				if (DE02.Initialize() != 0) return 1;	
            }
            else
            {

            }

            return 0;
        }

        private int DoInspectTips(ProcessAction PA)
        {
            Process_InspectTipFiring Cmd = PA as Process_InspectTipFiring;
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            int TipMask = mVM.GetIntFromText(Cmd.TipMask);
            int TestMask;
            bool NoPointSpecified;
            MachineCoordinate Pt = GetPoint(Cmd.Tip1Point, Cmd.XOffset, Cmd.YOffset, Cmd.ZOffset, out NoPointSpecified);

            if (NoPointSpecified) throw new Exception("Must specify a point for tip inspection");

            MessageBox.Show("DoInspectTips - Command needs programming work.  Do not use this command", "Error");
            return 0;  // TODO

            for (int Tip = 0; Tip < ThisRobot.TIP_COUNT; ++Tip)
            {
                TestMask = 1 << Tip;

                // is this tip selected?
                if ((TestMask & TipMask) == TestMask)
                {
                    // move above the point first
                    if (Pixy.MotionControl.MoveAbove(Pixy, Pixy.MachineParameters.Tips[Tip], Pixy.MachineParameters.SideCamInspectPoint, Pixy.MotionControl.DefaultSpeed_pct, true, true) != 0) return 1;

                    // move to the inspection point
                    if (Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Tip], Pixy.MachineParameters.SideCamInspectPoint, 0, Pixy.MotionControl.DefaultSpeed_pct, true, true) != 0) return 1;

                    /*
					using (InspectForm dialog = new InspectForm())
					{
						if (dialog.Show(Pixy.MotionControl, Pixy.FiringControl) != 0)
						{
							// pk25May Aborted - try to move up anyhow (camera is a dangerous place)
							if (Pixy.MotionControl.MoveZToSafeHeight(Pixy, true) != 0) return 1;
							return 1;
						}
					}
					*/

                    // pk25may  - Always move away safely.
                    if (Pixy.MotionControl.MoveZToSafeHeight(Pixy, true) != 0) return 1;

                    // TODO get user OK or abort

                }
            }

            return 0;
        }

        private int DoPiezoDispense(ProcessAction PA)               // 20Feb2008 - Big updates here.
        {
            return 0;
            // 13FEB2012, General cleanup.  Getting rid of all DE02 stuff.
            //Process_PiezoDispense Cmd = PA as Process_PiezoDispense;
            //if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            //int					Tip = mVM.GetIntFromText(Cmd.Tip);
            //bool				NoPointSpecified;
            //MachineCoordinate	Pt = GetPoint(Cmd.Tip1Point, Cmd.XOffset, Cmd.YOffset, "", out NoPointSpecified);
            //double				DispenseHeight = 0.0;
            //double				MoveHeight = 0.0;

            //if ((Tip < 1) || (Tip > Pixy.MachineParameters.Tips.Length)) throw new Exception("Invalid tip number in piezo dispense command: " + Cmd.Tip);

            //// leave this optional 03-03-07
            ////if (NoPointSpecified) throw new Exception("No point specified in piezo dispense command");

            //if (!NoPointSpecified)
            //{
            //    // get heights
            //    if (Cmd.MoveHeightAboveSurface != "") MoveHeight = mVM.GetDoubleFromText(Cmd.MoveHeightAboveSurface);
            //    if (Cmd.ZOffset != "") DispenseHeight = mVM.GetDoubleFromText(Cmd.ZOffset);

            //    // check heights
            //    if (DispenseHeight < 0.0) throw new Exception("Must specify a z offset > 0 for piezo dispense");
            //    if (MoveHeight < 0.0) throw new Exception("Move height must be greater than zero");

            //    // set z height for the move
            //    Pt.Z = (Cmd.MoveHeightAboveSurface != "") ? Pt.Z = (Pt.Z + MoveHeight) : (Pixy.MotionControl.ZSafe);

            //    // start the move there - don't wait for move to finish
            //    if (Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Tip-1], Pt, 0.0, Pixy.MotionControl.DefaultSpeed_pct, true, false) != 0) return 1;
            //}

            //// setup the waveform while moving
            //DE02WaveCosParameters waveform = new DE02WaveCosParameters(Tip);

            //waveform.freqOfBursts_hz = mVM.GetIntFromText(Cmd.FreqOfBursts);
            //waveform.numberOfBursts = mVM.GetIntFromText(Cmd.NumBursts);
            //waveform.numberOfDropsPerBurst = mVM.GetIntFromText(Cmd.DropsPerBurst);
            //waveform.amplitude = mVM.GetIntFromText(Cmd.PiezoAmplitude);
            //waveform.freqInBurst_hz = mVM.GetIntFromText(Cmd.PiezoFreq);


            //if (DE02.CheckWaveCosParameters(waveform) !=0) return 1;
            //if (DE02.UpdateWaveformDuration(ref waveform) !=0) return 1;

            //// Set the bulk of the parameters in the DE02, includes amplitude.
            //if (DE02.SetWaveform(waveform)!=0) return 1;

            //if (!NoPointSpecified)
            //{

            //    // make sure the move is done with parameters that will check to see if it's within a certain window..
            //    if (Pixy.MotionControl.WaitForEndOfMotion(ServoControl.DEFAULT_MOVE_TIMEOUT_SEC,Pixy.MachineParameters.Tips[Tip-1], Pt) != 0) return 1;


            //    // if we weren't moving at the dispense height then move there now
            //    if (MoveHeight != DispenseHeight)
            //    {
            //        Pt = GetPoint(Cmd.Tip1Point, Cmd.XOffset, Cmd.YOffset, Cmd.ZOffset, out NoPointSpecified);
            //        if (Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Tip-1], Pt, 0.0, Pixy.MotionControl.DefaultSpeed_pct, true, true) != 0) return 1;
            //    }
            //}

            //string message = string.Format("Dispensing with tip {0}", Tip);
            //UserDatalog(message);  
            //lbRunInfo2.Text = message;               // Message might look cool.

            //// pk25may - start to fire and wait for completion..  
            //if (DE02.FireWave(waveform,true) !=0) return 1;

            //return 0;
        }

        // PK new process

        private int DoDE03CosSetup(ProcessAction PA)

        {


            Process_DE03CosSetup Cmd = PA as Process_DE03CosSetup;
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            int Tip = mVM.GetIntFromText(Cmd.Tip);
            int DesiredFreq = mVM.GetIntFromText(Cmd.DesiredFreq);
            int InBurstNo = mVM.GetIntFromText(Cmd.InBurstNo);
            int CosAmplitude = mVM.GetIntFromText(Cmd.CosAmplitude);
            int StrobeDuration = mVM.GetIntFromText(Cmd.StrobeDuration);
            int StrobeDelay = mVM.GetIntFromText(Cmd.StrobeDelay);
            int NumOfBursts = mVM.GetIntFromText(Cmd.NumOfBursts);
            int TriggerSetting = mVM.GetIntFromText(Cmd.TriggerSetting);
            int TriggerDelay = mVM.GetIntFromText(Cmd.TriggerDelay);
            int TriggerPeriod = mVM.GetIntFromText(Cmd.TriggerPeriod);

            return DE03.CosSetup(Tip, DesiredFreq, InBurstNo, CosAmplitude, StrobeDuration, StrobeDelay,
                NumOfBursts, TriggerSetting, TriggerDelay,
                TriggerPeriod);

        }

        private int DoDE03TrapSetup(ProcessAction PA)

        {


            Process_DE03TrapSetup Cmd = PA as Process_DE03TrapSetup;
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            int Tip = mVM.GetIntFromText(Cmd.Tip);
            int Leading = mVM.GetIntFromText(Cmd.Leading);
            int Dwell = mVM.GetIntFromText(Cmd.Dwell);
            int Trailing = mVM.GetIntFromText(Cmd.Trailing);
            int TrapDrops = mVM.GetIntFromText(Cmd.TrapDrops);
            int TrapFreq = mVM.GetIntFromText(Cmd.TrapFreq);
            int TrapAmp = mVM.GetIntFromText(Cmd.TrapAmp);

            int StrobeDuration = mVM.GetIntFromText(Cmd.StrobeDuration);
            int StrobeDelay = mVM.GetIntFromText(Cmd.StrobeDelay);
            int TriggerSetting = mVM.GetIntFromText(Cmd.TriggerSetting);
            int TriggerDelay = mVM.GetIntFromText(Cmd.TriggerDelay);
            int TriggerPeriod = mVM.GetIntFromText(Cmd.TriggerPeriod);

            return DE03.TrapSetup(Tip, Leading, Dwell, Trailing, TrapDrops, TrapFreq, TrapAmp,
             StrobeDelay, StrobeDuration,
             TriggerSetting, TriggerDelay, TriggerPeriod);

        }

        private int DoDE03StartWaveform(ProcessAction PA)

        {


            Process_DE03StartWaveform Cmd = PA as Process_DE03StartWaveform;
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            bool WaitForCompletion = mVM.GetBooleanFromText(Cmd.WaitForCompletion);
            int Timeout_ms = mVM.GetIntFromText(Cmd.Timeout_ms);

            return DE03.StartWaveform(WaitForCompletion, Timeout_ms);

        }

        private int DoDE03StopWaveform(ProcessAction PA)

        {

            Process_DE03StopWaveform Cmd = PA as Process_DE03StopWaveform;
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            return DE03.StopWaveform();

        }

        private int DOIOSetOutput(ProcessAction PA)
        {

            Process_IOSetOutput Cmd = PA as Process_IOSetOutput;
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            int IONumber = mVM.GetIntFromText(Cmd.IONumber);

            return IO.SetOutput(IONumber);

        }

        private int DOIOWaitInput(ProcessAction PA)
        {

            Process_IOWaitInput Cmd = PA as Process_IOWaitInput;
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            int IONumber = mVM.GetIntFromText(Cmd.IONumber);
            int timeOut_ms = mVM.GetIntFromText(Cmd.TimeOut_ms);

            return IO.WaitInput(IONumber, timeOut_ms);

        }

        private int DoSuperPlunge(ProcessAction PA)
        {
            return 0;
            /*
            Process_SuperPlunge Cmd = PA as Process_SuperPlunge;
            if (Cmd == null) throw new Exception("Unexpected command type in DoPlungeAxisMove: " + PA.Name);
 
            bool skipMove=true;
            double MovePos_mm=0.0;
            double MoveSpeed_pct=10.0;
            int StartSolenoid = mVM.GetIntFromText(Cmd.StartSolenoid);
            int StartSensor = mVM.GetIntFromText(Cmd.StartSensor);
            int EndSensor = mVM.GetIntFromText(Cmd.EndSensor);
            if (Cmd.PositionZ2!="")
            {
                MovePos_mm = mVM.GetDoubleFromText(Cmd.PositionZ2);
                if (MovePos_mm > -900) skipMove = false;
                MoveSpeed_pct = GetMoveSpeed_pct(Cmd.Speed_pct);
            }

            Datalog log;
            int st = System.Environment.TickCount;   // Easier to read timing,  all in msec
            int et;

            #region Is datalogging significantly slowing things down ?  Test Code to study this.
  //          log = new Datalog(@"C:\DE03 Syringe Script App\LogFiles\SuperPlungeLog.txt", true);
  //          log.WriteLine("-------- Entering SuperPlunge Command ----------");
  //          log.WriteLine("Start:" + Datalog.Timestamp());
  //          int i,j;
  //          for (i = 0; i < 10; i++)
  //          {
  //              et = System.Environment.TickCount - st;
  //              log.WriteLine("In the loop:" + et.ToString());
  //              for (j = 0; j < 1000000; j++)
  //              {
  //                  // Nutin
  //              }
  //          }
  //          et = System.Environment.TickCount - st;
  //          log.WriteLine("End of Loop1:" + et.ToString());
  //          et = System.Environment.TickCount - st;
  //          log.WriteLine("Start Loop2:" + et.ToString());
  //          for (i = 0; i < 10; i++)
  //          {
  ////            log.WriteLine("In the loop:" + Datalog.Timestamp());
  //              for (j = 0; j < 1000000; j++)
  //              {
  //                  // Nutin
  //              }
  //          }
  //          et = System.Environment.TickCount - st;
  //          log.WriteLine("End Loop2:" + et.ToString());
  //          log.Close();
  //          return 0;
            #endregion
 
            try
            {
                log = new Datalog(@"C:\DE03 Syringe Script App\LogFiles\SuperPlungeLog.txt", true);
                log.WriteLine("-------- Entering SuperPlunge Command ----------");
                log.WriteLine("Start:"+Datalog.Timestamp());
                et = System.Environment.TickCount-st;
                log.WriteLine("Elapsed Time:"+et.ToString());
                log.WriteLine("   StartSolenoid={0}, StartSensor={1}, EndSensor={2}, skipMove={3}",StartSolenoid,StartSensor,EndSensor,skipMove);
                if (!skipMove)
                {
                    log.WriteLine("   MovePos_mm={0}, MoveSpeed_pct={1}", MovePos_mm, MoveSpeed_pct);
                }
            }
            catch
            { 
                MessageBox.Show("MainForm.cs: DoSuperPlunge  Unable to open log file");
                return -1;
            }

  
            // find out if user motion control was allowed
            bool AllowingManualControl = Pixy.MotionControl.ManualControlEnabled;

            try
            {
          
                // lock out the user motion control
                Pixy.MotionControl.EnableManualControl(Pixy, false);

                // Fire the start solenoid
                if (StartSolenoid>-900)
                {
                    IO.SetOutput(StartSolenoid);
                    et = System.Environment.TickCount-st;
                    log.WriteLine("Elapsed Time: "+et.ToString());
                }

                // Wait for the input sensor to fire
                if (StartSensor>-900)
                {
                    int ret=IO.WaitInput(StartSensor,10000);        //  Will wait up to 10 sec for sensor to fire
                    et = System.Environment.TickCount-st;
                    log.WriteLine("StartSensor: "+et.ToString());
                    if (ret!=0) 
                    {
                        log.WriteLine("StartSensor Timeout");
                        return ret;
                    }
                }
                
                bool moveDone = false;
                // Start the optional move
                if (skipMove)
                {
                    moveDone=true;
                }
                else
                {
                    if (Pixy.MotionControl.MoveZ2Only(Pixy, MovePos_mm, MoveSpeed_pct, false) != 0) return 1;
                    et = System.Environment.TickCount - st;
                    log.WriteLine("StartMotion: " + et.ToString());
                }
 
                bool ioDone = false;
                double actualPosition;

                if (EndSensor < -900)   // Check to see if we will even look for the end sensor
                {
                    ioDone = true;
                }

                int startMotionTs = System.Environment.TickCount;
                do
                {
                    if (!moveDone)
                    {
                        if (Pixy.MotionControl.IsZ2MoveDone())
                        //Pixy.MotionControl.GetCurrentPosition(PixyControl.ServoControl.Z2_AXIS,out actualPosition);
                        //if (Math.Abs(actualPosition-MovePos_mm)<0.1)   // Close enough to end point ??
                        {
                            moveDone = true;
                            et = System.Environment.TickCount-st;
                            log.WriteLine("EndMotion: "+et.ToString());
                            et = System.Environment.TickCount - startMotionTs;
                            log.WriteLine("EndMotion - StartMotion: " + et.ToString());
                        }
                    }
                    if (!ioDone)
                    {
     

                        if (IO.ReadInput(EndSensor))
                        {
                            ioDone=true;
                            et = System.Environment.TickCount-st;
                            log.WriteLine("EndSensor: "+et.ToString());
                            et = System.Environment.TickCount - startMotionTs;
                            log.WriteLine("EndSensor - StartMotion: " + et.ToString());
                        }
                    }
                    if ((System.Environment.TickCount-startMotionTs)>20000)             // 20 seconds
                    {
                        string mess1 = string.Format("Error Time Out,  Motion Done={0}, End Sensor Found ={1}", moveDone, ioDone);
                        log.WriteLine(mess1);
                        MessageBox.Show(mess1);
                        return 1;
                    }
                    if (ioDone && moveDone) return 0;

                } while (true);

           }
           finally
           {
                Pixy.MotionControl.WaitForEndOfZ2MotionOnly(1000);    // Cleanup all motion variables (move flag etc..)
                // allow user control again if it was allowed before
                Pixy.MotionControl.EnableManualControl(Pixy, AllowingManualControl);
                log.Close();
            }
             */
        }


        private int DoPlungeAxisMove(ProcessAction PA)
        {
            return 0;
            /*
            Process_PlungeAxisMove Cmd = PA as Process_PlungeAxisMove;
            if (Cmd == null) throw new Exception("Unexpected command type in DoPlungeAxisMove: " + PA.Name);
 
            double MovePos_mm = mVM.GetDoubleFromText(Cmd.PositionZ2);

            double MoveSpeed_pct = GetMoveSpeed_pct(Cmd.Speed_pct);
            if (Cmd.ZOffset != "") MovePos_mm = MovePos_mm + mVM.GetDoubleFromText(Cmd.ZOffset);
 
            // find out if user motion control was allowed
            bool AllowingManualControl = Pixy.MotionControl.ManualControlEnabled;

            try
            {
                // lock out the user motion control
                Pixy.MotionControl.EnableManualControl(Pixy, false);

                // say that we're moving
                ShowStatusMessage("Moving Plunge Axis to " + Cmd.PositionZ2);

                // then do the move

                if (Pixy.MotionControl.MoveZ2Only(Pixy, MovePos_mm, MoveSpeed_pct, true) != 0) return 1;
 //               if (Pixy.MotionControl.MoveSafely(Pixy, Tool, MachinePt, 0.0, MoveSpeed_pct, true, true) != 0) return 1;
            }
            finally
            {
                // allow user control again if it was allowed before
                Pixy.MotionControl.EnableManualControl(Pixy, AllowingManualControl);
            }
            */

            return 0;
        }


        private int DoMotionInitialize(ProcessAction PA)
        {
            Process_InitializeMotion Cmd = PA as Process_InitializeMotion;
            if (Cmd == null) throw new Exception("Unexpected command type in DoMotionInitialize: " + PA.Name);

            return Pixy.MotionControl.InitializeMotion(Pixy, false);
        }

        private int DoServoEnable(ProcessAction PA)
        {
            Process_ServoEnable Cmd = PA as Process_ServoEnable;
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            int Axis = mVM.GetIntFromText(Cmd.AxisNumber);

            if ((Axis < 0) || (Axis >= 4)) throw new Exception("Invalid axis: " + Axis + ". Must be 0, 1, 2, or 3");

            return Pixy.MotionControl.EnableAxis(Pixy, Axis, true);
        }

        private int DoServoDisable(ProcessAction PA)
        {
            Process_ServoDisable Cmd = PA as Process_ServoDisable;
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            int Axis = mVM.GetIntFromText(Cmd.AxisNumber);

            if ((Axis < 0) || (Axis >= 4)) throw new Exception("Invalid axis: " + Axis + ". Must be 0, 1, 2, or 3");

            return Pixy.MotionControl.EnableAxis(Pixy, Axis, false);
        }

        private int DoHomeAxis(ProcessAction PA)
        {
            Process_HomeAxis Cmd = PA as Process_HomeAxis;
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            int Axis = mVM.GetIntFromText(Cmd.AxisNumber);

            if ((Axis < 0) || (Axis >= 3)) throw new Exception("Invalid axis: " + Axis + ". Must be 0, 1, or 2");

            return Pixy.MotionControl.HomeAxis(Pixy, Axis);
        }

        private int DoSetMaxVelocity(ProcessAction PA)
        {
            Process_SetMaxVelocity Cmd = PA as Process_SetMaxVelocity;
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            bool UseGridRobot = false;
            if (Cmd.UseGridRobot != "") UseGridRobot = mVM.GetBooleanFromText(Cmd.UseGridRobot);

            int Axis = mVM.GetIntFromText(Cmd.AxisNumber);

            if ((Axis < 0) || (Axis >= 3)) throw new Exception("Invalid axis: " + Axis + ". Must be 0, 1, or 2");

            double MaxVelocity = mVM.GetDoubleFromText(Cmd.MaxVelocity);

            return Pixy.MotionControl.SetMaxVelocity(Pixy, Axis, MaxVelocity, UseGridRobot);
        }

        private int DoSetAccel(ProcessAction PA)
        {
            Process_SetAccel Cmd = PA as Process_SetAccel;
            bool UseGridRobot = false;
            if (Cmd.UseGridRobot != "") UseGridRobot = mVM.GetBooleanFromText(Cmd.UseGridRobot);
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            int Axis = mVM.GetIntFromText(Cmd.AxisNumber);

            if ((Axis < 0) || (Axis >= 3)) throw new Exception("Invalid axis: " + Axis + ". Must be 0, 1, or 2");


            double Accel = mVM.GetDoubleFromText(Cmd.Accel);
            double Decel = Accel;
            if (Cmd.Decel != "") Decel = mVM.GetDoubleFromText(Cmd.Decel);

            return Pixy.MotionControl.SetAccel(Pixy, Axis, Accel, Decel, UseGridRobot);
        }

        //PKv4.0,2015-04-28, 
        // Sets the motion parameters V,A,D back to defaults in the XML file.

        private int DoSetDefaultMotionParmaters(ProcessAction PA)
        {
            Process_SetDefaultMotionParameters Cmd = PA as Process_SetDefaultMotionParameters;
            bool UseGridRobot = false;
            if (Cmd.UseGridRobot != "") UseGridRobot = mVM.GetBooleanFromText(Cmd.UseGridRobot);
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            if (Cmd.AxisNumber == "")
            {
                return Pixy.MotionControl.SetDefaultMotionParameters(Pixy, UseGridRobot);
            }

            int Axis = mVM.GetIntFromText(Cmd.AxisNumber);
            if ((Axis < 0) || (Axis >= 3)) throw new Exception("Invalid axis: " + Axis + ". Must be 0, 1, or 2");

            return Pixy.MotionControl.SetDefaultMotionParameters(Pixy, Axis, UseGridRobot);
        }

        //PKv4.0,2015-04-20  Updated to work with UseGridRobot parameter.

        private int DoMoveRelative(ProcessAction PA)
        {
            Process_MoveRelative Cmd = PA as Process_MoveRelative;
            if (Cmd == null) throw new Exception("Unexpected command type in DoMoveRelative: " + PA.Name);

            bool UseGridRobot = false;
            if (Cmd.UseGridRobot != "") UseGridRobot = mVM.GetBooleanFromText(Cmd.UseGridRobot);

            MachineCoordinate Pt = new MachineCoordinate();

            if (Pixy.MotionControl.GetCurrentPosition(out Pt, UseGridRobot) != 0) return 1;

            if (Cmd.XOffset != "") Pt.X = Pt.X + mVM.GetDoubleFromText(Cmd.XOffset);
            if (Cmd.YOffset != "") Pt.Y = Pt.Y + mVM.GetDoubleFromText(Cmd.YOffset);
            if (Cmd.ZOffset != "") Pt.Z = Pt.Z + mVM.GetDoubleFromText(Cmd.ZOffset);

            return Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[0], Pt, 0.0, GetMoveSpeed_pct(Cmd.Speed_pct), true, true, UseGridRobot, false);

        }

        private int DoMoveToSafeHeight(ProcessAction PA)
        {
            Process_MoveToSafeHeight Cmd = PA as Process_MoveToSafeHeight;
            if (Cmd == null) throw new Exception("Unexpected command type in DoMoveToSafeHeight: " + PA.Name);

            bool UseGridRobot = false;
            if (Cmd.UseGridRobot != "") UseGridRobot = mVM.GetBooleanFromText(Cmd.UseGridRobot);

            return Pixy.MotionControl.MoveZToSafeHeight(Pixy, true, GetMoveSpeed_pct(Cmd.Speed_pct), UseGridRobot);
        }

        private int DoMoveToPoint(ProcessAction PA)
        {
            Process_MoveToPoint Cmd = PA as Process_MoveToPoint;
            if (Cmd == null) throw new Exception("Unexpected command type in DoMoveToPoint: " + PA.Name);

            bool NoPointSpecified;
            MachineCoordinate MachinePt = GetPoint(Cmd.Point, Cmd.XOffset, Cmd.YOffset, Cmd.ZOffset, out NoPointSpecified);
            MachineCoordinate Tool = mVM.GetCoordinateFromText(Cmd.Tool);
            double MoveSpeed_pct = GetMoveSpeed_pct(Cmd.Speed_pct);
            bool MoveToSafeHeightFirst = mVM.GetBooleanFromText(Cmd.MoveToSafeHeightFirst);

            bool UseGridRobot = false;
            if (Cmd.UseGridRobot != "") UseGridRobot = mVM.GetBooleanFromText(Cmd.UseGridRobot);

            if (NoPointSpecified) throw new Exception("No point specified for move command");

            // find out if user motion control was allowed
            bool AllowingManualControl = Pixy.MotionControl.ManualControlEnabled;

            try
            {
                // lock out the user motion control
                Pixy.MotionControl.EnableManualControl(Pixy, false);

                if (MoveToSafeHeightFirst)
                {
                    // first move to a safe height
                    ShowStatusMessage("Move to safe z height");
                    if (Pixy.MotionControl.MoveZToSafeHeight(Pixy, true, MoveSpeed_pct, UseGridRobot) != 0) return 1;

                }

                // say that we're moving
                ShowStatusMessage("Moving to " + Cmd.Point);

                // then do the move
                if (Pixy.MotionControl.MoveSafely(Pixy, Tool, MachinePt, 0.0, MoveSpeed_pct, true, true, UseGridRobot, false) != 0) return 1;
            }
            finally
            {
                // allow user control again if it was allowed before
                Pixy.MotionControl.EnableManualControl(Pixy, AllowingManualControl);
            }

            return 0;
        }

        private int DoMoveAbovePoint(ProcessAction PA)
        {
            Process_MoveAbovePoint Cmd = PA as Process_MoveAbovePoint;
            if (Cmd == null) throw new Exception("Unexpected command type in DoMoveAbovePoint: " + PA.Name);

            bool NoPointSpecified;
            MachineCoordinate MachinePt = GetPoint(Cmd.Point, Cmd.XOffset, Cmd.YOffset, "", out NoPointSpecified);
            MachineCoordinate Tool = mVM.GetCoordinateFromText(Cmd.Tool);
            double MoveSpeed_pct = GetMoveSpeed_pct(Cmd.Speed_pct);
            bool UseGridRobot = false;
            if (Cmd.UseGridRobot != "") UseGridRobot = mVM.GetBooleanFromText(Cmd.UseGridRobot);

            if (NoPointSpecified) throw new Exception("No point specified for move command");

            // find out if user motion control was allowed
            bool AllowingManualControl = Pixy.MotionControl.ManualControlEnabled;

            try
            {
                // lock out the user motion control
                Pixy.MotionControl.EnableManualControl(Pixy, false);

                ShowStatusMessage("Move above " + Cmd.Point);

                // then do the move
                if (Pixy.MotionControl.MoveAbove(Pixy, Tool, MachinePt, MoveSpeed_pct, true, true) != 0) return 1;
            }
            finally
            {
                // allow user control again if it was allowed before
                Pixy.MotionControl.EnableManualControl(Pixy, AllowingManualControl);
            }

            return 0;
        }

        // [LEA COMMAND DEF]
        private int DoDefineImport(ProcessAction PA)
        {
            Variable_DefineImport importAction = PA as Variable_DefineImport;

            if (importAction == null)
                throw new Exception("Unexpected command type in DoDefineImport: " + PA.Name);

            SequenceFile.ReadImportVariables(importAction, mVM);

            return 0;
        }

        // Will be called from ShowTestCommandWindow.
        // [LEA COMMAND DEF]
        private void AddImportVariables(VariableManager vm)
        {
            if (this.mSeqFile == null) return;

            SequenceFile.PreloadImportVariables(mCommandList, vm);
        }

        private int DoRotateMove(ProcessAction PA)          //PKv4.0,2015-04-13  Need to update
        {
            Process_RotateMove Cmd = PA as Process_RotateMove;
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            int positionCount = mVM.GetIntFromText(Cmd.PositionCount);

            return HarmonicDrive.Move(positionCount, true);
        }


        private int DoRotateSetup(ProcessAction PA)          //PKv4.0,2015-04-13  Need to update
        {
            Process_RotateSetup Cmd = PA as Process_RotateSetup;
            if (Cmd == null) throw new Exception("Unexpected command type: " + PA.Name);

            int accelCount = mVM.GetIntFromText(Cmd.AccelCount);
            int decelCount = mVM.GetIntFromText(Cmd.DecelCount);
            int maxVelCount = mVM.GetIntFromText(Cmd.MaxVelCount);

            return HarmonicDrive.SetParameters(maxVelCount, accelCount, decelCount);
        }


        #endregion Implementation of ProcessActions

        #region sequence file stuff

        //////////////////////////////////////////////////////////////////////////////////
        //
        //
        //////////////////////////////////////////////////////////////////////////////////

        private int OpenNewWorklist()
        {
            if (this.mSequenceChanged)
            {
                System.Windows.Forms.DialogResult Rslt = MessageBox.Show("Would you like to save changes first?", "Save Changes", System.Windows.Forms.MessageBoxButtons.YesNoCancel);

                if (Rslt == DialogResult.Cancel) return -1;
                if (Rslt == DialogResult.Yes) SaveSequenceToFile();
            }

            OpenFileDialog OpenDlg = new OpenFileDialog();

            //			OpenDlg.InitialDirectory = "c:\\" ;  
            OpenDlg.InitialDirectory = @"..\recipe\";    // pk14Feb2008
            OpenDlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            OpenDlg.FilterIndex = 2;
            OpenDlg.RestoreDirectory = true;
            OpenDlg.Multiselect = false;

            if (OpenDlg.ShowDialog() != DialogResult.OK) return -1;

            LoadSequenceFile(OpenDlg.FileName);
            return 0;
        }


        private void LoadSequenceFile(string FileName)
        {
            Clear();

            mCommandList = new ArrayList();
            mVM.Clear();
            mSeqFile = new SequenceFile();
            mSequenceChanged = false;

            // event handlers for sequence file execution
            mSeqFile.NewCommand += new NewCommandEventHandler(mSeqFile_NewCommand);
            mSeqFile.SequenceComplete += new SequenceCompleteEventHandler(mSeqFile_SequenceComplete);
            mSeqFile.VariableChanged += new VariableChangedEventHandler(mSeqFile_VariableChanged);

            SetState(PixyControl.frmMain.GUI_STATE.IdleNoSequence);

            if (mSeqFile.Read(FileName, mCommandList, mVM) != -1)
            {
                SetState(PixyControl.frmMain.GUI_STATE.IdleSequenceLoaded);
            }

            //	ShowCommandList();
        }

        private void SaveSequenceToFile()
        {
            if (this.mSeqFile == null) return;

            if ((mSeqFile.FileName == null) || (mSeqFile.FileName == ""))
            {
                if (SaveSequenceAs() == 0) mSequenceChanged = false;
            }
            else
            {
                if (mSeqFile.Write(this.mSeqFile.FileName) > 0) mSequenceChanged = false;  //pk15Feb2008 .
            }

            SetTitleText();
        }

        private int SaveSequenceAs()
        {
            if (this.mSeqFile == null) return -1;

            SaveFileDialog SaveDlg = new SaveFileDialog();

            SaveDlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            SaveDlg.FilterIndex = 2;
            SaveDlg.RestoreDirectory = true;
            SaveDlg.OverwritePrompt = true;

            if (SaveDlg.ShowDialog() != DialogResult.OK) return -1;

            if (this.mSeqFile.Write(SaveDlg.FileName) != 0) return -1;

            LoadSequenceFile(SaveDlg.FileName);

            return 0;
        }

        private void GetActionOperations(ProcessActionOpList ActionOpList)
        {
            ActionOpList.Clear();

            ActionOpList.Add(SequenceFile.CommandNames.Prime, new ProcessActionOp(this.DoPrime));
            ActionOpList.Add(SequenceFile.CommandNames.Aspirate, new ProcessActionOp(this.DoAspirate));
            ActionOpList.Add(SequenceFile.CommandNames.Dispense, new ProcessActionOp(this.DoDispense));
            ActionOpList.Add(SequenceFile.CommandNames.SyringeSetValve, new ProcessActionOp(this.DoSyringeSetValvePosition));
            ActionOpList.Add(SequenceFile.CommandNames.SyringeMove, new ProcessActionOp(this.DoSyringeMove));
            ActionOpList.Add(SequenceFile.CommandNames.SyringeEmpty, new ProcessActionOp(this.DoSyringeEmpty));
            ActionOpList.Add(SequenceFile.CommandNames.WashTips, new ProcessActionOp(this.DoWashTips));
            ActionOpList.Add(SequenceFile.CommandNames.ControllerEnable, new ProcessActionOp(this.DoControllerEnable));
            ActionOpList.Add(SequenceFile.CommandNames.InspectTips, new ProcessActionOp(this.DoInspectTips));
            ActionOpList.Add(SequenceFile.CommandNames.PiezoDispense, new ProcessActionOp(this.DoPiezoDispense));
            ActionOpList.Add(SequenceFile.CommandNames.MotionInitialize, new ProcessActionOp(this.DoMotionInitialize));
            ActionOpList.Add(SequenceFile.CommandNames.HomeAxis, new ProcessActionOp(this.DoHomeAxis));
            ActionOpList.Add(SequenceFile.CommandNames.ServoEnable, new ProcessActionOp(this.DoServoEnable));
            ActionOpList.Add(SequenceFile.CommandNames.ServoDisable, new ProcessActionOp(this.DoServoDisable));
            ActionOpList.Add(SequenceFile.CommandNames.MoveRelative, new ProcessActionOp(this.DoMoveRelative));
            ActionOpList.Add(SequenceFile.CommandNames.MoveToSafeHeight, new ProcessActionOp(this.DoMoveToSafeHeight));
            ActionOpList.Add(SequenceFile.CommandNames.MoveToPoint, new ProcessActionOp(this.DoMoveToPoint));
            ActionOpList.Add(SequenceFile.CommandNames.MoveAbovePoint, new ProcessActionOp(this.DoMoveAbovePoint));

            ActionOpList.Add(SequenceFile.CommandNames.SetMaxVelocity, new ProcessActionOp(this.DoSetMaxVelocity));
            ActionOpList.Add(SequenceFile.CommandNames.SetAccel, new ProcessActionOp(this.DoSetAccel));
            ActionOpList.Add(SequenceFile.CommandNames.SetDefaultMotionParameters, new ProcessActionOp(this.DoSetDefaultMotionParmaters));


            // [LEA COMMAND DEF]   add new commands here
            ActionOpList.Add(SequenceFile.CommandNames.DefineImport, new ProcessActionOp(this.DoDefineImport));   // [LEA COMMAND DEF]
            ActionOpList.Add(SequenceFile.CommandNames.DE03CosSetup, new ProcessActionOp(this.DoDE03CosSetup));   // [PK new command]
            ActionOpList.Add(SequenceFile.CommandNames.DE03StartWaveform, new ProcessActionOp(this.DoDE03StartWaveform)); // [PK new command]
            ActionOpList.Add(SequenceFile.CommandNames.DE03StopWaveform, new ProcessActionOp(this.DoDE03StopWaveform));  // [PK new command]
            ActionOpList.Add(SequenceFile.CommandNames.DE03TrapSetup, new ProcessActionOp(this.DoDE03TrapSetup));   // [PK new command]

            ActionOpList.Add(SequenceFile.CommandNames.IOSetOutput, new ProcessActionOp(this.DOIOSetOutput));   // [PK high level command for IO]
            ActionOpList.Add(SequenceFile.CommandNames.IOWaitInput, new ProcessActionOp(this.DOIOWaitInput));   // [PK high level command for IO]

            ActionOpList.Add(SequenceFile.CommandNames.PlungeAxisMove, new ProcessActionOp(this.DoPlungeAxisMove));   // [Plunge axis only move]
            ActionOpList.Add(SequenceFile.CommandNames.SuperPlunge, new ProcessActionOp(this.DoSuperPlunge));   // [Plunge axis only move]

            ActionOpList.Add(SequenceFile.CommandNames.RotateMove, new ProcessActionOp(this.DoRotateMove));   // [Move Rotate Axis]  //PKv4.0,2015-04-13
            ActionOpList.Add(SequenceFile.CommandNames.RotateSetup, new ProcessActionOp(this.DoRotateSetup));   // [Setup Rotate Axis] //PKv4.0,2015-04-13

        }

        private int MakeSureMachineIsInitializedBeforeSequence()
        {
            try
            {
                if (!Pixy.MotionControl.HardwareInitialized)
                {
                    if (Pixy.MotionControl.InitializeMotion(Pixy, false) != 0) return 1;
                }

                // maybe we should search the sequence file and only initialize necessary pumps  //Syringe-TODO
                //				if (!Pixy.SyringeControl.Initialized)
                //				{
                //					if (Pixy.SyringeControl.InitializePump(UserSyringeControl.BOTH_PUMP) != 0) return 1;
                //				}

            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error initializing machine.\n" + Ex.Message);
                return 1;
            }

            return 0;
        }


        private void StartSequence(bool DryRunOnly)
        {
            if (this.mSeqFile == null) return;

            // checks for motion and syringe init       // TODO - Need to put this back in 
            //		if (MakeSureMachineIsInitializedBeforeSequence() != 0) return;

            // don't let user execute commands while a sequence is running
            // need to at least close the test command window - wouldn't hurt to close them all
            CloseToolWindows();             // TODO - Make sure DIO window is closed too.

            // disable some buttons
            SetState(PixyControl.frmMain.GUI_STATE.Running);

            this.bAbort = false;
            this.bPause = false;

            // pass the sequence file engine a list of methods to call for 
            // any command types we want
            // no more than one method per command type though
            ProcessActionOpList ActionOpList = null;

            // if it's dry run mode, we won't pass it any actions
            // it will just run through the sequence and write debug text in the log file
            if (!DryRunOnly)
            {
                ActionOpList = new ProcessActionOpList();
                GetActionOperations(ActionOpList);
            }

            // can't pass parameters in when starting a thread
            // so give the sequence file this information first
            mSeqFile.LogFile = this.CurrentDatalogFile;
            mSeqFile.OperationList = ActionOpList;

            // clear out any old variables
            this.mVM.Clear();

            // add in system variables
            AddSystemVariables(this.mVM);

            try
            {
                // run execution in its own thread so that we don't miss GUI events
                ThreadStart EntryPoint = new ThreadStart(mSeqFile.ExecuteSequence);
                mExecutionThread = new Thread(EntryPoint);
                mExecutionThread.Name = "SequenceExecution";
                mExecutionThread.Start();
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error starting sequence execution\n" + Ex.Message);
                SetState(PixyControl.frmMain.GUI_STATE.IdleSequenceLoaded);
            }

            // state gets set back to idle when sequence file fires its SequenceEnd event as it exits
        }

        private void AddSystemVariables(VariableManager VM)
        {
            VM.DefineVariable(SystemVariableNames.SafePoint, Pixy.MachineParameters.SafePoint);
            VM.DefineVariable(SystemVariableNames.SideCamInspectPoint, Pixy.MachineParameters.SideCamInspectPoint);  //pk15Feb2008
            VM.DefineVariable(SystemVariableNames.WashPoint, Pixy.MachineParameters.WashPoint);  //pk15Feb2008

            //			VM.DefineVariable(SystemVariableNames.BackRightPoint, Pixy.MachineParameters.BackRight);
            //			VM.DefineVariable(SystemVariableNames.DeckOriginPoint, Pixy.MachineParameters.DispenseDeck);
            //			VM.DefineVariable(SystemVariableNames.MicrotiterPoint, Pixy.MachineParameters.Microtiter);
            //			VM.DefineVariable(SystemVariableNames.TipInspectionPoint, Pixy.MachineParameters.SideCamInspectPoint);
            //			VM.DefineVariable(SystemVariableNames.TipWashPoint, Pixy.MachineParameters.TipWash);
            //			VM.DefineVariable(SystemVariableNames.ReservoirPoint, Pixy.MachineParameters.Reservoir);

            for (int i = 0; i < Pixy.MachineParameters.Tips.Length; ++i)
            {
                VM.DefineVariable("System_Tip" + (i + 1), Pixy.MachineParameters.Tips[i]);
            }

        }

        private void AddSequenceVariables(VariableManager VM)
        {
            if (this.mSeqFile == null) return;

            SequenceFile.GetUserVariables(this.mCommandList, VM);

        }

        private void SetStepMode(bool Step)
        {
            this.mStepMode = Step;
            this.mnuStep.Checked = Step;
            //	this.toolBar1.Buttons[ToolbarButtons.StepMode].Pushed = Step;
        }

        private void CheckSequence()
        {
            int BadCmd = 0;
            string ErrMsg = "";

            if (this.mCommandList == null) return;

            this.mVM.Clear();
            AddSystemVariables(mVM);

            if (SequenceFile.SequenceOK(this.mCommandList, this.mVM, out BadCmd, out ErrMsg))
            {
                MessageBox.Show("Sequence OK");
            }
            else
            {
                MessageBox.Show(ErrMsg);
            }

            //if (BadCmd>0) SelectCmdNode(BadCmd-1);

        }

        private void StopNow()
        {
            Pixy.MotionControl.StopAllMotion();

            if ((this.mExecutionThread != null) && (this.mExecutionThread.IsAlive))
            {
                // can't abort a suspended thread - have to resume briefly to throw the thread exception
                if (mExecutionThread.ThreadState == System.Threading.ThreadState.Suspended) mExecutionThread.Resume();
                mExecutionThread.Abort();
                // can take a little while to abort - has to go through any finally statements - wait for it
                mExecutionThread.Join(1000);
            }

            this.bAbort = true;
        }

        private void StopSequence()
        {
            if (this.mExecutionThread == null) return;

            bool StepModeBefore = Pixy.MotionControl.StepMode;

            try
            {
                // stop motion at the next opportunity - may not be moving
                Pixy.MotionControl.StepMode = true;
                Pixy.MotionControl.WaitForEndOfMotion(10);

                // pause execution while we ask a question
                if (this.mExecutionThread.IsAlive) this.mExecutionThread.Suspend();

                // confirm the stop
                if (MessageBox.Show("Are you sure you want to abort execution?", "Abort?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    StopNow();
                    return;
                }

                // didn't stop it, so let it go on
                if (this.mExecutionThread.IsAlive) this.mExecutionThread.Resume();
            }
            finally
            {
                Pixy.MotionControl.StepMode = StepModeBefore;
            }
        }


        private void AddSelectedActionToSequence()
        {
            //	if (cboAction.SelectedItem == null) return;

            // get a new action object
            //	ProcessAction SelectedAction = cboAction.SelectedItem as ProcessAction;
            //ProcessAction PA = SequenceFile.CreateCommandOfSameType(SelectedAction);

            // create a new sequence file and command list if needed
            if (this.mSeqFile == null) this.mSeqFile = new SequenceFile();

            // find out where to insert the action


        }

        #endregion sequence file stuff







        private void ShowInNotepad(string FilePath)
        {
            System.Diagnostics.Process.Start("Notepad.exe", FilePath);
        }

        private void mnuCmdRef_Click(object sender, System.EventArgs e)
        {
            //			string FilePath = @"C:\Pixy_2.0\CommandHelp.txt";
            string FilePath = @"..\data\CommandHelp.txt";

            try
            {
                ProcessAction[] Cmds = SequenceFile.GetProcessActionList();
                StringBuilder SB;
                Type T;
                PropertyInfo[] Properties;
                ProcessActionArgumentAttribute Attrib;
                object[] Attributes;

                using (StreamWriter Writer = new StreamWriter(FilePath, false))
                {
                    foreach (ProcessAction Cmd in Cmds)
                    {
                        SB = new StringBuilder(Cmd.Name);
                        SB.Append(" (NameInCommandFile = ");
                        SB.Append(Cmd.NameInCommandFile);
                        SB.Append(")");
                        Writer.WriteLine(SB.ToString());

                        // get public properties
                        T = Cmd.GetType();
                        Properties = T.GetProperties();

                        foreach (PropertyInfo PropInfo in Properties)
                        {
                            // if the property is a ProcessActionArgument...
                            Attributes = PropInfo.GetCustomAttributes(typeof(ProcessActionArgumentAttribute), true);
                            foreach (object Att in Attributes)
                            {
                                Attrib = Att as ProcessActionArgumentAttribute;
                                if (Att != null)
                                {
                                    SB = new StringBuilder("\t");
                                    SB.Append(PropInfo.Name);
                                    Writer.WriteLine(SB.ToString());

                                    SB = new StringBuilder("\t\t");
                                    SB.Append("Type=");
                                    SB.Append(Attrib.TargetVariableType);
                                    Writer.WriteLine(SB.ToString());

                                    SB = new StringBuilder("\t\t");
                                    SB.Append("Required=");
                                    SB.Append(Attrib.IsRequired.ToString());
                                    Writer.WriteLine(SB.ToString());

                                    SB = new StringBuilder("\t\t");
                                    SB.Append("Description=");
                                    SB.Append(Attrib.Description);
                                    Writer.WriteLine(SB.ToString());

                                }
                            }
                        }

                        Writer.WriteLine("");
                    }
                }

                ShowInNotepad(FilePath);
            }
            catch { }
        }

        private void mnuHelpAbout_Click(object sender, System.EventArgs e)
        {
            aboutMenu();
        }

        private void aboutMenu()
        {
            string message = "Spotiton Application Software \n\n v105TR ,  Time Resolved \n\n Copyright Engineering Arts LLC, 2019\n\n www.engineeringarts.com";
            if (mdsf.RunOnPHXDevSystem)             // PKv5.5.5
            {
                message = message + "\n\n *** Running on Phoenix Development System ****";
            }
            MessageBox.Show(message, "Engineering Arts LLC", MessageBoxButtons.OK);
        }

        private void mnuStrobeOn_Click(object sender, System.EventArgs e)
        {
            //			DE02.StartStrobe();
        }

        private void mnuStrobeOff_Click(object sender, System.EventArgs e)
        {
            //			DE02.EndStrobe();
        }

        private void menuItem3_Click(object sender, System.EventArgs e)
        {
            MessageBox.Show("Must Change, Close and Reload for changes to take effect");
            if (!mdsf.RunOnPHXDevSystem)                                        // PKv5.5.5
                ShowInNotepad(ProjectFiles.MachineConfigurationFile);
            else
                ShowInNotepad(ProjectFiles.MachineConfigurationFileDev);

        }

        private void mnuReloadMachineSettings_Click(object sender, System.EventArgs e)
        {
            int i = Pixy.UpdateSettingsFromFile();                              // PKv5.5.5   This guy automatically remembers whetehr to use dev file.
            // PKv5.2
          //  cbClearTipsB4Plunge.Checked = Pixy.MachineParameters.ClearTipsB4Plunge;         // Let user see the values of these (Read only check box)
          //  cbUseBentTweezerPoints.Checked = Pixy.MachineParameters.UseBentTweezerPoints;

            // PKv5.5.1 , PKv5.5.3            // Let the users see this if it is changed in the file
            this.numericUpDownUSWashSyringeSpeedCode.Value = Pixy.MachineParameters.SyringePushSpeedPrime;
            this.numericUpDownOTFIDelayAfterTrip.Value = Pixy.MachineParameters.InsGridCamDelayAfterTrip;

            // PKv5.5.4
            this.numericUpDownOTFIPercentWayToTarget.Value = Pixy.MachineParameters.OTFSlowDownPercent;
            this.numericUpDownOTFISlowSpeed.Value = Pixy.MachineParameters.OTFSlowDownSpeed;


            // PKv5.2.4,   in case the user wants to reload from the file,  updated the firing parameters too

            this.numericUpDownTipAmp.Value = Pixy.MachineParameters.TrapAmp[Pixy.ActiveTip];

            this.numericUpDownTrapLeading.Value = Pixy.MachineParameters.DE03TrapSetupLeading[Pixy.ActiveTip];
            this.numericUpDownTrapDwell.Value = Pixy.MachineParameters.DE03TrapSetupDwell[Pixy.ActiveTip]; ;
            this.numericUpDownTrapTrailing.Value = Pixy.MachineParameters.DE03TrapSetupTrailing[Pixy.ActiveTip];

            this.numericUpDownCosFreq.Value = Pixy.MachineParameters.DE03CosSetupFreq[Pixy.ActiveTip];
            this.numericUpDownCosAmp.Value = Pixy.MachineParameters.DE03CosSetupAmp[Pixy.ActiveTip];

            this.numericUpDownAspirateVolume.Value = (decimal)Pixy.MachineParameters.AspirateVolume[Pixy.ActiveTip];  //2019-01-16

            this.numericUpDownSpotDropNumber.Value = Pixy.MachineParameters.NumberDropsToSpot[Pixy.ActiveTip];

            // PKv5.5.4
            this.numericUpDownOTFIPercentWayToTarget.Value = Pixy.MachineParameters.OTFSlowDownPercent;
            this.numericUpDownOTFISlowSpeed.Value = Pixy.MachineParameters.OTFSlowDownSpeed;

            // PKv5.6
            lblTipMovement.Text = "Tip Movements:    Backlite Z offset (mm) = " + Pixy.MachineParameters.OTFTargetZOffset_mm.ToString();

        }

        //public void RunSomeTest()
        //{
        //    Comm.Open(1, 9600, 8, Rs232.DataParity.Parity_None, Rs232.DataStopBit.StopBit_1);
        //    Comm.Write(1, "/1ZR\r");
        //}

        private void menuItem3_Click_1(object sender, EventArgs e)
        {
            //         EASerialPort.Open();
            //         EASerialPort.WriteLine("/1ZR");

            ////         Comm.Open(0, 9600, 8, Rs232.DataParity.Parity_None, Rs232.DataStopBit.StopBit_1);
            //         Comm.Write(0, "/1ZR\r");

            EASerialPort.Test();
        }

        private void mnuInitOutputs_Click(object sender, EventArgs e)
        {
            IO.InitializeOutputs();
        }

        private void mnuSetOutputs_Click(object sender, EventArgs e)
        {
            // TODO  Better to kick this off into a seperate thread 

            string tempStr;
            int whatIO = 0;
            do
            {
                Console.Write("Negative numbers turn ON (sinking outputs), 0 to exit: ");
                Console.Write("What IO Singnal to set / Clear (1 to 8) : ");
                tempStr = Console.ReadLine();
                whatIO = Convert.ToInt32(tempStr);
                if (whatIO == 0) break;
                IO.SetOutput(whatIO);
            } while (true);
        }

        private void mnuInitAllIO_Click(object sender, EventArgs e)
        {
            IO.InitInputAndOutputs();
        }


        private void mnuReadInputs_Click(object sender, System.EventArgs e)
        {
            // TODO might be better to kick this off into a seperate thread.
            // OR verify that a sequence is not running


            Console.WriteLine("Read Inputs Utility: ");

            IO.ReadAllInputs();   // Read and display all the inputs to console.
            Console.WriteLine("\n\n");
            string tempStr;
            int whatIO = 0;
            do
            {
                Console.Write("Negative numbers invert, 0 to exit: ");
                Console.Write("What IO Singnal to read (1 to 16) : ");
                tempStr = Console.ReadLine();
                whatIO = Convert.ToInt32(tempStr);
                if (whatIO == 0) break;
                Console.WriteLine("    Input {0} = {1}", whatIO, IO.ReadInput(whatIO));
            } while (true);
        }

        private void mnuIOControlWindow_Click(object sender, EventArgs e)
        {
            IOControlForm ioControlForm = new IOControlForm();
            ioControlForm.Show();
        }

        private void mnuInitInputsAndOutputs_Click(object sender, EventArgs e)
        {
            IO.InitInputAndOutputs();
        }

        private void mnuMoveGridRobotSafe_Click(object sender, EventArgs e)
        {
            if (Pixy.MotionControl.MoveZToSafeHeight(Pixy, true, 25, true) != 0) return;
            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[0], Pixy.MachineParameters.SafePointGrid, 0.0, Pixy.MotionControl.DefaultSpeed_pct, true, true, true, false);
        }

        private void mnuMovePipetteSafe_Click(object sender, EventArgs e)
        {
            if (Pixy.MotionControl.MoveZToSafeHeight(Pixy, true) != 0) return;
            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[0], Pixy.MachineParameters.SafePoint, 0.0, Pixy.MotionControl.DefaultSpeed_pct, true, true);
        }

        #region ivan do edits
        // Ivans functions added for the GUI version of the software
        private void UpdateTipVideoCenterDisplay(int activeTip)
        {
            String ready = "Ready";
            String notReady = "Not Ready";
            switch (activeTip)
            {
                case 0:
                    if (Pixy.MachineParameters.TipVideoCentered[0] == false)
                    {
                        this.labelTip1VideoCentered.Text = notReady;
                        this.labelTip1VideoCentered.ForeColor = System.Drawing.Color.Red;
                    }
                    else
                    {
                        this.labelTip1VideoCentered.Text = ready;
                        this.labelTip1VideoCentered.ForeColor = System.Drawing.Color.Green;

                    }
                    break;
                case 1:
                    if (Pixy.MachineParameters.TipVideoCentered[1] == false)
                    {
                        this.labelTip2VideoCentered.Text = notReady;
                        this.labelTip2VideoCentered.ForeColor = System.Drawing.Color.Red;
                    }
                    else
                    {
                        this.labelTip2VideoCentered.Text = ready;
                        this.labelTip2VideoCentered.ForeColor = System.Drawing.Color.Green;

                    }
                    break;
                case 2:
                    if (Pixy.MachineParameters.TipVideoCentered[2] == false)
                    {
                        this.labelTip3VideoCentered.Text = notReady;
                        this.labelTip3VideoCentered.ForeColor = System.Drawing.Color.Red;
                    }
                    else
                    {
                        this.labelTip3VideoCentered.Text = ready;
                        this.labelTip3VideoCentered.ForeColor = System.Drawing.Color.Green;
                    }
                    break;
            }
        }


        // this function converts the click coordinates from the video to actual 
        // movement of the axis with tips. The tip calibration for scale of mm/pixel 
        // from video data has to be performed before calling the DoGoToClickPosition

        // PKv5.2.8  Got rid of a bunch of code that was not used in the left click section

        // PKv5.2.8  Made the right click fire the tip when in staining mode,  trapezoid only


        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            clickedCoordinates = me.Location;

            if (me.Button == MouseButtons.Left)
            {

                DoGoToClickPosition(clickedCoordinates);
            }

            if (me.Button == MouseButtons.Right)
            {
                // PKv5.2.5

                if (!checkBoxExperimentMode.Checked)
                {
                    Console.WriteLine("pictureBox1_MouseClick:  *** Right Click will fire the tips in stain mode only, doing nothing returning");
                    return;
                }

                if (Pixy.ActiveTip == 0) Pixy.MachineParameters.DE03TrapSetupTrapDrops = (int)numericUpDownSTNTip1Drops.Value;
                if (Pixy.ActiveTip == 1) Pixy.MachineParameters.DE03TrapSetupTrapDrops = (int)numericUpDownSTNTip2Drops.Value;
                if (Pixy.ActiveTip == 2) Pixy.MachineParameters.DE03TrapSetupTrapDrops = (int)numericUpDownSTNTip3Drops.Value;

                Console.WriteLine("pictureBox1_Mouse Click: Right Click Firing Trap: A{0},T{1}_{2}_{3}, Drops = {4}",
                    Pixy.MachineParameters.TrapAmp[Pixy.ActiveTip],
                    Pixy.MachineParameters.DE03TrapSetupLeading[Pixy.ActiveTip],
                    Pixy.MachineParameters.DE03TrapSetupDwell[Pixy.ActiveTip],
                    Pixy.MachineParameters.DE03TrapSetupTrailing[Pixy.ActiveTip],
                    Pixy.MachineParameters.DE03TrapSetupTrapDrops);

                int tempStrobeDelay = Pixy.MachineParameters.DE03TrapSetupStrobeDelay;
                Pixy.MachineParameters.DE03TrapSetupStrobeDelay = 0;
                DoDE03TrapSetupGUI();
                DoDE03StartWaveformGUI(false, 0);
                Pixy.MachineParameters.DE03TrapSetupStrobeDelay = tempStrobeDelay;

            }
        }

        // Ivan added: this function is called when you click "Switch Camera" button
        // The function switches between the two cameras
        // also calls for event listeners for the Pixel Clock, FPS and Exposure
        // to be updated for the respective camera
        private void btnSwitchCamera_Click(object sender, EventArgs e)
        {
            checkBoxDebugEnableStrobe.Checked = false;          // Turn off light before switching to new camera.
            Thread.Sleep(50);       
           
            Camera.setSwitchCamera();
            trackBarPixelClock_Changed(sender, null);
            trackBarExposure_Changed(sender, null);
            trackBarFPS_Changed(sender, null);
            string camName = Camera.getCurrentCameraName();
            if (camName == "Back")
                lblActiveCam.Text = "(Grid)";
            else
                lblActiveCam.Text = "(Tip)";


            btnCamReadGain_Click(sender, null);

        }


        private void btnAspirate_Click(object sender, EventArgs e)
        {
            IfMixingEnabledTurnTempOff();       // PKv5.3.1 

            //move the grid robot to the safe position PKv5.2.5
            if (!Pixy.MachineParameters.PKDevSystemEnable)
            {
                Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                              Pixy.MachineParameters.SafePointGrid, 0.0, 15, true, true, true, false);
            }

            bool UseGridRobot = false;
            double AspirateVolume = -1.0;

            // Move pipette robot safe point, then rotate.
            DoMoveToPointOrderedGUI(Pixy.MachineParameters.SafePoint, 0, 0, 0,
                                Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                100, true, Pixy.MachineParameters.ThetaRotate_0, 1, 3, 2, UseGridRobot);

 
            // Move above by 12.0 mm
            DoMoveToPointOrderedGUI(Pixy.MachineParameters.AspiratePointDOWNTip1, 0, 0, 12.0,
                                       Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                       100, true, Pixy.MachineParameters.ThetaRotate_0, 1, 3, 2, UseGridRobot);


            if (MessageBox.Show("Everthing Lined up for aspiration 12mm above", "Exit?", System.Windows.Forms.MessageBoxButtons.OKCancel) == DialogResult.Cancel) return;


            // Move down very slowly
            DoMoveToPointGUI(Pixy.MachineParameters.AspiratePointDOWNTip1, 0, 0, 0,
                    Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                    10, false, false);

            // Set both valves to output
            if (AtSyringe.Control.SetValvePosition(0, SyringeValvePosition.OutputPosition, true) != 0) return;
            if (AtSyringe.Control.SetValvePosition(1, SyringeValvePosition.OutputPosition, true) != 0) return;

            int stTipIndex = Pixy.ActiveTip;
            int endTipIndex = Pixy.ActiveTip;
            if (cbSelectTip1andTip2.Checked)
            {
                stTipIndex = 0;
                endTipIndex = 1;
            }

            for (int tipIndex = stTipIndex; tipIndex <= endTipIndex; tipIndex++)
            {
                // AspirateVolume = (double)this.numericUpDownAspirateVolume.Value; Old way commented out
                AspirateVolume = Pixy.MachineParameters.AspirateVolume[tipIndex];

                Console.WriteLine("Tip Index {0}, Aspiration Volume {1}", tipIndex, AspirateVolume);
            
                if (AspirateVolume > 0 && AspirateVolume <= 20.0)
                {
                    // Will not turn valve to bypass after aspiration.
                    DoAspirateGUI(AspirateVolume,
                              tipIndex,
                              Pixy.MachineParameters.DelayAfterms,
                              Pixy.MachineParameters.SyringePullSpeed,
                              false,
                              false);
                    loadedSampleVolume = AspirateVolume;
                    UpdateLoadedSampleLabel();
                }


            // the maximum volume that we can aspirate is set a 20 ul.
            // this limit is set by the Engineering Arts.
                else if (AspirateVolume > 20.0)
                {
                    MessageBox.Show("Maximum allowed volume is 20.0 ul");
                }
                else { MessageBox.Show("Invalid aspiration volume"); }

            }

            // Set both valves to bypass and wait
            if (AtSyringe.Control.SetValvePosition(0, SyringeValvePosition.BypassPosition, true) != 0) return;
            if (AtSyringe.Control.SetValvePosition(1, SyringeValvePosition.BypassPosition, true) != 0) return;

            Thread.Sleep(Pixy.MachineParameters.AspriateDelayAfterBypass);

            // Move slowly out to the point 12mm above.
            DoMoveToPointGUI(Pixy.MachineParameters.AspiratePointDOWNTip1, 0, 0, 12,
                Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                10, false, false);

            // Move to safe.
            DoMoveToPointGUI(Pixy.MachineParameters.SafePoint, 0, 0, 0,
                                Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                75, true, false);

            IfMixingEnabledTurnBackOn();        // PKv5.3.1


        }

        private void btnStain_Click(object sender, EventArgs e)
        {
            //move the grid robot to the safe position PKv5.2.5
            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                          Pixy.MachineParameters.SafePointGrid, 0.0, 15, true, true, true, false);

            bool UseGridRobot = false;
            int StainTime = (int)this.numericUpDownStainTime.Value; ;
            //1 move to Safe point
            // MachineCoordinate temp_point = new MachineCoordinate(0, 0, 0);
            //MachineCoordinate temp_point2 = new MachineCoordinate(80, 65, 0);
            // temp_point2 = Pixy.MachineParameters.AspiratePointUPTip1;

            //    DoRotateMoveGUI(Pixy.MachineParameters.ThetaRotate_0);  // To get to SafePoint:  Moves Safe, Then XY, Then Theta, Then Z.
            DoMoveToPointOrderedGUI(Pixy.MachineParameters.SafePoint, 0, 0, 0,
                                Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                100, true, Pixy.MachineParameters.ThetaRotate_0, 1, 3, 2, UseGridRobot);

            DoStainGUI(StainTime, Pixy.MachineParameters.StainPointDOWNTip1);

            DoMoveToPointGUI(Pixy.MachineParameters.SafePoint, 0, 0, 0,
                                Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                25, true, false);
        }

        // Ivan added: function that is called on Prime click event
        // Makes sure that we have at least 1 prime and no more than 40
        // prime cycles. Then calls the "DoPrimeGui" function that actually
        // talks to the robot
        private void btnPrime_Click(object sender, EventArgs e)
        {
            checkBoxMixOn.Checked = false;      // PKv5.3.1,   If mixing is on then turn it off.
            int PrimeNumber = -1;
            //Int32.TryParse(this.numericUpDownPrimeNumber.Value, out PrimeNumber);
            PrimeNumber = (int)this.numericUpDownPrimeNumber.Value;
            int syringePrimePushSpeed = (int)numericUpDownUSWashSyringeSpeedCode.Value;                 // PKv5.5.0

            if (PrimeNumber > 0 && PrimeNumber <= 40)
            {
                DoPrimeGUI(Pixy.MachineParameters.PrimeVolume,
                            PrimeNumber,
                            Pixy.MachineParameters.SyringeMask,
                            Pixy.MachineParameters.WashPoint,
                            Pixy.MachineParameters.DelayAftermsPrime,
                            syringePrimePushSpeed,
                            Pixy.MachineParameters.SyringePullSpeedPrime);

 //               DoSyringeSetValvePositionGUI(Pixy.MachineParameters.SyringeMask,
 //                           "2",                        // 2 is bypass.
 //                           100);

                loadedSampleVolume = 0;
                UpdateLoadedSampleLabel();
            }
        }

        // Ivan added: function connects the clickin of the button with an actual function
        // that will command the robot to move.
        // All the "Do Actions" that have "GUI" are the ones that Ivan refactored for the new
        // GUI interface. Specifically these new "Do***GUI" Actions rely on the Machine Parameters
        // that are read from the xml file and store in the MachineParameter class of the Robot
        // if you would like to change any of the parameters please do so in the XML file and 
        // reload the software
        private int DoMoveToPointGUI(MachineCoordinate MachinePt, double XOffset, double YOffset, double ZOffset,
                                     MachineCoordinate Tool, double MoveSpeed_pct, bool MoveToSafeHeightFirst, bool moveAllAxis)
        {
            MachineCoordinate internalMachinePt = new MachineCoordinate();
            // if (NoPointSpecified) throw new Exception("No point specified for move command");

            // find out if user motion control was allowed
            bool AllowingManualControl = Pixy.MotionControl.ManualControlEnabled;
            internalMachinePt.X = MachinePt.X + XOffset;
            internalMachinePt.Y = MachinePt.Y + YOffset;
            internalMachinePt.Z = MachinePt.Z + ZOffset;

            bool UseGridRobot = false;

            try
            {
                // lock out the user motion control
                Pixy.MotionControl.EnableManualControl(Pixy, false);

                if (MoveToSafeHeightFirst)
                {
                    // first move to a safe height
                    ShowStatusMessage("Move to safe z height");
                    if (Pixy.MotionControl.MoveZToSafeHeight(Pixy, true, MoveSpeed_pct) != 0) return 1;

                }

                // say that we're moving


                if (Pixy.MotionControl.MoveSafely(Pixy, Tool, internalMachinePt, 0.0, MoveSpeed_pct, true, true, UseGridRobot, moveAllAxis) != 0) return 1;
                Pixy.MotionControl.WaitForEndOfMotion(1);

            }
            finally
            {
                // allow user control again if it was allowed before
                Pixy.MotionControl.EnableManualControl(Pixy, AllowingManualControl);
            }

            return 0;
        }
        // Ivan added: function connects the clickin of the button with an actual function
        // that will command the robot to move.
        // All the "Do Actions" that have "GUI" are the ones that Ivan refactored for the new
        // GUI interface. Specifically these new "Do***GUI" Actions rely on the Machine Parameters
        // that are read from the xml file and store in the MachineParameter class of the Robot
        // if you would like to change any of the parameters please do so in the XML file and 
        // reload the software
        private int DoMoveToPointOrderedGUI(MachineCoordinate MachinePt, double XOffset, double YOffset, double ZOffset,
                                            MachineCoordinate Tool, double MoveSpeed_pct, bool MoveToSafeHeightFirst,
                                            int ThetaRotationPosition, int XYorder, int Zorder, int ThetaOrder, bool UseGridRobot)
        {
            int currentStepOfMovement = 1;
            MachineCoordinate internalMachinePt = new MachineCoordinate();
            MachineCoordinate internalCurrentMachinePosition = new MachineCoordinate();

            // if (NoPointSpecified) throw new Exception("No point specified for move command");

            // find out if user motion control was allowed
            bool AllowingManualControl = Pixy.MotionControl.ManualControlEnabled;

            //  bool UseGridRobot = false;

            // lock out the user motion control
            Pixy.MotionControl.EnableManualControl(Pixy, false);

            while (currentStepOfMovement != 4)
            {
                if (currentStepOfMovement == XYorder)
                {
                    Pixy.MotionControl.GetCurrentPosition(out internalCurrentMachinePosition);
                    internalMachinePt.X = MachinePt.X + XOffset;
                    internalMachinePt.Y = MachinePt.Y + YOffset;
                    internalMachinePt.Z = internalCurrentMachinePosition.Z;
                    currentStepOfMovement++;

                    if (MoveToSafeHeightFirst)
                    {
                        // first move to a safe height
                        ShowStatusMessage("Move to safe z height");
                        if (Pixy.MotionControl.MoveZToSafeHeight(Pixy, true, MoveSpeed_pct) != 0) return 1;
                        internalMachinePt.Z = Pixy.MachineParameters.SafePoint.Z;
                    }

                    // say that we're moving
                    if (Pixy.MotionControl.MoveSafely(Pixy, Tool, internalMachinePt, 0.0, MoveSpeed_pct, true, true, UseGridRobot, false) != 0) return 1;
                    Pixy.MotionControl.WaitForEndOfMotion(1);

                }
                else if (currentStepOfMovement == Zorder)
                {
                    Pixy.MotionControl.GetCurrentPosition(out internalCurrentMachinePosition);
                    internalMachinePt.X = internalCurrentMachinePosition.X;
                    internalMachinePt.Y = internalCurrentMachinePosition.Y;
                    internalMachinePt.Z = MachinePt.Z + ZOffset;
                    currentStepOfMovement++;

                    // say that we're moving
                    if (Pixy.MotionControl.MoveSafely(Pixy, Tool, internalMachinePt, 0.0, MoveSpeed_pct, true, true, UseGridRobot, false) != 0) return 1;
                    Pixy.MotionControl.WaitForEndOfMotion(1);

                }
                else if (currentStepOfMovement == ThetaOrder)
                {
                    if (!Pixy.MachineParameters.PKDevSystemEnable)      // PKv5.5.0    No Theta axis on the machine.
                    {
                        DoRotateMoveGUI(ThetaRotationPosition);
                    }
                    currentStepOfMovement++;
                }
            };

            try
            {




            }
            finally
            {
                // allow user control again if it was allowed before
                Pixy.MotionControl.EnableManualControl(Pixy, AllowingManualControl);
            }

            return 0;
        }

        // Ivan added: this function calls all the necessary steps to make the robot "aspirate" a sample
        // 2019-01-19,  Simplified took all the motion out of this routine.

        private int DoAspirateGUI(double Volume_uL, int SyringeMask, int DelayAfter_ms, int SyringePullSpeed, bool ValveToBypass, bool AspFromReservoir)
        {
            if (AspFromReservoir)
            {
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.InputPosition, true) != 0) return 1;
            }
            else
            {
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.OutputPosition, true) != 0) return 1;
            }

            if (AtSyringe.Control.Aspirate(SyringeMask, Volume_uL, SyringePullSpeed, true) != 0) return 1;

            if (ValveToBypass)
            {
                if (DelayAfter_ms > 0) System.Threading.Thread.Sleep(DelayAfter_ms);        // Delay before and after switching to bypass.
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.BypassPosition, true) != 0) return 1;
            }

            // delay before moving out
            if (DelayAfter_ms > 0) System.Threading.Thread.Sleep(DelayAfter_ms);

            return 0;
        }

        // PK added: this function calls all the necessary steps to make the robot "stain" a sample
        private int DoStainGUI(int StainTime, MachineCoordinate Well)
        {
            bool UseGridRobot = false;
            // some error checking

            //move the grid robot to the safe position
            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                          Pixy.MachineParameters.SafePointGrid, 0.0, 75, true, true, true, false);

            if (Well != null)
            {

                DoMoveToPointOrderedGUI(Well, 0, 0, 0,
                                       Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                       100, true, Pixy.MachineParameters.ThetaRotate_0, 1, 3, 2, UseGridRobot);
            }

            // delay before moving out
            Console.WriteLine("Waiting - Staining Sample Delay (sec) = {0}", StainTime);
            if (StainTime > 0) System.Threading.Thread.Sleep(StainTime * 1000);
            Console.WriteLine("Stain Delay Complete");

            return 0;
        }

        // Ivan added: this functions performs all the necessary step to make the robot Prime the tips
        private int DoPrimeGUI(double PrimeVolume_uL, int NumSyringeStrokes, int SyringeMask, MachineCoordinate Pt, int DelayAfter_ms, int SyringePushSpeed, int SyringePullSpeed)
        {
            bool UseGridRobot = false;
            // some error checking
            if (NumSyringeStrokes <= 0)
            {  // user might disable the command this way by using a variable - allow it but record why it's skipped
                UserDatalog("{0} syringe strokes requested - bypassing prime", NumSyringeStrokes);
                return 0;
            }
            if (PrimeVolume_uL <= 0.0) throw new Exception("Invalid prime volume: " + PrimeVolume_uL);

           
            if (!cbPrimeWithoutMove.Checked)        // PKv5.5.0.6   Skip all movements 2017-02-23
            {
                //move the grid robot to the safe position

                if (!Pixy.MachineParameters.PKDevSystemEnable)
                {
                    Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                              Pixy.MachineParameters.SafePointGrid, 0.0, 75, true, true, true, false);
                }

                if (Pt != null)
                {
                    DoMoveToPointOrderedGUI(Pt, 0, 0, 0,
                                            Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                            100, true, Pixy.MachineParameters.ThetaRotate_0, 1, 3, 2, UseGridRobot);
                }
            }

            // 2019-01-16   Use StainItOn Adapted 
            // 2019-01-16TODO  Update to use
            // Pixy.ActiveTip = 1 means tip 2 is selected
            // cbSelectTip1andTip2.Checked = Use both tips


            Prime(0, SyringePushSpeed, NumSyringeStrokes, PrimeVolume_uL);

            /*  Got rid to the guts of the priming routine.
            if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.OutputPosition, true) != 0) return 1;
            if (AtSyringe.Control.EmptySyringe(SyringeMask, SyringePushSpeed, true) != 0) return 1;

            // prime to the tip
            for (int Cycle = 0; Cycle < NumSyringeStrokes; ++Cycle)
            {
                // fill the tip        //Syringe-TODO
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.InputPosition, true) != 0) return 1;
                if (AtSyringe.Control.Aspirate(SyringeMask, PrimeVolume_uL, SyringePullSpeed, true) != 0) return 1;

                // flush it out     //Syringe-TODO
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.OutputPosition, true) != 0) return 1;
                if (AtSyringe.Control.EmptySyringe(SyringeMask, SyringePushSpeed, true) != 0) return 1;
                //				if (Pixy.SyringeControl.EmptySyringe(SyringePushSpeed, true, SyringeMask) != 0) return 1;
            }

            // delay before moving out
            if (DelayAfter_ms > 0) System.Threading.Thread.Sleep(DelayAfter_ms);
            */


            return 0;
        }


        // 2019-01-16,  Imported StainItOn with modifications for 2 syringe pumps.
        // PKv4.5.1  Added option to specify the volume
        // PKv9.0  Added an option for specifying the pump number,  or all pumps   2018-07-18

        // pumpNo = 0,   All pumps   TODO - This needs to be coded.
        // pumpNo = (1to2), Which pump.


        private int Prime(int pumpNo, int pushSpeedCode, int cycles, double volume)
        {
            MachineConfigurationData ptrMachParam = Pixy.MachineParameters;   // Just a shorthand way address the machine parameters.   // PKv9.0

            int SyringePushSpeed = pushSpeedCode;
            int delayAfterPush_ms = 100;        // Before the valve switch
            int delayAfterPull_ms = 500;        // Before the valve switch
            int NumSyringeStrokes = cycles;
            int afterValveSwitchDelay_ms = 200;     // Inceased from 100

            int timeout = 20000;

            int pumpIndexSt;
            int pumpIndexEnd;
            if (pumpNo == 0)
            {
                pumpIndexSt = 0;
                pumpIndexEnd = 1;
            }
            else
            {
                pumpIndexSt = pumpNo - 1;
                pumpIndexEnd = pumpNo - 1;
            }

            // push to the tip
            for (int Cycle = 0; Cycle < NumSyringeStrokes; Cycle++)
            {
                // fill the tip  
                for (int pumpIndex = pumpIndexSt; pumpIndex <= pumpIndexEnd; pumpIndex++)
                {
                    if (AtSyringe.Control.SetValvePosition(pumpIndex, SyringeValvePosition.InputPosition, false) != 0) return 1;
                }

                Thread.Sleep(afterValveSwitchDelay_ms);
                if (Cycle == 0)       // Empty on the first one back in the reservoir.
                {
                    for (int pumpIndex = pumpIndexSt; pumpIndex <= pumpIndexEnd; pumpIndex++)
                    {
                        if (AtSyringe.Control.EmptySyringe(pumpIndex, AtSyringe.Control.SyringeRefillSpeed(pumpIndex), false) != 0) return 1;
                    }
                    if (WaitForSyringeToFinish(pumpNo, timeout) != 0) return 1;
                    Thread.Sleep(afterValveSwitchDelay_ms);
                }

                for (int pumpIndex = pumpIndexSt; pumpIndex <= pumpIndexEnd; pumpIndex++)
                {
                    if (AtSyringe.Control.Aspirate(pumpIndex, volume, AtSyringe.Control.SyringeRefillSpeed(pumpIndex), false) != 0) return 1;
                }
                if (WaitForSyringeToFinish(pumpNo, timeout) != 0) return 1;
                Thread.Sleep(delayAfterPull_ms);

                // flush it out 
                for (int pumpIndex = pumpIndexSt; pumpIndex <= pumpIndexEnd; pumpIndex++)
                {
                    if (AtSyringe.Control.SetValvePosition(pumpIndex, SyringeValvePosition.OutputPosition, false) != 0) return 1;
                }
                if (WaitForSyringeToFinish(pumpNo, timeout) != 0) return 1;
                Thread.Sleep(afterValveSwitchDelay_ms);

                for (int pumpIndex = pumpIndexSt; pumpIndex <= pumpIndexEnd; pumpIndex++)
                {
                    if (AtSyringe.Control.EmptySyringe(pumpIndex, SyringePushSpeed, false) != 0) return 1;               // TODO does this need to be empty could just push exact volume and it might be better.
                }
                if (WaitForSyringeToFinish(pumpNo, timeout) != 0) return 1;
                Thread.Sleep(delayAfterPush_ms);

            }

            return 0;
        }


        // PKv5.5.1  Added methods for ultrasonic wash.
        // Developer needs to be careful here that the ultrasonic sweeps are complete before the syringe stroke is done pushing.
        // a warning will be written to the Console

        // 019-01-16TODO, Delete this whole routine
        // No longer using this routine.

        private int DoUSWashGUI()
        {
            SyringeStatus syringeStatus;
            int NumSyringeStrokes = (int) numericUpDownUSWashNumberOfCycles.Value;
            double PrimeVolume_uL = (double) Pixy.MachineParameters.PrimeVolume;
            int SyringePushSpeed = Pixy.MachineParameters.SyringePushSpeedPrime;
            // int SyringeMask = Pixy.MachineParameters.SyringeMask;
            int SyringeMask = 0;
            int SyringeMask1 = 0;
            int SyringeMask2 = 1;
            if (cbSelectTip1andTip2.Checked)
                SyringeMask = SyringeMask1;
            else
                SyringeMask = Pixy.ActiveTip;

            int SyringePullSpeed = Pixy.MachineParameters.SyringePullSpeedPrime;
            int USWashSettleTime_ms = 100;
            // push to the tip
            for (int Cycle = 0; Cycle < NumSyringeStrokes; ++Cycle)
            {
                // fill the tip
                

                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.InputPosition, true) != 0) return 1;
                if (cbSelectTip1andTip2.Checked)
                    if (AtSyringe.Control.SetValvePosition(SyringeMask2, SyringeValvePosition.InputPosition, true) != 0) return 1;

                Thread.Sleep(USWashSettleTime_ms);
                if (Cycle == 0)       // Empty on the first one back in the reservoir.
                {
                    if (AtSyringe.Control.EmptySyringe(SyringeMask, SyringePushSpeed, true) != 0) return 1;
                    if (cbSelectTip1andTip2.Checked)
                        if (AtSyringe.Control.EmptySyringe(SyringeMask2, SyringePushSpeed, true) != 0) return 1;
                    Thread.Sleep(USWashSettleTime_ms);
                }
                if (AtSyringe.Control.Aspirate(SyringeMask, PrimeVolume_uL, SyringePullSpeed, true) != 0) return 1;
                Thread.Sleep(USWashSettleTime_ms);

                // flush it out     
                if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.OutputPosition, true) != 0) return 1;
                Thread.Sleep(USWashSettleTime_ms);
                if (AtSyringe.Control.EmptySyringe(SyringeMask, SyringePushSpeed, false) != 0) return 1;   // Do not wait for completion

                // Setup the DE03 and fire through the frequencies.
                for (int i = 0; i < Pixy.MachineParameters.usFreqSweepCycles; i++)
                {

                    DoDE03CosUltrasonicSetupGUI(Pixy.MachineParameters.USFreq1, Pixy.MachineParameters.USCount1);
                    DoDE03StartWaveformGUI(false, 1000);
                    // 2019-01-16
                    DoDE03StartWaveformGUI(false, 1000);
                    Thread.Sleep(Pixy.MachineParameters.USTime1_ms);
                    DoDE03StopWaveformGUI();

                    if (Pixy.MachineParameters.USFreq2 != 0)
                    {
                        DoDE03CosUltrasonicSetupGUI(Pixy.MachineParameters.USFreq2, Pixy.MachineParameters.USCount2);
                        DoDE03StartWaveformGUI(false, 1000);
                        Thread.Sleep(Pixy.MachineParameters.USTime2_ms);
                        DoDE03StopWaveformGUI();
                    }

                    if (Pixy.MachineParameters.USFreq3 != 0)
                    {
                        DoDE03CosUltrasonicSetupGUI(Pixy.MachineParameters.USFreq3, Pixy.MachineParameters.USCount3);
                        DoDE03StartWaveformGUI(false, 1000);
                        Thread.Sleep(Pixy.MachineParameters.USTime3_ms);
                        DoDE03StopWaveformGUI();
                    }
                }

                // Need to wait for the syringe to be finished  (Try this !!!)
                //                if (AtSyringe.Control.WaitForPumpMoveAndCheckStatus((byte)(SyringeMask - 1), 10000) != 0) return 1;   This did not work

                // Lets try a little harder way.
                syringeStatus = AtSyringe.Control.PumpStatus(SyringeMask);

                // 
                if (syringeStatus.Idle)
                {
                    Console.WriteLine("*** MainForm.cs:  DoUSWashGUI() WARNING / ERROR ultrasonic sweeps longer than syringe pushing time.  Fix that !!!");
                }

                long tm = System.Environment.TickCount + 20000;         // Give it up to 20 seconds.
                do
                {
                    Thread.Sleep(10);
                    syringeStatus = AtSyringe.Control.PumpStatus(SyringeMask);
                } while ((!syringeStatus.Idle) & (System.Environment.TickCount < tm));

                Console.WriteLine("Ultrasonic wash finished  stroke", syringeStatus.Idle);
                if (!syringeStatus.Idle)
                {
                    Console.WriteLine("DoUSWashGUI: Syringe Move Timeout during ultrasonic wash,  Sysringe Idle Status = {0}", syringeStatus.Idle);
                    return 1;
                }

                Thread.Sleep(USWashSettleTime_ms);

            }

            if (AtSyringe.Control.SetValvePosition(SyringeMask, SyringeValvePosition.BypassPosition, true) != 0)
            {
                Console.WriteLine("DoUSWashGUI:  Error switching valve back to bypass");
                return 1;
            }

            Thread.Sleep(USWashSettleTime_ms);


            return 0;
        }

        // 2019-01-16    Import this whole routine from the StainItOn system.
        // Used USWashAmp in StainItOn,    usAmp in this routine.
        // USFreqSweepCycles in StainItOn,  usFreqSweepCycles in this routine.

        // 2019-01-16TODO,  Might want to improve this routine so that it will work with only 1 of 2 tips based on the PIXY
        // Pixy.ActiveTip    and    cbSelectTip1andTip2.Checked  parameters.

        private int DoUSWashGUIImrpoved()
        {
            MachineConfigurationData ptrMachParam = Pixy.MachineParameters;   // Just a shorthand way address the machine parameters.   // PKv9.0
            int NumSyringeStrokes = (int)numericUpDownUSWashNumberOfCycles.Value; ///
            int SyringePushSpeed = Pixy.MachineParameters.SyringePushSpeedPrime;
            int USWashSettleTime_ms = 100;
            int delayAfterPush_ms = 2000;
            int delayAfterPull_ms = 1000;
            int usWashAmp = Pixy.MachineParameters.usAmp;   // 0 to 2047
        
            DE03.StopWaveform();        // Make sure it not already busy outputting a waveform,  important if we do internal sonication
            if (ptrMachParam.DE03_2_Present) DE03.StopWaveform(true);

            int tipNo = 1;           // DE03 #1 will cycle back and forth between tip 1 and tip 3 if ptrMachParam.US3TipHack
            int pumpIndexSt = 0;    // Will run with 2 syringe pumps.
            int pumpIndexEnd = 1;
            int waitForFinish = 0;  // All pumps

            //if (ptrMachParam.PKDevSystemEnable)     // Might wish to put this back in when going back to a single syringe pump.   2019-01-16
            //{
            //    pumpIndexEnd = 0;
            //    waitForFinish = 1;
            //}

            int timeout = 20000;

            // push to the tip
            for (int Cycle = 0; Cycle < NumSyringeStrokes; ++Cycle)
            {
                // fill the tip  
                for (int pumpIndex = pumpIndexSt; pumpIndex <= pumpIndexEnd; pumpIndex++)
                {
                    if (AtSyringe.Control.SetValvePosition(pumpIndex, SyringeValvePosition.InputPosition, false) != 0) return 1;
                }

                Thread.Sleep(USWashSettleTime_ms);
                if (Cycle == 0)       // Empty on the first one back in the reservoir.
                {
                    for (int pumpIndex = pumpIndexSt; pumpIndex <= pumpIndexEnd; pumpIndex++)
                    {
                        if (AtSyringe.Control.EmptySyringe(pumpIndex, AtSyringe.Control.SyringeRefillSpeed(pumpIndex), false) != 0) return 1;
                    }
                    if (WaitForSyringeToFinish(waitForFinish, timeout) != 0) return 1;
                    Thread.Sleep(USWashSettleTime_ms);
                }

                for (int pumpIndex = pumpIndexSt; pumpIndex <= pumpIndexEnd; pumpIndex++)
                {
                    Thread.Sleep(10);

                    if (AtSyringe.Control.Aspirate(pumpIndex, AtSyringe.Control.SyringeVolume_uL(pumpIndex), AtSyringe.Control.SyringeRefillSpeed(pumpIndex), false) != 0) return 1;
                }
                if (WaitForSyringeToFinish(waitForFinish, timeout) != 0) return 1;
                Thread.Sleep(delayAfterPull_ms);

                // Valve to tip 
                for (int pumpIndex = pumpIndexSt; pumpIndex <= pumpIndexEnd; pumpIndex++)
                {
                    Thread.Sleep(10);
                    if (AtSyringe.Control.SetValvePosition(pumpIndex, SyringeValvePosition.OutputPosition, false) != 0) return 1;
                }
                if (WaitForSyringeToFinish(waitForFinish, timeout) != 0) return 1;
                Thread.Sleep(USWashSettleTime_ms);

                // Start to flush
                for (int pumpIndex = pumpIndexSt; pumpIndex <= pumpIndexEnd; pumpIndex++)
                {
                    Thread.Sleep(10);
                    if (AtSyringe.Control.EmptySyringe(pumpIndex, SyringePushSpeed, false) != 0) return 1;               // TODO does this need to be empty could just push exact volume and it might be better.
                }

                // Setup the DE03 and fire through the frequencies.
                for (int i = 0; i < ptrMachParam.usFreqSweepCycles; i++)
                {
                    DE03.CosSetup(tipNo, ptrMachParam.USFreq1, ptrMachParam.USCount1, usWashAmp, 1, 1, 1, 0, 1, 1);
                    DE03.StartWaveform(false, 1000);
                    if (ptrMachParam.DE03_2_Present)
                    {
                        DE03.CosSetup(true, 2, ptrMachParam.USFreq1, ptrMachParam.USCount1, usWashAmp, 1, 1, 1, 0, 1, 1);
                        DE03.StartWaveform(true, false, 1000);
                    }

                    Thread.Sleep(ptrMachParam.USTime1_ms);
                    DE03.StopWaveform();
                    if (ptrMachParam.DE03_2_Present) DE03.StopWaveform(true);


                    if (ptrMachParam.USFreq2 != 0)
                    {
                        DE03.CosSetup(tipNo, ptrMachParam.USFreq2, ptrMachParam.USCount2, usWashAmp, 1, 1, 1, 0, 1, 1);
                        DE03.StartWaveform(false, 1000);
                        if (ptrMachParam.DE03_2_Present)
                        {
                            DE03.CosSetup(true, 2, ptrMachParam.USFreq2, ptrMachParam.USCount2, usWashAmp, 1, 1, 1, 0, 1, 1);
                            DE03.StartWaveform(true, false, 1000);
                        }
                        Thread.Sleep(ptrMachParam.USTime2_ms);
                        DE03.StopWaveform();
                        if (ptrMachParam.DE03_2_Present) DE03.StopWaveform(true);
                    }

                    if (ptrMachParam.USFreq3 != 0)
                    {
                        DE03.CosSetup(tipNo, ptrMachParam.USFreq3, ptrMachParam.USCount3, usWashAmp, 1, 1, 1, 0, 1, 1);
                        DE03.StartWaveform(false, 1000);
                        if (ptrMachParam.DE03_2_Present)
                        {
                            DE03.CosSetup(true, 2, ptrMachParam.USFreq3, ptrMachParam.USCount3, usWashAmp, 1, 1, 1, 0, 1, 1);
                            DE03.StartWaveform(true, false, 1000);
                        }
                        Thread.Sleep(ptrMachParam.USTime3_ms);
                        DE03.StopWaveform();
                        if (ptrMachParam.DE03_2_Present) DE03.StopWaveform(true);
                    }

                    // Toggle the first DE03 between tip 1 and tip 3 (for each cycle).
                    // 2019-01-16  Got rid of this section becuase tip 3 is non existent.
  
                }

                // TODO - Could verify that all syringes are still busy as an extra check to make sure all the parameters are correct. 

                // Wait for all 3 syringe pumps to finish
                if (WaitForSyringeToFinish(waitForFinish, timeout) != 0) return 1;
                Thread.Sleep(delayAfterPush_ms);
            }

            for (int pumpIndex = pumpIndexSt; pumpIndex <= pumpIndexEnd; pumpIndex++)
            {
                Thread.Sleep(10);
                if (AtSyringe.Control.SetValvePosition(pumpIndex, SyringeValvePosition.BypassPosition, false) != 0)
                {
                    Console.WriteLine("DoUSWashGUI:  Error switching valve back to bypass, (0 based) pumpIndex = {0}", pumpIndex);
                    return 1;
                }
            }

            if (WaitForSyringeToFinish(waitForFinish, timeout) != 0) return 1;

            Thread.Sleep(4000);                     // Give lots of time some time allow stuff to flow out and settle in bypass

            return 0;
        }

        // 2019-01-16, Imported this from StainItOn.
        // 2019-01-16 Updated to only use 2 syringe pumps.
        // PKv9.0
        // pumpNo= 0,  All the pumps
        // pumpNo= 1 to 3 Only one pump

        static public int WaitForSyringeToFinish(int pumpNo, int timeout_ms)
        {
            int pumpIndexSt, pumpIndexEnd;

            if (pumpNo == 0)
            {
                pumpIndexSt = 0;
                pumpIndexEnd = 1;
            }
            else
            {
                pumpIndexSt = pumpNo - 1;
                pumpIndexEnd = pumpNo - 1;
            }

            SyringeStatus syringeStatus;
            bool allIdle = false;

            long tm = System.Environment.TickCount + timeout_ms;
            do
            {
                allIdle = true;
                Thread.Sleep(10);
                for (int pumpIndex = pumpIndexSt; pumpIndex <= pumpIndexEnd; pumpIndex++)
                {
                    Thread.Sleep(10);
                    syringeStatus = AtSyringe.Control.PumpStatus(pumpIndex);
                    if (!syringeStatus.Idle) allIdle = false;
                }
            } while ((!allIdle) & (System.Environment.TickCount < tm));
            if (!allIdle)
            {
                // Find out which pump was not idle and print to the 
                for (int pumpIndex = pumpIndexSt; pumpIndex <= pumpIndexEnd; pumpIndex++)
                {
                    Thread.Sleep(10);
                    syringeStatus = AtSyringe.Control.PumpStatus(pumpIndex);
                    if (!syringeStatus.Idle)
                    {
                        Console.WriteLine("Syringe Pump (0 based) Index = {0},  TIMEOUT, Not Idle", pumpIndex);
                    }
                }

                MessageBox.Show("OTFDispense.cs,  WaitForSyringeToFinish  Timeout");
                return 1;
            }
            return 0;
        }

        // Ivan added: this function peforms the actual switching of the valve on the robot
        // 2019-01-16 Method was updated to always cycle through 2 syringe pumps
        // SyringeMask is ignored..

        private int DoSyringeSetValvePositionGUI(int SyringeMask, string valvePosition, int DelayAfter_ms)
        {
            int pumpIndexSt=0, pumpIndexEnd=1;
            for (int pumpIndex = pumpIndexSt; pumpIndex <= pumpIndexEnd; pumpIndex++)
            {
                Thread.Sleep(10);
                if (AtSyringe.Control.SetValvePosition(pumpIndex, GetSyringeValvePosition(valvePosition), true) != 0) return 1;
            }

            if (DelayAfter_ms > 0) System.Threading.Thread.Sleep(DelayAfter_ms);
            return 0;

            /* //2019-01-16,  Following was all commented out...

            //Process_SetSyringeValvePosition Cmd = PA as Process_SetSyringeValvePosition;
            //if (Cmd == null) throw new Exception("Unexpected command type in DoSyringeSetValvePosition: " + PA.Name);

            ////Syringe-TODO
            //			if (Pixy.SyringeControl.SetValvePosition(GetSyringeValvePosition(Cmd.ValvePosition), GetSyringeMask(Cmd.SyringeMask)) != 0) return 1;
            if (AtSyringe.Control.SetValvePosition(SyringeMask, GetSyringeValvePosition(valvePosition), true) != 0) return 1;

            //int DelayAfter_ms = GetOptionalDelay_ms(DelayAfter_ms);
            if (DelayAfter_ms > 0) System.Threading.Thread.Sleep(DelayAfter_ms);     /// WIERD !!

            */


        }




    // Ivan added: this functions sends all the necessary parameters to the DE03 board to setup the tips for firing
    private int DoDE03TrapSetupGUI()
        {
            return DE03.TrapSetup(Pixy.ActiveTip + 1,
                                  Pixy.MachineParameters.DE03TrapSetupLeading[Pixy.ActiveTip],
                                  Pixy.MachineParameters.DE03TrapSetupDwell[Pixy.ActiveTip],
                                  Pixy.MachineParameters.DE03TrapSetupTrailing[Pixy.ActiveTip],
                                  Pixy.MachineParameters.DE03TrapSetupTrapDrops,
                                  Pixy.MachineParameters.DE03TrapSetupTrapFreq,
                                  Pixy.MachineParameters.TrapAmp[Pixy.ActiveTip],
                                  Pixy.MachineParameters.DE03TrapSetupStrobeDelay,
                                  Pixy.MachineParameters.DE03TrapSetupStrobeDuration,
                                  Pixy.MachineParameters.DE03TrapSetupTriggerSetting,
                                  Pixy.MachineParameters.DE03TrapSetupTriggerDelay,
                                  Pixy.MachineParameters.DE03TrapSetupTriggerPeriod);

        }

        // 2019-01-24  Time resolved setup of the DE03.

        private int DoDE03OTFTRSetupGUI(int triggerCount1, int triggerCount2)
        {

            bool de03_1_Enable = false;
            bool de03_2_Enable = false;

            de03_1_Enable = (rbTimeResTip1.Checked | rbTimeResTip1and2.Checked);
            de03_2_Enable = (rbTimeResTip2.Checked | rbTimeResTip1and2.Checked);

            // Always stop firing both DE03
            DE03.StopWaveform();
            DE03.StopWaveform(true);


            Console.WriteLine("DoDE03OTFSetupGUI -Cosine Freq={0},Amp={1},NumberDrops={2}",
                    Pixy.MachineParameters.DE03CosSetupFreq[Pixy.ActiveTip], Pixy.MachineParameters.DE03CosSetupAmp[Pixy.ActiveTip],
                    Pixy.MachineParameters.NumberDropsToSpot[Pixy.ActiveTip]);


            int triggerSetting = 1; // External rising
            int triggerDelay = 5; // 5usec
            int strobeDuration = 5;
            int strobeDelay = 0;
            int numOfBursts = 1;

            if (de03_1_Enable)
            {
                int tipNo = 1;      // Always use tip number 1
                DE03.CosSetup(false, tipNo,
                Pixy.MachineParameters.DE03CosSetupFreq[Pixy.ActiveTip],
                Pixy.MachineParameters.NumberDropsToSpot[Pixy.ActiveTip],
                Pixy.MachineParameters.DE03CosSetupAmp[Pixy.ActiveTip], strobeDuration, strobeDelay, numOfBursts, triggerSetting, triggerDelay, triggerCount1);
            }

            if (de03_2_Enable)
            {
                int tipNo = 2;      // Always use tip number 2

                // Same call as DE03 1 but.... using DE03#2 and triggerCount2
                DE03.CosSetup(true, tipNo,
                 Pixy.MachineParameters.DE03CosSetupFreq[Pixy.ActiveTip],
                 Pixy.MachineParameters.NumberDropsToSpot[Pixy.ActiveTip],
                 Pixy.MachineParameters.DE03CosSetupAmp[Pixy.ActiveTip], strobeDuration, strobeDelay, numOfBursts, triggerSetting, triggerDelay, triggerCount2);
            }

            return 0;
        }

        private int DoDE03OTFSetupGUI()
        {
            // Always stop firing
            DE03.StopWaveform();
            bool useCos = true;     //   2019-01-24  Got rid of all references to....  cbOTF_UseCos.Checked
            if (useCos)
            {
                Console.WriteLine("DoDE03OTFSetupGUI -Cosine Freq={0},Amp={1},NumberDrops={2}",
                    Pixy.MachineParameters.DE03CosSetupFreq[Pixy.ActiveTip], Pixy.MachineParameters.DE03CosSetupAmp[Pixy.ActiveTip],
                    Pixy.MachineParameters.NumberDropsToSpot[Pixy.ActiveTip]);
                return DE03.CosSetup(Pixy.ActiveTip + 1,
                Pixy.MachineParameters.DE03CosSetupFreq[Pixy.ActiveTip],
                Pixy.MachineParameters.NumberDropsToSpot[Pixy.ActiveTip],
                Pixy.MachineParameters.DE03CosSetupAmp[Pixy.ActiveTip], 1, 1, 1, 1, 5, 1);

                //2019-01-23,  Need to change the trigger period to correspond to  
            }
            else
            {
                Console.WriteLine("DoDE03OTFSetupGUI - Trap=L{0}D{1}T{2},NumberDrops={3},Amp={4}",
                    Pixy.MachineParameters.DE03TrapSetupLeading[Pixy.ActiveTip],
                    Pixy.MachineParameters.DE03TrapSetupDwell[Pixy.ActiveTip],
                    Pixy.MachineParameters.DE03TrapSetupTrailing[Pixy.ActiveTip],
                    Pixy.MachineParameters.NumberDropsToSpot[Pixy.ActiveTip],
                    Pixy.MachineParameters.TrapAmp[Pixy.ActiveTip]);

                return DE03.TrapSetup(Pixy.ActiveTip + 1,
                                  Pixy.MachineParameters.DE03TrapSetupLeading[Pixy.ActiveTip],
                                  Pixy.MachineParameters.DE03TrapSetupDwell[Pixy.ActiveTip],
                                  Pixy.MachineParameters.DE03TrapSetupTrailing[Pixy.ActiveTip],
                                  Pixy.MachineParameters.NumberDropsToSpot[Pixy.ActiveTip],
                                  Pixy.MachineParameters.DE03TrapSetupTrapFreq,
                                  Pixy.MachineParameters.TrapAmp[Pixy.ActiveTip], 1, 1, 1, 5, 1);
            }
        }

        // PKv5.3.1,  
        // Will always mix in tip 1

        private int DoDE03StartMixingGUI()
        {
            int drops = (int)Math.Truncate(numericUpDownMixFreq.Value * numericUpDownMixDutyCycle.Value);
            int usecDelay = (int)(1000000 * (1 - numericUpDownMixDutyCycle.Value));
            int ret = DE03.CosSetup(1,
                (int)numericUpDownMixFreq.Value,
                drops,
                (int)numericUpDownMixAmplitude.Value, 1, 1,
                1000000,  // Number of bursts,  goes indefinetly for 1 million seconds.
                0,  // Internal trigger
                usecDelay, // Delay after waveform ends
                1);  // Don't think trigger period does anything ??

            if (ret != 0)
            {
                Console.WriteLine("MainForm.cs: DoDE03StartMixingGUI returned an error = {0}", ret);
                return ret;
            }

            return DE03.StartWaveform(false, 0);
        }

        private int DoDE03StopMixingGUI()
        {
            return DE03.StopWaveform();
        }

        // PKv5.2.4,   For now share some of the trapezoid settings.  Used for inpspection.
        // Updated PKv5.5.0   Updated for the option of high speed camera acquisition.

        private int DoDE03CosSetupInspectionGUI()
        {
            // 2019-01-15 Updated for 2nd DE03.
            if (Pixy.MachineParameters.InsDropUseHighSpeed)
            {
                // PKv5.5.0
                //CosSetup(int tipNo, int desiredFreq, int inBurstNo, int cosAmplitude, 
                // int strobeDuration, int strobeDelay, int numOfBursts,
                // int triggerSetting, int triggerDelay, int triggerPeriod);

                int numOfBursts = 1000;
                int tipNo = Pixy.ActiveTip + 1;
                bool use2ndDE03 = false;
                if (tipNo == 2) use2ndDE03 = true;
                return DE03.CosSetup(use2ndDE03, Pixy.ActiveTip + 1, Pixy.MachineParameters.DE03CosSetupFreq[Pixy.ActiveTip], Pixy.MachineParameters.DE03CosSetupDrops, Pixy.MachineParameters.DE03CosSetupAmp[Pixy.ActiveTip],
                    5, 0, numOfBursts,
                    1, 5, 1);
            }
            return 0;
        }


        private int DoDE03CosSetupGUI()
        {
            // 2019-01-15TODO Updated for 2nd DE03, Not sure if still used as side inspection is above.
            if (Pixy.MachineParameters.InsDropUseHighSpeed)
            {
                // PKv5.5.0
                //CosSetup(int tipNo, int desiredFreq, int inBurstNo, int cosAmplitude, 
                // int strobeDuration, int strobeDelay, int numOfBursts,
                // int triggerSetting, int triggerDelay, int triggerPeriod);

                int numOfBursts = 1000;
                int tipNo = Pixy.ActiveTip + 1;
                bool use2ndDE03 = false;
                if (tipNo == 2) use2ndDE03 = true;
                return DE03.CosSetup(use2ndDE03,Pixy.ActiveTip + 1, Pixy.MachineParameters.DE03CosSetupFreq[Pixy.ActiveTip], Pixy.MachineParameters.DE03CosSetupDrops, Pixy.MachineParameters.DE03CosSetupAmp[Pixy.ActiveTip], 
                    5, 0, numOfBursts,
                    1, 5, 1);
            }
            else
            {
                // 2019-01-15  Not used...
                return DE03.CosSetup(Pixy.ActiveTip + 1,
                    Pixy.MachineParameters.DE03CosSetupFreq[Pixy.ActiveTip],
                    Pixy.MachineParameters.DE03CosSetupDrops,
                    Pixy.MachineParameters.DE03CosSetupAmp[Pixy.ActiveTip], 1, 1, 1,
                    Pixy.MachineParameters.DE03TrapSetupTriggerSetting,
                    Pixy.MachineParameters.DE03TrapSetupTriggerDelay,
                    Pixy.MachineParameters.DE03TrapSetupTriggerPeriod);
            }
        }


        // PKv5.5.1,   

        private int DoDE03CosUltrasonicSetupGUI(int freq, int burstSize)

        {
            // PKv5.5.0
            //CosSetup(int tipNo, int desiredFreq, int inBurstNo, int cosAmplitude, 
            // int strobeDuration, int strobeDelay, int numOfBursts,
            // int triggerSetting, int triggerDelay, int triggerPeriod);

            Console.WriteLine("Ultrasonic Wash Setup:  Freq ={0}, BurstSize ={1}, Amplitude ={2}", freq, burstSize, Pixy.MachineParameters.usAmp);

                return DE03.CosSetup(Pixy.ActiveTip + 1, freq, burstSize, Pixy.MachineParameters.usAmp,
                    1, 0, 1,
                    0, 1, 1); 
        }



        //Ivan added: this functions calls onto DE03 to start the firing by starting the waveform
        private int DoDE03StartWaveformGUI(bool WaitForCompletion, int Timeout_ms)
        {
            // 2019-01-15TODO  Not sure if still used,  if so update for both tip case
            int tipNo = Pixy.ActiveTip + 1;
            bool use2ndDE03 = false;
            if (tipNo == 2) use2ndDE03 = true;
            return DE03.StartWaveform(use2ndDE03,WaitForCompletion, Timeout_ms);
        }

        // 2019-01-24,  Special case for OTF time resolved will strart both DE03 waiting for the trigger.

        private int DoDE03OTFTRStartWaveformGUI(bool WaitForCompletion, int Timeout_ms)
        {
            bool de03_1_Enable = false;
            bool de03_2_Enable = false;

            de03_1_Enable = (rbTimeResTip1.Checked | rbTimeResTip1and2.Checked);
            de03_2_Enable = (rbTimeResTip2.Checked | rbTimeResTip1and2.Checked);

            if (de03_1_Enable) DE03.StartWaveform(false, WaitForCompletion, Timeout_ms);
            if (de03_2_Enable) DE03.StartWaveform(true, WaitForCompletion, Timeout_ms);

            return 0;

        }

        private int DoDE03StartWaveformInspectionGUI(bool WaitForCompletion, int Timeout_ms)
        {
            // 2019-01-15  This is the side cam inspection case only.
            int tipNo = Pixy.ActiveTip + 1;
            bool use2ndDE03 = false;
            if (tipNo == 2) use2ndDE03 = true;
            return DE03.StartWaveform(use2ndDE03, WaitForCompletion, Timeout_ms);
        }

        // PKv5.5.0

        private int DoDE03StopWaveformGUI()
        {
            // 2019-01-15  Not sure if still used,  if so update for both tip case
            int tipNo = Pixy.ActiveTip + 1;
            bool use2ndDE03 = false;
            if (tipNo == 2) use2ndDE03 = true;
            return DE03.StopWaveform(use2ndDE03);
        }

        // PKv5.5.0

        private int DoDE03StopWaveformInspectionGUI()
        {
            // 2019-01-15  This is the side cam insepction case only
            int tipNo = Pixy.ActiveTip + 1;
            bool use2ndDE03 = false;
            if (tipNo == 2) use2ndDE03 = true;
            return DE03.StopWaveform(use2ndDE03);
        }

        // Ivan added: this functions calculates the actual robot position that corresponds 
        // to the position clicked on the camera image. Then actually performs the move 

        // PKv5.2.8  Cleaned up a bit to work with both tabs (Cryo and Stain).

        private void DoGoToClickPosition(Point DestinationCoordinate)
        {

            if (!Pixy.MachineParameters.TipVideoCentered[Pixy.ActiveTip])
            {
                Console.WriteLine("DoGoToClickPosition: Active Tip needs to be centered,  doing nothing,  returning");
                return;
            }

            if (!(this.tabControlRobot.SelectedTab == this.tabControlRobot.TabPages["tabSpotSample"]) &&
                !(this.tabControlRobot.SelectedTab == this.tabControlRobot.TabPages["tabStain"]))
            {
                Console.WriteLine("DoGoToClickPosition: GUI Needs to be on cryo or stain tab,  doing nothing,  returning");
                return;
            }

            int currentThetaPosition = HarmonicDrive.GetPosition();
            int distanceFromHorizontalPosition = Math.Abs(currentThetaPosition - Pixy.MachineParameters.ThetaRotate_90);

            if (distanceFromHorizontalPosition > 5000)
            {
                Console.WriteLine("DoGoToClickPosition: Harmonic drive needs to be horizontal,  doing nothing,  returning");
                return;
            }

            // Finally get on with the move.

            MachineCoordinate calculatedPosition = new MachineCoordinate(Pixy.MachineParameters.CalculatedVideoCenterPoint[Pixy.ActiveTip]);

            int x_sign, y_sign;
            movementDirectionSignPicker(376, 240, DestinationCoordinate.X, DestinationCoordinate.Y, out x_sign, out y_sign);
            calculatedPosition.Y += Pixy.MachineParameters.VideoToMotionScaler_X * x_sign * Math.Abs(376 - DestinationCoordinate.X);
            calculatedPosition.Z += Pixy.MachineParameters.VideoToMotionScaler_Y * y_sign * Math.Abs(240 - DestinationCoordinate.Y);
            DoMoveToPointGUI(calculatedPosition, 0, 0, 0, Pixy.MachineParameters.Tips[Pixy.ActiveTip], 100, false, true);

        }

        // Ivan added: moves the active tip to the wash position.
        // hovers above the reservoir, then dips into the wash reservoir
        // repeats this 2 times. 
        // Not sure why the SetValvePosition is called...will find out later
        private void buttonWashTip_Click(object sender, EventArgs e)
        {

            bool UseGridRobot = false;
            int numberOfWashes = 0; //the length of the wash is set in Machine parameters - WashTime in ms
            numberOfWashes = (int)numericUpDownWash.Value;

            //move the grid robot to the safe position
            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                          Pixy.MachineParameters.SafePointGrid, 0.0, 15, true, true, true, false);

            MessageBox.Show("Turn on the vacuum pump and flip the wash bowl switch. Otherwise the bowl will overflow!");

            IfMixingEnabledTurnTempOff();       // PKv5.3.1


            if (numberOfWashes > 0)
            {
                IO.SetOutput(-4); // turns on the washbowl water pump
                //DoSyringeSetValvePositionGUI(Pixy.MachineParameters.SyringeMask, "2", 100);
                for (int i = 0; i < numberOfWashes; i++)
                {
                    DoMoveToPointOrderedGUI(Pixy.MachineParameters.WashPointUPTip1, 0, 0, 0,
                                            Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                            100, true, Pixy.MachineParameters.ThetaRotate_0, 1, 3, 2, UseGridRobot);
                    //   DoMoveToPointGUI(Pixy.MachineParameters.WashPointUPTip1, 0, 0, 0,
                    //                        Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                    //                      50, true);
                    DoMoveToPointGUI(Pixy.MachineParameters.WashPointDOWNTip1, 0, 0, 0,
                                       Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                       100, false, false);
                    Thread.Sleep(Pixy.MachineParameters.WashTime);
                }

            }
            IO.SetOutput(4); // turns off the washbowl pump

            DoMoveToPointGUI(Pixy.MachineParameters.SafePoint, 0, 0, 0,
                                       Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                       50, true, false);

            IfMixingEnabledTurnBackOn();        // PKv5.3.1

        }

        // PKv5.5.1  Added to perform an ultrasonic wash.
        // 2019-01-16,  Updated for 2 syringe pumps

        private void btnUltasonicWash_Click(object sender, EventArgs e)
        {

            bool UseGridRobot = false;
            int numberOfWashes = (int)numericUpDownUSWashNumberOfCycles.Value;

            //move the grid robot to the safe position if we are NOT on the development system

            if (!Pixy.MachineParameters.PKDevSystemEnable)
            {
                Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                          Pixy.MachineParameters.SafePointGrid, 0.0, 15, true, true, true, false);
            }


            MessageBox.Show("Turn on the vacuum pump and flip the wash bowl switch. Otherwise the bowl will overflow!");

            IfMixingEnabledTurnTempOff();       // PKv5.3.1


            if (numberOfWashes > 0)
            {
                if (Pixy.MachineParameters.PKDevSystemEnable)
                    IO.SetOutput(-1); // turns on the washbowl water pump    
                else
                    IO.SetOutput(-4); // turns on the washbowl water pump

                DoMoveToPointOrderedGUI(Pixy.MachineParameters.WashPointUPTip1, 0, 0, 0,
                                            Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                            100, true, Pixy.MachineParameters.ThetaRotate_0, 1, 3, 2, UseGridRobot);

                DoMoveToPointGUI(Pixy.MachineParameters.WashPointDOWNTip1, 0, 0, 0,
                                       Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                       100, false, false);

 //             DoUSWashGUI();
                DoUSWashGUIImrpoved();          // 2019-01-16,  Completely new routine.

              loadedSampleVolume = 0;
              UpdateLoadedSampleLabel();

            }

            Thread.Sleep(5000);     // Final delay to let pressure stablelize before pulling out.

            if (Pixy.MachineParameters.PKDevSystemEnable)
                IO.SetOutput(1); // turns off the washbowl water pump  
            else
                IO.SetOutput(4); // turns off the washbowl water pump

            Thread.Sleep(5000);     // Final delay before pulling out.

            if (!Pixy.MachineParameters.PKDevSystemEnable)          // Move back to safe point if NOT on the development system,  else  Just come up.
                DoMoveToPointGUI(Pixy.MachineParameters.SafePoint, 0, 0, 0,
                                       Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                       50, true, false);
            else
                DoMoveToPointOrderedGUI(Pixy.MachineParameters.WashPointUPTip1, 0, 0, 10,
                                            Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                            20, false, Pixy.MachineParameters.ThetaRotate_0, 1, 3, 2, UseGridRobot);    // 2019-01-16, Move out slow

            IfMixingEnabledTurnBackOn();        // PKv5.3.1

        }

        // Ivan added: function that is called when you click the "Fire" button
        // This function tries to read the tip amplitude that is specified in the 
        // Textbox and if the amplitude is > 0 then we assign it to the current tip
        // and fire tip using the "DoDE03StartWaveformGUI" function

        // Updated PKv5.5.0  For high speed inspection.

        private void btnFire_Click(object sender, EventArgs e)
        {
            IfMixingEnabledTurnTempOff();  // PKv5.3.1   //2019-01-15TODO, Cant support mixing.

            int TipAmplitude = -1;
            if (checkBoxInspCosine.Checked)
            {
                Pixy.MachineParameters.DE03CosSetupFreq[Pixy.ActiveTip] = (int)this.numericUpDownCosFreq.Value;  // PKv5.2.4
                Pixy.MachineParameters.DE03CosSetupAmp[Pixy.ActiveTip] = (int)this.numericUpDownCosAmp.Value;  // PKv5.2.4

                if (Pixy.MachineParameters.InsDropUseHighSpeed)     // PKv5.5.0
                {
                    // 2019-01-15... This is the path.
                    btnFireHelper();                    // One tip at a time only
                    DoDE03StopWaveformInspectionGUI();  // One tip at a time only
                }
                else
                {
                    // 2019-01-15... Probably never used
                    Pixy.MachineParameters.DE03CosSetupDrops = 5000;   // Is this really needed or wise ?!?  PKv5.2.4
                    DoDE03CosSetupGUI();
                    DoDE03StartWaveformGUI(false, 0);
                    Thread.Sleep(1000);   // PKv5.3.1,  Added in case we're in mixing mode.  Does not hurt if not in mixing mode.
                }
            }
            else
            {
                TipAmplitude = (int)numericUpDownTipAmp.Value;
                if (TipAmplitude > 0)
                {
                    Pixy.MachineParameters.TrapAmp[Pixy.ActiveTip] = TipAmplitude;
                    Pixy.MachineParameters.DE03TrapSetupLeading[Pixy.ActiveTip] = (int)this.numericUpDownTrapLeading.Value;  // PKv5.2.4
                    Pixy.MachineParameters.DE03TrapSetupDwell[Pixy.ActiveTip] = (int)this.numericUpDownTrapDwell.Value;  // PKv5.2.4
                    Pixy.MachineParameters.DE03TrapSetupTrailing[Pixy.ActiveTip] = (int)this.numericUpDownTrapTrailing.Value;  // PKv5.2.4
                    Pixy.MachineParameters.DE03TrapSetupTrapDrops = 300;   // Is this really needed or wise ?!?  PKv5.2.4
                    DoDE03TrapSetupGUI();
                    DoDE03StartWaveformGUI(false, 0);
                    Thread.Sleep(1000);     // PKv5.3.1,  Added in case we're in mixing mode.  Does not hurt if not in mixing mode.
                }
            }

            IfMixingEnabledTurnBackOn();  // PKv5.3.1
        }

        
        private void btnFireHelper()
        {
            //2019-01-15  Updated for dual DE03,  Inspection only.

            Pixy.MachineParameters.DE03CosSetupDrops = Pixy.MachineParameters.InsDropBurstSize; 
            DoDE03CosSetupInspectionGUI();
            DoDE03StartWaveformInspectionGUI(false, 0);

            // Stop Live Acquisition
            Camera.StopLiveAcquistion();

            int stFrameCount = Camera.GetFrameCount();
            int st = System.Environment.TickCount;
            int wait_ms = 0;
            int totalLoops = Pixy.MachineParameters.InsDropTotalBursts;
            int workingTriggerDelay = Pixy.MachineParameters.InsDropCamDelayAfterTrip;    // PKv5.5.0.2

            if (checkBoxInspEndOfStream.Checked)
            {
                double period_us = 1000000.0 / Pixy.MachineParameters.DE03CosSetupFreq[Pixy.ActiveTip];
                workingTriggerDelay = (int)(Pixy.MachineParameters.InsDropEndOfStreamDropsFmEnd * period_us);
            }

            for (int loopCnt = 0; loopCnt < totalLoops; loopCnt++)
            {
                wait_ms = st + (loopCnt * Pixy.MachineParameters.InsDropDelayBTWTriggers_ms) - System.Environment.TickCount;

                if (wait_ms > 0) Thread.Sleep(wait_ms);

                Camera.StartTriggeredAcquisition((int)workingTriggerDelay, Pixy.MachineParameters.InsDropCamFlashDuration, Pixy.MachineParameters.InsDropCamFlashDelay);

                if (Pixy.MachineParameters.PKDevSystemEnable)
                    Pixy.MotionControl.ToggleTriggerOutput(this, 0, false);     // This should take less than 100msec.  x axis ,  pipette robot.
                else
                    Pixy.MotionControl.ToggleTriggerOutput(this, 2, true);     // This should take less than 100msec.   z axis,  grid robot

                workingTriggerDelay += Pixy.MachineParameters.InsDropTriggerDelayInc_usec;

            }

            wait_ms = st + ((Pixy.MachineParameters.InsDropTotalBursts* Pixy.MachineParameters.InsDropDelayBTWTriggers_ms) - System.Environment.TickCount);
            if (wait_ms > 0) Thread.Sleep(wait_ms);  // One last wait

            int et = System.Environment.TickCount - st;
            int averageloopTime = et / totalLoops;

            int totalFramesAcquired = Camera.GetFrameCount() - stFrameCount;
            int dropsConsumed = totalLoops * Pixy.MachineParameters.InsDropBurstSize;
            int nlConsumed = (int) (Pixy.MachineParameters.InsDropVolumePerDrop_nl * dropsConsumed);
            Console.WriteLine("Triggered Acquire Results:");
            Console.WriteLine("   Inspection Frequency = {0},  Inspection Burst Size = {1}", Pixy.MachineParameters.DE03CosSetupFreq[Pixy.ActiveTip], Pixy.MachineParameters.InsDropBurstSize);
            Console.WriteLine("   Total Frames Acquired {0} out of {1}", totalFramesAcquired, totalLoops);
            Console.WriteLine("   Measured Delay Between Triggers {0} Target was {1}", averageloopTime, Pixy.MachineParameters.InsDropDelayBTWTriggers_ms);
            Console.WriteLine("   Volume nl Consumed ={0}, (Droplets Consumed ={1} @ {2:0.000} nl per drop)", nlConsumed, dropsConsumed, Pixy.MachineParameters.InsDropVolumePerDrop_nl);

            loadedSampleVolume = loadedSampleVolume - (nlConsumed / 1000.0);      // PKv5.5.2
            UpdateLoadedSampleLabel();

            // Start Back Live Acquisition

            Thread.Sleep(1000);
            Camera.StartLiveAcquistion();


        }


        // PKv5.3.1

        private void IfMixingEnabledTurnTempOff()
        {
            if (checkBoxMixOn.Checked)
            {
                DoDE03StopMixingGUI();
            }
            return;
        }

        // PKv5.3.1

        private void IfMixingEnabledTurnBackOn()
        {
            if (checkBoxMixOn.Checked)
            {
                DoDE03StartMixingGUI();
            }
        }

        // Ivan added: this function determines the signs that need to be used in the
        // calculations of the relative movement of the tips.
        // Example of equation ----> newPosition = oldPosition + X_SIGN*deltaX
        // This function is used when we would like to do a calculated move with respect to
        // a known position.
        private void movementDirectionSignPicker(double current_x, double current_y, int desired_x, int desired_y, out int x_signed, out int y_signed)
        {
            if (current_x < desired_x)
            {
                x_signed = -1;
                if (current_y < desired_y)
                { y_signed = -1; }
                else { y_signed = 1; }

            }
            else
            {
                x_signed = 1;
                if (current_y < desired_y)
                { y_signed = -1; }
                else { y_signed = 1; }

            }
        }

        // A helper function to create a new object safe copy of a position. 

        private void SafeCopyPosition(out MachineCoordinate position_out, MachineCoordinate position_in)
        {
            position_out = new MachineCoordinate();
            position_out.X = position_in.X;
            position_out.Y = position_in.Y;
            position_out.Z = position_in.Z;
        }

        // OTF - On-The-Fly dispensing routines.
        // PKv5.2.5,  PKv5.2.6  ,  PKv5.2.7
        // PKv5.3.1 Updated for turning mixer on and off
        // PKv5.5.3

        private void DoOTFPlunge()
        {

            LoggingUtility("Entering DoOTFPlunge routine");   //

            #region - Logic Safety Checks (specific to OTF dispense),  don't break anything

            if (!Pixy.MachineParameters.UseBentTweezerPoints)
            {
                Console.WriteLine("***Parameter Check Error- DoOTFPlunge,  Must use bent tweezers when doing OTF dispense... Returning");
                return;
            }

            if (Pixy.MachineParameters.ClearTipsB4Plunge)
            {
                Console.WriteLine("***Parameter Check Error- DoOTFPlunge,  CANNOT clear tips b4 plunge when doing OTF dispense... Returning");
                return;
            }

            if (this.numQueuedCoordinates != 1)
            {
                Console.WriteLine("***Parameter Check Error- DoOTFPlunge,  Must only have 1 queued coordinate for OTF dispense ... Returning");
                Console.WriteLine("   Number of queued coordinates was: {0} ", this.numQueuedCoordinates);
                Console.WriteLine("   Clearning the queue ");
                this.numQueuedCoordinates = 0;
                this.QueuedCoordinates.Clear();
                return;
            }

            double tempDist = Math.Abs(Pixy.MachineParameters.GridToCameraPointBent.X - Pixy.MachineParameters.InLiquidEthaneBowlBent.X) +
                                    Math.Abs(Pixy.MachineParameters.GridToCameraPointBent.Y - Pixy.MachineParameters.InLiquidEthaneBowlBent.Y);
            if (tempDist > .010)
            {
                Console.WriteLine("***Parameter Check Error- DoOTFPlunge,  X & Y Values of /n    GridToCameraPoint and InLiquidEthaneBowlBent  DO NOT MATCH .... returning");
                return;
            }

            bool useCos = true;     //   2019-01-24  Got rid of all references to....  cbOTF_UseCos.Checked

            if (!useCos)
            {
                decimal calculatedNumberOfSpots = (decimal)Math.Truncate(Pixy.MachineParameters.OTFTrapStartOffset_mm / Pixy.MachineParameters.OTFTrapDropSpacing_mm) - 1; ;
                if ((calculatedNumberOfSpots == 0) | calculatedNumberOfSpots > 300)
                {
                    Console.WriteLine("***Parameter Check Error- DoOTFPlunge,  Trapezoid Mode,  Calculated number of drops out of range (>300): {0}", calculatedNumberOfSpots);
                    return;
                }
                Console.WriteLine("OTF with Trapezoid, Calculated Drops: {0}", calculatedNumberOfSpots);
                numericUpDownSpotDropNumber.Value = calculatedNumberOfSpots;

                double calculatedDispFreq = (double)calculatedNumberOfSpots / (Pixy.MachineParameters.OTFTrapStartOffset_mm / Pixy.MachineParameters.OTFTrapDispSpeed_mm_s);
                Console.WriteLine("OTF with Trapezoid, Calculated Drop Frequency: {0}", calculatedDispFreq);
                // Check if the frequency is above 500 you are operating in a danger mode.
                if (calculatedDispFreq > 600)
                {
                    Console.WriteLine("***Parameter Check Error- DoOTFPlunge,  Trapezoid Mode,  Calculated Disp Frequency too high (slow down) (>600): {0}", calculatedNumberOfSpots);
                    return;
                }
            }

            // PKv5.5.3,  PKv5.5.4,
            // Stop and image does not make any sense with flash acquisition or slow down
            if (numericUpDownPauseOTF.Value>0)
            {
                if (cbOTFIEnableSlowDown.Checked)
                {
                    Console.WriteLine("***Parameter Check Error- DoOTFPlunge,  Pause time and slow down cannot both be active");
                    return;
                }
                if (cbOTFIUseFlashAcquisition.Checked)
                {
                    Console.WriteLine("***Parameter Check Error- DoOTFPlunge,  Pause time and flash inspection cannot both be active");
                    return;
                }
            }

            // PKv5.6a
            if (numericUpDownPauseOTF.Value == 0)
            {
                if (!cbOTFIUseFlashAcquisition.Checked)
                {
                    if (!cbOTFIEnableSlowDown.Checked)
                    {
                        Console.WriteLine("***Parameter Check Error- DoOTFPlunge,  Pause time = 0, No Flash Acquisition,  No Slow Down");
                        return;
                    }
                }
            }

            // PKv5.5.3,  PKv5.5.4,
            // If we are not plunging all the way can't allow slow down or triggered acquisition
            // Turn them off temporarirly 

            bool tempOffcbOTFIUseFlashAcquisition = false;
            bool tempOffcbOTFIEnableSlowDown = false;

            if (!checkBoxAutomaticPlunge.Checked)           
            {   
                if (cbOTFIEnableSlowDown.Checked)
                {
                    cbOTFIEnableSlowDown.Checked = false;
                    tempOffcbOTFIEnableSlowDown = true;
                }
                
                if (cbOTFIUseFlashAcquisition.Checked)
                {
    //                cbOTFIUseFlashAcquisition.Checked = false;            PKv5.6b   We are now going to allow flash acquisition because we are stopping at lowergridcamera.
    //                tempOffcbOTFIUseFlashAcquisition = true;
                }
            }

            #endregion

            // PKv5.6, Get the next image file from the remote system
            string remoteImageFullPath = "";
            if (cbDGCEnabled.Checked)
            {
                remoteImageFullPath = Camera.GetNextRemoteImageFile(Pixy.MachineParameters.RemoteVideoFileDirectory, "bmp");
            }


            bool UseGridRobot = true;

            // Move to the one queued point (target position) with the pipette robot.
            DoGoToClickPosition(this.QueuedCoordinates[0]);

            #region Calculate startPosition ,targetPosition (queued), endPosition(s), slowPosition, tempPosition (set to GridToCameraPointBent) for moves.

            MachineCoordinate startPosition;
            MachineCoordinate endPosition;
            MachineCoordinate endPosition2;
            MachineCoordinate targetPosition;
            MachineCoordinate slowDownPosition;     // PKv5.5.4
            MachineCoordinate tempPosition = new MachineCoordinate();
            Pixy.MotionControl.GetCurrentPosition(out tempPosition, true);   // Grid robot current position is at GridToCameraPointBent
            SafeCopyPosition(out slowDownPosition, tempPosition);           //  Z value will get overwritten when cbOTFIEnableSlowDown.Checked but need to initialize now.

            // One more logic check
            tempDist = Math.Abs(Pixy.MachineParameters.GridToCameraPointBent.X - tempPosition.X) +
                         Math.Abs(Pixy.MachineParameters.GridToCameraPointBent.Y - tempPosition.Y);
            if (tempDist > .010)
            {
                Console.WriteLine("***Parameter Check Error- DoOTFPlunge,  Robot not at GridToCameraPointBent .... returning");
                return;
            }

            if (checkBoxAutomaticPlunge.Checked)   // Go all the way to ethane (eventually)
            {

               
                if ((numericUpDownPauseOTF.Value > 0))  // We are stopping on the way for a little viewing.
                {
                    SafeCopyPosition(out endPosition, tempPosition);
                    SafeCopyPosition(out endPosition2, Pixy.MachineParameters.InLiquidEthaneBowlBent);
                }
                else  // Don't stop in Cosine mode but need to stop in trap mode.
                {
                    if (useCos)
                    {
                        SafeCopyPosition(out endPosition, Pixy.MachineParameters.InLiquidEthaneBowlBent);
                        SafeCopyPosition(out endPosition2, Pixy.MachineParameters.InLiquidEthaneBowlBent);
                    }
                    else
                    {
                        SafeCopyPosition(out endPosition, tempPosition);
                        SafeCopyPosition(out endPosition2, Pixy.MachineParameters.InLiquidEthaneBowlBent);
                    }

                }
                if (cbOTFIEnableSlowDown.Checked)           // PKv5.5.4
                {
                    slowDownPosition.Z = slowDownPosition.Z + (Pixy.MachineParameters.OTFTargetZOffset_mm * (1 - ((double)numericUpDownOTFIPercentWayToTarget.Value / 100.0)));
                }
            }
            else   // Not going all the way to ethane
            {
                SafeCopyPosition(out endPosition, tempPosition);     // Will go no further.
                SafeCopyPosition(out endPosition2, tempPosition);    // Will go no further.

                // Set the z height of those positions to lowerGridCamZ     PKv5.6b
                endPosition.Z = Pixy.MachineParameters.OTFLowerGridCamZ;
                endPosition2.Z = Pixy.MachineParameters.OTFLowerGridCamZ;
            }


            // PKv5.2.6  - Dispense above camera point.
            SafeCopyPosition(out targetPosition, tempPosition);
            if (useCos)
            {
                targetPosition.Z += Pixy.MachineParameters.OTFTargetZOffset_mm;
            }
            else
            {
                targetPosition.Z += Pixy.MachineParameters.OTFTrapStartOffset_mm;   // Start and target point are the same.
            }

            // Start Point
            SafeCopyPosition(out startPosition, tempPosition);   // Will create a copy of the point.
            if (useCos)
            {
                startPosition.Z = startPosition.Z + Pixy.MachineParameters.OTFStartAbove_mm;
            }
            else
            {
                startPosition.Z = startPosition.Z + Pixy.MachineParameters.OTFTrapStartOffset_mm;
                decimal calculatedNumberOfSpots = (decimal)Math.Truncate(Pixy.MachineParameters.OTFTrapStartOffset_mm / Pixy.MachineParameters.OTFTrapDropSpacing_mm);
                numericUpDownSpotDropNumber.Value = (decimal)Math.Truncate(Pixy.MachineParameters.OTFTrapStartOffset_mm / Pixy.MachineParameters.OTFTrapDropSpacing_mm);

            }


            #endregion

            if (cbDebugAutoStartVideo.Checked)
            {
                if (!cbOTFIUseFlashAcquisition.Checked)                 //PKv5.6
                {
                    if (checkBoxAutomaticPlunge.Checked)
                    {
                        checkBoxDebugEnableStrobe.Checked = false;
                        Thread.Sleep(10);
                        checkBoxDebugEnableStrobe.Checked = true;
                        Thread.Sleep(10);
                        btnStartStopVideoRecording_Click(this, null);       // Starts the video recording
                    }
                }
            }
            // Move to grid startPosition
            LoggingUtility(" DoOTFPlunge - Moving to start position");
            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                   startPosition, 0.0, 25, true, true, UseGridRobot, true);
            LoggingUtility(" DoOTFPlunge - At start position");

            if (cbOTFIUseFlashAcquisition.Checked)          // PKv5.5.3
            {
                Camera.setSwitchCamera("Grid");   // Double check that we are on the grid camera
                lblActiveCam.Text = "Grid";

                // PKv5.5.0.6,  2017-02-23 Make sure the strobe light is enabled.
                checkBoxDebugEnableStrobe.Checked = false;
                Thread.Sleep(10);
                checkBoxDebugEnableStrobe.Checked = true;
                Thread.Sleep(10);

                Camera.EnableStrobe((int)numericUpDownDebugStrobeDuration.Value, (int)numericUpDownDebugStrobeDelay.Value);
                // Stop Live Acquisition
                Camera.StopLiveAcquistion();
            }

            // Move tip to firing position (above where it is now)
            MachineCoordinate tempTipPosition = new MachineCoordinate();
            Pixy.MotionControl.GetCurrentPosition(out tempTipPosition, false);   // Tip robot current position (at queued point)
            if (useCos)
            {
                Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                tempTipPosition, Pixy.MachineParameters.OTFTargetZOffset_mm, 75, true, true, false, true);
            }
            else
            {
                Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                tempTipPosition, Pixy.MachineParameters.OTFTrapStartOffset_mm, 75, true, true, false, true);
            }

            IfMixingEnabledTurnTempOff();       // PKv5.3.1



            // Arm the motor for generating a trigger
            if (useCos)
            {
                double relativePosition_mm = (Pixy.MachineParameters.OTFDispOffset_mm + targetPosition.Z) - startPosition.Z;   // Using tip1 as the reference.
                //               Pixy.MotionControl.ArmTriggerOutput(Pixy, 2, relativePosition_mm, UseGridRobot, Pixy.MachineParameters.Trigger_CW);   // PKv5.3.3


                Pixy.MotionControl.ArmTriggerOutput(Pixy, 2, Pixy.MachineParameters.OTFTRDistBetweenTriggers_mm, UseGridRobot, Pixy.MachineParameters.Trigger_CW);   // PKv5.3.3

                int trigger1Count =  Math.Abs((int) (relativePosition_mm / Pixy.MachineParameters.OTFTRDistBetweenTriggers_mm));
                int trigger2Count = Math.Abs((int)((relativePosition_mm - Pixy.MachineParameters.OTFTRDistBetweenTips_mm) / Pixy.MachineParameters.OTFTRDistBetweenTriggers_mm));                                                                                                                      //         
                
                // Pass in the divide by trigger counts for both DE03s.     2019-
                DoDE03OTFTRSetupGUI(trigger1Count,trigger2Count);   // will use Pixy.MachineParameters.NumberDropsToSpot[Pixy.ActiveTip]
                
                DoDE03OTFTRStartWaveformGUI(false, 100);

            }
            else
            {
                double relativePosition_mm = -1 * Pixy.MachineParameters.OTFTrapDropSpacing_mm;    // Don't use the .OTFDispOffset_mm in trap mode. 
                Pixy.MotionControl.ArmTriggerOutput(Pixy, 2, relativePosition_mm, UseGridRobot, Pixy.MachineParameters.Trigger_CW);   // PKv5.3.3
            }

            #region Open the shutter if we are automatically plunging
            if (checkBoxAutomaticPlunge.Checked)
            {
                // open shutter
                IO.SetOutput(2);
                LoggingUtility("  Starting Shutter Open");                   // PKv5.0 Logging the timing
                IO.WaitInput(-1, 1000);  // Wait for the shutter to open
            }
            #endregion
            LoggingUtility(" DoOTFPlunge - Shutter Open");

            // Set the speed, acc and decel for OTF move from start.
            Pixy.MotionControl.SetAccel(Pixy, 2, Pixy.MachineParameters.OTFAccelPlunge_mm_ss, Pixy.MachineParameters.OTFDecelPlunge_mm_ss, UseGridRobot);
            if (useCos)
                Pixy.MotionControl.SetMaxVelocity(Pixy, 2, Pixy.MachineParameters.OTFSpeedPlunge_mm_s, UseGridRobot);
            else
                Pixy.MotionControl.SetMaxVelocity(Pixy, 2, Pixy.MachineParameters.OTFTrapDispSpeed_mm_s, UseGridRobot);

            // Arm the triggered acquisition
            int frameCountTriggeredAcquistion=0;
            if (cbOTFIUseFlashAcquisition.Checked)          // PKv5.5.3
            {
                frameCountTriggeredAcquistion = Camera.GetFrameCount();
                Camera.StartTriggeredAcquisition((int) numericUpDownOTFIDelayAfterTrip.Value, Pixy.MachineParameters.InsDropCamFlashDuration, Pixy.MachineParameters.InsDropCamFlashDelay);
            }


            if (useCos && numericUpDownPauseOTF.Value == 0)
            {

                // Do full plunge single move into ethane (automatic plunge checked)
                // Note if we are going all the way to ethane and there is no stop to view,   endPosition=endPosition2

                if (cbOTFIEnableSlowDown.Checked)   // PKv5.5.4 Use slow down complex profile. 
                {
                    Console.WriteLine("MoveComplexProfile Pass parameters:  pos1(final target)={0},pos2(start slowing)={1},pos3(speed back up)={2}", endPosition.Z, slowDownPosition.Z, tempPosition.Z);
                    Console.WriteLine("MoveComplexProfile Pass parameters:  vel1={0},vel2={1},vel3={2} mm per sec", Pixy.MachineParameters.OTFSpeedPlunge_mm_s, Pixy.MachineParameters.OTFSlowDownSpeed, Pixy.MachineParameters.OTFSpeedPlunge_mm_s);

                    LoggingUtility(" DoOTFPlunge - Starting To plunge with slow down option in Cosine mode, no viewing");

                    Pixy.MotionControl.MoveComplexProfile(endPosition.Z, slowDownPosition.Z, tempPosition.Z,
                        Pixy.MachineParameters.OTFSpeedPlunge_mm_s, Pixy.MachineParameters.OTFSlowDownSpeed, Pixy.MachineParameters.OTFSpeedPlunge_mm_s,2,UseGridRobot);
                }
                else
                {
                    LoggingUtility(" DoOTFPlunge - Starting To plunge using standard move in Cosine mode, no viewing");
                    Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip], endPosition, 0.0, 100, true, true, UseGridRobot, true);
                }
                Pixy.MotionControl.SetMaxVelocity(Pixy, 2, Pixy.MachineParameters.OTFSpeedPlunge_mm_s, UseGridRobot);  // Always speed up for next move

            }
            else
            {
                LoggingUtility(" DoOTFPlunge - Starting To Plunge with new MoveDouble");

                if (useCos)
                {
                    // PKv5.5.5.3  Added the following option to wait for a dialog box click.
                    if (numericUpDownPauseOTF.Value == 999)
                    {
                        // Pixy.MachineParameters.Tips[Pixy.ActiveTip] all appear to be set to {0,0,0} in xml file.

                        // Move to first point
                        Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip], endPosition, 0.0, 100, true, true, UseGridRobot, true);
                        // Put up dialog box
                        MessageBox.Show("Paused in front of the camera, Click OK to continue", "", MessageBoxButtons.OK);
                        // Move to final point
                        Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip], endPosition2, 0.0, 100, true, true, UseGridRobot, true);
                    }
                    else
                    {
                        // This was the standard way for stop and go dispense before version PKv5.5.5.3
                        Pixy.MotionControl.MoveDouble(endPosition.Z, endPosition2.Z, (int)numericUpDownPauseOTF.Value, Pixy.MachineParameters.OTFSpeedPlunge_mm_s, Pixy.MachineParameters.OTFSpeedPlunge_mm_s, 2, UseGridRobot);
                    }
                }
                else
                {
                    Pixy.MotionControl.MoveDouble(endPosition.Z, endPosition2.Z, (int)numericUpDownPauseOTF.Value, Pixy.MachineParameters.OTFTrapDispSpeed_mm_s, Pixy.MachineParameters.OTFSpeedPlunge_mm_s, 2, UseGridRobot);
                }
            }

            Pixy.MotionControl.SetMaxVelocity(Pixy, 2, Pixy.MachineParameters.OTFSpeedPlunge_mm_s, UseGridRobot);  // Always speed up for next move

            // PKv5.6b   This whole region is done whether we are plunging to ethane or stopping at lower grid camera.
            if (!checkBoxAutomaticPlunge.Checked) remoteImageFullPath = "";         //  Will skip trying to save the remote image.

            if (cbOTFIUseFlashAcquisition.Checked)          // PKv5.5.3
            {
                int tempFrameCount = Camera.GetFrameCount();
                if ((tempFrameCount - frameCountTriggeredAcquistion) == 1)
                {
                    Console.WriteLine("Mainform.cs - SUCCESS !! Acquired one fly-by inspection frame");
                }
                else
                {
                    Console.WriteLine("Mainform.cs - ERROR !!! Acquired {0} fly-by inspection frames", (tempFrameCount - frameCountTriggeredAcquistion));
                }

                if (cbDebugTrySavingBitmap.Checked)             // PKv5.5.0.5   2017-02-22   Try saving just the bitmap image.
                {
                    //                     string bitmapFileName = Camera.GetLastVideoFileName() + ".bmp";
                    string bitmapFileName = Camera.GetBMPFileName(Pixy.MachineParameters.VideoFileDirectory);
                    if (cbDGCPromptAndSave.Checked)   Camera.SaveImage(bitmapFileName);             // PKv5.6b  made this coniditional
                    if (cbDGCEnabled.Checked) Thread.Sleep(500);     // Give it 500 ms for the remote image to show up.
                    if (cbDGCPromptAndSave.Checked) SaveExperimentalImagesAndSettingsNoPrompt(remoteImageFullPath);  //  Should save setting and remote image (if remote image not ="").

                    int videoNumber = Camera.GetVideoNumber() - 1;                                                  // PKv5.6b
                    this.labelRecordingVideo.Text = "Finished Acquiring Bitmap:" + videoNumber.ToString();
                }
            }

            if (!cbOTFIUseFlashAcquisition.Checked)
            {
                btnStopRecording_Click(this, null);         // PKv5.5.3  Stop Recording the video.
                if (cbDGCEnabled.Checked) Thread.Sleep(500);     // Give it 500 ms for the remote image to show up.
                if (cbDGCPromptAndSave.Checked) SaveExperimentalImagesAndSettingsNoPrompt(remoteImageFullPath);          //  Should save setting and remote image (if remote image not ="").
            }


            if (checkBoxAutomaticPlunge.Checked)
            {
                LoggingUtility(" DoOTFPlunge - Plunge to ethane is complete");
  
                TransferToNitrogen();

            }
            else
            {
                LoggingUtility(" DoOTFPlunge - Plunge to view point is complete");

                // PKv5.5.3,  PKv5.5.4,  Turn them back on if they were turned off temporarily
                if (tempOffcbOTFIEnableSlowDown)
                {
                    cbOTFIEnableSlowDown.Checked = true;
                }

                if (tempOffcbOTFIUseFlashAcquisition)
                {
                    cbOTFIUseFlashAcquisition.Checked = true;
                }

                Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                    tempTipPosition, 0, 75, true, true, false, true);                                   // PKv5.6b  Move the tip back to the queued point

                MessageBox.Show("Plunge to lower grid camera camera complete\n\nHit OK to proceed (will move grid back to start)", "Answer me this", MessageBoxButtons.OK);

                Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                    tempPosition, 0.0, 10, true, true, UseGridRobot, true);                               // PKv5.6b Slowly move the grid back to the start position.

            }

            // PKv5.6b Turn back on live image ... this section of code is moved down here.
            if (cbOTFIUseFlashAcquisition.Checked)          // PKv5.5.3
            {
                Camera.StartLiveAcquistion();
            }

            DE03.StopWaveform();             // Should not be on but just in case.
            DE03.StopWaveform(true);        // 2019-01-24  Why not stop both of them !!

            IfMixingEnabledTurnBackOn();   // PKv5.3.1

            this.numQueuedCoordinates = 0;
            this.QueuedCoordinates.Clear();

            // PKv5.6
            if (cbDGCEnabled.Checked)
            {
                Console.WriteLine(" Next Remote image expected was ->", remoteImageFullPath);
            }
 //          SaveExperimentalImagesAndSettingsPrompt(remoteImageFullPath);
        }

        // PKv5.2.9  
        // Method which will read the combo box and return an enumberated parameter.

        private STN_MODE StainMode()
        {
            if (comboBoxSTNMode.SelectedIndex == (int)STN_MODE.Tips2_1) return STN_MODE.Tips2_1;
            if (comboBoxSTNMode.SelectedIndex == (int)STN_MODE.Tips3_1) return STN_MODE.Tips3_1;
            if (comboBoxSTNMode.SelectedIndex == (int)STN_MODE.Tips3_2) return STN_MODE.Tips3_2;
            if (comboBoxSTNMode.SelectedIndex == (int)STN_MODE.Tips3_future) return STN_MODE.Tips3_future;
            Console.WriteLine("MainForm.StainMode:  ERROR - Should never reach this section of the code");
            return STN_MODE.Tips3_future;
        }

        // PKv5.2.8
        // Routine which will run the spotting queue when in stain mode only.
        // PKv5.2.9 Expanded for 3 tip functionality

        private void btnSTNRunStainQueue_Click(object sender, EventArgs e)
        {

            LoggingUtility("Entering btnSTNRunStainQueue_Click routine, logic safety checks");

            #region - Logic Safety Checks (Don't break anything !!)

            if (!this.checkBoxExperimentMode.Checked)
            {
                Console.WriteLine("****Parameter Check Error-btnSTNRunStainQueue_Click,  Stain Mode must be checked... Returning");
                return;
            }

            if (StainMode() == STN_MODE.Tips3_future)
            {
                Console.WriteLine("****Parameter Check Error Stain Mode is 3Tip,Future -- Code Not Written Yet ");
                return;
            }

            if (this.numQueuedCoordinates == 0)
            {
                Console.WriteLine("****Parameter Check Error-btnSTNRunStainQueue_Click, No queued coordinates... Returning");
                return;
            }

            if (!(Pixy.MachineParameters.TipVideoCentered[0] && Pixy.MachineParameters.TipVideoCentered[1]))
            {
                Console.WriteLine("****Parameter Check Error-btnSTNRunStainQueue_Click, Need tip 1 and tip 2 centered... Returning");
                return;
            }

            // PKv5.2.9  Some special checks for 3 tip operation.

            if (StainMode() > STN_MODE.Tips2_1)
            {
                if (!Pixy.MachineParameters.TipVideoCentered[2])
                {
                    Console.WriteLine("****Parameter Check Error-btnSTNRunStainQueue_Click, tip 3 needs to be centered... Returning");
                    return;
                }

                // Make sure its an even number of queued coordinates
                if (this.numQueuedCoordinates % 2 != 0)
                {
                    Console.WriteLine("****Parameter Check Error-btnSTNRunStainQueue_Click, When using 3 tips you must have an even number of queued coordinates");
                }
            }

            LoggingUtility("Entering btnSTNRunStainQueue_Click routine, logic safety checks complete");


            #endregion

            int k;
            string message = "";

            int tempStrobeDelay = Pixy.MachineParameters.DE03TrapSetupStrobeDelay;
            Pixy.MachineParameters.DE03TrapSetupStrobeDelay = 0;

            if (StainMode() == STN_MODE.Tips2_1)
            {
                // 2 Tip Stain Mode.
                // [*]= Queued target. Operation is as follows:
                // [0]SpotTip1-DLY1-StainTip2,  [1]SpotTip1-DLY1-StainTip2, 
                #region STN_MODE.Tips2_1
                for (k = 0; k < this.numQueuedCoordinates; k++)
                {
                    Pixy.ActiveTip = 0;
                    DoGoToClickPosition(this.QueuedCoordinates[k]);

                    Pixy.MachineParameters.DE03TrapSetupTrapDrops = (int)numericUpDownSTNTip1Drops.Value;
                    DoDE03TrapSetupGUI();
                    DoDE03StartWaveformGUI(false, 0);
                    message = string.Format("  Tip 1 Sample Queue {0} Dispensed ", k);
                    LoggingUtility(message);

                    if ((int)numericUpDownSTNDelay_ms.Value > 0) Thread.Sleep((int)numericUpDownSTNDelay_ms.Value);
                    message = string.Format("  Delay_ms after sample applied= {0} ", (int)numericUpDownSTNDelay_ms.Value);
                    LoggingUtility(message);

                    Pixy.ActiveTip = 1;
                    DoGoToClickPosition(this.QueuedCoordinates[k]);

                    Pixy.MachineParameters.DE03TrapSetupTrapDrops = (int)numericUpDownSTNTip1Drops.Value;
                    DoDE03TrapSetupGUI();
                    DoDE03StartWaveformGUI(false, 0);
                    message = string.Format("  Tip 2 Stain Queue {0} Dispensed", k);
                    LoggingUtility(message);
                }
                #endregion
            }

            if (StainMode() == STN_MODE.Tips3_1)
            {
                // 3 Tip Stain Mode - 1
                // [*]= Queued target.  Operation is as follows:
                // [0]SpotTip1-DLY1-StainTip3, [1]SpotTip2-DLY2-StainTip3,  [2]SpotTip1-DLY1-StainTip3,  [3]SpotTip2-DLY2-StainTip3,  etc.
                #region STN_MODE.Tips3_1
                int logTip = 0;  // Used for the log instead of 0 based Pixy.ActiveTip (0 to 2),  logTip is 1 to 3
                int msDelay = 0;
                for (k = 0; k < this.numQueuedCoordinates; k++)
                {
                    if (k % 2 == 0)
                    {
                        Pixy.ActiveTip = 0;
                        logTip = 1;
                        msDelay = (int)numericUpDownSTNDelay_ms.Value;
                    }
                    else
                    {
                        Pixy.ActiveTip = 1;
                        logTip = 2;
                        msDelay = (int)numericUpDownSTNDelay2_ms.Value;
                    }

                    DoGoToClickPosition(this.QueuedCoordinates[k]);

                    Pixy.MachineParameters.DE03TrapSetupTrapDrops = (int)numericUpDownSTNTip1Drops.Value;
                    DoDE03TrapSetupGUI();
                    DoDE03StartWaveformGUI(false, 0);
                    message = string.Format("  Tip {0} Sample Queue {1} Dispensed ", logTip, k);
                    LoggingUtility(message);

                    if (msDelay > 0) Thread.Sleep(msDelay);

                    message = string.Format("  Delay_ms after sample applied= {0} ", msDelay);
                    LoggingUtility(message);

                    // Now add the stain with tip 3
                    Pixy.ActiveTip = 2;
                    DoGoToClickPosition(this.QueuedCoordinates[k]);

                    Pixy.MachineParameters.DE03TrapSetupTrapDrops = (int)numericUpDownSTNTip2Drops.Value;
                    DoDE03TrapSetupGUI();
                    DoDE03StartWaveformGUI(false, 0);
                    message = string.Format("  Tip 3 Stain Queue {0} Dispensed", k);
                    LoggingUtility(message);
                }
                #endregion
            }

            if (StainMode() == STN_MODE.Tips3_2)
            {
                // 3 Tip Stain Mode - 2
                // [*]= Queued target.  Operation is as follows:
                // [0]SpotTip1  [1]SpotTip2  DLY1  [0]StainTip3 [1] StainTip3,  [2]SpotTip1  [3]SpotTip2  DLY1 [2]StainTip3 [3] StainTip3,    etc...
                #region STN_MODE.Tips3_2
                int logTip = 0;  // Used for the log instead of 0 based Pixy.ActiveTip
                for (k = 0; k < this.numQueuedCoordinates; k++)
                {
                    if (k % 2 == 0)
                    {
                        Pixy.ActiveTip = 0;
                        logTip = 1;
                    }
                    else
                    {
                        Pixy.ActiveTip = 1;
                        logTip = 2;
                    }

                    DoGoToClickPosition(this.QueuedCoordinates[k]);

                    Pixy.MachineParameters.DE03TrapSetupTrapDrops = (int)numericUpDownSTNTip1Drops.Value;
                    DoDE03TrapSetupGUI();
                    DoDE03StartWaveformGUI(false, 0);
                    message = string.Format("  Tip {0} Sample Queue {1} Dispensed ", logTip, k);
                    LoggingUtility(message);

                    if (k % 2 != 0)
                    {
                        if ((int)numericUpDownSTNDelay_ms.Value > 0) Thread.Sleep((int)numericUpDownSTNDelay_ms.Value);
                        message = string.Format("  Delay_ms after samples applied= {0} ", (int)numericUpDownSTNDelay_ms.Value);
                        LoggingUtility(message);

                        Pixy.ActiveTip = 2;
                        DoGoToClickPosition(this.QueuedCoordinates[k - 1]);

                        Pixy.MachineParameters.DE03TrapSetupTrapDrops = (int)numericUpDownSTNTip2Drops.Value;
                        DoDE03TrapSetupGUI();
                        DoDE03StartWaveformGUI(false, 0);
                        message = string.Format("  Tip 3 Stain Queue {0} Dispensed", (k - 1));
                        LoggingUtility(message);

                        DoGoToClickPosition(this.QueuedCoordinates[k]);
                        Pixy.MachineParameters.DE03TrapSetupTrapDrops = (int)numericUpDownSTNTip2Drops.Value;
                        DoDE03StartWaveformGUI(false, 0);
                        message = string.Format("  Tip 3 Stain Queue {0} Dispensed", (k - 1));
                        LoggingUtility(message);
                    }
                }
                #endregion
            }

            this.numQueuedCoordinates = 0;
            this.QueuedCoordinates.Clear();
            Pixy.MachineParameters.DE03TrapSetupStrobeDelay = tempStrobeDelay;   // Put back the strobe delay
            return;
        }

        // Convenient way to clear the ENTIRE queue if you mistakely add a point(s)

        private void btnSTNClearQueue_Click(object sender, EventArgs e)
        {
            this.numQueuedCoordinates = 0;
            this.QueuedCoordinates.Clear();
            Console.WriteLine("     Number of queued targets = 0");
        }


        // PKv5.0 Edits below this line by Peter Kahn,  2/28/2016  to log precise timing.
        // PKv5.1  - Making a much faster plunge move,  attempt.
        // PKv5.2.0 - Added safety check.
        // PKv5.2.5 - Added OTF plunging.
        // Cyro mode only.

        private void checkBoxClickToSpot_Click(object sender, EventArgs e)
        {

            double gridRobotLastX = 0, gridRobotLastY = 0;  // PKv5.1

            LoggingUtility("Entering checkBoxClickToSpot_Click routine");   // PKv5.0 Logging the timing

            // PKv5.2.0 , PKv5.2.5 Add a safety check to make sure that if we NOT clearing tips out of the way 
            // we must be using the bent tweezer points.
            // More logic safety checks added for number of queued spots !=0,  On the fly

            #region - Logic Safety Checks (Don't break anything !!)

            if (!this.checkBoxClickToSpot.Checked)
            {
                Console.WriteLine("****Parameter Check Error-ClickToSpot_Click,  ClickToSpot must be checked.. Returning");
                return; //PKv5.1  Will just return and do nothing.
            }

            if (!this.checkBoxCryoMode.Checked)
            {
                Console.WriteLine("****Parameter Check Error-ClickToSpot_Click,  CryoMode must be checked.. Returning");
                return; //PKv5.1  Will just return and do nothing.
            }

            if ((!Pixy.MachineParameters.ClearTipsB4Plunge) & (!Pixy.MachineParameters.UseBentTweezerPoints))
            {
                Console.WriteLine("****Parameter Check Error-ClickToSpot_Click,  Must Clear Tips B4 Plunge unless using bent tweezers...Returning");
                MessageBox.Show("MainForm.cs: Must Clear Tips B4 Plunge unless using bent tweezers,  Returning  ERROR");
                return;
            }

            if (this.numQueuedCoordinates == 0)
            {
                Console.WriteLine("****Parameter Check Error-ClickToSpot_Click, No queued coordinates... returning");
                LoggingUtility("  No queued coordinates,  returning ");
                return;
            }

            double inEthaneZ;  //PKv5.2.5, Preliminary check for moving out of ethane
            inEthaneZ = Pixy.MachineParameters.InLiquidEthaneBowl.Z;
            if (Pixy.MachineParameters.UseBentTweezerPoints) inEthaneZ = Pixy.MachineParameters.InLiquidEthaneBowlBent.Z;
            double aboveDistance_mm = Pixy.MachineParameters.AboveLiquidNitrogen.Z - inEthaneZ;
            if (aboveDistance_mm < 7.0)
            {
                Console.WriteLine("****Parameter Check Error-ClickToSpot_Click, AboveLiquidNitrogen.Z - InLiquidEthanceBowl(Bent) is less than 10mm");
                return;  // Do nothing just return.
            }

            #endregion

            if (cbOTF_Enable.Checked)
            {
                // PKv5.2.5  
                DoOTFPlunge();  // Updated for mixing
                this.checkBoxClickToSpot.Checked = false;  // PKv5.2.5
                return;
            }


            int k;
            string message = "";

            IfMixingEnabledTurnTempOff();   // PKv5.3.1

            for (k = 0; k < this.numQueuedCoordinates; k++)
            {
                #region Cycling thru queued coordiates
                DoGoToClickPosition(this.QueuedCoordinates[k]);
                // PKv5.1  - Remember the last position it moved to,  important if we want to try an unsafe move
                if (k == (this.numQueuedCoordinates - 1))
                {
                    MachineCoordinate tempPosition = new MachineCoordinate();
                    Pixy.MotionControl.GetCurrentPosition(out tempPosition, true);   // Grid robot
                    gridRobotLastX = tempPosition.X;
                    gridRobotLastY = tempPosition.Y;
                }

                Pixy.MachineParameters.DE03TrapSetupTrapDrops = Pixy.MachineParameters.NumberDropsToSpot[Pixy.ActiveTip];
                if (k == 0)
                {
                    int tempStrobeDelay = Pixy.MachineParameters.DE03TrapSetupStrobeDelay;
                    Pixy.MachineParameters.DE03TrapSetupStrobeDelay = 0;     // PKv5.0     ,  get rid of the strobe delay while at grid camera on first spot only.
                    DoDE03TrapSetupGUI();
                    Pixy.MachineParameters.DE03TrapSetupStrobeDelay = tempStrobeDelay;
                }
                if ((k == (this.numQueuedCoordinates - 1)) && this.checkBoxAutomaticPlunge.Checked)   //PKv5.1 Only open the shutter if automatic plunging and on last coordinate
                {
                    // open shutter
                    IO.SetOutput(2);
                    LoggingUtility("  Starting Shutter Open");                   // PKv5.0 Logging the timing
                    IO.WaitInput(-1, 1000);  // Wait for the shutter to open
                    LoggingUtility("  Found Shutter Open");                     // PKv5.0 Logging the timing
                }
                DoDE03StartWaveformGUI(false, 0);
                message = string.Format("  Spot Position {0} ", k);      // PKv5.0 Log timing of each spot
                LoggingUtility(message);                                 // PKv5.0 Log timing of each spot
                #endregion Cycling thru queued coordiates
            }

            this.numQueuedCoordinates = 0;
            this.QueuedCoordinates.Clear();

            // Automatic plunge portion
            if (this.checkBoxAutomaticPlunge.Checked)
            {
                
                #region Plunge into the ethane

                bool UseGridRobot = true;

                // Plunge grid into liquid ethane

                if (Pixy.MachineParameters.ClearTipsB4Plunge)               // PKv5.1
                {
                    MachineCoordinate plungeAwayPosition = new MachineCoordinate();
                    Pixy.MotionControl.GetCurrentPosition(out plungeAwayPosition);
                    plungeAwayPosition.X += 85.0;   // 2016-03-29, Changed from 70 to 85.
                    Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                                        plungeAwayPosition, 0.0, 100, true, false, false, false);
                }

                // If we are not close do a safe plunge  else  do an unsafe plunge (all 3 axis start moving).

                if ((Math.Abs(gridRobotLastX - Pixy.MachineParameters.InLiquidEthaneBowl.X) > 3.0) || (Math.Abs(gridRobotLastY - Pixy.MachineParameters.InLiquidEthaneBowl.Y) > 3.0))
                {
                    if (Pixy.MachineParameters.UseBentTweezerPoints)
                        Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                Pixy.MachineParameters.InLiquidEthaneBowlBent, 0.0, 100, true, true, UseGridRobot, false);
                    else
                        Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                Pixy.MachineParameters.InLiquidEthaneBowl, 0.0, 100, true, true, UseGridRobot, false);
                    // PKv5.1  - Might want to change last parameter back to false ,  in case a user forgot to hit the GOTO camera button.
                    LoggingUtility("  Plunge Complete, with safe move");       // PKv5.1 Log timing
                }
                else
                {
                    // PKv5.1
                    if (Pixy.MachineParameters.UseBentTweezerPoints)
                        Pixy.MotionControl.MoveUnSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                Pixy.MachineParameters.InLiquidEthaneBowlBent, 0.0, 100, true, true, UseGridRobot, true);
                    else
                        Pixy.MotionControl.MoveUnSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                Pixy.MachineParameters.InLiquidEthaneBowl, 0.0, 100, true, true, UseGridRobot, true);
                    LoggingUtility("  Plunge Complete with unsafe move");       // PKv5.1 Logging the timing
                }
                #endregion Plunge into the ethane

                IfMixingEnabledTurnBackOn();   // PKv5.3.1

                TransferToNitrogen();

            }

            this.checkBoxClickToSpot.Checked = false;  // PKv5.2.5
            LoggingUtility("Exiting checkBoxClickToSpot_Click routine");        // PKv5.0 Logging the timing

        }

        // PKv5.2.5
        // Enters this routine just after it has plunged into the liquid ethane.
        // Clears away pipette tips (if they were not cleared away before plunge)
        // Transfer to nitrogen and release tweezers.
        // PKv5.3.2 - Updated to test out that very annoying delay between moves.
        // PKv5.3.3 - Update for a very fast hop to the nitrogen.

        private void TransferToNitrogen()
        {
            bool UseGridRobot = true;
            int CryoDropLocationIndex = 0;

            if (!Pixy.MachineParameters.ClearTipsB4Plunge)
            {
                MachineCoordinate plungeAwayPosition = new MachineCoordinate();
                Pixy.MotionControl.GetCurrentPosition(out plungeAwayPosition);
                plungeAwayPosition.X += 85.0;   // 2016-03-29, Changed from 70 to 85.
                Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                              plungeAwayPosition, 0.0, 100, true, false, false, false);
            }

            // Ask where the user wants to put the frozen grid
            CryoDropLocationIndex = Prompt.ShowDialog("Cryo Grid Drop off Location", "Drop location?");

            //MessageBox.Show("Index " + CryoDropLocationIndex);

            // Quickly move the grid from where it is in the liquid ethane to above liquid ethane
            double inEthaneZ;
            inEthaneZ = Pixy.MachineParameters.InLiquidEthaneBowl.Z;
            if (Pixy.MachineParameters.UseBentTweezerPoints) inEthaneZ = Pixy.MachineParameters.InLiquidEthaneBowlBent.Z;
            double aboveDistance_mm = Pixy.MachineParameters.AboveLiquidNitrogen.Z - inEthaneZ;    // Note there is no AboveLiquidNitrogentBent.

            // PKv5.3.3

            if (Pixy.MachineParameters.HOPEnable)
            {
                #region using the HOP
                double downDistance_mm;
                if (Pixy.MachineParameters.UseBentTweezerPoints)
                    downDistance_mm = Pixy.MachineParameters.AboveLiquidNitrogen.Z - Pixy.MachineParameters.GridDropLocationBent[CryoDropLocationIndex].Z;
                else
                    downDistance_mm = Pixy.MachineParameters.AboveLiquidNitrogen.Z - Pixy.MachineParameters.GridDropLocation[CryoDropLocationIndex].Z;

                double yDistance_mm;  // Should always be a positive value.
                if (Pixy.MachineParameters.UseBentTweezerPoints)
                    yDistance_mm = Pixy.MachineParameters.GridDropLocationBent[CryoDropLocationIndex].Y - Pixy.MachineParameters.InLiquidEthaneBowlBent.Y;
                else
                    yDistance_mm = Pixy.MachineParameters.GridDropLocation[CryoDropLocationIndex].Y - Pixy.MachineParameters.InLiquidEthaneBowl.Y;

                Console.WriteLine("TransferToNitrogen.cs  Fast Hop Debug up={0}, y={1},down={2},ZCorner={3},YCorner={4},Settle={5}",
                    aboveDistance_mm, yDistance_mm, downDistance_mm, Pixy.MachineParameters.HOPZ2CornerCut_mm,
                    Pixy.MachineParameters.HOPY2CornerCut_mm, Pixy.MachineParameters.HOPSettleTime_ms);

                // Z Axis,  Acel,Decel and speed for the hop move.
                Pixy.MotionControl.SetAccel(Pixy, 2, Pixy.MachineParameters.HOPZ2Accel_mm_ss, Pixy.MachineParameters.HOPZ2Decel_mm_ss, UseGridRobot);
                Pixy.MotionControl.SetMaxVelocity(Pixy, 2, Pixy.MachineParameters.HOPZ2Speed_mm_s, UseGridRobot, true);  // Write to the motor.

                // Y Axis,  Acel,Decel and speed for the hop move.
                Pixy.MotionControl.SetAccel(Pixy, 1, Pixy.MachineParameters.HOPY2Accel_mm_ss, Pixy.MachineParameters.HOPY2Decel_mm_ss, UseGridRobot);
                Pixy.MotionControl.SetMaxVelocity(Pixy, 1, Pixy.MachineParameters.HOPY2Speed_mm_s, UseGridRobot, true);  // Write to the motor.

                // Put Breakpoint here,  Make sure parameters in the HopZYZ make sense.  PKv5.3.3

                Pixy.MotionControl.HopZYZ(aboveDistance_mm, yDistance_mm, downDistance_mm, Pixy.MachineParameters.HOPZ2CornerCut_mm,
                    Pixy.MachineParameters.HOPY2CornerCut_mm, Pixy.MachineParameters.HOPSettleTime_ms, UseGridRobot);

                // Z Axis,  Acel,Decel and speed back to general settings.
                Pixy.MotionControl.SetAccel(Pixy, 2, Pixy.MachineParameters.PlungeAxisAccel_General, Pixy.MachineParameters.PlungeAxisAccel_General, UseGridRobot);
                Pixy.MotionControl.SetMaxVelocity(Pixy, 2, Pixy.MachineParameters.PlungeAxisMaxSpeed_General, UseGridRobot);

                // Y Axis,  Acel,Decel and speed back to general settings.
                Pixy.MotionControl.SetAccel(Pixy, 1, Pixy.MachineParameters.Y2AccelGen_mm_ss, Pixy.MachineParameters.Y2DecelGen_mm_ss, UseGridRobot);
                Pixy.MotionControl.SetMaxVelocity(Pixy, 1, Pixy.MachineParameters.Y2SpeedGen_mm_s, UseGridRobot);

                #endregion

            }
            else
            {
                #region Not using the HOP,  the old way.

                Pixy.MotionControl.SetUseLastPositionTest(Pixy, true);       // PKv5.3.2

                if (Pixy.MachineParameters.UseBentTweezerPoints)
                    Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                       Pixy.MachineParameters.InLiquidEthaneBowlBent, aboveDistance_mm, 100, true, false, UseGridRobot, false);
                else
                    Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                       Pixy.MachineParameters.InLiquidEthaneBowl, aboveDistance_mm, 100, true, false, UseGridRobot, false);

                // Quickly move the grid from above liquid nitrogen to cryo grid drop off location 
                if (Pixy.MachineParameters.UseBentTweezerPoints)
                    Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                      Pixy.MachineParameters.GridDropLocationBent[CryoDropLocationIndex], 0.0, 100, true, true, UseGridRobot, false);
                else
                    Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                        Pixy.MachineParameters.GridDropLocation[CryoDropLocationIndex], 0.0, 100, true, true, UseGridRobot, false);

                Pixy.MotionControl.SetUseLastPositionTest(Pixy, false);         // PKv5.3.2

                #endregion

            }

            if (!cbDebugDisableGridDrop.Checked)
            {
                // open the tweezers
                IO.SetOutput(-3);
            }
            else
            {
                Console.WriteLine("***** Opening tweezer is disabled on debug panel");
            }

            //wait for user to click ok to continue
            MessageBox.Show("Continue, robot will go to Away position");
            // move the grid robot to "away" from camera position
            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                    Pixy.MachineParameters.SafePointGrid, 0.0, 25, true, true, UseGridRobot, false);

            IO.SetOutput(3);  // Close the tweezer  PKv5.1 

            //close the shutter
            IO.SetOutput(-2);
        }


        // PKv5.0,  Added the LoggingUtility method.

        private void LoggingUtility(string logMessage)  // Timestamping to disk method
        {
            Datalog log = new Datalog(@"..\LogFiles\TSLog.txt");
            log.WriteLine(logMessage + "," + Datalog.Timestamp());
            log.Close();
            Console.WriteLine(logMessage + Datalog.Timestamp());   // 
        }


        //this function adds the last "clicked coordinates" from the video to the QueuedCoordinates
        private void addQueueTargets_Click(object sender, EventArgs e)
        {

            if (!this.checkBoxClickToSpot.Checked)
            {

                if (this.QueuedCoordinates.Count == 0)
                {

                    this.QueuedCoordinates.Add(clickedCoordinates);
                    this.numQueuedCoordinates = this.numQueuedCoordinates + 1;
                    Console.WriteLine("Target added to queue. Number of Queued Targets: {0}", this.numQueuedCoordinates);

                }

                else if ((this.QueuedCoordinates.Count > 0) && (this.QueuedCoordinates[this.numQueuedCoordinates - 1] != clickedCoordinates))
                {

                    this.numQueuedCoordinates = this.numQueuedCoordinates + 1;
                    this.QueuedCoordinates.Add(clickedCoordinates);
                    Console.WriteLine("Target added to queue. Number of Queued Targets: {0}", this.numQueuedCoordinates);

                }
                else { }
            }
            else { }
        }



        // Ivan added: this function is called when we click the "Center Tip" button
        // This function calibrates the center of the "Back" camera view and determines
        // the corresponding robot coordinates
        // Function flow: 1) moves the robot to an initial position specified in the xml file
        // 2) Using image recognition we determine the center of the tip on the camera view in pixel
        // 3) Perform this centering 50 times and average, to diminish effects of any erroneous output
        // 3.a) Probaly we should do step 3 recursively until the step's error is less than some small error that we specify
        // 4) move the robot by a predetermined amount, in out case 0.2mm 
        // 5) recalculate the center
        // 6) given the second point we caculate the relationship between movements in millimeters to movements in pixels...aka. scale_x and scale_y
        // 7) calculate the center point of the video feed and assign that value to robot's Machine Parameters
        // 8) go to the calculated position
        private void btnCenterTip_Click(object sender, EventArgs e)
        {

            bool UseGridRobot = false;
            checkBoxDebugEnableStrobe.Checked = false;          // Turn off light before switching to new camera.
            Thread.Sleep(50);
            Camera.setSwitchCamera("Back");
            lblActiveCam.Text = "(Grid)";
 
            //move the grid robot to the safe position
            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                          Pixy.MachineParameters.SafePointGrid, 0.0, 15, true, true, true, false);

            DoMoveToPointOrderedGUI(Pixy.MachineParameters.BackCameraPointTips[Pixy.ActiveTip], 0, 0, 0,
                                    Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                    100, true, Pixy.MachineParameters.ThetaRotate_90, 1, 3, 2, UseGridRobot);

            Bitmap pictureInput;
            PointF centerOut;
            CircleDetection CD = new CircleDetection();

            MachineCoordinate initial_position = new MachineCoordinate();
            MachineCoordinate new_position = new MachineCoordinate();
            double delta_x = 1.20; //initial step size  // PKv5.5.0.3   Quick fix to adjust for smaller FOV of the grid camera. 2/20/2017
            double delta_y = 0.60; //initial step size
            double scale_x = 0; // mm/pixel
            double scale_y = 0; // mm/pixel
            int i = 0;
            double average_y = 0;
            double average_x = 0;

            double new_average_y = 0;
            double new_average_x = 0;
            int numberOfIterations = 100;

            //initial center recognition
            for (i = 0; i < numberOfIterations; i++)
            {
                pictureInput = Camera.GetCenterofTipOnFrame();
                centerOut = CD.getCircleCenter(pictureInput);
                average_y += centerOut.Y;
                average_x += centerOut.X;
                ShowStatusMessage("Calibrating: " + Decimal.Divide(i + 1, numberOfIterations) * 100 + "%");

            }
            average_y /= 100;
            average_x /= 100;
            int x_sign, y_sign;
            movementDirectionSignPicker(average_x, average_y, 376, 240, out x_sign, out y_sign);

            Pixy.MotionControl.GetCurrentPosition(out initial_position);

            //move robot by delta_y 
            new_position = new MachineCoordinate(initial_position);
            new_position.Z = initial_position.Z + y_sign * delta_y;
            new_position.Y = initial_position.Y + x_sign * delta_x;
            DoMoveToPointGUI(new_position, 0, 0, 0, Pixy.MachineParameters.Tips[Pixy.ActiveTip], 15, false, true);
            Pixy.MotionControl.GetCurrentPosition(out new_position);

            for (i = 0; i < numberOfIterations; i++)
            {
                pictureInput = Camera.GetCenterofTipOnFrame();
                centerOut = CD.getCircleCenter(pictureInput);
                new_average_y += centerOut.Y;
                new_average_x += centerOut.X;
                ShowStatusMessage("Calibrating: " + Decimal.Divide(i + 1, numberOfIterations) * 100 + "%");
            }
            new_average_y /= 100;
            new_average_x /= 100;
            scale_x = Math.Abs((new_position.Y - initial_position.Y) / (new_average_x - average_x));
            scale_y = Math.Abs((new_position.Z - initial_position.Z) / (new_average_y - average_y));
            Pixy.MachineParameters.VideoToMotionScaler_X = scale_x;
            Pixy.MachineParameters.VideoToMotionScaler_Y = scale_y;

            MessageBox.Show("scale X = " + scale_x + "scale Y=" + scale_y);

            //move to center
            movementDirectionSignPicker(new_average_x, new_average_y, 376, 240, out x_sign, out y_sign);
            new_position.Y += scale_x * x_sign * Math.Abs((376 - new_average_x));
            new_position.Z += scale_y * y_sign * Math.Abs((240 - new_average_y));

            Pixy.MachineParameters.CalculatedVideoCenterPoint[Pixy.ActiveTip] = new_position;
            Pixy.MachineParameters.TipVideoCentered[Pixy.ActiveTip] = true;
            UpdateTipVideoCenterDisplay(Pixy.ActiveTip);

            DoMoveToPointGUI(Pixy.MachineParameters.CalculatedVideoCenterPoint[Pixy.ActiveTip], 0, 0, 0, Pixy.MachineParameters.Tips[Pixy.ActiveTip], 15, false, true);

        }

        // Ivan added: this function is called when you click the "Go to Camera" button from the "Inspect Drop" tab
        private void btnTipToSideCamera_Click(object sender, EventArgs e)
        {
            bool UseGridRobot = false;
            // disable experiment mode
            MachineCoordinate currentPosition;
            // bool flag for moving to safe position
            bool moveToSafeHeight = true;

            if (!Pixy.MachineParameters.PKDevSystemEnable)          // PKv5.5.0
            {

                // Move grid to safe position in case it is in the way.
                Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                              Pixy.MachineParameters.SafePointGrid, 0.0, 75, true, true, true, false);
            }

            checkBoxDebugEnableStrobe.Checked = false;          // Turn off light before switching to new camera.
            Thread.Sleep(50);
            Camera.setSwitchCamera("Side");
            lblActiveCam.Text = "(Tip)";

            // // PKv5.5.0   Turn it back on...
            checkBoxDebugEnableStrobe.Checked = false;
            Thread.Sleep(100);
            checkBoxDebugEnableStrobe.Checked = true;
            Thread.Sleep(100);


            if (this.checkBoxExperimentMode.Checked)
            {
                this.checkBoxExperimentMode.Checked = !this.checkBoxExperimentMode.Checked;
            }

            Pixy.MotionControl.GetCurrentPosition(out currentPosition);

         //   if (!Pixy.MachineParameters.PKDevSystemEnable)          // PKv5.5.0,   // 2019-01-16   Lets use this on dev system too.
            {
                CheckPointProximity(out moveToSafeHeight, currentPosition, Pixy.MachineParameters.SideCamInspectPointTips[Pixy.ActiveTip]);
            }

            DoMoveToPointOrderedGUI(Pixy.MachineParameters.SideCamInspectPointTips[Pixy.ActiveTip], 0, 0, 0,
                                        Pixy.MachineParameters.Tips[Pixy.ActiveTip], 100, moveToSafeHeight, Pixy.MachineParameters.ThetaRotate_0, 1, 3, 2, UseGridRobot);
            //  DoRotateMoveGUI(Pixy.MachineParameters.ThetaRotate_0);
        }

        // Ivan added: function is called when you click the "Choose Video Folder" button
        // Read the folder and assign it to the Robot's Machine Parameters
        private void btnChooseVideoFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxVideoFolder.Text = folderBrowserDialog1.SelectedPath;
                Pixy.MachineParameters.VideoFileDirectory = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnStartStopVideoRecording_Click(object sender, EventArgs e)
        {
            int videoNumber = 0;
            videoNumber = Camera.RecordVideo(Pixy.MachineParameters.VideoFileDirectory,(int)numericUpDownDebugJPG.Value,(int)numericUpDownDebugVideoWidth.Value);
            this.labelRecordingVideo.ForeColor = System.Drawing.Color.Green;
            this.labelRecordingVideo.Text = "Recording Video No:" + videoNumber.ToString();
        }

        private void btnStopRecording_Click(object sender, EventArgs e)
        {
            int videoNumber = 0;
            videoNumber = Camera.StopRecordingVideo();
            this.labelRecordingVideo.ForeColor = System.Drawing.Color.Red;
            this.labelRecordingVideo.Text = "Finished Recording Video No:" + videoNumber.ToString();
        }
        private void btnWriteSettingsToFile_Click(object sender, EventArgs e)
        {
            Pixy.WriteSettingsToFile();
        }
        private void btn_clickGotoBackCamera(object sender, EventArgs e)
        {
            //move the grid robot to the safe position
            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                          Pixy.MachineParameters.SafePointGrid, 0.0, 15, true, true, true, false);

            bool UseGridRobot = false;
            checkBoxDebugEnableStrobe.Checked = false;          // Turn off light before switching to new camera.
            Thread.Sleep(50);
            Camera.setSwitchCamera("Back");
            lblActiveCam.Text = "(Grid)";
            DoMoveToPointOrderedGUI(Pixy.MachineParameters.BackCameraPointTips[Pixy.ActiveTip], 0, 0, 0,
                                    Pixy.MachineParameters.Tips[Pixy.ActiveTip], 100, true, Pixy.MachineParameters.ThetaRotate_90, 1, 3, 2, UseGridRobot);
        }

        private int DoPlungeAxisMoveGUI(double MovePos_mm, double MoveSpeed_pct, double ZOffset)
        {
            // Process_PlungeAxisMove Cmd = PA as Process_PlungeAxisMove;
            // if (Cmd == null) throw new Exception("Unexpected command type in DoPlungeAxisMove: " + PA.Name);

            //double MovePos_mm = mVM.GetDoubleFromText(Cmd.PositionZ2);

            //double MoveSpeed_pct = GetMoveSpeed_pct(Cmd.Speed_pct);
            //if (d.ZOffset != "") MovePos_mm = MovePos_mm + mVM.GetDoubleFromText(Cmd.ZOffset);

            // find out if user motion control was allowed
            bool AllowingManualControl = Pixy.MotionControl.ManualControlEnabled;

            try
            {
                // lock out the user motion control
                Pixy.MotionControl.EnableManualControl(Pixy, false);

                // say that we're moving
                ShowStatusMessage("Moving Plunge Axis to " + MovePos_mm);

                // then do the move

                // if (Pixy.MotionControl.MoveZ2Only(Pixy, MovePos_mm, MoveSpeed_pct, true) != 0) return 1;
                //               if (Pixy.MotionControl.MoveSafely(Pixy, Tool, MachinePt, 0.0, MoveSpeed_pct, true, true) != 0) return 1;
            }
            finally
            {
                // allow user control again if it was allowed before
                Pixy.MotionControl.EnableManualControl(Pixy, AllowingManualControl);
            }

            return 0;
        }
        /*    private int DoSuperPlungeGUI(double MovePos_mm, double MoveSpeed_pct)
            {
                // Process_SuperPlunge Cmd = PA as Process_SuperPlunge;
                // if (Cmd == null) throw new Exception("Unexpected command type in DoPlungeAxisMove: " + PA.Name);

                bool skipMove = true;
                //double MovePos_mm = 0.0;
                //double MoveSpeed_pct = 10.0;
             //   int StartSolenoid = IO.OutEthaneDown;
            //    int StartSensor = IO.InNOTEthaneUp;
             //   int EndSensor = IO.InEthaneDown;
                //if (Cmd.PositionZ2 != "")
                //{
                //  MovePos_mm = mVM.GetDoubleFromText(Cmd.PositionZ2);
                if (MovePos_mm > -900) skipMove = false;
                //  MoveSpeed_pct = GetMoveSpeed_pct(Cmd.Speed_pct);
                //}

                Datalog log;
                int st = System.Environment.TickCount;   // Easier to read timing,  all in msec
                int et;

                try
                {
                    log = new Datalog(@"C:\DE03 Syringe Script App\LogFiles\SuperPlungeLog.txt", true);
                    log.WriteLine("-------- Entering SuperPlunge Command ----------");
                    log.WriteLine("Start:" + Datalog.Timestamp());
                    et = System.Environment.TickCount - st;
                    log.WriteLine("Elapsed Time:" + et.ToString());
                    log.WriteLine("   StartSolenoid={0}, StartSensor={1}, EndSensor={2}, skipMove={3}", StartSolenoid, StartSensor, EndSensor, skipMove);
                    if (!skipMove)
                    {
                        log.WriteLine("   MovePos_mm={0}, MoveSpeed_pct={1}", MovePos_mm, MoveSpeed_pct);
                    }
                }
                catch
                {
                    MessageBox.Show("MainForm.cs: DoSuperPlunge  Unable to open log file");
                    return -1;
                }


                // find out if user motion control was allowed
                bool AllowingManualControl = Pixy.MotionControl.ManualControlEnabled;

                try
                {

                    // lock out the user motion control
                    Pixy.MotionControl.EnableManualControl(Pixy, false);

                    // Fire the start solenoid
                    if (StartSolenoid > -900)
                    {
                        IO.SetOutput(StartSolenoid);
                        et = System.Environment.TickCount - st;
                        log.WriteLine("Elapsed Time: " + et.ToString());
                    }

                    // Wait for the input sensor to fire
                    if (StartSensor > -900)
                    {
                        int ret = IO.WaitInput(StartSensor, 10000);        //  Will wait up to 10 sec for sensor to fire
                        et = System.Environment.TickCount - st;
                        log.WriteLine("StartSensor: " + et.ToString());
                        if (ret != 0)
                        {
                            log.WriteLine("StartSensor Timeout");
                            return ret;
                        }
                    }

                    bool moveDone = false;
                    // Start the optional move
                    if (skipMove)
                    {
                        moveDone = true;
                    }
                    else
                    {
                        if (Pixy.MotionControl.MoveZ2Only(Pixy, MovePos_mm, MoveSpeed_pct, false) != 0) return 1;
                        et = System.Environment.TickCount - st;
                        log.WriteLine("StartMotion: " + et.ToString());
                    }

                    bool ioDone = false;
                    // double actualPosition;

                    if (EndSensor < -900)   // Check to see if we will even look for the end sensor
                    {
                        ioDone = true;
                    }

                    int startMotionTs = System.Environment.TickCount;
                    do
                    {
                        if (!moveDone)
                        {
                            if (Pixy.MotionControl.IsZ2MoveDone())
                            //Pixy.MotionControl.GetCurrentPosition(PixyControl.ServoControl.Z2_AXIS,out actualPosition);
                            //if (Math.Abs(actualPosition-MovePos_mm)<0.1)   // Close enough to end point ??
                            {
                                moveDone = true;
                                et = System.Environment.TickCount - st;
                                log.WriteLine("EndMotion: " + et.ToString());
                                et = System.Environment.TickCount - startMotionTs;
                                log.WriteLine("EndMotion - StartMotion: " + et.ToString());
                            }
                        }
                        if (!ioDone)
                        {


                            if (IO.ReadInput(EndSensor))
                            {
                                ioDone = true;
                                et = System.Environment.TickCount - st;
                                log.WriteLine("EndSensor: " + et.ToString());
                                et = System.Environment.TickCount - startMotionTs;
                                log.WriteLine("EndSensor - StartMotion: " + et.ToString());
                            }
                        }
                        if ((System.Environment.TickCount - startMotionTs) > 20000)             // 20 seconds
                        {
                            string mess1 = string.Format("Error Time Out,  Motion Done={0}, End Sensor Found ={1}", moveDone, ioDone);
                            log.WriteLine(mess1);
                            MessageBox.Show(mess1);
                            return 1;
                        }
                        if (ioDone && moveDone) return 0;

                    } while (true);

                }
                finally
                {
                    Pixy.MotionControl.WaitForEndOfZ2MotionOnly(1000);    // Cleanup all motion variables (move flag etc..)
                    // allow user control again if it was allowed before
                    Pixy.MotionControl.EnableManualControl(Pixy, AllowingManualControl);
                    log.Close();
                }
            }
         * */

        private int DoRotateMoveGUI(int positionCount)          //PKv4.0,2015-04-13  Need to update
        {
            return HarmonicDrive.Move(positionCount, true);
        }


        private int DoRotateSetupGUI(int accelCount, int decelCount, int maxVelCount)          //PKv4.0,2015-04-13  Need to update
        {
            return HarmonicDrive.SetParameters(maxVelCount, accelCount, decelCount);
        }
        #endregion ivan do edits

        private void btnGotoSafePosition_Click(object sender, EventArgs e)
        {
            //move the grid robot to the safe position   //PKv5.2.5
            if (!Pixy.MachineParameters.PKDevSystemEnable)                  // PKv5.5.0
            {
                Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                          Pixy.MachineParameters.SafePointGrid, 0.0, 15, true, true, true, false);
            }
            bool UseGridRobot = false;
            DoMoveToPointOrderedGUI(Pixy.MachineParameters.SafePoint, 0, 0, 0,
                                    Pixy.MachineParameters.Tips[Pixy.ActiveTip], 100, true, Pixy.MachineParameters.ThetaRotate_0, 1, 3, 2, UseGridRobot);
        }

        private void buttonWipeTips_Click(object sender, EventArgs e)
        {

            //move the grid robot to the safe position PKv5.2.5
            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                          Pixy.MachineParameters.SafePointGrid, 0.0, 15, true, true, true, false);

            IfMixingEnabledTurnTempOff();       // PKv5.3.1

            bool UseGridRobot = false;
            {
                int numberOfWipes = 0; //the number of wipes that we shall perform
                numberOfWipes = (int)numericUpDownNumberWipeTips.Value;


                if (numberOfWipes > 0)
                {
                    for (int i = 0; i < numberOfWipes; i++)
                    {
                        DoMoveToPointOrderedGUI(Pixy.MachineParameters.TipWipingPointStart, 0, 0, 0,
                                                Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                                100, true, Pixy.MachineParameters.ThetaRotate_0, 1, 3, 2, UseGridRobot);

                        DoMoveToPointGUI(Pixy.MachineParameters.TipWipingPointStop, 0, 0, 0,
                                           Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                           10, false, false);
                        //Thread.Sleep(Pixy.MachineParameters.WashTime);
                    }

                }

                DoMoveToPointGUI(Pixy.MachineParameters.SafePoint, 0, 0, 0,
                                           Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                           100, true, false);
            }

            IfMixingEnabledTurnBackOn();        // PKv5.3.1

        }

        private void CheckPointProximity(out bool moveToSafeHeight, MachineCoordinate currentPoint, MachineCoordinate nextPoint)
        {
            // this function return false if the points are close to each other 
            // returns true if the points are far away from each other 
            // this is done to be in line with "safe z" move of the DoMoveToPointGui and DoMoveToPointOrderedGui functions
            double offsetX = 0;
            double offsetY = 0;
            double offsetZ = 0;
            double proximityDistance = 20.0; // arbitrary distance in mm that we say is "close enough" so that we are safe to move the robot without going to "safe z height"

            offsetX = currentPoint.X - nextPoint.X;
            offsetY = currentPoint.Y - nextPoint.Y;
            offsetZ = currentPoint.Z - nextPoint.Z;

            if (Math.Abs(offsetX) < proximityDistance && Math.Abs(offsetY) < proximityDistance && Math.Abs(offsetZ) < proximityDistance)
                moveToSafeHeight = false;
            else moveToSafeHeight = true;
        }

        // PKv5.2.1 - Modified to set a temporary slower max velocity of the z axis and then put back the higher speed.
        //     Also added a small x offset to approach the point from the left which seems easier than
        //     If this is still too jerky consider adding plunge acceleration and other accel

        private void buttonPositionGrid_Click(object sender, EventArgs e)
        {
            bool UseGridRobot = true;

            Pixy.MotionControl.SetMaxVelocity(Pixy, 2, Pixy.MachineParameters.PlungeAxisMaxSpeed_General, UseGridRobot);
            Pixy.MotionControl.SetAccel(Pixy, 2, Pixy.MachineParameters.PlungeAxisAccel_General, Pixy.MachineParameters.PlungeAxisAccel_General, UseGridRobot);

            MachineCoordinate internalMachinePt = new MachineCoordinate();

            if (Pixy.MachineParameters.UseBentTweezerPoints)   // PKv5.1
            {
                internalMachinePt.X = Pixy.MachineParameters.GridToCameraPointBent.X - 1.0;   // Approach from 1mm ,  Is this correct
                internalMachinePt.Y = Pixy.MachineParameters.GridToCameraPointBent.Y;
                internalMachinePt.Z = Pixy.MachineParameters.GridToCameraPointBent.Z;
                Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                            internalMachinePt, 0.0, 15, true, true, UseGridRobot, false);
                Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                            Pixy.MachineParameters.GridToCameraPointBent, 0.0, 15, true, true, UseGridRobot, true);
            }
            else
            {
                internalMachinePt.X = Pixy.MachineParameters.GridToCameraPoint.X - 1.0;   // Approach from 1mm to left. 
                internalMachinePt.Y = Pixy.MachineParameters.GridToCameraPoint.Y;
                internalMachinePt.Z = Pixy.MachineParameters.GridToCameraPoint.Z;
                Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                    internalMachinePt, 0.0, 15, true, true, UseGridRobot, false);
                Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                          Pixy.MachineParameters.GridToCameraPoint, 0.0, 15, true, true, UseGridRobot, false);
            }

            // Set back to the fast plunging speed.
            Pixy.MotionControl.SetMaxVelocity(Pixy, 2, Pixy.MachineParameters.PlungeAxisMaxSpeed_Plunge, UseGridRobot);
            Pixy.MotionControl.SetAccel(Pixy, 2, Pixy.MachineParameters.PlungeAxisAccel_Plunge, Pixy.MachineParameters.PlungeAxisAccel_Plunge, UseGridRobot);

        }

        // PKv5.2.1 - Modified to set a temporary slower max velocity of the z axis and then put back the higher speed.

        private void buttonPositionGridToSafePoint_Click(object sender, EventArgs e)
        {
            bool UseGridRobot = true;

            Pixy.MotionControl.SetMaxVelocity(Pixy, 2, Pixy.MachineParameters.PlungeAxisMaxSpeed_General, UseGridRobot);
            Pixy.MotionControl.SetAccel(Pixy, 2, Pixy.MachineParameters.PlungeAxisAccel_General, Pixy.MachineParameters.PlungeAxisAccel_General, UseGridRobot);

            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                                          Pixy.MachineParameters.SafePointGrid, 0.0, 15, true, true, UseGridRobot, false);

            // Set back to the fast plunging speed.
            Pixy.MotionControl.SetMaxVelocity(Pixy, 2, Pixy.MachineParameters.PlungeAxisMaxSpeed_Plunge, UseGridRobot);
            Pixy.MotionControl.SetAccel(Pixy, 2, Pixy.MachineParameters.PlungeAxisAccel_Plunge, Pixy.MachineParameters.PlungeAxisAccel_Plunge, UseGridRobot);

        }

        private void tabControlRobot_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControlRobot.SelectedTab != this.tabControlRobot.TabPages["tabSpotSample"])
            {
                this.checkBoxClickToSpot.Checked = false;
                this.checkBoxExperimentMode.Checked = false;
            }
        }

        private void buttonWriteSettingsToXMLFile_Click(object sender, EventArgs e)
        {

            // PKv5.5.3   In case this was updated ... we need to save it.
            Pixy.MachineParameters.InsGridCamDelayAfterTrip = (int)this.numericUpDownOTFIDelayAfterTrip.Value;

            // PKv5.5.4
            Pixy.MachineParameters.OTFSlowDownPercent = (int) this.numericUpDownOTFIPercentWayToTarget.Value;
            Pixy.MachineParameters.OTFSlowDownSpeed = (int)this.numericUpDownOTFISlowSpeed.Value;

            Pixy.WriteSettingsToFile();
        }


        private void buttonArchiveExperimentSettings_Click(object sender, EventArgs e)
        {
            
            saveFileDialogArchiveOnly.FileName = "";      // Clear out the filename first
            if (saveFileDialogArchiveOnly.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialogArchiveOnly.FileName;
                // Archive the settings
                string settingsFileName = fileName + ".txt";
                ArchiveSettingsUtility(settingsFileName);
            }
        }

        private void ArchiveSettingsUtility(string fullPath)  // Will write a whole bunch of the critical settings to a file. 
        {
            try
            {
 //               string fullPath = String.Format(@"{0}/{1}.txt", Pixy.MachineParameters.VideoFileDirectory, this.textBoxArchiveFile.Text);
                Datalog log = new Datalog(fullPath);
                log.WriteLine("Note1: " + this.textBoxArchiveFile.Text + " ,  " + Datalog.Timestamp());
                log.WriteLine("Note2: " + this.textBoxArchiveNote.Text);   // Added 7/12/2016

                // TODO -  Use the checkBoxCryoMode and only save relevant data for cryo, record different data if you are doing staining.

                if (checkBoxAutomaticPlunge.Checked)                        //PKv5.6b
                    log.WriteLine(" Automatic Plunge - ENABLED ");
                else
                    log.WriteLine(" Automatic Plunge - DISABLED ");


                if (cbOTF_Enable.Checked)
                    log.WriteLine(" OTF Enabled");
                else
                    log.WriteLine(" OTF Disabled");

                bool useCos = true;     //   2019-01-24  Got rid of all references to....  cbOTF_UseCos.Checked
                if (useCos)
                    log.WriteLine(" OTF Cos");
                else
                    log.WriteLine(" OTF Trapezoid");

                string tempString = String.Format(" OTF Pause (msec) = {0}", numericUpDownPauseOTF.Value);
                log.WriteLine(tempString);

                tempString = String.Format(" Number Of Drops = {0}", numericUpDownSpotDropNumber.Value);
                log.WriteLine(tempString);

                log.WriteLine(" Key Machine Parameters:");

                tempString = String.Format("    PlungeAxisMaxSpeed_Plunge = {0}", Pixy.MachineParameters.PlungeAxisMaxSpeed_Plunge);
                log.WriteLine(tempString);

                tempString = String.Format("    PlungeAxisAccel_Plunge = {0}", Pixy.MachineParameters.PlungeAxisAccel_Plunge);
                log.WriteLine(tempString);

                tempString = String.Format("    OTFStartAbove_mm = {0}", Pixy.MachineParameters.OTFStartAbove_mm);
                log.WriteLine(tempString);

                tempString = String.Format("    OTFTargetZOffset_mm = {0}", Pixy.MachineParameters.OTFTargetZOffset_mm);
                log.WriteLine(tempString);

                tempString = String.Format("    OTFAccelPlunge_mm_ss = {0}", Pixy.MachineParameters.OTFAccelPlunge_mm_ss);
                log.WriteLine(tempString);

                tempString = String.Format("    OTFDecelPlunge_mm_ss = {0}", Pixy.MachineParameters.OTFDecelPlunge_mm_ss);
                log.WriteLine(tempString);

                tempString = String.Format("    OTFSpeedPlunge_mm_s = {0}", Pixy.MachineParameters.OTFSpeedPlunge_mm_s);
                log.WriteLine(tempString);

                tempString = String.Format("    OTFTrapDispSpeed_mm_s = {0}", Pixy.MachineParameters.OTFTrapDispSpeed_mm_s);
                log.WriteLine(tempString);

                tempString = String.Format("    OTFTrapStartOffset_mm = {0}", Pixy.MachineParameters.OTFTrapStartOffset_mm);
                log.WriteLine(tempString);

                tempString = String.Format("    OTFTrapDropSpacing_mm = {0}", Pixy.MachineParameters.OTFTrapDropSpacing_mm);
                log.WriteLine(tempString);

                log.WriteLine(" End of archive");  // Give a couple of linefeeds at the end.
                log.WriteLine(" ");
                log.WriteLine(" ");


                log.Close();
            }
            catch
            {
                MessageBox.Show("Archive Failed", "That sucks");
            }
        }

        // Add message box if Automatic Plunge checkbox checked

        private void checkBoxAutomaticPlunge_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAutomaticPlunge.Checked)
            {
                MessageBox.Show("Remember to toggle delay for sensor trip to \"Plunge\" on lower camera!\n\n\t\tClick OK to continue.", "", MessageBoxButtons.OK);
            }
        }
        // PKv5.2.8   If we are in cyro mode make sure staining mode is off.

        private void checkBoxCryoMode_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCryoMode.Checked)
            {
                checkBoxExperimentMode.Checked = false;   // Make sure staining mode is off.
            }
        }

        // PKv5.2.8   If we are in staining mode make sure cyro mode is off.

        private void checkBoxExperimentMode_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxExperimentMode.Checked)
            {
                checkBoxCryoMode.Checked = false;   // Make sure cryo mode is off.
            }
        }

        // PKv5.3.1  

        private void checkBoxMixOn_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxMixOn.Checked)
            {
                DoDE03StartMixingGUI();
            }
            else
            {
                DoDE03StopMixingGUI();
            }
        }

        /*   PKv5.6   Took out the spinning motor from the debug panel to make room.

        private void checkBoxDebugSpinMotor_CheckedChanged(object sender, EventArgs e)
        {
            // PKv5.4.0,  Prototype to spin the motor on development system in Phoenix.  
            // Not to be used on actual Spotiton System.
            // Motor step resolution was set and saved in non-volitile memory,  40000 steps per revolution  "SP 200"
            // Default accelerations and max velocity need to be set correctly.

            updateCalcuatedSpeedDebugLabel();

            if (checkBoxDebugSpinMotor.Checked)
            {
                double step_per_sec = 40000 * (double) numericUpDownDebugRevPerSec.Value;
                string command = string.Format("SL {0:0}", step_per_sec);
                PixyControl.ServoControl.SendCommand("4", command);
            }
            else
                PixyControl.ServoControl.SendCommand("4", "SL 0");
        }

        

        private void updateCalcuatedSpeedDebugLabel()
        {
            double circumfrance = 2 * Math.PI * (double)numericUpDownDebugTargetDistance_mm.Value;
            double speed = (double)numericUpDownDebugRevPerSec.Value* circumfrance;
            string label = string.Format("Calculated Speed Of Target = {0:0} mm/sec",speed);
            labelDebugCalculatedTargetSpeed.Text = label;
        }



        private void numericUpDownDebugRevPerSec_ValueChanged(object sender, EventArgs e)
        {
            updateCalcuatedSpeedDebugLabel();
        }

        private void numericUpDownDebugTargetDistance_mm_ValueChanged(object sender, EventArgs e)
        {
            updateCalcuatedSpeedDebugLabel();
        }

         */

        private void checkBoxDebugEnableStrobe_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxDebugEnableStrobe.Checked)
            {
                Camera.EnableStrobe((int)numericUpDownDebugStrobeDuration.Value, (int)numericUpDownDebugStrobeDelay.Value);
            }
            else
            {
                Camera.DisableStrobe();
            }
        }

        // PKv5.5.0.1   Gain read and set

        private void btnCamReadGain_Click(object sender, EventArgs e)
        {
            numericUpDownCamGain.Value = (decimal)Camera.getGain();            // PKv4.3.2
        }


        private void btnCamSetGain_Click(object sender, EventArgs e)
        {
            Camera.setGain((int)numericUpDownCamGain.Value);                   // PKv4.3.2
        }

        // PKv5.5.0.1   Saving parameters to and from files.


        private void btnCamLoadSettingsFromFile_Click(object sender, EventArgs e)
        {
            Camera.LoadCamParametersFromFile("");
            trackBarPixelClock_Changed(this, null);
            trackBarExposure_Changed(this, null);
            trackBarFPS_Changed(this, null);
            btnCamReadGain_Click(sender, null);
        }

        private void btnCamSaveSettingsToFile_Click(object sender, EventArgs e)
        {
            Camera.SaveCamParametersToFile("");
        }

        // PKv5.5.2
        private void btnLoadedSampleVolReset_Click(object sender, EventArgs e)
        {
            loadedSampleVolume = 0.0;
            UpdateLoadedSampleLabel();
        }

        // PKv5.5.2
        private void UpdateLoadedSampleLabel()
        {
            lblLoadedSampleVolume.Text = string.Format("Loaded Sample Volume:  {0:0.000} ul", loadedSampleVolume);
        }

        // PKv5.5.0.5  Save a bitmap,  Good utility for flyby grid images.

        private void btnSaveBitmap_Click(object sender, EventArgs e)
        {
            saveFileBMPDialog.Filter = "BMP No Graphics (*.bmp)|*.bmp";
            DialogResult result = saveFileBMPDialog.ShowDialog();
            if (result == DialogResult.Cancel) return;
            Camera.SaveImage(saveFileBMPDialog.FileName);    // Save the last graphics image.
        }

        private void btnTest123_Click(object sender, EventArgs e)
        {
            // PKv5.5.5.3
            MessageBox.Show("Make sure video still records \n Paused in front of the camera, Click OK to continue", "", MessageBoxButtons.OK);
        }

        private void btnTestDoubleTriggeredAcquision_Click(object sender, EventArgs e)
        {

            // PKv5.5.5.4.    
            // If the tip grid is traveling at 1 meter per second and there is 25mm between cameras.   Then we have 25msec between completing acquisitions.
            int ts1=0, ts2=0;

            int timeoutToWaitForTrips_ms = 10000;

            int startFrameCntGrid =0;
            int finalFrameCntGrid = 0;

            int startFrameCntTip =0;
            int finalFrameCntTip = 0;

            #region Setup and acquire with the grid camera
            Camera.setSwitchCamera("Grid");   // Start with the tip camera
            lblActiveCam.Text = "Grid";
            Camera.EnableStrobe((int)numericUpDownDebugStrobeDuration.Value, (int)numericUpDownDebugStrobeDelay.Value);
//            Camera.StopLiveAcquistion();
            // Acquire with the grid camera
            startFrameCntGrid = Camera.GetFrameCount();
            Camera.StartTriggeredAcquisition((int)numericUpDownOTFIDelayAfterTrip.Value, Pixy.MachineParameters.InsDropCamFlashDuration, Pixy.MachineParameters.InsDropCamFlashDelay);

            int timeout = System.Environment.TickCount + timeoutToWaitForTrips_ms;
            do
            {
                finalFrameCntGrid = Camera.GetFrameCount();
                if (finalFrameCntGrid == startFrameCntGrid + 1)
                {
                    Console.WriteLine("Grid Camera Single Frame Acquired");
                    ts1 = System.Environment.TickCount;
                    break;
                }

            } while (System.Environment.TickCount < timeout);
            if (finalFrameCntGrid != startFrameCntGrid + 1)
            {
                Console.WriteLine("TIMEOUT - Grid Camera Single Frame");
            }
            #endregion


            #region Acquire with the tip camera
            Camera.setSwitchCamera("Tip");   // Start with the tip camera
            lblActiveCam.Text = "Tip";

            Camera.EnableStrobe((int)numericUpDownDebugStrobeDuration.Value, (int)numericUpDownDebugStrobeDelay.Value);
            //            Camera.StopLiveAcquistion();

            startFrameCntTip = Camera.GetFrameCount();
            Camera.StartTriggeredAcquisition((int)numericUpDownOTFIDelayAfterTrip.Value, Pixy.MachineParameters.InsDropCamFlashDuration, Pixy.MachineParameters.InsDropCamFlashDelay);

            timeout = System.Environment.TickCount + timeoutToWaitForTrips_ms;
            do
            {
                finalFrameCntTip = Camera.GetFrameCount();
                if (finalFrameCntTip == startFrameCntTip+1)
                {
                    Console.WriteLine("Tip Camera Single Frame Acquired");
                    ts2 = System.Environment.TickCount;
                    break;
                }

            } while (System.Environment.TickCount < timeout);
            if (finalFrameCntTip != startFrameCntTip + 1)
            {
                Console.WriteLine("TIMEOUT - Tip Camera Single Frame");
            }

            Console.WriteLine("Starting Frame Count = {0},  Final Frame Count {1}", startFrameCntTip, finalFrameCntTip);
            #endregion

            int et_ms = ts2 - ts1;
            Console.WriteLine("Time between acquisitions (ms) = {0}", et_ms);

        }

        private void btnTestStartLive_Click(object sender, EventArgs e)
        {
            // Tip Camera then the grid camera
            Camera.setSwitchCamera("Tip");   // Start with the tip camera
            lblActiveCam.Text = "Tip";
            Camera.StartLiveAcquistion();

            Camera.setSwitchCamera("Grid");   // Grid Camera
            lblActiveCam.Text = "Grid";
            Camera.StartLiveAcquistion();

        }

        private void btnTestStopLive_Click(object sender, EventArgs e)
        {
            // Tip Camera then the grid camera
            Camera.setSwitchCamera("Tip");   // Start with the tip camera
            lblActiveCam.Text = "Tip";
            Camera.StopLiveAcquistion();

            Camera.setSwitchCamera("Grid");   // Grid Camera
            lblActiveCam.Text = "Grid";
            Camera.StopLiveAcquistion();

        }
        // DGC = Dual Grid Camera
        // PKv5.6  A little button for some offline testing.

        private void btnDGCTest_Click(object sender, EventArgs e)
        {
            string remoteImageFullPath = Camera.GetNextRemoteImageFile(Pixy.MachineParameters.RemoteVideoFileDirectory,"bmp",true);

            SaveExperimentalImagesAndSettingsPrompt(remoteImageFullPath);
         }

        private void SaveExperimentalImagesAndSettingsNoPrompt(string remoteImageFullPath)
        {
            string lastRecordingFileName = Camera.GetLastVideoFileName();
            if (lastRecordingFileName == "")
            {
                MessageBox.Show("MainForm: SaveExperimentalImagesAndSettings\n\nCannot Save Results\nNo video recorded yet");
                return;
            }

 
            string baseFileName = lastRecordingFileName.Substring(0, (lastRecordingFileName.Length - 4));

            try
            {
                Console.WriteLine("\nSaveExperimentalImagesAndSettingsNoPrompt (avi) or (bmp) file:");
                Console.WriteLine(" Main: {0}", lastRecordingFileName);
                if (remoteImageFullPath != "")
                {
                    string lowerCameraDestinationPath = baseFileName + "-LOWER.bmp";
                    Console.WriteLine("\nCopy Lower Grid Camera (bmp) file:");
                    Console.WriteLine(" From: {0}", remoteImageFullPath);
                    Console.WriteLine("   To: {0}", lowerCameraDestinationPath);
                    File.Copy(remoteImageFullPath, lowerCameraDestinationPath);
                }
           }
           catch (Exception e)
           {
               MessageBox.Show("MainForm: SaveExperimentalImagesAndSettings\n\nCould not copy images or videos for some reason");
               Console.WriteLine("Exception = {0}", e.ToString());
           }

            // Archive the settings
            string settingsFileName = baseFileName + ".txt";
            ArchiveSettingsUtility(settingsFileName);
        }

        // PKv5.6
        // 

        private void SaveExperimentalImagesAndSettingsPrompt(string remoteImageFullPath)
        {
            string lastRecordingFileName = Camera.GetLastVideoFileName();
            if (lastRecordingFileName == "")
            {
                MessageBox.Show("MainForm: SaveExperimentalImagesAndSettings\n\nCannot Save Results\nNo video recorded yet");
                return;
            }
            saveImagesAndSettingsFileDialog.FileName = "";      // Clear out the filename first
            if (saveImagesAndSettingsFileDialog.ShowDialog() == DialogResult.OK)
            {
                string baseFileName = saveImagesAndSettingsFileDialog.FileName;   // No extension 
                try
                {
                    string upperCameraDestinationPath = baseFileName + Path.GetExtension(lastRecordingFileName);
                    Console.WriteLine("\nCopy Main Grid Camera (avi) or (bmp) file:");
                    Console.WriteLine(" From: {0}", lastRecordingFileName);
                    Console.WriteLine("   To: {0}", upperCameraDestinationPath);
                    File.Copy(lastRecordingFileName, upperCameraDestinationPath);

                    if (remoteImageFullPath != "")
                    {
                        string lowerCameraDestinationPath = baseFileName + "(lower)" + Path.GetExtension(remoteImageFullPath);
                        Console.WriteLine("\nCopy Lower Grid Camera (bmp) file:");
                        Console.WriteLine(" From: {0}", remoteImageFullPath);
                        Console.WriteLine("   To: {0}", lowerCameraDestinationPath);
                        File.Copy(remoteImageFullPath, lowerCameraDestinationPath);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("MainForm: SaveExperimentalImagesAndSettings\n\nCould not copy images or videos for some reason");
                    Console.WriteLine("Exception = {0}", e.ToString());
                }

                // Archive the settings
                string settingsFileName = baseFileName + ".txt";
                ArchiveSettingsUtility(settingsFileName);
            }
        }

        // PKv5.6

        private void btn3StackLoadDefaults_Click(object sender, EventArgs e)
        {
            numericUpDown3StackAccel.Value = (decimal) Pixy.MachineParameters.OTFAccelPlunge_mm_ss;
            numericUpDown3StackDecel.Value = (decimal) Pixy.MachineParameters.OTFDecelPlunge_mm_ss;
            numericUpDown3StackVel.Value = (decimal)Pixy.MachineParameters.OTFSpeedPlunge_mm_s;
        }

        // PKv5.6

        private void btn3StackTestPlunge_Click(object sender, EventArgs e)
        {

            // Prompt the user... Give him a chance to cancel out
            DialogResult result = MessageBox.Show("Are you sure you want to run a test plunge ? \n\nMake sure Pipette Robot OUT OF THE WAY !!!", "Test Plunge Utility", MessageBoxButtons.OKCancel);
            if (result == DialogResult.Cancel) return;

            #region - Logic Safety Checks (specific to OTF dispense),  don't break anything

            if (!Pixy.MachineParameters.UseBentTweezerPoints)
            {
                Console.WriteLine("***Parameter Check Error- DoOTFPlunge,  Must use bent tweezers when doing OTF dispense... Returning");
                return;
            }

            double tempDist = Math.Abs(Pixy.MachineParameters.GridToCameraPointBent.X - Pixy.MachineParameters.InLiquidEthaneBowlBent.X) +
                                    Math.Abs(Pixy.MachineParameters.GridToCameraPointBent.Y - Pixy.MachineParameters.InLiquidEthaneBowlBent.Y);
            if (tempDist > .010)
            {
                Console.WriteLine("***Parameter Check Error- DoOTFPlunge,  X & Y Values of /n    GridToCameraPoint and InLiquidEthaneBowlBent  DO NOT MATCH .... returning");
                return;
            }

            #endregion

            bool UseGridRobot = true;

            #region Calculate startPosition (A bit above GridToCameraPointBent), endPosition (in the ethane bowl), tempPosition (set to GridToCameraPointBent) for moves.

            MachineCoordinate startPosition;
            MachineCoordinate endPosition;
            MachineCoordinate tempPosition = new MachineCoordinate();
            Pixy.MotionControl.GetCurrentPosition(out tempPosition, true);   // Grid robot current position should be at  GridToCameraPointBent

            // One more logic check   Are we at the grid to camera bent point ???  At least in X and Y
            tempDist = Math.Abs(Pixy.MachineParameters.GridToCameraPointBent.X - tempPosition.X) +
                         Math.Abs(Pixy.MachineParameters.GridToCameraPointBent.Y - tempPosition.Y);
            if (tempDist > .010)
            {
                Console.WriteLine("***Parameter Check Error- DoOTFPlunge,  Robot not at GridToCameraPointBent .... returning");
                return;
            }

            SafeCopyPosition(out endPosition, Pixy.MachineParameters.InLiquidEthaneBowlBent);

            // Start Point
            SafeCopyPosition(out startPosition, tempPosition);   // Will create a copy of the point.
            startPosition.Z = startPosition.Z + Pixy.MachineParameters.OTFStartAbove_mm;
 
            #endregion

            // Move to grid startPosition
            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                   startPosition, 0.0, 25, true, true, UseGridRobot, true);

            #region Setup to do fly by with main grid camera too. 
            Camera.setSwitchCamera("Grid");   // Double check that we are on the grid camera
            lblActiveCam.Text = "Grid";

            // PKv5.5.0.6,  2017-02-23 Make sure the strobe light is enabled.
            checkBoxDebugEnableStrobe.Checked = false;
            Thread.Sleep(10);
            checkBoxDebugEnableStrobe.Checked = true;
            Thread.Sleep(10);
            Camera.EnableStrobe((int)numericUpDownDebugStrobeDuration.Value, (int)numericUpDownDebugStrobeDelay.Value);
            // Stop Live Acquisition
            Camera.StopLiveAcquistion();

            int stFrameCountTriggeredAcquistion = Camera.GetFrameCount();
            Camera.StartTriggeredAcquisition((int)numericUpDownOTFIDelayAfterTrip.Value, Pixy.MachineParameters.InsDropCamFlashDuration, Pixy.MachineParameters.InsDropCamFlashDelay);

            #endregion

            #region Open the shutter.
            // open shutter
            IO.SetOutput(2);
            IO.WaitInput(-1, 1000);  // Wait for the shutter to open
            #endregion

            Pixy.MotionControl.SetAccel(Pixy, 2, (double) numericUpDown3StackAccel.Value, (double) numericUpDown3StackDecel.Value, UseGridRobot);
            Pixy.MotionControl.SetMaxVelocity(Pixy, 2, (double) numericUpDown3StackVel.Value, UseGridRobot);

            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip], endPosition, 0.0, 100, true, true, UseGridRobot, true);

            // Prompt and move to grid startPosition at a very slow speed.

            int endFrameCountTriggeredAcquistion = Camera.GetFrameCount();
            Console.WriteLine("Start Frame Count = {0},   End Frame Count = {1}", stFrameCountTriggeredAcquistion, endFrameCountTriggeredAcquistion);

            result = MessageBox.Show("Moving the axis back to the starting position.", "Answer me this", MessageBoxButtons.OKCancel);
            if (result == DialogResult.Cancel) return;

            tempPosition.Z = Pixy.MachineParameters.GridToCameraPointBent.Z;        // Set the Z properly in case we plunged from the wrong height.  // PKv5.6a

            Pixy.MotionControl.MoveSafely(Pixy, Pixy.MachineParameters.Tips[Pixy.ActiveTip],
                   tempPosition, 0.0, 10, true, true, UseGridRobot, true);

            // Return the Speed, Accel and Decel to normal.
            Pixy.MotionControl.SetAccel(Pixy, 2, Pixy.MachineParameters.OTFAccelPlunge_mm_ss, Pixy.MachineParameters.OTFDecelPlunge_mm_ss, UseGridRobot);
            Pixy.MotionControl.SetMaxVelocity(Pixy, 2, Pixy.MachineParameters.OTFSpeedPlunge_mm_s, UseGridRobot);

            Camera.StartLiveAcquistion();   // Lets set it back

        }

        // 2019-01-16  
        private void btnBypassSyringeValves_Click(object sender, EventArgs e)
        {
            DoSyringeSetValvePositionGUI(Pixy.MachineParameters.SyringeMask,
            "2",                        // 2 is bypass.
            100);
        }

        private void btnCloseSyringeValves_Click(object sender, EventArgs e)
        {
            DoSyringeSetValvePositionGUI(Pixy.MachineParameters.SyringeMask,
            "1",                        // 1 is Output.
            100);
        }
    }
	
}
