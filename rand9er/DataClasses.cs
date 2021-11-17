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

    //  ONE PER FIELD_ID
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
    public class FieldData
    {
        private int fieldID;
        private string fieldName;
        private List<int> exitNum;    //  headerC
        private List<int[]> exitAddress;  //  21  (exit,exit,---,exit * 7)
        private List<int[]> genAddress;   //  7   (---,---,gen,--- * 7)
        private List<int> exitValue;    //  1 per exit in field, 
        private List<int> genValue;
        private List<int> allGE;
        public FieldData(int fieldID, string fieldName, List<int> exitNum, List<int[]> exitAddress, List<int[]> genAddress, List<int> exitValue, List<int> genValue, List<int> allGE)
        {
            this.fieldID = fieldID;
            this.fieldName = fieldName;
            this.exitNum = exitNum;
            this.exitAddress = exitAddress;
            this.genAddress = genAddress;
            this.exitValue = exitValue;
            this.genValue = genValue;
            this.allGE = allGE;
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
        public List<int> ExitNum
        {
            get { return exitNum; }
            set { exitNum = value; }
        }
        public List<int[]> ExitAddress
        {
            get { return exitAddress; }
            set { exitAddress = value; }
        }
        public List<int[]> GenAddress
        {
            get { return genAddress; }
            set { genAddress = value; }
        }
        public List<int> ExitValue
        {
            get { return exitValue; }
            set { exitValue = value; }
        }
        public List<int> GenValue
        {
            get { return genValue; }
            set { genValue = value; }
        }
        public List<int> AllGE
        {
            get { return allGE; }
            set { allGE = value; }
        }
    }

    //unused
    public class GEindex
    {
        private int fieldID;
        private bool touched;
        private List<int> entrances;
        //

        public GEindex(int fieldID, bool touched, List<int> entrances)
        {
            this.fieldID = fieldID;
            this.touched = touched;
            this.entrances = entrances;
        }
        public int FieldID
        {
            get { return fieldID; }
            set { fieldID = value; }
        }
        public bool Touched
        {
            get { return touched; }
            set { touched = value; }
        }
        public List<int> Entrances
        {
            get { return entrances; }
            set { entrances = value; }
        }

    }
    public class DataP2        //  one per field, each List element is an exiting (external fid/ge), each AllGE is incoming GE's found in other fields 
    {
        private int fieldID;
        private string fieldName;
        private List<List<int>> tet;
        private List<int[]> exitAddress;  //  21  exit,exit,---,exit * 7
        private List<int[]> genAddress;   //  7   ---,---,gen,--- * 7
        private List<int> exitValue;
        private List<int> genValue;
        private List<int> allGE;    //  all possible GE's for this field
        public DataP2(int fieldID, string fieldName, List<int[]> exitAddress, List<int[]> genAddress, List<int> exitValue, List<int> genValue, List<int> allGE)
        {
            this.fieldID = fieldID;
            this.fieldName = fieldName;
            this.exitAddress = exitAddress;
            this.genAddress = genAddress;
            this.exitValue = exitValue;
            this.genValue = genValue;
            this.allGE = allGE;
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
        public List<int[]> ExitAddress
        {
            get { return exitAddress; }
            set { exitAddress = value; }
        }
        public List<int[]> GenAddress
        {
            get { return genAddress; }
            set { genAddress = value; }
        }
        public List<int> ExitValue
        {
            get { return exitValue; }
            set { exitValue = value; }
        }
        public List<int> GenValue
        {
            get { return genValue; }
            set { genValue = value; }
        }
        public List<int> AllGE
        {
            get { return allGE; }
            set { allGE = value; }
        }
    }

    class DataClasses
    {

    }
}
