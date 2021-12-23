using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;

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
            richTextBox_output.SelectionStart = richTextBox_output.Text.Length;
            richTextBox_output.ScrollToCaret();
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
                //  SPLITTER
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
                //  SEED RNG
                for (int i = 0; i < data_arr.Length; i++)
                {
                    Random rnd = new Random(data_arr[i]);
                    data_str = data_str + rnd.Next(4269, 42069);
                }
                //string data_str has output data

                Compressor();
            }
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

        private void textBox_seed_TextChanged(object sender, EventArgs e)
        {

        }

        private void cm_treasure_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cm_stiltzkin_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void c_randbag_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void c_order_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void c_exicons_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void c_chests_CheckedChanged(object sender, EventArgs e)
        {

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

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            //richTextBox1.SelectionStart = richTextBox_output.Text.Length;
            //richTextBox1.ScrollToCaret();
        }

        private void Manual_search()
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.Description = "locate ffix";
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
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
                /*c_allpaths.Enabled = true;
                c_nologic.Enabled = true;
                if (!c_nologic.Checked & !c_allpaths.Checked)
                {
                    c_allpaths.Checked = true;
                }*/
            }
            else if (!cm_entrances.Checked)
            {
                /*c_allpaths.Enabled = false;
                c_nologic.Enabled = false;*/
            }
        }
        private void c_nologic_CheckedChanged(object sender, EventArgs e)
        {
            // connect randomly with no gaurenteed completion path
            /*if (c_nologic.Checked)
            {
                c_allpaths.Checked = false;
            }*/
        }
        private void c_allpaths_CheckedChanged(object sender, EventArgs e)
        {
            // connect randomly and make sure all paths connect to all exits
            /*if (c_allpaths.Checked)
            {
                c_nologic.Checked = false;
            }*/
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
                    //c_es2.Enabled = true;
                    //c_es3.Enabled = true;
                    c_es4.Enabled = true;
                }
                if (c_edrops.Checked)
                {
                    c_ed1.Enabled = true;
                    //c_ed2.Enabled = true;
                    //c_ed3.Enabled = true;
                    c_ed4.Enabled = true;
                }
            }
            else if (!cm_enemies.Checked)
            {
                c_esteals.Enabled = false;
                c_edrops.Enabled = false;
                c_es1.Enabled = false;
                //c_es2.Enabled = false;
                //c_es3.Enabled = false;
                c_es4.Enabled = false;
                c_ed1.Enabled = false;
                //c_ed2.Enabled = false;
                //c_ed3.Enabled = false;
                c_ed4.Enabled = false;
            }

        }
        private void c_esteals_CheckedChanged(object sender, EventArgs e)
        {
            //      random steals selection
            if (c_esteals.Checked)
            {
                c_es1.Enabled = true;
                //c_es2.Enabled = true;
                //c_es3.Enabled = true;
                c_es4.Enabled = true;
                if (!c_es1.Checked & /*!c_es2.Checked & !c_es3.Checked &*/ !c_es4.Checked)
                {
                    //c_es3.Checked = true;
                    c_es4.Checked = true;
                }
            }
            else if (!c_esteals.Checked )
            {
                c_es1.Enabled = false;
                //c_es2.Enabled = false;
                //c_es3.Enabled = false;
                c_es4.Enabled = false;
            }
        }
        private void c_edrops_CheckedChanged(object sender, EventArgs e)
        {
            //      random drops selection
            if (c_edrops.Checked)
            {
                c_ed1.Enabled = true;
                //c_ed2.Enabled = true;
                //c_ed3.Enabled = true;
                c_ed4.Enabled = true;
                if (!c_ed1.Checked & /*!c_ed2.Checked & !c_ed3.Checked &*/ !c_ed4.Checked)
                {
                    //c_ed3.Checked = true;
                    c_ed4.Checked = true;
                }
            }
            else if (!c_edrops.Checked)
            {
                c_ed1.Enabled = false;
                //c_ed2.Enabled = false;
                //c_ed3.Enabled = false;
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
            if (!File.Exists(binloc + "\\p0data7.bak")) 
            { File.Copy(binloc + "\\p0data7.bin", binloc + "\\p0data7.bak"); }
            if (!File.Exists(binloc + "\\p0data2.bak")) 
            { File.Copy(binloc + "\\p0data2.bin", binloc + "\\p0data2.bak"); }
            ba_p0data7 = File.ReadAllBytes(binloc + "\\p0data7.bin");
            //ba_p2 = File.ReadAllBytes(binloc + "\\p0data2.bin");

            Dictionary<int, string> ff9Fields = new Dictionary<int, string>
        {
            { 50, "EVT_ALEX1_TS_CARGO_0" }, { 51, "EVT_ALEX1_TS_CARGO_3" }, { 52, "EVT_ALEX1_TS_MEET_0" }, { 53, "EVT_ALEX1_TS_MEET_1" }, { 54, "EVT_ALEX1_TS_UPPER" }, { 55, "EVT_ALEX1_TS_ORCHE" }, { 56, "EVT_ALEX1_TS_ENGIN_UP" }, { 57, "EVT_ALEX1_TS_ENGIN" }, { 58, "EVT_ALEX1_TS_STORAGE" }, { 59, "EVT_ALEX1_TS_UNDER" },
            { 60, "EVT_ALEX1_TS_COCKPIT" }, { 61, "EVT_ALEX1_TS_STAGE_BK" }, { 62, "EVT_ALEX1_TS_STAGE_1A" }, { 63, "EVT_ALEX1_TS_STAGE_1B" }, { 64, "EVT_ALEX1_TS_SWD_BTL" }, { 65, "EVT_ALEX1_TS_STAGE_2A" }, { 66, "EVT_ALEX1_TS_STAGE_2B" }, { 67, "EVT_ALEX1_TS_STAGE_2C" }, { 68, "EVT_ALEX1_TS_ROYAL_1" }, { 69, "EVT_ALEX1_TS_ROYAL_2" },
            { 70, "EVT_ALEX1_TS_OPENING" }, { 100, "EVT_ALEX1_AT_STREET_A" }, { 101, "EVT_ALEX1_AT_STREET_B" }, { 102, "EVT_ALEX1_AT_STREET_C" }, { 103, "EVT_ALEX1_AT_CENTER" }, { 104, "EVT_ALEX1_AT_TICKET" }, { 105, "EVT_ALEX1_AT_BACK_STR" }, { 106, "EVT_ALEX1_AT_TSS" }, { 107, "EVT_ALEX1_AT_GATE" }, { 108, "EVT_ALEX1_AT_ITEM" },
            { 109, "EVT_ALEX1_AT_WEAPON" }, { 110, "EVT_ALEX1_AT_MAGIC" }, { 111, "EVT_ALEX1_AT_INN" }, { 112, "EVT_ALEX1_AT_PUB" }, { 113, "EVT_ALEX1_AT_HOUSE_1" }, { 114, "EVT_ALEX1_AT_HOUSE_2" }, { 115, "EVT_ALEX1_AT_SENTOU" }, { 116, "EVT_ALEX1_AT_ROOF" }, { 117, "EVT_ALEX1_AT_RUBY" }, { 150, "EVT_ALEX1_AC_GUARD" },
            { 151, "EVT_ALEX1_AC_SEAT_R" }, { 153, "EVT_ALEX1_AC_H2F" }, { 154, "EVT_ALEX1_AC_ENT_2F" }, { 155, "EVT_ALEX1_AC_LIB" }, { 156, "EVT_ALEX1_AC_PARTY" }, { 157, "EVT_ALEX1_AC_KITCHEN" }, { 158, "EVT_ALEX1_AC_OUT_E" }, { 159, "EVT_ALEX1_AC_OUT_C" }, { 160, "EVT_ALEX1_AC_OUT_L" }, { 161, "EVT_ALEX1_AC_OUT_R" },
            { 162, "EVT_ALEX1_AC_TOWER_L1" }, { 163, "EVT_ALEX1_AC_TOWER_L2" }, { 164, "EVT_ALEX1_AC_TOWER_L3" }, { 165, "EVT_ALEX1_AC_TOWER_L4" }, { 166, "EVT_ALEX1_AC_TOWER_L5" }, { 167, "EVT_ALEX1_AC_LIB_2" }, { 200, "EVT_DOWNSHIP_BT_CCD_0" }, { 201, "EVT_DOWNSHIP_BT_CCP_0" }, { 202, "EVT_DOWNSHIP_BT_CCR_0" },
            { 203, "EVT_DOWNSHIP_BT_CMR_0" }, { 204, "EVT_DOWNSHIP_BT_CNR_0" }, { 205, "EVT_DOWNSHIP_BT_CUS_0" }, { 206, "EVT_DOWNSHIP_BT_FRT_0" }, { 207, "EVT_DOWNSHIP_BT_GTR_0" }, { 208, "EVT_DOWNSHIP_BT_STR_0" }, { 209, "EVT_DOWNSHIP_BT_CF1_0" }, { 250, "EVT_EFOREST1_EF_ENT_0" }, { 251, "EVT_EFOREST1_EF_FR1_0" },
            { 252, "EVT_EFOREST1_EF_FR2_0" }, { 253, "EVT_EFOREST1_EF_HEL_0" }, { 254, "EVT_EFOREST1_EF_NSP_0" }, { 255, "EVT_EFOREST1_EF_NRV_0" }, { 256, "EVT_EFOREST1_EF_FR3_0" }, { 257, "EVT_EFOREST1_EF_DEP_0" }, { 258, "EVT_EFOREST1_EF_FR4_0" }, { 259, "EVT_EFOREST1_EF_FR5_0" }, { 260, "EVT_EFOREST1_EF_FR6_0" },
            { 261, "EVT_EFOREST1_EF_EXT_0" }, { 262, "EVT_EFOREST1_EF_TNT_0" }, { 300, "EVT_ICE_IC_ENT_0" }, { 301, "EVT_ICE_IC_STP_0" }, { 302, "EVT_ICE_IC_TER_0" }, { 303, "EVT_ICE_IC_JMP_0" }, { 304, "EVT_ICE_IC_BRI_0" }, { 305, "EVT_ICE_IC_MEN_0" }, { 306, "EVT_ICE_IC_MRM_0" }, { 307, "EVT_ICE_IC_STA_0" },
            { 308, "EVT_ICE_IC_WBF_0" }, { 309, "EVT_ICE_IC_WAF_0" }, { 310, "EVT_ICE_IC_CAF_0" }, { 311, "EVT_ICE_IC_XIT_0" }, { 312, "EVT_ICE_DL_VIW_0" }, { 350, "EVT_DALI_V_DL_WHL" }, { 351, "EVT_DALI_V_DL_INN0" }, { 352, "EVT_DALI_V_DL_INN1" }, { 353, "EVT_DALI_V_DL_MYR" }, { 354, "EVT_DALI_V_DL_SHP" },
            { 355, "EVT_DALI_V_DL_BAR" }, { 356, "EVT_DALI_V_DL_WMS0" }, { 357, "EVT_DALI_V_DL_WMS1" }, { 358, "EVT_DALI_V_DL_FWM" }, { 359, "EVT_DALI_V_DL_ENT" }, { 400, "EVT_DALI_F_UF_BLK_0" }, { 401, "EVT_DALI_F_UF_BLK_1" }, { 402, "EVT_DALI_F_UF_DEV_0" }, { 403, "EVT_DALI_F_UF_EGG_0" }, { 404, "EVT_DALI_F_UF_ENT_0" },
            { 405, "EVT_DALI_F_UF_MST_0" }, { 406, "EVT_DALI_F_UF_RST_0" }, { 407, "EVT_DALI_F_UF_STO_0" }, { 408, "EVT_DALI_F_UF_STO_1" }, { 450, "EVT_DALI_A_AP_FID_0" }, { 451, "EVT_DALI_A_AP_AIR_0" }, { 452, "EVT_DALI_A_AP_AIR_1" }, { 453, "EVT_DALI_A_AP_AIR_2" }, { 454, "EVT_DALI_A_AP_AIR_3" }, { 455, "EVT_DALI_A_AP_WTE_0" },
            { 456, "EVT_DALI_A_AP_WTE_1" }, { 457, "EVT_DALI_A_AP_WTI_0" }, { 500, "EVT_CARGO_CA_DCK_1" }, { 501, "EVT_CARGO_CA_DCK_W" }, { 502, "EVT_CARGO_CA_COP_0" }, { 503, "EVT_CARGO_CA_COP_1" }, { 504, "EVT_CARGO_CA_ERM_0" }, { 505, "EVT_CARGO_CA_RDK_0" }, { 506, "EVT_CARGO_FN_DCK_0" }, { 507, "EVT_CARGO_CA_DCK_0" },
            { 550, "EVT_LIND1_TN_LB_GAT_0" }, { 551, "EVT_LIND1_TN_LB_MTM_0" }, { 552, "EVT_LIND1_TN_LB_MST_0" }, { 553, "EVT_LIND1_TN_LB_IN1_0" }, { 554, "EVT_LIND1_TN_LB_IN2_0" }, { 555, "EVT_LIND1_TN_LB_BAZ_0" }, { 556, "EVT_LIND1_TN_LB_ALY_0" }, { 557, "EVT_LIND1_TN_LB_TMP_0" }, { 558, "EVT_LIND1_TN_LB_HUS_0" },
            { 559, "EVT_LIND1_TN_LB_PLZ_0" }, { 560, "EVT_LIND1_TN_LB_ECH_0" }, { 561, "EVT_LIND1_TN_LB_ITM_0" }, { 562, "EVT_LIND1_TN_LB_WPN_0" }, { 563, "EVT_LIND1_TN_LB_STN_0" }, { 564, "EVT_LIND1_TN_LB_ATL_0" }, { 565, "EVT_LIND1_TN_LB_TTL_0" }, { 566, "EVT_LIND1_TN_LB_RFS_0" }, { 567, "EVT_LIND1_TN_LB_HIG_0" },
            { 568, "EVT_LIND1_TN_LB_FTH_0" }, { 569, "EVT_LIND1_TN_LB_AET_0" }, { 570, "EVT_LIND1_TN_LB_ROD_0" }, { 571, "EVT_LIND1_TN_LB_SLN_0" }, { 572, "EVT_LIND1_TN_LB_FTM_0" }, { 573, "EVT_LIND1_TN_LB_LDH_0" }, { 574, "EVT_LIND1_TN_LB_WGN_0" }, { 575, "EVT_LIND1_TN_LB_CAG_0" }, { 576, "EVT_LIND1_TN_LB_SHT_0" },
            { 600, "EVT_LIND1_CS_LB_CFR_0" }, { 601, "EVT_LIND1_CS_LB_LPF_0" }, { 602, "EVT_LIND1_CS_LB_EXB_0" }, { 603, "EVT_LIND1_CS_LB_LLP_0" }, { 604, "EVT_LIND1_CS_LB_POT_0" }, { 605, "EVT_LIND1_CS_LB_PLA_0" }, { 606, "EVT_LIND1_CS_LB_TSP_0" }, { 607, "EVT_LIND1_CS_LB_HNG_0" }, { 608, "EVT_LIND1_CS_LB_SHB_0" },
            { 609, "EVT_LIND1_CS_LB_BRG_0" }, { 610, "EVT_LIND1_CS_LB_CTM_0" }, { 611, "EVT_LIND1_CS_LB_GRM_0" }, { 612, "EVT_LIND1_CS_LB_MHW_0" }, { 613, "EVT_LIND1_CS_LB_HAL_0" }, { 614, "EVT_LIND1_CS_LB_ASD_0" }, { 615, "EVT_LIND1_CS_LB_OBS_0" }, { 616, "EVT_LIND1_CS_LB_LPR_0" }, { 617, "EVT_LIND1_CS_LB_UHW_0" },
            { 618, "EVT_LIND1_CS_LB_MET_0" }, { 619, "EVT_LIND1_CS_LB_HFC_0" }, { 620, "EVT_LIND1_CS_LB_LFT_0" }, { 650, "EVT_KUINA_KM_ENT_0" }, { 651, "EVT_KUINA_KM_DWN_0" }, { 652, "EVT_KUINA_KM_DWN_1" }, { 653, "EVT_KUINA_KM_DWN_2" }, { 654, "EVT_KUINA_KM_DWN_3" }, { 655, "EVT_KUINA_KM_RED_0" }, { 657, "EVT_KUINA_KM_SWP_1" },
            { 660, "EVT_KUINA_KM_THE_0" }, { 661, "EVT_KUINA_KM_THI_0" }, { 701, "EVT_GIZ_ENTER_1" }, { 702, "EVT_GIZ_CAVERN_0" }, { 703, "EVT_GIZ_CAVERN_1" }, { 704, "EVT_GIZ_BELL_0" }, { 705, "EVT_GIZ_BELL_1" }, { 706, "EVT_GIZ_TO_WORLD" }, { 707, "EVT_GIZ_BOSS" }, { 750, "EVT_BURMECIA_ENTER" }, { 751, "EVT_BURMECIA_TOWN_0" }, { 752, "EVT_BURMECIA_TOWN_1" },
            { 753, "EVT_BURMECIA_TOWN_2" }, { 754, "EVT_BURMECIA_HOUSE_0" }, { 755, "EVT_BURMECIA_HOUSE_1" }, { 756, "EVT_BURMECIA_HOUSE_2" }, { 757, "EVT_BURMECIA_HOUSE_3" }, { 758, "EVT_BURMECIA_PATH" }, { 759, "EVT_BURMECIA_PATIO_0" }, { 760, "EVT_BURMECIA_PATIO_1" }, { 761, "EVT_BURMECIA_PATIO_2" }, { 762, "EVT_BURMECIA_PATIO_3" },
            { 763, "EVT_BURMECIA_SQUARE_0" }, { 764, "EVT_BURMECIA_SQUARE_1" }, { 765, "EVT_BURMECIA_SQUARE_2" }, { 766, "EVT_BURMECIA_PALACE_0" }, { 767, "EVT_BURMECIA_PALACE_1" }, { 768, "EVT_BURMECIA_PALACE_2" }, { 800, "EVT_GATE_S_SG_RND_0" }, { 801, "EVT_GATE_S_SG_RND_1" }, { 802, "EVT_GATE_S_SG_RND_3" }, { 806, "EVT_GATE_S_SG_ALX_3" },
            { 813, "EVT_GATE_S_SG_TRN_0" }, { 814, "EVT_GATE_S_SG_TRN_1" }, { 850, "EVT_GATE_N_NG_BDA_0" }, { 851, "EVT_GATE_N_NG_BMA_0" }, { 852, "EVT_GATE_N_NG_MDA_0" }, { 1256, "EVT_PINA_PR_ENT_1" }, { 2950, "EVT_CHOCO_CH_FST_0" }, { 2953, "EVT_CHOCO_CH_DWL_0" }, { 152, "EVT_ALEX1_AC_SEAT_N" }, { 656, "EVT_KUINA_KM_SWP_0" },
            { 658, "EVT_KUINA_KM_SWP_2" }, { 659, "EVT_KUINA_KM_SWP_3" }, { 662, "EVT_KUINA_KM_FET_0" }, { 663, "EVT_KUINA_KM_FET_1" }, { 769, "EVT_BURMECIA_PALACE_3" }, { 803, "EVT_GATE_S_SG_ALX_0" }, { 804, "EVT_GATE_S_SG_ALX_1" }, { 805, "EVT_GATE_S_SG_ALX_2" }, { 807, "EVT_GATE_S_SG_ALX_4" }, { 809, "EVT_GATE_S_SG_TOP_0" },
            { 810, "EVT_GATE_S_SG_TOP_1" }, { 811, "EVT_GATE_S_SG_TOP_2" }, { 812, "EVT_GATE_S_SG_TOP_3" }, { 815, "EVT_GATE_S_SG_TRN_2" }, { 816, "EVT_GATE_S_SG_TRN_3" }, { 900, "EVT_TRENO1_TR_BAR_0" }, { 901, "EVT_TRENO1_TR_BHE_0" }, { 902, "EVT_TRENO1_TR_BHM_0" }, { 903, "EVT_TRENO1_TR_BTA_0" }, { 904, "EVT_TRENO1_TR_EKH_0" },
            { 905, "EVT_TRENO1_TR_FBH_0" }, { 906, "EVT_TRENO1_TR_FKH_0" }, { 907, "EVT_TRENO1_TR_FQH_0" }, { 908, "EVT_TRENO1_TR_GAT_0" }, { 909, "EVT_TRENO1_TR_KHA_0" }, { 910, "EVT_TRENO1_TR_KHM_0" }, { 911, "EVT_TRENO1_TR_QHM_0" }, { 912, "EVT_TRENO1_TR_RES_0" }, { 913, "EVT_TRENO1_TR_TMS_0" }, { 914, "EVT_TRENO1_TR_TOT_0" },
            { 915, "EVT_TRENO1_TR_WBH_0" }, { 916, "EVT_TRENO1_TR_WHF_0" }, { 930, "EVT_TOT_TR_TOT_1" }, { 931, "EVT_TOT_TR_WAT_0" }, { 932, "EVT_TOT_AC_LBR_A" }, { 950, "EVT_GARGAN_GR_CEN_0" }, { 951, "EVT_GARGAN_GR_LEF_0" }, { 952, "EVT_GARGAN_GR_GBA_0" }, { 953, "EVT_GARGAN_GR_REN_0" }, { 954, "EVT_GARGAN_GR_GGT_0" },
            { 955, "EVT_GARGAN_GR_GGT_1" }, { 956, "EVT_GARGAN_GR_ALX_0" }, { 957, "EVT_GARGAN_GR_CON_0" }, { 1000, "EVT_CLEYRA1_ENT_0" }, { 1001, "EVT_CLEYRA1_ENT_1" }, { 1002, "EVT_CLEYRA1_ENT_2" }, { 1003, "EVT_CLEYRA1_DUN_00" }, { 1004, "EVT_CLEYRA1_DUN_01A" }, { 1005, "EVT_CLEYRA1_DUN_01B" }, { 1006, "EVT_CLEYRA1_DUN_03" },
            { 1007, "EVT_CLEYRA1_DUN_04" }, { 1008, "EVT_CLEYRA1_DUN_05" }, { 1009, "EVT_CLEYRA1_DUN_06" }, { 1010, "EVT_CLEYRA1_DUN_07" }, { 1011, "EVT_CLEYRA1_DUN_08" }, { 1012, "EVT_CLEYRA1_DUN_09" }, { 1013, "EVT_CLEYRA1_DUN_10" }, { 1014, "EVT_CLEYRA1_DUN_11" }, { 1015, "EVT_CLEYRA1_DUN_12A" }, { 1016, "EVT_CLEYRA1_DUN_12B" },
            { 1017, "EVT_CLEYRA1_DUN_13A" }, { 1018, "EVT_CLEYRA1_DUN_13B" }, { 1050, "EVT_CLEYRA2_LADDER" }, { 1051, "EVT_CLEYRA2_ENTER" }, { 1052, "EVT_CLEYRA2_ANTRION" }, { 1053, "EVT_CLEYRA2_WATER" }, { 1054, "EVT_CLEYRA2_WIND" }, { 1055, "EVT_CLEYRA2_TOWN" }, { 1056, "EVT_CLEYRA2_INN" }, { 1057, "EVT_CLEYRA2_VISTA" },
            { 1058, "EVT_CLEYRA2_CATHE_0" }, { 1059, "EVT_CLEYRA2_CATHE_1" }, { 1060, "EVT_CLEYRA2_CATHE_2" }, { 1100, "EVT_CLEYRA3_LADDER" }, { 1101, "EVT_CLEYRA3_ENTER" }, { 1102, "EVT_CLEYRA3_ANTRION" }, { 1103, "EVT_CLEYRA3_WATER" }, { 1104, "EVT_CLEYRA3_WIND" }, { 1105, "EVT_CLEYRA3_TOWN" }, { 1106, "EVT_CLEYRA3_INN" },
            { 1107, "EVT_CLEYRA3_VISTA" }, { 1108, "EVT_CLEYRA3_CATHE_0" }, { 1109, "EVT_CLEYRA3_CATHE_1" }, { 1110, "EVT_CLEYRA3_CATHE_2" }, { 1150, "EVT_ROSE_DECK" }, { 1151, "EVT_ROSE_CABIN_1" }, { 1152, "EVT_ROSE_CABIN_2" }, { 1153, "EVT_ROSE_BRIDGE" }, { 1200, "EVT_ALEX2_AC_SEAT_R" }, { 1201, "EVT_ALEX2_AC_S_ROOM" },
            { 1202, "EVT_ALEX2_AC_STAIR_A" }, { 1203, "EVT_ALEX2_AC_STAIR_B" }, { 1204, "EVT_ALEX2_AC_STAIR_C" }, { 1205, "EVT_ALEX2_AC_SUMMON_A" }, { 1206, "EVT_ALEX2_AC_QUEEN" }, { 1207, "EVT_ALEX2_AC_PRINCESS" }, { 1208, "EVT_ALEX2_AC_PRISON_A" }, { 1209, "EVT_ALEX2_AC_PRISON_B" }, { 1210, "EVT_ALEX2_AC_TOWER_L1" },
            { 1211, "EVT_ALEX2_AC_TOWER_R1" }, { 1212, "EVT_ALEX2_AC_TOWER_R2" }, { 1213, "EVT_ALEX2_AC_GUARD" }, { 1214, "EVT_ALEX2_AC_H2F" }, { 1215, "EVT_ALEX2_AC_ENT_2F" }, { 1216, "EVT_ALEX2_AC_LIB" }, { 1217, "EVT_ALEX2_AC_PARTY" }, { 1218, "EVT_ALEX2_AC_KITCHEN" }, { 1219, "EVT_ALEX2_AC_OUT_E" }, { 1220, "EVT_ALEX2_AC_OUT_C" },
            { 1221, "EVT_ALEX2_AC_OUT_L" }, { 1222, "EVT_ALEX2_AC_OUT_R" }, { 1223, "EVT_ALEX2_AC_QUEEN_2" }, { 1224, "EVT_ALEX2_AC_R_HALL" }, { 1225, "EVT_ALEX2_AC_QUEEN_3" }, { 1226, "EVT_ALEX2_AC_LIB_2" }, { 1227, "EVT_ALEX2_AC_SWD_BTL" }, { 1250, "EVT_PINA_AC_RST_0" }, { 1251, "EVT_PINA_PR_GAL_0" }, { 1252, "EVT_PINA_PR_PW1_0" },
            { 1253, "EVT_PINA_PR_PW2_0" }, { 1254, "EVT_PINA_PR_PW3_0" }, { 1255, "EVT_PINA_PR_ENT_0" }, { 1300, "EVT_LIND2_TN_LB_GAT_0" }, { 1301, "EVT_LIND2_TN_LB_MTM_0" }, { 1302, "EVT_LIND2_TN_LB_MST_0" }, { 1303, "EVT_LIND2_TN_LB_IN1_0" }, { 1304, "EVT_LIND2_TN_LB_IN2_0" }, { 1305, "EVT_LIND2_TN_LB_BAZ_0" },
            { 1306, "EVT_LIND2_TN_LB_HUS_0" }, { 1307, "EVT_LIND2_TN_LB_PLZ_0" }, { 1308, "EVT_LIND2_TN_LB_ECH_0" }, { 1309, "EVT_LIND2_TN_LB_WPN_0" }, { 1310, "EVT_LIND2_TN_LB_STN_0" }, { 1311, "EVT_LIND2_TN_LB_ATL_0" }, { 1312, "EVT_LIND2_TN_LB_TTL_0" }, { 1313, "EVT_LIND2_TN_LB_RFS_0" }, { 1314, "EVT_LIND2_TN_LB_HIG_0" },
            { 1315, "EVT_LIND2_TN_LB_OMG_W" }, { 1350, "EVT_LIND2_CS_LB_CFR_0" }, { 1351, "EVT_LIND2_CS_LB_LPF_0" }, { 1352, "EVT_LIND2_CS_LB_EXB_0" }, { 1353, "EVT_LIND2_CS_LB_LLP_0" }, { 1354, "EVT_LIND2_CS_LB_POT_0" }, { 1355, "EVT_LIND2_CS_LB_PLA_0" }, { 1356, "EVT_LIND2_CS_LB_TSP_0" }, { 1357, "EVT_LIND2_CS_LB_HNG_0" },
            { 1358, "EVT_LIND2_CS_LB_SHB_0" }, { 1359, "EVT_LIND2_CS_LB_BRG_0" }, { 1360, "EVT_LIND2_CS_LB_CTM_0" }, { 1361, "EVT_LIND2_CS_LB_GRM_0" }, { 1362, "EVT_LIND2_CS_LB_MHW_0" }, { 1363, "EVT_LIND2_CS_LB_HAL_0" }, { 1364, "EVT_LIND2_CS_LB_ASD_1" }, { 1365, "EVT_LIND2_CS_LB_OBS_0" }, { 1366, "EVT_LIND2_CS_LB_LPR_0" },
            { 1367, "EVT_LIND2_CS_LB_UHW_0" }, { 1368, "EVT_LIND2_CS_LB_MET_0" }, { 1369, "EVT_LIND2_CS_LB_HFC_0" }, { 1370, "EVT_LIND2_CS_LB_LFT_0" }, { 1400, "EVT_FOSSIL_FR_MV1_0" }, { 1401, "EVT_FOSSIL_FR_MV2_0" }, { 1402, "EVT_FOSSIL_FR_MV3_0" }, { 1403, "EVT_FOSSIL_FR_MV4_0" }, { 1404, "EVT_FOSSIL_FR_MV5_0" },
            { 1405, "EVT_FOSSIL_FR_SL1_0" }, { 1406, "EVT_FOSSIL_FR_SL2_0" }, { 1407, "EVT_FOSSIL_FR_SL3_0" }, { 1408, "EVT_FOSSIL_FR_SL4_0" }, { 1409, "EVT_FOSSIL_FR_SL5_0" }, { 1410, "EVT_FOSSIL_FR_SM1_0" }, { 1411, "EVT_FOSSIL_FR_SR1_0" }, { 1412, "EVT_FOSSIL_FR_SR2_0" }, { 1413, "EVT_FOSSIL_FR_SR3_0" }, { 1414, "EVT_FOSSIL_FR_SR4_0" },
            { 1415, "EVT_FOSSIL_FR_SR5_0" }, { 1416, "EVT_FOSSIL_FR_SR6_0" }, { 1417, "EVT_FOSSIL_FR_SR7_0" }, { 1418, "EVT_FOSSIL_FR_DN1_0" }, { 1419, "EVT_FOSSIL_FR_DN2_0" }, { 1420, "EVT_FOSSIL_FR_DN3_0" }, { 1421, "EVT_FOSSIL_FR_DN4_0" }, { 1422, "EVT_FOSSIL_FR_ENT_0" }, { 1423, "EVT_FOSSIL_FR_TRP_0" }, { 1424, "EVT_FOSSIL_FR_SBW_0" },
            { 1425, "EVT_FOSSIL_FR_EXT_0" }, { 1450, "EVT_MAGE_BV_ENT_0" }, { 1451, "EVT_MAGE_BV_ABR_0" }, { 1452, "EVT_MAGE_BV_GVY_0" }, { 1453, "EVT_MAGE_BV_GVY_1" }, { 1454, "EVT_MAGE_BV_INN_0" }, { 1455, "EVT_MAGE_BV_ITS_0" }, { 1456, "EVT_MAGE_BV_WPN_0" }, { 1457, "EVT_MAGE_BV_ORT_0" }, { 1458, "EVT_MAGE_BV_CSK_0" },
            { 1459, "EVT_MAGE_BV_CSI_0" }, { 1460, "EVT_MAGE_BV_ITM_0" }, { 1461, "EVT_MAGE_BV_ZDN_0" }, { 1462, "EVT_MAGE_BV_EVE_0" }, { 1463, "EVT_MAGE_BV_FR1_0" }, { 1464, "EVT_MAGE_BV_FR2_0" }, { 1500, "EVT_PATA_T_CP_ENT_0" }, { 1501, "EVT_PATA_T_CP_GL1_0" }, { 1502, "EVT_PATA_T_CP_GL2_0" }, { 1503, "EVT_PATA_T_CP_STR_0" },
            { 1504, "EVT_PATA_T_CP_EXT_0" }, { 1505, "EVT_PATA_T_CP_SHN_0" }, { 1506, "EVT_PATA_T_CP_ALT_0" }, { 1507, "EVT_PATA_T_CP_STP_0" }, { 1508, "EVT_PATA_T_CP_ROM_0" }, { 1509, "EVT_PATA_T_CP_TOL_0" }, { 1550, "EVT_PATA_M_CM_MP0_0" }, { 1551, "EVT_PATA_M_CM_MP1_0" }, { 1552, "EVT_PATA_M_CM_MP2_0" }, { 1553, "EVT_PATA_M_CM_MP3_0" },
            { 1554, "EVT_PATA_M_CM_MP4_0" }, { 1555, "EVT_PATA_M_CM_MP5_0" }, { 1556, "EVT_PATA_M_CM_MP6_0" }, { 1557, "EVT_PATA_M_CM_MP7_0" }, { 1600, "EVT_SARI1_MS_ENT_0" }, { 1601, "EVT_SARI1_MS_FNT_0" }, { 1602, "EVT_SARI1_MS_EKH_0" }, { 1603, "EVT_SARI1_MS_PMR_0" }, { 1604, "EVT_SARI1_MS_MRR_0" }, { 1605, "EVT_SARI1_MS_MRR_2" },
            { 1606, "EVT_SARI1_MS_EIK_0" }, { 1607, "EVT_SARI1_MS_KTN_0" }, { 1608, "EVT_SARI1_MS_SCR_0" }, { 1609, "EVT_SARI1_MS_SSD_0" }, { 1610, "EVT_SARI1_MS_SSD_1" }, { 1650, "EVT_EVA1_IF_ENT_0" }, { 1651, "EVT_EVA1_IF_PUG_0" }, { 1652, "EVT_EVA1_IF_BAS_0" }, { 1653, "EVT_EVA1_IF_BAS_2" }, { 1654, "EVT_EVA1_IF_MKJ_0" },
            { 1655, "EVT_EVA1_IF_PTS_0" }, { 1656, "EVT_EVA1_IF_PTS_1" }, { 1657, "EVT_EVA1_IF_GGT_0" }, { 1658, "EVT_EVA1_IF_MSD_0" }, { 1659, "EVT_EVA1_IF_CST_0" }, { 1660, "EVT_EVA1_BF_FLS_0" }, { 1661, "EVT_EVA1_BF_DCK_0" }, { 1662, "EVT_EVA1_BF_DCK_1" }, { 1663, "EVT_EVA1_IF_MKJ_1" }, { 1750, "EVT_EVA2_IU_EVE_0" },
            { 1751, "EVT_EVA2_IU_EUG_0" }, { 1752, "EVT_EVA2_IU_EUG_1" }, { 1753, "EVT_EVA2_IU_EUG_2" }, { 1754, "EVT_EVA2_IU_ELV_0" }, { 1755, "EVT_EVA2_IU_SDV_0" }, { 1756, "EVT_EVA2_IU_SDV_1" }, { 1757, "EVT_EVA2_IF_ENT_1" }, { 1758, "EVT_EVA2_IF_PUG_1" }, { 1759, "EVT_EVA2_IF_BAS_1" }, { 1800, "EVT_ALEX3_AC_OHAKA" },
            { 1950, "EVT_KUWAN_QH_CAV_0" }, { 1951, "EVT_KUWAN_QH_HUH_0" }, { 1952, "EVT_KUWAN_QH_HUR_0" }, { 1953, "EVT_KUWAN_QH_LNI_0" }, { 808, "EVT_GATE_S_SG_ALX_5" }, { 853, "EVT_GATE_N_NG_TRA_1" }, { 854, "EVT_GATE_N_NG_BDA_1" }, { 855, "EVT_GATE_N_NG_BMA_1" }, { 856, "EVT_GATE_N_NG_MDA_1" }, { 1801, "EVT_ALEX3_AC_QUEEN" },
            { 1802, "EVT_ALEX3_AC_PRINCESS" }, { 1803, "EVT_ALEX3_AC_GUARD" }, { 1806, "EVT_ALEX3_AC_H2F" }, { 1807, "EVT_ALEX3_AC_ENT_2F" }, { 1808, "EVT_ALEX3_AC_LIB" }, { 1809, "EVT_ALEX3_AC_PARTY" }, { 1810, "EVT_ALEX3_AC_KITCHEN" }, { 1811, "EVT_ALEX3_AC_OUT_E" }, { 1812, "EVT_ALEX3_AC_OUT_C" }, { 1813, "EVT_ALEX3_AC_OUT_L" },
            { 1814, "EVT_ALEX3_AC_OUT_R" }, { 1815, "EVT_ALEX3_AC_OUT_C2" }, { 1816, "EVT_ALEX3_AC_OUT_C9" }, { 1817, "EVT_ALEX3_AC_PORT_A" }, { 1818, "EVT_ALEX3_AC_PORT_B" }, { 1819, "EVT_ALEX3_AC_PORT_C0" }, { 1820, "EVT_ALEX3_AC_TOWER_L1" }, { 1821, "EVT_ALEX3_AC_R_HALL" }, { 1822, "EVT_ALEX3_AC_LIB_2" }, { 1823, "EVT_ALEX3_AC_H2F_2" },
            { 1824, "EVT_ALEX3_AC_SWD_BTL" }, { 1850, "EVT_ALEX3_AT_STREET_A" }, { 1851, "EVT_ALEX3_AT_STREET_B" }, { 1852, "EVT_ALEX3_AT_STREET_C" }, { 1853, "EVT_ALEX3_AT_CENTER" }, { 1854, "EVT_ALEX3_AT_BACK_STR" }, { 1855, "EVT_ALEX3_AT_TSS" }, { 1856, "EVT_ALEX3_AT_GATE" }, { 1857, "EVT_ALEX3_AT_ITEM" }, { 1858, "EVT_ALEX3_AT_WEAPON" },
            { 1859, "EVT_ALEX3_AT_MAGIC" }, { 1860, "EVT_ALEX3_AT_INN" }, { 1861, "EVT_ALEX3_AT_PUB" }, { 1862, "EVT_ALEX3_AT_HOUSE_1" }, { 1863, "EVT_ALEX3_AT_HOUSE_2" }, { 1864, "EVT_ALEX3_AT_RUBY" }, { 1865, "EVT_ALEX3_AT_SENTOU" }, { 1866, "EVT_ALEX3_AT_WHARF" }, { 1900, "EVT_TRENO2_TR_BAR_0" }, { 1901, "EVT_TRENO2_TR_BHE_0" },
            { 1902, "EVT_TRENO2_TR_BHM_0" }, { 1903, "EVT_TRENO2_TR_BTA_0" }, { 1904, "EVT_TRENO2_TR_EKH_0" }, { 1905, "EVT_TRENO2_TR_FBH_0" }, { 1906, "EVT_TRENO2_TR_FKH_0" }, { 1907, "EVT_TRENO2_TR_FQH_0" }, { 1908, "EVT_TRENO2_TR_GAT_0" }, { 1909, "EVT_TRENO2_TR_KHA_0" }, { 1910, "EVT_TRENO2_TR_KHM_0" }, { 1911, "EVT_TRENO2_TR_QHM_0" },
            { 1912, "EVT_TRENO2_TR_RES_0" }, { 1913, "EVT_TRENO2_TR_TMS_0" }, { 1914, "EVT_TRENO2_TR_TOT_0" }, { 1915, "EVT_TRENO2_TR_WBH_0" }, { 1916, "EVT_TRENO2_TR_KHM_1" }, { 2000, "EVT_ALEX4_AC_HILDA2_A" }, { 2001, "EVT_ALEX4_AC_PRINCESS" }, { 2002, "EVT_ALEX4_AC_SAI_A" }, { 2003, "EVT_ALEX4_AC_SAI_B" }, { 2004, "EVT_ALEX4_AC_SAI_C" },
            { 2005, "EVT_ALEX4_AC_SAI_D" }, { 2006, "EVT_ALEX4_AC_SAI_E" }, { 2007, "EVT_ALEX4_AC_SAI_F" }, { 2008, "EVT_ALEX4_AC_SAI_G" }, { 2009, "EVT_ALEX4_AC_SEAT_R" }, { 2050, "EVT_ALEX4_AT_STREET_A" }, { 2051, "EVT_ALEX4_AT_STREET_B" }, { 2052, "EVT_ALEX4_AT_STREET_C" }, { 2053, "EVT_ALEX4_AT_CENTER" }, { 2054, "EVT_ALEX4_AT_GATE" },
            { 2055, "EVT_ALEX4_AT_IV_CTR_1" }, { 2100, "EVT_LIND3_TN_LB_GAT_0" }, { 2101, "EVT_LIND3_TN_LB_MTM_0" }, { 2102, "EVT_LIND3_TN_LB_MST_0" }, { 2103, "EVT_LIND3_TN_LB_IN1_0" }, { 2104, "EVT_LIND3_TN_LB_IN2_0" }, { 2105, "EVT_LIND3_TN_LB_BAZ_0" }, { 2106, "EVT_LIND3_TN_LB_HUS_0" }, { 2107, "EVT_LIND3_TN_LB_PLZ_0" },
            { 2108, "EVT_LIND3_TN_LB_ECH_0" }, { 2109, "EVT_LIND3_TN_LB_WPN_0" }, { 2110, "EVT_LIND3_TN_LB_STN_0" }, { 2111, "EVT_LIND3_TN_LB_ATL_0" }, { 2112, "EVT_LIND3_TN_LB_TTL_0" }, { 2113, "EVT_LIND3_TN_LB_RFS_0" }, { 2114, "EVT_LIND3_TN_LB_HIG_0" }, { 2150, "EVT_LIND3_CS_LB_CFR_0" }, { 2151, "EVT_LIND3_CS_LB_LPF_0" },
            { 2152, "EVT_LIND3_CS_LB_EXB_0" }, { 2153, "EVT_LIND3_CS_LB_LLP_0" }, { 2155, "EVT_LIND3_CS_LB_PLA_0" }, { 2157, "EVT_LIND3_CS_LB_HNG_0" }, { 2158, "EVT_LIND3_CS_LB_SHB_0" }, { 2159, "EVT_LIND3_CS_LB_BRG_0" }, { 2160, "EVT_LIND3_CS_LB_CTM_0" }, { 2161, "EVT_LIND3_CS_LB_GRM_0" }, { 2162, "EVT_LIND3_CS_LB_MHW_0" },
            { 2163, "EVT_LIND3_CS_LB_HAL_0" }, { 2164, "EVT_LIND3_CS_LB_ASD_1" }, { 2167, "EVT_LIND3_CS_LB_LPR_0" }, { 2168, "EVT_LIND3_CS_LB_UHW_0" }, { 2169, "EVT_LIND3_CS_LB_MET_0" }, { 2170, "EVT_LIND3_CS_LB_HFC_0" }, { 2171, "EVT_LIND3_CS_LB_LFT_0" }, { 2172, "EVT_LIND3_CS_LB_OBS_M" }, { 2173, "EVT_LIND3_CS_LB_POT_M" },
            { 2200, "EVT_SUNA_SP_RRO_0" }, { 2201, "EVT_SUNA_SP_RPW_0" }, { 2202, "EVT_SUNA_SP_RPE_0" }, { 2203, "EVT_SUNA_SP_RRM_0" }, { 2204, "EVT_SUNA_SP_RFG_0" }, { 2205, "EVT_SUNA_SP_RSW_0" }, { 2206, "EVT_SUNA_SP_REX_0" }, { 2207, "EVT_SUNA_SP_KPW_0" }, { 2208, "EVT_SUNA_SP_KDR_0" }, { 2209, "EVT_SUNA_SP_KKR_0" },
            { 2211, "EVT_SUNA_SP_KDC_0" }, { 2212, "EVT_SUNA_SP_KEX_0" }, { 2213, "EVT_SUNA_SP_MP1_0" }, { 2214, "EVT_SUNA_SP_MP2_0" }, { 2215, "EVT_SUNA_SP_MT1_0" }, { 2216, "EVT_SUNA_SP_MP3_0" }, { 2217, "EVT_SUNA_SP_MHL_0" }, { 2220, "EVT_SUNA_SP_MT2_0" }, { 2221, "EVT_SUNA_SP_MT3_0" }, { 2222, "EVT_SUNA_SP_MBS_0" },
            { 2250, "EVT_OEIL_UV_EXT_0" }, { 2251, "EVT_OEIL_UV_ENT_0" }, { 2252, "EVT_OEIL_UV_PST_0" }, { 2253, "EVT_OEIL_UV_DP1_0" }, { 2254, "EVT_OEIL_UV_DP2_0" }, { 2255, "EVT_OEIL_UV_ELV_0" }, { 2256, "EVT_OEIL_UV_VSL_0" }, { 2257, "EVT_OEIL_UV_VSL_1" }, { 2258, "EVT_OEIL_UV_RFM_0" }, { 2259, "EVT_OEIL_UV_DEP_0" },
            { 2260, "EVT_OEIL_UV_FDP_0" }, { 2261, "EVT_OEIL_G1_BRG_0" }, { 2300, "EVT_EST_EG_EXT_0" }, { 2301, "EVT_EST_EG_RM1_0" }, { 2302, "EVT_EST_EG_RM1_1" }, { 2303, "EVT_EST_EG_RM2_0" }, { 2304, "EVT_EST_EG_OBS_0" }, { 2305, "EVT_EST_EG_EGV_0" }, { 2350, "EVT_GULUGU_GV_WHL_0" }, { 2351, "EVT_GULUGU_GV_IDO_0" },
            { 2352, "EVT_GULUGU_GV_RM1_0" }, { 2353, "EVT_GULUGU_GV_RM2_0" }, { 2354, "EVT_GULUGU_GV_RM3_0" }, { 2355, "EVT_GULUGU_GV_RM4_0" }, { 2356, "EVT_GULUGU_GV_ATK_0" }, { 2357, "EVT_GULUGU_GV_HLD_0" }, { 2358, "EVT_GULUGU_GV_PW1_0" }, { 2359, "EVT_GULUGU_GV_PW2_0" }, { 2360, "EVT_GULUGU_GV_PW3_0" }, { 2361, "EVT_GULUGU_GV_TIN_0" },
            { 2362, "EVT_GULUGU_GV_MGS_0" }, { 2363, "EVT_GULUGU_GV_PW4_0" }, { 2364, "EVT_GULUGU_GV_MG1_0" }, { 2365, "EVT_GULUGU_GV_MG2_0" }, { 2400, "EVT_ALEX5_AC_PORT_A" }, { 2401, "EVT_ALEX5_AC_PORT_B" }, { 2402, "EVT_ALEX5_AC_PORT_C0" }, { 2403, "EVT_ALEX5_AC_PORT_C1" }, { 2404, "EVT_ALEX5_AC_OUT_C" }, { 2405, "EVT_ALEX5_AC_OUT_L" },
            { 2406, "EVT_ALEX5_AC_TOWER_L1" }, { 2450, "EVT_ALEX5_AT_STREET_A" }, { 2451, "EVT_ALEX5_AT_STREET_C" }, { 2452, "EVT_ALEX5_AT_CENTER" }, { 2453, "EVT_ALEX5_AT_BACK_STR" }, { 2454, "EVT_ALEX5_AT_TSS" }, { 2455, "EVT_ALEX5_AT_INN" }, { 2456, "EVT_ALEX5_AT_SENTOU" }, { 2457, "EVT_ALEX5_AT_RUBY" }, { 2458, "EVT_ALEX5_AT_WHARF" },
            { 2500, "EVT_IPSEN_IP_EXT_0" }, { 2502, "EVT_IPSEN_IP_EN1_0" }, { 2503, "EVT_IPSEN_IP_EN2_0" }, { 2504, "EVT_IPSEN_IP_RM1_0" }, { 2505, "EVT_IPSEN_IP_RM2_0" }, { 2506, "EVT_IPSEN_IP_HL1_0" }, { 2507, "EVT_IPSEN_IP_HL2_0" }, { 2508, "EVT_IPSEN_IP_ST1_0" }, { 2509, "EVT_IPSEN_IP_ST2_0" }, { 2510, "EVT_IPSEN_IP_CNT_0" },
            { 2512, "EVT_IPSEN_IP_CNT_2" }, { 2513, "EVT_IPSEN_IP_SMN_0" }, { 2550, "EVT_SHRINE_ES_ENT_0" }, { 2551, "EVT_SHRINE_ES_ENT_1" }, { 2552, "EVT_SHRINE_ES_SEL_1" }, { 2553, "EVT_SHRINE_ES_SEL_2" }, { 2554, "EVT_SHRINE_ES_TRP_0" }, { 2600, "EVT_TERA_TA_THL_0" }, { 2601, "EVT_TERA_TA_HLD_0" }, { 2602, "EVT_TERA_TA_BST_0" },
            { 2603, "EVT_TERA_TA_PRB_0" }, { 2604, "EVT_TERA_TA_RPB_0" }, { 2605, "EVT_TERA_TA_ROU_0" }, { 2606, "EVT_TERA_TA_ROD_0" }, { 2607, "EVT_TERA_TA_TOB_0" }, { 2608, "EVT_TERA_TA_IVK_0" }, { 2650, "EVT_BAL_BB_STW_0" }, { 2651, "EVT_BAL_BB_SQR_0" }, { 2652, "EVT_BAL_BB_FGT_0" }, { 2653, "EVT_BAL_BB_CDR_0" },
            { 2654, "EVT_BAL_BB_CDL_0" }, { 2655, "EVT_BAL_BB_WPS_0" }, { 2656, "EVT_BAL_BB_GTH_0" }, { 2657, "EVT_BAL_BB_ITS_0" }, { 2658, "EVT_BAL_BB_UDL_0" }, { 2659, "EVT_BAL_BB_BRG_0" }, { 2660, "EVT_BAL_BB_THL_0" }, { 2661, "EVT_BAL_BB_INV_0" }, { 2700, "EVT_PANDEMO_PD_STR_0" }, { 2701, "EVT_PANDEMO_PD_CAO_0" },
            { 2702, "EVT_PANDEMO_PD_EAO_0" }, { 2703, "EVT_PANDEMO_PD_SAO_0" }, { 2704, "EVT_PANDEMO_PD_AOS_0" }, { 2705, "EVT_PANDEMO_PD_BWR_0" }, { 2706, "EVT_PANDEMO_PD_RM1_0" }, { 2707, "EVT_PANDEMO_PD_MCD_0" }, { 2708, "EVT_PANDEMO_PD_RM2_0" }, { 2709, "EVT_PANDEMO_PD_OLB_0" }, { 2710, "EVT_PANDEMO_PD_ULB_0" },
            { 2711, "EVT_PANDEMO_PD_OEV_0" }, { 2712, "EVT_PANDEMO_PD_ELV_0" }, { 2713, "EVT_PANDEMO_PD_EVD_0" }, { 2714, "EVT_PANDEMO_PD_MZM_0" }, { 2715, "EVT_PANDEMO_PD_RTA_0" }, { 2716, "EVT_PANDEMO_PD_RTB_0" }, { 2717, "EVT_PANDEMO_PD_RTC_0" }, { 2718, "EVT_PANDEMO_PD_ALB_0" }, { 2719, "EVT_PANDEMO_PD_PEV_0" },
            { 2720, "EVT_PANDEMO_PD_RTB_1" }, { 2750, "EVT_INV_IV_BRG_0" }, { 2753, "EVT_INV_IV_CXI_0" }, { 2800, "EVT_DAG_DG_ENT_0" }, { 2801, "EVT_DAG_DG_SRH_0" }, { 2802, "EVT_DAG_DG_LFT_0" }, { 2803, "EVT_DAG_DG_2F0_0" }, { 2850, "EVT_HILDA3_G3_BRG_0" }, { 2851, "EVT_HILDA3_G3_EGR_0" }, { 2852, "EVT_HILDA3_G3_DCK_0" },
            { 2853, "EVT_HILDA3_G3_EXT_0" }, { 2854, "EVT_HILDA3_G3_BRG_1" }, { 2855, "EVT_HILDA3_BN_DCR_0" }, { 2856, "EVT_HILDA3_BN_DCR_1" }, { 2951, "EVT_CHOCO_CH_HLG_0" }, { 2952, "EVT_CHOCO_CH_FGD_0" }, { 2954, "EVT_CHOCO_CH_PDS_0" }, { 2955, "EVT_CHOCO_CH_PDS_1" }, { 3100, "EVT_MOGNET_MOG_VER1" }, { 1700, "EVT_SARI2_MS_ENT_1" },
            { 1701, "EVT_SARI2_MS_FNT_1" }, { 1702, "EVT_SARI2_MS_EKH_1" }, { 1703, "EVT_SARI2_MS_PMR_1" }, { 1704, "EVT_SARI2_MS_MRR_1" }, { 1705, "EVT_SARI2_MS_EIK_1" }, { 1706, "EVT_SARI2_MS_KTN_1" }, { 1707, "EVT_SARI2_MS_SCR_1" }, { 1917, "EVT_TRENO2_TR_KHM_2" }, { 2154, "EVT_LIND3_CS_LB_POT_0" }, { 2165, "EVT_LIND3_CS_LB_ASD_2" },
            { 2166, "EVT_LIND3_CS_LB_OBS_0" }, { 2501, "EVT_IPSEN_IP_EXT_1" }, { 2751, "EVT_INV_IV_BRG_1" }, { 2752, "EVT_INV_IV_BRG_2" }, { 2754, "EVT_INV_IV_CTR_0" }, { 2755, "EVT_INV_G3_BRG_2" }, { 2756, "EVT_INV_RR_BRG_1" }, { 2900, "EVT_LAST_CW_ENT_0" }, { 2901, "EVT_LAST_CW_ETH_0" }, { 2902, "EVT_LAST_CW_1LY_0" },
            { 2903, "EVT_LAST_CW_1LY_1" }, { 2904, "EVT_LAST_CW_1LY_2" }, { 2905, "EVT_LAST_CW_GIA_0" }, { 2906, "EVT_LAST_CW_2LY_0" }, { 2907, "EVT_LAST_CW_2LY_1" }, { 2908, "EVT_LAST_CW_GIA_1" }, { 2909, "EVT_LAST_CW_3LY_0" }, { 2910, "EVT_LAST_CW_3LY_1" }, { 2911, "EVT_LAST_CW_3LY_2" }, { 2912, "EVT_LAST_CW_GIA_2" },
            { 2913, "EVT_LAST_CW_4LY_0" }, { 2914, "EVT_LAST_CW_4LY_1" }, { 2915, "EVT_LAST_CW_GIA_3" }, { 2916, "EVT_LAST_CW_5LY_0" }, { 2917, "EVT_LAST_CW_GIA_4" }, { 2918, "EVT_LAST_CW_LTP_0" }, { 2919, "EVT_LAST_CW_EMP_0" }, { 2920, "EVT_LAST_CW_SPC_0" }, { 2921, "EVT_LAST_CW_SPC_1" }, { 2922, "EVT_LAST_CW_CYW_0" },
            { 2923, "EVT_LAST_CW_CYW_1" }, { 2924, "EVT_LAST_CW_CYW_2" }, { 2925, "EVT_LAST_CW_CYW_3" }, { 2926, "EVT_LAST_CW_LST_0" }, { 2927, "EVT_LAST_CW_LST_A" }, { 2928, "EVT_LAST_CW_LST_1" }, { 2929, "EVT_LAST_CW_MBG_A" }, { 2930, "EVT_LAST_CW_MBG_0" }, { 2931, "EVT_LAST_G3_BRG_0" }, { 2932, "EVT_LAST_RR_BRG_0" },
            { 2933, "EVT_LAST_CW_MBG_1" }, { 2934, "EVT_LAST_CW_MBG_2" }, { 3000, "EVT_ENDING_AT_1" }, { 3001, "EVT_ENDING_AC_1" }, { 3002, "EVT_ENDING_AC_2" }, { 3003, "EVT_ENDING_AC_3" }, { 3004, "EVT_ENDING_BU_1" }, { 3005, "EVT_ENDING_KM_1" }, { 3006, "EVT_ENDING_LB_1" }, { 3007, "EVT_ENDING_TR_1" }, { 3008, "EVT_ENDING_TH_0" },
            { 3009, "EVT_ENDING_TH_1" }, { 3010, "EVT_ENDING_TH_2" }, { 3011, "EVT_ENDING_TH_3" }, { 3012, "EVT_ENDING_AC_4" }, { 3050, "EVT_MAGE2_BV_ENT_0" }, { 3051, "EVT_MAGE2_BV_ABR_0" }, { 3052, "EVT_MAGE2_BV_GVY_0" }, { 3053, "EVT_MAGE2_BV_INN_0" }, { 3054, "EVT_MAGE2_BV_ITS_0" }, { 3055, "EVT_MAGE2_BV_WPN_0" },
            { 3056, "EVT_MAGE2_BV_ORT_0" }, { 3057, "EVT_MAGE2_BV_CSK_0" }, { 3058, "EVT_MAGE2_BV_CSI_0" }, { 3059, "EVT_MAGE2_BV_ITM_0" }
        };
            List<FieldIndex> FieldIndices = new List<FieldIndex>(); //  temp obj
            List<Datapoint> Dataset = new List<Datapoint>();        //  perm obj
            List<FieldIndex> FinalFieldIndices = new List<FieldIndex>();    //  perm obj
            List<Datapoint> DS3 = new List<Datapoint>();      // perm
            List<FieldData> MainFieldIndex = new List<FieldData>(); //perm
            List<Datapoint> DataSIMP = new List<Datapoint>();
            //ListFI FieldIndicesObj = new ListFI(FieldIndices);
            //ListFI FinalFieldIndicesObj = new ListFI(FieldIndices);
            int[] deadzone = { 33122111, 34426256 };    //  last file byte 68034959 0x40e218f
            byte[] pattern1a_preload = new byte[] { 253, 0, 5 };        //var zone 1a x
            byte[] pattern1b_setvar = new byte[] { 5, 217, 21, 125 };   //var zone 1b x
            byte[] pattern2a_setent = new byte[] { 5, 216, 2, 125 };    //var zone 2a y
            byte[] pattern2a2_d802 = new byte[] { 5, 216, 2, 216, 2 };    //D802 x2 for double entrance
            byte[] pattern2a3_7e = new byte[] { 5, 216, 2, 126 };       //L long bytes (still only two bytes)
            byte[] pattern2b_field = new byte[] { 44, 127, 43, 0 };     //var zone 2b x
            byte[] pattern3_return = new byte[] { 4, 255 };             // returns are just 4, not 4, 0
            byte[] pattern4_loop = new byte[] { 1, 255, 255, 0, 0 };    // ignore 255s
            byte[] pattern5_range = new byte[] { 4, 5, 122, 2, 127, 3, 1, 0, 4 };  // range*_region
            byte[] pattern6_init = new byte[] { 1, 2, 0, 0, 8, 0, 2, 0, 255, 255, 41, 0, 255 };  // range*_init  offset between init and range for init data.
            //  read 255s first two is Uint16 of initdata after this pattern ends, last one is number of Uint16 pairs, so times by 4 to get length of data
            byte[] pattern6_BTNinit = new byte[] { 1, 3, 0, 0, 12, 0, 2, 0, 255, 255, 3, 0, 255 };  // range*_init  offset between init and range for init data.
            //                                      +1      +4                      +10
            byte[] pattern6_404init = new byte[] { 1, 3, 0, 0, 12, 0, 2, 0, 255, 255, 22, 0, 255 };  // range*_init  offset between init and range for init data.
            byte[] pattern7_Tri127 = new byte[] { 39, 0, 127 };  // SetTriangleFlagMask
            byte[] pattern7_OpCea = new byte[] { 234 };  // EA OpCode
            byte[] pattern7_OpCa9 = new byte[] { 169, 0, 250 };  // A9 OpCode
            int[] pattern8_noPreload = new int[] { 552, 555, 1302, 1305, 2922, 2923, 2924, 2925 };    //  thse need special allowance through all pattern1 loops, and them dummy data in place of preload patterns
            int[] windowAsyncFP = new int[] { 44, 127, 32, 0, 1, 16, 38 }; // 2C 7F 20 00 01 10 26 WindowAsync False Positive in char loops
            int[] progression = new int[] { 50, 104, 105, 106, 115, 150, 55, 54, 51, 58, 69, 209, 206, 251, 207, 208, 201, 204, 203, 202, 200, 252, 257, 261, 262, 300, 301, 307, 309, 312, 352, 354, 355, 358, 353, 356, 404, 406, 403, 400, 401, 456, 455, 
                457, 452, 451, 454, 504, 507, 500, 506, 503, 614, 600, 571, 607, 554, 565, 1365, 611, 613, 612, 615, 575, 563, 550, 551, 552, 555, 556, 559, 566, 567, 568, 569, 570, 572, 618, 701, 703, 705, 707, 800, 801, 750, 752, 755, 757, 753, 758, 
                760, 768, 812, 810, 809, 807, 908, 904, 909, 900, 916, 913, 914, 950, 951, 953, 952, 1201, 1000, 1001, 1051, 1059, 1055, 1052, 1060, 1107, 1101, 1010, 1110, 1153, 1150, 1208, 1211, 1206, 1205, 1225, 1203, 1204, 955, 1250, 1251, 1254, 
                1255, 1300, 1307, 1315, 1352, 1422, 1425, 3100, 1501, 1505, 1463, 1451, 1454, 1450, 1500, 1503, 1504, 1550, 1554, 1555, 1600, 1601, 1607, 1602, 1605, 1606, 1650, 1652, 1754, 1755, 1756, 1757, 1608, 1610, 1658, 1657, 1654, 1661, 1663, 
                1662, 1659, 1861, 1854, 1864, 1819, 1803, 1807, 1866, 1816, 1905, 1903, 2054, 2050, 2007, 2000, 2006, 2055, 2105, 2161, 2150, 2172, 2169, 2211, 2107, 2111, 2114, 2855, 2856, 1452, 1458, 2200, 2202, 2201, 2209, 2261, 2250, 2251, 2253, 
                2254, 2257, 2258, 2259, 2260, 2204, 2205, 2213, 2222, 2207, 2212, 2301, 2357, 2157, 1800, 2800, 2850, 2854, 2500, 2502, 2512, 2510, 2504, 2505, 2551, 2851, 2852, 2550, 2853, 2601, 2603, 2604, 2605, 2606, 2607, 2651, 2653, 2654, 2657, 
                2656, 2658, 2652, 2700, 2701, 2706, 2710, 2711, 2717, 2719, 2660, 2750, 2751, 3050, 2753, 2756, 2900, 2904, 2905, 2907, 2908, 2912, 2914, 2915, 2917, 2918, 2919, 2920, 2921, 2922, 2926, 2934 };   //  this is the required fields to advance the story. some are used multiple times, as long as we have a complete path once, the user can walk backwards when needed.
            int[] blank2ints = new int[] { 0, 0 };
            byte[] blank2bytes = new byte[] { 0, 0 };
            int[][] raddresses = { blank2ints, blank2ints, blank2ints, blank2ints, blank2ints, blank2ints, blank2ints, };

            int[] pat1a, pat1b, pat2a, pat2b = blank2ints;
            byte[] pat1abytes, pat1bbytes, pat2abytes, pat2bbytes = blank2bytes;

            
            //int[][] blank42ints = new int[][] { new int[2], new int[2], new int[2], new int[2], };
            //byte[][] blank42bytes = new byte[][] { new byte[2], new byte[2], new byte[2], new byte[2], };

            //      Funcs       //
            (int key, string value, byte[] asc2) DictBytesConvert(int i)
            {
                KeyValuePair<int, string> fieldSet = ff9Fields.ElementAt(i);
                byte[] ascii = Encoding.ASCII.GetBytes(fieldSet.Value);
                byte[] ascii2 = new byte[] { 46, 101, 98 }; //   add "2e 65 62" ".eb"
                byte[] ascii3 = new byte[ascii.Length + ascii2.Length];
                ascii.CopyTo(ascii3, 0);
                ascii2.CopyTo(ascii3, ascii.Length);
                return (key: fieldSet.Key, value: fieldSet.Value, asc2: ascii3);
            }
            void CheckSumChecks(string opt)
            {
                //if (j == 1) for (int i = 0; i < FieldIndices.Count; i++) FieldIndices[i].SumCheck();
                if (opt == "ffi") for (int i = 0; i < FinalFieldIndices.Count; i++) { FinalFieldIndices[i].SumCheck(); }
                if (opt == "ds") for (int i = 0; i < Dataset.Count; i++) { Dataset[i].SumCheck(); }
            }
            //       Cache      //
            string Md5()
            {
                //  CURRENT
                // p0data7.bin    mog  99a8a52d4abdfcedc570b948c1ff8a75     fresh mog install
                // p0data7.bin    reg  df4d0b9acd66df48452c40028e29b8ef     fresh install
                // SB_FFIO_mog.bin     6945a5aa5c24726f1120dac254ccadc9     non ListFI, just List<FieldIndex>
                // SB_FFIO_reg.bin     cd14fa2647f6641dc7587c8818cc305f     non ListFI, just List<FieldIndex>

                //  UNEEDED
                // SB_FIO_reg.bin      e62b65f7bc9e8221b030fcc564b3512c     fresh
                // SB_FIO_mog.bin mog  f1c48c76ef1ae9f383979108b0c1489c    checked
                // SB_FFIO_reg.bin     53202caa6cc6dea598fd37364815c752     verified    ListFI
                // SB_FFIO_mog.bin     fc79ff1b71a4871feff1b69864508c83     updated     ListFI

                string md5cacheFI = "FFIO nomatch";
                string md5p7 = "df4d0b9acd66df48452c40028e29b8ef";
                string dirp7 = "\\StreamingAssets\\p0data7.bin";
                string md5p7w = "";
                string md5ficache = "cd14fa2647f6641dc7587c8818cc305f";
                string dirCache = "\\SB_FFIO_reg.bin";
                string md5Cache = "";
                if (c_mogdetect.Checked)
                {
                    md5p7 = "99a8a52d4abdfcedc570b948c1ff8a75";
                    dirp7 = "\\MoguriFiles\\StreamingAssets\\p0data7.bin";
                    md5ficache = "6945a5aa5c24726f1120dac254ccadc9";
                    dirCache = "\\SB_FFIO_mog.bin";
                }

                //  md5 action
                using (var md5 = MD5.Create())
                {

                    //  md5 p0data7.bin
                    using (var stream = File.OpenRead(tb_fl.Text + dirp7))
                    { md5p7w = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant(); }

                    //  if cache file exists, md5 it also
                    if (File.Exists(dirCache))
                    {
                        using (var stream2 = File.OpenRead(tb_fl.Text + dirCache))
                        { md5Cache = BitConverter.ToString(md5.ComputeHash(stream2)).Replace("-", "").ToLowerInvariant(); }
                    }
                    else if (!File.Exists(dirCache))
                    {
                        if (md5p7 == md5p7w)
                        {
                            //restore cache files from inside program for known p0data7 md5
                            CacheIO("restore SB_FFIO");
                            richTextBox_debug.Text += "\nFieldIndex Cache Done";
                            // md5 newly created cache
                            using (var stream2 = File.OpenRead(tb_fl.Text + dirCache))
                            { md5Cache = BitConverter.ToString(md5.ComputeHash(stream2)).Replace("-", "").ToLowerInvariant(); }                     ///////////////////////
                        }
                        // else fallout of loop no match
                    }
                }

                //  bool md5s
                if ((md5p7w == md5p7) && (md5Cache == md5ficache)) { md5cacheFI = "FFIO match"; }

                return md5cacheFI;
            }
            void CacheCheck()
            {
                string md5cacheFI = Md5();

                //      FIELD INDEX CACHE
                if (md5cacheFI == "FFIO match")
                {
                    richTextBox_debug.Text += "\nUsing FieldIndex Cache";
                    CacheIO("read SB_FFIO");
                }
                if (md5cacheFI == "FFIO nomatch")     //  manually genearte new FI cache for unkown p0data7
                {
                    richTextBox_debug.Text += "\nGenerating New Cache Please wait";
                    var t = Task.Run(async delegate { await Task.Delay(600); }); t.Wait();
                    GenFieldIndex();    //  ONLY TIME TO RUN THIS
                    GenFinalFieldIndex();   //  ONLY TIME TO RUN THIS
                }
                //      OTHER CACHE

                //i think check if user cache IS different and we generate it, perhaps make an inclusion for a usercache check
                //currently if no md5 match, manual recreation everytime. need a way of storing the cache hash to skip recreation for user caches

            }
            void CacheIO(string opt)
            {
                string ffio = "\\SB_FFIO_reg.bin";
                if (c_mogdetect.Checked)
                {
                    ffio = "\\SB_FFIO_mog.bin";
                }
                string sffio = tb_fl.Text + ffio;

                if (opt == "write SB_FFIO") //  serialize
                {
                    //FinalFieldIndices = new List<FieldIndex>(FinalFieldIndices);
                    FileStream fs = new FileStream(sffio, FileMode.Create);
                    BinaryFormatter formatter = new BinaryFormatter();
                    try { formatter.Serialize(fs, FinalFieldIndices); }
                    catch (System.Runtime.Serialization.SerializationException e) { richTextBox_debug.Text = "SerialF: " + e.Message; throw; }
                    catch (System.Security.SecurityException e) { richTextBox_debug.Text = "SerialF: " + e.Message; throw; }
                    catch (ArgumentNullException e) { richTextBox_debug.Text = "SerialF: " + e.Message; throw; }
                    finally { fs.Close(); }
                }
                if (opt == "read SB_FFIO")  //  deserialize
                {
                    FileStream fs = new FileStream(sffio, FileMode.Open);
                    BinaryFormatter formatter = new BinaryFormatter();
                    try { FinalFieldIndices = (List<FieldIndex>)formatter.Deserialize(fs); }
                    catch (System.Runtime.Serialization.SerializationException e) { richTextBox_debug.Text = "SerialF: " + e.Message; throw; }
                    catch (System.Security.SecurityException e) { richTextBox_debug.Text = "SerialF: " + e.Message; throw; }
                    catch (ArgumentNullException e) { richTextBox_debug.Text = "SerialF: " + e.Message; throw; }
                    finally { fs.Close(); }
                    //FinalFieldIndices = FieldIndicesObj.Filist;

                }
                if (opt == "restore SB_FFIO")
                {
                    byte[] sb_ffio = Resources.Caches.SB_FFIO_reg;
                    if (c_mogdetect.Checked)
                    {
                        sb_ffio = Resources.Caches.SB_FFIO_mog;
                    }
                    File.WriteAllBytes(sffio, sb_ffio);
                }

                //  end of CacheIO()
            }
            //    Field Index   //
            void GenFieldIndex()
            {
                int[][] r2 = raddresses; int p7w = 0; int r7 = 0; int[] r3 = new int[7];
                pbar_tree.Minimum = 0; pbar_tree.Value = 0; pbar_tree.Maximum = ff9Fields.Count;
                richTextBox_debug.Text += "\nFI gen >";
                var t = Task.Run(async delegate { await Task.Delay(600); }); t.Wait();
                for (int fe = 0; fe < ff9Fields.Count; fe++)
                {
                    (int key, string value, byte[] asc2) = DictBytesConvert(fe);
                    r2 = raddresses; r3 = new int[7]; p7w = 0; r7 = 0;    //  only reset byte worker and ranges once during field
                    for (int p7 = 0; p7 < ba_p0data7.Length; p7++)    //  start/continue iteration using worker
                    {
                        if (p7 > deadzone[0] & p7 < deadzone[1]) { p7 = deadzone[1] + 1; }  //  deadzone check and pass
                        if (ba_p0data7[p7] == asc2[0])   //  first byte match
                        {
                            p7w = p7;   //  worker zone p7w
                            for (int fbl = 0; fbl < asc2.Length; fbl++)  //  iterate and match field ascii
                            {
                                if (!(ba_p0data7[p7w] == asc2[fbl])) { break; } //  break early if false match
                                if (ba_p0data7[p7w] == asc2[fbl])   //  found matching byte, worker++
                                {
                                    p7w++;
                                    if ((p7w - p7) == asc2.Length)   //  found match same length as the source pattern
                                    {
                                        r3[r7] = p7w;
                                        r7++;   //  thriple loop here not needed. we just r7++ untill end of file, always 7
                                    }
                                }
                            }
                            p7 = p7w;   //  leaving worker zone p7w   update p7 to last work address
                        }
                    }
                    //  add 7 r3 has all values, need to mux into r2
                    r2 = new int[][] { new int[] { r3[0], 0}, new int[] { r3[1], 0 }, new int[] { r3[2], 0 }, new int[] { r3[3], 0 }, new int[] { r3[4], 0 }, new int[] { r3[5], 0 }, new int[] { r3[6], 0 } };
                    FieldIndices.Add(new FieldIndex(key, value, asc2, false, r2));  //  only create 1 with all r2[r7] data
                    //richTextBox_output.Text += "\nr2[0] " + FieldIndices.Last().RangeStartStop[0][0] + " r2[1] " + FieldIndices.Last().RangeStartStop[1][0] + "";
                    pbar_tree.Value = fe;
                }
                richTextBox_debug.Text += "> " + FieldIndices.Count + " generated";
                //CacheIO("write SB_FIO");                
            }
            void GenFinalFieldIndex()
            {
                int[] start = new int[5719];    //  FieldIndicies.Count * FieldIndicies.RangesStartStop.Length = 5719
                int k = 0; pbar_tree.Value = 0; pbar_tree.Maximum = FieldIndices.Count;
                for (int i = 0; i < FieldIndices.Count; i++)
                {
                    for (int j = 0; j < FieldIndices[i].RangeStartStop.Length; j++)
                    {
                        start[k] = FieldIndices[i].RangeStartStop[j][0];
                        k++;
                    }
                }
                Array.Sort(start);
                for (int i = 0; i < FieldIndices.Count; i++)
                {
                    for (int j = 0; j < FieldIndices[i].RangeStartStop.Length; j++)
                    {
                        for (int kk = 0; kk < start.Length; kk++)
                        {
                            if (start[kk] == FieldIndices[i].RangeStartStop[j][0])
                            {
                                int end = 0;
                                if (kk == start.Length - 1) { end = ba_p0data7.Length; } //  to account for end file
                                else { end = start[kk + 1]; }   //  normal range end
                                FieldIndices[i].RangeStartStop[j][1] = end;
                            }
                        }
                    }
                    pbar_tree.Value = i;
                }
                FinalFieldIndices = FieldIndices;
                CheckSumChecks("ffi");
                richTextBox_debug.Text += "\nFFI generated";
                CacheIO("write SB_FFIO");
            }
            //    Datapoints    //
            void FIDataset()
            {
                pbar_tree.Minimum = 0; pbar_tree.Value = 0; pbar_tree.Maximum = FinalFieldIndices.Count;
                for (int ffi = 0; ffi < FinalFieldIndices.Count; ffi++) //  iterate through FFI (817)
                {
                    bool special = false; //    for pattern8 bypass lane
                    for (int ffir = 0; ffir < FinalFieldIndices[ffi].RangeStartStop.Length; ffir++) //  iterate through all ranges (7)
                    {
                        //  build new headers everytime the range changes (ffir)
                        List<int> headers = new List<int>();

                        for (int ffirs = FinalFieldIndices[ffi].RangeStartStop[ffir][0]; ffirs < FinalFieldIndices[ffi].RangeStartStop[ffir][1]; ffirs++)   //  scan range for headers before pattern searching
                        {
                            if (ba_p0data7[ffirs] == pattern6_init[0]) //  first byte of range init
                            {
                                //new pattern6 search
                                //match pattern 6 and 5 using corrected offsets
                                if ((ba_p0data7[ffirs] == pattern6_init[0]) && ((ba_p0data7[ffirs + 1] == pattern6_init[1]) | (ba_p0data7[ffirs + 1] == pattern6_BTNinit[1])) && (ba_p0data7[ffirs + 2] == pattern6_init[2]) && (ba_p0data7[ffirs + 3] == pattern6_init[3]) && ((ba_p0data7[ffirs + 4] == pattern6_init[4]) | (ba_p0data7[ffirs + 4] == pattern6_BTNinit[4])) && (ba_p0data7[ffirs + 5] == pattern6_init[5]) && (ba_p0data7[ffirs + 6] == pattern6_init[6]) && (ba_p0data7[ffirs + 7] == pattern6_init[7]) && ((ba_p0data7[ffirs + 10] == pattern6_init[10]) | (ba_p0data7[ffirs + 10] == pattern6_BTNinit[10]) | (ba_p0data7[ffirs + 10] == pattern6_404init[10])) && (ba_p0data7[ffirs + 11] == pattern6_init[11]))
                                {
                                    int i = BitConverter.ToUInt16(ba_p0data7, ffirs + 8);   //correct
                                                                                            //int j = (ba_p0data7[ffirs + 12] * 4); //weird
                                                                                            //int k = ffirs + 12 + j + 1; //pretty sure its right
                                                                                            //int l = (ffirs + 10 + i) - 9;   //  precise
                                    int k = (ffirs + i + 1);    //better
                                    if ((ba_p0data7[k] == pattern5_range[0]) && (ba_p0data7[k + 1] == pattern5_range[1]) && (ba_p0data7[k + 2] == pattern5_range[2]) && (ba_p0data7[k + 3] == pattern5_range[3]) && (ba_p0data7[k + 4] == pattern5_range[4]) && (ba_p0data7[k + 5] == pattern5_range[5]) && (ba_p0data7[k + 6] == pattern5_range[6]) && (ba_p0data7[k + 7] == pattern5_range[7]) && (ba_p0data7[k + 8] == pattern5_range[8]))
                                    {
                                        // range match, log
                                        ffirs = k + 9;
                                        headers.Add(ffirs);
                                        //ffirs = (k + 9);
                                    }
                                }
                            }
                        }
                        if ((FinalFieldIndices[ffi].FieldID == pattern8_noPreload[0]) || (FinalFieldIndices[ffi].FieldID == pattern8_noPreload[1]) || (FinalFieldIndices[ffi].FieldID == pattern8_noPreload[2]) || (FinalFieldIndices[ffi].FieldID == pattern8_noPreload[3]) || (FinalFieldIndices[ffi].FieldID == pattern8_noPreload[4]) || (FinalFieldIndices[ffi].FieldID == pattern8_noPreload[5]) || (FinalFieldIndices[ffi].FieldID == pattern8_noPreload[6]) || (FinalFieldIndices[ffi].FieldID == pattern8_noPreload[7]))
                        {
                            special = true;
                        }
                        for (int hc = 0; hc < headers.Count; hc++)  //  for each header, scanny scan
                        {
                            int hl = hc; bool hlast = false;
                            if (hc == headers.Count - 1)  //  last header
                            {
                                hl = FinalFieldIndices[ffi].RangeStartStop[ffir][1];
                                hlast = true;
                            }
                            if (hc < headers.Count - 1)   //  all but last header
                            {
                                hl = headers[hc + 1] - 38;  //  22+16 good enough
                                hlast = false;
                            }
                            int i = Dataset.Count;  //  log current dataset count
                            for (int ffirs = headers[hc]; ffirs < hl; ffirs++)  //  for each byte between headers
                            {
                                //  searching through index start and stops for 4 patterns in sequence
                                if (special || ba_p0data7[ffirs] == pattern1a_preload[0])  //  postive match on first byte
                                {
                                    ffirs = PatternFieldExits(ffi, ffir, ffirs, hc, hl, hlast, special);
                                    //  if return from fail, will cycle naturally to next value to start search
                                    //  if return from success, will continue search after positive match
                                }
                            }

                        }
                    }
                    pbar_tree.Value = ffi;
                }
                richTextBox_output.Text += "\nFin";
                int PatternFieldExits(int ffi, int ffir, int ffirs, int hc, int hl, bool hlast, bool special)
                {
                    if (special)
                    {   //special search
                        if ((ba_p0data7[ffirs] == pattern7_Tri127[0]) && (ba_p0data7[ffirs + 1] == pattern7_Tri127[1]) && (ba_p0data7[ffirs + 2] == pattern7_Tri127[2])) //  match triangles isntead
                        {
                            //positive match on p7_tri127   dummy data
                            pat1abytes = new byte[] { 0, 0 };
                            pat1a = new int[] { 0, 0 };
                            //match next pattern
                            if ((ba_p0data7[ffirs + 3] == pattern7_OpCea[0]) || ((ba_p0data7[ffirs + 3] == pattern7_OpCa9[0]) && (ba_p0data7[ffirs + 4] == pattern7_OpCa9[1]) && (ba_p0data7[ffirs + 5] == pattern7_OpCa9[2])))
                            {
                                //positive match on p7OpCodes   dummy data
                                pat1bbytes = new byte[] { 0, 0 };
                                pat1b = new int[] { 0, 0 };
                                int ffirs2 = ffirs + 6;   //  worker2 for new search updated to +6 for max opcode length
                                                          //next pattern may be lines away, need to continue search before next match.
                                for (ffirs2 = ffirs + 6; ffirs2 < hl; ffirs2++)   //  iterate through ba_p0data7 using indexes (/~/) range seasrch
                                {
                                    if (ba_p0data7[ffirs2] == pattern2a_setent[0])  //  postive match on first byte
                                    {

                                        if ((ba_p0data7[ffirs2] == pattern2a_setent[0]) && (ba_p0data7[ffirs2 + 1] == pattern2a_setent[1]) && (ba_p0data7[ffirs2 + 2] == pattern2a_setent[2]) && (ba_p0data7[ffirs2 + 3] == pattern2a_setent[3]))   //  match all
                                        {
                                            //positive match on p2a
                                            pat2abytes = new byte[] { ba_p0data7[ffirs2 + 4], ba_p0data7[(ffirs2 + 5)] };
                                            pat2a = new int[] { ffirs2 + 4, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 4) };
                                            //match next pattern
                                            if ((ba_p0data7[ffirs2 + 6] == pattern2b_field[0]) && (ba_p0data7[ffirs2 + 7] == pattern2b_field[1]) && (ba_p0data7[ffirs2 + 8] == pattern2b_field[2]) && (ba_p0data7[ffirs2 + 9] == pattern2b_field[3]))   //  match all 
                                            {
                                                //positive match on p2b
                                                pat2bbytes = new byte[] { ba_p0data7[ffirs2 + 10], ba_p0data7[(ffirs2 + 11)] };
                                                pat2b = new int[] { ffirs2 + 10, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 10) };
                                                int[][] dp_init = new int[][] { pat1a, pat1b, pat2a, pat2b };
                                                byte[][] dp_byte = new byte[][] { pat1abytes, pat1bbytes, pat2abytes, pat2bbytes };
                                                Dataset.Add(new Datapoint(FinalFieldIndices[ffi].FieldID, FinalFieldIndices[ffi].FieldName, false, false, ffir, hc, false, dp_init, dp_byte));
                                                if (Dataset.Count > 1)
                                                {
                                                    if ((Dataset[Dataset.Count - 1].FieldID == Dataset[Dataset.Count - 2].FieldID) && (Dataset[Dataset.Count - 1].RangeV == Dataset[Dataset.Count - 2].RangeV) && (Dataset[Dataset.Count - 1].HeaderC == Dataset[Dataset.Count - 2].HeaderC))   //  ifelse drop from list
                                                    {
                                                        Dataset.RemoveAt(Dataset.Count - 1);
                                                        Dataset.RemoveAt(Dataset.Count - 1);
                                                    }
                                                }
                                                ffirs2 += 12;
                                                if (hlast)
                                                {
                                                    if (ba_p0data7[ffirs2] == 4) // simple return exit following match
                                                    {
                                                        ffirs2 = hl;
                                                        return ffirs2;
                                                    }
                                                    //do small search for 2c 7f 04 return
                                                    for (int i = ffirs2; i < ffirs2 + 36; i++)
                                                    {
                                                        if ((ba_p0data7[i] == 4) && (ba_p0data7[i - 1] == 127) && (ba_p0data7[i - 2] == 44))
                                                        {
                                                            ffirs2 = hl;
                                                            return ffirs2;
                                                        }
                                                    }
                                                }
                                                return ffirs2; //return back to range search with address after positive match
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {   // regular search path
                        if ((ba_p0data7[ffirs] == pattern1a_preload[0]) && (ba_p0data7[ffirs + 1] == pattern1a_preload[1]) && (ba_p0data7[ffirs + 2] == pattern1a_preload[2])) //  match all
                        {
                            //positive match on p1a
                            pat1abytes = new byte[] { ba_p0data7[ffirs + 3], ba_p0data7[(ffirs + 4)] };
                            pat1a = new int[] { ffirs + 3, BitConverter.ToUInt16(ba_p0data7, ffirs + 3) };
                            //match next pattern
                            if ((ba_p0data7[ffirs + 5] == pattern1b_setvar[0]) && (ba_p0data7[ffirs + 6] == pattern1b_setvar[1]) && (ba_p0data7[ffirs + 7] == pattern1b_setvar[2]) && (ba_p0data7[ffirs + 8] == pattern1b_setvar[3]))
                            {
                                //positive match on p1b
                                pat1bbytes = new byte[] { ba_p0data7[ffirs + 9], ba_p0data7[(ffirs + 10)] };
                                pat1b = new int[] { ffirs + 9, BitConverter.ToUInt16(ba_p0data7, ffirs + 9) };
                                int ffirs2 = ffirs + 11;   //  worker2 for new search updated to +11 for pattern p1a+varzone1+p1b+varzone2
                                                           //next pattern may be lines away, need to continue search before next match.
                                for (ffirs2 = ffirs + 11; ffirs2 < hl; ffirs2++)   //  iterate through ba_p0data7 using indexes (/~/) range seasrch        foreach range , now iterate through header address ranges      I think it was adding 11 twice, fixed now.
                                {
                                    //reminders
                                    //  pattern2a_setent = new byte[] { 5, 216, 2, 125 };    //var zone 2a y
                                    //  pattern2a2_d802 =  new byte[] { 5, 216, 2, 216, 2 };    //D802 x2 for double entrance
                                    //  pattern2a3_7e =    new byte[] { 5, 216, 2, 126 };      //7E for Long bytes (still only 2 bytes)
                                    //  pattern3_return = new byte[] { 4, 0 };                  // 04 00 = return of function
                                    //reminders

                                    if (ba_p0data7[ffirs2] == pattern2a_setent[0])  //  postive match on first byte
                                    {
                                        bool bool1 = (ba_p0data7[ffirs2] == pattern2a_setent[0]) && (ba_p0data7[ffirs2 + 1] == pattern2a_setent[1]) && (ba_p0data7[ffirs2 + 2] == pattern2a_setent[2]); //  0,1,2, first three match, all patterns share this
                                        bool bool2 = (ba_p0data7[ffirs2 + 3] == pattern2a_setent[3]);                                                                                                   //  offset 12 3, match setent, regular, grab next two bytes
                                        bool bool3 = (ba_p0data7[ffirs2 + 3] == pattern2a3_7e[3]);                                                                                                      //  offset 14 3, match 7e plus 2 [00] [00] bytes after, need to account for this
                                        bool bool4 = (ba_p0data7[ffirs2 + 3] == pattern2a2_d802[3]) && (ba_p0data7[ffirs2 + 4] == pattern2a2_d802[4]);                                                  //  offset 11 3,4, match d802 and store 3,4, losing a byte here "=" missing, need to account for this

                                        if (bool1 && (bool2 | bool3 | bool4))   //  bool1 has to match, and either 2 or 3 can match, or 4
                                        {
                                            if (bool2)  //  regular
                                            {
                                                //positive match on p2a
                                                pat2abytes = new byte[] { ba_p0data7[ffirs2 + 4], ba_p0data7[(ffirs2 + 5)] };
                                                pat2a = new int[] { ffirs2 + 4, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 4) };

                                                //match next pattern
                                                if ((ba_p0data7[ffirs2 + 6] == pattern2b_field[0]) && (ba_p0data7[ffirs2 + 7] == pattern2b_field[1]) && (ba_p0data7[ffirs2 + 8] == pattern2b_field[2]) && (ba_p0data7[ffirs2 + 9] == pattern2b_field[3]))   //  match all 
                                                {
                                                    //positive match on p2b
                                                    pat2bbytes = new byte[] { ba_p0data7[ffirs2 + 10], ba_p0data7[(ffirs2 + 11)] };
                                                    pat2b = new int[] { ffirs2 + 10, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 10) };
                                                    int[][] dp_init = new int[][] { pat1a, pat1b, pat2a, pat2b };
                                                    byte[][] dp_byte = new byte[][] { pat1abytes, pat1bbytes, pat2abytes, pat2bbytes };
                                                    Dataset.Add(new Datapoint(FinalFieldIndices[ffi].FieldID, FinalFieldIndices[ffi].FieldName, false, false, ffir, hc, false, dp_init, dp_byte));
                                                    if (Dataset.Count > 1)
                                                    {
                                                        if ((Dataset[Dataset.Count - 1].FieldID == Dataset[Dataset.Count - 2].FieldID) && (Dataset[Dataset.Count - 1].RangeV == Dataset[Dataset.Count - 2].RangeV) && (Dataset[Dataset.Count - 1].HeaderC == Dataset[Dataset.Count - 2].HeaderC))   //  ifelse drop from list
                                                        {
                                                            Dataset.RemoveAt(Dataset.Count - 1);
                                                            Dataset.RemoveAt(Dataset.Count - 1);
                                                        }
                                                    }
                                                    ffirs2 += 12;
                                                    if (hlast)
                                                    {
                                                        if (ba_p0data7[ffirs2] == 4) // simple return exit following match
                                                        {
                                                            ffirs2 = hl;
                                                            return ffirs2;
                                                        }
                                                        //do small search for 2c 7f 04 return
                                                        for (int i = ffirs2; i < ffirs2 + 36; i++)
                                                        {
                                                            if ((ba_p0data7[i] == 4) && (ba_p0data7[i - 1] == 127) && (ba_p0data7[i - 2] == 44))
                                                            {
                                                                ffirs2 = hl;
                                                                return ffirs2;
                                                            }
                                                        }
                                                    }
                                                    //richTextBox_output.Text += "\nmulti-header-7d";
                                                    return ffirs2; //return back to range search with address after positive match
                                                }
                                            }
                                            if (bool3)  //  7e  same as regular, but need to skip 2 extra bytes after gathering data, +2 total length later
                                            {
                                                //positive match on p2a
                                                pat2abytes = new byte[] { ba_p0data7[ffirs2 + 4], ba_p0data7[(ffirs2 + 5)] };
                                                pat2a = new int[] { ffirs2 + 4, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 4) };

                                                //match next pattern
                                                if ((ba_p0data7[ffirs2 + 8] == pattern2b_field[0]) && (ba_p0data7[ffirs2 + 9] == pattern2b_field[1]) && (ba_p0data7[ffirs2 + 10] == pattern2b_field[2]) && (ba_p0data7[ffirs2 + 11] == pattern2b_field[3]))   //  match all
                                                {
                                                    //positive match on p2b
                                                    pat2bbytes = new byte[] { ba_p0data7[ffirs2 + 12], ba_p0data7[(ffirs2 + 13)] };
                                                    pat2b = new int[] { ffirs2 + 12, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 12) };
                                                    int[][] dp_init = new int[][] { pat1a, pat1b, pat2a, pat2b };
                                                    byte[][] dp_byte = new byte[][] { pat1abytes, pat1bbytes, pat2abytes, pat2bbytes };
                                                    Dataset.Add(new Datapoint(FinalFieldIndices[ffi].FieldID, FinalFieldIndices[ffi].FieldName, false, false, ffir, hc, false, dp_init, dp_byte));
                                                    if (Dataset.Count > 1)
                                                    {
                                                        if ((Dataset[Dataset.Count - 1].FieldID == Dataset[Dataset.Count - 2].FieldID) && (Dataset[Dataset.Count - 1].RangeV == Dataset[Dataset.Count - 2].RangeV) && (Dataset[Dataset.Count - 1].HeaderC == Dataset[Dataset.Count - 2].HeaderC))   //  ifelse drop from list
                                                        {
                                                            Dataset.RemoveAt(Dataset.Count - 1);
                                                            Dataset.RemoveAt(Dataset.Count - 1);
                                                        }
                                                    }
                                                    ffirs2 += 14;
                                                    if (hlast)
                                                    {
                                                        if (ba_p0data7[ffirs2] == 4) // simple return exit following match
                                                        {
                                                            ffirs2 = hl;
                                                            return ffirs2;
                                                        }
                                                        //do small search for 2c 7f 04 return
                                                        for (int i = ffirs2; i < ffirs2 + 36; i++)
                                                        {
                                                            if ((ba_p0data7[i] == 4) && (ba_p0data7[i - 1] == 127) && (ba_p0data7[i - 2] == 44))
                                                            {
                                                                ffirs2 = hl;
                                                                return ffirs2;
                                                            }
                                                        }
                                                    }
                                                    //richTextBox_output.Text += "\nmulti-header-7e";
                                                    return ffirs2; //return back to range search with address after positive match
                                                }
                                            }
                                            if (bool4)  //  d802    start gathering one byte prior to regular, one less total length later
                                            {
                                                //positive match on p2a
                                                pat2abytes = new byte[] { ba_p0data7[ffirs2 + 3], ba_p0data7[(ffirs2 + 4)] };
                                                pat2a = new int[] { ffirs2 + 3, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 3) };

                                                //match next pattern
                                                if ((ba_p0data7[ffirs2 + 5] == pattern2b_field[0]) && (ba_p0data7[ffirs2 + 6] == pattern2b_field[1]) && (ba_p0data7[ffirs2 + 7] == pattern2b_field[2]) && (ba_p0data7[ffirs2 + 8] == pattern2b_field[3]))   //  match all
                                                {
                                                    //positive match on p2b
                                                    pat2bbytes = new byte[] { ba_p0data7[ffirs2 + 9], ba_p0data7[(ffirs2 + 10)] };
                                                    pat2b = new int[] { ffirs2 + 9, BitConverter.ToUInt16(ba_p0data7, ffirs2 + 9) };
                                                    int[][] dp_init = new int[][] { pat1a, pat1b, pat2a, pat2b };
                                                    byte[][] dp_byte = new byte[][] { pat1abytes, pat1bbytes, pat2abytes, pat2bbytes };
                                                    Dataset.Add(new Datapoint(FinalFieldIndices[ffi].FieldID, FinalFieldIndices[ffi].FieldName, false, false, ffir, hc, false, dp_init, dp_byte));
                                                    if (Dataset.Count > 1)
                                                    {
                                                        if ((Dataset[Dataset.Count - 1].FieldID == Dataset[Dataset.Count - 2].FieldID) && (Dataset[Dataset.Count - 1].RangeV == Dataset[Dataset.Count - 2].RangeV) && (Dataset[Dataset.Count - 1].HeaderC == Dataset[Dataset.Count - 2].HeaderC))   //  ifelse drop from list
                                                        {
                                                            Dataset.RemoveAt(Dataset.Count - 1);
                                                            Dataset.RemoveAt(Dataset.Count - 1);
                                                        }
                                                    }
                                                    ffirs2 += 11;
                                                    if (hlast)
                                                    {
                                                        if (ba_p0data7[ffirs2] == 4) // simple return exit following match
                                                        {
                                                            ffirs2 = hl;
                                                            return ffirs2;
                                                        }
                                                        //do small search for 2c 7f 04 return
                                                        for (int i = ffirs2; i < ffirs2 + 36; i++)
                                                        {
                                                            if ((ba_p0data7[i] == 4) && (ba_p0data7[i - 1] == 127) && (ba_p0data7[i - 2] == 44))
                                                            {
                                                                ffirs2 = hl;
                                                                return ffirs2;
                                                            }
                                                        }
                                                    }
                                                    //richTextBox_output.Text += "\nmulti-header-d802d802";
                                                    return ffirs2; //return back to range search with address after positive match
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                    return ffirs;  //  return back to range search with current address to failing match
                }


            }
            //    ReArrange     //
            void MFIgen()
            {
                for (int fi = 0; fi < ff9Fields.Count; fi++)
                {
                    int fid = ff9Fields.ElementAt(fi).Key; string fname = ff9Fields.ElementAt(fi).Value;
                    MainFieldIndex.Add(new FieldData(fid, fname, new List<int>(), new List<int[]>(), new List<int[]>(), new List<int>(), new List<int>(), new List<int>()));   //  create new DP2 at first datapoint
                    DataSIMP = new List<Datapoint>(Dataset);    //  create new copy of list each loop
                    DataSIMP.RemoveAll(datapoint => !(datapoint.FieldID == fid));

                    if (DataSIMP.Count > 0)
                    {
                        MFIsimp();
                        void MFIsimp()
                        {
                            int exitNum = DataSIMP[0].HeaderC;
                            int[] exitAddress = new int[21];    //  
                            int[] genAddress = new int[7];     //  same
                            int i = 0; int j = 0; int exitValue = 0; int genValue = 0;
                            for (int si = 0; si < DataSIMP.Count; si++)    //  iterate all list and log data
                            {
                                bool temp31b = (DataSIMP[si].AddressNvalue[3][1] == DataSIMP[0].AddressNvalue[3][1]);   //exit fid check
                                bool temp21b = (DataSIMP[si].AddressNvalue[2][1] == DataSIMP[0].AddressNvalue[2][1]);   //exit gen check
                                bool tempheadb = (DataSIMP[si].HeaderC == DataSIMP[0].HeaderC); //exit header check
                                if (temp31b && temp21b && tempheadb) //  match both exitfid and gen of first in list.
                                {
                                    //store current line data into worker arrays
                                    exitAddress[i] = DataSIMP[si].AddressNvalue[0][0];
                                    exitAddress[i + 1] = DataSIMP[si].AddressNvalue[1][0];
                                    exitAddress[i + 2] = DataSIMP[si].AddressNvalue[3][0];
                                    i += 3;
                                    genAddress[j] = DataSIMP[si].AddressNvalue[2][0];
                                    j += 1;
                                    exitValue = DataSIMP[si].AddressNvalue[3][1];
                                    genValue = DataSIMP[si].AddressNvalue[2][1];
                                }
                            }
                            //  add total worker to last data
                            MainFieldIndex.Last().ExitAddress.Add(exitAddress);
                            MainFieldIndex.Last().GenAddress.Add(genAddress);
                            MainFieldIndex.Last().ExitValue.Add(exitValue);
                            MainFieldIndex.Last().GenValue.Add(genValue);
                            MainFieldIndex.Last().ExitNum.Add(exitNum);

                            DataSIMP.RemoveAll(datapoint => datapoint.HeaderC == exitNum);  //  remove logged data from current list by means of headerC
                            if (DataSIMP.Count > 0)
                            {
                                MFIsimp();
                            }
                        }
                    }
                    
                }
                //  now we have MFI ordered much better, output and verify
            }

            void MFIgv()    //  add all GE's to source field they exit to.
            {
                for (int i = 0; i < MainFieldIndex.Count; i++)
                {
                    for (int j = 0; j < MainFieldIndex[i].ExitValue.Count; j++)
                    {
                        for (int k = 0; k < MainFieldIndex.Count; k++)
                        {
                            if (MainFieldIndex[k].FieldID == MainFieldIndex[i].ExitValue.ElementAt(j))
                            {
                                MainFieldIndex[k].AllGE.Add(MainFieldIndex[i].GenValue.ElementAt(j));
                            }
                        }
                    }
                }


            }

            //      END Funcs       //


            Logic();

            void Logic()
            {
                //  cache
                CacheCheck();   //  md5 checks, restore cache from inside if needed, generate cache as fallback
                                //  FFIO created!, verified with some testing, spot checked output. quick output all rstored 


                //  scan and create Dataset<datapoint>
                FIDataset();

                //  re arrange
                //MFIgen();

                //  rng time
                //Rubiks();


                //  output dataset for testing



                void DSout()
                {
                    string temp = ""; string[] temp2 = new string[] { temp, "" };
                    int count = 1; string temp4 = ""; int[] temp5 = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                    for (int i = 0; i < Dataset.Count; i++)
                    {
                        Dataset[i].SumCheck(); Dataset[i].SumCheckBytes();
                        if (i > 0)
                        {
                            if (Dataset[i].FieldID == Dataset[i - 1].FieldID)
                            {
                                if (!(Dataset[i].RangeV == Dataset[i - 1].RangeV))
                                {
                                    if (!(Dataset[i].HeaderC == 0))
                                    {
                                        richTextBox_output.Text += "\n skipped one";
                                    }
                                }
                                if (!(Dataset[i].HeaderC == Dataset[i - 1].HeaderC))
                                {
                                    temp += "DSID=" + Dataset[i].FieldID + " r=" + Dataset[i].RangeV + " CheckSum=" + Dataset[i].CheckSum + " headerC=" + Dataset[i].HeaderC + " 00=" + Dataset[i].AddressNvalue[0][0] + " 01=" + Dataset[i].AddressNvalue[0][1] + " 10=" + Dataset[i].AddressNvalue[1][0] + " 11=" + Dataset[i].AddressNvalue[1][1] + " 20=" + Dataset[i].AddressNvalue[2][0] + " 21=" + Dataset[i].AddressNvalue[2][1] + " 30=" + Dataset[i].AddressNvalue[3][0] + " 31=" + Dataset[i].AddressNvalue[3][1] + "\n";

                                }
                            }
                            else
                            {
                                if (!(Dataset[i].HeaderC == 0))
                                {
                                    richTextBox_output.Text += "\n skipped one";
                                }
                                temp += "DSID=" + Dataset[i].FieldID + " r=" + Dataset[i].RangeV + " CheckSum=" + Dataset[i].CheckSum + " headerC=" + Dataset[i].HeaderC + " 00=" + Dataset[i].AddressNvalue[0][0] + " 01=" + Dataset[i].AddressNvalue[0][1] + " 10=" + Dataset[i].AddressNvalue[1][0] + " 11=" + Dataset[i].AddressNvalue[1][1] + " 20=" + Dataset[i].AddressNvalue[2][0] + " 21=" + Dataset[i].AddressNvalue[2][1] + " 30=" + Dataset[i].AddressNvalue[3][0] + " 31=" + Dataset[i].AddressNvalue[3][1] + "\n";

                            }


                        }
                        else
                        {
                            temp += "DSID=" + Dataset[i].FieldID + " r=" + Dataset[i].RangeV + " CheckSum=" + Dataset[i].CheckSum + " headerC=" + Dataset[i].HeaderC + " 00=" + Dataset[i].AddressNvalue[0][0] + " 01=" + Dataset[i].AddressNvalue[0][1] + " 10=" + Dataset[i].AddressNvalue[1][0] + " 11=" + Dataset[i].AddressNvalue[1][1] + " 20=" + Dataset[i].AddressNvalue[2][0] + " 21=" + Dataset[i].AddressNvalue[2][1] + " 30=" + Dataset[i].AddressNvalue[3][0] + " 31=" + Dataset[i].AddressNvalue[3][1] + "\n";

                        }


                        //temp += "DSID=" + Dataset[i].FieldID + " r=" + Dataset[i].RangeV + " CheckSum=" + Dataset[i].CheckSum + " headerC=" + Dataset[i].HeaderC + " 00=" + Dataset[i].AddressNvalue[0][0] + " 01=" + Dataset[i].AddressNvalue[0][1] + " 10=" + Dataset[i].AddressNvalue[1][0] + " 11=" + Dataset[i].AddressNvalue[1][1] + " 20=" + Dataset[i].AddressNvalue[2][0] + " 21=" + Dataset[i].AddressNvalue[2][1] + " 30=" + Dataset[i].AddressNvalue[3][0] + " 31=" + Dataset[i].AddressNvalue[3][1] + "\n";
                        //temp += "DSID=" + Dataset[i].FieldID + " r=" + Dataset[i].RangeV + " CheckSum=" + Dataset[i].CheckSum + " 00=" + Dataset[i].ValueBytes[0][0] + " 01=" + Dataset[i].ValueBytes[0][1] + " 10=" + Dataset[i].ValueBytes[1][0] + " 11=" + Dataset[i].ValueBytes[1][1] + " 20=" + Dataset[i].ValueBytes[2][0] + " 21=" + Dataset[i].ValueBytes[2][1] + " 30=" + Dataset[i].ValueBytes[3][0] + " 31=" + Dataset[i].ValueBytes[3][1] + "\n";

                    }
                    temp2 = new string[] { temp, "" }; //  using right now
                    Directory.SetCurrentDirectory("C:\\Users\\drew\\Desktop\\Testdata\\");
                    File.WriteAllLines("DSlite.txt", temp2);
                    temp = ""; temp2 = new string[] { "", "" };
                }
                void DSoutFull()
                {
                    string temp = ""; string[] temp2 = new string[] { temp, "" };
                    int count = 1; string temp4 = ""; int[] temp5 = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                    for (int i = 0; i < Dataset.Count; i++)
                    {
                        Dataset[i].SumCheck(); Dataset[i].SumCheckBytes();
                        temp += "DSID=" + Dataset[i].FieldID + " r=" + Dataset[i].RangeV + " CheckSum=" + Dataset[i].CheckSum + " headerC=" + Dataset[i].HeaderC + " 00=" + Dataset[i].AddressNvalue[0][0] + " 01=" + Dataset[i].AddressNvalue[0][1] + " 10=" + Dataset[i].AddressNvalue[1][0] + " 11=" + Dataset[i].AddressNvalue[1][1] + " 20=" + Dataset[i].AddressNvalue[2][0] + " 21=" + Dataset[i].AddressNvalue[2][1] + " 30=" + Dataset[i].AddressNvalue[3][0] + " 31=" + Dataset[i].AddressNvalue[3][1] + "\n";

                    }
                    temp2 = new string[] { temp, "" }; //  using right now
                    Directory.SetCurrentDirectory("C:\\Users\\user\\Desktop\\");
                    File.WriteAllLines("DSfull.txt", temp2);
                    temp = ""; temp2 = new string[] { "", "" };
                }

                void MFIoutFull()
                {
                    string temp = ""; string[] temp2 = new string[] { temp, "" };
                    int count = 1; string temp4 = ""; int[] temp5 = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                    for (int i = 0; i < MainFieldIndex.Count; i++)
                    {
                        string ea = "";
                        for (int j = 0; j < MainFieldIndex[i].ExitAddress.Count; j++)
                        {
                            for (int k = 0; k < MainFieldIndex[i].ExitAddress.ElementAt(j).Length; k++)
                            {
                                ea += j + "=" + MainFieldIndex[i].ExitAddress.ElementAt(j)[k] + " ";
                            }
                        }
                        string ga = "";
                        for (int j = 0; j < MainFieldIndex[i].GenAddress.Count; j++)
                        {
                            for (int k = 0; k < MainFieldIndex[i].GenAddress.ElementAt(j).Length; k++)
                            {
                                ga += j + "=" + MainFieldIndex[i].GenAddress.ElementAt(j)[k] + " ";
                            }
                        }
                        string allge = "";
                        for (int j = 0; j < MainFieldIndex[i].AllGE.Count; j++)
                        {
                            allge += j + "=" + MainFieldIndex[i].AllGE[j] + " ";
                        }
                        string ev = "";
                        for (int j = 0; j < MainFieldIndex[i].ExitValue.Count; j++)
                        {
                            ev += j + "=" + MainFieldIndex[i].ExitValue[j] + " ";
                        }
                        string gv = "";
                        for (int j = 0; j < MainFieldIndex[i].GenValue.Count; j++)
                        {
                            gv += j + "=" + MainFieldIndex[i].GenValue[j] + " ";
                        }
                        temp += "MFId=" + MainFieldIndex[i].FieldID + " exit=" + ev + " gen=" + gv + " allge=" + allge + " eAdds=" + ea + " gAdds=" + ga + "\n";
                    }
                    temp2 = new string[] { temp, "" }; //  using right now
                    Directory.SetCurrentDirectory("C:\\Users\\user\\Desktop\\");
                    File.WriteAllLines("MFIFull.txt", temp2);
                    temp = ""; temp2 = new string[] { "", "" };
                }
                void MFIoutLite()
                {
                    string temp = ""; string[] temp2 = new string[] { temp, "" };
                    int count = 1; string temp4 = ""; int[] temp5 = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                    for (int i = 0; i < MainFieldIndex.Count; i++)
                    {
                        string ea = "";
                        for (int j = 0; j < MainFieldIndex[i].ExitAddress.Count; j++)
                        {
                            for (int k = 0; k < MainFieldIndex[i].ExitAddress.ElementAt(j).Length; k++)
                            {
                                ea += j + "=" + MainFieldIndex[i].ExitAddress.ElementAt(j)[k] + " ";
                            }
                        }
                        string ga = "";
                        for (int j = 0; j < MainFieldIndex[i].GenAddress.Count; j++)
                        {
                            for (int k = 0; k < MainFieldIndex[i].GenAddress.ElementAt(j).Length; k++)
                            {
                                ga += j + "=" + MainFieldIndex[i].GenAddress.ElementAt(j)[k] + " ";
                            }
                        }
                        string allge = "";
                        for (int j = 0; j < MainFieldIndex[i].AllGE.Count; j++)
                        {
                            allge += j + "=" + MainFieldIndex[i].AllGE[j] + " ";
                        }
                        string ev = "";
                        for (int j = 0; j < MainFieldIndex[i].ExitValue.Count; j++)
                        {
                            ev += j + "=" + MainFieldIndex[i].ExitValue[j] + " ";
                        }
                        string gv = "";
                        for (int j = 0; j < MainFieldIndex[i].GenValue.Count; j++)
                        {
                            gv += j + "=" + MainFieldIndex[i].GenValue[j] + " ";
                        }
                        temp += "MFId=" + MainFieldIndex[i].FieldID + " exit=" + ev + " gen=" + gv + " allge=" + allge + "\n";
                    }
                    temp2 = new string[] { temp, "" }; //  using right now
                    Directory.SetCurrentDirectory("C:\\Users\\user\\Desktop\\");
                    File.WriteAllLines("MFIlite.txt", temp2);
                    temp = ""; temp2 = new string[] { "", "" };
                }


                void RangeVCheck()
                {
                    string temp = ""; string[] temp2 = new string[] { temp, "" };
                    int count = 1; string temp4 = ""; int[] temp5 = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                    for (int i = 0; i < Dataset.Count; i++)
                    {

                        if (i > 0)
                        {
                            //count r=n per field id
                            if ((Dataset[i].FieldID == Dataset[i - 1].FieldID) && (Dataset[i].RangeV == Dataset[i - 1].RangeV))
                            {
                                count++;
                            }
                            else if ((Dataset[i].FieldID == Dataset[i - 1].FieldID) && (Dataset[i].RangeV > Dataset[i - 1].RangeV))
                            {
                                //temp4 += "F=" + Dataset[i - 1].FieldID + " R=" + Dataset[i - 1].RangeV + " exitCount=" + count + "\n";
                                temp5[Dataset[i - 1].RangeV] = count;
                                count = 1;

                            }
                            else if (!(Dataset[i].FieldID == Dataset[i - 1].FieldID))
                            {
                                temp5[Dataset[i - 1].RangeV] = count;
                                bool temp6 = (temp5[0] == temp5[1] && temp5[0] == temp5[2] && temp5[0] == temp5[3] && temp5[0] == temp5[4] && temp5[0] == temp5[5] && temp5[0] == temp5[6]);
                                temp += "F=" + Dataset[i - 1].FieldID + " language range bool=" + temp6 + "\n";
                                count = 1;

                            }

                            if ((i == Dataset.Count - 1) && (counter > 1)) //   last run
                            {
                                temp5[Dataset[i - 1].RangeV] = count;
                                bool temp6 = (temp5[0] == temp5[1] && temp5[0] == temp5[2] && temp5[0] == temp5[3] && temp5[0] == temp5[4] && temp5[0] == temp5[5] && temp5[0] == temp5[6]);
                                temp += "F=" + Dataset[i - 1].FieldID + " language range bool=" + temp6 + "\n";
                                count = 1;
                            }
                        }

                    }
                    temp2 = new string[] { temp, "" }; //  using right now
                    Directory.SetCurrentDirectory("C:\\Users\\drew\\Desktop\\Testdata\\");
                    File.WriteAllLines("DSfull.txt", temp2);
                    temp = ""; temp2 = new string[] { "", "" };

                }


                //DSout();
                //  testing output with this func
                //RangeVCheck();
                DSoutFull();
                //DS3out();
                //  output to txt file
                MFIgen();
                MFIgv();
                MFIoutFull();
                MFIoutLite();




                //  connect all exits to verify full map match
                Mapp();

                //  math to scramble
                //Rubiks();

                //  write bin
                Ink();


                void Testing()
                {

                    //      LINES OUTPUT TEST CODE


                    /*
                string temp = ""; string[] temp2 = new string[] { temp, "" };
                for (int i = 0; i < FinalFieldIndices.Count; i++)
                {
                    FinalFieldIndices[i].SumCheck();
                    temp += "\nFFID= " + FinalFieldIndices[i].FieldID + " CheckSum= " + FinalFieldIndices[i].CheckSum + " 00=" + FinalFieldIndices[i].RangeStartStop[0][0] + " 01=" + FinalFieldIndices[i].RangeStartStop[0][1] + " 10=" + FinalFieldIndices[i].RangeStartStop[1][0] + " 11=" + FinalFieldIndices[i].RangeStartStop[1][1] + " 20=" + FinalFieldIndices[i].RangeStartStop[2][0] + " 21=" + FinalFieldIndices[i].RangeStartStop[2][1] + " 30=" + FinalFieldIndices[i].RangeStartStop[3][0] + " 31=" + FinalFieldIndices[i].RangeStartStop[3][1] + " 40=" + FinalFieldIndices[i].RangeStartStop[4][0] + " 41=" + FinalFieldIndices[i].RangeStartStop[4][1] + " 50=" + FinalFieldIndices[i].RangeStartStop[5][0] + " 51=" + FinalFieldIndices[i].RangeStartStop[5][1] + " 60=" + FinalFieldIndices[i].RangeStartStop[6][0] + " 61=" + FinalFieldIndices[i].RangeStartStop[6][1] + "\n";

                }
                temp2 = new string[] { temp, "" };
                Directory.SetCurrentDirectory("C:\\Users\\drew\\Desktop\\Testdata\\");
                File.WriteAllLines("Final Field Indexes restiored from cache object inside program.txt", temp2);
                temp = ""; temp2 = new string[] { "", "" };
                */


                    /*
                    string temp = ""; string[] temp2 = new string[] { temp, "" };
                    for (int i = 0; i < FinalFieldIndices.Count; i++)
                    { temp += "FFI: fID= " + FinalFieldIndices[i].FieldID + " CheckSum= " + FinalFieldIndices[i].CheckSum; }
                    temp2 = new string[] { temp, "" };
                    Directory.SetCurrentDirectory("C:\\Users\\drew\\Desktop\\Testdata\\");
                    File.WriteAllLines("Final Field Indexes.txt", temp2);                
                    temp = ""; temp2 = new string[] { "", "" };
                    */

                    //CacheIO("read SB_FFIO");
                    //GenFieldIndex();
                    //GenFinalFieldIndex();
                    /*
                    using (var md5 = MD5.Create())
                    {
                        string md52;
                        string dirTest = "\\SB_FFIO_mog.bin";
                        //  md5 p0data7.bin
                        using (var stream = File.OpenRead(tb_fl.Text + dirTest))
                        { md52 = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant(); }
                        richTextBox_output.Text += "\n" + md52;
                    }
                    */
                    //richTextBox_output.Text += "\n" + md52;
                    /*
                    if (FinalFieldIndices.Count > 0)
                    {
                        richTextBox_output.Text += "\nFFI count= " + FinalFieldIndices.Count + " spot " + FinalFieldIndices[500].RangeStartStop[3][1];

                    }
                    */
                }


            }


            void Mapp()
            {
                // here we try to build connections using DS
                /*              
                 *              draw a box
                 */




            }

            
            void Ink()
            {

            }

            //      end of Bytes_IO()
        }


        //  ONE PER ENTRANCE, 7 per FieldID
        [Serializable]
        public class Datapoint
        {
            private int fieldID;
            private string fieldName;
            private bool checkSum;
            private bool checkSumBytes;
            private int rangeV;
            private int headerC;
            private bool touched;
            private int[][] addressNvalue;  //  [4][2]
            private byte[][] valueBytes;    //  [4][2]
            public Datapoint(int fieldID, string fieldName, bool checkSum, bool checkSumBytes, int rangeV, int headerC, bool touched, int[][] addressNvalue, byte[][] valueBytes)
            {
                this.fieldID = fieldID;
                this.fieldName = fieldName;
                this.checkSum = checkSum;
                this.checkSumBytes = checkSumBytes;
                this.rangeV = rangeV;
                this.headerC = headerC;
                this.touched = touched;
                this.addressNvalue = addressNvalue;
                this.valueBytes = valueBytes;
            }
            public int FieldID
            {
                get { return fieldID; }
                set { fieldID = value; }
            }
            public string FieldName
            {
                get { return fieldName; }
                set { fieldName = value; }
            }
            public bool CheckSum
            {
                get { return checkSum; }
                set { checkSum = value; }
            }
            public bool CheckSumBytes
            {
                get { return checkSumBytes; }
                set { checkSumBytes = value; }
            }
            public int RangeV
            {
                get { return rangeV; }
                set { rangeV = value; }
            }
            public int HeaderC
            {
                get { return headerC; }
                set { headerC = value; }
            }
            public bool Touched
            {
                get { return touched; }
                set { touched = value; }
            }
            public int[][] AddressNvalue
            {
                get { return addressNvalue; }
                set { addressNvalue = value; }
            }
            public byte[][] ValueBytes
            {
                get { return valueBytes; }
                set { valueBytes = value; }
            }
            public void SumCheck()
            {
                checkSum = false;
                checkSum = ((addressNvalue[0][1] == addressNvalue[1][1]) && (addressNvalue[0][1] == addressNvalue[3][1]));
            }
            public void SumCheckBytes()
            {
                checkSumBytes = false;
                checkSumBytes = ((valueBytes[0][0] == valueBytes[1][0]) && (valueBytes[0][0] == valueBytes[3][0]) && (valueBytes[0][1] == valueBytes[1][1]) && (valueBytes[0][1] == valueBytes[3][1]));
            }
        }

        //  ONE PER FIELD_ID
        [Serializable]
        public class FieldIndex
        {
            private int fieldID;
            private string fieldName;
            private byte[] fieldBytes;
            private bool checkSum;
            private int[][] rangeStartStop; //  [7][2]
            public FieldIndex(int fieldID, string fieldName, byte[] fieldBytes, bool checkSum, int[][] rangeStartStop)
            {
                this.fieldID = fieldID;
                this.fieldName = fieldName;
                this.fieldBytes = fieldBytes;
                this.checkSum = checkSum;
                this.rangeStartStop = rangeStartStop;
            }
            public int FieldID
            {
                get { return fieldID; }
                set { fieldID = value; }
            }
            public string FieldName
            {
                get { return fieldName; }
                set { fieldName = value; }
            }
            public byte[] FieldBytes
            {
                get { return fieldBytes; }
                set { fieldBytes = value; }
            }
            public bool CheckSum
            {
                get { return checkSum; }
                set { checkSum = value; }
            }
            public int[][] RangeStartStop
            {
                get { return rangeStartStop; }
                set { rangeStartStop = value; }
            }
            public void SumCheck()
            {
                checkSum = false; bool range1 = false; bool range2 = false; bool range3 = false; bool range4 = false; bool range5 = false; bool range6 = false; bool range7 = false;
                if (rangeStartStop[0][0] < rangeStartStop[0][1]) { range1 = true; }
                if (rangeStartStop[1][0] < rangeStartStop[1][1]) { range2 = true; }
                if (rangeStartStop[2][0] < rangeStartStop[2][1]) { range3 = true; }
                if (rangeStartStop[3][0] < rangeStartStop[3][1]) { range4 = true; }
                if (rangeStartStop[4][0] < rangeStartStop[4][1]) { range5 = true; }
                if (rangeStartStop[5][0] < rangeStartStop[5][1]) { range6 = true; }
                if (rangeStartStop[6][0] < rangeStartStop[6][1]) { range7 = true; }
                checkSum = (range1 && range2 && range3 && range4 && range5 && range6 && range7);
            }
        }
        public class FieldData
        {
            private int fieldID;
            private string fieldName;
            private List<int> exitNum;    //  headerC
            private List<int[]> exitAddress;  //  21  (exit,exit,---,exit * 7)
            private List<int[]> genAddress;   //  7   (---,---,gen,--- * 7)
            private List<int> exitValue;    //  1 per exit in field, 
            private List<int> genValue;
            private List<int> allGE;
            public FieldData(int fieldID, string fieldName, List<int> exitNum, List<int[]> exitAddress, List<int[]> genAddress, List<int> exitValue, List<int> genValue, List<int> allGE)
            {
                this.fieldID = fieldID;
                this.fieldName = fieldName;
                this.exitNum = exitNum;
                this.exitAddress = exitAddress;
                this.genAddress = genAddress;
                this.exitValue = exitValue;
                this.genValue = genValue;
                this.allGE = allGE;
            }
            public int FieldID
            {
                get { return fieldID; }
                set { fieldID = value; }
            }
            public string FieldName
            {
                get { return fieldName; }
                set { fieldName = value; }
            }
            public List<int> ExitNum
            {
                get { return exitNum; }
                set { exitNum = value; }
            }
            public List<int[]> ExitAddress
            {
                get { return exitAddress; }
                set { exitAddress = value; }
            }
            public List<int[]> GenAddress
            {
                get { return genAddress; }
                set { genAddress = value; }
            }
            public List<int> ExitValue
            {
                get { return exitValue; }
                set { exitValue = value; }
            }
            public List<int> GenValue
            {
                get { return genValue; }
                set { genValue = value; }
            }
            public List<int> AllGE
            {
                get { return allGE; }
                set { allGE = value; }
            }
        }



        //  END of rand9er : form

    }

}