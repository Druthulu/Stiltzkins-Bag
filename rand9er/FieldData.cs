﻿using System;
using System.Collections.Generic;

namespace rand9er
{
    [Serializable]
    public class FieldData
    {
        private int fieldID;
        private string fieldName;
        private string roomType;
        private List<int> exitNum;    //  headerC
        private List<int[]> exitAddress;  //  21  (exit,exit,---,exit * 7)
        private List<int[]> genAddress;   //  7   (---,---,gen,--- * 7)
        private List<int> exitValue;    //  1 per exit in field, 
        private List<int> genValue;
        private List<int> allGE;
        public FieldData(int fieldID, string fieldName, string roomType, List<int> exitNum, List<int[]> exitAddress, List<int[]> genAddress, List<int> exitValue, List<int> genValue, List<int> allGE)
        {
            this.fieldID = fieldID;
            this.fieldName = fieldName;
            this.roomType = roomType;
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
        public string RoomType
        {
            get { return roomType; }
            set { roomType = value; }
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
}
