using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rand9er
{
    [Serializable]
    public class FieldIndex
    {
        private int fieldID;
        private string fieldName;
        private byte[] fieldBytes;
        private bool checkSum;
        private int[][] rangeStartStop; //  [7][2]
        public FieldIndex(int fieldID, string fieldName, byte[] fieldBytes, bool checkSum, int[][] rangeStartStop)
        {
            this.fieldID = fieldID;
            this.fieldName = fieldName;
            this.fieldBytes = fieldBytes;
            this.checkSum = checkSum;
            this.rangeStartStop = rangeStartStop;
        }
        public int FieldID
        {
            get { return fieldID; }
            set { fieldID = value; }
        }
        public string FieldName
        {
            get { return fieldName; }
            set { fieldName = value; }
        }
        public byte[] FieldBytes
        {
            get { return fieldBytes; }
            set { fieldBytes = value; }
        }
        public bool CheckSum
        {
            get { return checkSum; }
            set { checkSum = value; }
        }
        public int[][] RangeStartStop
        {
            get { return rangeStartStop; }
            set { rangeStartStop = value; }
        }
        public void SumCheck()
        {
            checkSum = false; bool range1 = false; bool range2 = false; bool range3 = false; bool range4 = false; bool range5 = false; bool range6 = false; bool range7 = false;
            if (rangeStartStop[0][0] < rangeStartStop[0][1]) { range1 = true; }
            if (rangeStartStop[1][0] < rangeStartStop[1][1]) { range2 = true; }
            if (rangeStartStop[2][0] < rangeStartStop[2][1]) { range3 = true; }
            if (rangeStartStop[3][0] < rangeStartStop[3][1]) { range4 = true; }
            if (rangeStartStop[4][0] < rangeStartStop[4][1]) { range5 = true; }
            if (rangeStartStop[5][0] < rangeStartStop[5][1]) { range6 = true; }
            if (rangeStartStop[6][0] < rangeStartStop[6][1]) { range7 = true; }
            checkSum = (range1 && range2 && range3 && range4 && range5 && range6 && range7);
        }
    }
}
