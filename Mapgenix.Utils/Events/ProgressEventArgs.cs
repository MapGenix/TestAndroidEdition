using System.ComponentModel;

namespace Mapgenix.Utils
{
    public class ProgressEventArgs : ProgressChangedEventArgs
    {
        private readonly int _featuresDrawn;
        private readonly int _featuresToDraw;

        public ProgressEventArgs()
            : this(0, null, 0, 0)
        {
        }

        public ProgressEventArgs(int progressPercentage, object userState)
            : this(progressPercentage, userState, 0, 0)
        {
        }

        public ProgressEventArgs(int progressPercentage, object userState, int featuresToDraw, int featuresDrawn)
            : base(progressPercentage, userState)
        {
            _featuresToDraw = featuresToDraw;
            _featuresDrawn = featuresDrawn;
        }

        public bool Cancel { get; set; }

        public int Total
        {
            get { return _featuresToDraw; }
        }

        public int Completed
        {
            get { return _featuresDrawn; }
        }
    }
}