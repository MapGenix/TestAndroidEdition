using System;
using Mapgenix.Shapes;

namespace Mapgenix.FeatureSource
{
    public class ShapeFileIndexEventArgs : EventArgs
    {
        private readonly int recordCount;
        private readonly int currentRecordIndex;
        private readonly Feature currentFeature;
        private readonly DateTime startProcessTime;
        private bool cancel;
        private readonly string shapePathFileName;

       
        public ShapeFileIndexEventArgs()
            : this(0, 0, new Feature(), DateTime.Now, false, string.Empty)
        {
        }

        public ShapeFileIndexEventArgs(int recordCount, int currentRecordIndex, Feature currentFeature, DateTime startProcessTime, bool cancel)
            : this(recordCount, currentRecordIndex, currentFeature, startProcessTime, cancel, string.Empty)
        {
        }

        public ShapeFileIndexEventArgs(int recordCount, int currentRecordIndex, Feature currentFeature, DateTime startProcessTime, bool cancel, string shapePathFileName)
        {
            this.recordCount = recordCount;
            this.currentRecordIndex = currentRecordIndex;
            this.currentFeature = currentFeature;
            this.startProcessTime = startProcessTime;
            this.cancel = cancel;
            this.shapePathFileName = shapePathFileName;
        }

        public int RecordCount
        {
            get { return recordCount; }
        }

       
        public int CurrentRecordIndex
        {
            get { return currentRecordIndex; }
        }

        public Feature CurrentFeature
        {
            get { return currentFeature; }
        }

       
        public DateTime StartProcessTime
        {
            get { return startProcessTime; }
        }

        public bool Cancel
        {
            get { return cancel; }
            set { cancel = value; }
        }

        public string ShapePathFileName
        {
            get { return shapePathFileName; }
        }
    }
}
