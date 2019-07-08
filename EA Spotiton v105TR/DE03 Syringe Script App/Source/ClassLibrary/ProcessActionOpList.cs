using System;
using System.Collections;

namespace EA.PixyControl.ClassLibrary
{
    public class ProcessActionOpList
    {
        private Hashtable mHT = new Hashtable(73);

        public void Add(string ProcessActionName, ProcessActionOp ActionOp)
        {
            if (mHT[ProcessActionName] != null) throw new Exception("A method has already been added for " + ProcessActionName);

            mHT.Add(ProcessActionName, ActionOp);
        }

        public void Remove(string ProcessActionName)
        {
            mHT.Remove(ProcessActionName);
        }

        public void Clear()
        {
            mHT.Clear();
        }

        public ProcessActionOp GetProcessActionOp(string ProcessActionName)
        {
            object Obj = mHT[ProcessActionName];

            if (Obj != null) return (ProcessActionOp)mHT[ProcessActionName];

            return null;
        }
    }
}