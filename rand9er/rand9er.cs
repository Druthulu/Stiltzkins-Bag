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

        string input_str, output_str, swap, strmeth, seed2, seed6, seed7, seed = "42", status = "init";
        int seed4, baddies, counter, incount;
        int[] seed3, seed5;


        private void Form1_Load(object sender, EventArgs e)
        {
            //assign default seed on load, maybe random later
            textBox_seed.Text = seed; 
        }
        private void clear_Click(object sender, EventArgs e)
        {
            richTextBox_debug.Text = "";
            richTextBox_output.Text = "";
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
        private void richTextBox_debug_TextChanged(object sender, EventArgs e)
        {
            richTextBox_debug.SelectionStart = richTextBox_debug.Text.Length;
            richTextBox_debug.ScrollToCaret();
        }
        //Shop Items TAB

        int randl = 23, items = 235;

        int[] a_shopItems = new int[] { 16, 16, 9, 14, 25, 18, 28, 13, 14, 32, 14, 32, 29, 21, 22, 25, 21, 30, 21, 30, 6, 12, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //default
        int[] a_shopItems_1safe = new int[] { 16, 16, 9, 14, 25, 18, 28, 13, 14, 32, 14, 32, 29, 21, 22, 25, 21, 30, 21, 30, 6, 12, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //filled zeros so no exceptions
        int[] a_s_safe2 = new int[] { 0, 0, 7, 7, 0, 0, 0, 9, 0, 0, 9, 0, 0, 11, 9, 7, 11, 0, 10, 0, 0, 12, 10, 0, 0, 5, 7, 8, 10, 11, 12, 12 }; //medic items for safe list. not sure if needed later
        int[] a_shopItems_2max = new int[] { 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] a_shopItems_2maxm = new int[] { 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31 };
        int[] a_shopItems_3rand = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };


        private void radio_shopitems_1safe_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false; status = "shop";
            c_medicitems.Checked = false;
            c_medicshops.Checked = false;
            if (radio_shopitems_1safe.Checked) { a_shopItems = a_shopItems_1safe; }
        }
        private void radio_shopitems_2max_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false; status = "shop";
            if (radio_shopitems_2max.Checked) { c_medicitems.Checked = true; c_medicshops.Checked = true; a_shopItems = a_shopItems_2maxm; }
        }
        private void radio_shopitems_3rand_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false; status = "shop";
            if (radio_shopitems_3rand.Checked) { a_shopItems = a_shopItems_3rand; }
        }
        private void c_medicitems_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_shopitems_1safe.Checked & c_medicitems.Checked)
            {
                radio_shopitems_3rand.Checked = true;
                c_medicitems.Checked = true;
            }
            if (c_medicitems.Checked) { items = 250; }
            if (!c_medicitems.Checked) { items = 235; }

        }
        private void c_medicshops_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_shopitems_1safe.Checked & c_medicshops.Checked)
            {
                radio_shopitems_3rand.Checked = true;
                c_medicshops.Checked = true;
            }
            if (c_medicshops.Checked)
            {
                if (radio_shopitems_3rand.Checked) { randl = 32; }
                if (radio_shopitems_2max.Checked) { a_shopItems = a_shopItems_2maxm; }
            }
            if (!c_medicshops.Checked)
            {
                if (radio_shopitems_3rand.Checked) { randl = 23; }
                if (radio_shopitems_2max.Checked) { a_shopItems = a_shopItems_2max; }
            }
        }


        public string SeedRNG(string swap, int counter) //string swap to input
        {
            strmeth = "//convert and compress seed//";
            input_str = swap;
            incount = counter;
            string mid_str = "", 
            output_str = "";
            int[] mid1_arr = new int[input_str.Length];
            int[] mid2_arr = new int[input_str.Length];
            int mid2c = 0;
            //richTextBox_debug.Text = "SeedRNG()";
            //test
            richTextBox_debug.Text = richTextBox_debug.Text + "\nSeedRNG(swap= " + swap + " counter= " + counter + ")";
            for (int i = 0; i < input_str.Length; i++)
            {
                progressBar1.Value = i;
                if ((i > 0) & ((i % 2) == 0))
                {
                    if (((int)input_str[i]) == ((int)input_str[i - 1])) 
                    {
                        Random rnd = new Random((int)input_str[i]);
                        mid_str = mid_str + rnd.Next(33, 255);
                    } else
                    {
                        mid_str = mid_str + (int)input_str[i];
                    }
                } else
                {
                    mid_str = mid_str + (int)input_str[i];
                }

                //test
                richTextBox_debug.Text = richTextBox_debug.Text + "\n" + counter + "\n(int)input_str[i= " + i + "]= " + input_str[i] + " " + (int)input_str[i] + "   mid_str = " + mid_str ;
                if (int.TryParse(mid_str, out int test2))
                {
                    mid1_arr[i] = test2;
                    //test
                    richTextBox_debug.Text = richTextBox_debug.Text + "\n" + counter + "\nTryParse success mid1_arr= " + mid1_arr[i];
                }
                else
                {
                    mid_str = (int)input_str[i] + "";
                    //test
                    richTextBox_debug.Text = richTextBox_debug.Text + "\n" + counter + "\nTryParse failure restart mid_str= " + mid_str;
                    mid1_arr[i] = 0;
                    mid2_arr[mid2c] = mid1_arr[i - 1];
                    //test
                    richTextBox_debug.Text = richTextBox_debug.Text + "\n" + counter + "\nmid2_arr[mid2c= " + mid2c + "] = mid1_arr[i - 1]= " + mid1_arr[i - 1];
                    mid2c++;
                }
                if (i == input_str.Length-1)
                {
                    mid2_arr[mid2c] = mid1_arr[i];
                    //test
                    richTextBox_debug.Text = richTextBox_debug.Text + "\n" + counter + "\n last one= mid1_arr[i= " + i + "]=" + mid1_arr[i];
                }
            }
            progressBar2.Maximum = mid2_arr.Length;
            for (int i = 0; i < mid2_arr.Length; i++)
            {
                progressBar2.Value = i;
                if (mid2_arr[i] > 0)
                {
                    Random rnd = new Random(mid2_arr[i]);
                    output_str = output_str + rnd.Next(42, 420);
                    //test
                    richTextBox_debug.Text = richTextBox_debug.Text + "\n" + counter + "\nmid2_arr[i= " + i + "]=" + mid2_arr[i] + " --rng-- output_str= " + output_str;
                }
            }
            return output_str;
        }

        private void ShopItems()
        {
            if (radio_shopitems_3rand.Checked)
            {

                for (int i = 0; i < randl; i++)
                {
                    Random rnd = new Random(i ^ seed4);
                    a_shopItems[i] = rnd.Next(1, 31);
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

            //richTextBox_output.Text = "";
            for (int i = 0; i < a_shopItems_jag.Length; i++)
            {
                //richTextBox_output.Text = richTextBox_output.Text + "\n i: " + i + " ";
                richTextBox_output.Text = richTextBox_output.Text + "\n;";
                for (int j = 0; j < a_shopItems_jag[i].Length; j++)
                {
                    Random rnd = new Random(seed4 * i + seed4 ^ j);
                    int rnd2 = rnd.Next(1, items);
                    if (rnd2 == 250) { rnd2 = 253; }
                    a_shopItems_jag[i][j] = rnd2;
                    richTextBox_output.Text = richTextBox_output.Text + /*"j: " + j + " :r: "*/ a_shopItems_jag[i][j] + "; ";
                }
            }


        }


        public void button_rand_Click(object sender, EventArgs e)
        {
            //input seed
            
            seed = textBox_seed.Text;
            int seedl = seed.Length;
            counter = 1;
            /*if (seed.Length < 10)
            {
                richTextBox_output.Text = "Need more seed data";
                return;
            }*/

            //fun error message
            if (!(radio_shopitems_1safe.Checked || radio_shopitems_2max.Checked ||
            radio_shopitems_3rand.Checked || radio_synthesis.Checked ||
            radio_defaultequipment.Checked || radio_basestats.Checked ||
            radio_abilitygems.Checked)) 
            { pictureBox_yuno.Visible = true; }

            //methods

            //need to incorporate the progress bar, need to parse the length of seed to determine work, then prolly just use progress bar / length of seed current possition
            //two progress bars?


            progressBar1.Maximum = seedl;
            
            //parse input seed, sanatize, convert to usable data
            swap = seed;
            output_str = SeedRNG(swap, counter);
            swap = output_str;
            //if seed is huge =1000 chars, verified possible output rng. ! 15 cycles for 1000char seed
            //1000char seed -> ints grouped into array of 9 digits per element, 1039 elements
            //random using each element as seed, reducing 9 to 2 or 3.
            //continue to cycle untill under int32.maxvalue. i round to 9 digits or less for simplicity.
            //super sweet, but i dont like that most of the elements are leading with 4 or 5s. it implies a pattern.
            // need to do some research and check back
            //
            
            if (output_str.Length > 9)
            {
                counter++;
                richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n" + counter + "..\n";
                output_str = SeedRNG(swap, counter);
                swap = output_str;
                if (output_str.Length > 9)
                {
                    counter++;
                    richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                    output_str = SeedRNG(swap, counter);
                    swap = output_str;
                    if (output_str.Length > 9)
                    {
                        counter++;
                        richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                        output_str = SeedRNG(swap, counter);
                        swap = output_str;
                        if (output_str.Length > 9)
                        {
                            counter++;
                            richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                            output_str = SeedRNG(swap, counter);
                            swap = output_str;
                            if (output_str.Length > 9)
                            {
                                counter++;
                                richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                output_str = SeedRNG(swap, counter);
                                swap = output_str;
                                if (output_str.Length > 9)
                                {
                                    counter++;
                                    richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                    output_str = SeedRNG(swap, counter);
                                    swap = output_str;
                                    if (output_str.Length > 9)
                                    {
                                        counter++;
                                        richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                        output_str = SeedRNG(swap, counter);
                                        swap = output_str;
                                        if (output_str.Length > 9)
                                        {
                                            counter++;
                                            richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                            output_str = SeedRNG(swap, counter);
                                            swap = output_str;
                                            if (output_str.Length > 9)
                                            {
                                                counter++;
                                                richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                                output_str = SeedRNG(swap, counter);
                                                swap = output_str;
                                                if (output_str.Length > 9)
                                                {
                                                    counter++;
                                                    richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                                    output_str = SeedRNG(swap, counter);
                                                    swap = output_str;
                                                    if (output_str.Length > 9)
                                                    {
                                                        counter++;
                                                        richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n" + counter + "..\n";
                                                        output_str = SeedRNG(swap, counter);
                                                        swap = output_str;
                                                        if (output_str.Length > 9)
                                                        {
                                                            counter++;
                                                            richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                                            output_str = SeedRNG(swap, counter);
                                                            swap = output_str;
                                                            if (output_str.Length > 9)
                                                            {
                                                                counter++;
                                                                richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                                                output_str = SeedRNG(swap, counter);
                                                                swap = output_str;
                                                                if (output_str.Length > 9)
                                                                {
                                                                    counter++;
                                                                    richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                                                    output_str = SeedRNG(swap, counter);
                                                                    swap = output_str;
                                                                    if (output_str.Length > 9)
                                                                    {
                                                                        counter++;
                                                                        richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                                                        output_str = SeedRNG(swap, counter);
                                                                        swap = output_str;
                                                                        if (output_str.Length > 9)
                                                                        {
                                                                            counter++;
                                                                            richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                                                            output_str = SeedRNG(swap, counter);
                                                                            swap = output_str;
                                                                            if (output_str.Length > 9)
                                                                            {
                                                                                counter++;
                                                                                richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                                                                output_str = SeedRNG(swap, counter);
                                                                                swap = output_str;
                                                                                if (output_str.Length > 9)
                                                                                {
                                                                                    counter++;
                                                                                    richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                                                                    output_str = SeedRNG(swap, counter);
                                                                                    swap = output_str;
                                                                                    if (output_str.Length > 9)
                                                                                    {
                                                                                        counter++;
                                                                                        richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                                                                        output_str = SeedRNG(swap, counter);
                                                                                        swap = output_str;
                                                                                        if (output_str.Length > 9)
                                                                                        {
                                                                                            counter++;
                                                                                            richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                                                                            output_str = SeedRNG(swap, counter);
                                                                                            swap = output_str;
                                                                                            if (output_str.Length > 9)
                                                                                            {
                                                                                                counter++;
                                                                                                richTextBox_debug.Text = richTextBox_debug.Text + "\n..\n.. Overflow, compressing again ..\n.." + counter + "\n";
                                                                                                output_str = SeedRNG(swap, counter);
                                                                                                swap = output_str;
                                                                                                if (output_str.Length > 9)
                                                                                                {
                                                                                                    pictureBox_yuno.Visible = true;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            if (int.TryParse(output_str, out int test2))
            {
                seed4 = test2;
            }
            else
            {
                //if something goes wrong, reset seed and continue
                seed4 = 0;
            }

            if (radio_shopitems_1safe.Checked || radio_shopitems_2max.Checked || radio_shopitems_3rand.Checked)
            {
                ShopItems();
            }


            //debug output
            richTextBox_debug.Text = richTextBox_debug.Text
                + "\nrandl: " + randl.ToString()
                + "\nbaddies\n" + baddies.ToString()
                + "\ninput seed string\n" + seed.ToString()
                + "\n" + strmeth
                + "\n" + seed4
                + "\nseeded random (1-" + items + ")\n"
                ;

        }


        private void Synthesis()
        {

        }
    }


}



/*     stock shop items   
    radio, safe, random, max, check, medic items, medic shops
    medic items are 236-249, 253      250 is dark matter, 251, 252 are gysahl greens and dead pepper, 253 is tent, 254 is ore
    besides tent 250+ cant be bought ever.
            

//items.csv lines 7-261 (item numbers 000(hammer),001-254) 001-235 =234 normal items
//item number 236+ is potions etc 250 dark matter, 251,252 gysahl,pepper
//default shop item amounts inserted into first cell shop number xx00, potions+ in last cell shop number, shops 25+ later maybe

*medic items 0,0,7,7,0,0,0,9,0,0,9,0,0,11,9,7,11,0,10,0,0,12,10,0,0,5,7,8,10,11,12,12
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
         //not used until later when building safe file
        int[] a_s_medicshops = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
 */