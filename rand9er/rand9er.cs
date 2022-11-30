using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
//using System.Reflection;
//using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using rand9er;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
//using System.Security.Cryptography;
//using System.Runtime.Serialization.Formatters.Binary;

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
        public static int data_int = 0, counter, randl = 23, items = 236;
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
        public static bool pathFound = false, pathSeedFolder = false;
        public static string pathSeed = "";

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox_seed.Text = seed;
            tb_fl.Text = path_search(pswap);
        }
        
        //      MAIN Program Execution        //
        //      MAIN Program Execution        //

        private void button_rand_Click(object sender, EventArgs e)
        {
            // ffix location, run seed and create folder
            if (pathFound)
            {
                if (MessageBox.Show("This will save all Randoms a new Mod folder", "Cormfirm Write Data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                {
                    ExecuteProgram();
                }
            }
        }

        void ExecuteProgram()
        {
            //Gaurentee Seed
            if (textBox_seed.Text.Length == 0)
            {
                seed = "42";
                textBox_seed.Text = seed;
            }
            seed = textBox_seed.Text;

            //string "seed" has input data rand9er.textBox_seed
            data_str = "";
            counter = 1;
            // test if valid seed and use
            if (Seed.TestSeed() == false)
            {
                Seed.SeedIngest();
                Seed.Compressor();
                Seed.CleanSeedInt();
            }
            textBox_seed.Text = data_int.ToString();

            //Create new mod folder with seed name
            pathSeed = @tb_fl.Text + "StiltzkinsBag2.0-Seed-" + data_int.ToString();
            DirectoryInfo di = Directory.CreateDirectory(pathSeed);

            // Seed done
            // Path done

            // ready to continue work

            // need to read memoria.ini for mods to pull data from

            // Copy over CSV files from mod
            // we need to read the memoria.ini file for mods.
            // ff9 path, open memoria.ini, search for "[Mod]"
            // next line is "FolderNames=" need to read each folder name in quotes. make a list. add "StreamingAssets" to the end.
            // if any list item contains Stilkin's bag, delete that list item.
            // search first folder for "Data\Battle|Characters|Items"
            // and then down the list until finally StreamingAssets.
            // This ensures we use any current mod folder that has CSVs except Stilken's Bag seeds.
            // Duplicate the CSV folders



            // So our new Seed folder is prepped with CSV files to use in randomizatiion.


            // BIN file extraction

            // Next we search the same folders for BINs, if we find p0data2 or p0data7, we will use this file. IF not, we use the BIN's in streaming assets. 
            // This allows this mod to work with Moguri, defaut (with memoria engine), and very likely any other mod.








            // After writting mods, we should write a Seed_Source.txt and in it, report which mod/stock we are pulling our data from.

            // create txt file with randomized details



            // Finally, we add the seed folder name to the START of the list of MemoriaMods, and then write that line back into the memoria.ini file.
            // Having our mod first ensures, any *.bytes files and BINs are used in place of other mods.
            // This should in theory, make our mod compatble with all other mods. Alternate Fantasy etc.



            //  END MAIN CODE   //
            //  END MAIN CODE   //
            //  END MAIN CODE   //
            //  END MAIN CODE   //


            // Refactor layout of checkbox options.

            //RANDOMZIZNIG CSVs need refactor
            if (cm_itemshop.Checked | cm_synth.Checked | cm_char.Checked | cm_enemies.Checked | cm_entrances.Checked)
            {
                if (MessageBox.Show("This will save all Randoms to game files", "Cormfirm Write Data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                {
                    //reset
                    pbar_tree.Value = pbar_tree.Minimum;

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
                    richTextBox_debug.Text = richTextBox_debug.Text + "\nrandl: " + randl.ToString() + "\nSeed DNA " + data_int + "\nitems: (1-" + items + ")\n";


                    //writing data needs refactor
                    if (cm_itemshop.Checked | cm_synth.Checked | cm_char.Checked)
                    {
                        ReadWrite();
                    }

                    //Enemies binary needs refactor
                    if (cm_enemies.Checked | cm_entrances.Checked)
                    {
                        Bytes.Bytes_IO();
                    }
                    pbar_tree.Value = pbar_tree.Maximum;
                }
            }

        }



        //              MISC FUNCS              //
        //              MISC FUNCS              //

        // Refactor new method
        public void ReadWrite()
        {

            





            button_rand.Text = "Writing Data";
            var t = Task.Run(async delegate
            {
                await Task.Delay(600);
            });
            t.Wait();



            // Refactor Writing data. use Async
            // 

            // use seed folder created before this.  @tb_fl.Text \\ seedfolder \\ CSV locations

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

                    if (c_all_e.Checked | c_random_e.Checked | c_main_e.Checked)
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

        // Refactor Stock IO
        public void IO_stock()
        {
            // Remove SB from mod load order

            // read memoria.ini and update mod list







        }


        // Misc UI elements
        public string path_search(string pswap)
        {
            string path_search1 = "", path_search2 = "", path_search3 = "";
            RegistryKey rkTest = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\Shell\\MuiCache");
            RegistryKey rkTest2 = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Store");
            RegistryKey rkTest3 = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FeatureUsage\\AppSwitched");
            string[] vnames = rkTest.GetValueNames();
            string[] vnames2 = rkTest2.GetValueNames();
            string[] vnames3 = rkTest3.GetValueNames();

            foreach (string s in vnames)
            {
                RegistryValueKind rvk = rkTest.GetValueKind(s);
                if (rvk == RegistryValueKind.String)
                {
                    string value = (string)rkTest.GetValue(s);
                    if (value == "FINAL FANTASY IX")
                    {
                        path_search1 = s.Substring(0, s.IndexOf("FF9_Launcher.exe"));
                    }
                }
            }
            foreach (string s in vnames2)
            {
                if (s.Contains("FF9_Launcher.exe"))
                {
                    path_search2 = s.Substring(0, s.IndexOf("FF9_Launcher.exe"));
                }
            }
            foreach (string s in vnames3)
            {
                if (s.Contains("FF9_Launcher.exe"))
                {
                    path_search3 = s.Substring(0, s.IndexOf("FF9_Launcher.exe"));
                }
            }

            rkTest.Close();
            rkTest2.Close();
            rkTest3.Close();

            // check each
            if (path_search1.Length > 0)
            {
                if (MessageBox.Show(path_search1, "Use this Path?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                {
                    pswap = path_search1;
                    pathFound = true;
                }
            }
            if (path_search2.Length > 0)
            {
                if (MessageBox.Show(path_search2, "Use this Path?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                {
                    pswap = path_search2;
                    pathFound = true;
                }
            }
            if (path_search3.Length > 0)
            {
                if (MessageBox.Show(path_search3, "Use this Path?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                {
                    pswap = path_search3;
                    pathFound = true;
                }
            }
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
                pathFound = true;
            }
        }
        private void b_restore_Click(object sender, EventArgs e)
        {
            
            //need to write
            //actually I dont think we need to do this. DISABLED for now

            if (MessageBox.Show("Selected options will be restored to Stock settings", "Cormfirm Write Data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
            {
                IO_stock();
            }
        }
        private void richTextBox_debug_TextChanged(object sender, EventArgs e)
        {
            richTextBox_debug.SelectionStart = richTextBox_debug.Text.Length;
            richTextBox_debug.ScrollToCaret();
        }
        private void b_rseed_Click_1(object sender, EventArgs e)
        {
            Random rnd = new Random();
            textBox_seed.Text = rnd.Next(100000000, 999999999).ToString();
        }
        private void b_open_Click(object sender, EventArgs e)
        {
            Manual_search();
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
        private void cm_itemshop_CheckedChanged(object sender, EventArgs e)
        {
            
        }
        private void cm_synth_CheckedChanged(object sender, EventArgs e)
        {
            if (cm_synth.Checked)
            {
                c_require.Enabled = true;
                c_result.Enabled = true;
                c_prices.Enabled = true;
            }
            else if (!cm_synth.Checked)
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
                c_random_eEn = true;
                c_all_e.Checked = false;
                c_main_e.Checked = false;
            }
            if (!c_random_e.Checked)
            {
                c_random_eBool = false;
                c_random_eEn = false;
            }
        }
        private void c_all_e_CheckedChanged(object sender, EventArgs e)
        {
            if (c_all_e.Checked)
            {
                c_all_eEn = true;
                c_all_eBool = true;
                c_random_e.Checked = false;
                c_main_e.Checked = false;
            }
            if (!c_all_e.Checked)
            {
                c_all_eEn = false;
                c_all_eBool = false;
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
        //test
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //          change source selection
            // part of Field Randomizing, ignoring for now
            var instance = new Logic();
            instance.LogicOut();
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //          background pic
        }
        private void tb_fl_TextChanged(object sender, EventArgs e)
        {
            tb_flText = @tb_fl.Text;
        }
        private void textBox_seed_TextChanged(object sender, EventArgs e)
        {
            textBoxSeed = textBox_seed.Text;
        }
        private void cm_treasure_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void cm_stiltzkin_CheckedChanged(object sender, EventArgs e)
        {

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
        private void c_es4_CheckedChanged(object sender, EventArgs e)
        {
            // steal slot 4
        }
        private void c_ed1_CheckedChanged(object sender, EventArgs e)
        {
            // drop slot 1
        }
        private void c_ed4_CheckedChanged(object sender, EventArgs e)
        {
            // drop slot 4
        }
        private void Serial_But_CheckedChanged(object sender, EventArgs e)
        {
            /*if (Serial_But.Checked)
            {
                bool c_serial = true;
            }*/
        }


    }


}