using System;
using System.Windows.Forms;

namespace PDP10GraphicalFrontEnd
{
    public partial class OuterDebuggerForm : Form
    {
        public OuterDebuggerForm()
        {
            InitializeComponent();

            Load += OuterDebuggerForm_Load;
        }

        private void OuterDebuggerForm_Load(object sender, EventArgs e)
        {
            debuggerScreen1.cpuSetup();
        }
    }
}