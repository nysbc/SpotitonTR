using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Reflection;
using EA.PixyControl.ClassLibrary;


namespace ControlLibrary
{
	public class ProcessActionViewer : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.DataGrid	dgProperties;
		private System.Data.DataTable			dtProperties;
		private ProcessAction					mProcessAction;
		private bool							mAllowEdit = false;
		private bool							mEdited = false;
		
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.Button btnReset;
		
		public bool AllowEdit
		{
			get {return mAllowEdit;}
			set {mAllowEdit = value;}
		}

		public bool Edited
		{
			get {return mEdited;}
		}

		public event ParameterChangedEventHandler ParameterChanged;
		
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ProcessActionViewer()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitComponent call

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}


		public void ShowProcessAction(ProcessAction Obj, bool AllowEditing)
		{			
			this.mProcessAction = Obj;
			this.mAllowEdit = AllowEditing;

			this.UpdateDisplay();

			mEdited = false;

			this.btnApply.Visible = false;
			this.btnReset.Visible = false;
		}
		
		private void UpdateDisplay()
		{
			Type			T;
			PropertyInfo []	Properties;
			object []		Attributes;
			DataRow			NewRow = null;

	//		System.Windows.Forms.DataGridCell

			this.dgProperties.DataSource = null;
			this.dgProperties.CaptionText = "Properties";

			if (mProcessAction == null) return;
			
			dtProperties = new DataTable();
			this.dgProperties.CaptionText = mProcessAction.Name + " Properties";

			dtProperties.Columns.Add("Parameter", typeof(string)).ReadOnly = true;;
			dtProperties.Columns.Add("Type", typeof(string)).ReadOnly = true;
			dtProperties.Columns.Add("Value", typeof(string)).ReadOnly = !mAllowEdit;
			dtProperties.Columns.Add("Required", typeof(bool)).ReadOnly = true;
			dtProperties.Columns.Add("Description", typeof(string)).ReadOnly = true;
			
			// find out which type of class
			T = mProcessAction.GetType();

			// get a list of all the public fields
			Properties = T.GetProperties();

			foreach(PropertyInfo PropInfo in Properties)
			{
				Attributes = PropInfo.GetCustomAttributes(typeof(ProcessActionArgumentAttribute), false);
				
				if (Attributes.Length > 0)
				{
					NewRow = dtProperties.NewRow();
					
					NewRow["Parameter"] = PropInfo.Name;
					NewRow["Type"] = ((ProcessActionArgumentAttribute)Attributes[0]).TargetVariableType;
					NewRow["Value"] = PropInfo.GetValue(mProcessAction, null).ToString();
					NewRow["Required"] = ((ProcessActionArgumentAttribute)Attributes[0]).IsRequired;
					NewRow["Description"] = ((ProcessActionArgumentAttribute)Attributes[0]).Description;

					dtProperties.Rows.Add(NewRow);
				}
			}

			this.dgProperties.DataSource = dtProperties;
			this.dtProperties.RowChanged += new DataRowChangeEventHandler(Row_Changed);

			AutoSizeColumns();


		}

	
		private void AutoSizeColumns()
		{
			DataGridTableStyle ts = new DataGridTableStyle();
			ts.MappingName = dtProperties.TableName;

			this.dgProperties.TableStyles.Clear();
			this.dgProperties.TableStyles.Add(ts);

			foreach (DataColumn Col in dtProperties.Columns)
			{
				this.dgProperties.TableStyles[dtProperties.TableName].GridColumnStyles[Col.ColumnName].Width = LongestField(dtProperties, Col.ColumnName);			
			}

			this.dgProperties.TableStyles[dtProperties.TableName].AlternatingBackColor = System.Drawing.Color.LightGray;
		}
	
		
		private int LongestField (DataTable DT, string ColumnName)
		{
			int		maxlength = 0;
			int		tot = DT.Rows.Count;
			string	straux = "";
			int		intaux = 0;

			Graphics g = dgProperties.CreateGraphics();

			// Take width one blank space to add to the new width to the Column   
			int offset = Convert.ToInt32(Math.Ceiling(g.MeasureString(" ", dgProperties.Font).Width));

			for (int i=0; i<tot; ++i)
			{
				straux =  DT.Rows[i][ColumnName].ToString();

				// Get the width of Current Field String according to the Font
				intaux = Convert.ToInt32(Math.Ceiling(g.MeasureString(straux, dgProperties.Font).Width));
				if (intaux > maxlength)				
				{
					maxlength = intaux;
				}
			}

			// check the header too
			intaux = Convert.ToInt32(Math.Ceiling(g.MeasureString(ColumnName, dgProperties.Font).Width));
			if (intaux > maxlength)	maxlength = intaux;
			
			return maxlength + offset;
		}
 
		
		public void OnParameterChanged(object sender, ParameterChangedEventArgs ev)
		{
			if (this.ParameterChanged != null) this.ParameterChanged(sender, ev);
		}

		
		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.dgProperties = new System.Windows.Forms.DataGrid();
			this.btnApply = new System.Windows.Forms.Button();
			this.btnReset = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dgProperties)).BeginInit();
			this.SuspendLayout();
			// 
			// dgProperties
			// 
			this.dgProperties.AlternatingBackColor = System.Drawing.Color.LightGray;
			this.dgProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dgProperties.BackColor = System.Drawing.Color.Gainsboro;
			this.dgProperties.BackgroundColor = System.Drawing.Color.Silver;
			this.dgProperties.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dgProperties.CaptionBackColor = System.Drawing.Color.LightSteelBlue;
			this.dgProperties.CaptionForeColor = System.Drawing.Color.MidnightBlue;
			this.dgProperties.CaptionText = "Properties";
			this.dgProperties.DataMember = "";
			this.dgProperties.FlatMode = true;
			this.dgProperties.Font = new System.Drawing.Font("Tahoma", 8F);
			this.dgProperties.ForeColor = System.Drawing.Color.Black;
			this.dgProperties.GridLineColor = System.Drawing.Color.DimGray;
			this.dgProperties.GridLineStyle = System.Windows.Forms.DataGridLineStyle.None;
			this.dgProperties.HeaderBackColor = System.Drawing.Color.MidnightBlue;
			this.dgProperties.HeaderFont = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.dgProperties.HeaderForeColor = System.Drawing.Color.White;
			this.dgProperties.LinkColor = System.Drawing.Color.MidnightBlue;
			this.dgProperties.Location = new System.Drawing.Point(0, 0);
			this.dgProperties.Name = "dgProperties";
			this.dgProperties.ParentRowsBackColor = System.Drawing.Color.DarkGray;
			this.dgProperties.ParentRowsForeColor = System.Drawing.Color.Black;
			this.dgProperties.PreferredColumnWidth = 120;
			this.dgProperties.SelectionBackColor = System.Drawing.Color.CadetBlue;
			this.dgProperties.SelectionForeColor = System.Drawing.Color.White;
			this.dgProperties.Size = new System.Drawing.Size(368, 224);
			this.dgProperties.TabIndex = 0;
			// 
			// btnApply
			// 
			this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnApply.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnApply.Location = new System.Drawing.Point(8, 232);
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = new System.Drawing.Size(96, 24);
			this.btnApply.TabIndex = 1;
			this.btnApply.Text = "Apply";
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// btnReset
			// 
			this.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnReset.Location = new System.Drawing.Point(120, 232);
			this.btnReset.Name = "btnReset";
			this.btnReset.Size = new System.Drawing.Size(96, 24);
			this.btnReset.TabIndex = 2;
			this.btnReset.Text = "Reset";
			this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
			// 
			// ProcessActionViewer
			// 
			this.Controls.Add(this.btnReset);
			this.Controls.Add(this.btnApply);
			this.Controls.Add(this.dgProperties);
			this.Name = "ProcessActionViewer";
			this.Size = new System.Drawing.Size(368, 264);
			((System.ComponentModel.ISupportInitialize)(this.dgProperties)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion
	
		
		private void btnReset_Click(object sender, System.EventArgs e)
		{
			if (!this.mEdited) return;

			this.UpdateDisplay();

			this.btnReset.Visible = false;
			this.btnApply.Visible = false;
		}

		private void btnApply_Click(object sender, System.EventArgs e)
		{
			if (!this.mEdited) return;

			ParameterChangedEventArgs Ev = new ParameterChangedEventArgs(this.dtProperties);
			OnParameterChanged(this, Ev);
		}
		
		private void Row_Changed( object sender, DataRowChangeEventArgs e )
		{
			if (!this.mAllowEdit) return;

			if (!this.mEdited) this.dgProperties.CaptionText = this.dgProperties.CaptionText + " *";
			
			this.mEdited = true;
			this.btnApply.Visible = true;
			this.btnReset.Visible = true;
			
		}

	}

	public class ParameterChangedEventArgs : EventArgs
	{
		DataTable newInfo;
			
		public ParameterChangedEventArgs(DataTable DT) : base()
		{
			newInfo = DT;
		}

		public DataTable NewInfo
		{
			get {return newInfo;}
			set {newInfo = value;}
		}
	}

	public delegate void ParameterChangedEventHandler(object sender, ParameterChangedEventArgs ev);

}
