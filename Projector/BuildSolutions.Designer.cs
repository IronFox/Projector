namespace Projector
{
	partial class BuildSolutions
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
			System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("All <-> None");
			this.buildSelection = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
			this.buildControl = new System.Windows.Forms.Panel();
			this.buildButton = new System.Windows.Forms.Button();
			this.eventLog = new System.Windows.Forms.ListBox();
			this.buildConfigurations = new System.Windows.Forms.ComboBox();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.buildMode = new System.Windows.Forms.ComboBox();
			this.statusStrip1.SuspendLayout();
			this.buildControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// buildSelection
			// 
			this.buildSelection.CheckBoxes = true;
			this.buildSelection.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.buildSelection.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buildSelection.FullRowSelect = true;
			this.buildSelection.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			listViewItem2.StateImageIndex = 0;
			this.buildSelection.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem2});
			this.buildSelection.Location = new System.Drawing.Point(0, 0);
			this.buildSelection.MultiSelect = false;
			this.buildSelection.Name = "buildSelection";
			this.buildSelection.Size = new System.Drawing.Size(930, 711);
			this.buildSelection.TabIndex = 0;
			this.buildSelection.UseCompatibleStateImageBehavior = false;
			this.buildSelection.View = System.Windows.Forms.View.Details;
			this.buildSelection.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.buildSelection_ItemChecked);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Project";
			this.columnHeader1.Width = 229;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Platform";
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar,
            this.toolStripStatusLabel});
			this.statusStrip1.Location = new System.Drawing.Point(0, 689);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(930, 22);
			this.statusStrip1.TabIndex = 1;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// toolStripProgressBar
			// 
			this.toolStripProgressBar.Name = "toolStripProgressBar";
			this.toolStripProgressBar.Size = new System.Drawing.Size(100, 16);
			// 
			// buildControl
			// 
			this.buildControl.Controls.Add(this.buildMode);
			this.buildControl.Controls.Add(this.buildButton);
			this.buildControl.Controls.Add(this.eventLog);
			this.buildControl.Controls.Add(this.buildConfigurations);
			this.buildControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buildControl.Location = new System.Drawing.Point(0, 400);
			this.buildControl.Name = "buildControl";
			this.buildControl.Size = new System.Drawing.Size(930, 289);
			this.buildControl.TabIndex = 2;
			// 
			// buildButton
			// 
			this.buildButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buildButton.Location = new System.Drawing.Point(549, 6);
			this.buildButton.Name = "buildButton";
			this.buildButton.Size = new System.Drawing.Size(369, 23);
			this.buildButton.TabIndex = 2;
			this.buildButton.Text = "Build";
			this.buildButton.UseVisualStyleBackColor = true;
			this.buildButton.Click += new System.EventHandler(this.buildButton_Click);
			// 
			// eventLog
			// 
			this.eventLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.eventLog.FormattingEnabled = true;
			this.eventLog.Location = new System.Drawing.Point(12, 35);
			this.eventLog.Name = "eventLog";
			this.eventLog.Size = new System.Drawing.Size(906, 238);
			this.eventLog.TabIndex = 1;
			// 
			// buildConfigurations
			// 
			this.buildConfigurations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.buildConfigurations.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.buildConfigurations.FormattingEnabled = true;
			this.buildConfigurations.Location = new System.Drawing.Point(217, 6);
			this.buildConfigurations.Name = "buildConfigurations";
			this.buildConfigurations.Size = new System.Drawing.Size(326, 21);
			this.buildConfigurations.TabIndex = 0;
			// 
			// splitter1
			// 
			this.splitter1.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.Location = new System.Drawing.Point(0, 397);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(930, 3);
			this.splitter1.TabIndex = 3;
			this.splitter1.TabStop = false;
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// toolStripStatusLabel
			// 
			this.toolStripStatusLabel.Name = "toolStripStatusLabel";
			this.toolStripStatusLabel.Size = new System.Drawing.Size(0, 17);
			// 
			// buildMode
			// 
			this.buildMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.buildMode.FormattingEnabled = true;
			this.buildMode.Items.AddRange(new object[] {
            "Force Rebuild Intermediate Projects",
            "Force Rebuild Final Projects",
            "No Forced Rebuilds"});
			this.buildMode.Location = new System.Drawing.Point(12, 6);
			this.buildMode.Name = "buildMode";
			this.buildMode.Size = new System.Drawing.Size(199, 21);
			this.buildMode.TabIndex = 3;
			// 
			// BuildSolutions
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(930, 711);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.buildControl);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.buildSelection);
			this.Name = "BuildSolutions";
			this.Text = "BuildSolutions";
			this.Shown += new System.EventHandler(this.BuildSolutions_Shown);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.buildControl.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView buildSelection;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.Panel buildControl;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.ComboBox buildConfigurations;
		private System.Windows.Forms.Button buildButton;
		private System.Windows.Forms.ListBox eventLog;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
		private System.Windows.Forms.ComboBox buildMode;
	}
}