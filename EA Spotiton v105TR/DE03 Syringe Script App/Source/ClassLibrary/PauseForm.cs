using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

namespace EA.PixyControl.ClassLibrary
{
	/// <summary>
	/// Summary description for PauseForm.
	/// </summary>
	public class frmPause : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label lblMsg;
		private System.Windows.Forms.Label lblTimeLeft;
		private System.Windows.Forms.Timer timer1;
		private System.ComponentModel.IContainer components;
		private DateTime	mStartTime;
		private int			mPauseTime_ms;

		public frmPause()
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
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		public void DoPause(string Msg, int PauseTime_ms)
		{
			if (PauseTime_ms <= 0) return;

            if (Msg.ToLower() == "none")
            {
                Console.WriteLine("Pausing without dialog box {0} ms", PauseTime_ms);
                Thread.Sleep(PauseTime_ms);
                return;
            }

			this.lblMsg.Text = Msg + "\n";

			mStartTime = System.DateTime.Now;
			mPauseTime_ms = PauseTime_ms;
			
			this.ShowDialog();
		}

		private double ShowTimeLeft()
		{
			System.TimeSpan Diff = System.DateTime.Now - mStartTime;
			double			TimeLeft_ms = mPauseTime_ms - Diff.TotalMilliseconds;

			if (TimeLeft_ms > 0)
				this.lblTimeLeft.Text = string.Format("{0:F1} seconds left", (TimeLeft_ms / 1000));
			else
				this.lblTimeLeft.Text = "0.0 seconds left";

			return TimeLeft_ms;
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.lblMsg = new System.Windows.Forms.Label();
			this.lblTimeLeft = new System.Windows.Forms.Label();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// lblMsg
			// 
			this.lblMsg.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblMsg.Location = new System.Drawing.Point(16, 40);
			this.lblMsg.Name = "lblMsg";
			this.lblMsg.Size = new System.Drawing.Size(368, 64);
			this.lblMsg.TabIndex = 0;
			// 
			// lblTimeLeft
			// 
			this.lblTimeLeft.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblTimeLeft.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblTimeLeft.Location = new System.Drawing.Point(16, 8);
			this.lblTimeLeft.Name = "lblTimeLeft";
			this.lblTimeLeft.Size = new System.Drawing.Size(368, 24);
			this.lblTimeLeft.TabIndex = 1;
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// frmPause
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(392, 112);
			this.Controls.Add(this.lblTimeLeft);
			this.Controls.Add(this.lblMsg);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "frmPause";
			this.Text = " Pause";
			this.ResumeLayout(false);

		}
		#endregion

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			if (ShowTimeLeft() <= 0.0) this.Close();
		}
	}
}
