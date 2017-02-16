using System;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class MapKeyEventArgs : EventArgs
    {
        private KeyEventInteractionArguments _interactionArguments;

        public MapKeyEventArgs(KeyEventInteractionArguments interactionArguments)
        {
            this._interactionArguments = interactionArguments;
        }

        public KeyEventInteractionArguments InteractionArguments
        {
            get
            {
                return _interactionArguments;
            }
            set
            {
                _interactionArguments = value;
            }
        }
    }
}