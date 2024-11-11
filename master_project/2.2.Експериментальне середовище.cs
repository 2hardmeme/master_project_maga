using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace master_project
{
    public partial class Form7 : Form
    {
        private double period;
        private string[] yDots;
        private string[] xDots;
        public Form7(double period, string[] yDots, string[] xDots)
        {
            InitializeComponent();
            this.period = period;
            this.yDots = yDots;
            this.xDots = xDots;
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
