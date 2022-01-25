using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rand9er
{
    public class Shops
    {
        public static void ShopItems()
        {
            //settings for comment line numebr 5
            rand9er.set = "#Randomized by Stiltzkin's Bag 1.0   Seed:" + rand9er.data_int + "   set:";
            if (rand9er.c_safeBool) { rand9er.set = rand9er.set + "/safe "; }
            if (rand9er.c_randomBool) { rand9er.set = rand9er.set + "/random"; }
            if (rand9er.c_maxBool) { rand9er.set = rand9er.set + "/max"; }
            if (rand9er.c_randomEn & rand9er.c_randomBool | rand9er.c_maxEn & rand9er.c_maxBool)
            {
                if (rand9er.c_medicitemsBool) { rand9er.set += "+medic items "; }
                if (rand9er.c_medicshopsBool) { rand9er.set += "->override medic shops"; }
            }

            rand9er.a_stockShopItems = StockCSVs.StockShopItemsCSV();

            if (rand9er.c_medicshopsBool) { rand9er.randl = 32; } else { rand9er.randl = 23; }
            if (rand9er.c_randomBool)
            {
                for (int i = 0; i < rand9er.randl; i++)
                {
                    Random rnd = new Random((rand9er.data_int / (i+1)) + i);
                    rand9er.a_shopItems[i] = rnd.Next(1, 32);
                    //if (i == 0) { richTextBox_debug.Text = richTextBox_debug.Text + "\ntest " + a_shopItems[i]; }
                }
            }
            int[] a_shopItems = rand9er.a_shopItems;
            int[][] aa_shopItems1 =
            {
                new int[a_shopItems[0]],new int[a_shopItems[1]],new int[a_shopItems[2]],new int[a_shopItems[3]],new int[a_shopItems[4]],new int[a_shopItems[5]],
                new int[a_shopItems[6]],new int[a_shopItems[7]],new int[a_shopItems[8]],new int[a_shopItems[9]],new int[a_shopItems[10]],new int[a_shopItems[11]],
                new int[a_shopItems[12]],new int[a_shopItems[13]],new int[a_shopItems[14]],new int[a_shopItems[15]],new int[a_shopItems[16]],new int[a_shopItems[17]],
                new int[a_shopItems[18]],new int[a_shopItems[19]],new int[a_shopItems[20]],new int[a_shopItems[21]],new int[a_shopItems[22]],new int[a_shopItems[23]],
                new int[a_shopItems[24]],new int[a_shopItems[25]],new int[a_shopItems[26]],new int[a_shopItems[27]],new int[a_shopItems[28]],new int[a_shopItems[29]],
                new int[a_shopItems[30]],new int[a_shopItems[31]]
            };
            rand9er.aa_shopItems = aa_shopItems1;
            //richTextBox_output.Text = "";
            for (int i = 0; i < rand9er.randl; i++)
            {
                //richTextBox_output.Text = richTextBox_output.Text + "\n i: " + i + " ";
                for (int j = 0; j < rand9er.aa_shopItems[i].Length; j++)
                {
                    Random rnd = new Random((rand9er.data_int / (i + j*9 + 1)) + (rand9er.data_int / (j + 1)));
                    int rnd2 = rnd.Next(1, rand9er.items);
                    if (rnd2 == 250) { rnd2 = 253; }
                    rand9er.aa_shopItems[i][j] = rnd2;
                    //richTextBox_output.Text = richTextBox_output.Text + /*"j: " + j + " :r: "*/ aa_shopItems[i][j] + "; ";
                }
                //richTextBox_output.Text = richTextBox_output.Text + "\n;";
            }
        }
        public static void MedicItems()
        {
            //int[] a_empty = { 0 }; //33
            int[] a_empty = rand9er.a_empty;
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
            string[] medicShopsE1 = { "0", "0", "0", "0", "0", "0", "0", "0", "0" };
            if (rand9er.c_safeBool)
            {
                rand9er.aa_medicItems = aa_medicItems1;
            }
            else if (rand9er.c_safeBool) { rand9er.aa_medicItems = aa_medicItemsE1; }

            if (rand9er.c_medicshopsEn & rand9er.c_medicshopsBool)
            {
                rand9er.medicShops = medicShopsE1;
            }
            else if (!rand9er.c_medicshopsEn | rand9er.c_medicshopsBool) { rand9er.medicShops = medicShops1; }

        }
        public static void ShopCombo()
        {
            int[][] aa_cs_medici = rand9er.aa_medicItems; // input medic items lines 7-29
            int[][] aa_cs_shopi = rand9er.aa_shopItems; // input RNG shop items
            string[] a_cs_medics = rand9er.medicShops, a_ssi = rand9er.a_stockShopItems; //input medic shops lines 30-38
            int[] a_shopItems_1safe = new int[] { 16, 16, 9, 13, 25, 18, 28, 13, 14, 32, 14, 32, 29, 21, 22, 25, 21, 30, 21, 30, 6, 12, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0 };  //use this to 
            int[] a_medicl = new int[] { 0, 0, 7, 7, 0, 0, 0, 9, 0, 0, 9, 0, 0, 11, 9, 7, 11, 0, 10, 0, 0, 12, 10, 0, 0, 5, 7, 8, 10, 11, 12, 12 };
            string[] a_cs_comboSafe = rand9er.a_comboSafe;

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
                        if (rand9er.c_safeBool & i < 23)
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
                        if (rand9er.c_safeBool)
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
                        if (rand9er.c_safeBool & i < 23)
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
                    if (i > 22 & !rand9er.c_medicshopsEn | i > 22 & !rand9er.c_medicshopsBool)
                    {
                        a_cs_comboSafe[i] = rand9er.medicShops[i - 23];
                    }
                }
            }
            //resize out back into fullsize datafile
            for (int i = 6; i < a_ssi.Length; i++)
            {
                a_ssi[i] = a_cs_comboSafe[i - 6];
            }
            a_ssi[5] = rand9er.set;
            rand9er.a_comboSafe = a_ssi;
            //string[] a_comboSafe has output data
        }
        //public static void ShopOutput()
        //{
        //    //string[] a_comboSafe has input data
        //    for (int i = 0; i < a_comboSafe.Length; i++)
        //    {
        //        richTextBox_output.Text = richTextBox_output.Text + a_comboSafe[i] + "\n";
        //    }
        //}

    }
}
