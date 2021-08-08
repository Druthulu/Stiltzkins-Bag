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
        
        //init
        private string strmeth, seed = "42", status = "init";
        private int seed2, seed3, baddies;

        private void Form1_Load(object sender, EventArgs e)
        {
            //assign default seed on load, maybe random later
            textBox_seed.Text = seed; 
        }

        private void radio_shopitems_CheckedChanged(object sender, EventArgs e)
        {
            //radio button control vars, update to array later or you know what, fix it right now, try just using .Checked
            // old method ints for each radio, no need .Checked during button click works fine
            pictureBox_yuno.Visible = false; status = "shop";
        }
        private void radio_synthesis_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false; status = "synth";
        }
        private void radio_defaultequipment_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false; status = "equip";
        }
        private void radio_basestats_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false; status = "stats";
        }
        private void radio_abilitygems_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false; status = "ability";
        }
        private void radio_levels_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false; status = "levels";
        }
        private void button_rand_Click(object sender, EventArgs e)
        {
            //input
            seed = textBox_seed.Text;

            //bools
            bool ss = radio_shopitems.Checked;
            bool sy = radio_synthesis.Checked;
            bool de = radio_defaultequipment.Checked;
            bool bs = radio_basestats.Checked;
            bool ag = radio_abilitygems.Checked;
            bool rl = radio_levels.Checked;
            bool radio = ss || sy || de || bs || ag || rl;

            //fun error message
            if (!radio)
            {
                pictureBox_yuno.Visible = true;
            }

            //methods
            SeedRNG();
            Mathrulz();


            //debug output
            richTextBox_debug.Text = "goodies:"
                + "\nss " + ss.ToString()
                + "\nsy " + sy.ToString()
                + "\nde " + de.ToString()
                + "\nbs " + bs.ToString()
                + "\nag " + ag.ToString()
                + "\nrl " + rl.ToString()
                + "\n\nbaddies\n" + baddies.ToString()
                + "\n\nseed string\n" + seed.ToString()
                + "\n\n" + strmeth
                + "\n\nseed int\n" + seed2.ToString()
                + "\nseeded random (1-255)\n"
                + seed3.ToString();
        }

        private void SeedRNG()
        {
            strmeth = "//convert input seed to usable data//";
            //clean input seed, failure optional, fix with char map in future
            bool test = int.TryParse(seed, out int test2);
            if (test)
            { 
                seed2 = test2; 
            }
            else
            { 
                pictureBox_yuno.Visible = true;
                return;
            }
            //
            Random rnd = new Random(seed2);
            seed3 = rnd.Next(1, 255);
            
        }

        private void Mathrulz()
        {
            //formatting for each option


        }


        private void pictureBox_yuno_Click(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false;
        }
    }

}
