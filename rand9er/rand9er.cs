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
        public static string data_str, pswap, binloc, set, seed = "42", tb_flText, textBoxSeed;
        public static int data_int, counter, randl = 23, items = 236;
        public static int weapa = 0, weapb = 85, armleta = 88, armletb = 112, helma = 112;
        public static int helmb = 148, armora = 148, armorb = 192, acca = 192, accb = 224;
        public static byte[] ba_p0data2, ba_p0data7;
        public static char[] separators = new char[] { ';', ';' };
        public static int[] data_arr, a_empty = { 0 };
        public static string[] medicShops, a_stockShopItems, a_synthdata, a_stockSynthesis, a_statdata, a_stockBaseStats, a_equipdata, a_stockDefaultEquipment, a_itemdata, a_stockItems, a_gemdata, a_stockAbilityGems, a_comboSafe = new string[32];
        public static int[][] aa_medicItems, aa_shopItems;
        public static int[] a_shopItems = new int[] { 16, 16, 9, 13, 25, 18, 28, 13, 14, 32, 14, 32, 29, 21, 22, 25, 21, 30, 21, 30, 6, 12, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //default
        public static int[] a_shopItems_1safe = new int[] { 16, 16, 9, 13, 25, 18, 28, 13, 14, 32, 14, 32, 29, 21, 22, 25, 21, 30, 21, 30, 6, 12, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //filled zeros so no exceptions
        public static int[] a_shopItems_2max = new int[] { 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static int[] a_shopItems_2maxm = new int[] { 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32 };
        public static int[] a_shopItems_3rand = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static bool c_mogBool, c_safeBool, c_maxBool, c_randomBool, c_medicshopsBool, c_medicitemsBool, c_medicshopsEn, c_randomEn, c_maxEn, c_requireBool, c_resultBool, c_pricesBool, c_all_eEn, c_all_eBool, c_random_eEn, c_random_eBool, c_main_eBool, c_abilitygemsBool, c_defaultBool, c_basestatsBool;
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
                c_safeBool = true;
                c_random.Checked = false; 
                c_max.Checked = false; 
                a_shopItems = a_shopItems_1safe; 
            } else if (!c_safe.Checked)
            {
                c_safeBool = false;
            }

        }
        private void c_random_CheckedChanged(object sender, EventArgs e)
        {
            if (c_random.Checked) 
            {
                c_randomBool = true;
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
            else if (!c_random.Checked)
            {
                c_randomBool = false;
            }
            if (!c_max.Checked & !c_random.Checked)
            {
                c_medicitems.Enabled = false;
                c_medicshops.Enabled = false;
            }
        }
        private void c_random_EnabledChanged(object sender, EventArgs e)
        {
            if (c_random.Enabled)
            {
                c_randomEn = true;
            }
            if (!c_random.Enabled)
            {
                c_randomEn = false;
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
                c_maxBool = true;
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
            else if (!c_max.Checked)
            {
                c_maxBool = false;
            }
            if (!c_max.Checked & !c_random.Checked)
            {
                c_medicitems.Enabled = false;
                c_medicshops.Enabled = false;
            }
        }
        private void c_max_EnabledChanged(object sender, EventArgs e)
        {
            if (c_max.Enabled)
            {
                c_maxEn = true;
            }
            if (!c_max.Enabled)
            {
                c_maxEn = false;
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
            if (c_medicitems.Checked == true) { items = 250; c_medicitemsBool = true; }
            else
            if (c_medicitems.Checked == false) { items = 235; c_medicitemsBool = false; }

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
                c_medicshopsBool = true;
                if (c_max.Checked == true) { a_shopItems = a_shopItems_2maxm; }
            }
            if (c_medicshops.Checked == false)
            {
                c_medicshopsBool = false;
                if (c_max.Checked == true) { a_shopItems = a_shopItems_2max; }
            }
        }
        private void c_medicshops_EnabledChanged(object sender, EventArgs e)
        {
            
            if (c_medicshops.Enabled == true)
            {
                c_medicshopsEn = true;
            }
            if (c_medicshops.Enabled == false)
            {
                c_medicshopsEn = false;
            }
        }
        private void c_basestats_CheckedChanged(object sender, EventArgs e)
        {
            if (c_basestats.Checked)
            {
                c_basestatsBool = true;
            }
            if (!c_basestats.Checked)
            {
                c_basestatsBool = false;
            }
        }
        private void c_abilitygems_CheckedChanged(object sender, EventArgs e)
        {
            if (c_abilitygems.Checked)
            {
                c_abilitygemsBool = true;
            }
            if (!c_abilitygems.Checked)
            {
                c_abilitygemsBool = false;
            }
        }
        private void c_default_CheckedChanged(object sender, EventArgs e)
        {
            if (c_default.Checked)
            {
                c_defaultBool = true;
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
                c_defaultBool = false;
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
                c_main_eBool = true;
                c_random_e.Checked = false;
                c_all_e.Checked = false;
            }
            if (!c_main_e.Checked)
            {
                c_main_eBool = false;
            }
        }
        private void c_random_e_CheckedChanged(object sender, EventArgs e)
        {
            if (c_random_e.Checked)
            {
                c_random_eBool = true;
                c_all_e.Checked = false;
                c_main_e.Checked = false;
            }
            if (!c_random_e.Checked)
            {
                c_random_eBool = false;
            }
        }
        private void c_random_e_EnabledChanged(object sender, EventArgs e)
        {
            if (c_random_e.Enabled)
            {
                c_random_eEn = true;
            }
            if (!c_random_e.Enabled)
            {
                c_random_eEn = false;
            }
        }
        private void c_all_e_CheckedChanged(object sender, EventArgs e)
        {
            if (c_all_e.Checked)
            {
                c_all_eBool = true;
                c_random_e.Checked = false;
                c_main_e.Checked = false;
            }
            if (!c_all_e.Checked)
            {
                c_all_eBool = false;
            }
            
        }
        private void c_all_e_EnabledChanged(object sender, EventArgs e)
        {
            if (c_all_e.Enabled)
            {
                c_all_eEn = true;
            }
            if (!c_all_e.Enabled)
            {
                c_all_eEn = false;
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
            if (c_require.Checked)
            {
                c_requireBool = true;
            } 
            else if (!c_require.Checked)
            {
                c_requireBool = false;
            }
        }
        private void c_result_CheckedChanged(object sender, EventArgs e)
        {
            if (c_result.Checked)
            {
                c_resultBool = true;
            }
            else if (!c_result.Checked)
            {
                c_resultBool = false;
            }
        }
        private void c_prices_CheckedChanged(object sender, EventArgs e)
        {
            if (c_prices.Checked)
            {
                c_pricesBool = true;
            }
            else if (!c_prices.Checked)
            {
                c_pricesBool = false;
            }
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
            tb_flText = @tb_fl.Text;
        }
        private void l_shopm_Click(object sender, EventArgs e)
        {

        }
        private void label2_Click(object sender, EventArgs e)
        {

        }
        private void textBox_seed_TextChanged(object sender, EventArgs e)
        {
            textBoxSeed =  textBox_seed.Text;
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
        private void c_mogdetect_CheckedChanged(object sender, EventArgs e)
        {
            if (c_mogdetect.Checked)
            {
                c_mogBool = true;
            }
            if (!c_mogdetect.Checked)
            {
                c_mogBool = false;
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
            else if (!c_esteals.Checked)
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
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            //richTextBox1.SelectionStart = richTextBox_output.Text.Length;
            //richTextBox1.ScrollToCaret();
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
                    if (textBox_seed.Text.Length == 0) { seed = "42";textBox_seed.Text = seed; }
                    seed = textBox_seed.Text;
                    Seed.SeedIngest();
                    Seed.Compressor();
                    Seed.CleanSeedInt();
                    if (cm_itemshop.Checked)
                    {
                        Shops.ShopItems();
                        Shops.MedicItems();
                        Shops.ShopCombo();
                    }
                    if (cm_synth.Checked)
                    {
                        Synth.Synthesis();
                    }
                    if (cm_char.Checked)
                    {
                        Char.Character();
                    }
                    //richTextBox_debug.Text = richTextBox_debug.Text + "\nrandl: " + randl.ToString() + "\nSeed DNA " + data_int + "\nitems: (1-" + items + ")\n";
                    
                    if (cm_itemshop.Checked | cm_synth.Checked | cm_char.Checked)
                    {
                        ReadWrite();
                    }
                    if (cm_enemies.Checked | cm_entrances.Checked)
                    {
                        Bytes.Bytes_IO();
                    }
                    pbar_tree.Value = pbar_tree.Maximum;
                }
            }
        }
        //MISC FUNCS
        public string path_search(string pswap)
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
        public void Manual_search()
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
        public void ReadWrite()
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
        public void IO_stock()
        {
            b_restore.Text = "Restoring...";
            var t = Task.Run(async delegate
            {
                await Task.Delay(600);
            });
            t.Wait();
            if (cm_itemshop.Checked)
            {
                a_stockShopItems = StockCSVs.StockShopItemsCSV();
                Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Items");
                File.WriteAllLines("ShopItems.csv", a_stockShopItems);
            }
            if (cm_synth.Checked)
            {
                a_stockSynthesis = StockCSVs.StockSynthesisCSV();
                Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Items");
                File.WriteAllLines("Synthesis.csv", a_stockSynthesis);
            }
            if (cm_char.Checked)
            {

                if (c_abilitygems.Checked)
                {
                    a_stockAbilityGems = StockCSVs.StockAbilityGemsCSV();
                    Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Characters\\Abilities");
                    File.WriteAllLines("AbilityGems.csv", a_stockAbilityGems);
                }
                if (c_basestats.Checked)
                {
                    a_stockBaseStats = StockCSVs.StockBaseStatsCSV();
                    Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Characters");
                    File.WriteAllLines("BaseStats.csv", a_stockBaseStats);
                }
                if (c_default.Checked)
                {
                    a_stockDefaultEquipment = StockCSVs.StockDefaultEquipmentCSV();
                    Directory.SetCurrentDirectory(@tb_fl.Text + "\\StreamingAssets\\Data\\Characters");
                    File.WriteAllLines("DefaultEquipment.csv", a_stockDefaultEquipment);
                    a_stockItems = StockCSVs.StockItemsCSV();
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
        public void MogDetect()
        {
            if (Directory.Exists(@tb_fl.Text+ "\\MoguriFiles\\StreamingAssets"))
            {
                c_mogdetect.Checked = true;
                c_mogdetect.Text = "Moguri detected";
                binloc = tb_flText + "\\MoguriFiles\\StreamingAssets";
            } else
            {
                binloc = tb_flText + "\\StreamingAssets";
            }
        }
        
    }

}