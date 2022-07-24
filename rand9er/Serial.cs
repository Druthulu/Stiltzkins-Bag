using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
//using System.Runtime.Serialization.Formatters.Binary;

namespace rand9er
{
    public class Serial
    {
        //  We are attempting to deseralize the entire bin2/7 files and determine if it would make our work easier
        public static void Deserialize() {
            // scan bin2 or bin7, then part out
            //bin7 is 
            if (!File.Exists(rand9er.binloc + "\\p0data7.bak"))
            { File.Copy(rand9er.binloc + "\\p0data7.bin", rand9er.binloc + "\\p0data7.bak"); }
            if (!File.Exists(rand9er.binloc + "\\p0data2.bak"))
            { File.Copy(rand9er.binloc + "\\p0data2.bin", rand9er.binloc + "\\p0data2.bak"); }
            rand9er.ba_p0data7 = File.ReadAllBytes(rand9er.binloc + "\\p0data7.bin");
            //ba_p2 = File.ReadAllBytes(binloc + "\\p0data2.bin");


            //  we need to scan the entire bin array, 


        }

    }
}
