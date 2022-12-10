using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rand9er
{
    public partial class DebugForm : Form
    {
        public DebugForm()
        {
            InitializeComponent();
        }

        //We have two folders containing folders and files. We need to scan each, and match them up.
        //I think we can use the same function to scan into a storage object. Like a array of byte arrays.
        // should be a staggered array, varibale file size,
        //  scan through both at the same time, compareing bytes.
        // log any mismatch
        // output mismatch percent and value.

        // using a custom class list of another custom class data
        //EnemyList enemy1List = new EnemyList();
        //EnemyList enemy2List = new EnemyList();
        List<EnemiesData> enemy1List = new List<EnemiesData>();
        List<EnemiesData> enemy2List = new List<EnemiesData>();

        // Default Offsets for drops,steals,bmagic,card
        //int[] allvalueLocations = new int[] { 84, 85, 86, 87, 88, 89, 90, 91, 135, 169 };

        // Enemy Offsets - 1 enemy in list, 1 group
        int[] dropOffsets = new int[] { 84, 85, 86, 87 };
        int[] stealOffsets = new int[] { 88, 89, 90, 91 };
        int[] blueMagicOffset = new int[] { 135 };
        int[] cardDropOffset = new int[] { 169 };

        int enemyListOffset = 1;
        int enemyGroupOffset = 2;
        int enemyListSize = 116;
        int enemyGroupSize = 56;

        List<bool> bools = new List<bool>();

        void DebugFunc(){

            // Goal here is to compare two sets of files and ensure they are the same, 
            // We wont need this in the future, this was just to ensure we were on the right track
            // Also need to update func to work for other types of files later


            List<string> files1 = new List<string>();
            List<string> files2 = new List<string>();
            List<EnemiesData> enemies1 = new List<EnemiesData>();
            List<EnemiesData> enemies2 = new List<EnemiesData>();
            //gather 2 lists of all files in the folders as well as the paths.
            (files1, files2) = FolderScan();
            bools.Add(files1.Count > 0 && files2.Count > 0);
            // now we have a list of files. now to import those files into the storage object

            // now we have two lists of files.  -- This has been verified,
            

            // now to load all the data from them into storage objects
            (enemies1, enemies2) = FilesScan(files1, files2);
            bools.Add(enemies1.Count > 0 && enemies2.Count > 0);

            // enemies1 and enemies2 are the same. Only need to use one from now on


            // now we have a list of all binary data, we need to scan each
            // for each check offset for value

            // take that value and and for every size value, add that address to a list
            // build list of values we need to edit for each section
            // using this list, randomzie values and write them out.


            // now each enemyData in the list has the value in 01 and 02 for each enemylist and groups.
            
            // when randomizing we iterate though these two values to know the proper offets and amounts for each file

            





            //scrapping these two funcs
            //(enemies2) = TrimZeros(enemies2);
            // check trimjob
            //bools.Add(TrimCheck(enemies1, enemies2));




            // so all passed, we now have truncated files. We need to test write them back to files and then load the game to test

            // What we will do here is just write all 01 to all the slots. that will be dagger,
            // if we can steal a dagger from a few battles, we know our method is sound

            
            TestRand(enemies2);
            //check again
            //bools.Add(TrimCheck(enemies1, enemies2));


            // last

            if (!bools.Contains(false))
            {
                eventText.Text = "FINISH true";
                //out bytes array back to files
                OutputFiles(enemies2);
                eventText.Text = "FINISH written";
            }
            else
            {
                eventText.Text = "FINISH fail";
            }


            // once ready, write funcs to store list as object for refference in future runs


        }

        //  MISC FUNCS
        string ManualSearch(string dir)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.Description = "Locate " + dir;
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                return folderDlg.SelectedPath;
            }
            else
            {
                return "";
            }
        }
        (List<string>, List<string>) FolderScan()
        {
            //generaete 2 list of files from two directories
            List<string> dirFiles = new List<string>();
            List<string> dir1Files = new List<string>();
            List<string> dir2Files = new List<string>();
            DirSearch_ex3(@dir1Text.Text);
            dir1Files = dirFiles;
            dirFiles = new List<string>();
            DirSearch_ex3(@dir2Text.Text);
            dir2Files = dirFiles;
            dirFiles = new List<string>();
            void DirSearch_ex3(string sDir)
            {
                try
                {
                    foreach (string f in Directory.GetFiles(sDir))
                    {
                        dirFiles.Add(f);
                        //outputText.Text += "\n" + f;
                    }

                    foreach (string d in Directory.GetDirectories(sDir))
                    {
                        DirSearch_ex3(d);
                    }
                }
                catch (System.Exception excpt)
                {
                    Console.WriteLine(excpt.Message);
                }
            }
            return (dir1Files, dir2Files);
        }
        (List<EnemiesData>, List<EnemiesData>) FilesScan(List<string> files1, List<string> files2)
        {
            List<EnemiesData> enemies = new List<EnemiesData>();
            List<EnemiesData> enemies1 = new List<EnemiesData>();
            List<EnemiesData> enemies2 = new List<EnemiesData>();
            enemies = new List<EnemiesData>();
            FilesScan(files1);
            enemies1 = enemies;
            enemies = new List<EnemiesData>();
            FilesScan(files2);
            enemies2 = enemies;
            enemies = new List<EnemiesData>();
            // now we have our data files.
            // compare and scan all files now
            if (enemies1.Count == enemies2.Count)
            {
                for (int i = 0; i < enemies1.Count; i++)
                {
                    for (int j = 0; j < enemies1[i].EnemyBytes.Length; j++)
                    {
                        bool f = enemies1[i].EnemyBytes[j] == enemies1[i].EnemyBytes[j];
                        if (!f)
                        {
                            outputText.Text += "\nebyte Failed " + enemies1[i].EnemyBytes[j] + " " + enemies2[i].EnemyBytes[j];
                        }
                        else
                        {
                            //outputText.Text += "\nebyte Match " + enemies1[i].EnemyBytes[j] + " " + enemies2[i].EnemyBytes[j];
                        }
                    }
                    
                }
            }
            return (enemies1, enemies2);
            void FilesScan(List<String> files)
            {
                //List<EnemiesData> enemies = new List<EnemiesData>();
                for (int i = 0; i < files.Count; i++)
                {
                    //outputText.Text += "\nEnemy Group Amount= " + files[i][1] + " Enemy List Amount+= " + files[i][2];
                    enemies.Add(new EnemiesData(@files[i], File.ReadAllBytes(files[i])));
                }
            }
        }
        List<EnemiesData> TrimZeros(List<EnemiesData> enemies2)
        {
            //scrapping this func
            for (int i = 0; i < enemies2.Count; i++)
            {
                
                int counter = 0;
                for (int j = enemies2[i].EnemyBytes.Length - 1; j > -1; j--)
                {
                    if (enemies2[i].EnemyBytes[j] == 0)
                    {
                        counter++;
                    }
                    else
                    {
                        break;
                    }
                }
                int newSize = enemies2[i].EnemyBytes.Length - counter;
                byte[] buffer = new byte[newSize];
                for (int j = 0; j < newSize; j++)
                {
                    buffer[j] = enemies2[i].EnemyBytes[j];
                }
                enemies2[i].EnemyBytes = buffer;
            }
            return enemies2;
        }
        bool TrimCheck(List<EnemiesData> enemies1, List<EnemiesData> enemies2)
        {
            //scrapping this func
            for (int i = 0; i < enemies2.Count; i++)
            {
                //outputText.Text += "\nNewByte[]: " + enemies2[i];
                for (int j = 0; j < enemies2[i].EnemyBytes.Length; j++)
                {
                    if (enemies2[i].EnemyBytes[j] != enemies1[i].EnemyBytes[j])
                    {
                        outputText.Text += "\nFail" + enemies2[i].EnemyBytes.Length + ": " + enemies2[i].EnemyBytes[j] + " " + enemies1[i].EnemyBytes[j];
                        //return false;
                    }
                    else
                    {
                        //outputText.Text += "\nMatch" + enemies2[i].EnemyBytes.Length + ": " + enemies2[i].EnemyBytes[j] + " " + enemies1[i].EnemyBytes[j];
                    }
                }
            }
                return true;
        }

        void TestRand(List<EnemiesData> enemies2)
        {
            //drop  84, 85, 86, 87
            //steal 88, 89, 90, 91
            //blu   135
            //card  169
            byte testValue = 1;

            int[] valueLocations = new int[] { 84, 85, 86, 87, 88, 89, 90, 91, 135, 169 };
            int[] valueLocations0group = new int[] { 28, 29, 30, 31, 32, 33, 34, 35, 79, 113 }; // this is offsets for 0 enemy groups, includes 

            //  Scan all enemies
            for (int i = 0; i < enemies2.Count; i++)
            {
                // Scan each enemy in the enemy list
                for (int j = 0; j < enemies2[i].EnemyListAmount; j++) // starting at 0 as to not add 116 for a single enemy.
                {

                    //  Group pffset is the current enemy group value multiplied by 56

                    int groupOffset = enemies2[i].EnemyGroupAmount * enemyGroupSize;

                    //  List offset is the current enemys 
                    int listOffset = j * enemyListSize;

                    //  Scan each value in the test values location array
                    for (int k = 0; k < valueLocations0group.Length; k++)
                    {
                        int vl0gk = valueLocations0group[k];
                        int writeByte = vl0gk + groupOffset + listOffset;
                        //int writeByte = valueLocations0group[k] + groupOffset + listOffset;
                        enemies2[i].EnemyBytes[writeByte] = testValue;
                        outputText.Text += "\n writeByte=" + writeByte + " Ega=" + enemies2[i].EnemyGroupAmount + " Ela= " + enemies2[i].EnemyListAmount;
                    }
                }
                // this should leave us with an enemies2 files with all bytes written that we need.

                // scan each enemy byte array
                //
                // group amount is offset for all other values
                // enemyList amount needs to be written for each value
                //

                /*
                // old partially failed test, change each enemy group offset
                for (int j = 0; j < enemies2[i].EnemyGroupAmount; j++)
                {
                    enemies2[i].EnemyBytes[valueLocations[j]] = testValue;
                }
                */

            }
        }
        void OutputFiles(List<EnemiesData> enemies2)
        {
            for (int i = 0; i < enemies2.Count; i++)
            {
                SaveByteArrayToFileWithBinaryWriter(enemies2[i].EnemyBytes, @enemies2[i].EnemyFolder);
            }
        }
        void SaveByteArrayToFileWithBinaryWriter(byte[] data, string filePath)
        {
            var writer = new BinaryWriter(File.OpenWrite(filePath));
            writer.Write(data);
        }
        private void runButton_Click(object sender, EventArgs e)
        {
            if (dir1Text.Text.Length > 0 && dir2Text.Text.Length > 0)
            {
                DebugFunc();
            }
            else
            {
                eventText.Text = "Need Dirs";
            }
        }

        private void dir1Button_Click(object sender, EventArgs e)
        {
            //locate dir 1
            dir1Text.Text = ManualSearch("First Base Directory");
        }

        private void dir2Button_Click(object sender, EventArgs e)
        {
            //locate dir 2
            dir2Text.Text = ManualSearch("Second Base Directory");
        }

        private void outputText_TextChanged(object sender, EventArgs e)
        {
            outputText.SelectionStart = outputText.Text.Length;
            outputText.ScrollToCaret();
        }
    }
}
