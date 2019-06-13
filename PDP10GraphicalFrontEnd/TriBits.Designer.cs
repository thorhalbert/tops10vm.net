namespace PDP10GraphicalFrontEnd
{
	partial class TriBits
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.bitDisplay1 = new PDP10GraphicalFrontEnd.BitDisplay();
			this.bitDisplay2 = new PDP10GraphicalFrontEnd.BitDisplay();
			this.bitDisplay3 = new PDP10GraphicalFrontEnd.BitDisplay();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.bitDisplay1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.bitDisplay2, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.bitDisplay3, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(346, 196);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.label1, 3);
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("OCR A Extended", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(3, 87);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(340, 109);
			this.label1.TabIndex = 3;
			this.label1.Text = "0";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// bitDisplay1
			// 
			this.bitDisplay1.BackColor = System.Drawing.Color.Transparent;
			this.bitDisplay1.Bit = 0;
			this.bitDisplay1.Location = new System.Drawing.Point(3, 3);
			this.bitDisplay1.Name = "bitDisplay1";
			this.bitDisplay1.Size = new System.Drawing.Size(75, 75);
			this.bitDisplay1.TabIndex = 0;
			this.bitDisplay1.Text = "bitDisplay1";
			this.bitDisplay1.Value = ((ulong)(0ul));
			// 
			// bitDisplay2
			// 
			this.bitDisplay2.BackColor = System.Drawing.Color.Transparent;
			this.bitDisplay2.Bit = 0;
			this.bitDisplay2.Location = new System.Drawing.Point(90, 3);
			this.bitDisplay2.Name = "bitDisplay2";
			this.bitDisplay2.Size = new System.Drawing.Size(75, 75);
			this.bitDisplay2.TabIndex = 1;
			this.bitDisplay2.Text = "bitDisplay2";
			this.bitDisplay2.Value = ((ulong)(0ul));
			// 
			// bitDisplay3
			// 
			this.bitDisplay3.BackColor = System.Drawing.Color.Transparent;
			this.bitDisplay3.Bit = 0;
			this.bitDisplay3.Location = new System.Drawing.Point(177, 3);
			this.bitDisplay3.Name = "bitDisplay3";
			this.bitDisplay3.Size = new System.Drawing.Size(75, 75);
			this.bitDisplay3.TabIndex = 2;
			this.bitDisplay3.Text = "bitDisplay3";
			this.bitDisplay3.Value = ((ulong)(0ul));
			// 
			// TriBits
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.Name = "TriBits";
			this.Size = new System.Drawing.Size(346, 196);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private BitDisplay bitDisplay1;
		private BitDisplay bitDisplay2;
		private BitDisplay bitDisplay3;
		private System.Windows.Forms.Label label1;
	}
}
