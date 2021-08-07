using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

///RichTextBox richTextBox_output;
///Button button_rand;
///RadioButton radio_shopitems;
///RadioButton radio_synthesis;
///RadioButton radio_defaultequipment;
///RadioButton radio_basestats;
///RadioButton radio_abilitygems;
///RadioButton radio_levels;
///Label label_item;
///Label label_char;
///Label label_output;
///Label label_seed;
///TextBox textBox_seed;

namespace rand9er
{
    public partial class rand9er : Form
    {
        public rand9er()
        {
            InitializeComponent();
        }
        
        private int ss = 0;
        private int sy = 0;
        private int de = 0;
        private int bs = 0;
        private int ag = 0;
        private int rl = 0;



        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void radio_shopitems_CheckedChanged(object sender, EventArgs e)
        {
            ss = 1;sy = 0;de = 0;bs = 0;ag = 0;rl = 0;

        }
        private void radio_synthesis_CheckedChanged(object sender, EventArgs e)
        {
            ss = 0; sy = 1; de = 0; bs = 0; ag = 0; rl = 0;
        }

        private void radio_defaultequipment_CheckedChanged(object sender, EventArgs e)
        {
            ss = 0; sy = 0; de = 1; bs = 0; ag = 0; rl = 0;
        }

        private void radio_basestats_CheckedChanged(object sender, EventArgs e)
        {
            ss = 0; sy = 0; de = 0; bs = 1; ag = 0; rl = 0;
        }

        private void radio_abilitygems_CheckedChanged(object sender, EventArgs e)
        {
            ss = 0; sy = 0; de = 0; bs = 0; ag = 1; rl = 0;
        }

        private void radio_levels_CheckedChanged(object sender, EventArgs e)
        {
            ss = 0; sy = 0; de = 0; bs = 0; ag = 0; rl = 1;
        }

        private void button_rand_Click(object sender, EventArgs e)
        {
            //string seed = textBox_seed.Text;
            //richTextBox_output.Text = textBox_seed.Text;
            richTextBox_debug.Text = ss.ToString()
                + "\n" + sy.ToString()
                + "\n" + de.ToString()
                + "\n" + bs.ToString()
                + "\n" + ag.ToString()
                + "\n" + rl.ToString();
        }

    }

}
