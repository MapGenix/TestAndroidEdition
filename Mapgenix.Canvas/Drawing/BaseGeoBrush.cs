using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Mapgenix.Canvas
{
    /// <summary>Brush to fill the interiors of polygon based features.</summary>
    /// <remarks>
    /// 	<para>Abstract base class. Cannot be instantiated. To create a GeoBrush object, use sub classes such as GeoSolidBrush.</para>
    /// </remarks>
    [Serializable]
    public abstract class BaseGeoBrush
    {
        private static long _geoBrushIdCounter;
        private readonly long _id;

        /// <summary>Constructor of BaseGeoBrush</summary>
        /// <remarks>None</remarks>
        protected BaseGeoBrush()
        {
            _geoBrushIdCounter += 1;
            _id = _geoBrushIdCounter;
        }

        /// <summary>
        /// Id of BaseGeoBrush. Used as a key for cached brushes.
        /// </summary>
        public long Id
        {
            get { return _id; }
        }

        /// <summary>Creates a copy of BaseGeoBrush using the deep clone technique.</summary>
        /// <returns>Cloned BaseGeoBrush.</returns>
        /// <remarks>Deep cloning copies the cloned object as well as all the objects within it.</remarks>
        public BaseGeoBrush CloneDeep()
        {
            return CloneDeepCore();
        }

        /// <summary>Creates a copy of BaseGeoBrush using the deep clone technique. By default serialization is used.</summary>
        /// <returns>A cloned GeoBrush.</returns>
        /// <remarks>Deep cloning copies the cloned object as well as all the objects within it.</remarks>
        protected virtual BaseGeoBrush CloneDeepCore()
        {
            return (BaseGeoBrush) SerializeCloneDeep(this);
        }

        private static object SerializeCloneDeep(object instance)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, instance);
            stream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(stream);
        }
    }
}