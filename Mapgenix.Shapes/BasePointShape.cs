using System;

namespace Mapgenix.Shapes 
{
    /// <summary>Root of all point-based shapes, such as PointShape and MultiPointShape.</summary>
    [Serializable]
    public abstract class BasePoint : BaseShape
    {
        protected BasePoint()
        {
        }
    }
}
