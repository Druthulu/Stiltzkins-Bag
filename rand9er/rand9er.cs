using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
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
        string data_str, pswap, binloc, set, seed = "42";
        int data_int, counter, randl = 23, items = 236;
        int weapa = 0; int weapb = 85;int armleta = 88; int armletb = 112;int helma = 112; 
        int helmb = 148; int armora = 148; int armorb = 192; int acca = 192; int accb = 224;
        byte[] ba_p0data2, ba_p0data7;
        char[] separators = new char[] { ';', ';' };
        int[] data_arr, a_empty = { 0 };
        string[] medicShops, a_stockShopItems, a_synthdata, a_stockSynthesis, a_statdata, a_stockBaseStats, a_equipdata, a_stockDefaultEquipment, a_itemdata, a_stockItems, a_gemdata, a_stockAbilityGems, a_comboSafe = new string[32];
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
            MogDetect();
        }
        private void richTextBox_debug_TextChanged(object sender, EventArgs e)
        {
            //richTextBox_debug.SelectionStart = richTextBox_debug.Text.Length;
            //richTextBox_debug.ScrollToCaret();
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
        private void b_open_Click(object sender, EventArgs e)
        {
            Manual_search();
        }
        private void b_restore_Click(object sender, EventArgs e)
        {

            //need to write
            if (MessageBox.Show("Selected options will be restored to Stock settings", "Cormfirm Write Data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
            {
                IO_stock();
            }
        }
        private void b_rw_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This will save all Randoms to game files", "Cormfirm Write Data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
            {
                ReadWrite();
            }
        }
        private void b_search_Click(object sender, EventArgs e)
        {
            pswap = "";
            pswap = path_search(pswap);
            if (pswap.Length > 0)
            {
                tb_fl.Text = pswap;
            }
            MogDetect();
        }
        private void cm_itemshop_CheckedChanged(object sender, EventArgs e)
        {
            if (cm_itemshop.Checked)
            {
                c_safe.Enabled = true;
                c_random.Enabled = true;
                c_max.Enabled = true;
                l_shopm.Enabled = true;
                if (!c_safe.Checked & !c_random.Checked & !c_max.Checked)
                {
                    c_random.Checked = true;
                }
            }
            else if (!cm_itemshop.Checked)
            {
                c_safe.Enabled = false;
                c_random.Enabled = false;
                c_max.Enabled = false;
                l_shopm.Enabled = false;
            }
        }
        private void cm_synth_CheckedChanged(object sender, EventArgs e)
        {
            if (cm_synth.Checked)
            {
                c_require.Enabled = true;
                c_result.Enabled = true;
                c_prices.Enabled = true;
                if (!c_require.Checked & !c_result.Checked & !c_prices.Checked)
                {
                    c_require.Checked = true; c_result.Checked = true;
                }

            } else if (!cm_synth.Checked)
            {
                c_require.Enabled = false;
                c_result.Enabled = false;
                c_prices.Enabled = false;
            }
        }
        private void cm_char_CheckedChanged(object sender, EventArgs e)
        {
            if (cm_char.Checked)
            {
                c_default.Enabled = true;
                c_basestats.Enabled = true;
                c_abilitygems.Enabled = true;
                if (!c_default.Checked & !c_basestats.Checked & !c_abilitygems.Checked)
                {
                    c_default.Checked = true; c_basestats.Checked = true;
                }
            }
            else if (!cm_char.Checked)
            {
                c_default.Enabled = false;
                c_basestats.Enabled = false;
                c_abilitygems.Enabled = false;
            }
        }
        private void l_shops_Click(object sender, EventArgs e)
        {
            if (cm_itemshop.Checked)
            {
                cm_itemshop.Checked = false;
            }
            else cm_itemshop.Checked = true;
        }
        private void l_synth_Click(object sender, EventArgs e)
        {
            if (cm_synth.Checked)
            {
                cm_synth.Checked = false;
            }
            else cm_synth.Checked = true;
        }
        private void l_char_Click(object sender, EventArgs e)
        {
            if (cm_char.Checked)
            {
                cm_char.Checked = false;
            }
            else cm_char.Checked = true;
        }
        private void c_safe_CheckedChanged(object sender, EventArgs e)
        {
            if (c_safe.Checked) 
            {
                c_random.Checked = false; 
                c_max.Checked = false; 
                a_shopItems = a_shopItems_1safe; 
            }
        }
        private void c_random_CheckedChanged(object sender, EventArgs e)
        {
            if (c_random.Checked) 
            { 
                c_safe.Checked = false; 
                c_max.Checked = false; 
                c_medicitems.Enabled = true; 
                c_medicshops.Enabled = true; 
                if (!c_medicitems.Checked & !c_medicshops.Checked)
                {
                    c_medicshops.Checked = true;
                    c_medicitems.Checked = true;
                }
                a_shopItems = a_shopItems_3rand; 
            }
            if (!c_max.Checked & !c_random.Checked)
            {
                c_medicitems.Enabled = false;
                c_medicshops.Enabled = false;
            }
        }
        private void c_random_EnabledChanged(object sender, EventArgs e)
        {
            if (!c_random.Enabled)
            {
                c_medicitems.Enabled = false;
                c_medicshops.Enabled = false;
            }
            else if (c_random.Enabled & c_random.Checked)
            {
                c_medicitems.Enabled = true;
                c_medicshops.Enabled = true;
            }
        }
        private void c_max_CheckedChanged(object sender, EventArgs e)
        {
            if (c_max.Checked) 
            { 
                c_random.Checked = false; 
                c_safe.Checked = false;
                c_medicitems.Enabled = true;
                c_medicshops.Enabled = true;
                if (!c_medicitems.Checked & !c_medicshops.Checked)
                {
                    c_medicitems.Checked = true;
                    c_medicshops.Checked = true;
                }
                a_shopItems = a_shopItems_2maxm; 
            }
            if (!c_max.Checked & !c_random.Checked)
            {
                c_medicitems.Enabled = false;
                c_medicshops.Enabled = false;
            }
        }
        private void c_max_EnabledChanged(object sender, EventArgs e)
        {
            if (!c_max.Enabled)
            {
                c_medicitems.Enabled = false;
                c_medicshops.Enabled = false;
            }
            else if (c_max.Enabled & c_max.Checked)
            {
                c_medicitems.Enabled = true;
                c_medicshops.Enabled = true;
            }
        }
        private void c_medicitems_CheckedChanged(object sender, EventArgs e)
        {
            if (c_safe.Checked == true & c_medicitems.Checked == true)
            {
                c_random.Checked = true;
                c_medicitems.Checked = true;
            }
            if (c_medicitems.Checked == true) { items = 250; }
            else
            if (c_medicitems.Checked == false) { items = 235; }

        }
        private void c_medicshops_CheckedChanged(object sender, EventArgs e)
        {
            if (c_safe.Checked == true & c_medicshops.Checked == true)
            {
                c_random.Checked = true;
                c_medicshops.Checked = true;
            }
            if (c_medicshops.Checked == true)
            {
                if (c_max.Checked == true) { a_shopItems = a_shopItems_2maxm; }
            }
            if (c_medicshops.Checked == false)
            {
                if (c_max.Checked == true) { a_shopItems = a_shopItems_2max; }
            }
        }
        private void c_basestats_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void c_abilitygems_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void c_default_CheckedChanged(object sender, EventArgs e)
        {
            if (c_default.Checked)
            {
                c_all_e.Enabled = true;
                c_random_e.Enabled = true;
                c_main_e.Enabled = true;

                if (!c_all_e.Checked & !c_random_e.Checked & !c_main_e.Checked)
                {
                    c_random_e.Checked = true;
                }
            }
            if (!c_default.Checked)
            {
                c_all_e.Enabled = false;
                c_random_e.Enabled = false;
                c_main_e.Enabled = false;
            }
        }
        private void c_default_EnabledChanged(object sender, EventArgs e)
        {
            if (!c_default.Enabled)
            {
                c_all_e.Enabled = false;
                c_random_e.Enabled = false;
                c_main_e.Enabled = false;
            }
            else if (c_default.Enabled & c_default.Checked)
            {
                c_all_e.Enabled = true;
                c_random_e.Enabled = true;
                c_main_e.Enabled = true;
            }
        }
        private void c_main_e_CheckedChanged(object sender, EventArgs e)
        {
            if (c_main_e.Checked)
            {
                c_random_e.Checked = false;
                c_all_e.Checked = false;
            }
        }
        private void c_random_e_CheckedChanged(object sender, EventArgs e)
        {
            if (c_random_e.Checked)
            {
                c_all_e.Checked = false;
                c_main_e.Checked = false;
            }
        }
        private void c_all_e_CheckedChanged(object sender, EventArgs e)
        {
            if (c_all_e.Checked)
            {
                c_random_e.Checked = false;
                c_main_e.Checked = false;
            }
        }
        private void pbar_tree_Click(object sender, EventArgs e)
        {

        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void richTextBox_output_TextChanged(object sender, EventArgs e)
        {

        }
        private void c_require_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void c_result_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void c_prices_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //          change source selection
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //          background pic
        }
        private void tb_fl_TextChanged(object sender, EventArgs e)
        {

        }
        private void l_shopm_Click(object sender, EventArgs e)
        {

        }
        private void label2_Click(object sender, EventArgs e)
        {

        }


        //MAIN
        private void button_rand_Click(object sender, EventArgs e)
        {
            if (cm_itemshop.Checked | cm_synth.Checked | cm_char.Checked | cm_enemies.Checked | cm_entrances.Checked)
            {
                if (MessageBox.Show("This will save all Randoms to game files", "Cormfirm Write Data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                {
                    //reset
                    pbar_tree.Value = pbar_tree.Minimum;
                    SeedIngest();
                    Compressor();
                    CleanSeedInt();
                    if (cm_itemshop.Checked)
                    {
                        ShopItems();
                        MedicItems();
                        ShopCombo();
                    }
                    if (cm_synth.Checked)
                    {
                        Synthesis();
                    }
                    if (cm_char.Checked)
                    {
                        Character();
                    }
                    //richTextBox_debug.Text = richTextBox_debug.Text + "\nrandl: " + randl.ToString() + "\nSeed DNA " + data_int + "\nitems: (1-" + items + ")\n";
                    
                    if (cm_itemshop.Checked | cm_synth.Checked | cm_char.Checked)
                    {
                        ReadWrite();
                    }
                    if (cm_enemies.Checked | cm_entrances.Checked)
                    {
                        Bytes_IO();
                    }
                    pbar_tree.Value = pbar_tree.Maximum;
                }
            }
        }

        //Stock
        private void StockShopItemsCSV()
        {
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
        }
        private void StockSynthesisCSV()
        {
            string[] a_s_stockSynthesis =
            {
                "# This file contains a set of game items that can be synthesized.",
                "# You must set 64 different items.",
                "# -",
                "# Comment;Id;Shops;Price;Result;Item1;Item2",
                "#;Int32;Byte;Byte;Byte;Byte;Byte",
                "# -",
                "Butterfly Sword;0;79;300;7;1;2",
                "The Ogre;1;95;700;8;2;2",
                "Exploda;2;92;1000;9;2;3",
                "Rune Tooth;3;88;2000;10;3;3",
                "Angel Bless;4;112;9000;11;3;4",
                "Sargatanas;5;96;12000;12;4;5",
                "Masamune;6;64;16000;13;5;6",
                "Duel Claws;7;64;16000;49;45;46",
                "Priest’s Racket;8;64;11000;55;51;218",
                "Bracer;9;64;24000;101;197;107",
                "Gauntlets;10;64;8000;111;104;99",
                "Golden Skullcap;11;64;15000;134;141;128",
                "Circlet;12;64;20000;135;129;204",
                "Grand Helm;13;64;20000;147;142;200",
                "Rubber Suit;14;64;20000;166;163;95",
                "Brave Suit;15;64;26000;167;154;58",
                "Cotton Robe;16;63;1000;168;88;115",
                "Silk Robe;17;60;2000;169;150;118",
                "Magician Robe;18;48;3000;170;70;156",
                "Glutton’s Robe;19;32;6000;171;81;168",
                "White Robe;20;32;8000;172;161;97",
                "Black Robe;21;32;8000;173;161;96",
                "Light Robe;22;64;20000;174;170;90",
                "Robe of Lords;23;128;30000;175;172;173",
                "Tin Armour;24;128;50000;176;0;254",
                "Grand Armour;25;64;45000;191;18;180",
                "Desert Boots;26;95;300;192;112;149",
                "Yellow Scarf;27;95;400;212;114;115",
                "Glass Buckle;28;95;500;202;90;89",
                "Germinas Boots;29;94;900;194;192;79",
                "Cachusha;30;62;1000;218;117;136",
                "Coral Ring;31;62;1200;206;73;57",
                "Gold Choker;32;94;1300;213;178;242",
                "Magician Shoes;33;60;1500;193;194;91",
                "Barette;34;60;1800;219;80;139",
                "Power Belt;35;60;2000;200;202;179",
                "Madain’s Ring;36;56;3000;203;91;59",
                "Fairy Earrings;37;56;3200;214;93;242",
                "Extension;38;56;3500;220;120;52",
                "Reflect Ring;39;56;7000;205;199;203",
                "Anklet;40;48;4000;199;213;231",
                "Feather Boots;41;48;4000;196;193;249",
                "Black Belt;42;48;4000;201;122;157",
                "Pearl Rouge;43;48;5000;216;229;239",
                "Promist Ring;44;32;6000;207;94;230",
                "Battle Boots;45;32;6500;197;196;87",
                "Rebirth Ring;46;32;7000;208;227;199",
                "Angel Earrings;47;32;8000;215;214;219",
                "Running Shoes;48;64;12000;198;197;228",
                "Rosetta Ring;49;64;24000;204;203;38",
                "Protect Ring;50;128;40000;209;250;208",
                "Pumice;51;128;50000;211;210;210",
                "Garnet;52;224;350;224;254;247",
                "Amethyst;53;224;200;225;254;248",
                "Peridot;54;224;100;231;254;242",
                "Sapphire;55;224;200;232;254;243",
                "Opal;56;224;100;233;254;236",
                "Topaz;57;224;100;234;254;244",
                "Lapis Lazuli;58;192;400;235;254;252",
                "Pumice Piece;59;128;25000;210;0;211",
                "Save the Queen;60;128;50000;26;31;103",
                "Phoenix Pinion;61;128;300;249;240;251",
                "Ether;62;128;500;238;241;246",
                "Thief Gloves;63;32;50000;98;92;12"
            };
            a_stockSynthesis = a_s_stockSynthesis;
        }
        private void StockAbilityGemsCSV()
        {
            string[] a_s_stockAbilityGems =
            {
                "# This file contains a number of gems, which are required for the activation of passive abilities.",
                "# You must set 64 different items.",
                "# -",
                "# Comment;Id;Gems",
                "#;Int32;UInt8",
                "# -",
                "Auto-Reflect;0;15",
                "Auto-Float;1;6",
                "Auto-Haste;2;9",
                "Auto-Regen;3;10",
                "Auto-Life;4;12",
                "HP+10%;5;4",
                "HP+20%;6;8",
                "MP+10%;7;4",
                "MP+20%;8;8",
                "Accuracy+;9;2",
                "Distract;10;5",
                "Long Reach;11;16",
                "MP Attack;12;5",
                "Bird Killer;13;3",
                "Bug Killer;14;2",
                "Stone Killer;15;4",
                "Undead Killer;16;2",
                "Dragon Killer;17;3",
                "Devil Killer;18;2",
                "Beast Killer;19;4",
                "Man Eater;20;2",
                "High Jump;21;4",
                "Master Thief;22;5",
                "Steal Gil;23;5",
                "Healer;24;2",
                "Add Status;25;3",
                "Gamble Defence;26;1",
                "Chemist;27;4",
                "Power Throw;28;19",
                "Power Up;29;3",
                "Reflect-Null;30;7",
                "Reflectx2;31;17",
                "Mag Elem Null;32;13",
                "Concentrate;33;10",
                "Half MP;34;11",
                "High Tide;35;8",
                "Counter;36;8",
                "Cover;37;6",
                "Protect Girls;38;4",
                "Eye 4 Eye;39;5",
                "Body Temp;40;4",
                "Alert;41;4",
                "Initiative;42;5",
                "Level Up;43;7",
                "Ability Up;44;3",
                "Millionaire;45;5",
                "Flee-Gil;46;3",
                "Guardian Mog;47;3",
                "Insomniac;48;5",
                "Antibody;49;4",
                "Bright Eyes;50;4",
                "Loudmouth;51;4",
                "Restore HP;52;8",
                "Jelly;53;4",
                "Return Magic;54;9",
                "Absorb MP;55;6",
                "Auto-Potion;56;3",
                "Locomotion;57;4",
                "Clear Headed;58;5",
                "Boost;59;12",
                "Odin’s Sword;60;5",
                "Mug;61;3",
                "Bandit;62;5",
                "Void;63;20"
            };
            a_stockAbilityGems = a_s_stockAbilityGems;
        }
        private void StockBaseStatsCSV()
        {
            string[] a_s_stockBaseStats =
            {
                "# This file contains base stats of characters.",
                "# You must set 12 different items.",
                "# -",
                "# Comment;Id;Dexterity;Strength;Magic;Will;Gems",
                "#;Int32;UInt8;UInt8;UInt8;UInt8;UInt8",
                "# -",
                "Zidane;0;23;21;18;23;18",
                "Vivi ;1;16;12;24;19;14",
                "Garnet;2;21;14;23;17;14",
                "Steiner;3;18;24;12;21;17",
                "Freya;4;20;20;16;22;18",
                "Quina;5;14;18;20;11;15",
                "Eiko;6;19;13;21;18;12",
                "Amarant;7;22;22;13;15;18",
                "Cinna;8;19;15;16;19;8",
                "Marcus;9;20;18;11;21;8",
                "Blank;10;23;21;12;21;12",
                "Beatrix;11;24;21;19;23;10"
            };
            a_stockBaseStats = a_s_stockBaseStats;
        }
        private void StockDefaultEquipmentCSV()
        {
            string[] a_s_stockDefaultEquipment =
            {
                "# This file contains predefined equipment sets",
                "# You must set at least 15 different items",
                "# -",
                "# Comment;Id;# Weapon;Head;Wrist;Armor;Accessory;",
                "#;Int32;# UInt8;UInt8;UInt8;UInt8;UInt8;",
                "# -",
                "Zidane; 0;1;112;88;149;-1;# 0000 - Zidane (Dagger, Leather Hat, Wrist, Leather Shirt)",
                "Vivi;1;70;112;-1;149;-1;# 0001 - Vivi (Mage Staff, Leather Hat, Leather Shirt)",
                "Garnet;2;57;-1;-1;150;-1;# 0002 - Garnet (Rod, Silk Shirt)",
                "Steiner;3;16;137;-1;177;-1;# 0003 - Steiner (Broadsword, Bronze Helm, Bronze Armour)",
                "Freya;4;31;136;102;178;-1;# 0004 - Freya (Javelin, Rubber Helm, Bronze Gloves, Linen Cuirass)",
                "Quina;5;79;-1;-1;-1;-1;# 0005 - Quina (Fork)",
                "Eiko;6;64;114;90;150;232;# 0006 - Eiko (Golem’s Flute, Feather Hat, Glass Armlet, Silk Shirt, Sapphire)",
                "Amarant;7;41;-1;89;155;194;# 0007 - Amarant (Cat’s Claws, Leather Wrist, Adaman Vest, Germinas Boots)",
                "Cinna;8;0;112;-1;-1;-1;# 0008 - Cinna (Hammer, Leather Hat)",
                "Marcus;9;16;112;88;149;-1;# 0009 - Marcus (Broadsword, Leather Hat, Wrist, Leather Shirt)",
                "Blank;10;16;-1;-1;150;-1;# 0010 - Blank (Broadsword, Silk Shirt)",
                "Beatrix;11;26;138;104;179;192;# 0011 - Beatrix (Save the Queen, Iron Helm, Mythril Gloves, Chain Mail, Desert Boots)",
                "Marcus 2;12;17;114;88;151;-1;# 0012 - Marcus 2 (Iron Sword, Feather Hat, Wrist, Leather Plate)",
                "Beatrix 2;13;26;142;105;181;212;# 0013 - Beatrix 2 (Save the Queen, Cross Helm, Thunder Gloves, Plate Mail, Yellow Scarf)",
                "Blank 2;14;17;112;-1;150;-1;# 0013 - Blank 2(Iron Sword, Leather Hat, Silk Shirt)",
                "Empty;15;-1;-1;-1;-1;-1;# 0014 - Empty"
            };
            a_stockDefaultEquipment = a_s_stockDefaultEquipment;
        }
        private void StockItemsCSV()
        {
            string[] a_s_stockItems =
            {
                "# This file contains a set of game items.",
                "# You must set 256 different items.",
                "# -",
                "# Price;GraphicsId;ColorId;Quality;BonusId;AbilityIds;Weapon;Armlet;Helmet;Armor;Accessory;Item;Gem;Usable;Order;Zidane;Vivi;Garnet;Steiner;Freya;Quina;Eiko;Amarant;Cinna;Marcus;Blank;Beatrix;",
                "# UInt16;UInt8;UInt8;UInt8;UInt8;UInt8[];UInt8;Bit;Bit;Bit;Bit;Bit;Bit;Bit;UInt8;Bit;Bit;Bit;Bit;Bit;Bit;Bit;Bit;Bit;Bit;Bit;Bit;",
                "# -",
                "250;0;0;1;0;0, 0, 0;1;0;0;0;0;0;0;0;21;0;0;0;0;0;0;0;0;1;0;0;0;# 000 - Hammer",
                "320;1;0;1;0;101, 0, 0;1;0;0;0;0;0;0;0;22;1;0;0;0;0;0;0;0;0;0;0;0;# 001 - Dagger",
                "500;1;12;2;0;102, 101, 0;1;0;0;0;0;0;0;0;23;1;0;0;0;0;0;0;0;0;0;0;0;# 002 - Mage Masher",
                "950;1;0;3;0;254, 0, 0;1;0;0;0;0;0;0;0;24;1;0;0;0;0;0;0;0;0;0;0;0;# 003 - Mythril Dagger",
                "2300;1;6;6;0;105, 107, 0;1;0;0;0;0;0;0;0;25;1;0;0;0;0;0;0;0;0;0;0;0;# 004 - Gladius",
                "6000;1;0;9;0;101, 0, 0;1;0;0;0;0;0;0;0;26;1;0;0;0;0;0;0;0;0;0;0;0;# 005 - Zorlin Shape",
                "17000;1;0;13;149;102, 0, 0;1;0;0;0;0;0;0;0;27;1;0;0;0;0;0;0;0;0;0;0;0;# 006 - Orichalcon",
                "1300;2;0;4;0;103, 230, 0;1;0;0;0;0;0;0;0;28;1;0;0;0;0;0;0;0;0;0;0;0;# 007 - Butterfly Sword",
                "1700;2;0;5;0;104, 0, 0;1;0;0;0;0;0;0;0;29;1;0;0;0;0;0;0;0;0;0;0;0;# 008 - The Ogre",
                "2800;2;0;7;0;106, 107, 0;1;0;0;0;0;0;0;0;30;1;0;0;0;0;0;0;0;0;0;0;0;# 009 - Exploda",
                "3800;2;0;8;0;107, 0, 0;1;0;0;0;0;0;0;0;31;1;0;0;0;0;0;0;0;0;0;0;0;# 010 - Rune Tooth",
                "7000;2;0;10;0;108, 0, 0;1;0;0;0;0;0;0;0;32;1;0;0;0;0;0;0;0;0;0;0;0;# 011 - Angel Bless",
                "9500;2;0;11;0;105, 0, 0;1;0;0;0;0;0;0;0;33;1;0;0;0;0;0;0;0;0;0;0;0;# 012 - Sargatanas",
                "13000;2;0;12;150;106, 0, 0;1;0;0;0;0;0;0;0;34;1;0;0;0;0;0;0;0;0;0;0;0;# 013 - Masamune",
                "30000;2;0;14;0;107, 108, 0;1;0;0;0;0;0;0;0;35;1;0;0;0;0;0;0;0;0;0;0;0;# 014 - The Tower",
                "40000;2;0;15;0;101, 0, 0;1;0;0;0;0;0;0;0;36;1;0;0;0;0;0;0;0;0;0;0;0;# 015 - Ultima Weapon",
                "330;3;0;1;0;211, 0, 0;1;0;0;0;0;0;0;0;37;0;0;0;1;0;0;0;0;0;1;1;0;# 016 - Broadsword",
                "660;3;0;2;0;142, 0, 0;1;0;0;0;0;0;0;0;38;0;0;0;1;0;0;0;0;0;1;1;0;# 017 - Iron Sword",
                "1300;3;0;3;0;145, 0, 0;1;0;0;0;0;0;0;0;39;0;0;0;1;0;0;0;0;0;1;1;0;# 018 - Mythril Sword",
                "1900;3;13;4;0;141, 0, 0;1;0;0;0;0;0;0;0;40;0;0;0;1;0;0;0;0;0;1;1;0;# 019 - Blood Sword",
                "3780;3;5;5;0;146, 0, 0;1;0;0;0;0;0;0;0;41;0;0;0;1;0;0;0;0;0;1;1;0;# 020 - Ice Brand",
                "4000;3;11;6;0;148, 0, 0;1;0;0;0;0;0;0;0;42;0;0;0;1;0;0;0;0;0;1;1;0;# 021 - Coral Sword",
                "4700;3;0;7;0;144, 0, 0;1;0;0;0;0;0;0;0;43;0;0;0;1;0;0;0;0;0;1;1;0;# 022 - Diamond Sword",
                "5190;3;9;8;0;147, 0, 0;1;0;0;0;0;0;0;0;44;0;0;0;1;0;0;0;0;0;1;1;0;# 023 - Flame Saber",
                "8900;3;3;9;0;143, 0, 0;1;0;0;0;0;0;0;0;45;0;0;0;1;0;0;0;0;0;1;1;0;# 024 - Rune Blade",
                "9340;3;0;10;151;149, 0, 0;1;0;0;0;0;0;0;0;46;0;0;0;1;0;0;0;0;0;1;1;0;# 025 - Defender",
                "2;4;0;1;152;0, 0, 0;1;0;0;0;0;0;0;0;47;0;0;0;0;0;0;0;0;0;0;0;1;# 026 - Save the Queen",
                "14000;4;0;12;0;150, 0, 0;1;0;0;0;0;0;0;0;48;0;0;0;1;0;0;0;0;0;0;0;0;# 027 - Ultima Sword",
                "19000;4;3;13;0;151, 0, 0;1;0;0;0;0;0;0;0;49;0;0;0;1;0;0;0;0;0;0;0;0;# 028 - Excalibur",
                "29000;4;0;14;0;152, 149, 0;1;0;0;0;0;0;0;0;50;0;0;0;1;0;0;0;0;0;0;0;0;# 029 - Ragnarok",
                "39000;4;3;15;0;142, 151, 150;1;0;0;0;0;0;0;0;51;0;0;0;1;0;0;0;0;0;0;0;0;# 030 - Excalibur II",
                "880;5;0;1;0;209, 0, 0;1;0;0;0;0;0;0;0;52;0;0;0;0;1;0;0;0;0;0;0;0;# 031 - Javelin",
                "1100;5;0;2;0;118, 0, 0;1;0;0;0;0;0;0;0;53;0;0;0;0;1;0;0;0;0;0;0;0;# 032 - Mythril Spear",
                "1600;5;0;3;0;117, 227, 0;1;0;0;0;0;0;0;0;54;0;0;0;0;1;0;0;0;0;0;0;0;# 033 - Partisan",
                "2430;5;5;4;0;120, 0, 0;1;0;0;0;0;0;0;0;55;0;0;0;0;1;0;0;0;0;0;0;0;# 034 - Ice Lance",
                "3580;5;0;5;0;121, 0, 0;1;0;0;0;0;0;0;0;56;0;0;0;0;1;0;0;0;0;0;0;0;# 035 - Trident",
                "4700;5;6;6;0;122, 0, 0;1;0;0;0;0;0;0;0;57;0;0;0;0;1;0;0;0;0;0;0;0;# 036 - Heavy Lance",
                "6000;5;12;7;0;123, 234, 0;1;0;0;0;0;0;0;0;58;0;0;0;0;1;0;0;0;0;0;0;0;# 037 - Obelisk",
                "11000;5;3;8;0;124, 118, 0;1;0;0;0;0;0;0;0;59;0;0;0;0;1;0;0;0;0;0;0;0;# 038 - Holy Lance",
                "15000;5;0;9;0;124, 123, 120;1;0;0;0;0;0;0;0;60;0;0;0;0;1;0;0;0;0;0;0;0;# 039 - Kain’s Lance",
                "23500;5;0;10;0;119, 0, 0;1;0;0;0;0;0;0;0;61;0;0;0;0;1;0;0;0;0;0;0;0;# 040 - Dragon’s Hair",
                "4000;6;0;1;0;125, 228, 0;1;0;0;0;0;0;0;0;62;0;0;0;0;0;0;0;1;0;0;0;0;# 041 - Cat’s Claws",
                "5000;6;13;2;0;126, 228, 0;1;0;0;0;0;0;0;0;63;0;0;0;0;0;0;0;1;0;0;0;0;# 042 - Poison Knuckles",
                "6500;6;0;3;0;129, 228, 0;1;0;0;0;0;0;0;0;64;0;0;0;0;0;0;0;1;0;0;0;0;# 043 - Mythril Claws",
                "8000;6;0;4;0;128, 228, 0;1;0;0;0;0;0;0;0;65;0;0;0;0;0;0;0;1;0;0;0;0;# 044 - Scissor Fangs",
                "10360;6;0;5;0;127, 228, 0;1;0;0;0;0;0;0;0;66;0;0;0;0;0;0;0;1;0;0;0;0;# 045 - Dragon’s Claws",
                "13500;6;0;6;0;130, 228, 0;1;0;0;0;0;0;0;0;67;0;0;0;0;0;0;0;1;0;0;0;0;# 046 - Tiger Fangs",
                "16000;6;6;7;0;131, 228, 0;1;0;0;0;0;0;0;0;68;0;0;0;0;0;0;0;1;0;0;0;0;# 047 - Avenger",
                "18000;6;0;8;0;132, 129, 228;1;0;0;0;0;0;0;0;69;0;0;0;0;0;0;0;1;0;0;0;0;# 048 - Kaiser Knuckles",
                "21500;6;0;9;0;128, 127, 228;1;0;0;0;0;0;0;0;70;0;0;0;0;0;0;0;1;0;0;0;0;# 049 - Duel Claws",
                "28800;6;3;10;0;126, 131, 130;1;0;0;0;0;0;0;0;71;0;0;0;0;0;0;0;1;0;0;0;0;# 050 - Rune Claws",
                "400;7;0;2;0;7, 8, 0;1;0;0;0;0;0;0;0;72;0;0;1;0;0;0;1;0;0;0;0;0;# 051 - Air Racket",
                "750;7;0;5;0;19, 9, 11;1;0;0;0;0;0;0;0;73;0;0;1;0;0;0;1;0;0;0;0;0;# 052 - Multina Racket",
                "1350;7;0;8;153;18, 15, 1;1;0;0;0;0;0;0;0;74;0;0;1;0;0;0;1;0;0;0;0;0;# 053 - Magic Racket",
                "2250;7;0;11;0;16, 11, 12;1;0;0;0;0;0;0;0;75;0;0;1;0;0;0;1;0;0;0;0;0;# 054 - Mythril Racket",
                "8000;7;0;17;0;14, 22, 0;1;0;0;0;0;0;0;0;76;0;0;1;0;0;0;1;0;0;0;0;0;# 055 - Priest’s Racket",
                "5800;7;0;19;0;21, 0, 0;1;0;0;0;0;0;0;0;77;0;0;1;0;0;0;1;0;0;0;0;0;# 056 - Tiger Racket",
                "260;8;0;1;0;1, 8, 12;1;0;0;0;0;0;0;0;78;0;0;1;0;0;0;0;0;0;0;0;0;# 057 - Rod",
                "560;8;0;3;0;5, 14, 11;1;0;0;0;0;0;0;0;79;0;0;1;0;0;0;0;0;0;0;0;0;# 058 - Mythril Rod",
                "760;8;12;4;154;236, 16, 20;1;0;0;0;0;0;0;0;80;0;0;1;0;0;0;0;0;0;0;0;0;# 059 - Stardust Rod",
                "1770;8;4;9;0;216, 2, 5;1;0;0;0;0;0;0;0;81;0;0;1;0;0;0;0;0;0;0;0;0;# 060 - Healing Rod",
                "3180;8;3;12;0;15, 17, 14;1;0;0;0;0;0;0;0;82;0;0;1;0;0;0;0;0;0;0;0;0;# 061 - Asura’s Rod",
                "3990;8;0;15;0;3, 12, 11;1;0;0;0;0;0;0;0;83;0;0;1;0;0;0;0;0;0;0;0;0;# 062 - Wizard Rod",
                "10280;8;0;18;0;3, 5, 0;1;0;0;0;0;0;0;0;84;0;0;1;0;0;0;0;0;0;0;0;0;# 063 - Whale Whisker",
                "2700;9;7;6;0;195, 2, 5;1;0;0;0;0;0;0;0;85;0;0;0;0;0;0;1;0;0;0;0;0;# 064 - Golem’s Flute",
                "3800;9;2;7;0;20, 9, 14;1;0;0;0;0;0;0;0;86;0;0;0;0;0;0;1;0;0;0;0;0;# 065 - Lamia’s Flute",
                "4500;9;0;10;0;10, 13, 4;1;0;0;0;0;0;0;0;87;0;0;0;0;0;0;1;0;0;0;0;0;# 066 - Fairy Flute",
                "5700;9;3;13;0;3, 22, 23;1;0;0;0;0;0;0;0;88;0;0;0;0;0;0;1;0;0;0;0;0;# 067 - Hamelin",
                "7000;9;0;14;0;6, 21, 10;1;0;0;0;0;0;0;0;89;0;0;0;0;0;0;1;0;0;0;0;0;# 068 - Siren’s Flute",
                "8300;9;0;16;0;24, 10, 3;1;0;0;0;0;0;0;0;90;0;0;0;0;0;0;1;0;0;0;0;0;# 069 - Angel Flute",
                "320;10;0;1;0;25, 0, 0;1;0;0;0;0;0;0;0;91;0;1;0;0;0;0;0;0;0;0;0;0;# 070 - Mage Staff",
                "1100;10;9;2;0;26, 28, 0;1;0;0;0;0;0;0;0;92;0;1;0;0;0;0;0;0;0;0;0;0;# 071 - Flame Staff",
                "980;10;5;2;0;30, 32, 0;1;0;0;0;0;0;0;0;93;0;1;0;0;0;0;0;0;0;0;0;0;# 072 - Ice Staff",
                "1200;10;11;2;0;34, 37, 0;1;0;0;0;0;0;0;0;94;0;1;0;0;0;0;0;0;0;0;0;0;# 073 - Lightning Staff",
                "2400;10;0;5;0;36, 38, 40;1;0;0;0;0;0;0;0;95;0;1;0;0;0;0;0;0;0;0;0;0;# 074 - Oak Staff",
                "3200;10;2;6;0;41, 44, 42;1;0;0;0;0;0;0;0;96;0;1;0;0;0;0;0;0;0;0;0;0;# 075 - Cypress Pile",
                "4500;10;0;7;155;27, 31, 35;1;0;0;0;0;0;0;0;97;0;1;0;0;0;0;0;0;0;0;0;0;# 076 - Octagon Rod",
                "6000;10;0;8;0;46, 39, 0;1;0;0;0;0;0;0;0;98;0;1;0;0;0;0;0;0;0;0;0;0;# 077 - High Mage Staff",
                "10000;10;0;9;0;48, 0, 0;1;0;0;0;0;0;0;0;99;0;1;0;0;0;0;0;0;0;0;0;0;# 078 - Mace of Zeus",
                "1100;11;0;1;0;227, 0, 0;1;0;0;0;0;0;0;0;100;0;0;0;0;0;1;0;0;0;0;0;0;# 079 - Fork",
                "3100;11;0;2;0;227, 0, 0;1;0;0;0;0;0;0;0;101;0;0;0;0;0;1;0;0;0;0;0;0;# 080 - Needle Fork",
                "4700;11;0;3;0;227, 0, 0;1;0;0;0;0;0;0;0;102;0;0;0;0;0;1;0;0;0;0;0;0;# 081 - Mythril Fork",
                "7400;11;2;4;0;227, 0, 0;1;0;0;0;0;0;0;0;103;0;0;0;0;0;1;0;0;0;0;0;0;# 082 - Silver Fork",
                "10300;11;4;5;0;227, 0, 0;1;0;0;0;0;0;0;0;104;0;0;0;0;0;1;0;0;0;0;0;0;# 083 - Bistro Fork",
                "13300;11;0;6;0;227, 0, 0;1;0;0;0;0;0;0;0;105;0;0;0;0;0;1;0;0;0;0;0;0;# 084 - Gastro Fork",
                "200;12;0;0;0;0, 0, 0;1;0;0;0;0;0;0;0;18;0;0;0;0;0;0;0;0;0;0;0;0;# 085 - Pinwheel",
                "500;12;0;0;0;0, 0, 0;1;0;0;0;0;0;0;0;19;0;0;0;0;0;0;0;0;0;0;0;0;# 086 - Rising Sun",
                "3000;12;0;0;0;0, 0, 0;1;0;0;0;0;0;0;0;20;0;0;0;0;0;0;0;0;0;0;0;0;# 087 - Wing Edge",
                "130;13;3;20;1;238, 0, 0;0;1;0;0;0;0;0;0;107;1;1;1;0;0;1;1;1;1;1;1;0;# 088 - Wrist",
                "200;13;3;22;2;211, 29, 0;0;1;0;0;0;0;0;0;108;1;1;1;0;0;1;1;1;1;1;1;0;# 089 - Leather Wrist",
                "250;13;3;24;3;215, 241, 0;0;1;0;0;0;0;0;0;109;1;1;1;0;0;1;1;1;1;1;1;0;# 090 - Glass Armlet",
                "330;13;14;26;4;217, 0, 0;0;1;0;0;0;0;0;0;110;1;1;1;0;0;1;1;1;1;1;1;0;# 091 - Bone Wrist",
                "500;13;7;28;5;206, 0, 0;0;1;0;0;0;0;0;0;111;1;1;1;0;0;1;1;1;1;1;1;0;# 092 - Mythril Armlet",
                "1000;13;2;32;6;250, 14, 0;0;1;0;0;0;0;0;0;112;0;1;1;0;0;1;1;0;0;0;0;0;# 093 - Magic Armlet",
                "1200;13;2;33;7;253, 217, 0;0;1;0;0;0;0;0;0;113;1;1;1;0;0;1;1;1;1;1;1;0;# 094 - Chimera Armlet",
                "2000;13;7;34;8;211, 235, 0;0;1;0;0;0;0;0;0;114;1;1;1;0;0;1;1;1;1;1;1;0;# 095 - Egoist’s Armlet",
                "3000;13;3;29;9;254, 208, 45;0;1;0;0;0;0;0;0;115;1;1;1;0;0;1;1;1;1;1;1;0;# 096 - N-Kai Armlet",
                "3400;13;2;30;10;232, 227, 0;0;1;0;0;0;0;0;0;116;1;1;1;0;0;1;1;1;1;1;1;0;# 097 - Jade Armlet",
                "50000;13;2;39;11;214, 0, 0;0;1;0;0;0;0;0;0;117;1;0;0;0;0;0;0;1;1;1;1;0;# 098 - Thief Gloves",
                "4800;13;7;38;12;245, 117, 0;0;1;0;0;0;0;0;0;118;1;1;1;0;1;1;1;1;1;1;1;0;# 099 - Dragon Wrist",
                "5100;13;1;42;13;201, 0, 0;0;1;0;0;0;0;0;0;119;1;1;1;0;0;1;1;1;1;1;1;0;# 100 - Power Wrist",
                "8000;13;3;43;14;217, 220, 0;0;1;0;0;0;0;0;0;120;1;1;1;0;0;1;1;1;1;1;1;0;# 101 - Bracer",
                "480;14;9;21;15;241, 0, 0;0;1;0;0;0;0;0;0;121;0;0;0;1;1;0;0;0;0;0;0;1;# 102 - Bronze Gloves",
                "720;14;8;23;16;208, 0, 0;0;1;0;0;0;0;0;0;122;0;0;0;1;1;0;0;0;0;0;0;1;# 103 - Silver Gloves",
                "980;14;8;25;17;212, 206, 0;0;1;0;0;0;0;0;0;123;0;0;0;1;1;0;0;0;0;0;0;1;# 104 - Mythril Gloves",
                "1200;14;11;27;18;210, 217, 0;0;1;0;0;0;0;0;0;124;0;0;0;1;1;0;0;0;0;0;0;1;# 105 - Thunder Gloves",
                "2000;14;8;31;19;236, 245, 0;0;1;0;0;0;0;0;0;125;0;0;0;1;1;0;0;0;0;0;0;1;# 106 - Diamond Gloves",
                "2800;14;2;41;20;193, 228, 0;0;1;0;0;0;0;0;0;126;0;0;0;1;1;0;0;0;0;0;0;1;# 107 - Venetia Shield",
                "6000;14;2;40;21;198, 0, 0;0;1;0;0;0;0;0;0;127;0;0;0;1;1;0;0;0;0;0;0;1;# 108 - Defense Gloves",
                "2;14;2;36;22;227, 0, 0;0;1;0;0;0;0;0;0;128;0;0;0;1;1;0;0;0;0;0;0;1;# 109 - Genji Gloves",
                "7000;14;2;35;23;148, 0, 0;0;1;0;0;0;0;0;0;129;0;0;0;1;0;0;0;0;0;0;0;1;# 110 - Aegis Gloves",
                "8800;14;2;37;24;229, 0, 0;0;1;0;0;0;0;0;0;130;0;0;0;1;1;0;0;0;0;0;0;1;# 111 - Gauntlets",
                "150;15;8;51;25;25, 0, 0;0;0;1;0;0;0;0;0;131;1;1;1;0;0;1;1;1;1;1;1;0;# 112 - Leather Hat",
                "1500;15;8;0;26;0, 0, 0;0;0;1;0;0;0;0;0;132;1;1;1;1;1;1;1;1;1;1;1;1;# 113 - Straw Hat",
                "200;15;8;53;27;242, 217, 0;0;0;1;0;0;0;0;0;133;1;1;1;0;0;1;1;0;1;1;1;0;# 114 - Feather Hat",
                "260;15;8;55;28;12, 0, 0;0;0;1;0;0;0;0;0;134;1;1;1;0;0;1;1;0;1;1;1;0;# 115 - Steepled Hat",
                "330;15;2;52;29;208, 0, 0;0;0;1;0;0;0;0;0;135;1;1;1;0;0;1;1;1;1;1;1;0;# 116 - Headgear",
                "400;15;8;57;30;32, 0, 0;0;0;1;0;0;0;0;0;136;1;1;1;0;0;1;1;0;1;1;1;0;# 117 - Magus Hat",
                "500;29;1;59;31;212, 240, 0;0;0;1;0;0;0;0;0;137;1;1;1;0;0;1;1;1;1;1;1;0;# 118 - Bandana",
                "600;15;12;61;32;243, 26, 0;0;0;1;0;0;0;0;0;138;0;1;1;0;0;1;1;0;0;0;0;0;# 119 - Mage’s Hat",
                "800;29;4;64;33;250, 17, 20;0;0;1;0;0;0;0;0;139;0;0;1;0;1;1;1;0;1;1;1;0;# 120 - Lamia’s Tiara",
                "1000;15;8;63;34;228, 242, 208;0;0;1;0;0;0;0;0;140;1;1;1;0;0;1;1;1;1;1;1;0;# 121 - Ritual Hat",
                "1200;29;12;65;35;218, 217, 0;0;0;1;0;0;0;0;0;141;1;1;1;0;0;1;1;1;1;1;1;0;# 122 - Twist Headband",
                "1500;29;14;66;36;198, 241, 0;0;0;1;0;0;0;0;0;142;1;1;1;0;0;1;1;1;1;1;1;0;# 123 - Mantra Band",
                "1800;15;8;68;37;227, 245, 0;0;0;1;0;0;0;0;0;143;1;1;1;0;0;1;1;1;1;1;1;0;# 124 - Dark Hat",
                "2180;15;4;69;38;236, 250, 0;0;0;1;0;0;0;0;0;144;1;1;1;0;0;1;1;1;1;1;1;0;# 125 - Green Beret",
                "2550;15;7;72;39;201, 249, 43;0;0;1;0;0;0;0;0;145;1;1;1;0;0;1;1;0;1;1;1;0;# 126 - Black Hood",
                "3000;15;1;71;40;204, 229, 0;0;0;1;0;0;0;0;0;146;1;1;1;0;0;1;1;1;1;1;1;0;# 127 - Red Hat",
                "3700;29;3;74;41;195, 243, 0;0;0;1;0;0;0;0;0;147;1;1;1;0;0;1;1;1;1;1;1;0;# 128 - Golden Hairpin",
                "4400;29;2;76;42;212, 246, 0;0;0;1;0;0;0;0;0;148;1;1;1;0;0;1;1;1;1;1;1;0;# 129 - Coronet",
                "5200;15;2;78;43;231, 211, 0;0;0;1;0;0;0;0;0;149;1;1;1;0;0;0;1;1;1;1;1;0;# 130 - Flash Hat",
                "6100;15;8;75;44;198, 218, 0;0;0;1;0;0;0;0;0;150;1;1;1;0;0;1;1;1;1;1;1;0;# 131 - Adaman Hat",
                "7100;15;8;79;45;203, 107, 253;0;0;1;0;0;0;0;0;151;1;0;0;0;0;0;0;0;1;1;1;0;# 132 - Thief Hat",
                "8300;15;2;80;46;240, 232, 0;0;0;1;0;0;0;0;0;152;0;1;1;0;0;1;1;0;0;0;0;0;# 133 - Holy Miter",
                "12000;15;9;83;47;221, 249, 0;0;0;1;0;0;0;0;0;153;1;1;1;0;0;1;1;1;1;1;1;0;# 134 - Golden Skullcap",
                "13000;29;10;84;48;245, 250, 0;0;0;1;0;0;0;0;0;154;1;1;1;0;0;1;1;1;1;1;1;0;# 135 - Circlet",
                "250;16;8;54;49;142, 0, 0;0;0;1;0;0;0;0;0;155;0;0;0;1;1;0;0;0;0;0;0;1;# 136 - Rubber Helm",
                "330;16;9;56;50;206, 0, 0;0;0;1;0;0;0;0;0;156;0;0;0;1;1;0;0;0;0;0;0;1;# 137 - Bronze Helm",
                "450;16;8;58;51;242, 235, 0;0;0;1;0;0;0;0;0;157;0;0;0;1;1;0;0;0;0;0;0;1;# 138 - Iron Helm",
                "600;16;8;60;52;233, 209, 0;0;0;1;0;0;0;0;0;158;0;0;0;1;1;0;0;0;0;0;0;1;# 139 - Barbut",
                "1000;16;8;62;53;240, 241, 0;0;0;1;0;0;0;0;0;159;0;0;0;1;1;0;0;0;0;0;0;1;# 140 - Mythril Helm",
                "1800;16;1;67;54;146, 118, 250;0;0;1;0;0;0;0;0;160;0;0;0;1;1;0;0;0;0;0;0;1;# 141 - Gold Helm",
                "2200;16;8;70;55;204, 210, 0;0;0;1;0;0;0;0;0;161;0;0;0;1;1;0;0;0;0;0;0;1;# 142 - Cross Helm",
                "3000;16;12;73;56;201, 240, 0;0;0;1;0;0;0;0;0;162;0;0;0;1;1;0;0;0;0;0;0;1;# 143 - Diamond Helm",
                "4600;16;8;77;57;244, 207, 0;0;0;1;0;0;0;0;0;163;0;0;0;1;1;0;0;0;0;0;0;1;# 144 - Platinum Helm",
                "7120;16;8;82;58;231, 0, 0;0;0;1;0;0;0;0;0;164;0;0;0;1;1;0;0;0;0;0;0;1;# 145 - Kaiser Helm",
                "2;16;8;81;59;198, 0, 0;0;0;1;0;0;0;0;0;165;0;0;0;1;1;0;0;0;0;0;0;1;# 146 - Genji Helmet",
                "14000;16;3;85;60;227, 0, 0;0;0;1;0;0;0;0;0;166;0;0;0;1;1;0;0;0;0;0;0;1;# 147 - Grand Helm",
                "19000;18;8;0;61;0, 0, 0;0;0;0;1;0;0;0;0;167;1;1;1;1;1;1;1;1;1;1;1;1;# 148 - Aloha T-shirt",
                "270;18;0;102;62;230, 0, 0;0;0;0;1;0;0;0;0;168;1;1;1;0;0;1;1;1;1;1;1;0;# 149 - Leather Shirt",
                "400;18;8;103;63;1, 33, 0;0;0;0;1;0;0;0;0;169;1;1;1;0;0;1;1;0;1;1;1;0;# 150 - Silk Shirt",
                "530;18;0;104;64;125, 0, 0;0;0;0;1;0;0;0;0;170;1;0;0;0;0;0;0;1;1;1;1;0;# 151 - Leather Plate",
                "670;18;9;105;65;245, 0, 0;0;0;0;1;0;0;0;0;171;1;1;1;0;0;1;1;1;1;1;1;0;# 152 - Bronze Vest",
                "810;18;8;107;66;210, 0, 0;0;0;0;1;0;0;0;0;172;1;0;0;0;0;0;0;1;1;1;1;0;# 153 - Chain Plate",
                "1180;18;8;110;67;248, 0, 0;0;0;0;1;0;0;0;0;173;1;1;1;0;0;1;1;1;1;1;1;0;# 154 - Mythril Vest",
                "1600;18;8;112;68;207, 205, 0;0;0;0;1;0;0;0;0;174;1;1;1;0;0;1;1;1;1;1;1;0;# 155 - Adaman Vest",
                "1850;18;6;114;69;240, 199, 0;0;0;0;1;0;0;0;0;175;0;1;1;0;0;1;1;0;0;0;0;0;# 156 - Magician Cloak",
                "2900;18;8;117;70;249, 241, 253;0;0;0;1;0;0;0;0;176;1;1;1;0;0;1;1;1;1;1;1;0;# 157 - Survival Vest",
                "4300;18;8;120;71;236, 246, 0;0;0;0;1;0;0;0;0;177;1;0;0;0;0;0;0;1;1;1;1;0;# 158 - Brigandine",
                "5000;18;10;122;72;202, 197, 0;0;0;0;1;0;0;0;0;178;1;1;1;0;0;1;1;1;1;1;1;0;# 159 - Judo Uniform",
                "7200;18;10;125;73;207, 218, 228;0;0;0;1;0;0;0;0;179;1;1;1;0;0;1;1;1;1;1;1;0;# 160 - Power Vest",
                "8700;18;0;124;74;240, 227, 39;0;0;0;1;0;0;0;0;180;1;1;1;0;0;1;1;1;1;1;1;0;# 161 - Gaia Gear",
                "10250;18;8;127;75;210, 248, 249;0;0;0;1;0;0;0;0;181;1;1;1;0;0;1;1;1;1;1;1;0;# 162 - Demon’s Vest",
                "12200;18;8;129;76;244, 227, 0;0;0;0;1;0;0;0;0;182;0;0;1;0;1;0;1;0;0;0;0;1;# 163 - Minerva’s Plate",
                "14000;18;14;133;77;233, 249, 231;0;0;0;1;0;0;0;0;183;1;0;0;0;0;0;0;1;1;1;1;0;# 164 - Ninja Gear",
                "16300;18;14;137;78;250, 245, 0;0;0;0;1;0;0;0;0;184;1;1;1;0;0;1;1;1;1;1;1;0;# 165 - Dark Gear",
                "20000;18;11;140;79;231, 10, 0;0;0;0;1;0;0;0;0;185;0;0;1;0;1;0;1;0;0;0;0;1;# 166 - Rubber Suit",
                "22500;18;8;141;80;244, 195, 0;0;0;0;1;0;0;0;0;186;1;0;0;0;0;0;0;1;1;1;1;0;# 167 - Brave Suit",
                "4000;17;2;108;81;219, 11, 0;0;0;0;1;0;0;0;0;187;0;1;1;0;0;1;1;0;0;0;0;0;# 168 - Cotton Robe",
                "5800;17;2;115;82;236, 243, 0;0;0;0;1;0;0;0;0;188;0;1;1;0;0;1;1;0;0;0;0;0;# 169 - Silk Robe",
                "8000;17;8;119;83;248, 199, 0;0;0;0;1;0;0;0;0;189;0;1;1;0;0;1;1;0;0;0;0;0;# 170 - Magician Robe",
                "16000;17;8;131;84;241, 232, 195;0;0;0;1;0;0;0;0;190;0;0;0;0;0;1;0;0;0;0;0;0;# 171 - Glutton’s Robe",
                "29000;17;3;134;85;243, 248, 24;0;0;0;1;0;0;0;0;191;0;0;1;0;0;0;1;0;0;0;0;0;# 172 - White Robe",
                "29000;17;14;136;86;200, 47, 223;0;0;0;1;0;0;0;0;192;0;1;0;0;0;1;0;0;0;0;0;0;# 173 - Black Robe",
                "40000;17;1;132;87;226, 195, 6;0;0;0;1;0;0;0;0;193;0;1;1;0;0;1;1;0;0;0;0;0;# 174 - Light Robe",
                "52000;17;8;139;88;222, 225, 0;0;0;0;1;0;0;0;0;194;0;1;1;0;0;1;1;0;0;0;0;0;# 175 - Robe of Lords",
                "20;19;8;100;89;0, 0, 0;0;0;0;1;0;0;0;0;195;0;0;0;1;0;0;0;0;0;0;0;0;# 176 - Tin Armor",
                "650;19;11;106;90;205, 0, 0;0;0;0;1;0;0;0;0;196;0;0;0;1;1;0;0;0;0;0;0;1;# 177 - Bronze Armor",
                "800;19;8;109;91;229, 0, 0;0;0;0;1;0;0;0;0;197;0;0;0;1;1;0;0;0;0;0;0;1;# 178 - Linen Cuirass",
                "1200;19;2;111;92;197, 205, 0;0;0;0;1;0;0;0;0;198;0;0;0;1;1;0;0;0;0;0;0;1;# 179 - Chain Mail",
                "1830;19;2;113;93;245, 229, 0;0;0;0;1;0;0;0;0;199;0;0;0;1;1;0;0;0;0;0;0;1;# 180 - Mythril Armor",
                "2320;19;2;116;94;249, 208, 0;0;0;0;1;0;0;0;0;200;0;0;0;1;1;0;0;0;0;0;0;1;# 181 - Plate Mail",
                "2950;19;3;118;95;207, 0, 0;0;0;0;1;0;0;0;0;201;0;0;0;1;1;0;0;0;0;0;0;1;# 182 - Gold Armor",
                "4300;19;2;121;96;202, 0, 0;0;0;0;1;0;0;0;0;202;0;0;0;1;1;0;0;0;0;0;0;1;# 183 - Shield Armor",
                "5900;19;8;123;97;227, 0, 0;0;0;0;1;0;0;0;0;203;0;0;0;1;1;0;0;0;0;0;0;1;# 184 - Demon’s Mail",
                "8800;19;12;126;98;236, 0, 0;0;0;0;1;0;0;0;0;204;0;0;0;1;1;0;0;0;0;0;0;1;# 185 - Diamond Armor",
                "10500;19;2;128;99;211, 0, 0;0;0;0;1;0;0;0;0;205;0;0;0;1;1;0;0;0;0;0;0;1;# 186 - Platina Armor",
                "12300;19;8;130;100;195, 0, 0;0;0;0;1;0;0;0;0;206;0;0;0;1;1;0;0;0;0;0;0;1;# 187 - Carabini Mail",
                "14000;19;6;135;101;213, 0, 0;0;0;0;1;0;0;0;0;207;0;0;0;0;1;0;0;0;0;0;0;1;# 188 - Dragon Mail",
                "2;19;2;138;102;232, 201, 0;0;0;0;1;0;0;0;0;208;0;0;0;1;1;0;0;0;0;0;0;1;# 189 - Genji Armor",
                "22600;19;6;142;103;198, 0, 0;0;0;0;1;0;0;0;0;209;0;0;0;1;0;0;0;0;0;0;0;1;# 190 - Maximillian",
                "28000;19;6;143;104;219, 244, 0;0;0;0;1;0;0;0;0;210;0;0;0;1;1;0;0;0;0;0;0;1;# 191 - Grand Armor",
                "1500;23;7;153;105;238, 12, 7;0;0;0;0;1;0;0;0;211;1;1;1;1;1;1;1;1;1;1;1;1;# 192 - Desert Boots",
                "7500;23;6;157;106;199, 250, 19;0;0;0;0;1;0;0;0;212;1;1;1;1;1;1;1;1;1;1;1;1;# 193 - Magician Shoes",
                "4000;23;9;153;107;233, 197, 101;0;0;0;0;1;0;0;0;213;1;1;1;1;1;1;1;1;1;1;1;1;# 194 - Germinas Boots",
                "1200;23;5;0;108;0, 0, 0;0;0;0;0;1;0;0;0;214;1;1;1;1;1;1;1;1;1;1;1;1;# 195 - Sandals",
                "6000;23;9;154;109;193, 20, 15;0;0;0;0;1;0;0;0;215;1;1;1;1;1;1;1;1;1;1;1;1;# 196 - Feather Boots",
                "21000;23;10;161;110;204, 234, 198;0;0;0;0;1;0;0;0;216;1;1;1;1;1;1;1;1;1;1;1;1;# 197 - Battle Boots",
                "33000;23;3;158;111;194, 248, 13;0;0;0;0;1;0;0;0;217;1;1;1;1;1;1;1;1;1;1;1;1;# 198 - Running Shoes",
                "3200;33;4;156;112;249, 216, 228;0;0;0;0;1;0;0;0;218;0;0;1;0;1;0;1;1;0;0;0;1;# 199 - Anklet",
                "7000;31;9;160;113;204, 228, 26;0;0;0;0;1;0;0;0;219;1;1;1;1;1;1;1;1;1;1;1;1;# 200 - Power Belt",
                "11000;31;14;159;114;198, 211, 41;0;0;0;0;1;0;0;0;220;1;1;1;1;1;1;1;1;1;1;1;1;# 201 - Black Belt",
                "1600;31;8;156;115;241, 217, 33;0;0;0;0;1;0;0;0;221;1;1;1;1;1;1;1;1;1;1;1;1;# 202 - Glass Buckle",
                "7500;20;10;160;116;232, 219, 239;0;0;0;0;1;0;0;0;222;1;1;1;1;1;1;1;1;1;1;1;1;# 203 - Madain’s Ring",
                "36000;20;1;162;117;235, 225, 223;0;0;0;0;1;0;0;0;223;1;1;1;1;1;1;1;1;1;1;1;1;# 204 - Rosetta Ring",
                "7000;20;14;159;118;192, 202, 16;0;0;0;0;1;0;0;0;224;1;1;1;1;1;1;1;1;1;1;1;1;# 205 - Reflect Ring",
                "4000;20;11;154;119;240, 212, 117;0;0;0;0;1;0;0;0;225;1;1;1;1;1;1;1;1;1;1;1;1;# 206 - Coral Ring",
                "9000;20;2;154;120;244, 247, 224;0;0;0;0;1;0;0;0;226;1;1;1;1;1;1;1;1;1;1;1;1;# 207 - Promist Ring",
                "10000;20;3;160;121;196, 5, 130;0;0;0;0;1;0;0;0;227;1;1;1;1;1;1;1;1;1;1;1;1;# 208 - Rebirth Ring",
                "40000;20;6;163;122;203, 224, 226;0;0;0;0;1;0;0;0;228;1;1;1;1;1;1;1;1;1;1;1;1;# 209 - Protect Ring",
                "2;32;2;159;123;251, 0, 0;0;0;0;0;1;0;0;0;229;1;1;1;1;1;1;1;1;1;1;1;1;# 210 - Pumice Piece",
                "2;32;3;161;124;64, 0, 0;0;0;0;0;1;0;0;0;230;1;1;1;1;1;1;1;1;1;1;1;1;# 211 - Pumice",
                "1800;33;3;159;125;205, 237, 215;0;0;0;0;1;0;0;0;231;1;1;1;1;1;1;1;1;1;1;1;1;# 212 - Yellow Scarf",
                "4000;33;11;159;126;248, 238, 11;0;0;0;0;1;0;0;0;232;1;1;1;1;1;1;1;1;1;1;1;1;# 213 - Gold Choker",
                "6000;35;12;157;127;235, 232, 4;0;0;0;0;1;0;0;0;233;1;1;1;1;1;1;1;1;1;1;1;1;# 214 - Fairy Earrings",
                "20000;35;3;160;128;195, 200, 118;0;0;0;0;1;0;0;0;234;0;0;1;0;1;0;1;0;1;1;1;1;# 215 - Angel Earrings",
                "4000;34;2;155;129;235, 222, 243;0;0;0;0;1;0;0;0;235;0;0;1;0;1;0;1;0;0;0;0;1;# 216 - Pearl Rouge",
                "980;33;2;0;130;0, 0, 0;0;1;0;0;0;0;0;0;106;1;1;1;1;1;1;1;1;1;1;1;1;# 217 - Pearl Armlet",
                "3000;29;8;159;131;242, 236, 5;0;0;0;0;1;0;0;0;236;0;0;1;0;1;0;1;0;0;0;0;1;# 218 - Cachusha",
                "7000;33;1;159;132;219, 218, 2;0;0;0;0;1;0;0;0;237;0;0;1;0;1;0;1;0;0;0;0;1;# 219 - Barette",
                "10000;33;5;160;133;248, 199, 235;0;0;0;0;1;0;0;0;238;0;0;1;0;1;0;1;0;0;0;0;1;# 220 - Extension",
                "2;33;14;160;134;74, 236, 239;0;0;0;0;1;0;0;0;239;1;1;1;1;1;1;1;1;1;1;1;1;# 221 - Ribbon",
                "2;28;3;159;135;195, 0, 0;0;0;0;0;1;0;0;0;240;0;0;1;0;1;0;1;0;0;0;0;1;# 222 - Maiden Prayer",
                "2;28;8;155;136;252, 0, 0;0;0;0;0;1;0;1;0;241;0;0;1;0;1;0;1;0;0;0;0;1;# 223 - Ancient Aroma",
                "2;21;13;0;137;62, 216, 0;0;0;0;0;1;1;1;0;242;1;1;1;1;1;1;1;1;1;1;1;1;# 224 - Garnet",
                "2;21;4;0;138;55, 41, 0;0;0;0;0;1;1;1;0;243;1;1;1;1;1;1;1;1;1;1;1;1;# 225 - Amethyst",
                "2;21;0;0;139;60, 197, 0;0;0;0;0;1;1;1;0;244;1;1;1;1;1;1;1;1;1;1;1;1;# 226 - Aquamarine",
                "2;21;7;0;140;232, 202, 0;0;0;0;0;1;1;1;0;245;1;1;1;1;1;1;1;1;1;1;1;1;# 227 - Diamond",
                "2;21;5;0;141;13, 199, 120;0;0;0;0;1;1;1;0;246;1;1;1;1;1;1;1;1;1;1;1;1;# 228 - Emerald",
                "2;21;12;0;142;11, 211, 0;0;0;0;0;1;1;1;0;247;1;1;1;1;1;1;1;1;1;1;1;1;# 229 - Moonstone",
                "2;21;1;0;143;68, 16, 0;0;0;0;0;1;1;1;0;248;1;1;1;1;1;1;1;1;1;1;1;1;# 230 - Ruby",
                "2;21;6;0;144;53, 34, 0;0;0;0;0;1;1;1;0;249;1;1;1;1;1;1;1;1;1;1;1;1;# 231 - Peridot",
                "2;21;8;0;145;66, 227, 0;0;0;0;0;1;1;1;0;250;1;1;1;1;1;1;1;1;1;1;1;1;# 232 - Sapphire",
                "2;21;2;0;146;49, 30, 0;0;0;0;0;1;1;1;0;251;1;1;1;1;1;1;1;1;1;1;1;1;# 233 - Opal",
                "2;21;3;0;147;51, 26, 0;0;0;0;0;1;1;1;0;252;1;1;1;1;1;1;1;1;1;1;1;1;# 234 - Topaz",
                "2;21;10;0;148;236, 201, 0;0;0;0;0;1;1;1;0;253;1;1;1;1;1;1;1;1;1;1;1;1;# 235 - Lapis Lazuli",
                "50;22;8;0;0;0, 0, 0;0;0;0;0;0;1;0;1;0;0;0;0;0;0;0;0;0;0;0;0;0;# 236 - Potion",
                "200;22;6;0;0;0, 0, 0;0;0;0;0;0;1;0;1;1;0;0;0;0;0;0;0;0;0;0;0;0;# 237 - Hi-Potion",
                "2000;22;9;0;0;0, 0, 0;0;0;0;0;0;1;1;1;2;0;0;0;0;0;0;0;0;0;0;0;0;# 238 - Ether",
                "2;22;2;0;0;0, 0, 0;0;0;0;0;0;1;1;1;3;0;0;0;0;0;0;0;0;0;0;0;0;# 239 - Elixir",
                "150;25;9;0;0;0, 0, 0;0;0;0;0;0;1;0;1;4;0;0;0;0;0;0;0;0;0;0;0;0;# 240 - Phoenix Down",
                "50;24;10;0;0;0, 0, 0;0;0;0;0;0;1;0;1;8;0;0;0;0;0;0;0;0;0;0;0;0;# 241 - Echo Screen",
                "100;24;11;0;0;0, 0, 0;0;0;0;0;0;1;0;1;9;0;0;0;0;0;0;0;0;0;0;0;0;# 242 - Soft",
                "50;24;6;0;0;0, 0, 0;0;0;0;0;0;1;0;1;6;0;0;0;0;0;0;0;0;0;0;0;0;# 243 - Antidote",
                "50;24;8;0;0;0, 0, 0;0;0;0;0;0;1;0;1;7;0;0;0;0;0;0;0;0;0;0;0;0;# 244 - Eye Drops",
                "100;24;4;0;0;0, 0, 0;0;0;0;0;0;1;0;1;10;0;0;0;0;0;0;0;0;0;0;0;0;# 245 - Magic Tag",
                "100;24;0;0;0;0, 0, 0;0;0;0;0;0;1;0;1;12;0;0;0;0;0;0;0;0;0;0;0;0;# 246 - Vaccine",
                "300;24;3;0;0;0, 0, 0;0;0;0;0;0;1;0;1;5;0;0;0;0;0;0;0;0;0;0;0;0;# 247 - Remedy",
                "150;24;2;0;0;0, 0, 0;0;0;0;0;0;1;0;1;11;0;0;0;0;0;0;0;0;0;0;0;0;# 248 - Annoyntment",
                "2000;25;3;0;0;72, 0, 0;0;0;0;0;1;1;0;0;14;0;0;0;0;0;0;1;0;0;0;0;0;# 249 - Phoenix Pinion",
                "2;30;0;0;156;58, 0, 0;0;0;0;0;1;1;0;0;17;1;1;1;1;1;1;1;1;0;0;0;1;# 250 - Dark Matter",
                "60;27;9;0;0;0, 0, 0;0;0;0;0;0;1;0;1;13;0;0;0;0;0;0;0;0;0;0;0;0;# 251 - Gysahl Greens",
                "100;27;1;0;0;0, 0, 0;0;0;0;0;0;1;0;0;15;0;0;0;0;0;0;0;0;0;0;0;0;# 252 - Dead Pepper",
                "800;26;9;0;0;0, 0, 0;0;0;0;0;0;1;0;0;16;0;0;0;0;0;0;0;0;0;0;0;0;# 253 - Tent",
                "300;21;14;0;0;0, 0, 0;0;0;0;0;0;1;1;0;254;0;0;0;0;0;0;0;0;0;0;0;0;# 254 - Ore",
                "0;0;0;0;0;0, 0, 0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;# 255 - open 255"
            };
            a_stockItems = a_s_stockItems;
        }

        //Seed
        private void SeedIngest()
        {
            //string seed has input data
            if (textBox_seed.Text.Length == 0) { seed = "Default SEED 4 u"; textBox_seed.Text = seed; }
            seed = textBox_seed.Text;
            data_str = ""; //converted seed to data
            counter = 1; //reset these counters
            //richTextBox_debug.Text = seed.Length + " seeds planted ";
            for (int i = 0; i < seed.Length; i++)
            {
                Random rnd = new Random((int)seed[i]);
                data_str = data_str + rnd.Next(426942, 4269420);
            }
            //string data_str has output data
        }
        private void Compressor()
        {
            //string data_str has input data
            if (data_str.Length > 9)
            {
                counter++;
                Splitter();
                SeedRNG();
                Compressor();
            }
        }
        private void Splitter()
        {
            //string data_str has input data

            int r = 0; if (!(data_str.Length % 9 == 0)) { r = 1; }
            int r2 = data_str.Length % 9;
            int al = ((data_str.Length / 9) + r); //array length plus 1 for/if remainder
            data_arr = new int[al]; //reset worker array each cycle
            int ai = 0;
            for (int i = 0; i < data_str.Length; i = i + 9)
            {
                //pbar_leaf.Value = i;
                ai = ((i + 9) / 9) - 1;
                if ((al - ai == 1) & (r == 1))
                {
                    int.TryParse(data_str.Substring(i, data_str.Length - i), out data_arr[ai]); // add remainder if any to array
                    i = i + 9; //prevent substring(i,9) exception
                }
                else
                {
                    int.TryParse(data_str.Substring(i, 9), out data_arr[ai]);
                }
            }
            data_str = ""; //reset data_str
            //array data_arr has output data
        }
        private void SeedRNG()
        {
            for (int i = 0; i < data_arr.Length; i++)
            {
                Random rnd = new Random(data_arr[i]);
                data_str = data_str + rnd.Next(4269, 42069);
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
        private void ShopItems()
        {
            //settings for comment line numebr 5
            set = "#Randomized by Stiltzkin's Bag 1.0   Seed:" + data_int + "   set:";
            if (c_safe.Checked == true) { set = set + "/safe "; }
            if (c_random.Checked == true) { set = set + "/random"; }
            if (c_max.Checked == true) { set = set + "/max"; }
            if (c_random.Enabled & c_random.Checked | c_max.Enabled & c_max.Checked) 
            {
                if (c_medicitems.Checked == true) { set = set + "+medic items "; }
                if (c_medicshops.Checked == true) { set = set + "->override medic shops"; }
            }

            StockShopItemsCSV();

            if (c_medicshops.Checked) { randl = 32; } else { randl = 23; }
            if (c_random.Checked)
            {
                for (int i = 0; i < randl; i++)
                {
                    Random rnd = new Random((data_int / (i+1)) + i);
                    a_shopItems[i] = rnd.Next(1, 32);
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
            //richTextBox_output.Text = "";
            for (int i = 0; i < randl; i++)
            {
                //richTextBox_output.Text = richTextBox_output.Text + "\n i: " + i + " ";
                for (int j = 0; j < aa_shopItems[i].Length; j++)
                {
                    Random rnd = new Random((data_int / (i + j*9 + 1)) + (data_int / (j + 1)));
                    int rnd2 = rnd.Next(1, items);
                    if (rnd2 == 250) { rnd2 = 253; }
                    aa_shopItems[i][j] = rnd2;
                    //richTextBox_output.Text = richTextBox_output.Text + /*"j: " + j + " :r: "*/ aa_shopItems[i][j] + "; ";
                }
                //richTextBox_output.Text = richTextBox_output.Text + "\n;";
            }
        }
        private void MedicItems()
        {
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
            if (c_safe.Checked == true)
            {
                aa_medicItems = aa_medicItems1;
            } else if (c_safe.Checked == false) { aa_medicItems = aa_medicItemsE1; }
            
            if (c_medicshops.Enabled & c_medicshops.Checked == true)
            {
                medicShops = medicShopsE1;
            } else if (!c_medicshops.Enabled | c_medicshops.Checked == false) { medicShops = medicShops1; }

        }
        private void ShopCombo()
        {
            int[][] aa_cs_medici = aa_medicItems; // input medic items lines 7-29
            int[][] aa_cs_shopi = aa_shopItems; // input RNG shop items
            string[] a_cs_medics = medicShops, a_ssi = a_stockShopItems; //input medic shops lines 30-38
            int[] a_shopItems_1safe = new int[] { 16, 16, 9, 13, 25, 18, 28, 13, 14, 32, 14, 32, 29, 21, 22, 25, 21, 30, 21, 30, 6, 12, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0 };  //use this to 
            int[] a_medicl = new int[] { 0, 0, 7, 7, 0, 0, 0, 9, 0, 0, 9, 0, 0, 11, 9, 7, 11, 0, 10, 0, 0, 12, 10, 0, 0, 5, 7, 8, 10, 11, 12, 12 };
            string[] a_cs_comboSafe = a_comboSafe;

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
                        if (c_safe.Checked == true & i < 23)
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
                        if (c_safe.Checked == false)
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
                        if (c_safe.Checked == true & i < 23)
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
                    //last comment
                    if (j == 35)
                    {
                        a_cs_comboSafe[i] = a_cs_comboSafe[i] + "#Randomized";
                    }
                    if (i > 22 & c_medicshops.Enabled == false | i > 22 & c_medicshops.Checked == false)
                    {
                        a_cs_comboSafe[i] = medicShops[i - 23];
                    }
                }
            }
            //resize out back into fullsize datafile
            for (int i =6; i < a_ssi.Length; i++)
            {
                a_ssi[i] = a_cs_comboSafe[i - 6];
            }
            a_ssi[5] = set;
            a_comboSafe = a_ssi;
            //string[] a_comboSafe has output data
        }
        private void ShopOutput()
        {
            //string[] a_comboSafe has input data
            for (int i = 0; i < a_comboSafe.Length; i++)
            {
                richTextBox_output.Text = richTextBox_output.Text + a_comboSafe[i] + "\n";
            }
        }
        
        //Synth Tab
        private void Synthesis()
        {
            //settings for comment line numebr 5
            set = "#Randomized by Stiltzkin's Bag 1.0   Seed:" + data_int + "   set:";
            if (c_require.Checked == true) { set = set + "/requirements "; }
            if (c_result.Checked == true) { set = set + "/result "; }
            if (c_prices.Checked == true) { set = set + "/prices "; }

            //int data_int has seedDNA
            StockSynthesisCSV();
            a_synthdata = a_stockSynthesis;
            int a=0, b=0;
            for (int i = 0; i < a_synthdata.Length; i++)
            {
                //lines 6+ has useful data
                if (i > 5)
                {
                    string[] a_synthline = a_synthdata[i].Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 3; j < a_synthline.Length; j++)
                    {
                        Random rnd = new Random((data_int / (i + j*2 + 42)) + (data_int / (j + 1)) + (data_int / ((i*2 * j*3) + 1)));
                        if ((j == 3) & (c_prices.Checked == true))
                        {
                            a = 200; b = 20000;
                            a_synthline[j] = rnd.Next(a, b).ToString();
                            a_synthline[0] = "#Randomized";
                        }
                        if ((j == 4) & (c_result.Checked == true))
                        {
                            a = 1; b = 236;
                            a_synthline[j] = rnd.Next(a, b).ToString();
                            a_synthline[0] = "#Randomized";
                        }
                        if ((j > 4 & j < 7) & (c_require.Checked == true))
                        {
                            a = 1; b = 236;
                            a_synthline[j] = rnd.Next(a, b).ToString();
                            a_synthline[0] = "#Randomized";
                        }
                    }
                    //rebuild line array to  line string, store in string array
                    a_synthdata[i] = String.Join(";", a_synthline);
                }
            }
            a_synthdata[5] = set;
            //string[] a_synthdata has output data
        }
        private void SynthOutput()
        {
            //string[] a_synthdata has input data
            for (int i = 0; i < a_synthdata.Length; i++)
            {
                richTextBox_output.Text = richTextBox_output.Text + a_synthdata[i] + "\n";
            }
        }

        
        //Char Tab
        private void Character()
        {
            void Set()
            {
                //settings for comment line numebr 5
                set = "#Randomized by Stiltzkin's Bag 1.0   Seed:" + data_int + "   set:";
                if (c_abilitygems.Checked == true) { set = set + "/abilitygems "; }
                if (c_basestats.Checked == true) { set = set + "/base stats "; }
                if (c_default.Checked == true)
                {
                    set = set + "/default equipment";
                    if (c_random_e.Checked) { set = set + "->random"; }
                    if (c_all_e.Checked) { set = set + "->share"; }
                    if (c_main_e.Checked) { set = set + "->maintain"; }
                }
            }
            Set();
            if (c_abilitygems.Checked)
            {
                StockAbilityGemsCSV();
                a_gemdata = a_stockAbilityGems;
                int gema = 3, gemb = 14;
                for (int i = 6; i < a_gemdata.Length; i++)
                {
                    string[] a_gemline = a_gemdata[i].Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < a_gemline.Length; j++) //needs this loop, weird
                    {
                        if (j == 2) //also needs this part, weirder
                        {
                            Random rnd = new Random((data_int / (i + 4)) + (data_int / (i + 13)));
                            a_gemline[2] = rnd.Next(gema, gemb).ToString();
                        }
                    }
                    a_gemdata[i] = String.Join(";", a_gemline);
                }
                a_gemdata[5] = set;
            }
            //string[] a_gemdata has output data
            if (c_basestats.Checked)
            {
                StockBaseStatsCSV();
                a_statdata = a_stockBaseStats;
                int stata = 8, statb = 24;
                for (int i = 6; i < a_statdata.Length; i++)
                {

                    string[] a_statline = a_statdata[i].Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 2; j < a_statline.Length; j++)
                    {
                        Random rnd = new Random((data_int / (i*6 + j + 7)) + (data_int / (j + 4)) + (data_int / ((i*3 * j*5) + 1)));
                        a_statline[j] = rnd.Next(stata, statb).ToString();
                    }
                    a_statdata[i] = String.Join(";", a_statline);
                }
                a_statdata[5] = set;
            }
            //string[] a_statdata has output data
            if (c_default.Checked)
            {
                StockItemsCSV();
                StockDefaultEquipmentCSV();
                a_itemdata = a_stockItems;
                a_equipdata = a_stockDefaultEquipment;
                PermissionEdit();
                Set();
                int rr = 0; int a = 0; int b = 224;
                for (int i = 0; i < a_equipdata.Length; i++)
                {
                    if ((i > 5) & (i < 21))
                    {
                        string[] a_equipline = a_equipdata[i].Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        string[] a_itemline;
                        Random rnd; int rrr = 1;

                        for (int j = 2; j < a_equipline.Length; j++)
                        {
                            if (j < 7)
                            {
                                rnd = new Random((data_int / (i*4 + j + 1)) + (data_int / (j + 7)) + (data_int / ((i * j*8) + 1)));
                                rr = 0; a = 0; b = 224;
                                void ReRoll()
                                {
                                    rr = rnd.Next(a, b);
                                    //pull line from item file using the items.csv+6 offset
                                    a_itemline = a_itemdata[rr + 6].Split(separators, StringSplitOptions.RemoveEmptyEntries);
                                    //offsets for dup characters in file
                                    int temp_i = i;
                                    if (i == 18) { temp_i = i-3; }
                                    if (i == 19) { temp_i = i-2; }
                                    if (i == 20) { temp_i = i-4; }
                                    //offset is i+9 from defaultequipment file to items file line
                                    if (a_itemline[temp_i + 9] == "0") //fail permission check
                                    {
                                        rrr++;
                                        richTextBox_debug.Text = rrr + " rolls\nI" + rr + "C" + i + "P" + j;
                                        rnd = new Random((data_int / (i + j*2 + rrr)) + (data_int / (j + rr)) + (data_int / ((i*7 * j) + rrr)));
                                        ReRoll(); //continue failing till succeed
                                    }
                                }

                                if (j == 2) //weapon        0-84
                                {
                                    a = weapa; b = weapb;
                                    ReRoll();
                                    a_equipline[j] = rr.ToString();
                                }
                                if (j == 3) //armlet        88-111
                                {
                                    a = armleta; b = armletb;
                                    ReRoll();
                                    a_equipline[j] = rr.ToString();
                                }
                                if (j == 4) //helmets       112-147
                                {
                                    a = helma; b = helmb;
                                    ReRoll();
                                    a_equipline[j] = rr.ToString();
                                }
                                if (j == 5) //armor         148-191
                                {
                                    a = armora; b = armorb;
                                    ReRoll();
                                    a_equipline[j] = rr.ToString();

                                }
                                if (j == 6) //accessory     192-223
                                {
                                    a = acca; b = accb;
                                    ReRoll();
                                    a_equipline[j] = rr.ToString();
                                }
                            }
                            if (j == 7) //comment
                            {
                                a_equipline[j] = "# randomized";
                            }
                        }
                        a_equipdata[i] = String.Join(";", a_equipline);
                    }
                }
                a_equipdata[5] = set;
            }
            //string[]  a_equipdata has output data
        }
        private void PermissionEdit()
        {
            //settings for comment line numebr 5
            set = "#Randomized by Stiltzkin's Bag 1.0   Seed:" + data_int + "   set:";
            if (c_abilitygems.Checked == true) { set = set + "/abilitygems "; }
            if (c_basestats.Checked == true) { set = set + "/base stats "; }
            if (c_default.Checked == true)
            {
                set = set + "/default equipment";
                if (c_random_e.Checked) { set = set + "->random"; }
                if (c_all_e.Checked) { set = set + "->share"; }
                if (c_main_e.Checked) { set = set + "->maintain"; }
            }

            if (c_random_e.Enabled & c_random_e.Checked)
            {
                //edit permissions to random 0s or 1s
                string[] a_permdata = a_itemdata;
                for (int i = 6; i < 229; i++)
                {
                    string[] a_permline = a_permdata[i].Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 15; j < a_permline.Length - 1; j++)
                    {
                        Random rnd = new Random((data_int / (i + j*5)) + (data_int / j+8) + (data_int / (i*3 * j)));
                        a_permline[j] = rnd.Next(0,2).ToString();
                    }
                    a_permline[27] = a_permline[27] + " +Random Permissions";
                    a_permdata[i] = String.Join(";", a_permline);
                }
                a_permdata[5] = set;
                a_itemdata = a_permdata;
            }
            if (c_all_e.Enabled & c_all_e.Checked)
            {
                //edit permissions to all 1s
                string[] a_permdata = a_itemdata;
                for (int i =6; i < 229; i++)
                {
                    string[] a_permline = a_permdata[i].Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 15; j < a_permline.Length - 1; j++)
                    {
                        a_permline[j] = "1";
                    }
                    a_permline[27] = a_permline[27] + " +All Permissions";
                    a_permdata[i] = String.Join(";", a_permline);
                }
                a_permdata[5] = set;
                a_itemdata = a_permdata;
            }
            if (c_main_e.Checked)
            {
                //no permission edits needed for this one
            }
            //default ranges--we could make this more accurate per i, but not nesseccary.
            weapa = 0; weapb = 85;
            armleta = 88; armletb = 112;
            helma = 112; helmb = 148;
            armora = 148; armorb = 192;
            acca = 192; accb = 224;
            //string[] a_itemdata has output data
        }
        private void CharOutput()
        {
            //string[] a_statdata has output data
            //string[] a_gemdata has output data
            //string[]  a_equipdata has output data
            if (c_abilitygems.Checked == true)
            {
                for (int i = 0; i < a_gemdata.Length; i++)
                {
                    richTextBox_output.Text = richTextBox_output.Text + a_gemdata[i] + "\n";
                }
            }
            if (c_basestats.Checked == true)
            {
                for (int i = 0; i < a_statdata.Length; i++)
                {
                    richTextBox_output.Text = richTextBox_output.Text + a_statdata[i] + "\n";
                }
            }
            if (c_default.Checked == true)
            {
                for (int i = 0; i < a_equipdata.Length; i++)
                {
                    richTextBox_output.Text = richTextBox_output.Text + a_equipdata[i] + "\n";
                }
            }
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
                        b_search.Text = "FFIX Located!";
                        pswap = s.Substring(0, s.IndexOf("FF9_Launcher.exe"));
                        var t = Task.Run(async delegate
                        {
                            await Task.Delay(600);
                        });
                        t.Wait();
                        b_search.Text = "Auto Locate";
                        break;
                    }
                }
            }
            rkTest.Close();
            return pswap;
        }
        private void Manual_search()
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.Description = "locate ffix";
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                /*[] separators = new char[] { '\\', '\\' };
                string[] a_path = folderDlg.SelectedPath.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                for (int i = a_path.Length-1; i > 0; i--)
                {
                    if (a_path[i] == "Data" | a_path[i] == "data" | a_path[i] == "DATA")
                    {
                        break;
                    }
                    if (a_path[i] == "FINAL FANTASY IX" | a_path[i] == "final fantasy ix" | a_path[i] == "Final Fantasy IX")
                    {
                        folderDlg.SelectedPath = folderDlg.SelectedPath;
                        break;
                    }
                }*/
                tb_fl.Text = folderDlg.SelectedPath;
            }
            MogDetect();
        }
        private void ReadWrite()
        {
            button_rand.Text = "Writing Data";
            var t = Task.Run(async delegate
            {
                await Task.Delay(600);
            });
            t.Wait();
            if (cm_itemshop.Checked)
            {
                Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Items");
                File.WriteAllLines("ShopItems.csv", a_comboSafe);
            }
            if (cm_synth.Checked)
            {
                Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Items");
                File.WriteAllLines("Synthesis.csv", a_synthdata);
            }
            if (cm_char.Checked)
            {
                if (c_abilitygems.Checked)
                {
                    Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Characters\\Abilities");
                    File.WriteAllLines("AbilityGems.csv", a_gemdata);
                }
                if (c_basestats.Checked)
                {
                    Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Characters");
                    File.WriteAllLines("BaseStats.csv", a_statdata);
                }
                if (c_default.Checked)
                {
                    Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Characters");
                    File.WriteAllLines("DefaultEquipment.csv", a_equipdata);

                    if (c_all_e.Checked | c_random_e.Checked) 
                    {
                        Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Items");
                        File.WriteAllLines("Items.csv", a_itemdata);
                    }
                }
            }
            button_rand.Text = "Done!";
            t = Task.Run(async delegate
            {
                await Task.Delay(600);
            });
            t.Wait();
            button_rand.Text = "Buy for 333 Gil";
        }
        private void IO_stock()
        {
            b_restore.Text = "Restoring...";
            var t = Task.Run(async delegate
            {
                await Task.Delay(600);
            });
            t.Wait();
            if (cm_itemshop.Checked)
            {
                StockShopItemsCSV();
                Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Items");
                File.WriteAllLines("ShopItems.csv", a_stockShopItems);
            }
            if (cm_synth.Checked)
            {
                StockSynthesisCSV();
                Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Items");
                File.WriteAllLines("Synthesis.csv", a_stockSynthesis);
            }
            if (cm_char.Checked)
            {

                if (c_abilitygems.Checked)
                {
                    StockAbilityGemsCSV();
                    Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Characters\\Abilities");
                    File.WriteAllLines("AbilityGems.csv", a_stockAbilityGems);
                }
                if (c_basestats.Checked)
                {
                    StockBaseStatsCSV();
                    Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Characters");
                    File.WriteAllLines("BaseStats.csv", a_stockBaseStats);
                }
                if (c_default.Checked)
                {
                    StockDefaultEquipmentCSV();
                    Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Characters");
                    File.WriteAllLines("DefaultEquipment.csv", a_stockDefaultEquipment);
                    StockItemsCSV();
                    Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Items");
                    File.WriteAllLines("Items.csv", a_stockItems);
                }
            }
            if (cm_enemies.Checked)
            {
                //p0data2.bin
                Directory.SetCurrentDirectory(binloc);
                if (File.Exists(binloc + "\\p0data2.bak"))
                {
                    File.Copy(binloc + "\\p0data2.bak", binloc + "\\p0data2.bin");
                }
                
            }
            if (cm_entrances.Checked)
            {
                //p0data7.bin
                Directory.SetCurrentDirectory(binloc);
                if (File.Exists(binloc + "\\p0data7.bak"))
                {
                    File.Copy(binloc + "\\p0data7.bak", binloc + "\\p0data7.bin");
                }
            }
            b_restore.Text = "Done!";
            t = Task.Run(async delegate
            {
                await Task.Delay(600);
            });
            t.Wait();
            b_restore.Text = "Restore Stock Checked Files";
        }

        //New GUI changes

        private void c_mogdetect_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void MogDetect()
        {
            if (Directory.Exists(@tb_fl.Text+ "\\MoguriFiles\\StreamingAssets"))
            {
                c_mogdetect.Checked = true;
                c_mogdetect.Text = "Moguri detected";
                binloc = tb_fl.Text + "\\MoguriFiles\\StreamingAssets";
            } else
            {
                binloc = tb_fl.Text + "\\StreamingAssets";
            }
        }

        //Entrances
        private void cm_entrances_CheckedChanged(object sender, EventArgs e)
        {
            //          random entrances selection
            if (cm_entrances.Checked)
            {
                c_allpaths.Enabled = true;
                c_nologic.Enabled = true;
                if (!c_nologic.Checked & !c_allpaths.Checked)
                {
                    c_allpaths.Checked = true;
                }
            }
            else if (!cm_entrances.Checked)
            {
                c_allpaths.Enabled = false;
                c_nologic.Enabled = false;
            }
        }
        private void c_nologic_CheckedChanged(object sender, EventArgs e)
        {
            // connect randomly with no gaurenteed completion path
            if (c_nologic.Checked)
            {
                c_allpaths.Checked = false;
            }
        }
        private void c_allpaths_CheckedChanged(object sender, EventArgs e)
        {
            // connect randomly and make sure all paths connect to all exits
            if (c_allpaths.Checked)
            {
                c_nologic.Checked = false;
            }
        }

        //Enemies
        private void cm_enemies_CheckedChanged(object sender, EventArgs e)
        {
            //          random enemies selection
            if (cm_enemies.Checked)
            {
                c_esteals.Enabled = true;
                c_edrops.Enabled = true;
                if (!c_esteals.Checked & !c_edrops.Checked)
                {
                    c_esteals.Checked = true;
                    c_edrops.Checked = true;
                }
                if (c_esteals.Checked)
                {
                    c_es1.Enabled = true;
                    c_es2.Enabled = true;
                    c_es3.Enabled = true;
                    c_es4.Enabled = true;
                }
                if (c_edrops.Checked)
                {
                    c_ed1.Enabled = true;
                    c_ed2.Enabled = true;
                    c_ed3.Enabled = true;
                    c_ed4.Enabled = true;
                }
            }
            else if (!cm_enemies.Checked)
            {
                c_esteals.Enabled = false;
                c_edrops.Enabled = false;
                c_es1.Enabled = false;
                c_es2.Enabled = false;
                c_es3.Enabled = false;
                c_es4.Enabled = false;
                c_ed1.Enabled = false;
                c_ed2.Enabled = false;
                c_ed3.Enabled = false;
                c_ed4.Enabled = false;
            }

        }
        private void c_esteals_CheckedChanged(object sender, EventArgs e)
        {
            //      random steals selection
            if (c_esteals.Checked)
            {
                c_es1.Enabled = true;
                c_es2.Enabled = true;
                c_es3.Enabled = true;
                c_es4.Enabled = true;
                if (!c_es1.Checked & !c_es2.Checked & !c_es3.Checked & !c_es4.Checked)
                {
                    c_es3.Checked = true;
                    c_es4.Checked = true;
                }
            }
            else if (!c_esteals.Checked )
            {
                c_es1.Enabled = false;
                c_es2.Enabled = false;
                c_es3.Enabled = false;
                c_es4.Enabled = false;
            }
        }
        private void c_edrops_CheckedChanged(object sender, EventArgs e)
        {
            //      random drops selection
            if (c_edrops.Checked)
            {
                c_ed1.Enabled = true;
                c_ed2.Enabled = true;
                c_ed3.Enabled = true;
                c_ed4.Enabled = true;
                if (!c_ed1.Checked & !c_ed2.Checked & !c_ed3.Checked & !c_ed4.Checked)
                {
                    c_ed3.Checked = true;
                    c_ed4.Checked = true;
                }
            }
            else if (!c_edrops.Checked)
            {
                c_ed1.Enabled = false;
                c_ed2.Enabled = false;
                c_ed3.Enabled = false;
                c_ed4.Enabled = false;
            }
        }

        private void c_es1_CheckedChanged(object sender, EventArgs e)
        {
            // steal slot 1
        }
        private void c_es2_CheckedChanged(object sender, EventArgs e)
        {
            // steal slot 2
        }
        private void c_es3_CheckedChanged(object sender, EventArgs e)
        {
            // steal slot 3
        }
        private void c_es4_CheckedChanged(object sender, EventArgs e)
        {
            // steal slot 4
        }

        private void c_ed1_CheckedChanged(object sender, EventArgs e)
        {
            // drop slot 1
        }
        private void c_ed2_CheckedChanged(object sender, EventArgs e)
        {
            // drop slot 2
        }
        private void c_ed3_CheckedChanged(object sender, EventArgs e)
        {
            // drop slot 3
        }
        private void c_ed4_CheckedChanged(object sender, EventArgs e)
        {
            // drop slot 4
        }

        //GUI changes done

        //New logic
        
        private void Bytes_IO()
        {
            //check if bak exists, if not need to copy the bin as bak, this should ensure I only restore data to before i touched it
            //      string binloc has binary location
            //backup entrances
            
            
            
            if (!File.Exists(binloc+ "\\p0data7.bak"))
            {
                File.Copy(binloc + "\\p0data7.bin", binloc + "\\p0data7.bak");
            }
            //backup enemies
            if (!File.Exists(binloc + "\\p0data2.bak"))
            {
                File.Copy(binloc + "\\p0data2.bin", binloc + "\\p0data2.bak");
            }
            
            
            //need to read bin

            ba_p0data7 = File.ReadAllBytes(binloc + "\\p0data7.bin");
            //ba_p2 = File.ReadAllBytes(binloc + "\\p0data2.bin");

            //create address lists, check out pointers or just the address offset of the pattern


            //      using ba_p7 array, iterate every "pattern_length here". check for pattern
            //      there wont be a way to know if the chunk seached is aligned with the pattern size.
            //      so we cant chunk it and search each for match. I need to seach one byte at a time and if match first pattern byte then check for further matches.
            //      so reguardless, we need a loop, check for first byte match, then loop again, check second. so on and so forth.

            //  preload and set var
            //      fd0005?x??05d9157d?x??
            //      "fd0005" + x[0000-ffff] + "05d9167d" + x[0000-ffff]

            //  set entrance, load field()
            //      05D8027D?y??2C7F2B00?x??
            //      "05d8027d" + y[0000-ffff] + "2c7f2b00" + x[0000-ffff]
            //each set should contain both patterns and only in this order.
            //this means the range is setup to preload a field and does exit, so it is an exit field.
            //any other ones, im not sure yet.


            //if (ba_p7[i] == pattern[0])
            //richTextBox_output.Text = richTextBox_output.Text + "\n" + BitConverter.(ba_p7[i]).Replace("-", "");

            //this stacked for loop lets us match certain parts with any bytes in between, i.e. a variable pattern search

            byte[] pattern1a_preload = new byte[] { 253, 0, 5 };
            // var zone1a
            byte[] pattern1b_setvar = new byte[] { 5, 217, 21, 125 };
            //var zone 1b
            byte[] pattern2a_setent = new byte[] { 5, 216, 2, 125 };
            //var zone 2a
            byte[] pattern2b_field = new byte[] { 44, 127, 43, 0 };
            //var zone 2b
            
            
            //length of dataset
            //1 for fieldID (known)
            //1 for rangeID (known)
            //pairs: 4 for addresses (unknown), 4 for values(known)
            //

            //for each data point
            int fieldID, rangeID, add1, val1, add2, val2, add3, val3, add4, val4;
            
            int[] i_datapoint = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

            //now we need a dataset array of datapoints this is the one we dont know the length of yet
            //but I could get if we absolutely need it. oof

            /*int[][] i_dataset =
            {
                i_datapoint, i_datapoint
            
            };*/

            
            //unsure length of total data to store atm.
            List<byte> l_byte_stor = new List<byte>();

            //array lists
            // Insert(index, data) inbetwee, after index, 
            //

            //shit we might need the length of our expected dataset..

            List<int[][]> l_dataset = new List<int[][]>() { };
            //nvm, we can use a list of int[]s that way we can continue without knowing max length.


            //iterate through and when encounter, #HW fileid 50 // Prima Vista/Cargo Room, we know which field we are on.
            //log this data as the current fieldID, use to create dataset.
            //everytime we are scanning and we encounter a new pattern match for fieldID, we change the current fieldID value
            //and continue scanning.

            byte[] ba_byte_stor = new byte[] {};

            for (int i = 0; i < ba_p0data7.Length; i++)//match first byte
            {
                //  preload and set var
                //      fd      0005?x??05d9157d?x??
                //  set entrance, load field()
                //      05      D8027D?y??2C7F2B00?x??
                
                if (ba_p0data7[i] == pattern1a_preload[0])
                {
                    int k = i;
                    for (int j = 0; j < pattern1a_preload.Length; j++)
                    {
                        
                        if (!(ba_p0data7[k] == pattern1a_preload[j]))
                        {
                            break;
                        } else
                        {

                            k++;
                        }
                        
                    }
                    i = k;
                }

                for ()//match rest of known pattern
                {
                    //  preload and set var
                    //      00 05        ?x??05d9157d?x??
                    //  set entrance, load field()
                    //      D8 02 7D      ?y??2C7F2B00?x??

                    for ()//match any bytes, as this is the variable zone
                    {
                        //  preload and set var
                        //      ?x ??        05d9157d?x??
                        //  set entrance, load field()
                        //      ?y ??        2C7F2B00?x??

                        for ()//match more/rest of known pattern
                        {
                            //  preload and set var
                            //      05 d9 15 7d        ?x??
                            //  set entrance, load field()
                            //      2C 7F 2B 00        ?x??

                            for ()//match any bytes, as this is also variable zone
                            {
                                //  preload and set var
                                //      ?x??
                                //  set entrance, load field()
                                //      ?x??

                                /*for ()//match anymore known data at the end of the pattern
                                {
                                    //keep this for later if needed.

                                }*/

                            }

                        }

                    }

                }

            }






            





            //write bin

            //

        }




    }
}