using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rand9er
{
    public class Synth
    {
        public static void Synthesis()
        {
            //settings for comment line numebr 5
            rand9er.set = "#Randomized by Stiltzkin's Bag 1.0   Seed:" + rand9er.data_int + "   set:";
            if (rand9er.c_requireBool) { rand9er.set += "/requirements "; }
            if (rand9er.c_resultBool) { rand9er.set += "/result "; }
            if (rand9er.c_pricesBool) { rand9er.set += "/prices "; }

            //int data_int has seedDNA
            rand9er.a_stockSynthesis = StockCSVs.StockSynthesisCSV();
            rand9er.a_synthdata = rand9er.a_stockSynthesis;
            int a = 0, b = 0;
            for (int i = 0; i < rand9er.a_synthdata.Length; i++)
            {
                //lines 6+ has useful data
                if (i > 5)
                {
                    string[] a_synthline = rand9er.a_synthdata[i].Split(rand9er.separators, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 3; j < a_synthline.Length; j++)
                    {
                        Random rnd = new Random((rand9er.data_int / (i + j*2 + 42)) + (rand9er.data_int / (j + 1)) + (rand9er.data_int / ((i*2 * j*3) + 1)));
                        if ((j == 3) & (rand9er.c_pricesBool))
                        {
                            a = 200; b = 20000;
                            a_synthline[j] = rnd.Next(a, b).ToString();
                            a_synthline[0] = "#Randomized";
                        }
                        if ((j == 4) & (rand9er.c_resultBool))
                        {
                            a = 1; b = 236;
                            a_synthline[j] = rnd.Next(a, b).ToString();
                            a_synthline[0] = "#Randomized";
                        }
                        if ((j > 4 & j < 7) & (rand9er.c_requireBool))
                        {
                            a = 1; b = 236;
                            a_synthline[j] = rnd.Next(a, b).ToString();
                            a_synthline[0] = "#Randomized";
                        }
                    }
                    //rebuild line array to  line string, store in string array
                    rand9er.a_synthdata[i] = String.Join(";", a_synthline);
                }
            }
            rand9er.a_synthdata[5] = rand9er.set;
            //string[] a_synthdata has output data
        }
        //private void SynthOutput()
        //{
        //    //string[] a_synthdata has input data
        //    for (int i = 0; i < a_synthdata.Length; i++)
        //    {
        //        richTextBox_output.Text = richTextBox_output.Text + a_synthdata[i] + "\n";
        //    }
        //}

    }
}
