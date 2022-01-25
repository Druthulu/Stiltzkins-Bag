using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rand9er
{
    public class Seed
    {
        public static void SeedIngest()
        {
            //rand9er.textBox_seed
            //string seed has input data
            rand9er.data_str = ""; //converted seed to data
            rand9er.counter = 1; //reset these counters
            //richTextBox_debug.Text = seed.Length + " seeds planted ";
            for (int i = 0; i < rand9er.seed.Length; i++)
            {
                Random rnd = new Random((int)rand9er.seed[i]);
                rand9er.data_str = rand9er.data_str + rnd.Next(426942, 4269420);
            }
            //string data_str has output data
        }
        public static void Compressor()
        {
            //string data_str has input data
            if (rand9er.data_str.Length > 9)
            {
                rand9er.counter++;
                //  SPLITTER
                int r = 0; if (!(rand9er.data_str.Length % 9 == 0)) { r = 1; }
                int r2 = rand9er.data_str.Length % 9;
                int al = ((rand9er.data_str.Length / 9) + r); //array length plus 1 for/if remainder
                rand9er.data_arr = new int[al]; //reset worker array each cycle
                int ai = 0;
                for (int i = 0; i < rand9er.data_str.Length; i = i + 9)
                {
                    //pbar_leaf.Value = i;
                    ai = ((i + 9) / 9) - 1;
                    if ((al - ai == 1) & (r == 1))
                    {
                        int.TryParse(rand9er.data_str.Substring(i, rand9er.data_str.Length - i), out rand9er.data_arr[ai]); // add remainder if any to array
                        i += 9; //prevent substring(i,9) exception
                    }
                    else
                    {
                        int.TryParse(rand9er.data_str.Substring(i, 9), out rand9er.data_arr[ai]);
                    }
                }
                rand9er.data_str = ""; //reset data_str
                //array data_arr has output data
                //  SEED RNG
                for (int i = 0; i < rand9er.data_arr.Length; i++)
                {
                    Random rnd = new Random(rand9er.data_arr[i]);
                    rand9er.data_str = rand9er.data_str + rnd.Next(4269, 42069);
                }
                //string data_str has output data

                Compressor();
            }
        }
        public static void CleanSeedInt()
        {
            //string data_str has input data
            int.TryParse(rand9er.data_str, out rand9er.data_int);
            //int data_int has output data
        }
    }
}
