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
using System.Reflection;
//using System.Security.Cryptography;
//using System.Runtime.Serialization.Formatters.Binary;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;
using System.CodeDom;
using System.Data.SqlClient;

namespace rand9er
{
   
    public partial class rand9er : Form
    {
        public rand9er()
        {
            InitializeComponent();
        }
        private Thread trd;
        public string seedFolderName = "";
        public static int data_int = 0;
        public static string seed = "42";
        private void character_CheckedChanged(object sender, EventArgs e)
        {
            charBaseStats.Enabled = character.Checked;
            charBaseStats.Checked = character.Checked;
            charSpeciality.Enabled = character.Checked;
            charSpeciality.Checked = character.Checked;
            charAbilities.Enabled = character.Checked;
            charEquipment.Enabled = character.Checked;
        }
        private void item_CheckedChanged_1(object sender, EventArgs e)
        {
            itemsTreasure.Enabled = item.Checked;
            itemsShops.Enabled = item.Checked;
            itemsSynth.Enabled = item.Checked;
            itemsBonusSets.Enabled = item.Checked;
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
                if (MessageBox.Show("Randomize Selected Options", "Create new Seed Folder", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
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
                    string seedFolderName = "StiltzkinsBag-Seed-" + data_int.ToString();

                    Dictionary<string, bool> options = new Dictionary<string, bool>();
                    options.Add("Version", checkBoxRecommended.Checked);
                    options.Add("Char", character.Checked);
                    options.Add("BaseStats", charBaseStats.Checked);
                    options.Add("Speciality", charSpeciality.Checked);


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

                // Perhaps we build a character object and store data we read into it.
                List<Character> Chars = new List<Character>();


                // Char 1.1 Read base stats into file. Need to always read this for other use

                        // Base Stats
                        // Base Stats
                        // Base Stats
                string baseStats = Properties.Resources.BaseStats;
                string statsCSVrand = "";
                var csv = new StringBuilder();
                using (StringReader sr = new StringReader(baseStats))
                {
                    string line;
                    
                    while ((line = sr.ReadLine()) != null)
                    {
                        if ((line.StartsWith("#")))
                        {
                            csv.AppendLine(line);
                        }
                        if ((!line.StartsWith("#")) && line.Length > 1)
                        {
                            var n = line.Split(';')[0];
                            byte index = Convert.ToByte(line.Split(';')[1]);
                            line = line.Substring(line.IndexOf(';') + 1); // Drop Name
                            line = line.Substring(line.IndexOf(';') + 1); // Drop Index
                            string[] stri = line.Split(';'); // String array of rest
                            byte[] bytes = stri.Select(byte.Parse).ToArray();
                            Chars.Add(new Character(n, index, bytes));
                        }
                    }
                }
                        // Now we should have a list of Characters and their Base stats
                        /*foreach (Character c in Chars)
                        {
                            Console.WriteLine(c.Name + ";" + c.BaseStatsIndex + ";" + c.BaseStats[0] + "," + c.BaseStats[1] + "," + c.BaseStats[2] + "," + c.BaseStats[3] + "," + c.BaseStats[4] + " , length" + c.BaseStats.Length);
                        }*/
                        // yes we do, I confirmed it.

                // generate stat arrays from character list
                int[][] statsArr = new int[5][];
                for (int statNum = 0; statNum < 5; statNum++)
                {
                    int[] statArr = new int[12];
                    for (int j = 0; j < Chars.Count; j++)
                    {
                        statArr[j] = Chars[j].BaseStats[statNum];
                    }
                    statsArr[statNum] = statArr;
                }


                // Char 1.1 Read base stats into file.


                // Char 1.2 if Base stats randomizer is checked, randomize base stats

                if (opt["BaseStats"])
                {
                    
                    // randomize all 5 stat types
                    int iteration = 1;
                    int[][] statsArrRandomized = new int[5][];
                    for (int i = 0; i < statsArr.Length; i++)
                    {
                        int[] stat = statsArr[i];
                        int[] statWork = new int[12];
                        int statMax = stat.Max() + 1; int statMin = stat.Min(); int statAvgW = 0;
                        int statAvg = stat.Sum() / stat.Length;
                        int offset = new Random(data_int + (iteration * (i + 3))).Next(1, 5);
                        Console.WriteLine("\nOffset: " + offset + " Staring iteration: " + iteration + " Average: " + statAvg);
                        while (statAvgW < (statAvg - 1) || statAvgW > (statAvg + 1))
                        {
                            for (int j = 0; j < statWork.Length; j++)
                            {
                                // these values look good, I think we can keep this version
                                statWork[j] = new Random(data_int + (iteration++ * (j + 3))).Next(statMin - offset, statMax + offset);
                                if (j > 0)
                                {
                                    while (statWork[j] == statWork[j - 1])
                                    {
                                        statWork[j] = new Random(data_int + (iteration++ * (j + 3))).Next(statMin - offset, statMax + offset);
                                    }
                                }
                            }
                            statAvgW = statWork.Sum() / statWork.Length;
                        }
                        Console.WriteLine("\n Final iteration: " + iteration + " Average achieved : " + statAvgW);
                        foreach (int s in statWork)
                        {
                            Console.Write(s + " ");
                        }
                        statsArrRandomized[i] = statWork;
                    }

                    // this worked, we have randomzied stats

                    // show stock arrays
                    foreach (int[] a in statsArr)
                    {
                        Console.WriteLine("");
                        foreach (int s in a)
                        {
                            Console.Write(s + " ");
                        }
                    }

                    Console.WriteLine("");
                    // show randomized arrays
                    statsArr = statsArrRandomized;
                    foreach (int[] a in statsArr)
                    {
                        Console.WriteLine("");
                        foreach (int s in a)
                        {
                            Console.Write(s + " ");
                        }
                    }
                    Console.WriteLine("");


                    //                  statsArrRandomized[][] has the randomized base stats data in it



                            //formed CSV seems to be broken when writtin to file.
                            /*
                            foreach (Character c in Chars)
                            {
                                string arr = "";
                                for (int i = 0; i < 5; i++)
                                {
                                    arr += ";" + statsArrRandomized[i][c.BaseStatsIndex];
                                }
                                statsCSVrand += c.Name + ";" + c.BaseStatsIndex + arr;
                                statsCSVrand += Environment.NewLine;
                            }
                            Console.Write(statsCSVrand);
                            */

                    // Base stats End       
                    // Base stats End       int[][] statsArrRandomized contains the randomzied stats
                    // Base stats End       string statsCSVrand contains the export ready data

                    // output random stats to seed folder.

                    foreach (Character c in Chars)
                    {
                        string arr = "";
                        for (int i = 0; i < 5; i++)
                        {
                            arr += ";" + statsArrRandomized[i][c.BaseStatsIndex];
                        }
                        statsCSVrand = c.Name + ";" + c.BaseStatsIndex + arr;
                        csv.AppendLine(statsCSVrand);

                    }

                    // Need to move this to after niitial folder creation
                    string dirbs = @dir + @sbfolder + "\\StreamingAssets\\Data\\Characters\\BaseStats.csv";
                    Console.WriteLine(@dirbs);
                    //File.WriteAllText(@dirbs, csv.ToString());
                    Console.Write(csv);

                }

                // Char 1.2 if Base stats randomizer is checked, randomize base stats


                // Char 1.3 if Specifiality is checked, generate new speciality

                if (opt["Speciality"])
                {
                    // Command Sets
                    // Command Sets
                    // Command Sets


                    string commandSets = Properties.Resources.CommandSets;
                    // lines 7-14 need edit.

                    /*
                     * There are 8 characters, and 15 options. 1 option takes a full char, 14 for 7 remaining mix/match
                     * 
                     Theif
                         * Steal(2)         = highest dex

                     Barbarian
                         * Skill(25)        = highest str
                         * Dyne(26)         = highest str

                    Wizard1
                         * Focus(12)        = high magic 3rd

                    Wizard2
                         * BlkMag(22)       = lowest str, med mag
                         * DblMag(23)       = lowest str, med mag

                    Cleric
                         * WhtMag(17)       = high magic

                    KonDar
                         * Summon(16)       = highest mag, 2nd lowest str
                         * Eidolon(18)      = highest mag, 2nd lowest str

                    Fighter
                         * SwdArt(30)       = high magic, 3rd highest will

                    Warlock1
                         * SwdMag(31)       = mag and physical attacho

                    Druid
                         * Dragon(27)       = 2nd highest will, high mag

                    Rogue1
                         * Jump(3)          = 3rd highest str, 2nd highest dex
                         * Jump(12)         = 3rd highest str

                    BaChingChing
                         * BluMag(24)       = lowest will, med mag
                         * Eat(8)           = lowest will, med mag
                         * Cook(9)          = lowest will, med mag

                    Summoner
                         * Summon(20)       = 2nd highest med mag (eiko summons)

                    WhiteMage
                         * WhtMag(19)       = med mag
                         * DblWht(21)       = med mag

                    Rogue2
                         * Throw(15)        = 2nd lowest mag

                    Warlock2
                         * Flair(28)        = med str/dex/will
                         * Elan(29)         = med str/dex/will
                     * 
                     * */

                    // Go through each requirement and assign the class to a character based on who has the highest x value
                    int[][] commandSet = new int[8][];
                    // 8 by 4 array to store all commands
                    int[] dex = new int[8];
                    int[] str = new int[8];
                    int[] mag = new int[8];
                    int[] will = new int[8];
                    int[] gems = new int[8];

                    for (int j = 0; j < 8; j++)
                    {
                        dex[j] = statsArr[0][j];
                        str[j] = statsArr[1][j];
                        mag[j] = statsArr[2][j];
                        will[j] = statsArr[3][j];
                        gems[j] = statsArr[4][j];
                    }

                    int[] dexOrder = new int[8];
                    int[] strOrder = new int[8];
                    int[] magOrder = new int[8];
                    int[] willOrder = new int[8];
                    int[] gemsOrder = new int[8];

                    for (int i = 0; i < 8; i++)
                    {
                        dexOrder[i] = dex.ToList().IndexOf(dex.Max());
                        strOrder[i] = str.ToList().IndexOf(str.Max());
                        magOrder[i] = mag.ToList().IndexOf(mag.Max());
                        willOrder[i] = will.ToList().IndexOf(will.Max());
                        gemsOrder[i] = gems.ToList().IndexOf(gems.Max());
                        dex[dexOrder[i]] = 0;
                        str[strOrder[i]] = 0;
                        mag[magOrder[i]] = 0;
                        will[willOrder[i]] = 0;
                        gems[gemsOrder[i]] = 0;
                    }


                    for (int i = 0; i < 8; i++)
                    {
                        Console.WriteLine(i + " DexIndex: " + dexOrder[i] + " strIndex: " + strOrder[i] + " magIndex: " + magOrder[i] + " willIndex: " + willOrder[i] + " gemsIndex: " + gemsOrder[i]);
                    }

                    // This works, now we have an order to all stats, knowing who is best, 2nd best, etc. all the way to worst.

                    // list of slots and usability.
                    int[][] charSlots = new int[8][];
                    int[][] charSlots2 = new int[8][];
                    List<int> charUsed = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 7 };

                    // BaChingChing
                    charSlots[willOrder[7]] = new int[] { 8, 9 };
                    charUsed.RemoveAt(charUsed.IndexOf(willOrder[7]));
                    charSlots2[willOrder[7]] = new int[] { 24, 24 };
                    charUsed.RemoveAt(charUsed.IndexOf(willOrder[7]));



                    // BaChingChing = willOrder[7];
                    // This user cant be used again. So next settings will restrict use of this.

                    if (charUsed.Contains(dexOrder[0]) == true)
                    {
                        if (charUsed.Count(n => n == dexOrder[0]) == 2)
                        {

                        }
                        else if (charUsed.Count(n => n == dexOrder[0]) == 1)
                        {

                        }
                        charSlots[willOrder[7]] = new int[] { 8, 24, 9, 24 };
                    }
                    // Thief = dexOrder[0]; this value can be used again
                    // Barbaian = strOrder[0]; This value can be used again




                    for (int i = 0; i < 8; i++)
                    {
                        int[] set = new int[4];

                        commandSet[i] = set;
                    }





                    // Need to analyze this part, I think we are ording the lists of each stat type and taking the largest of some to do work with
                    // I think we are assigning classes based on highest magic, str etc. There are certain groups that need to be 
                    // Decided how to assing them.
                    // maybe a list of objects that has a char index and their stats, highest stat returns the char index.
                    // we run through each of the classes and once the char list is empty, we have assigned all the classes.
                    // Then we recreate the commandsets.csv with this new data.
                    // refference basestats csv.








                    // Commands End
                    // Commands End
                    // Commands End
                }

                // Char 1.3 if Specifiality is checked, generate new speciality









                // Abilities
                // Once we have the specialities decided, we can now determine which abilities the char needs to learn in order to fully maximize that class
                // We can shuffle a lot of the good stats, since most of those aren't affected by class, but cure/life is. rememebr those.
                // So we form a list of which magic/dyne/blu abilities need to go with which class.
                // Then we write out the CSV for each char of their stats using their new class.

                // Items
                // Now we can shuffle weapons and armor, and items, and assign the correct permission and stat based on their class.
                // We can have random weapon types per char, or randomize all weapons per char. That would be a bit cooler I suppose. like go from atk damage to a rod to use blk mag.
                // hmm, maybe, maybe not. So we pick which class can access which items, and then fill in the abilities for that class.


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





            // End of the program, everything is done. Write out files and then write the INI to use the mod folder.


            // Need to write folders and data inside
            pathSeed = @dir + sbfolder;
            DirectoryInfo seedDirBase = Directory.CreateDirectory(pathSeed);
            DirectoryInfo seedDirSA = Directory.CreateDirectory(pathSeed + "\\StreamingAssets");
            DirectoryInfo seedDirItems = Directory.CreateDirectory(pathSeed + "\\StreamingAssets\\Data\\Items");
            DirectoryInfo seedDirChar = Directory.CreateDirectory(pathSeed + "\\StreamingAssets\\Data\\Characters");
            DirectoryInfo seedDirAbility = Directory.CreateDirectory(pathSeed + "\\StreamingAssets\\Data\\Characters\\Abilities");
            DirectoryInfo seedDirBattle = Directory.CreateDirectory(pathSeed + "\\StreamingAssets\\Data\\Battle");





            // Write out files to these folders.





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
        public static string data_str = "", pswap, binloc, set, tb_flText, textBoxSeed;
        public static int counter, randl = 23, items = 236;
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

        
        // OLD program
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