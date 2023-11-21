using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rand9er
{
    public class Character
    {
        private string name;
        private byte baseStatsIndex;
        private byte[] baseStats;
        public Character(string name, byte baseStatsIndex, byte[] baseStats)
        {
            this.name = name;
            this.baseStatsIndex = baseStatsIndex;
            this.baseStats = baseStats;
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public byte BaseStatsIndex
        {
            get { return baseStatsIndex; }
            set { baseStatsIndex = value; }
        }
        public byte[] BaseStats
        {
            get { return baseStats; }
            set { baseStats = value; }
        }
    }
}
