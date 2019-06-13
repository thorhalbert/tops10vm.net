using System.Drawing;
using System.Windows.Forms;

namespace PDP10GraphicalFrontEnd
{
    public partial class NixieNumeric : Control
    {
        private bool geomDirty = true;

        public ulong Value { get; set; }

        public int Digits
        {
            get { return digits; }
            set
            {
                digits = value;
                geomDirty = true;
                calcGeom();
            }
        }

        public bool Leadzero
        {
            get { return leadzero; }
            set
            {
                leadzero = value;
                geomDirty = true;
                Invalidate();
            }
        }

        public int NumBase
        {
            get { return numBase; }
            set
            {
                numBase = value;
                geomDirty = true;
                Invalidate();
            }
        }

        public new Size Size
        {
            get { return size; }
            set
            {
                size = value;
                geomDirty = true;
                Invalidate();
            }
        }

        public new int Height
        {
            get { return size.Height; }
            set
            {
                size.Height = value;
                geomDirty = true;
                Invalidate();
            }
        }

        public new int Width
        {
            get { return size.Width; }
            set
            {
                size.Width = value;
                geomDirty = true;
                Invalidate();
            }
        }

        private Size size;

        private int digits = 6;
        private bool leadzero = true;
        private int numBase = 8;

        private float singleDigitWid;

        public NixieNumeric()
        {
            InitializeComponent();
            calcGeom();
        }

        private void calcGeom()
        {
            float wid = Nixies.ImageSize.Width*digits;

            if (Width <= 0) Width = (int) wid;

            var haspect = wid/Width;

            singleDigitWid = Nixies.ImageSize.Width*haspect;

            Height = (int) (Nixies.ImageSize.Height/haspect);

            SetBounds(0, 0, Width, Height, BoundsSpecified.All);

            geomDirty = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if (geomDirty)
                calcGeom();

            var g = pe.Graphics;

            var sum = Value;

            var digs = new int[digits];

            for (var i = 0; i < digits; i++)
            {
                digs[digits - i - 1] = (int) sum%numBase;
                sum /= (ulong) numBase;
            }

            var leadSeen = leadzero;

            float x = 0;
            for (var i = 0; i < digits; i++, x += singleDigitWid)
            {
                if (digs[i] != 0)
                    leadSeen = true;
                if (!leadSeen) continue; // Skip leading digits

                Nixies.Draw(g, (int) x, 0, (int) singleDigitWid, Height, digs[i]);
            }
        }
    }
}