namespace PDP10GraphicalFrontEnd
{
	partial class NixieNumeric
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NixieNumeric));
			this.Nixies = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// Nixies
			// 
			this.Nixies.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("Nixies.ImageStream")));
			this.Nixies.TransparentColor = System.Drawing.Color.Transparent;
			this.Nixies.Images.SetKeyName(0, "M100.gif");
			this.Nixies.Images.SetKeyName(1, "M101.gif");
			this.Nixies.Images.SetKeyName(2, "M102.gif");
			this.Nixies.Images.SetKeyName(3, "M103.gif");
			this.Nixies.Images.SetKeyName(4, "M104.gif");
			this.Nixies.Images.SetKeyName(5, "M105.gif");
			this.Nixies.Images.SetKeyName(6, "M106.gif");
			this.Nixies.Images.SetKeyName(7, "M107.gif");
			this.Nixies.Images.SetKeyName(8, "M108.gif");
			this.Nixies.Images.SetKeyName(9, "M109.gif");
			this.Nixies.Images.SetKeyName(10, "P100.gif");
			this.Nixies.Images.SetKeyName(11, "P101.gif");
			this.Nixies.Images.SetKeyName(12, "qblank.gif");
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ImageList Nixies;
	}
}
