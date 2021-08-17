using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace rand9er
{
    public partial class rand9er : Form
    {
        public rand9er()
        {
            InitializeComponent();
        }

        //init//
        string data_str, pswap, set, seed = "42", status = "init";
        int rngc, data_int, baddies, counter, randl = 23, items = 235;
        int[] data_arr, a_empty = { 0 };
        string[] medicShops, a_stockShopItems, a_comboSafe = new string[32];
        int[][] aa_medicItems, aa_shopItems;
        int[] a_shopItems = new int[] { 16, 16, 9, 13, 25, 18, 28, 13, 14, 32, 14, 32, 29, 21, 22, 25, 21, 30, 21, 30, 6, 12, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //default
        int[] a_shopItems_1safe = new int[] { 16, 16, 9, 13, 25, 18, 28, 13, 14, 32, 14, 32, 29, 21, 22, 25, 21, 30, 21, 30, 6, 12, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //filled zeros so no exceptions
        int[] a_shopItems_2max = new int[] { 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        int[] a_shopItems_2maxm = new int[] { 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32 };
        int[] a_shopItems_3rand = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        //FORM1//
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox_seed.Text = seed;
            pswap = path_search(pswap);
            tb_fl.Text = pswap;
            if (pswap.Length > 1) { richTextBox_debug.Text = richTextBox_debug.Text + "\nLocated FF9 install location\n"; }
        }
        private void richTextBox_debug_TextChanged(object sender, EventArgs e)
        {
            richTextBox_debug.SelectionStart = richTextBox_debug.Text.Length;
            richTextBox_debug.ScrollToCaret();
        }
        private void b_rseed_Click_1(object sender, EventArgs e)
        {
            Random rnd = new Random();
            textBox_seed.Text = rnd.Next(100000000, 2140000000).ToString();
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
        private void b_open_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.Description = "locate ff9, or seperate Data folder of CSVs";
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                tb_fl.Text = folderDlg.SelectedPath;
            }
        }
        private void b_restore_Click(object sender, EventArgs e)
        {
            StockShopItemsCSV();
        }
        private void b_rw_Click(object sender, EventArgs e)
        {
            ReadWrite();
        }
        private void b_search_Click(object sender, EventArgs e)
        {
            pswap = "";
            pswap = path_search(pswap);
            if (pswap.Length > 0)
            {
                tb_fl.Text = pswap;
            }
        }
        private void button_rand_Click(object sender, EventArgs e)
        {
            progressBar1.Value = progressBar1.Minimum;
            progressBar2.Value = progressBar2.Minimum;

            if (!(radio_shopitems_1safe.Checked || radio_shopitems_2max.Checked ||
            radio_shopitems_3rand.Checked || radio_synthesis.Checked ||
            radio_defaultequipment.Checked || radio_basestats.Checked ||
            radio_abilitygems.Checked)) { pictureBox_yuno.Visible = true; }


            //string seed has input data
            SeedIngest();
            //string data_str has output data
            Compressor();
            //string data_str has output data (under 10 digit string)
            CleanSeedInt();
            //int data_int has output data (under 10 digit int)


            if (radio_shopitems_1safe.Checked || radio_shopitems_2max.Checked || radio_shopitems_3rand.Checked)
            {
                ShopItems();
                MedicItems();
                ShopCombo();
                ShopOutput();
            }

            //debug output
            richTextBox_debug.Text = richTextBox_debug.Text
                + "\nrandl: " + randl.ToString()
                + "\nbaddies\n" + baddies.ToString()
                + "\ncompressed seed\n" + data_int
                + "\nitems: (1-" + items + ")\n"
                ;

            progressBar1.Value = progressBar1.Maximum;
            progressBar2.Value = progressBar2.Maximum;

        }

        //SHOP ITEMS TAB//
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
            if (radio_shopitems_1safe.Checked == true & c_medicitems.Checked == true)
            {
                radio_shopitems_3rand.Checked = true;
                c_medicitems.Checked = true;
            }
            if (c_medicitems.Checked == true) { items = 250; } else
            if (c_medicitems.Checked == false) { items = 235; }

        }
        private void c_medicshops_CheckedChanged(object sender, EventArgs e)
        {
            if (radio_shopitems_1safe.Checked == true & c_medicshops.Checked == true)
            {
                radio_shopitems_3rand.Checked = true;
                c_medicshops.Checked = true;
            }
            if (c_medicshops.Checked == true)
            {
                if (radio_shopitems_2max.Checked == true) { a_shopItems = a_shopItems_2maxm; }
            }
            if (c_medicshops.Checked == false)
            {
                if (radio_shopitems_2max.Checked == true) { a_shopItems = a_shopItems_2max; }
            }
        }

        //SYNTH TAB//
        private void radio_synthesis_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox_yuno.Visible = false; status = "synth";
        }

        //CHAR TAB//
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


        //          MATHODS          //

        //Seed
        private void SeedIngest()
        {
            //string seed has input data
            if (textBox_seed.Text.Length == 0) { seed = "Default SEED 4 u"; textBox_seed.Text = seed; }
            seed = textBox_seed.Text;
            data_str = ""; //converted seed to data
            counter = 1; //reset these counters
            richTextBox_debug.Text = "SeedIngest()" + seed.Length;
            progressBar1.Maximum = seed.Length;
            for (int i = 0; i < seed.Length; i++)
            {
                progressBar1.Value = i;
                Random rnd = new Random((int)seed[i]);
                data_str = data_str + rnd.Next(42069, 420420);
                //data_str = data_str + (int)seed[i];
            }
            richTextBox_debug.Text = richTextBox_debug.Text + "\n Ingested " + seed.Length;
            //string data_str has output data
        }
        private void Compressor()
        {
            //string data_str has input data
            progressBar2.Maximum = data_str.Length;
            progressBar2.Value = (counter/ data_str.Length);
            l_counter.Text = counter.ToString();
            if (data_str.Length > 9)
            {
                richTextBox_debug.Text = richTextBox_debug.Text + "\nCompressed= " + counter + "\nlength= " + data_str.Length;
                counter++;
                Splitter();
                SeedRNG();
                Compressor();
            }
        }
        private void Splitter()
        {
            //string data_str has input data
            progressBar1.Maximum = data_str.Length;
            int r = 0; if (!(data_str.Length % 9 == 0)) { r = 1; }
            int r2 = data_str.Length % 9;
            int al = ((data_str.Length / 9) + r); //array length plus 1 for/if remainder
            data_arr = new int[al]; //reset worker array each cycle
            int ai = 0;
            for (int i = 0; i < data_str.Length; i = i + 9)
            {
                progressBar1.Value = i;
                ai = ((i + 9) / 9) - 1;
                if ((al - ai == 1) & (r == 1))
                {
                    int.TryParse(data_str.Substring(i, data_str.Length-i), out data_arr[ai]); // add remainder if any to array
                    i = i + 9; //prevent substring(i,9) exception
                } else 
                { 
                    int.TryParse(data_str.Substring(i, 9), out data_arr[ai]); 
                }
            }
            data_str = ""; //reset data_str
            //array data_arr has output data
        }
        private void SeedRNG()
        {
            //array data_arr has input data
            progressBar1.Maximum = data_arr.Length;
            for (int i = 0; i < data_arr.Length; i++)
            {
                Random rnd = new Random(data_arr[i]);
                data_str = data_str + rnd.Next(420, 4269);
                progressBar1.Value = i;
            }
            //string data_str has output data
        }
        private void CleanSeedInt()
        {
            //string data_str has input data
            int.TryParse(data_str, out data_int);
            //int data_int has output data
        }


        //Shop Tab
        //          DONE
        private void StockShopItemsCSV()
        {
            //stock ShopItems.csv
            string[] a_s_stockShopItems =
            {
                "# This file contains a set of item shops.",
                "# You must set at least 32 different items.",
                "# ",
                "# Comment;Id;# Item01;Item02;Item03;Item04;Item05;Item06;Item07;Item08;Item09;Item10;Item11;Item12;Item13;Item14;Item15;Item16;Item17;Item18;Item19;Item20;Item21;Item22;Item23;Item24;Item25;Item26;Item27;Item28;Item29;Item30;Item31;Item32;;",
                "#;Int32;# UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;UInt8;;",
                "# ",
                "Shop 0000;0;1;2;16;17;57;70;88;89;102;112;114;136;137;149;150;177;-1;;;;;;;;;;;;;;;;;# Shop 0000",
                "Shop 0001;1;1;2;3;17;31;57;79;89;90;102;103;115;116;138;151;178;-1;;;;;;;;;;;;;;;;;# Shop 0001",
                "Shop 0002;2;80;90;104;115;116;117;139;152;178;236;240;241;242;243;244;253;-1;;;;;;;;;;;;;;;;;# Shop 0002",
                "Shop 0003;3;51;58;90;103;104;115;116;117;136;138;139;152;178;236;240;241;242;243;244;253;-1;;;;;;;;;;;;;# Shop 0003",
                "Shop 0004;4;1;2;3;18;32;51;58;71;72;73;79;80;89;90;91;104;117;118;139;150;151;152;153;178;179;-1;;;;;;;;# Shop 0004",
                "Shop 0005;5;33;52;58;71;72;80;91;92;104;105;117;118;119;140;153;154;179;180;-1;;;;;;;;;;;;;;;# Shop 0005",
                "Shop 0006;6;1;2;3;20;33;52;59;71;72;73;89;90;91;92;104;105;116;117;118;119;140;150;151;152;153;154;179;180;-1;;;;;# Shop 0006",
                "Shop 0007;7;20;33;52;59;92;105;118;119;140;153;154;155;180;236;240;241;242;243;244;247;248;253;-1;;;;;;;;;;;# Shop 0007",
                "Shop 0008;8;42;52;59;71;72;73;74;81;92;93;120;121;155;156;-1;;;;;;;;;;;;;;;;;;;# Shop 0008",
                "Shop 0009;9;2;3;4;59;70;71;72;73;74;81;89;90;91;92;93;112;114;115;116;117;118;119;120;121;150;151;152;153;154;155;156;157;-1;# Shop 0009",
                "Shop 0010;10;1;2;3;4;42;52;64;85;93;120;121;155;156;157;236;240;241;242;243;244;245;248;253;-1;;;;;;;;;;# Shop 0010",
                "Shop 0011;11;3;4;20;33;34;41;42;59;60;65;71;72;73;74;85;90;91;92;93;104;105;120;121;122;139;140;141;156;157;158;180;181;-1;# Shop 0011",
                "Shop 0012;12;1;2;3;4;20;21;33;34;41;42;52;59;60;65;74;93;104;105;120;121;122;140;141;156;157;158;178;180;181;-1;;;;# Shop 0012",
                "Shop 0013;13;21;33;34;42;53;60;65;75;81;85;94;105;122;123;124;141;156;157;158;180;181;236;237;240;241;242;243;244;245;247;248;253;-1;# Shop 0013",
                "Shop 0014;14;22;35;43;53;60;66;75;82;85;94;95;105;106;123;124;125;141;142;158;159;181;182;237;240;242;243;244;245;247;248;253;-1;;# Shop 0014",
                "Shop 0015;15;4;5;22;23;36;44;53;61;67;75;76;82;86;95;96;97;106;107;126;127;142;159;160;182;183;237;240;245;246;247;248;253;-1;# Shop 0015",
                "Shop 0016;16;2;17;32;42;52;58;65;74;80;86;96;97;107;127;128;142;143;160;161;183;184;236;237;240;241;242;243;244;245;246;248;253;-1;# Shop 0016",
                "Shop 0017;17;2;3;4;5;24;37;46;54;61;67;76;86;91;92;93;94;95;96;97;107;108;120;122;128;129;143;161;162;184;185;-1;;;# Shop 0017",
                "Shop 0018;18;1;2;3;4;5;6;25;38;47;54;83;86;99;108;129;130;131;144;162;163;186;237;240;241;242;243;244;246;247;248;253;-1;;# Shop 0018",
                "Shop 0019;19;6;25;27;38;47;48;54;79;83;86;93;95;99;100;108;110;119;120;128;129;130;131;144;145;162;163;164;186;187;188;-1;;;# Shop 0019",
                "Shop 0020;20;62;68;77;132;133;165;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0020",
                "Shop 0021;21;3;18;32;43;54;58;81;92;104;140;154;180;236;237;240;241;242;243;244;245;246;247;248;253;-1;;;;;;;;;# Shop 0021",
                "Shop 0022;22;22;35;43;53;60;66;75;82;85;86;94;95;106;123;124;125;142;158;159;182;236;237;240;241;242;243;245;247;248;253;-1;;;# Shop 0022",
                "Shop 0023;23;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0023",
                "Shop 0024;24;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0024",
                "Shop 0025;25;236;240;243;244;253;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0025",
                "Shop 0026;26;236;240;241;242;243;244;253;-1;;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0026",
                "Shop 0027;27;236;240;241;242;243;244;248;253;-1;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0027",
                "Shop 0028;28;236;237;240;241;242;243;244;247;248;253;-1;;;;;;;;;;;;;;;;;;;;;;;# Shop 0028",
                "Shop 0029;29;236;237;240;241;242;243;244;245;247;248;253;-1;;;;;;;;;;;;;;;;;;;;;;# Shop 0029",
                "Shop 0030;30;236;237;240;241;242;243;244;245;246;247;248;253;-1;;;;;;;;;;;;;;;;;;;;;# Shop 0030",
                "Shop 0031;31;236;237;240;241;242;243;244;245;246;247;248;253;-1;;;;;;;;;;;;;;;;;;;;;# Shop 0031"
            };
            a_stockShopItems = a_s_stockShopItems;
            rngc = 1;
            richTextBox_debug.Text = richTextBox_debug.Text + "\nRNG complete =" + rngc;
            richTextBox_output.Text = "";
            for (int i = 0; i < a_stockShopItems.Length; i++)
            {
                richTextBox_output.Text = richTextBox_output.Text + a_stockShopItems[i] + "\n";
            }
        }
        private void ShopItems()
        {

            if (c_medicshops.Checked) { randl = 32; } else { randl = 23; }
            if (radio_shopitems_3rand.Checked)
            {
                for (int i = 0; i < randl; i++)
                {
                    
                    Random rnd = new Random((data_int / (i+1)) + i);
                    a_shopItems[i] = rnd.Next(1, 31);
                    //if (i == 0) { richTextBox_debug.Text = richTextBox_debug.Text + "\ntest " + a_shopItems[i]; }
                }
            }
            int[][] aa_shopItems1 =
            {
                new int[a_shopItems[0]],new int[a_shopItems[1]],new int[a_shopItems[2]],new int[a_shopItems[3]],new int[a_shopItems[4]],new int[a_shopItems[5]],
                new int[a_shopItems[6]],new int[a_shopItems[7]],new int[a_shopItems[8]],new int[a_shopItems[9]],new int[a_shopItems[10]],new int[a_shopItems[11]],
                new int[a_shopItems[12]],new int[a_shopItems[13]],new int[a_shopItems[14]],new int[a_shopItems[15]],new int[a_shopItems[16]],new int[a_shopItems[17]],
                new int[a_shopItems[18]],new int[a_shopItems[19]],new int[a_shopItems[20]],new int[a_shopItems[21]],new int[a_shopItems[22]],new int[a_shopItems[23]],
                new int[a_shopItems[24]],new int[a_shopItems[25]],new int[a_shopItems[26]],new int[a_shopItems[27]],new int[a_shopItems[28]],new int[a_shopItems[29]],
                new int[a_shopItems[30]],new int[a_shopItems[31]]
            };
            aa_shopItems = aa_shopItems1;
            richTextBox_output.Text = "";
            for (int i = 0; i < randl; i++)
            {
                //richTextBox_output.Text = richTextBox_output.Text + "\n i: " + i + " ";
                for (int j = 0; j < aa_shopItems[i].Length; j++)
                {
                    //int seed5 = (data_int + ((30 * i) + (2 * j)));
                    Random rnd = new Random((data_int / (i + j + 1)) + (data_int / (j + 1)));
                    int rnd2 = rnd.Next(1, items);
                    if (rnd2 == 250) { rnd2 = 253; }
                    aa_shopItems[i][j] = rnd2;
                    richTextBox_output.Text = richTextBox_output.Text + /*"j: " + j + " :r: "*/ aa_shopItems[i][j] + "; ";
                }
                richTextBox_output.Text = richTextBox_output.Text + "\n;";
            }
        }
        private void MedicItems()
        {
            //medic items per shop 0,0,7,7,0,0,0,9,0,0,9,0,0,11,9,7,11,0,10,0,0,12,10,0,0,5,7,8,10,11,12,12
            //int[] a_empty = { 0 }; //33
            int[] a_mi0 = a_empty;
            int[] a_mi1 = a_empty;
            int[] a_mi2 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 236, 240, 241, 242, 243, 244, 253 };
            int[] a_mi3 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 236, 240, 241, 242, 243, 244, 253 };
            int[] a_mi4 = a_empty;
            int[] a_mi5 = a_empty;
            int[] a_mi6 = a_empty;
            int[] a_mi7 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 236, 240, 241, 242, 243, 244, 247, 248, 253 };
            int[] a_mi8 = a_empty;
            int[] a_mi9 = a_empty;
            int[] a_mi10 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 236, 240, 241, 242, 243, 244, 245, 248, 253 };
            int[] a_mi11 = a_empty;
            int[] a_mi12 = a_empty;
            int[] a_mi13 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 236, 237, 240, 241, 242, 243, 244, 245, 247, 248, 253 };
            int[] a_mi14 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 237, 240, 242, 243, 244, 245, 247, 248, 253 };
            int[] a_mi15 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 237, 240, 245, 246, 247, 248, 253 };
            int[] a_mi16 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 236, 237, 240, 241, 242, 243, 244, 245, 246, 248, 253 };
            int[] a_mi17 = a_empty;
            int[] a_mi18 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 237, 240, 241, 242, 243, 244, 246, 247, 248, 253 };
            int[] a_mi19 = a_empty;
            int[] a_mi20 = a_empty;
            int[] a_mi21 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 236, 237, 240, 241, 242, 243, 244, 245, 246, 247, 248, 253 };
            int[] a_mi22 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 236, 237, 240, 241, 242, 243, 245, 247, 248, 253 };
            int[][] aa_medicItems1 =
            {
                a_mi0,a_mi1,a_mi2,a_mi3,a_mi4,a_mi5,a_mi6,a_mi7,a_mi8,a_mi9,a_mi10,a_mi11,a_mi12,a_mi13,a_mi14,a_mi15,a_mi16,a_mi17,a_mi18,a_mi19,a_mi20,a_mi21,a_mi22
            }; //lines 7-29
            int[][] aa_medicItemsE1 =
            {
                a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty,a_empty
            };
            string[] medicShops1 =
            {
                 "Shop 0023;23;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0023",
                 "Shop 0024;24;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0024",
                 "Shop 0025;25;236;240;243;244;253;-1;;;;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0025",
                 "Shop 0026;26;236;240;241;242;243;244;253;-1;;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0026",
                 "Shop 0027;27;236;240;241;242;243;244;248;253;-1;;;;;;;;;;;;;;;;;;;;;;;;;# Shop 0027",
                 "Shop 0028;28;236;237;240;241;242;243;244;247;248;253;-1;;;;;;;;;;;;;;;;;;;;;;;# Shop 0028",
                 "Shop 0029;29;236;237;240;241;242;243;244;245;247;248;253;-1;;;;;;;;;;;;;;;;;;;;;;# Shop 0029",
                 "Shop 0030;30;236;237;240;241;242;243;244;245;246;247;248;253;-1;;;;;;;;;;;;;;;;;;;;;# Shop 0030",
                 "Shop 0031;31;236;237;240;241;242;243;244;245;246;247;248;253;-1;;;;;;;;;;;;;;;;;;;;;# Shop 0031"
            }; //lines 30-38
            string[] medicShopsE1 = { "0",  "0", "0", "0", "0", "0", "0", "0", "0" };
            if (radio_shopitems_1safe.Checked == true)
            {
                aa_medicItems = aa_medicItems1;
            } else if (radio_shopitems_1safe.Checked == false) { aa_medicItems = aa_medicItemsE1; }
            
            if (c_medicshops.Checked == true)
            {
                medicShops = medicShopsE1;
            } else if (c_medicshops.Checked == false) { medicShops = medicShops1; }



        }
        private void ShopCombo()
        {
            //import arrays incase i break the arrays during the work

            int[][] aa_cs_medici = aa_medicItems; // input medic items lines 7-29
            int[][] aa_cs_shopi = aa_shopItems; // input RNG shop items
            string[] a_cs_medics = medicShops; //input medic shops lines 30-38
            int[] a_shopItems_1safe = new int[] { 16, 16, 9, 13, 25, 18, 28, 13, 14, 32, 14, 32, 29, 21, 22, 25, 21, 30, 21, 30, 6, 12, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0 };  //use this to 
            int[] a_medicl = new int[] { 0, 0, 7, 7, 0, 0, 0, 9, 0, 0, 9, 0, 0, 11, 9, 7, 11, 0, 10, 0, 0, 12, 10, 0, 0, 5, 7, 8, 10, 11, 12, 12 };
            //string[][] aa_cs_comboSafe = aa_comboSafe; //output

            //test
            string[] a_cs_comboSafe = a_comboSafe;
            //combine aa_cs_mi and aa_cs_si

            for (int i = 0; i < 32; i++) //rows 0-22 for safe list, 23-31 for medicShops
            {
                for (int j = 0; j < 36; j++) //elements 36, 35semis
                {
                    //first element
                    if (j == 0)
                    {
                        //set first string in each line
                        a_cs_comboSafe[i] = "Shop 00";
                        //add extra 0 for first 10 shop string Shop 000
                        if (i < 10)
                        {
                            a_cs_comboSafe[i] = a_cs_comboSafe[i] + "0";
                        }
                        //add shop number and ";" to first element
                        a_cs_comboSafe[i] = a_cs_comboSafe[i] + i + ";";
                    }
                    if (j == 1)
                    {
                        a_cs_comboSafe[i] = a_cs_comboSafe[i] + i + ";";
                    }
                    if ((j > 1) & (j < 35))
                    {
                        //need to include offset only on the j input, not the output
                        //medic items check
                        
                        if (radio_shopitems_1safe.Checked == true & i < 23)
                        {
                            if ((j - 2) < aa_cs_medici[i].Length)
                            {
                                if (aa_cs_medici[i][j - 2] > 0)
                                {
                                    a_cs_comboSafe[i] = a_cs_comboSafe[i] + aa_cs_medici[i][j - 2].ToString() + ";";
                                }
                            }
                        }
                        
                        //shop items check
                        if ((j - 2) < aa_cs_shopi[i].Length)
                        {
                            if (aa_cs_shopi[i][j - 2] > 0)
                            {
                                a_cs_comboSafe[i] = a_cs_comboSafe[i] + aa_cs_shopi[i][j - 2].ToString() + ";";
                            }
                        }
                        if (radio_shopitems_1safe.Checked == false)
                        {
                            //outside range of -1
                            if (j - 2 > aa_cs_shopi[i].Length - 1)
                            {
                                if (j - 2 == aa_cs_shopi[i].Length)
                                {
                                    a_cs_comboSafe[i] = a_cs_comboSafe[i] + "-1;";
                                }
                                if (j - 2 > aa_cs_shopi[i].Length)
                                {
                                    a_cs_comboSafe[i] = a_cs_comboSafe[i] + ";";
                                }
                            }
                        }

                        if (radio_shopitems_1safe.Checked == true & i < 23)
                        {
                            if (((j - 2) > aa_cs_medici[i].Length - 1) & ((j - 2) > aa_cs_shopi[i].Length - 1))
                            {
                                //-1 termination point
                                if (((j - 2) == aa_cs_medici[i].Length) | ((j - 2) == aa_cs_shopi[i].Length))
                                {
                                    a_cs_comboSafe[i] = a_cs_comboSafe[i] + "-1;";
                                }
                                //remainder semicolons added
                                if (((j - 2) > aa_cs_medici[i].Length) & ((j - 2) > aa_cs_shopi[i].Length))
                                {
                                    a_cs_comboSafe[i] = a_cs_comboSafe[i] + ";";
                                }
                            }
                        }
                    }
                    //last comment and shop id
                    if (j == 35)
                    {
                        a_cs_comboSafe[i] = a_cs_comboSafe[i] + "# Shop 00";
                        if (i < 10)
                        {
                            a_cs_comboSafe[i] = a_cs_comboSafe[i] + "0";
                        }
                        a_cs_comboSafe[i] = a_cs_comboSafe[i] + i;
                    }
                    if (i > 22 & c_medicshops.Checked == false)
                    {
                        //combine medicshops at end
                        a_cs_comboSafe[i] = medicShops[i - 23];
                    }
                }
            }
            a_comboSafe = a_cs_comboSafe;
            //string[] a_comboSafe has output data, should be all elements for a full shopitems.csv file
        }
        private void ShopOutput()
        {
            //string[] a_comboSafe has input data
            if (radio_shopitems_1safe.Checked == true) { set = "safe"; }
            if (radio_shopitems_2max.Checked == true) { set = "max"; }
            if (radio_shopitems_3rand.Checked == true) { set = "random"; }
            if (c_medicitems.Checked == true) { set = set + " include medic items"; }
            if (c_medicshops.Checked == true) { set = set + " override medic shops"; }
            richTextBox_output.Text = "#Randomized by FFIX Randomizer Assistant\n#Seed=" + seed + "\n#settings: " + set + "\n";
            for (int i = 0; i < a_comboSafe.Length; i++)
            {
                richTextBox_output.Text = richTextBox_output.Text + a_comboSafe[i] + "\n";
            }
            rngc = 1;
            richTextBox_debug.Text = richTextBox_debug.Text + "\nRNG complete =" + rngc;
        }


        //Synth Tab
        private void Synthesis()
        {

            /*
            * #				
            *  ;Int32;Byte;Byte;Byte;Byte;Byte
            # Comment		;Id;Shops;Price;Result;Item1;Item2
            # ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------;;;;;;
            Butterfly Sword	;0;79;300;7;1;2
            The Ogre		;1;95;700;8;2;2
            Exploda			;2;92;1000;9;2;3
            Rune Tooth		;3;88;2000;10;3;3
            Angel Bless		;4;112;9000;11;3;4
            Sargatanas		;5;96;12000;12;4;5
            Masamune		;6;64;16000;13;5;6
            Duel Claws		;7;64;16000;49;45;46
            Priest’s Racket	;8;64;11000;55;51;218
            Bracer			;9;64;24000;101;197;107
            Gauntlets		;10;64;8000;111;104;99
            Golden Skullcap	;11;64;15000;134;141;128
            Circlet			;12;64;20000;135;129;204
            Grand Helm		;13;64;20000;147;142;200
            Rubber Suit		;14;64;20000;166;163;95
            Brave Suit		;15;64;26000;167;154;58
            Cotton Robe		;16;63;1000;168;88;115
            Silk Robe		;17;60;2000;169;150;118
            Magician Robe	;18;48;3000;170;70;156
            Glutton’s Robe	;19;32;6000;171;81;168
            White Robe		;20;32;8000;172;161;97
            Black Robe		;21;32;8000;173;161;96
            Light Robe		;22;64;20000;174;170;90
            Robe of Lords	;23;128;30000;175;172;173
            Tin Armour		;24;128;50000;176;0;254
            Grand Armour	;25;64;45000;191;18;180
            Desert Boots	;26;95;300;192;112;149
            Yellow Scarf	;27;95;400;212;114;115
            Glass Buckle	;28;95;500;202;90;89
            Germinas Boots	;29;94;900;194;192;79
            Cachusha		;30;62;1000;218;117;136
            Coral Ring		;31;62;1200;206;73;57
            Gold Choker		;32;94;1300;213;178;242
            Magician Shoes	;33;60;1500;193;194;91
            Barette			;34;60;1800;219;80;139
            Power Belt		;35;60;2000;200;202;179
            Madain’s Ring	;36;56;3000;203;91;59
            Fairy Earrings	;37;56;3200;214;93;242
            Extension		;38;56;3500;220;120;52
            Reflect Ring	;39;56;7000;205;199;203
            Anklet;40		;48;4000;199;213;231
            Feather Boots	;41;48;4000;196;193;249
            Black Belt		;42;48;4000;201;122;157
            Pearl Rouge		;43;48;5000;216;229;239
            Promist Ring	;44;32;6000;207;94;230
            Battle Boots	;45;32;6500;197;196;87
            Rebirth Ring	;46;32;7000;208;227;199
            Angel Earrings	;47;32;8000;215;214;219
            Running Shoes	;48;64;12000;198;197;228
            Rosetta Ring	;49;64;24000;204;203;38
            Protect Ring	;50;128;40000;209;250;208
            Pumice			;51;128;50000;211;210;210
            Garnet			;52;224;350;224;254;247
            Amethyst		;53;224;200;225;254;248
            Peridot			;54;224;100;231;254;242
            Sapphire		;55;224;200;232;254;243
            Opal			;56;224;100;233;254;236
            Topaz			;57;224;100;234;254;244
            Lapis Lazuli	;58;192;400;235;254;252
            Pumice Piece	;59;128;25000;210;0;211
            Save the Queen	;60;128;50000;26;31;103
            Phoenix Pinion	;61;128;300;249;240;251
            Ether			;62;128;500;238;241;246
            Thief Gloves	;63;32;50000;98;92;12
            */

        }
        
        //IO
        private string path_search(string pswap)
        {
            RegistryKey rkTest = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\Shell\\MuiCache");
            string[] vnames = rkTest.GetValueNames();
            foreach (string s in vnames)
            {
                RegistryValueKind rvk = rkTest.GetValueKind(s);
                if (rvk == RegistryValueKind.String)
                {
                    string value = (string)rkTest.GetValue(s);
                    if (value == "FINAL FANTASY IX")
                    {
                        richTextBox_debug.Text = richTextBox_debug.Text + "\nfound ff9";
                        pswap = s.Substring(0, s.IndexOf("FF9_Launcher.exe")) + "StreamingAssets\\Data";
                    }
                }
            }
            rkTest.Close();
            return pswap;
        }
        private void ReadWrite()
        {
            if (rngc == 1)
            {
                if (radio_shopitems_1safe.Checked || radio_shopitems_2max.Checked || radio_shopitems_3rand.Checked)
                {
                    Directory.SetCurrentDirectory(@tb_fl.Text + "\\Items");
                    File.WriteAllText("ShopItems.csv", richTextBox_output.Text);
                }
            }
        }
    }
}



