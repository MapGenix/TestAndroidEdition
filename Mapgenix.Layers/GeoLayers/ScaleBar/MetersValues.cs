using System;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    /// <summary>Meters values for scale bar.</summary>
    [Serializable]
    public struct MetersValues
    {
        private readonly double _v10;
        private readonly double _v100;
        private readonly double _v1000;
        private readonly double _v20;
        private readonly double _v200;
        private readonly double _v30;
        private readonly double _v300;
        private readonly double _v40;
        private readonly double _v400;
        private readonly double _v50;
        private readonly double _v500;

        public MetersValues(int width, double extentLengthBar, DistanceUnit distanceUnit)
        {
            this._v10 = (10 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Meter);
            this._v20 = (20 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Meter);
            this._v30 = (30 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Meter);
            this._v40 = (40 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Meter);
            this._v50 = (50 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Meter);
            this._v100 = (100 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Meter);
            this._v200 = (200 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Meter);
            this._v300 = (300 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Meter);
            this._v400 = (400 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Meter);
            this._v500 = (500 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Meter);
            this._v1000 = (1000 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Meter);
        }

        public double V10
        {
            get { return _v10; }
        }

        public double V20
        {
            get { return _v20; }
        }

        public double V30
        {
            get { return _v30; }
        }

        public double V40
        {
            get { return _v40; }
        }

        public double V50
        {
            get { return _v50; }
        }

        public double V100
        {
            get { return _v100; }
        }

        public double V200
        {
            get { return _v200; }
        }

        public double V300
        {
            get { return _v300; }
        }

        public double V400
        {
            get { return _v400; }
        }

        public double V500
        {
            get { return _v500; }
        }

        public double V1000
        {
            get { return _v1000; }
        }
    }
}