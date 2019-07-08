using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace Aurigin
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class FormSyringe : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.StatusBar statusBar1;
		private AuButton btnInit;
		private AuButton btnEmpty;
		private AuButton btnDispense;
		private Aurigin.AuButton btnAspirate;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FormSyringe()
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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnDispense = new Aurigin.AuButton();
			this.btnAspirate = new Aurigin.AuButton();
			this.btnEmpty = new Aurigin.AuButton();
			this.btnInit = new Aurigin.AuButton();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.tabControl1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(450, 303);
			this.tabControl1.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.BackColor = System.Drawing.Color.CornflowerBlue;
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(442, 277);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = " Input Probe ";
			// 
			// tabPage2
			// 
			this.tabPage2.BackColor = System.Drawing.Color.CornflowerBlue;
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(440, 275);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "  Micro Pipette";
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.LightSteelBlue;
			this.panel1.Controls.Add(this.btnDispense);
			this.panel1.Controls.Add(this.btnAspirate);
			this.panel1.Controls.Add(this.btnEmpty);
			this.panel1.Controls.Add(this.btnInit);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel1.Location = new System.Drawing.Point(450, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(144, 303);
			this.panel1.TabIndex = 1;
			// 
			// btnDispense
			// 
			this.btnDispense.AccessLevel = 0;
			this.btnDispense.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.btnDispense.Location = new System.Drawing.Point(8, 160);
			this.btnDispense.Name = "btnDispense";
			this.btnDispense.Scheme = Aurigin.AuButton.Schemes.Default;
			this.btnDispense.Shape = Aurigin.AuButton.Shapes.Rectangle;
			this.btnDispense.Size = new System.Drawing.Size(128, 32);
			this.btnDispense.TabIndex = 3;
			this.btnDispense.Text = "Dispense";
			this.btnDispense.ToolTip = "";
			// 
			// btnAspirate
			// 
			this.btnAspirate.AccessLevel = 0;
			this.btnAspirate.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.btnAspirate.Location = new System.Drawing.Point(8, 112);
			this.btnAspirate.Name = "btnAspirate";
			this.btnAspirate.Scheme = Aurigin.AuButton.Schemes.Default;
			this.btnAspirate.Shape = Aurigin.AuButton.Shapes.Rectangle;
			this.btnAspirate.Size = new System.Drawing.Size(128, 32);
			this.btnAspirate.TabIndex = 2;
			this.btnAspirate.Text = "Aspirate";
			this.btnAspirate.ToolTip = "";
			// 
			// btnEmpty
			// 
			this.btnEmpty.AccessLevel = 0;
			this.btnEmpty.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.btnEmpty.Location = new System.Drawing.Point(8, 64);
			this.btnEmpty.Name = "btnEmpty";
			this.btnEmpty.Scheme = Aurigin.AuButton.Schemes.Default;
			this.btnEmpty.Shape = Aurigin.AuButton.Shapes.Rectangle;
			this.btnEmpty.Size = new System.Drawing.Size(128, 32);
			this.btnEmpty.TabIndex = 1;
			this.btnEmpty.Text = "Empty";
			this.btnEmpty.ToolTip = "";
			// 
			// btnInit
			// 
			this.btnInit.AccessLevel = 0;
			this.btnInit.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.btnInit.Location = new System.Drawing.Point(8, 16);
			this.btnInit.Name = "btnInit";
			this.btnInit.Scheme = Aurigin.AuButton.Schemes.Default;
			this.btnInit.Shape = Aurigin.AuButton.Shapes.Rectangle;
			this.btnInit.Size = new System.Drawing.Size(128, 32);
			this.btnInit.TabIndex = 0;
			this.btnInit.Text = "Initialize";
			this.btnInit.ToolTip = "";
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 303);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(594, 22);
			this.statusBar1.TabIndex = 0;
			// 
			// FormSyringe
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.LightSteelBlue;
			this.ClientSize = new System.Drawing.Size(594, 325);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.statusBar1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(600, 350);
			this.MinimumSize = new System.Drawing.Size(600, 350);
			this.Name = "FormSyringe";
			this.Text = "Syringe Diagnostics";
			this.TopMost = true;
			this.tabControl1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new FormSyringe());
		}
	}
}
