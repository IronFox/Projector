namespace Projector
{
	partial class About
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
			this.aboutTitle = new System.Windows.Forms.Label();
			this.textBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// aboutTitle
			// 
			this.aboutTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.aboutTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.aboutTitle.Image = global::Projector.Properties.Resources.Projector;
			this.aboutTitle.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.aboutTitle.Location = new System.Drawing.Point(0, 0);
			this.aboutTitle.Name = "aboutTitle";
			this.aboutTitle.Size = new System.Drawing.Size(284, 30);
			this.aboutTitle.TabIndex = 0;
			this.aboutTitle.Text = "Projector";
			this.aboutTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBox
			// 
			this.textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox.Location = new System.Drawing.Point(0, 30);
			this.textBox.Multiline = true;
			this.textBox.Name = "textBox";
			this.textBox.ReadOnly = true;
			this.textBox.Size = new System.Drawing.Size(284, 149);
			this.textBox.TabIndex = 1;
			this.textBox.TabStop = false;
			this.textBox.Text = resources.GetString("textBox.Text");
			// 
			// About
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 179);
			this.Controls.Add(this.textBox);
			this.Controls.Add(this.aboutTitle);
			this.MinimumSize = new System.Drawing.Size(300, 217);
			this.Name = "About";
			this.Text = "About";
			this.Load += new System.EventHandler(this.About_Load);
			this.Shown += new System.EventHandler(this.About_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label aboutTitle;
		private System.Windows.Forms.TextBox textBox;
	}
}