/*
                    RW notes
                    lines 7 through 29 also through 38

   stock shop items   
    radio, safe, random, max, check, medic items, medic shops
    medic items are 236-249, 253      250 is dark matter, 251, 252 are gysahl greens and dead pepper, 253 is tent, 254 is ore
    besides tent 250+ cant be bought ever.

 *medic items 0,0,7,7,0,0,0,9,0,0,9,0,0,11,9,7,11,0,10,0,0,12,10,0,0,5,7,8,10,11,12,12
            

//items.csv lines 7-261 (item numbers 000(hammer),001-254) 001-235 =234 normal items
//item number 236+ is potions etc 250 dark matter, 251,252 gysahl,pepper
//default shop item amounts inserted into first cell shop number xx00, potions+ in last cell shop number, shops 25+ later maybe


 Shop 1600;0;1;2;16;17;57;70;88;89;102;112;114;136;137;149;150;177;-1;;;;;;;;;;;;;;;;;# Shop 0000
 Shop 1601;1;1;2;3;17;31;57;79;89;90;102;103;115;116;138;151;178;-1;;;;;;;;;;;;;;;;;# Shop 0001
 Shop 0902;2;80;90;104;115;116;117;139;152;178;236;240;241;242;243;244;253;-1;;;;;;;;;;;;;;;;;# Shop 0702
 Shop 1303;3;51;58;90;103;104;115;116;117;136;138;139;152;178;236;240;241;242;243;244;253;-1;;;;;;;;;;;;;# Shop 0703
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