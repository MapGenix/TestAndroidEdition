using System;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class MapMouseEventArgs : EventArgs
    {
        private InteractionArguments _interactionArguments;

        public MapMouseEventArgs(InteractionArguments interactionArguments)
        {
            this._interactionArguments = interactionArguments;
        }

        public InteractionArguments InteractionArguments
        {
            get
            {
                return _interactionArguments;
            }
        }
    }
}