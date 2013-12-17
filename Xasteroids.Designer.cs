namespace Xasteroids
{
	partial class Xasteroids
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
			this.SuspendLayout();
			// 
			// Xasteroids
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(784, 562);
			this.MinimumSize = new System.Drawing.Size(800, 600);
			this.Name = "Xasteroids";
			this.Text = "Xasteroids";
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BeyondBeyaan_MouseDown);
			this.MouseEnter += new System.EventHandler(this.BeyondBeyaan_MouseEnter);
			this.MouseLeave += new System.EventHandler(this.BeyondBeyaan_MouseLeave);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.BeyondBeyaan_MouseMove);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BeyondBeyaan_MouseUp);
			this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.BeyondBeyaan_MouseWheel);
			this.ResumeLayout(false);

		}

		#endregion
	}
}

