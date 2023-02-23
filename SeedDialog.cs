using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GOL_Project
{
    public partial class SeedDialog : Form
    {
        public SeedDialog()
        {
            InitializeComponent();          
        }

        //public seed
        public int ModalSeed
        {
            get
            {
                return (int)numericUpDown1.Value;
            }
            set
            {
                numericUpDown1.Value = value;
            }
        }

        //randomize button
        private void button3_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            numericUpDown1.Value = rand.Next();
        }
    }
}
