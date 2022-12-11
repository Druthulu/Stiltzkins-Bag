using System;
using System.Collections.Generic;
//using System.Linq;
using System.IO;

//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
//using static System.Net.WebRequestMethods;

namespace rand9er
{
    public partial class DebugForm : Form
    {
        public DebugForm()
        {
            InitializeComponent();
        }
        
        // Enemy Files VARs
        List<EnemiesData> enemy1List = new List<EnemiesData>();
        List<EnemiesData> enemy2List = new List<EnemiesData>();
            // Enemy Offsets - 1 enemy in list, 1 group
        int[] dropOffsets = new int[] { 84, 85, 86, 87 };
        int[] stealOffsets = new int[] { 88, 89, 90, 91 };
        int[] blueMagicOffset = new int[] { 135 };
        int[] cardDropOffset = new int[] { 169 };
        int enemyListOffset = 1;
        int enemyGroupOffset = 2;
        int enemyListSize = 116;
        int enemyGroupSize = 56;
        List<string> files1 = new List<string>();
        List<string> files2 = new List<string>();
        List<EnemiesData> enemies1 = new List<EnemiesData>();
        List<EnemiesData> enemies2 = new List<EnemiesData>();
        List<EnemiesData> enemies3 = new List<EnemiesData>();

        // Enemy BINs VARs

        // Field Files VARs

        // Field BINs VARs




        void DebugFunc()
        {
            List<bool> bools = new List<bool>();

            //gather 2 lists of all files in the folders as well as the paths.
            (files1, files2) = FolderScan();
            bools.Add(files1.Count > 0 && files2.Count > 0);
            // now we have two lists of files.  -- This has been verified,
            FilesScan(files1, files2);


            // p0data7 Compare Files
            if (p0data2Files.Checked)
            {

                bools.Add(enemies1.Count > 0 && enemies2.Count > 0);
                // enemies1 and enemies2 are the same. Only need to use one from now on


                // REDO

                enemies3 = EnemyFilesTrimZeros(enemies1);
                
                bools.Add(enemies3.Count > 0);
                outputText.Text += "\n enemies3 count= " + enemies3.Count;
                outputText.Text += "\n enemies3 dir= " + @enemies3[0].EnemyFolder + "\n";
                for (int j = 0; j < enemies3[0].EnemyBytes.Length; j++)
                {
                    //outputText.Text += "" + enemies3[0].EnemyBytes[j] + ", ";
                }
                // check trimjob
                //bools.Add(TrimCheck(enemies1, enemies2));

                //JSON STORAGE
                bools.Add(EnemyJsonStoreFiles(enemies3));

                //EnemyFilesTestRand(enemies3); // This worked, verified test.

                //out bytes array back to folder\files\bytes
                //  WILL NEED THIS FUNCTION FOR SEED FOLDER OUTPUT
                //OutputFilesEnemiesBytesFiles(enemies3);

            }


            // p0data7 Compare Files vs BIN
            if (p0data2BIN.Checked)
            {

            }
            // p0data7 Compare Files
            if (p0data7Files.Checked)
            {

            }
            // p0data7 Compare Files vs BIN
            if (p0data7BIN.Checked)
            {

            }



            //  LAST
            if (!bools.Contains(false) && bools.Contains(true))
            {
                eventText.Text = "FINISH true";
                
                eventText.Text = "FINISH written";
            }
            else if(bools.Contains(false))
            {
                eventText.Text = "FINISH fail";
            }
            else if (!bools.Contains(true) && !bools.Contains(false))
            {
                eventText.Text = "FINISH No Bools added";
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
        void FilesScan(List<string> files1, List<string> files2)
        {
            if (p0data2Files.Checked)
            {
                enemies1 = EnemyFilesScan(files1);
                enemies2 = EnemyFilesScan(files2);
                // compare and scan all files now
                // EnemyCountCompare(); - not needed anymore, mog and stock enemies are identical
            }

            if (p0data2BIN.Checked)
            {
                enemies1 = EnemyFilesScan(files1);
            }
            if (p0data7Files.Checked)
            {

            }
            if (p0data7BIN.Checked)
            {

            }

            // Enemy Files Funcs
            List<EnemiesData> EnemyFilesScan(List<String> files)
            {
                List<EnemiesData> enemiesF = new List<EnemiesData>();

                for (int i = 0; i < files.Count; i++)
                {
                    //string subPath = @files[i];
                    //remove first part of path

                    //  JSON MODE
                    int indexDir = @files[i].IndexOf("StreamingAssets");
                    string subPath = @files[i].Substring(indexDir);
                    enemiesF.Add(new EnemiesData("\\" + @subPath, File.ReadAllBytes(files[i])));

                    // BYTES FILE MODE
                    //outputText.Text += "\nEnemy Group Amount= " + files[i][1] + " Enemy List Amount+= " + files[i][2];
                    //enemiesF.Add(new EnemiesData(@files[i], File.ReadAllBytes(files[i])));

                }
                return enemiesF;
            }
            
            //  Unused Enemy func
            void EnemyCountCompare()
            {
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
                //FileScanBackupMethod
                //enemies = new List<EnemiesData>();
                //enemies = EnemyFilesScan(files1);
                //enemies1 = enemies;
                //enemies = new List<EnemiesData>();
                //enemies = EnemyFilesScan(files2);
                //enemies2 = enemies;
                //enemies = new List<EnemiesData>();
            }
            

            //  Enemy BIN funcs





        }
        //  REDO funcs
        byte[] TrimEnd(byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);

            Array.Resize(ref array, lastIndex - 3);

            return array;
        }
        List<EnemiesData> EnemyFilesTrimZeros(List<EnemiesData> enemiesT)
        {
            List<EnemiesData> enemiesTZ = new List<EnemiesData>();
            for (int i = 0; i < enemiesT.Count; i++)
            {
                byte[] buffer = enemiesT[i].EnemyBytes;
                //outputText.Text += "\n before trim: " + buffer.Length;

                int lastIndex = Array.FindLastIndex(buffer, b => b != 0);
                Array.Resize(ref buffer, lastIndex - 3); // should be +1, but according to Hades, should be -3. Trusting Hades

                //outputText.Text += "\n after trim: " + buffer.Length;
                enemiesT[i].EnemyBytes = buffer;
                //outputText.Text += "\n after write: " + enemies2[i].EnemyBytes.Length;
                enemiesTZ.Add(new EnemiesData(@enemiesT[i].EnemyFolder, buffer));
                //outputText.Text += "\n after write: " + enemiesT[i].EnemyBytes.Length + "[ ";
                for (int j = 0; j < enemiesTZ[i].EnemyBytes.Length; j++)
                {
                    //outputText.Text += "" + enemiesTZ[i].EnemyBytes[j] + ", ";
                }
            }
            return enemiesTZ;
            //scrapping this func
            /*
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
            */
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

        void EnemyFilesTestRand(List<EnemiesData> enemies2)
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
                        //outputText.Text += "\n writeByte=" + writeByte + " Ega=" + enemies2[i].EnemyGroupAmount + " Ela= " + enemies2[i].EnemyListAmount;
                    }

                    // NEED TO BREAK THIS OUT INTO SERPATE WRITES, drop, steal etc.

                    // NEED TO BREAK THIS OUT INTO SERPATE WRITES, drop, steal etc.

                    // NEED TO BREAK THIS OUT INTO SERPATE WRITES, drop, steal etc.

                    // NEED TO BREAK THIS OUT INTO SERPATE WRITES, drop, steal etc.


                }

