using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using System.Data;
using EA.PixyControl.ClassLibrary;

namespace EA.PixyControl
{
	/// <summary>
	/// Summary description for DoActionForm.
	/// </summary>
	public class DoActionForm : System.Windows.Forms.Form
	{
		private ControlLibrary.ProcessActionViewer Viewer;
		private System.Windows.Forms.ComboBox cboAction;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem mnuCheck;
		private System.Windows.Forms.MenuItem mnuGo;
		private System.Windows.Forms.MenuItem mnuExit;
		private System.Windows.Forms.DataGrid dgVariables;

		private ProcessActionOpList		mActionOps = null;
		private VariableManager			mVM = null;
		private static DoActionForm		mSingleInstance = null;
		
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DoActionForm()
		{
			if (mSingleInstance != null)
			{
				try{mSingleInstance.Dispose();}
				catch{}
			}
			
			mSingleInstance = this;
			
			// Required for Windows Form Designer support
			InitializeComponent();

			this.cboAction.Items.Add(new Process_Wash());
			this.cboAction.Items.Add(new Process_Prime());
			this.cboAction.Items.Add(new Process_Aspirate());
			this.cboAction.Items.Add(new Process_Dispense());
			this.cboAction.Items.Add(new Process_SetSyringeValvePosition());
			this.cboAction.Items.Add(new Process_SyringeMove());
			this.cboAction.Items.Add(new Process_SyringeEmpty());
			this.cboAction.Items.Add(new Process_EnableController());
			this.cboAction.Items.Add(new Process_InspectTipFiring());
			this.cboAction.Items.Add(new Process_PiezoDispense());
			this.cboAction.Items.Add(new Process_InitializeMotion());
			this.cboAction.Items.Add(new Process_HomeAxis());
			this.cboAction.Items.Add(new Process_MoveRelative());
			this.cboAction.Items.Add(new Process_MoveToSafeHeight());
			this.cboAction.Items.Add(new Process_MoveAbovePoint());
			this.cboAction.Items.Add(new Process_MoveToPoint());
			this.cboAction.Items.Add(new Process_MoveAbovePoint());

			foreach (object o in cboAction.Items)
			{
				ProcessAction Cmd = o as ProcessAction;
				SequenceFile.AssignDefaultParameters(Cmd);
			}

			this.cboAction.SelectedIndex = 0;
		
			this.Viewer.ParameterChanged += new ControlLibrary.ParameterChangedEventHandler(Viewer_ParameterChanged);
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

		public void AllowProcessActions(ProcessActionOpList ActionOpList, VariableManager VM)
		{
			
			if (ActionOpList == null) 
			{
				MessageBox.Show("Must supply a process action operation list");
				return;
			}

			if (VM == null)
			{
				MessageBox.Show("Must supply a variable manager");
				return;
			}

			mActionOps = ActionOpList;
			mVM = VM;

			DataTable DT = mVM.ListVariables();
			this.dgVariables.DataSource = DT;

			AutoSizeColumnVariableViewer();

			this.Show();
		}


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(DoActionForm));
			this.Viewer = new ControlLibrary.ProcessActionViewer();
			this.cboAction = new System.Windows.Forms.ComboBox();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.mnuCheck = new System.Windows.Forms.MenuItem();
			this.mnuGo = new System.Windows.Forms.MenuItem();
			this.mnuExit = new System.Windows.Forms.MenuItem();
			this.dgVariables = new System.Windows.Forms.DataGrid();
			((System.ComponentModel.ISupportInitialize)(this.dgVariables)).BeginInit();
			this.SuspendLayout();
			// 
			// Viewer
			// 
			this.Viewer.AllowEdit = false;
			this.Viewer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.Viewer.Location = new System.Drawing.Point(16, 40);
			this.Viewer.Name = "Viewer";
			this.Viewer.Size = new System.Drawing.Size(352, 264);
			this.Viewer.TabIndex = 0;
			// 
			// cboAction
			// 
			this.cboAction.DisplayMember = "Name";
			this.cboAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboAction.Location = new System.Drawing.Point(16, 8);
			this.cboAction.Name = "cboAction";
			this.cboAction.Size = new System.Drawing.Size(280, 21);
			this.cboAction.TabIndex = 1;
			this.cboAction.ValueMember = "Name";
			this.cboAction.SelectedIndexChanged += new System.EventHandler(this.cboAction_SelectedIndexChanged);
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.mnuCheck,
																					  this.mnuGo,
																					  this.mnuExit});
			// 
			// mnuCheck
			// 
			this.mnuCheck.Index = 0;
			this.mnuCheck.Text = "&Check";
			this.mnuCheck.Click += new System.EventHandler(this.mnuCheck_Click);
			// 
			// mnuGo
			// 
			this.mnuGo.Index = 1;
			this.mnuGo.Text = "&Go";
			this.mnuGo.Click += new System.EventHandler(this.mnuGo_Click);
			// 
			// mnuExit
			// 
			this.mnuExit.Index = 2;
			this.mnuExit.Text = "E&xit";
			this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
			// 
			// dgVariables
			// 
			this.dgVariables.AlternatingBackColor = System.Drawing.Color.LightGray;
			this.dgVariables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dgVariables.BackColor = System.Drawing.Color.Gainsboro;
			this.dgVariables.BackgroundColor = System.Drawing.Color.Silver;
			this.dgVariables.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dgVariables.CaptionBackColor = System.Drawing.Color.LightSteelBlue;
			this.dgVariables.CaptionForeColor = System.Drawing.Color.MidnightBlue;
			this.dgVariables.CaptionText = "Variables";
			this.dgVariables.DataMember = "";
			this.dgVariables.FlatMode = true;
			this.dgVariables.Font = new System.Drawing.Font("Tahoma", 8F);
			this.dgVariables.ForeColor = System.Drawing.Color.Black;
			this.dgVariables.GridLineColor = System.Drawing.Color.DimGray;
			this.dgVariables.GridLineStyle = System.Windows.Forms.DataGridLineStyle.None;
			this.dgVariables.HeaderBackColor = System.Drawing.Color.MidnightBlue;
			this.dgVariables.HeaderFont = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.dgVariables.HeaderForeColor = System.Drawing.Color.White;
			this.dgVariables.LinkColor = System.Drawing.Color.MidnightBlue;
			this.dgVariables.Location = new System.Drawing.Point(16, 304);
			this.dgVariables.Name = "dgVariables";
			this.dgVariables.ParentRowsBackColor = System.Drawing.Color.DarkGray;
			this.dgVariables.ParentRowsForeColor = System.Drawing.Color.Black;
			this.dgVariables.SelectionBackColor = System.Drawing.Color.CadetBlue;
			this.dgVariables.SelectionForeColor = System.Drawing.Color.White;
			this.dgVariables.Size = new System.Drawing.Size(352, 128);
			this.dgVariables.TabIndex = 2;
			// 
			// DoActionForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(384, 449);
			this.Controls.Add(this.dgVariables);
			this.Controls.Add(this.cboAction);
			this.Controls.Add(this.Viewer);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mainMenu1;
			this.Name = "DoActionForm";
			this.Text = "Action";
			((System.ComponentModel.ISupportInitialize)(this.dgVariables)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void cboAction_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			ProcessAction PA = this.cboAction.SelectedItem as ProcessAction;
			UpdateViewer(PA);
		}

		private void UpdateViewer(ProcessAction PA)
		{
			this.Viewer.ShowProcessAction(PA, true);
		}

		private void Viewer_ParameterChanged(object sender, ControlLibrary.ParameterChangedEventArgs ev)
		{
			ProcessAction Cmd = this.cboAction.SelectedItem as ProcessAction;
			
			if (Cmd != null)
			{
				string			PropName;
				DataTable		DT = ev.NewInfo;
				Type			T = Cmd.GetType();
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
			}

			UpdateViewer(Cmd);
		}


		private void AutoSizeColumnVariableViewer()
		{
			if (this.dgVariables.DataSource == null) return;

			DataGridTableStyle ts = new DataGridTableStyle();
			DataTable dt = (DataTable)this.dgVariables.DataSource;
			ts.MappingName =dt.TableName;

			this.dgVariables.TableStyles.Clear();
			this.dgVariables.TableStyles.Add(ts);

			foreach (DataColumn c in dt.Columns)
			{
				this.dgVariables.TableStyles[dt.TableName].GridColumnStyles[c.Caption].Width = LongestField(dt, c.Caption);			
			}

			this.dgVariables.TableStyles[dt.TableName].AlternatingBackColor = System.Drawing.Color.LightGray;

		}
	
		
		private int LongestField (DataTable DT, string ColumnName)
		{
			int		maxlength = 0;
			int		tot = DT.Rows.Count;
			string	straux = "";
			int		intaux = 0;

			Graphics g = dgVariables.CreateGraphics();

			// Take width one blank space to add to the new width to the Column   
			int offset = Convert.ToInt32(Math.Ceiling(g.MeasureString(" ", dgVariables.Font).Width));

			for (int i=0; i<tot; ++i)
			{
				straux =  DT.Rows[i][ColumnName].ToString();

				// Get the width of Current Field String according to the Font
				intaux = Convert.ToInt32(Math.Ceiling(g.MeasureString(straux, dgVariables.Font).Width));
				if (intaux > maxlength)				
				{
					maxlength = intaux;
				}
			}

			// check the header too
			intaux = Convert.ToInt32(Math.Ceiling(g.MeasureString(ColumnName, dgVariables.Font).Width));
			if (intaux > maxlength)	maxlength = intaux;
			
			return maxlength + offset;
		}

		private void mnuCheck_Click(object sender, System.EventArgs e)
		{
			ProcessAction Cmd = this.cboAction.SelectedItem as ProcessAction;
			
			if (CommandOK(Cmd)) MessageBox.Show("Parameters OK");
		}

		private bool CommandOK(ProcessAction Cmd)
		{
			string	ErrorMsg = "";
		
			if (Cmd == null) 
			{
				MessageBox.Show("No command selected", "Command Error");
				return false;
			}

			if (Cmd.ParametersOK(mVM, out ErrorMsg) == false)
			{
				MessageBox.Show(ErrorMsg, "Command Error");
				return false;
			}

			return true;
		}

		private void mnuGo_Click(object sender, System.EventArgs e)
		{
			ProcessAction Cmd = this.cboAction.SelectedItem as ProcessAction;
			
			try
			{
				ExecuteCommand(Cmd);
			}
			catch(Exception Ex)
			{
				MessageBox.Show("Error executing command:\n" + Ex.Message);
			}
		}

		private void ExecuteCommand(ProcessAction Cmd)
		{
			if (!CommandOK(Cmd)) return;

			// find the method assigned to that command type
			ProcessActionOp		AssignedMethod = this.mActionOps.GetProcessActionOp(Cmd.NameInCommandFile);

			// there should be one for everything we put in the combo box, but just in case...
			if (AssignedMethod == null) 
			{
				MessageBox.Show("No method associated with this command type", "Execution Error");
				return;
			}

			// do the method
			if (AssignedMethod(Cmd) != 0) MessageBox.Show("Command failed");
		}

		private void mnuExit_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
