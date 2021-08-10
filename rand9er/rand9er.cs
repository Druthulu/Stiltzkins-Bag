using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rand9er
{
    public partial class rand9er : Form
    {
        public rand9er()
        {
            InitializeComponent();
        }
        //init
        string strmeth, seed = "42", status = "init";
        int seed2, seed3, baddies;
        



        private void Form1_Load(object sender, EventArgs e)
        {
            //assign default seed on load, maybe random later
            textBox_seed.Text = seed; 
        }
        private void pictureBox_yuno_Click(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false;
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

        //Shop Items TAB

        int shops, shopr, shopm, mi, ms;

        int[] a_shopItems = new int[] { 16, 16, 9, 14, 25, 18, 28, 13, 14, 32, 14, 32, 29, 21, 22, 25, 21, 30, 21, 30, 6, 12, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //default
        int[] a_shopItems_1safe = new int[] { 16, 16, 9, 14, 25, 18, 28, 13, 14, 32, 14, 32, 29, 21, 22, 25, 21, 30, 21, 30, 6, 12, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //filled zeros so no exceptions
        int[] a_shopItems_2max = new int[] { 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31 };
        int[] a_shopItems_3rand = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private void radio_shopitems_1safe_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false; status = "shop";
            c_medicitems.Checked = false;
            c_medicshops.Checked = false;
            if (radio_shopitems_1safe.Checked) { shops = 1; a_shopItems = a_shopItems_1safe; } else { shops = 0; }
        }
        private void radio_shopitems_2max_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false; status = "shop";
            if (radio_shopitems_2max.Checked) { c_medicitems.Checked = true; c_medicshops.Checked = true; shopm = 1; a_shopItems = a_shopItems_2max; } else { shopm = 0; }
        }
        private void radio_shopitems_3rand_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false; status = "shop";
            if (radio_shopitems_3rand.Checked) { shopr = 1; a_shopItems = a_shopItems_3rand; } else { shopr = 0; }
        }
        private void c_medicitems_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_shopitems_1safe.Checked & c_medicitems.Checked)
            {
                radio_shopitems_3rand.Checked = true;
                c_medicitems.Checked = true;
            }
            if (c_medicitems.Checked) { mi = 1; } else { mi = 0; }
        }
        private void c_medicshops_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_shopitems_1safe.Checked & c_medicshops.Checked)
            {
                radio_shopitems_3rand.Checked = true;
                c_medicshops.Checked = true;
            }
            if (c_medicshops.Checked) { ms = 1; } else { ms = 0; }
        }

        //if safe use 23 shops list
        // if max use 32 shops list, maybe click meic items when click max