                /*
                // old partially failed test, change each enemy group offset
                for (int j = 0; j < enemies2[i].EnemyGroupAmount; j++)
                {
                    enemies2[i].EnemyBytes[valueLocations[j]] = testValue;
                }
                */
            }
        }
        void OutputFilesEnemiesBytesFiles(List<EnemiesData> enemiesW)
        {
            for (int i = 0; i < enemiesW.Count; i++)
            {
                //SaveByteArrayToFileWithBinaryWriter(enemiesW[i].EnemyBytes, @enemiesW[i].EnemyFolder);
                File.WriteAllBytes(@enemiesW[i].EnemyFolder, enemiesW[i].EnemyBytes);
            }
        }
        void SaveByteArrayToFileWithBinaryWriter(byte[] data, string filePath)
        {
            var writer = new BinaryWriter(File.OpenWrite(filePath));
            writer.Write(data);
        }

        // Long term storage
        bool EnemyJsonStoreFiles(List<EnemiesData> enemies2)
        {
            string fileName = "StockEnemyBytesJson.json";
            string eJsonString = JsonSerializer.Serialize(enemies2);
            DirectoryInfo di5 = Directory.CreateDirectory(@dir2Text.Text + "\\Json");
            Directory.SetCurrentDirectory(@dir2Text.Text + "\\Json\\");
            File.WriteAllText(fileName, eJsonString);
            Console.WriteLine(File.ReadAllText(fileName));

            return true;

            // Read file and deserialize to usable data.
            //string jsonString = File.ReadAllText(fileName);
            //List<EnemiesData> enenmies3 = JsonSerializer.Deserialize<List<EnemiesData>>(eJsonString);
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

        //  UI
        //  UI
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

        private void p0data2Files_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
