using System;
using System.Collections.Generic;
//using System.Linq;
using System.IO;

//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Schema;
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

        byte[] p0data2BA = new byte[0];



        // Field Files VARs

        // Field BINs VARs




        void DebugFunc()
        {
            List<bool> bools = new List<bool>();
            outputText.Text += "\n Running Debufunc() in ";

            // p0data2 Compare Files
            if (p0data2Files.Checked)
            {
                outputText.Text += " p0data2 Files mode ";
                if (dir1Text.Text.Length > 0)
                {
                    outputText.Text += "...dir1 set ";
                }
                if (dir2Text.Text.Length > 0)
                {
                    outputText.Text += "...dir2 set ";
                }

                // This was to compare enemies byte[] against another set of the same
                // We discovered that Moguri and Stock Enemies byte[] are the exactly the same.

                outputText.Text += "\nScanning Folders..";
                //gather 2 lists of all files in the folders as well as the paths.
                (files1, files2) = FolderScan();

                outputText.Text += "...done. files1 count: " + files1.Count + " files2 count: " + files2.Count;

                bools.Add(files1.Count > 0);
                if (dir2Text.Text.Length > 10)
                {
                    bools.Add(files1.Count > 0 && files2.Count > 0);
                }
                // now we have two lists of files.  -- This has been verified,

                outputText.Text += "\nScanning Files into objects..";

                FilesScan(files1, files2);

                outputText.Text += "...done. enemies1 count: " +enemies1.Count;

                bools.Add(enemies1.Count > 0);

                if (dir2Text.Text.Length > 10)
                {
                    bools.Add(enemies1.Count > 0 && enemies2.Count > 0);
                    // enemies1 and enemies2 are the same. Only need to use one from now on
                }

                outputText.Text += "\nTrimming Zeros..";

                enemies3 = EnemyFilesTrimZeros(enemies1);

                outputText.Text += " ..done.";

                bools.Add(enemies3.Count > 0);
                //outputText.Text += "\n enemies3 count= " + enemies3.Count;
                //outputText.Text += "\n enemies3 dir= " + @enemies3[0].EnemyFolder + "\n";
                //for (int j = 0; j < enemies3[0].EnemyBytes.Length; j++)
                //{
                //outputText.Text += "" + enemies3[0].EnemyBytes[j] + ", ";
                //}

                if (JsonCheckBox.Checked)
                {
                    //JSON STORAGE
                    // outputs to dir1
                    outputText.Text += "\nWriting Json";
                    bools.Add(EnemyJsonStoreFiles(enemies3));
                }
                else
                {
                    //EnemyFilesTestRand(enemies3); // This worked, verified test.

                    //out bytes array back to folder\files\bytes
                    //  WILL NEED THIS FUNCTION FOR SEED FOLDER OUTPUT
                    outputText.Text += "\nWriting byte[] to dir1";
                    OutputFilesEnemiesBytesFiles(enemies3);
                }

            }


            // p0data7 Compare Files vs BIN
            if (p0data2BIN.Checked)
            {
                outputText.Text += "p0data2 BIN mode ";
                if (dir1Text.Text.Length > 0)
                {
                    outputText.Text += "...dir1 set ";
                }
                if (dir2Text.Text.Length > 0)
                {
                    outputText.Text += "...dir2 set ";
                }
                
                // deserlize json from resources

                outputText.Text += "\n Deserializing Json from Resources..";
                string resourceFile = @Properties.Resources.StockEnemyBytesJsonNoZeros;
                var jsonEdatas = JsonSerializer.Deserialize<List<EnemiesData>>(resourceFile);

                outputText.Text += " ..done: " + jsonEdatas.Count + " deserialized.\n";

                // deserialise back to stroage object
                //write out to files once to ensure data integrity
                //done

                /*
                foreach (byte b in jsonEdatas[0].EnemyBytes)
                {
                    outputText.Text += b.ToString("X") + " ";
                }
                */

                //outputText.Text += "\n " + jsonEdatas[0].EnemyBytes;

                // import p0data2 BIN to byte array,
                p0data2BA = File.ReadAllBytes(@dir2Text.Text);


                // Scan enemy list and for each enemy scan byte array
                // only testing on 1
                int countLeft = jsonEdatas.Count;
                int countMistakes = 0;

                //absolute vars
                int maxBytesWrong = 0;
                int overallBytesRead = 0;
                int overallBytesWrong = 0;
                float overallPercent = 0.0f;

                List<int> badRanges = new List<int>();
                int badRangeCounter = 0;
                bool foundOne = false;
                for (int i = 0; i < jsonEdatas.Count; i++)
                {
                    // Scan BIN array and find matching byte array
                    for (int j = 0; j < (p0data2BA.Length - jsonEdatas[i].EnemyBytes.Length); j++)
                    {
                        /*
                        for (int k = 0; k < badRanges.Count - 1; k = k+2)
                        {
                            if ((j >= badRanges[badRangeCounter] && j <= badRanges[badRangeCounter + 1]) || (j + jsonEdatas[i].EnemyBytes.Length-1 >= badRanges[badRangeCounter] && j + jsonEdatas[i].EnemyBytes.Length-1 <= badRanges[badRangeCounter + 1]))
                            {

                                j = badRanges[badRangeCounter + 1] + 1;
                                break;
                            }
                        }
                        */

                        /*
                        // check this area
                        if (j == 101592272)
                        {
                            // substring each for the length
                            byte[] p0data2Enemy = new byte[(jsonEdatas[i].EnemyBytes.Length)];
                            Array.Copy(p0data2BA, j, p0data2Enemy, 0, (jsonEdatas[i].EnemyBytes.Length));




                            //byte[] a = SubArray(p0data2BA, j, jsonEdatas[i].EnemyBytes.Length);
                            // compare p0data2Enemy with jsonEdatas[i].EnemyBytes

                            //outputText.Text += "\n j: " + j + " p0data2Enemy Length: " + p0data2Enemy.Length;
                            double percentWrong = -1.0;
                            int badBytesCount = 0;

                            //compare each byte
                            for (int k = 0; k < p0data2Enemy.Length; k++)
                            {
                                if (p0data2Enemy[k] != jsonEdatas[i].EnemyBytes[k])
                                {
                                    badBytesCount++;
                                }

                            }
                            //generate a percent of incorrectness
                            percentWrong = ((badBytesCount * 1.0f) / (jsonEdatas[i].EnemyBytes.Length * 1.0f)) * 100.0f;



                            outputText.Text += "\nThis should be it:";
                            outputText.Text += "\nSub Array: \n";
                            for (int p = 0; p < p0data2Enemy.Length; p++)
                            {
                                outputText.Text += "" + p0data2Enemy[p];
                            }
                            outputText.Text += "\nSource Array: \n";
                            for (int p = 0; p < jsonEdatas[i].EnemyBytes.Length; p++)
                            {
                                outputText.Text += "" + jsonEdatas[i].EnemyBytes[p];
                            }
                            outputText.Text += "\nPercentWrong: " + percentWrong + " BadBytesCount: " + badBytesCount + " StartAddress: " + j;
                        }
                        */

                        if (p0data2BA[j] == jsonEdatas[i].EnemyBytes[0]
                            && p0data2BA[j + jsonEdatas[i].EnemyBytes.Length - 1] == jsonEdatas[i].EnemyBytes[jsonEdatas[i].EnemyBytes.Length - 1]
                            && p0data2BA[j + jsonEdatas[i].EnemyBytes.Length - 2] == jsonEdatas[i].EnemyBytes[jsonEdatas[i].EnemyBytes.Length - 2]
                            && p0data2BA[j + jsonEdatas[i].EnemyBytes.Length - 3] == jsonEdatas[i].EnemyBytes[jsonEdatas[i].EnemyBytes.Length - 3]
                            && p0data2BA[j + jsonEdatas[i].EnemyBytes.Length - 4] == jsonEdatas[i].EnemyBytes[jsonEdatas[i].EnemyBytes.Length - 4]

                            )
                        {
                            // substring each for the length
                            byte[] p0data2Enemy = new byte[(jsonEdatas[i].EnemyBytes.Length)];
                            Array.Copy(p0data2BA, j, p0data2Enemy, 0, (jsonEdatas[i].EnemyBytes.Length));

                            


                            //byte[] a = SubArray(p0data2BA, j, jsonEdatas[i].EnemyBytes.Length);
                            // compare p0data2Enemy with jsonEdatas[i].EnemyBytes

                            //outputText.Text += "\n j: " + j + " p0data2Enemy Length: " + p0data2Enemy.Length;
                            float percentWrong = -1.0f;
                            int badBytesCount = 0;

                            //compare each byte
                            for (int k = 0; k < p0data2Enemy.Length; k++)
                            {
                                if (p0data2Enemy[k] != jsonEdatas[i].EnemyBytes[k])
                                {
                                    badBytesCount++;
                                }
                                
                            }
                            //generate a percent of incorrectness
                            percentWrong = ((float)badBytesCount / (float)jsonEdatas[i].EnemyBytes.Length) * 100.0f;

                            
                            

                            if (percentWrong <= 10.0f)
                            {
                                //outputText.Text += "\nPercentWrong: " + percentWrong + " BadBytesCount: " + badBytesCount;
                            }
                            //checks for
                            /*
                            if (percentWrong < 1.0f && percentWrong >= 1.0f)
                            {
                                countMistakes++;
                                overallBytesWrong += badBytesCount;
                                if (badBytesCount > maxBytesWrong)
                                {
                                    maxBytesWrong = badBytesCount;
                                }
                                outputText.Text += "\nName: " + jsonEdatas[i].EnemyFolder.Substring(56, 18) + " MisCount: " + countMistakes + " PercentWrong: " + percentWrong + " BadBytesCount: " + badBytesCount;
                            }
                            */
                            if (percentWrong < 0.1f)
                            {
                                outputText.Text += "\nPercentWrong: " + percentWrong + " BadBytesCount: " + badBytesCount + " StartAddress: " + j;
                                jsonEdatas[i].BinAddressStart = j;
                                //outputText.Text += "\nNumber of Bytes: " + jsonEdatas[i].EnemyBytes.Length + " Incorrect Byte Count: " + badBytesCount + " Incorrect Byte Percent: " + jsonEdatas[i].IncorrectBytesPercent + " Bin Address: " + jsonEdatas[i].BinAddressStart;
                                /*
                                outputText.Text += "\nSub Array: \n";
                                
                                for (int p = 0; p < p0data2Enemy.Length; p++)
                                {
                                    outputText.Text += "" + p0data2Enemy[p];
                                }
                                outputText.Text += "\nSource Array: \n";
                                for (int p = 0; p < jsonEdatas[i].EnemyBytes.Length; p++)
                                {
                                    outputText.Text += "" + jsonEdatas[i].EnemyBytes[p];
                                }
                                */
                                //stop checking once found good match
                                jsonEdatas[i].IncorrectBytesCount = badBytesCount;
                                jsonEdatas[i].IncorrectBytesPercent = (float)percentWrong;
                                jsonEdatas[i].BinAddressStart = j;
                                //outputText.Text += "\nNumber of Bytes: " + jsonEdatas[i].EnemyBytes.Length + " Incorrect Byte Count: " + jsonEdatas[i].IncorrectBytesCount + " Incorrect Byte Percent: " + jsonEdatas[i].IncorrectBytesPercent + " Bin Address: " + jsonEdatas[i].BinAddressStart;
                                countLeft--;
                                outputText.Text += "\nName: " + jsonEdatas[i].EnemyFolder.Substring(56, 18) + " Count: " + countLeft;
                                foundOne = true;
                                //badRanges.Add(j);
                                //badRanges.Add(j+ jsonEdatas[i].EnemyBytes.Length -1);
                                //break;
                            }
                            
                            
                        }
                    }
                    //fail
                    //outputText.Text += "\nFailed \nNumber of Bytes: " + jsonEdatas[i].EnemyBytes.Length + " Incorrect Byte Count: " + jsonEdatas[i].IncorrectBytesCount + " Incorrect Byte Percent: " + jsonEdatas[i].IncorrectBytesPercent + " Bin Address: " + jsonEdatas[i].BinAddressStart;
                    if (foundOne == false)
                    {
                        outputText.Text += "\nFailed to find a match for: " + jsonEdatas[i].EnemyFolder.Substring(56, 18);
                    }
                    foundOne = false;
                    //overallBytesRead += jsonEdatas[i].EnemyBytes.Length;
                    //overallPercent = ((float)overallBytesWrong / (float)overallBytesRead) * 100.0f;
                    //outputText.Text += "\nOveral Bytes Wrong: " + overallBytesWrong + " Overall Percent: " + overallPercent + " Max Bytes wrong: " + maxBytesWrong;
                }
                /*
                byte[] SubArray(byte[] data, int index, int length)
                {
                    byte[] result = new byte[length];
                    Array.Copy(data, index, result, 0, length);
                    return result;
                }
                void test()
                {
                    byte[] data = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                    byte[] sub = SubArray(data, 3, 4); // contains {3,4,5,6}
                }
                */


                // for each byte array in the enemy json, search the binary for this file and report match.



            }

            // p0data7 Compare Files
            if (p0data2Rand.Checked)
            {
                // Build list of features to randomize based on UI check box. 

                // if checkbox, add this to work list

                // need to add lists of limits here.(){
                var xLength = 0;
                    // the length of my research for limit items.
                int[][] natLimits =
                {
                    new int[xLength], new int[xLength], new int[xLength]
                };

                var worklist = WorkListBuild(options);

                EnemiesRand(workList);



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


        List<int[]> WorkListBuild(string options)
        {
            // build list of all options checked and return the list
            
            return workList;
        }


        void EnemiesRand(List<int[]> workList)
        {
            // Iterate through all work lists and edit all values., items, steals, each array is is the stock addresses
            for (int wl = 0; wl < workList.Count; wl++)
            {
                // this is a list of 

                // Iterate through all all battle files
                for (int e2c = 0; e2c < enemies2.Count; e2c++)
                {

                    // Iterate through all values in the work array.
                    // for each value, randomize,
                    //  check against work list to ensure no duplicates.
                    // check against natlimit, if natlimit 3 = nat limit 2,

                    // if not, use that randomzied value, and incrmente natlmit3
                    // add to worklist and continue.
                    // once the work list is full. empty it
                    // continue

                    // This way we ensure all items are accessable at least once before reshuffling.

                    // on this note, we don't want to blow through all the items in the frist 100 items, shops etc
                    // we want to spread out writing values across all enemies and stores equally.

                    // so mabye we iterate through a different order, so instead of editing all values of a single enemy, shop etc. 
                    // we iterate through one value of each enemy file. 

                    // in the end we cover all values of all enemies, its just the order we write them will help spread all items out
                    //



                }


            }



            //drop  84, 85, 86, 87
            //steal 88, 89, 90, 91
            //blu   135
            //card  169
            byte testValue = 1;

            int[] valueLocations = new int[] { 84, 85, 86, 87, 88, 89, 90, 91, 135, 169 };
            int[] valueLocations0group = new int[] { 28, 29, 30, 31, 32, 33, 34, 35, 79, 113 }; // this is offsets for 0 enemy groups, includes 


            //Iterate through all enemies in enemies2.count
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
            }

            

            /*
            // old partially failed test, change each enemy group offset
            for (int j = 0; j < enemies2[i].EnemyGroupAmount; j++)
            {
                enemies2[i].EnemyBytes[valueLocations[j]] = testValue;
            }
            */
            
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
            // Only scan second dir if needed
            if (dir2Text.Text.Length > 0)
            {
                DirSearch_ex3(@dir2Text.Text);
                dir2Files = dirFiles;
                dirFiles = new List<string>();
            }
            
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
                // only create Enemies Data file if needed
                if (dir2Text.Text.Length > 10)
                {
                    enemies2 = EnemyFilesScan(files2);
                }
                
                
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

                    if (JsonCheckBox.Checked)
                    {
                        //  JSON MODE
                        int indexDir = @files[i].IndexOf("StreamingAssets");
                        string subPath = @files[i].Substring(indexDir);
                        enemiesF.Add(new EnemiesData("\\" + @subPath, File.ReadAllBytes(files[i])));
                    }
                    else
                    {
                        // BYTES FILE MODE
                        //outputText.Text += "\nEnemy Group Amount= " + files[i][1] + " Enemy List Amount+= " + files[i][2];
                        enemiesF.Add(new EnemiesData(@files[i], File.ReadAllBytes(files[i])));
                    }
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
                //outputText.Text += "\nBefore trim: " + buffer.Length;



                int lastIndex = Array.FindLastIndex(buffer, b => b != 0);
                //p0data2 enemies, hdes seems to cut at  3 digits short of last 00, including the last digit, so it 
                Array.Resize(ref buffer, lastIndex - 3); // should be +1, but according to Hades, should be -3. Trusting Hades

                //outputText.Text += "\nafter  " + buffer.Length;
                //outputText.Text += "\nTrim bytes: ";
                /*
                for (int j = buffer.Length - 1; j < enemiesT[i].EnemyBytes.Length; j++)
                {
                    outputText.Text += enemiesT[i].EnemyBytes[j] + " ";
                }
                */
                /*
                for (int j = 0; j < enemiesT[i].EnemyBytes.Length; j++)
                {
                    if (j > buffer.Length)
                    {
                        outputText.Text += enemiesT[i].EnemyBytes[j] + " ";
                    }
                    
                }
                */
                enemiesT[i].EnemyBytes = buffer;

                //outputText.Text += "\n after write: " + enemiesT[i].EnemyBytes.Length;
                enemiesTZ.Add(new EnemiesData(@enemiesT[i].EnemyFolder, buffer));
                /*
                for (int j = 0; j < enemiesTZ[i].EnemyBytes.Length; j++)
                {
                    //outputText.Text += "" + enemiesTZ[i].EnemyBytes[j] + ", ";
                }
                */
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
        bool EnemyJsonStoreFiles(List<EnemiesData> enemies3)
        {
            string fileName = "StockEnemyBytesJson.json";
            string eJsonString = JsonSerializer.Serialize(enemies3);
            DirectoryInfo di5 = Directory.CreateDirectory(@dir1Text.Text + "\\Json");
            Directory.SetCurrentDirectory(@dir1Text.Text + "\\Json\\");
            File.WriteAllText(fileName, eJsonString);
            //output file to console during write
            //Console.WriteLine(File.ReadAllText(fileName));

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
            if (dir1Text.Text.Length > 0)
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
