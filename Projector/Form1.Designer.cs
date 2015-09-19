namespace Projector
{
    partial class ProjectView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectView));
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.recentSolutionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.clearListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.solutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.buildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.buildAtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openSolutionDialog = new System.Windows.Forms.OpenFileDialog();
			this.openProjectDialog = new System.Windows.Forms.OpenFileDialog();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.solutionView = new System.Windows.Forms.TreeView();
			this.log = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.toolSet = new System.Windows.Forms.ComboBox();
			this.chooseDestination = new System.Windows.Forms.SaveFileDialog();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.buildSolutionButton = new System.Windows.Forms.Button();
			this.menuStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.solutionToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(577, 24);
			this.menuStrip1.TabIndex = 2;
			this.menuStrip1.Text = "menuStrip1";
			this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadProjectToolStripMenuItem,
            this.toolStripMenuItem3,
            this.recentSolutionsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.quitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// loadProjectToolStripMenuItem
			// 
			this.loadProjectToolStripMenuItem.Name = "loadProjectToolStripMenuItem";
			this.loadProjectToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
			this.loadProjectToolStripMenuItem.Text = "Load Solution...";
			this.loadProjectToolStripMenuItem.Click += new System.EventHandler(this.loadProjectToolStripMenuItem_Click);
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(159, 6);
			// 
			// recentSolutionsToolStripMenuItem
			// 
			this.recentSolutionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.clearListToolStripMenuItem});
			this.recentSolutionsToolStripMenuItem.Name = "recentSolutionsToolStripMenuItem";
			this.recentSolutionsToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
			this.recentSolutionsToolStripMenuItem.Text = "Recent Solutions";
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(119, 6);
			// 
			// clearListToolStripMenuItem
			// 
			this.clearListToolStripMenuItem.Name = "clearListToolStripMenuItem";
			this.clearListToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
			this.clearListToolStripMenuItem.Text = "Clear List";
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(159, 6);
			// 
			// quitToolStripMenuItem
			// 
			this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
			this.quitToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
			this.quitToolStripMenuItem.Text = "Quit";
			this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
			// 
			// solutionToolStripMenuItem
			// 
			this.solutionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buildToolStripMenuItem,
            this.buildAtToolStripMenuItem});
			this.solutionToolStripMenuItem.Enabled = false;
			this.solutionToolStripMenuItem.Name = "solutionToolStripMenuItem";
			this.solutionToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
			this.solutionToolStripMenuItem.Text = "Solution";
			// 
			// buildToolStripMenuItem
			// 
			this.buildToolStripMenuItem.Name = "buildToolStripMenuItem";
			this.buildToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
			this.buildToolStripMenuItem.Text = "Build";
			this.buildToolStripMenuItem.Click += new System.EventHandler(this.buildToolStripMenuItem_Click);
			// 
			// buildAtToolStripMenuItem
			// 
			this.buildAtToolStripMenuItem.Name = "buildAtToolStripMenuItem";
			this.buildAtToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
			this.buildAtToolStripMenuItem.Text = "Build at...";
			this.buildAtToolStripMenuItem.Click += new System.EventHandler(this.buildAtToolStripMenuItem_Click);
			// 
			// openSolutionDialog
			// 
			this.openSolutionDialog.DefaultExt = "solution";
			this.openSolutionDialog.Filter = "Solutions|*.solution|All files|*.*";
			this.openSolutionDialog.ReadOnlyChecked = true;
			// 
			// openProjectDialog
			// 
			this.openProjectDialog.DefaultExt = "project";
			this.openProjectDialog.Filter = "Projects|*.project|All files|*.*";
			this.openProjectDialog.ReadOnlyChecked = true;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = new System.Drawing.Point(0, 55);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.solutionView);
			this.splitContainer1.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel1_Paint);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.log);
			this.splitContainer1.Panel2.Controls.Add(this.label2);
			this.splitContainer1.Size = new System.Drawing.Size(577, 378);
			this.splitContainer1.SplitterDistance = 259;
			this.splitContainer1.TabIndex = 3;
			// 
			// solutionView
			// 
			this.solutionView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.solutionView.Location = new System.Drawing.Point(0, 0);
			this.solutionView.Name = "solutionView";
			this.solutionView.Size = new System.Drawing.Size(577, 259);
			this.solutionView.TabIndex = 2;
			// 
			// log
			// 
			this.log.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.log.Location = new System.Drawing.Point(0, 22);
			this.log.MaxLength = 65536;
			this.log.Multiline = true;
			this.log.Name = "log";
			this.log.ReadOnly = true;
			this.log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.log.Size = new System.Drawing.Size(577, 93);
			this.log.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 6);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(25, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Log";
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.buildSolutionButton);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.toolSet);
			this.panel1.Location = new System.Drawing.Point(0, 28);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(577, 23);
			this.panel1.TabIndex = 4;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(80, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Toolset Version";
			// 
			// toolSet
			// 
			this.toolSet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.toolSet.DisplayMember = "1";
			this.toolSet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.toolSet.FormattingEnabled = true;
			this.toolSet.Items.AddRange(new object[] {
            "12.0 (VS 2013)",
            "14.0 (VS 2015)"});
			this.toolSet.Location = new System.Drawing.Point(89, 0);
			this.toolSet.Name = "toolSet";
			this.toolSet.Size = new System.Drawing.Size(349, 21);
			this.toolSet.TabIndex = 0;
			this.toolSet.ValueMember = "14.0 (VS 2015)";
			this.toolSet.SelectedIndexChanged += new System.EventHandler(this.toolSet_SelectedIndexChanged);
			// 
			// statusStrip
			// 
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
			this.statusStrip.Location = new System.Drawing.Point(0, 436);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Size = new System.Drawing.Size(577, 22);
			this.statusStrip.TabIndex = 5;
			this.statusStrip.Text = "statusStrip1";
			// 
			// toolStripStatusLabel1
			// 
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
			this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
			// 
			// buildSolutionButton
			// 
			this.buildSolutionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buildSolutionButton.Enabled = false;
			this.buildSolutionButton.Location = new System.Drawing.Point(444, -1);
			this.buildSolutionButton.Name = "buildSolutionButton";
			this.buildSolutionButton.Size = new System.Drawing.Size(132, 23);
			this.buildSolutionButton.TabIndex = 2;
			this.buildSolutionButton.Text = "Build Solution";
			this.buildSolutionButton.UseVisualStyleBackColor = true;
			this.buildSolutionButton.Click += new System.EventHandler(this.buildToolStripMenuItem_Click);
			// 
			// ProjectView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(577, 458);
			this.Controls.Add(this.statusStrip);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "ProjectView";
			this.Text = "Projector";
			this.Load += new System.EventHandler(this.ProjectView_Load);
			this.Shown += new System.EventHandler(this.ProjectView_Shown);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openSolutionDialog;
        private System.Windows.Forms.OpenFileDialog openProjectDialog;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView solutionView;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox toolSet;
        private System.Windows.Forms.TextBox log;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem solutionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem buildToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem buildAtToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recentSolutionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem clearListToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog chooseDestination;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.Button buildSolutionButton;
    }
}

