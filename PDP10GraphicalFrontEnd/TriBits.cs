using System.Windows.Forms;

namespace PDP10GraphicalFrontEnd
{
    public partial class TriBits : UserControl
    {
        private int bitSize = 16;
        private int bitMargin = 2;

        public TriBits()
        {
            InitializeComponent();

            SetMargin();
        }

        public int BitSize
        {
            get { return bitSize; }
            set
            {
                bitSize = value;
                SetMargin();
            }
        }

        public int BitMargin
        {
            get { return bitMargin; }
            set
            {
                bitMargin = value;
                SetMargin();
            }
        }

        private void SetMargin()
        {
            bitDisplay1.Height =
                bitDisplay1.Width =
                bitDisplay2.Height =
                bitDisplay2.Width =
                bitDisplay3.Height =
                bitDisplay3.Width = bitSize;

            tableLayoutPanel1.RowStyles[0].Height = bitDisplay1.Height;
            tableLayoutPanel1.RowStyles[1].Height = label1.Height;

            tableLayoutPanel1.ColumnStyles[0].Width =
                tableLayoutPanel1.ColumnStyles[1].Width =
                tableLayoutPanel1.ColumnStyles[2].Width = bitDisplay1.Width;

            Width = bitDisplay1.Size.Width*3;
            Height = bitDisplay1.Size.Height + label1.Height;
        }
    }
}