namespace PDP10GraphicalFrontEnd
{
	partial class OuterDebuggerForm
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
			this.debuggerScreen1 = new PDP10GraphicalFrontEnd.DebuggerScreen();
			this.SuspendLayout();
			// 
			// debuggerScreen1
			// 
			this.debuggerScreen1.BackColor = System.Drawing.SystemColors.ControlText;
			this.debuggerScreen1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.debuggerScreen1.ForeColor = System.Drawing.SystemColors.Control;
			this.debuggerScreen1.Location = new System.Drawing.Point(0, 0);
			this.debuggerScreen1.Name = "debuggerScreen1";
			this.debuggerScreen1.Size = new System.Drawing.Size(869, 605);
			this.debuggerScreen1.TabIndex = 0;
			// 
			// OuterDebuggerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(869, 605);
			this.Controls.Add(this.debuggerScreen1);
			this.Name = "OuterDebuggerForm";
			this.Text = "OuterDebuggerForm";
			this.ResumeLayout(false);

		}

		#endregion

		private DebuggerScreen debuggerScreen1;
	}
}