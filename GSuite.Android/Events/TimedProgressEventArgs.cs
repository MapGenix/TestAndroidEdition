using System;
using System.ComponentModel;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class TimedProgressEventArgs : ProgressChangedEventArgs
    {
       
        public TimedProgressEventArgs(int elapsedMilliseconds, int drawTilesProgress, object userState)
            : base(drawTilesProgress, userState)
        {
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        public int ElapsedMilliseconds { get; set; }
    }
}
