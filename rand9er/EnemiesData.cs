using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rand9er
{
    public class EnemyList
    {
        private List<EnemiesData> enemyList;
        public EnemyList()
        {
            List<EnemiesData> enemyList = new List<EnemiesData>();
        }
        public EnemiesData EnemyAdd(EnemiesData eData)
        {
            enemyList.Add(eData);
            return null;
        }
        public EnemiesData EnemyGet(int value)
        {
            return enemyList[value];
        }
        public int EnemyCount
        {
            get { return enemyList.Count(); }
        }
    }


    public class EnemiesData
    {
        //private int enemyID;
        private string enemyFolder;
        private byte[] enemyBytes;
        private int enemyGroupAmount;
        private int enemyListAmount;
        private int binAddressStart;
        private int binAddressEnd;
        private bool checkSum;
        public EnemiesData(/*int enemyID, */string enemyFolder, byte[] enemyBytes)
        {
            //this.enemyID = enemyID;
            this.enemyFolder = enemyFolder;
            this.enemyBytes = enemyBytes;
            this.enemyGroupAmount = enemyBytes[1];
            this.enemyListAmount = enemyBytes[2];
            this.binAddressStart = -1;
            this.binAddressEnd = -1;
            this.checkSum = false;
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
        public bool CheckSum
        {
            get { return SumCheck(); }
            set { checkSum = value; }
        }
        private bool SumCheck()
        {
            checkSum = false; 
            bool /*range1 = false, */range2 = false, range3 = false, range4 = false, range5 = false;
            //if (enemyID > -1) { range1 = true; }
            if (enemyFolder.Length > 0) { range2 = true; }
            if (EnemyBytes.Length > 0) { range3 = true; }
            if (binAddressStart > -1) { range4 = true; }
            if (binAddressEnd > -1) { range5 = true; }
            return (/*range1 && */range2 && range3 && range4 && range5);
        }
    }
}
