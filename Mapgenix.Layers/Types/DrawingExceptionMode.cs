using System;

namespace Mapgenix.Layers
{
    /// <summary>Exceptions when layer or overlay fails to draw.</summary>
    [Serializable]
    public enum DrawingExceptionMode
    {
        Default = 0,
        ThrowException = 1,
        DrawException = 2
    }
}
