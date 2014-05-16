using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using ScreenScanner;

namespace screenColorTest
{
    public partial class Form1 : Form
    {
        private Scanner scanner = new Scanner();
        private bool serialIsReady = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (scanner.Enabled == false)
            {
                scanner.Start();
                timer1.Enabled = true;
            }
            else
            {
                scanner.Stop();
                timer1.Enabled = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            textBox1.Text = Convert.ToString(scanner.Output.R);
            textBox2.Text = Convert.ToString(scanner.Output.G);
            textBox3.Text = Convert.ToString(scanner.Output.B);

            textBox4.Text = textBox1.Text + "," + textBox2.Text + "," + textBox3.Text + "\n";

            BackColor = scanner.Output;
        }

    }
}
