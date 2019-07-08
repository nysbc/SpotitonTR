using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace EA.PixyControl.ClassLibrary
{
	/// <summary>
	/// Summary description for UserInputForm.
	/// </summary>
	public class frmUserInput : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;

		private bool mOkPressed;
		private System.Windows.Forms.Label lblMessage;
		private System.Windows.Forms.TextBox txtEntry;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmUserInput()
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

        // Revision R2.02,  R2.03
        // More lines possible with the integer user input.

        // Use defaultValue = -1 to leave default value out.

		public int GetInteger(string Msg, int Min, int Max, int ValueOnCancel, int defaultValue)
		{
			this.txtEntry.Visible = true;
			this.txtEntry.Text = "";
            //int numOfLine = 1+(int)Math.Truncate((Msg.Length-1) /40.0);
            //int tempLength = 0;
            //this.lblMessage.Text = "";
            //for (int i = 0; i < numOfLine; i++)
            //{
            //    if (Msg.Length>((i*40+40)))
            //        tempLength=40;
            //    else
            //        tempLength=Msg.Length-(i*40);

            //    this.lblMessage.Text = this.lblMessage.Text + Msg.Substring(i * 40, tempLength) + "\n";
            //}

            this.lblMessage.Text= Msg.Replace(@"\N", "\n");


            this.lblMessage.Text = this.lblMessage.Text + "\n\n"+"(Integer from " + Min + " to " + Max + ")";
			this.btnOK.Text = "OK";
			this.btnCancel.Text = "Cancel";

            // R2.03
            if (defaultValue != -1)
                this.txtEntry.Text = defaultValue.ToString();

			while(true)
			{
				this.ShowDialog();

				if (this.mOkPressed)
				{
					try
					{
						int i = System.Convert.ToInt32(this.txtEntry.Text);
						if ((i >= Min) && (i <= Max)) return i; 
					}
					catch{}
				}
				else
				{
					return ValueOnCancel;
				}

				System.Windows.Forms.MessageBox.Show("Invalid entry");
			}
		}

		public double GetDouble(string Msg, double Min, double Max, double ValueOnCancel)
		{
			this.txtEntry.Visible = true;
			this.txtEntry.Text = "";
			this.lblMessage.Text = Msg + "\n" + "(Double from " + Min + " to " + Max + ")";
			this.btnOK.Text = "OK";
			this.btnCancel.Text = "Cancel";

			while(true)
			{
				this.ShowDialog();

				if (this.mOkPressed)
				{
					try
					{
						double i = System.Convert.ToDouble(this.txtEntry.Text);
						if ((i >= Min) && (i <= Max)) return i; 
					}
					catch{}
				}
				else
				{
					return ValueOnCancel;
				}

				System.Windows.Forms.MessageBox.Show("Invalid entry");
			}
		}

		public bool GetBoolean(string Msg, bool ValueOnCancel)
		{
			this.txtEntry.Visible = false;
			this.lblMessage.Text = Msg;
			this.btnOK.Text = "Yes";
			this.btnCancel.Text = "No";

			this.ShowDialog();

			return this.mOkPressed;
		}


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.lblMessage = new System.Windows.Forms.Label();
            this.txtEntry = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblMessage
            // 
            this.lblMessage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.Location = new System.Drawing.Point(16, 8);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(374, 120);
            this.lblMessage.TabIndex = 0;
            this.lblMessage.Text = "Message";
            // 
            // txtEntry
            // 
            this.txtEntry.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEntry.Location = new System.Drawing.Point(16, 140);
            this.txtEntry.Name = "txtEntry";
            this.txtEntry.Size = new System.Drawing.Size(374, 22);
            this.txtEntry.TabIndex = 1;
            this.txtEntry.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtEntry_KeyPress);
            // 
            // btnOK
            // 
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(49, 181);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(96, 32);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(294, 181);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(96, 32);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // frmUserInput
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(417, 225);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtEntry);
            this.Controls.Add(this.lblMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmUserInput";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = " User Input";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			this.mOkPressed = true;
			this.Close();
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.mOkPressed = false;
			this.Close();
		}

		private void txtEntry_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (e.KeyChar != 13) return;

			this.mOkPressed = true;
			this.Close();
		}

	}
}
