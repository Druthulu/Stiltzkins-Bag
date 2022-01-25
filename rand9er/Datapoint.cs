using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rand9er
{
    //  ONE PER ENTRANCE, 7 per FieldID
    [Serializable]
    public class Datapoint
    {
        private int fieldID;
        private string fieldName;
        private bool checkSum;
        private bool checkSumBytes;
        private int rangeV;
        private int headerC;
        private bool touched;
        private int[][] addressNvalue;  //  [4][2]
        private byte[][] valueBytes;    //  [4][2]
        public Datapoint(int fieldID, string fieldName, bool checkSum, bool checkSumBytes, int rangeV, int headerC, bool touched, int[][] addressNvalue, byte[][] valueBytes)
        {
            this.fieldID = fieldID;
            this.fieldName = fieldName;
            this.checkSum = checkSum;
            this.checkSumBytes = checkSumBytes;
            this.rangeV = rangeV;
            this.headerC = headerC;
            this.touched = touched;
            this.addressNvalue = addressNvalue;
            this.valueBytes = valueBytes;
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
        public bool CheckSum
        {
            get { return checkSum; }
            set { checkSum = value; }
        }
        public bool CheckSumBytes
        {
            get { return checkSumBytes; }
            set { checkSumBytes = value; }
        }
        public int RangeV
        {
            get { return rangeV; }
            set { rangeV = value; }
        }
        public int HeaderC
        {
            get { return headerC; }
            set { headerC = value; }
        }
        public bool Touched
        {
            get { return touched; }
            set { touched = value; }
        }
        public int[][] AddressNvalue
        {
            get { return addressNvalue; }
            set { addressNvalue = value; }
        }
        public byte[][] ValueBytes
        {
            get { return valueBytes; }
            set { valueBytes = value; }
        }
        public void SumCheck()
        {
            checkSum = false;
            checkSum = ((addressNvalue[0][1] == addressNvalue[1][1]) && (addressNvalue[0][1] == addressNvalue[3][1]));
        }
        public void SumCheckBytes()
        {
            checkSumBytes = false;
            checkSumBytes = ((valueBytes[0][0] == valueBytes[1][0]) && (valueBytes[0][0] == valueBytes[3][0]) && (valueBytes[0][1] == valueBytes[1][1]) && (valueBytes[0][1] == valueBytes[3][1]));
        }
    }

}
