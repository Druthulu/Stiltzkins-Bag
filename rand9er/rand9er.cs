using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
//using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using rand9er;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Runtime.CompilerServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
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
        private Thread trd;
        string seedFolderName = "";
        private void character_CheckedChanged(object sender, EventArgs e)
        {
            charBaseStats.Enabled = character.Checked;
            charSpeciality.Enabled = character.Checked;
            charAbilities.Enabled = character.Checked;
            charEquipment.Enabled = character.Checked;
        }
        private void item_CheckedChanged_1(object sender, EventArgs e)
        {
            itemsTreasure.Enabled = item.Checked;
            itemsShops.Enabled = item.Checked;
            itemsSynth.Enabled = item.Checked;
        }
        private void enemies_CheckedChanged(object sender, EventArgs e)
        {
            enemyItemDrops.Enabled = enemies.Checked;
            enemyItemSteals.Enabled = enemies.Checked;
            enemyCardDrop.Enabled = enemies.Checked;
            enemyBlueMagic.Enabled = enemies.Checked;
        }
        private void tm_CheckedChanged_1(object sender, EventArgs e)
        {
            tmCardStats.Enabled = tm.Checked;
            tmCardOrder.Enabled = tm.Checked;
            tmDecks.Enabled = tm.Checked;
        }
        private void checkBoxRecommended_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxRecommended.Checked)
            {
                checkBoxChaos.Checked = false;
                character.Checked = true;
                item.Checked = true;
                enemies.Checked = true;
                tm.Checked = true;
            }
            else
            {
                checkBoxChaos.Checked = true;
            }
        }
        private void checkBoxChaos_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxChaos.Checked)
            {
                checkBoxRecommended.Checked = false;
            }
            else
            {
                checkBoxRecommended.Checked = true;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox_seed.Text = seed;
            tb_fl.Text = path_search(pswap);
            checkBoxRecommended.Checked = true;
        }
        private void button_rand_Click(object sender, EventArgs e)
        {
            // ffix location, run seed and create folder
            if (pathFound)
            {
                if (MessageBox.Show("Ransomize Selected Options", "Cormfirm Write Data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                {
                    //Gaurentee Seed
                    if (textBox_seed.Text.Length == 0)
                    {
                        seed = "42";
                        textBox_seed.Text = seed;
                    }
                    seed = textBox_seed.Text;

                    // Run Seed Code if needed, Reuse seed if valid
                    if (Seed.TestSeed() == false)
                    {
                        Seed.SeedIngest();
                        Seed.Compressor();
                        Seed.CleanSeedInt();
                    }
                    textBox_seed.Text = data_int.ToString();

                    //Create new mod folder with seed name
                    string seedFolderName = "StiltzkinsBag2.0-Seed-" + data_int.ToString();

                    Dictionary<string, bool> options = new Dictionary<string, bool>();
                    options.Add("Version", checkBoxRecommended.Checked);
                    options.Add("Char", character.Checked);
                    options.Add("Enemies", enemies.Checked);
                    options.Add("Items", item.Checked);
                    options.Add("TetraMaster", tm.Checked);

                    // Launch work sending options and seed and dir
                    //MessageBox.Show("This is the main thread");
                    Thread trd = new Thread(() => ThreadTask(options, Int32.Parse(textBox_seed.Text), @tb_fl.Text, seedFolderName));
                    trd.IsBackground = true;
                    trd.Start();
                }
            }
            else
            {
                if (MessageBox.Show("You need to specify the FFIX install path first", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                {
                    Manual_search();
                }
            }
        }

        private void ThreadTaskDemo(Dictionary<string, bool> opt, int seedling, string dir)
        {
            int stp;
            int newval;
            Random rnd = new Random();

            // Test to see if data made into this thread
            MethodInvoker n = new MethodInvoker(() => richTextBox_debug.Text += @dir);
            richTextBox_debug.Invoke(n);
            // yep
            // works
            while (true)
            {
                stp = this.progressBar2.Step * rnd.Next(-1, 2);
                newval = this.progressBar2.Value + stp;
                if (newval > this.progressBar2.Maximum)
                    newval = this.progressBar2.Maximum;
                else if (newval < this.progressBar2.Minimum)
                    newval = this.progressBar2.Minimum;
                //this.progressBar1.Value = newval;
                Thread.Sleep(100);

                MethodInvoker m = new MethodInvoker(() => progressBar2.Value = newval);
                progressBar2.Invoke(m);
            }
        }
        private void ThreadTask(Dictionary<string, bool> opt, int seedling, string dir, string sbfolder)
        {

            if (opt["Char"])
            {
                // Character
            }
            if (opt["Enemies"])
            {
                // Enemy
            }
            if (opt["Items"])
            {
                // Items
            }
            if (opt["TetraMaster"])
            {
                // Cards
            }


            // Need to write folders and data inside
            pathSeed = @dir + sbfolder;
            DirectoryInfo seedDirBase = Directory.CreateDirectory(pathSeed);
            DirectoryInfo seedDirSA = Directory.CreateDirectory(pathSeed + "\\StreamingAssets");
            DirectoryInfo seedDirItems = Directory.CreateDirectory(pathSeed + "\\StreamingAssets\\Data\\Items");
            DirectoryInfo seedDirChar = Directory.CreateDirectory(pathSeed + "\\StreamingAssets\\Data\\Characters");
            DirectoryInfo seedDirAbility = Directory.CreateDirectory(pathSeed + "\\StreamingAssets\\Data\\Characters\\Abilities");
            DirectoryInfo seedDirBattle = Directory.CreateDirectory(pathSeed + "\\StreamingAssets\\Data\\Battle");

            // Need to WriteMemoriaINI
            if (File.Exists(@dir + "\\memoria.ini"))
            {
                memoriaINI = File.ReadAllLines(@dir + "\\memoria.ini").ToList();
                for (int i = 0; i < memoriaINI.Count; i++)
                {
                    //need to Identify Mod list
                    if (memoriaINI[i] == "[Mod]")
                    {
                        if (memoriaINI[i + 1].Contains("FolderNames="))
                        {
                            memoriaFolderNamesLine = memoriaINI[i + 1].Substring(12);
                            memoriaINI[i + 1] = "FolderNames=" + "\"" + sbfolder + "\"," + memoriaFolderNamesLine;
                            File.WriteAllLines(@dir + "\\memoria.ini", memoriaINI);
                        }
                    }
                }
            }
            // End Thread
        }
        // Remove from MemoriaINI
        private void RemoveSBfromMemoria()
        {
            // Remove SB from mod load order
            if (File.Exists(@tb_fl.Text + "\\memoria.ini"))
            {
                memoriaINI = File.ReadAllLines(@tb_fl.Text + "\\memoria.ini").ToList();
                for (int i = 0; i < memoriaINI.Count; i++)
                {
                    if (memoriaINI[i] == "[Mod]")
                    {
                        if (memoriaINI[i + 1].Contains("FolderNames="))
                        {
                            memoriaFolderNamesLine = memoriaINI[i + 1].Substring(12);
                            modNames = memoriaFolderNamesLine.Split(',').ToList();
                            for (int j = 0; j < modNames.Count; j++)
                            {
                                if (modNames[j].Contains("Stiltzkins"))
                                {
                                    modNames.RemoveAt(j);
                                }
                            }
                            memoriaFolderNamesLine = modNames.ToString();
                            memoriaINI[i + 1] = "FolderNames=" + "\"" + seedFolderName + "\"," + memoriaFolderNamesLine;
                            File.WriteAllLines(@tb_fl.Text + "\\memoria.ini", memoriaINI);
                            MessageBox.Show("Removed Stitzkin's Bag from Mod Load Order", "Randomized disabled", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);

                        }
                    }
                }
            }
        }
        // Add to MemoriaINI
        private bool WriteMemoria()
        {
            if (File.Exists(@tb_fl.Text + "\\memoria.ini"))
            {
                memoriaINI = File.ReadAllLines(@tb_fl.Text + "\\memoria.ini").ToList();
                for (int i = 0; i < memoriaINI.Count; i++)
                {
                    //need to Identify Mod list
                    if (memoriaINI[i] == "[Mod]")
                    {
                        if (memoriaINI[i + 1].Contains("FolderNames="))
                        {
                            memoriaFolderNamesLine = memoriaINI[i + 1].Substring(12);
                            memoriaINI[i + 1] = "FolderNames=" + "\"" + seedFolderName + "\"," + memoriaFolderNamesLine;
                            File.WriteAllLines(@tb_fl.Text + "\\memoria.ini", memoriaINI);
                            return true;
                        }
                    }
                }
                return false;
            }
            return false;
        }

        // Generate Seed folder data, i.e., list of folders/files to write 


        //  Randomize the files

        //










        // OLD CODE


        //init//
        public static string data_str = "", pswap, binloc, set, seed = "42", tb_flText, textBoxSeed;
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

        List<string> memoriaINI;

        List<string> modNames = new List<string>();
        string memoriaFolderNamesLine;
        List<string> csvNeeded = new List<string>
            {
                "\\StreamingAssets\\Data\\Items\\Items.csv",
                "\\StreamingAssets\\Data\\Items\\ShopItems.csv",
                "\\StreamingAssets\\Data\\Items\\Synthesis.csv",
                "\\StreamingAssets\\Data\\Characters\\BaseStats.csv",
                "\\StreamingAssets\\Data\\Characters\\DefaultEquipment.csv",
                "\\StreamingAssets\\Data\\Characters\\AbilityGems.csv",
                "\\StreamingAssets\\p0data2.bin",
                "\\StreamingAssets\\p0data7.bin"
            };

        
        
        //      MAIN Program Execution        //
        //      MAIN Program Execution        //

        

        public void ExecuteProgram()
        {
            //Gaurentee Seed
            if (textBox_seed.Text.Length == 0)
            {
                seed = "42";
                textBox_seed.Text = seed;
            }
            seed = textBox_seed.Text;

            // Run Seed Code if needed, Reuse seed if valid
            if (Seed.TestSeed() == false)
            {
                Seed.SeedIngest();
                Seed.Compressor();
                Seed.CleanSeedInt();
            }
            textBox_seed.Text = data_int.ToString();

            //Create new mod folder with seed name
            seedFolderName = "StiltzkinsBag2.0-Seed-" + data_int.ToString();
            pathSeed = @tb_fl.Text + seedFolderName;
            DirectoryInfo di = Directory.CreateDirectory(pathSeed);
            DirectoryInfo di2 = Directory.CreateDirectory(pathSeed + "\\StreamingAssets\\Data\\Items");
            DirectoryInfo di3 = Directory.CreateDirectory(pathSeed + "\\StreamingAssets\\Data\\Characters");
            DirectoryInfo di4 = Directory.CreateDirectory(pathSeed + "\\StreamingAssets\\Data\\Characters\\Abilities");
            DirectoryInfo di5 = Directory.CreateDirectory(pathSeed + "\\StreamingAssets\\Data\\Battle");

            // Seed done
            // Path done

            // need to read memoria.ini for mods folders to pull CSV/BIN data from
            if (ReadMemoria())
            {
                modNames = CleanModNames(modNames);
                if (CopySourceData(modNames))
                {
                    //Source data copied, ready to do more work
                    // We now have all CSV and BIN Files in our seed folder
                    // So our new Seed folder is prepped with CSV/BIN files to use in randomizatiion.
                }
            }

            // BIN file extraction next

            // We need to write a function to scan and 

            // After writting mods, we should write a Seed_Source.txt and in it, report which mod/stock we are pulling our data from.

            // create txt file with randomized details

            // Finally, we add the seed folder name to the START of the list of MemoriaMods, and then write that line back into the memoria.ini file.
            // once write this method, add to stock button

            // Having our mod first ensures, any *.bytes files and BINs are used in place of other mods.
            // This should in theory, make our mod compatble with all other mods. Alternate Fantasy etc.

            //test
            //modNames.Insert(0, seedFolderName);
            //write memoria file

            //  END MAIN CODE   //
            //  END MAIN CODE   //
            //  END MAIN CODE   //
            //  END MAIN CODE   //

            // Refactor layout of checkbox options.

        }

        
        bool ReadMemoria()
        {
            if (File.Exists(@tb_fl.Text + "\\memoria.ini"))
            {
                memoriaINI = File.ReadAllLines(@tb_fl.Text + "\\memoria.ini").ToList();
                for (int i = 0; i < memoriaINI.Count; i++)
                {
                    //need to Identify Mod list
                    if (memoriaINI[i] == "[Mod]")
                    {
                        if (memoriaINI[i + 1].Contains("FolderNames="))
                        {
                            memoriaFolderNamesLine = memoriaINI[i + 1].Substring(12);
                            modNames = memoriaFolderNamesLine.Split(',').ToList();
                            return true;
                        }
                    }
                }
                return false;
            }
            else
            {
                // No Memoria found
                MessageBox.Show("Memoria Engine Required. Moguri and other mods use Memoria engine. If this is a stock FFIX install, you need to at least install Memoria Engine", "Memoria Engine Required", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return false;
            }
        }

        List<string> CleanModNames(List<string> modNamesCleaned)
        {
            // modNames.Add(seedFolderName); test only
            for (int i = 0; i < modNamesCleaned.Count; i++)
            {
                if (modNamesCleaned[i].Contains("Stiltzkins"))
                {
                    modNamesCleaned.RemoveAt(i);
                }
            }
            return modNamesCleaned;
        }

        bool CopySourceData(List<string> modNamesCopy)
        {
            modNamesCopy.Add(""); //Required, stock StreamingAssests dir
                                  // Search all mod folders for all needed CSV/BIN files and create copy to Seed folder.
            for (int i = 0; i < modNamesCopy.Count; i++)
            {
                List<int> csvIndex = new List<int>();
                for (int j = 0; j < csvNeeded.Count; j++)
                {
                    string csvModPathCheck = @tb_fl.Text + "\\" + modNamesCopy[i] + @csvNeeded[j];
                    string csvSeedPathCheck = pathSeed + csvNeeded[j];
                    if (File.Exists(@csvModPathCheck))
                    {
                        File.Copy(@csvModPathCheck, @csvSeedPathCheck, true);
                        csvIndex.Add(j);
                    }
                }
                if (csvIndex.Count > 0)
                {
                    int counter = 0;
                    for (int j = 0; j < csvIndex.Count; j++)
                    {
                        csvNeeded.RemoveAt(csvIndex[j - counter]);
                        counter++;
                    }
                }
            }
            if (csvNeeded.Count == 0)
            {
                return true;
            }
            return false;
        }

        

        //              MISC FUNCS              //
        //              MISC FUNCS              //

        // Refactor new method
        public void ReadWrite()
        {
            // needs refactor
            button_rand.Text = "Writing Data";
            var t = Task.Run(async delegate
            {
                await Task.Delay(600);
            });
            t.Wait();

            // Refactor Writing data. use Async
            // use seed folder created before this.  @tb_fl.Text \\ seedfolder \\ CSV locations
        }

        // Misc UI elements
        private void debugButton_Click(object sender, EventArgs e)
        {
            //Run Debug Form
            DebugForm DebugForm2 = new DebugForm();
            this.Hide();
            DebugForm2.ShowDialog();
            this.Show();
        }
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
            if (MessageBox.Show("This will remove Stilzkin's Bag from the Mod Load Order, Seed folders will remain", "Remove Stiltzkin's Bag", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
            {
                RemoveSBfromMemoria();
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
        private void tb_fl_TextChanged(object sender, EventArgs e)
        {
            tb_flText = @tb_fl.Text;
        }
        private void textBox_seed_TextChanged(object sender, EventArgs e)
        {
            textBoxSeed = textBox_seed.Text;
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
    }
}