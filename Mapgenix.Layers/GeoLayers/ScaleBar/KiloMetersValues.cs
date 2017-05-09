﻿using System;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    /// <summary>Kilometers values for scale bar.</summary>
    [Serializable]
    public struct KiloMetersValues
    {
        private readonly double _v1;
        private readonly double _v10;
        private readonly double _v100;
        private readonly double _v1000;
        private readonly double _v2;
        private readonly double _v20;
        private readonly double _v200;
        private readonly double _v2000;
        private readonly double _v3;
        private readonly double _v30;
        private readonly double _v300;
        private readonly double _v4;
        private readonly double _v40;
        private readonly double _v400;
        private readonly double _v4000;
        private readonly double _v5;
        private readonly double _v50;
        private readonly double _v500;

        public KiloMetersValues(int width, double extentLengthBar, DistanceUnit distanceUnit)
        {
            this._v1 = (1 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v2 = (2 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v3 = (3 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v4 = (4 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v5 = (5 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v10 = (10 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v20 = (20 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v30 = (30 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v40 = (40 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v50 = (50 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v100 = (100 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v200 = (200 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v300 = (300 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v400 = (400 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v500 = (500 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v1000 = (1000 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v2000 = (2000 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
            this._v4000 = (4000 * width) / Conversion.ConvertMeasureUnits(extentLengthBar, distanceUnit, DistanceUnit.Kilometer);
        }

        public double V1
        {
            get { return _v1; }
        }

        public double V2
        {
            get { return _v2; }
        }

        public double V3
        {
            get { return _v3; }
        }

        public double V4
        {
            get { return _v4; }
        }

        public double V5
        {
            get { return _v5; }
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

        public double V2000
        {
            get { return _v2000; }
        }

        public double V4000
        {
            get { return _v4000; }
        }
    }
}