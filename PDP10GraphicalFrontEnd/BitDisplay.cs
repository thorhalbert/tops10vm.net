using System.Drawing;
using System.Windows.Forms;

namespace PDP10GraphicalFrontEnd
{
    public partial class BitDisplay : Control
    {
        public int Bit { get; set; }
        public ulong Value { get; set; }

        public new Size Size
        {
            get { return size; }
            set
            {
                //// Keep the aspect ratio square (for now)
                //if (size.Width == value.Width &&
                //    size.Height != value.Height)
                //    size.Width = size.Height = value.Height;
                //if (size.Height == value.Height &&
                //    size.Width != value.Width)
                //    size.Width = size.Height = value.Width;

                size.Width = size.Height = value.Width;

                specBounds();
            }
        }

        public new int Width
        {
            get { return size.Width; }
            set
            {
                size.Width = size.Height = value;
                specBounds();
            }
        }

        public new int Height
        {
            get { return size.Height; }
            set
            {
                size.Width = size.Height = value;
                specBounds();
            }
        }

        private void specBounds()
        {
            base.Width = size.Width;
            base.Height = size.Height;

            SetBounds(0, 0, size.Width + Margin.Horizontal, size.Height + Margin.Vertical, BoundsSpecified.All);

            BackColor = Color.Transparent;
        }

        private Size size;

        public BitDisplay()
        {
            InitializeComponent();

            size.Height = size.Width;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            specBounds();
        }

        private const int Image_Circle_Red = 3;

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            var g = pe.Graphics;

            IconImagery.Draw(g, Margin.Top, Margin.Left, Size.Width, Size.Height, Image_Circle_Red);
        }

        public void ValueChanged(ulong newValue)
        {
            Value = newValue;
        }
    }
}