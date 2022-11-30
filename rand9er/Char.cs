using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rand9er
{
    public static class Char
    {

        // Refactor checks and string builder for CSV files



        public static void Set()
        {

            //settings for comment line numebr 5
            rand9er.set = "#Randomized by Stiltzkin's Bag 1.0   Seed:" + rand9er.data_int + "   set:";
            if (rand9er.c_abilitygemsBool) { rand9er.set += "/abilitygems "; }
            if (rand9er.c_basestatsBool) { rand9er.set += "/base stats "; }
            if (rand9er.c_defaultBool)
            {
                rand9er.set += "/default equipment";
                if (rand9er.c_random_eBool) { rand9er.set += "->random"; }
                if (rand9er.c_all_eBool) { rand9er.set += "->share"; }
                if (rand9er.c_main_eBool) { rand9er.set += "->stock"; }
            }
        }
        public static void Character()
        {
            Set();
            if (rand9er.c_abilitygemsBool)
            {
                rand9er.a_stockAbilityGems = StockCSVs.StockAbilityGemsCSV();
                rand9er.a_gemdata = rand9er.a_stockAbilityGems;
                int gema = 3, gemb = 14;
                for (int i = 6; i < rand9er.a_gemdata.Length; i++)
                {
                    string[] a_gemline = rand9er.a_gemdata[i].Split(rand9er.separators, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < a_gemline.Length; j++) //needs this loop, weird
                    {
                        if (j == 2) //also needs this part, weirder
                        {
                            Random rnd = new Random((rand9er.data_int / (i + 4)) + (rand9er.data_int / (i + 13)));
                            a_gemline[2] = rnd.Next(gema, gemb).ToString();
                        }
                    }
                    rand9er.a_gemdata[i] = String.Join(";", a_gemline);
                }
                rand9er.a_gemdata[5] = rand9er.set;
            }
            //string[] a_gemdata has output data
            if (rand9er.c_basestatsBool)
            {
                rand9er.a_stockBaseStats = StockCSVs.StockBaseStatsCSV();
                rand9er.a_statdata = rand9er.a_stockBaseStats;
                int stata = 8, statb = 24;
                for (int i = 6; i < rand9er.a_statdata.Length; i++)
                {

                    string[] a_statline = rand9er.a_statdata[i].Split(rand9er.separators, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 2; j < a_statline.Length; j++)
                    {
                        Random rnd = new Random((rand9er.data_int / (i*6 + j + 7)) + (rand9er.data_int / (j + 4)) + (rand9er.data_int / ((i*3 * j*5) + 1)));
                        a_statline[j] = rnd.Next(stata, statb).ToString();
                    }
                    rand9er.a_statdata[i] = String.Join(";", a_statline);
                }
                rand9er.a_statdata[5] = rand9er.set;
            }
            //string[] a_statdata has output data
            if (rand9er.c_defaultBool)
            {
                rand9er.a_stockItems = StockCSVs.StockItemsCSV();
                rand9er.a_stockDefaultEquipment = StockCSVs.StockDefaultEquipmentCSV();
                rand9er.a_itemdata = rand9er.a_stockItems;
                rand9er.a_equipdata = rand9er.a_stockDefaultEquipment;
                PermissionEdit();
                Set();
                int rr = 0; int a = 0; int b = 224;
                for (int i = 0; i < rand9er.a_equipdata.Length; i++)
                {
                    if ((i > 5) & (i < 21))
                    {
                        string[] a_equipline = rand9er.a_equipdata[i].Split(rand9er.separators, StringSplitOptions.RemoveEmptyEntries);
                        string[] a_itemline;
                        Random rnd; int rrr = 1;

                        for (int j = 2; j < a_equipline.Length; j++)
                        {
                            if (j < 7)
                            {
                                rnd = new Random((rand9er.data_int / (i*4 + j + 1)) + (rand9er.data_int / (j + 7)) + (rand9er.data_int / ((i * j*8) + 1)));
                                rr = 0; a = 0; b = 224;
                                void ReRoll()
                                {
                                    rr = rnd.Next(a, b);
                                    //pull line from item file using the items.csv+6 offset
                                    a_itemline = rand9er.a_itemdata[rr + 6].Split(rand9er.separators, StringSplitOptions.RemoveEmptyEntries);
                                    //offsets for dup characters in file
                                    int temp_i = i;
                                    if (i == 18) { temp_i = i-3; }
                                    if (i == 19) { temp_i = i-2; }
                                    if (i == 20) { temp_i = i-4; }
                                    //offset is i+9 from defaultequipment file to items file line
                                    if (a_itemline[temp_i + 9] == "0") //fail permission check
                                    {
                                        rrr++;
                                        //rand9er.richTextBox_debug.Text = rrr + " rolls\nI" + rr + "C" + i + "P" + j;
                                        rnd = new Random((rand9er.data_int / (i + j*2 + rrr)) + (rand9er.data_int / (j + rr)) + (rand9er.data_int / ((i*7 * j) + rrr)));
                                        ReRoll(); //continue failing till succeed
                                    }
                                }

                                if (j == 2) //weapon        0-84
                                {
                                    a = rand9er.weapa; b = rand9er.weapb;
                                    ReRoll();
                                    a_equipline[j] = rr.ToString();
                                }
                                if (j == 3) //armlet        88-111
                                {
                                    a = rand9er.armleta; b = rand9er.armletb;
                                    ReRoll();
                                    a_equipline[j] = rr.ToString();
                                }
                                if (j == 4) //helmets       112-147
                                {
                                    a = rand9er.helma; b = rand9er.helmb;
                                    ReRoll();
                                    a_equipline[j] = rr.ToString();
                                }
                                if (j == 5) //armor         148-191
                                {
                                    a = rand9er.armora; b = rand9er.armorb;
                                    ReRoll();
                                    a_equipline[j] = rr.ToString();

                                }
                                if (j == 6) //accessory     192-223
                                {
                                    a = rand9er.acca; b = rand9er.accb;
                                    ReRoll();
                                    a_equipline[j] = rr.ToString();
                                }
                            }
                            if (j == 7) //comment
                            {
                                a_equipline[j] = "# randomized";
                            }
                        }
                        rand9er.a_equipdata[i] = String.Join(";", a_equipline);
                    }
                }
                rand9er.a_equipdata[5] = rand9er.set;
            }
            //string[]  a_equipdata has output data
        }
        public static void PermissionEdit()
        {
            Set();

            if (rand9er.c_random_eEn & rand9er.c_random_eBool)
            {
                //edit permissions to random 0s or 1s
                string[] a_permdata = rand9er.a_itemdata;
                for (int i = 6; i < 230; i++)
                {
                    string[] a_permline = a_permdata[i].Split(rand9er.separators, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 15; j < a_permline.Length - 1; j++)
                    {
                        Random rnd = new Random((rand9er.data_int / (i + j*5)) + (rand9er.data_int / j+8) + (rand9er.data_int / (i*3 * j)));
                        a_permline[j] = rnd.Next(0, 2).ToString();
                    }
                    a_permline[27] = a_permline[27] + " +Random Permissions";
                    a_permdata[i] = String.Join(";", a_permline);
                }
                a_permdata[5] = rand9er.set;
                rand9er.a_itemdata = a_permdata;
            }
            if (rand9er.c_all_eEn & rand9er.c_all_eBool)
            {
                //edit permissions to all 1s
                string[] a_permdata = rand9er.a_itemdata;
                for (int i = 6; i < 230; i++)
                {
                    string[] a_permline = a_permdata[i].Split(rand9er.separators, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 15; j < a_permline.Length - 1; j++)
                    {
                        a_permline[j] = "1";
                    }
                    a_permline[27] = a_permline[27] + " +All Permissions";
                    a_permdata[i] = String.Join(";", a_permline);
                }
                a_permdata[5] = rand9er.set;
                rand9er.a_itemdata = a_permdata;
            }
            if (rand9er.c_main_eBool)
            {
                rand9er.a_itemdata = StockCSVs.StockItemsCSV();
                
                rand9er.a_itemdata[5] = rand9er.set;
                //no permission edits needed for this one
            }
        }
    }
}
