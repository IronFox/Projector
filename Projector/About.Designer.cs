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
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
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
			// richTextBox1
			// 
			this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox1.Location = new System.Drawing.Point(0, 30);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new System.Drawing.Size(284, 149);
			this.richTextBox1.TabIndex = 1;
			this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
			// 
			// About
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 179);
			this.Controls.Add(this.richTextBox1);
			this.Controls.Add(this.aboutTitle);
			this.Name = "About";
			this.Text = "About";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label aboutTitle;
		private System.Windows.Forms.RichTextBox richTextBox1;
	}
}