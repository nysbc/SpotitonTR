namespace EA.PixyControl
{
    partial class IOControlForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.cbOut3 = new System.Windows.Forms.CheckBox();
            this.cbOut2 = new System.Windows.Forms.CheckBox();
            this.cbOut1 = new System.Windows.Forms.CheckBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnClose = new System.Windows.Forms.Button();
            this.cbOut4 = new System.Windows.Forms.CheckBox();
            this.cbOut5 = new System.Windows.Forms.CheckBox();
            this.cbOut6 = new System.Windows.Forms.CheckBox();
            this.cbOut7 = new System.Windows.Forms.CheckBox();
            this.cbOut8 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cbOut3
            // 
            this.cbOut3.AutoSize = true;
            this.cbOut3.Location = new System.Drawing.Point(24, 72);
            this.cbOut3.Name = "cbOut3";
            this.cbOut3.Size = new System.Drawing.Size(49, 17);
            this.cbOut3.TabIndex = 0;
            this.cbOut3.Text = "Out3";
            this.cbOut3.UseVisualStyleBackColor = true;
            this.cbOut3.CheckedChanged += new System.EventHandler(this.cbOut3_CheckedChanged);
            // 
            // cbOut2
            // 
            this.cbOut2.AutoSize = true;
            this.cbOut2.Location = new System.Drawing.Point(24, 49);
            this.cbOut2.Name = "cbOut2";
            this.cbOut2.Size = new System.Drawing.Size(49, 17);
            this.cbOut2.TabIndex = 1;
            this.cbOut2.Text = "Out2";
            this.cbOut2.UseVisualStyleBackColor = true;
            this.cbOut2.CheckedChanged += new System.EventHandler(this.cbOut2_CheckedChanged);
            // 
            // cbOut1
            // 
            this.cbOut1.AutoSize = true;
            this.cbOut1.Location = new System.Drawing.Point(24, 26);
            this.cbOut1.Name = "cbOut1";
            this.cbOut1.Size = new System.Drawing.Size(49, 17);
            this.cbOut1.TabIndex = 2;
            this.cbOut1.Text = "Out1";
            this.cbOut1.UseVisualStyleBackColor = true;
            this.cbOut1.CheckedChanged += new System.EventHandler(this.cbOut1_CheckedChanged);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(172, 226);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // cbOut4
            // 
            this.cbOut4.AutoSize = true;
            this.cbOut4.Location = new System.Drawing.Point(24, 95);
            this.cbOut4.Name = "cbOut4";
            this.cbOut4.Size = new System.Drawing.Size(49, 17);
            this.cbOut4.TabIndex = 4;
            this.cbOut4.Text = "Out4";
            this.cbOut4.UseVisualStyleBackColor = true;
            this.cbOut4.CheckedChanged += new System.EventHandler(this.cbOut4_CheckedChanged);
            // 
            // cbOut5
            // 
            this.cbOut5.AutoSize = true;
            this.cbOut5.Location = new System.Drawing.Point(24, 118);
            this.cbOut5.Name = "cbOut5";
            this.cbOut5.Size = new System.Drawing.Size(49, 17);
            this.cbOut5.TabIndex = 5;
            this.cbOut5.Text = "Out5";
            this.cbOut5.UseVisualStyleBackColor = true;
            this.cbOut5.CheckedChanged += new System.EventHandler(this.cbOut5_CheckedChanged);
            // 
            // cbOut6
            // 
            this.cbOut6.AutoSize = true;
            this.cbOut6.Location = new System.Drawing.Point(24, 141);
            this.cbOut6.Name = "cbOut6";
            this.cbOut6.Size = new System.Drawing.Size(49, 17);
            this.cbOut6.TabIndex = 6;
            this.cbOut6.Text = "Out6";
            this.cbOut6.UseVisualStyleBackColor = true;
            this.cbOut6.CheckedChanged += new System.EventHandler(this.cbOut6_CheckedChanged);
            // 
            // cbOut7
            // 
            this.cbOut7.AutoSize = true;
            this.cbOut7.Location = new System.Drawing.Point(24, 164);
            this.cbOut7.Name = "cbOut7";
            this.cbOut7.Size = new System.Drawing.Size(49, 17);
            this.cbOut7.TabIndex = 7;
            this.cbOut7.Text = "Out7";
            this.cbOut7.UseVisualStyleBackColor = true;
            this.cbOut7.CheckedChanged += new System.EventHandler(this.cbOut7_CheckedChanged);
            // 
            // cbOut8
            // 
            this.cbOut8.AutoSize = true;
            this.cbOut8.Location = new System.Drawing.Point(24, 187);
            this.cbOut8.Name = "cbOut8";
            this.cbOut8.Size = new System.Drawing.Size(49, 17);
            this.cbOut8.TabIndex = 8;
            this.cbOut8.Text = "Out8";
            this.cbOut8.UseVisualStyleBackColor = true;
            this.cbOut8.CheckedChanged += new System.EventHandler(this.cbOut8_CheckedChanged);
            // 
            // IOControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 274);
            this.Controls.Add(this.cbOut8);
            this.Controls.Add(this.cbOut7);
            this.Controls.Add(this.cbOut6);
            this.Controls.Add(this.cbOut5);
            this.Controls.Add(this.cbOut4);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.cbOut1);
            this.Controls.Add(this.cbOut2);
            this.Controls.Add(this.cbOut3);
            this.Name = "IOControlForm";
            this.Text = "IOControlForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IOControlForm_FormClosing);
            this.Load += new System.EventHandler(this.IOControlForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbOut3;
        private System.Windows.Forms.CheckBox cbOut2;
        private System.Windows.Forms.CheckBox cbOut1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.CheckBox cbOut4;
        private System.Windows.Forms.CheckBox cbOut5;
        private System.Windows.Forms.CheckBox cbOut6;
        private System.Windows.Forms.CheckBox cbOut7;
        private System.Windows.Forms.CheckBox cbOut8;
    }
}