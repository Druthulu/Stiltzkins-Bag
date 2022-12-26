using System.Collections.Generic;
using System.Linq;

namespace rand9er
{
    public class EnemiesData
    {
        //private int enemyID;
        private string enemyFolder;
        private byte[] enemyBytes;
        private int enemyGroupAmount;
        private int enemyListAmount;
        private int binAddressStart;
        private float incorrectBytesPercent;
        public EnemiesData(/*int enemyID, */string enemyFolder, byte[] enemyBytes)
        {
            //this.enemyID = enemyID;
            this.enemyFolder = enemyFolder;
            this.enemyBytes = enemyBytes;
            this.enemyGroupAmount = enemyBytes[1];
            this.enemyListAmount = enemyBytes[2];
            this.binAddressStart = -1;
            this.incorrectBytesPercent = 0;
        }
        /*public int EnemyID
        {
            get { return enemyID; }
            set { enemyID = value; }
        }*/
        public string EnemyFolder
        {
            get { return enemyFolder; }
            set { enemyFolder = value; }
        }
        public byte[] EnemyBytes
        {
            get { return enemyBytes; }
            set { enemyBytes = value; }
        }
        public int EnemyListAmount
        {
            get { return enemyListAmount; }
            set { enemyListAmount = value; }
        }
        public int EnemyGroupAmount
        {
            get { return enemyGroupAmount; }
            set { EnemyGroupAmount = value; }
        }
        public int BinAddressStart
        {
            get { return binAddressStart; }
            set { binAddressStart = value; }
        }
        public float IncorrectBytesPercent
        {
            get { return incorrectBytesPercent; }
            set { incorrectBytesPercent = value; }
        }
    }
}
