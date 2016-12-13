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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("All <-> None");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectView));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.flushPathRegistryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.locationOfProjectFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.pathRegistryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recentSolutionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.clearListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadedSolutionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openGeneratedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.unloadSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.solutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buildAtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.openGeneratedSolutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.overwriteExistingVSUserConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.forceOverwriteProjectFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSolutionDialog = new System.Windows.Forms.OpenFileDialog();
            this.openProjectDialog = new System.Windows.Forms.OpenFileDialog();
            this.solutionViewSplit = new System.Windows.Forms.SplitContainer();
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.tabRecent = new System.Windows.Forms.TabPage();
            this.recentSolutions = new System.Windows.Forms.Panel();
            this.tabLoaded = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.openSelectedButton = new System.Windows.Forms.Button();
            this.generateSelectedButton = new System.Windows.Forms.Button();
            this.loadedSolutionsView = new System.Windows.Forms.ListView();
            this.solutionNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabSelected = new System.Windows.Forms.TabPage();
            this.solutionView = new System.Windows.Forms.TreeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.openGeneratedSolutionButton = new System.Windows.Forms.Button();
            this.buildSolutionButton = new System.Windows.Forms.Button();
            this.log = new System.Windows.Forms.TextBox();
            this.logLabel = new System.Windows.Forms.Label();
            this.chooseDestination = new System.Windows.Forms.SaveFileDialog();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.toolsetLabel = new System.Windows.Forms.Label();
            this.toolSet = new System.Windows.Forms.ComboBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.solutionViewSplit)).BeginInit();
            this.solutionViewSplit.Panel1.SuspendLayout();
            this.solutionViewSplit.Panel2.SuspendLayout();
            this.solutionViewSplit.SuspendLayout();
            this.mainTabControl.SuspendLayout();
            this.tabRecent.SuspendLayout();
            this.tabLoaded.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tabSelected.SuspendLayout();
            this.panel1.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.loadedSolutionsToolStripMenuItem,
            this.solutionToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(564, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadProjectToolStripMenuItem,
            this.toolStripMenuItem3,
            this.flushPathRegistryToolStripMenuItem,
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
            // flushPathRegistryToolStripMenuItem
            // 
            this.flushPathRegistryToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.locationOfProjectFileToolStripMenuItem,
            this.toolStripMenuItem5,
            this.pathRegistryToolStripMenuItem});
            this.flushPathRegistryToolStripMenuItem.Name = "flushPathRegistryToolStripMenuItem";
            this.flushPathRegistryToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.flushPathRegistryToolStripMenuItem.Text = "Flush";
            // 
            // locationOfProjectFileToolStripMenuItem
            // 
            this.locationOfProjectFileToolStripMenuItem.Enabled = false;
            this.locationOfProjectFileToolStripMenuItem.Name = "locationOfProjectFileToolStripMenuItem";
            this.locationOfProjectFileToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.locationOfProjectFileToolStripMenuItem.Text = "Location of Project File";
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(192, 6);
            // 
            // pathRegistryToolStripMenuItem
            // 
            this.pathRegistryToolStripMenuItem.Name = "pathRegistryToolStripMenuItem";
            this.pathRegistryToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.pathRegistryToolStripMenuItem.Text = "Entire Path Registry";
            this.pathRegistryToolStripMenuItem.Click += new System.EventHandler(this.pathRegistryToolStripMenuItem_Click_1);
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
            // loadedSolutionsToolStripMenuItem
            // 
            this.loadedSolutionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.generateSelectedToolStripMenuItem,
            this.openGeneratedToolStripMenuItem,
            this.toolStripMenuItem6,
            this.unloadSelectedToolStripMenuItem});
            this.loadedSolutionsToolStripMenuItem.Name = "loadedSolutionsToolStripMenuItem";
            this.loadedSolutionsToolStripMenuItem.Size = new System.Drawing.Size(110, 20);
            this.loadedSolutionsToolStripMenuItem.Text = "Loaded Solutions";
            // 
            // generateSelectedToolStripMenuItem
            // 
            this.generateSelectedToolStripMenuItem.Name = "generateSelectedToolStripMenuItem";
            this.generateSelectedToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.generateSelectedToolStripMenuItem.Text = "Generate Selected";
            this.generateSelectedToolStripMenuItem.Click += new System.EventHandler(this.generateSelectedButton_Click);
            // 
            // openGeneratedToolStripMenuItem
            // 
            this.openGeneratedToolStripMenuItem.Name = "openGeneratedToolStripMenuItem";
            this.openGeneratedToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.openGeneratedToolStripMenuItem.Text = "Open Generated";
            this.openGeneratedToolStripMenuItem.Click += new System.EventHandler(this.openSelectedButton_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(165, 6);
            // 
            // unloadSelectedToolStripMenuItem
            // 
            this.unloadSelectedToolStripMenuItem.Name = "unloadSelectedToolStripMenuItem";
            this.unloadSelectedToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.unloadSelectedToolStripMenuItem.Text = "Unload Selected";
            this.unloadSelectedToolStripMenuItem.Click += new System.EventHandler(this.unloadSelectedToolStripMenuItem_Click);
            // 
            // solutionToolStripMenuItem
            // 
            this.solutionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buildToolStripMenuItem,
            this.buildAtToolStripMenuItem,
            this.toolStripMenuItem4,
            this.openGeneratedSolutionToolStripMenuItem});
            this.solutionToolStripMenuItem.Enabled = false;
            this.solutionToolStripMenuItem.Name = "solutionToolStripMenuItem";
            this.solutionToolStripMenuItem.Size = new System.Drawing.Size(110, 20);
            this.solutionToolStripMenuItem.Text = "Focused Solution";
            this.solutionToolStripMenuItem.Click += new System.EventHandler(this.solutionToolStripMenuItem_Click);
            // 
            // buildToolStripMenuItem
            // 
            this.buildToolStripMenuItem.Name = "buildToolStripMenuItem";
            this.buildToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.buildToolStripMenuItem.Text = "Build";
            this.buildToolStripMenuItem.Click += new System.EventHandler(this.buildToolStripMenuItem_Click);
            // 
            // buildAtToolStripMenuItem
            // 
            this.buildAtToolStripMenuItem.Name = "buildAtToolStripMenuItem";
            this.buildAtToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.buildAtToolStripMenuItem.Text = "Build at...";
            this.buildAtToolStripMenuItem.Click += new System.EventHandler(this.buildAtToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(204, 6);
            // 
            // openGeneratedSolutionToolStripMenuItem
            // 
            this.openGeneratedSolutionToolStripMenuItem.Enabled = false;
            this.openGeneratedSolutionToolStripMenuItem.Name = "openGeneratedSolutionToolStripMenuItem";
            this.openGeneratedSolutionToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.openGeneratedSolutionToolStripMenuItem.Text = "Open Generated Solution";
            this.openGeneratedSolutionToolStripMenuItem.Click += new System.EventHandler(this.openGeneratedSolutionToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.overwriteExistingVSUserConfigToolStripMenuItem,
            this.forceOverwriteProjectFilesToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // overwriteExistingVSUserConfigToolStripMenuItem
            // 
            this.overwriteExistingVSUserConfigToolStripMenuItem.Name = "overwriteExistingVSUserConfigToolStripMenuItem";
            this.overwriteExistingVSUserConfigToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.overwriteExistingVSUserConfigToolStripMenuItem.Text = "Overwrite existing VS User-Config";
            // 
            // forceOverwriteProjectFilesToolStripMenuItem
            // 
            this.forceOverwriteProjectFilesToolStripMenuItem.Name = "forceOverwriteProjectFilesToolStripMenuItem";
            this.forceOverwriteProjectFilesToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.forceOverwriteProjectFilesToolStripMenuItem.Text = "Force overwrite project files";
            this.forceOverwriteProjectFilesToolStripMenuItem.Click += new System.EventHandler(this.forceOverwriteProjectFilesToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
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
            // solutionViewSplit
            // 
            this.solutionViewSplit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.solutionViewSplit.Location = new System.Drawing.Point(0, 55);
            this.solutionViewSplit.Name = "solutionViewSplit";
            this.solutionViewSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // solutionViewSplit.Panel1
            // 
            this.solutionViewSplit.Panel1.Controls.Add(this.mainTabControl);
            this.solutionViewSplit.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel1_Paint);
            // 
            // solutionViewSplit.Panel2
            // 
            this.solutionViewSplit.Panel2.Controls.Add(this.log);
            this.solutionViewSplit.Panel2.Controls.Add(this.logLabel);
            this.solutionViewSplit.Size = new System.Drawing.Size(564, 382);
            this.solutionViewSplit.SplitterDistance = 260;
            this.solutionViewSplit.TabIndex = 3;
            // 
            // mainTabControl
            // 
            this.mainTabControl.Controls.Add(this.tabRecent);
            this.mainTabControl.Controls.Add(this.tabLoaded);
            this.mainTabControl.Controls.Add(this.tabSelected);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Location = new System.Drawing.Point(0, 0);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(564, 260);
            this.mainTabControl.TabIndex = 3;
            // 
            // tabRecent
            // 
            this.tabRecent.Controls.Add(this.recentSolutions);
            this.tabRecent.Location = new System.Drawing.Point(4, 22);
            this.tabRecent.Name = "tabRecent";
            this.tabRecent.Padding = new System.Windows.Forms.Padding(3);
            this.tabRecent.Size = new System.Drawing.Size(556, 234);
            this.tabRecent.TabIndex = 0;
            this.tabRecent.Text = "Recent";
            this.tabRecent.UseVisualStyleBackColor = true;
            // 
            // recentSolutions
            // 
            this.recentSolutions.AllowDrop = true;
            this.recentSolutions.AutoScroll = true;
            this.recentSolutions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recentSolutions.Location = new System.Drawing.Point(3, 3);
            this.recentSolutions.Name = "recentSolutions";
            this.recentSolutions.Size = new System.Drawing.Size(550, 228);
            this.recentSolutions.TabIndex = 7;
            this.recentSolutions.DragDrop += new System.Windows.Forms.DragEventHandler(this.recentSolutions_DragDrop);
            this.recentSolutions.DragEnter += new System.Windows.Forms.DragEventHandler(this.recentSolutions_DragEnter);
            // 
            // tabLoaded
            // 
            this.tabLoaded.Controls.Add(this.panel3);
            this.tabLoaded.Controls.Add(this.loadedSolutionsView);
            this.tabLoaded.Location = new System.Drawing.Point(4, 22);
            this.tabLoaded.Name = "tabLoaded";
            this.tabLoaded.Size = new System.Drawing.Size(556, 234);
            this.tabLoaded.TabIndex = 2;
            this.tabLoaded.Text = "Loaded";
            this.tabLoaded.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.AutoSize = true;
            this.panel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel3.Controls.Add(this.openSelectedButton);
            this.panel3.Controls.Add(this.generateSelectedButton);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 204);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(556, 30);
            this.panel3.TabIndex = 1;
            // 
            // openSelectedButton
            // 
            this.openSelectedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.openSelectedButton.Location = new System.Drawing.Point(283, 0);
            this.openSelectedButton.Name = "openSelectedButton";
            this.openSelectedButton.Size = new System.Drawing.Size(273, 27);
            this.openSelectedButton.TabIndex = 1;
            this.openSelectedButton.Text = "Open Generated";
            this.openSelectedButton.UseVisualStyleBackColor = true;
            this.openSelectedButton.Click += new System.EventHandler(this.openSelectedButton_Click);
            // 
            // generateSelectedButton
            // 
            this.generateSelectedButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.generateSelectedButton.Location = new System.Drawing.Point(-1, 0);
            this.generateSelectedButton.Name = "generateSelectedButton";
            this.generateSelectedButton.Size = new System.Drawing.Size(283, 27);
            this.generateSelectedButton.TabIndex = 0;
            this.generateSelectedButton.Text = "Generate Selected Solutions";
            this.generateSelectedButton.UseVisualStyleBackColor = true;
            this.generateSelectedButton.Click += new System.EventHandler(this.generateSelectedButton_Click);
            // 
            // loadedSolutionsView
            // 
            this.loadedSolutionsView.AllowDrop = true;
            this.loadedSolutionsView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loadedSolutionsView.CheckBoxes = true;
            this.loadedSolutionsView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.solutionNameHeader,
            this.columnHeader1,
            this.columnHeader2});
            this.loadedSolutionsView.FullRowSelect = true;
            this.loadedSolutionsView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listViewItem1.StateImageIndex = 0;
            this.loadedSolutionsView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.loadedSolutionsView.Location = new System.Drawing.Point(0, 0);
            this.loadedSolutionsView.MultiSelect = false;
            this.loadedSolutionsView.Name = "loadedSolutionsView";
            this.loadedSolutionsView.Size = new System.Drawing.Size(556, 197);
            this.loadedSolutionsView.TabIndex = 0;
            this.loadedSolutionsView.UseCompatibleStateImageBehavior = false;
            this.loadedSolutionsView.View = System.Windows.Forms.View.Details;
            this.loadedSolutionsView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.solutionListBox_ItemCheck);
            this.loadedSolutionsView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.loadedSolutionsView_ItemChecked);
            this.loadedSolutionsView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.loadedSolutionsView_ItemSelectionChanged);
            this.loadedSolutionsView.DragDrop += new System.Windows.Forms.DragEventHandler(this.recentSolutions_DragDrop);
            this.loadedSolutionsView.DragEnter += new System.Windows.Forms.DragEventHandler(this.recentSolutions_DragEnter);
            // 
            // solutionNameHeader
            // 
            this.solutionNameHeader.Text = "Name";
            this.solutionNameHeader.Width = 270;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Primary";
            this.columnHeader1.Width = 180;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "#Projects";
            // 
            // tabSelected
            // 
            this.tabSelected.Controls.Add(this.solutionView);
            this.tabSelected.Controls.Add(this.panel1);
            this.tabSelected.Location = new System.Drawing.Point(4, 22);
            this.tabSelected.Name = "tabSelected";
            this.tabSelected.Padding = new System.Windows.Forms.Padding(3);
            this.tabSelected.Size = new System.Drawing.Size(556, 234);
            this.tabSelected.TabIndex = 1;
            this.tabSelected.Text = "Focused (none)";
            this.tabSelected.UseVisualStyleBackColor = true;
            // 
            // solutionView
            // 
            this.solutionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.solutionView.FullRowSelect = true;
            this.solutionView.Location = new System.Drawing.Point(3, 26);
            this.solutionView.Name = "solutionView";
            this.solutionView.ShowLines = false;
            this.solutionView.Size = new System.Drawing.Size(550, 205);
            this.solutionView.TabIndex = 6;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.openGeneratedSolutionButton);
            this.panel1.Controls.Add(this.buildSolutionButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(550, 23);
            this.panel1.TabIndex = 5;
            // 
            // openGeneratedSolutionButton
            // 
            this.openGeneratedSolutionButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.openGeneratedSolutionButton.Enabled = false;
            this.openGeneratedSolutionButton.Location = new System.Drawing.Point(275, 0);
            this.openGeneratedSolutionButton.Name = "openGeneratedSolutionButton";
            this.openGeneratedSolutionButton.Size = new System.Drawing.Size(275, 23);
            this.openGeneratedSolutionButton.TabIndex = 3;
            this.openGeneratedSolutionButton.Text = "Open Generated";
            this.openGeneratedSolutionButton.UseVisualStyleBackColor = true;
            this.openGeneratedSolutionButton.Click += new System.EventHandler(this.openGeneratedSolutionToolStripMenuItem_Click);
            // 
            // buildSolutionButton
            // 
            this.buildSolutionButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buildSolutionButton.Enabled = false;
            this.buildSolutionButton.Location = new System.Drawing.Point(0, 0);
            this.buildSolutionButton.Name = "buildSolutionButton";
            this.buildSolutionButton.Size = new System.Drawing.Size(276, 23);
            this.buildSolutionButton.TabIndex = 2;
            this.buildSolutionButton.Text = "Build Solution";
            this.buildSolutionButton.UseVisualStyleBackColor = true;
            this.buildSolutionButton.Click += new System.EventHandler(this.buildSolutionButton_Click);
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
            this.log.Size = new System.Drawing.Size(564, 95);
            this.log.TabIndex = 1;
            // 
            // logLabel
            // 
            this.logLabel.AutoSize = true;
            this.logLabel.Location = new System.Drawing.Point(3, 6);
            this.logLabel.Name = "logLabel";
            this.logLabel.Size = new System.Drawing.Size(25, 13);
            this.logLabel.TabIndex = 0;
            this.logLabel.Text = "Log";
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip.Location = new System.Drawing.Point(0, 440);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(564, 22);
            this.statusStrip.TabIndex = 5;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.toolsetLabel);
            this.panel2.Controls.Add(this.toolSet);
            this.panel2.Location = new System.Drawing.Point(0, 24);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(563, 31);
            this.panel2.TabIndex = 6;
            // 
            // toolsetLabel
            // 
            this.toolsetLabel.AutoSize = true;
            this.toolsetLabel.Location = new System.Drawing.Point(0, 7);
            this.toolsetLabel.Name = "toolsetLabel";
            this.toolsetLabel.Size = new System.Drawing.Size(80, 13);
            this.toolsetLabel.TabIndex = 3;
            this.toolsetLabel.Text = "Toolset Version";
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
            "14.0 (VS 2015)",
            "14.1 (VS 2017)"});
            this.toolSet.Location = new System.Drawing.Point(86, 5);
            this.toolSet.Name = "toolSet";
            this.toolSet.Size = new System.Drawing.Size(178, 21);
            this.toolSet.TabIndex = 2;
            this.toolSet.ValueMember = "14.0 (VS 2015)";
            this.toolSet.SelectedIndexChanged += new System.EventHandler(this.toolSet_SelectedIndexChanged_1);
            // 
            // ProjectView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(564, 462);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.solutionViewSplit);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(579, 499);
            this.Name = "ProjectView";
            this.Text = "Projector";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ProjectView_FormClosed);
            this.Load += new System.EventHandler(this.ProjectView_Load);
            this.Shown += new System.EventHandler(this.ProjectView_Shown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.solutionViewSplit.Panel1.ResumeLayout(false);
            this.solutionViewSplit.Panel2.ResumeLayout(false);
            this.solutionViewSplit.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.solutionViewSplit)).EndInit();
            this.solutionViewSplit.ResumeLayout(false);
            this.mainTabControl.ResumeLayout(false);
            this.tabRecent.ResumeLayout(false);
            this.tabLoaded.ResumeLayout(false);
            this.tabLoaded.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.tabSelected.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
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
		private System.Windows.Forms.SplitContainer solutionViewSplit;
        private System.Windows.Forms.TextBox log;
        private System.Windows.Forms.Label logLabel;
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
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
		private System.Windows.Forms.ToolStripMenuItem openGeneratedSolutionToolStripMenuItem;
		private System.Windows.Forms.TabControl mainTabControl;
		private System.Windows.Forms.TabPage tabRecent;
		private System.Windows.Forms.Panel recentSolutions;
		private System.Windows.Forms.TabPage tabSelected;
		private System.Windows.Forms.TreeView solutionView;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button openGeneratedSolutionButton;
		private System.Windows.Forms.Button buildSolutionButton;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label toolsetLabel;
		private System.Windows.Forms.ComboBox toolSet;
		private System.Windows.Forms.TabPage tabLoaded;
		private System.Windows.Forms.ListView loadedSolutionsView;
		private System.Windows.Forms.ColumnHeader solutionNameHeader;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ToolStripMenuItem loadedSolutionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem generateSelectedToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openGeneratedToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
		private System.Windows.Forms.ToolStripMenuItem unloadSelectedToolStripMenuItem;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Button openSelectedButton;
		private System.Windows.Forms.Button generateSelectedButton;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem flushPathRegistryToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pathRegistryToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem locationOfProjectFileToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem overwriteExistingVSUserConfigToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem forceOverwriteProjectFilesToolStripMenuItem;
	}
}

