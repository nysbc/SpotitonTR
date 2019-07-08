using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aurigin;

namespace EA.PixyControl
{
    public partial class IOControlForm : Form
    {
        public IOControlForm()
        {
            InitializeComponent();
        }

        private bool formLoading;

        private void IOControlForm_Load(object sender, EventArgs e)
        {
            formLoading = true;
            UpdateOutputCheckBoxes();
            this.timer1.Enabled = false;   // Cannot contiue to update these boxes.
            formLoading = false;
        }

        private void UpdateOutputCheckBoxes()
        {
           this.cbOut1.Checked = IO.ReadOutput(-1);    // Inverted is the normal
           this.cbOut2.Checked = IO.ReadOutput(-2);
           this.cbOut3.Checked = IO.ReadOutput(-3);
           this.cbOut4.Checked = IO.ReadOutput(-4);
           this.cbOut5.Checked = IO.ReadOutput(-5);
           this.cbOut6.Checked = IO.ReadOutput(-6);
           this.cbOut7.Checked = IO.ReadOutput(-7);
           this.cbOut8.Checked = IO.ReadOutput(-8);
 
        }

        private void cbOut1_CheckedChanged(object sender, EventArgs e)
        {
            if (!formLoading)
            {
                if (!this.cbOut1.Checked)
                    IO.SetOutput(1);
                else
                    IO.SetOutput(-1);
            }
        }

        private void cbOut2_CheckedChanged(object sender, EventArgs e)
        {
            if (!formLoading)
            {
                if (!this.cbOut2.Checked)
                    IO.SetOutput(2);
                else
                    IO.SetOutput(-2);
            }
        }

        private void cbOut3_CheckedChanged(object sender, EventArgs e)
        {
            if (!formLoading)
            {
                if (!this.cbOut3.Checked)
                    IO.SetOutput(3);
                else
                    IO.SetOutput(-3);
            }
        }

        private void cbOut4_CheckedChanged(object sender, EventArgs e)
        {
            if (!formLoading)
            {
                if (!this.cbOut4.Checked)
                    IO.SetOutput(4);
                else
                    IO.SetOutput(-4);
            }
        }

        private void cbOut5_CheckedChanged(object sender, EventArgs e)
        {
            if (!formLoading)
            {
                if (!this.cbOut5.Checked)
                    IO.SetOutput(5);
                else
                    IO.SetOutput(-5);
            }
        }

        private void cbOut6_CheckedChanged(object sender, EventArgs e)
        {
            if (!formLoading)
            {
                if (!this.cbOut6.Checked)
                    IO.SetOutput(6);
                else
                    IO.SetOutput(-6);
            }
        }

        private void cbOut7_CheckedChanged(object sender, EventArgs e)
        {
            if (!formLoading)
            {
                if (!this.cbOut7.Checked)
                    IO.SetOutput(7);
                else
                    IO.SetOutput(-7);
            }
        }

        private void cbOut8_CheckedChanged(object sender, EventArgs e)
        {
            if (!formLoading)
            {
                if (!this.cbOut8.Checked)
                    IO.SetOutput(8);
                else
                    IO.SetOutput(-8);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateOutputCheckBoxes();
        }

        private void IOControlForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.timer1.Enabled = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

