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

        private void pictureBox_yuno_Click(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false;
        }

        private void radio_shopitems_1safe_CheckedChanged(object sender, EventArgs e)
        {
            //radio button control vars, update later or you know what, fix it right now, try just using .Checked
            //worked, old method ints for each radio, bools and .Checked works fine
            pictureBox_yuno.Visible = false; status = "shop";
        }
        private void radio_shopitems_2max_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false; status = "shop";
        }
        private void radio_shopitems_3rand_CheckedChanged(object sender, EventArgs e)
        {
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
            bool si1 = radio_shopitems_1safe.Checked;
            bool si2 = radio_shopitems_2max.Checked;
            bool si3 = radio_shopitems_3rand.Checked;

            bool sy = radio_synthesis.Checked;
            bool de = radio_defaultequipment.Checked;
            bool bs = radio_basestats.Checked;
            bool ag = radio_abilitygems.Checked;
            bool rl = radio_levels.Checked;
            bool radio = si1 || si2 || si3 || sy || de || bs || ag || rl;

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
                + "\nrl " + rl.ToString()
                + "\n\nbaddies\n" + baddies.ToString()
                + "\n\nseed string\n" + seed.ToString()
                + "\n\n" + strmeth
                + "\n\nseed int\n" + seed2.ToString()
                + "\nseeded random (1-235)\n"
                + seed3.ToString();
        }

        public void SeedRNG()
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
            
            
            //init shop items arrie
            int[] a_shopItems_1safe = new int[] { 16, 16, 9, 14, 25, 18, 28, 13, 14, 32, 14, 32, 29, 21, 22, 25, 21, 30, 21, 30, 6, 12, 20 };
            int[] a_shopItems_2max = new int[] { 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31 };
            int[] a_shopItems_3rand = new int[31];





            //default
            int[] a_shopItems = a_shopItems_1safe;

            //intial notes

            //items.csv lines 7-261 (item numbers 000(hammer),001-254) 001-235 =234 normal items
            //item number 236+ is potions etc 250 dark matter, 251,252 gysahl,pepper

            //shop items si

            //ShopItems.csv lines 7-38, 32 shops total, 0-22 are suggested editable 23 total stores
            //shops 0023,0024 unused, 25+ are potions etc, best left alone so you can always buy potions etc. maybe random later for nightmare mode or something
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
            // for now, lets output replacement lines to the output window. later, edit a file directly using an open dialog or file path to detect all the files.
            //perhaps they don't have this file because they havent run the randomizer?
            //Shop.  000x.    same x.   items, 236+ do NOT count.     -1, to indicate end.  lots of delimiters, i think they indicate the max number of items possible  #comment shop number
            //shop 0015 has max, 32 items then -1, shop 0000 has 16, -1 then 16 spaces, total 33, shop 0008 has 14, -1, 18, total 33.
            //shops can hold max of 32 items, end with -1, must have ; for each empty space not used. coult randomize the ammount of items in each shop as well
            //throughout all this i dont think its possible to guarantee all items appear, at least not at this point. this is raw random crazyness
            //format
            //"Shop " + 000x + ";" + x + ";" + _items;_ + "-1;" + _empty_semi_colons_to_32-items;_ + "# Shop " + 000x
            //                 1;        2;         3-35; 
            //can be 32 items then a -1 to end. total 35 semi colons per line, last cell = "# "+firstcell, second cell = no extra 0 shop number
            //cells UInt8 if that helps. 32x36 matrix could scoop all this data
            // I think we have all the data we need to write a method to generate new shop data.
            // there are lots of repeats in the shop data, more precise generation way way later maybe
            //condensed notes: 23 shop datasets:: number of items,potions, then -1, suggest 1:1 copy of orignal potion+ items, only editing the normal items.

            //n = 16,0 16,0 9,7 14,7 25,0 18,0 28,0 13,9 14,0 32,0 14,9 32,0 29,0 21,11 22,9 25,7 21,11 30,0 21,10 30,0 6,0 12,12 20,10
            /*2d array attempt, I dont think the 2nd demension data is needed for this
             * int[,] a_shopItems = new int[,]
            { 
                { 16, 0 }, { 16, 0 }, { 9, 7 }, { 14, 7 }, { 25, 0 }, 
                { 18, 0 }, { 28, 0 }, { 13, 9 }, { 14, 0 }, { 32, 0 }, 
                { 14, 9 }, { 32, 0 }, { 29, 0 }, { 21, 11 }, { 22, 9 }, 
                { 25, 7 }, { 21, 11 }, { 30, 0 }, { 21, 10 }, { 30, 0 }, 
                { 6, 0 }, { 12, 12 }, { 20, 10 } 
            };*/

            //need to generate random ints followed by semi colons, potions+ based on shop number maybe
            //n number array of x,y dataset, nested loop cycling through each n, loop generating X number of randoms
            //checks for 1st, 2nd, Y cells, -1 in between X and Y, last cells 
            //Final: nested double loop one for n array, one for X
            //just realized the Y data is only to specify how many fields we need to copy over from the potions+ section for each store
            //if we are going to copy a file and make changes, we dont need to copy sets of potions+ we just need to not edit that part of the file
            //first steps were to output to window then file, maybe skip to file
            /*
            for (int i = 0; i < a_shopItems.GetLength(0); i++)
            {
                for (int j = 1; j < a_shopItems[i]; j++)
                {
                    Random rnd = new Random(seed2);
                    a_shopItems_rng[i][j] = rnd.Next(1, 235);

                    //richTextBox_output.Text = "array length=" + i + "\n " + a_shopItems[j,0];
                }
                Console.WriteLine("Value of i: {0}", i);
            }*/

            //hang on i just read up on jagged multi-dimensional arrays and i just lost all respect for arrays
            //i suppose i could create a 1d array for each possible shop to edit, but i would have to adjust the loop for each

            //stock amount of items in each shop, can randomize later if wanted
            if (radio_shopitems_1safe.Checked)
            {
                a_shopItems = a_shopItems_1safe;
            }
            if (radio_shopitems_2max.Checked)
            {
                a_shopItems = a_shopItems_2max;
            }
            if (radio_shopitems_3rand.Checked)
            {
                a_shopItems = a_shopItems_3rand;
            }

            //add if gui select which type of shop randomness, check and apply selected array data to 
            
            //a_shopItems is gonna be the var array we will use

            //new items jagged array, or manual set of arrays. still reading.
            //attempting jagged empty for easy loop access and data read/write
            //remove 23 shop limit ->32 and allow for any inputarray to adjust storage size of each subarray
            int[][] a_shopItems_rng =
            {
                new int[a_shopItems[0]],new int[a_shopItems[1]],new int[a_shopItems[2]],new int[a_shopItems[3]],new int[a_shopItems[4]],new int[a_shopItems[5]],
                new int[a_shopItems[6]],new int[a_shopItems[7]],new int[a_shopItems[8]],new int[a_shopItems[9]],new int[a_shopItems[10]],new int[a_shopItems[11]],
                new int[a_shopItems[12]],new int[a_shopItems[13]],new int[a_shopItems[14]],new int[a_shopItems[15]],new int[a_shopItems[16]],new int[a_shopItems[17]],
                new int[a_shopItems[18]],new int[a_shopItems[19]],new int[a_shopItems[20]],new int[a_shopItems[21]],new int[a_shopItems[22]]
            };
        }


    }

}
