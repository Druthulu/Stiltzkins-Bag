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
        
        private int ss;
        private int sy;
        private int de;
        private int bs;
        private int ag;
        private int rl;
        private string strmeth;
        private string seed = "42";
        private int temp;
        private int seed2;
        private int seed3;
        private int baddies;


        private void Form1_Load(object sender, EventArgs e)
        {
            textBox_seed.Text = seed; 
        }

        private void radio_shopitems_CheckedChanged(object sender, EventArgs e)
        {
            ss = 1;sy = 0;de = 0;bs = 0;ag = 0;rl = 0;pictureBox_yuno.Visible = false;

        }
        private void radio_synthesis_CheckedChanged(object sender, EventArgs e)
        {
            ss = 0; sy = 1; de = 0; bs = 0; ag = 0; rl = 0;pictureBox_yuno.Visible = false;
        }

        private void radio_defaultequipment_CheckedChanged(object sender, EventArgs e)
        {
            ss = 0; sy = 0; de = 1; bs = 0; ag = 0; rl = 0;pictureBox_yuno.Visible = false;
        }

        private void radio_basestats_CheckedChanged(object sender, EventArgs e)
        {
            ss = 0; sy = 0; de = 0; bs = 1; ag = 0; rl = 0;pictureBox_yuno.Visible = false;
        }

        private void radio_abilitygems_CheckedChanged(object sender, EventArgs e)
        {
            ss = 0; sy = 0; de = 0; bs = 0; ag = 1; rl = 0;pictureBox_yuno.Visible = false;
        }

        private void radio_levels_CheckedChanged(object sender, EventArgs e)
        {
            ss = 0; sy = 0; de = 0; bs = 0; ag = 0; rl = 1;pictureBox_yuno.Visible = false;
        }

        private void button_rand_Click(object sender, EventArgs e)
        {
            if (ss + sy + de + bs + ag + rl == 0){pictureBox_yuno.Visible = true;}
            seed = textBox_seed.Text;
            Testmeth();
            //richTextBox_output.Text = textBox_seed.Text;
            richTextBox_debug.Text = "goodies:"
                + "\n" + ss.ToString()
                + "\n" + sy.ToString()
                + "\n" + de.ToString()
                + "\n" + bs.ToString()
                + "\n" + ag.ToString()
                + "\n" + rl.ToString()
                + "\n" + "baddies\n" + baddies.ToString()
                + "\n" + "seed string\n" + seed.ToString() 
                + "\n\n" + strmeth + "\n" 
                + "seed int\n" + seed2.ToString()
                + "\n" + "seeded random (1-100)\n"
                + temp.ToString();

        }

        private void Testmeth()
        {
            bool test = int.TryParse(seed, out int test2);
            if (test) { 
                seed2 = test2; 
            }
            else { 
                pictureBox_yuno.Visible = true;
                return;
            }
            //seed2 = int.TryParse(seed);
            Random rnd = new Random(seed2);
            temp = rnd.Next(1, 100);
            strmeth = "//parse input seed string to int//";

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false;
        }
    }

}