private void button_rand_Click(object sender, EventArgs e)
        {
            //input
            seed = textBox_seed.Text;

            //bools
            bool si1 = radio_shopitems_1safe.Checked;
            bool si2 = radio_shopitems_2max.Checked;
            bool si3 = radio_shopitems_3rand.Checked;

            bool sy = radio_synthesis.Checked;
            bool de = radio_defaultequipment.Checked;
            bool bs = radio_basestats.Checked;
            bool ag = radio_abilitygems.Checked;
            //bool rl = radio_levels.Checked;
            bool radio = si1 || si2 || si3 || sy || de || bs || ag;

            //fun error message
            if (!radio)
            {
                pictureBox_yuno.Visible = true;
            }

            //methods
            //parse input seed, sanatize, convert to usable data
            SeedRNG();
            
            //bools determine which method to run
            if (si1 || si2 || si3)
            {
                ShopItems();
            }


            //debug output
            richTextBox_debug.Text = "goodies:"
                + "\nsi1 " + si1.ToString()
                + "\nsi2 " + si2.ToString()
                + "\nsi3 " + si3.ToString()
                + "\nsy " + sy.ToString()
                + "\nde " + de.ToString()
                + "\nbs " + bs.ToString()
                + "\nag " + ag.ToString()
                + "\n\nbaddies\n" + baddies.ToString()
                + "\n\nseed string\n" + seed.ToString()
                + "\n\n" + strmeth
                + "\n\nseed int\n" + seed2.ToString()
                + "\nseeded random (1-235)\n"
                + seed3.ToString()
                + "\nrandom shop#test\n";
        }

        private void SeedRNG()
        {
            strmeth = "//convert input seed to usable data//";
            //sanatize input seed, failure optional, fix with char map in future
            
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
            //seed rng
            Random rnd = new Random(seed2);
            seed3 = rnd.Next(1, 235);
            
        }

        private void ShopItems()
        {

            if (radio_shopitems_3rand.Checked)
            {
                //richTextBox_output.Text = "a_shopItems_3rand 1D This tests 1D array of seeded random generation\n";
                for (int i = 0; i < a_shopItems_3rand.GetLength(0); i++)
                {
                    Random rnd = new Random(i^seed2);

                    a_shopItems_3rand[i] = rnd.Next(1, 31);

                }
            }
            int[][] a_shopItems_jag =
            {
                new int[a_shopItems[0]],new int[a_shopItems[1]],new int[a_shopItems[2]],new int[a_shopItems[3]],new int[a_shopItems[4]],new int[a_shopItems[5]],
                new int[a_shopItems[6]],new int[a_shopItems[7]],new int[a_shopItems[8]],new int[a_shopItems[9]],new int[a_shopItems[10]],new int[a_shopItems[11]],
                new int[a_shopItems[12]],new int[a_shopItems[13]],new int[a_shopItems[14]],new int[a_shopItems[15]],new int[a_shopItems[16]],new int[a_shopItems[17]],
                new int[a_shopItems[18]],new int[a_shopItems[19]],new int[a_shopItems[20]],new int[a_shopItems[21]],new int[a_shopItems[22]],new int[a_shopItems[23]],
                new int[a_shopItems[24]],new int[a_shopItems[25]],new int[a_shopItems[26]],new int[a_shopItems[27]],new int[a_shopItems[28]],new int[a_shopItems[29]],
                new int[a_shopItems[30]],new int[a_shopItems[31]]
            };

            for (int i = 0; i < a_shopItems_jag.Length; i++)
            {
                //richTextBox_output.Text = richTextBox_output.Text + "\n i: " + i + " ";
                richTextBox_output.Text = richTextBox_output.Text + "\n;";
                for (int j = 0; j < a_shopItems_jag[i].Length; j++)
                {
                    Random rnd = new Random(seed2 * i + seed2 ^ j);
                    a_shopItems_jag[i][j] = rnd.Next(1, 235);
                    richTextBox_output.Text = richTextBox_output.Text + /*"j: " + j + " :r: "*/ a_shopItems_jag[i][j] + "; ";
                }
            }

            /* radio, safe, random, max, check, medic items, medic shops
            medic items are 236-249, 253      250 is dark matter, 251, 252 are gysahl greens and dead pepper, 253 is tent, 254 is ore
            besides tent 250+ cant be bought ever.
            if medic items checked extend to 1,250, check if 250 write 253 instead for tent.
            if medic shops checked
            if max u */



            //items.csv lines 7-261 (item numbers 000(hammer),001-254) 001-235 =234 normal items
            //item number 236+ is potions etc 250 dark matter, 251,252 gysahl,pepper
            //default shop item amounts inserted into first cell shop number xx00, potions+ in last cell shop number, shops 25+ later maybe
            /*     stock shop items      
             Shop 1600;0;1;2;16;17;57;70;88;89;102;112;114;136;137;149;150;177;-1;;;;;;;;;;;;;;;;;# Shop 0000
             Shop 1601;1;1;2;3;17;31;57;79;89;90;102;103;115;116;138;151;178;-1;;;;;;;;;;;;;;;;;# Shop 0001
             Shop 0902;2;80;90;104;115;116;117;139;152;178;236;240;241;242;243;244;253;-1;;;;;;;;;;;;;;;;;# Shop 0702
             Shop 1403;3;51;58;90;103;104;115;116;117;136;138;139;152;178;236;240;241;242;243;244;253;-1;;;;;;;;;;;;;# Shop 0703
             Shop 2504;4;1;2;3;18;32;51;58;71;72;73;79;80;89;90;91;104;117;118;139;150;151;152;153;178;179;-1;;;;;;;;# Shop 0004
             Shop 1805;5;33;52;58;71;72;80;91;92;104;105;117;118;119;140;153;154;179;180;-1;;;;;;;;;;;;;;;# Shop 0005
             Shop 2806;6;1;2;3;20;33;52;59;71;72;73;89;90;91;92;104;105;116;117;118;119;140;150;151;152;153;154;179;180;-1;;;;;# Shop 0006
             Shop 1307;7;20;33;52;59;92;105;118;119;140;153;154;155;180;236;240;241;242;243;244;247;248;253;-1;;;;;;;;;;;# Shop 0907
             Shop 1408;8;42;52;59;71;72;73;74;81;92;93;120;121;155;156;-1;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;# Shop 0008
             Shop 3209;9;2;3;4;59;70;71;72;73;74;81;89;90;91;92;93;112;114;115;116;117;118;119;120;121;150;151;152;153;154;155;156;157;-1;# Shop 0009
             Shop 1410;10;1;2;3;4;42;52;64;85;93;120;121;155;156;157;236;240;241;242;243;244;245;248;253;-1;;;;;;;;;;# Shop 0910
             Shop 3211;11;3;4;20;33;34;41;42;59;60;65;71;72;73;74;85;90;91;92;93;104;105;120;121;122;139;140;141;156;157;158;180;181;-1;# Shop 0011
             Shop 2912;12;1;2;3;4;20;21;33;34;41;42;52;59;60;65;74;93;104;105;120;121;122;140;141;156;157;158;178;180;181;-1;;;;# Shop 0012
             Shop 2113;13;21;33;34;42;53;60;65;75;81;85;94;105;122;123;124;141;156;157;158;180;181;236;237;240;241;242;243;244;245;247;248;253;-1;# Shop 1113
             Shop 2214;14;22;35;43;53;60;66;75;82;85;94;95;105;106;123;124;125;141;142;158;159;181;182;237;240;242;243;244;245;247;248;253;-1;;# Shop 0914
             Shop 2515;15;4;5;22;23;36;44;53;61;67;75;76;82;86;95;96;97;106;107;126;127;142;159;160;182;183;237;240;245;246;247;248;253;-1;# Shop 0715
             Shop 2116;16;2;17;32;42;52;58;65;74;80;86;96;97;107;127;128;142;143;160;161;183;184;236;237;240;241;242;243;244;245;246;248;253;-1;# Shop 1116
             Shop 3017;17;2;3;4;5;24;37;46;54;61;67;76;86;91;92;93;94;95;96;97;107;108;120;122;128;129;143;161;162;184;185;-1;;;# Shop 0017
             Shop 2118;18;1;2;3;4;5;6;25;38;47;54;83;86;99;108;129;130;131;144;162;163;186;237;240;241;242;243;244;246;247;248;253;-1;;# Shop 1018
             Shop 3019;19;6;25;27;38;47;48;54;79;83;86;93;95;99;100;108;110;119;120;128;129;130;131;144;145;162;163;164;186;187;188;-1;;;# Shop 0019
             Shop 0620;20;62;68;77;132;133;165;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0020
             Shop 1221;21;3;18;32;43;54;58;81;92;104;140;154;180;236;237;240;241;242;243;244;245;246;247;248;253;-1;;;;;;;;;# Shop 1221
             Shop 2022;22;22;35;43;53;60;66;75;82;85;86;94;95;106;123;124;125;142;158;159;182;236;237;240;241;242;243;245;247;248;253;-1;;;# Shop 1022
             Shop 0023;23;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0023
             Shop 0024;24;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0024
             Shop 0025;25;236;240;243;244;253;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0025
             Shop 0026;26;236;240;241;242;243;244;253;-1;;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0026
             Shop 0027;27;236;240;241;242;243;244;248;253;-1;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0027
             Shop 0028;28;236;237;240;241;242;243;244;247;248;253;-1;;;;;;;;;;;;;;;;;;;;;;;# Shop 0028
             Shop 0029;29;236;237;240;241;242;243;244;245;247;248;253;-1;;;;;;;;;;;;;;;;;;;;;;# Shop 0029
             Shop 0030;30;236;237;240;241;242;243;244;245;246;247;248;253;-1;;;;;;;;;;;;;;;;;;;;;# Shop 0030
             Shop 0031;31;236;237;240;241;242;243;244;245;246;247;248;253;-1;;;;;;;;;;;;;;;;;;;;;# Shop 0031
             */


        }

        private void Synthesis()
        {

        }
    }


}
