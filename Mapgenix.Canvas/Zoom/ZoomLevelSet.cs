using System;
using System.Collections.ObjectModel;
using Mapgenix.Shapes;
using Mapgenix.Utils;


namespace Mapgenix.Canvas
{
    /// <summary>Collection of ZoomLevels.</summary>
    /// <remarks>Each ZoomLevel has a different scale. It stores Styles determining how the features draw.</remarks>
    [Serializable]
    public class ZoomLevelSet
    {
        private Collection<ZoomLevel> _customZoomLevels;
        private string _name;

        private ZoomLevel _zoomLevel01;
        private ZoomLevel _zoomLevel02;
        private ZoomLevel _zoomLevel03;
        private ZoomLevel _zoomLevel04;
        private ZoomLevel _zoomLevel05;
        private ZoomLevel _zoomLevel06;
        private ZoomLevel _zoomLevel07;
        private ZoomLevel _zoomLevel08;
        private ZoomLevel _zoomLevel09;
        private ZoomLevel _zoomLevel10;
        private ZoomLevel _zoomLevel11;
        private ZoomLevel _zoomLevel12;
        private ZoomLevel _zoomLevel13;
        private ZoomLevel _zoomLevel14;
        private ZoomLevel _zoomLevel15;
        private ZoomLevel _zoomLevel16;
        private ZoomLevel _zoomLevel17;
        private ZoomLevel _zoomLevel18;
        private ZoomLevel _zoomLevel19;
        private ZoomLevel _zoomLevel20;

        /// <summary>Gets and sets the name of the ZoomSet.</summary>
        /// <value>Name of the ZoomSet.</value>
        /// <remarks>Usefull for legend, for example.</remarks>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>Gets the custom zoom levels of the zoomLevelSet.</summary>
        /// <remarks>None</remarks>
        /// <value>Custom zoom levels of the zoomLevelSet.</value>
        public Collection<ZoomLevel> CustomZoomLevels
        {
            get
            {
                if (_customZoomLevels == null)
                {
                    _customZoomLevels = new Collection<ZoomLevel>();
                }
                return _customZoomLevels;
            }
        }

        /// <summary>Gets the ZoomLevel for Level01.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level01.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel01
        {
            get
            {
                if (_zoomLevel01 == null)
                {
                    _zoomLevel01 = new ZoomLevel(590591790);
                }
                return _zoomLevel01;
            }
        }

        /// <summary>Gets the ZoomLevel for Level02.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level012.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel02
        {
            get
            {
                if (_zoomLevel02 == null)
                {
                    _zoomLevel02 = new ZoomLevel(295295895);
                }
                return _zoomLevel02;
            }
        }

        /// <summary>Gets the ZoomLevel for Level03.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level03.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel03
        {
            get
            {
                if (_zoomLevel03 == null)
                {
                    _zoomLevel03 = new ZoomLevel(147647947.5);
                }
                return _zoomLevel03;
            }
        }

        /// <summary>Gets the ZoomLevel for Level04.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level04.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel04
        {
            get
            {
                if (_zoomLevel04 == null)
                {
                    _zoomLevel04 = new ZoomLevel(73823973.75);
                }
                return _zoomLevel04;
            }
        }

        /// <summary>Gets the ZoomLevel for Level05.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level05.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel05
        {
            get
            {
                if (_zoomLevel05 == null)
                {
                    _zoomLevel05 = new ZoomLevel(36911986.875);
                }
                return _zoomLevel05;
            }
        }


        /// <summary>Gets the ZoomLevel for Level06.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level06.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel06
        {
            get
            {
                if (_zoomLevel06 == null)
                {
                    _zoomLevel06 = new ZoomLevel(18455993.4375);
                }
                return _zoomLevel06;
            }
        }

        /// <summary>Gets the ZoomLevel for Level07.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level07.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel07
        {
            get
            {
                if (_zoomLevel07 == null)
                {
                    _zoomLevel07 = new ZoomLevel(9227996.71875);
                }
                return _zoomLevel07;
            }
        }

        /// <summary>Gets the ZoomLevel for Level08.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level08.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel08
        {
            get
            {
                if (_zoomLevel08 == null)
                {
                    _zoomLevel08 = new ZoomLevel(4613998.359375);
                }
                return _zoomLevel08;
            }
        }

        /// <summary>Gets the ZoomLevel for Level09.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level09.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel09
        {
            get
            {
                if (_zoomLevel09 == null)
                {
                    _zoomLevel09 = new ZoomLevel(2306999.1796875);
                }
                return _zoomLevel09;
            }
        }

        /// <summary>Gets the ZoomLevel for Level10.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level10.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel10
        {
            get
            {
                if (_zoomLevel10 == null)
                {
                    _zoomLevel10 = new ZoomLevel(1153499.58984375);
                }
                return _zoomLevel10;
            }
        }

        /// <summary>Gets the ZoomLevel for Level11.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level11.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel11
        {
            get
            {
                if (_zoomLevel11 == null)
                {
                    _zoomLevel11 = new ZoomLevel(576749.794921875);
                }
                return _zoomLevel11;
            }
        }

        /// <summary>Gets the ZoomLevel for Level12.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level12.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel12
        {
            get
            {
                if (_zoomLevel12 == null)
                {
                    _zoomLevel12 = new ZoomLevel(288374.8974609375);
                }
                return _zoomLevel12;
            }
        }

        /// <summary>Gets the ZoomLevel for Level13.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level13.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel13
        {
            get
            {
                if (_zoomLevel13 == null)
                {
                    _zoomLevel13 = new ZoomLevel(144187.44873046875);
                }
                return _zoomLevel13;
            }
        }

        /// <summary>Gets the ZoomLevel for Level14.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level14.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel14
        {
            get
            {
                if (_zoomLevel14 == null)
                {
                    _zoomLevel14 = new ZoomLevel(72093.724365234375);
                }
                return _zoomLevel14;
            }
        }

        /// <summary>Gets the ZoomLevel for Level15.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level15.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel15
        {
            get
            {
                if (_zoomLevel15 == null)
                {
                    _zoomLevel15 = new ZoomLevel(36046.862182617188);
                }
                return _zoomLevel15;
            }
        }

        /// <summary>Gets the ZoomLevel for Level16.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level16.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel16
        {
            get
            {
                if (_zoomLevel16 == null)
                {
                    _zoomLevel16 = new ZoomLevel(18023.431091308594);
                }
                return _zoomLevel16;
            }
        }

        /// <summary>Gets the ZoomLevel for Level17.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level17.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel17
        {
            get
            {
                if (_zoomLevel17 == null)
                {
                    _zoomLevel17 = new ZoomLevel(9011.7155456542969);
                }
                return _zoomLevel17;
            }
        }

        /// <summary>Gets the ZoomLevel for Level18.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level18.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel18
        {
            get
            {
                if (_zoomLevel18 == null)
                {
                    _zoomLevel18 = new ZoomLevel(4505.8577728271484);
                }
                return _zoomLevel18;
            }
        }

        /// <summary>Gets the ZoomLevel for Level19.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level19.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel19
        {
            get
            {
                if (_zoomLevel19 == null)
                {
                    _zoomLevel19 = new ZoomLevel(2252.9288864135742);
                }
                return _zoomLevel19;
            }
        }

        /// <summary>Gets the ZoomLevel for Level20.</summary>
        /// <decimalDegreesValue>ZoomLevel for Level20.</decimalDegreesValue>
        /// <remarks>Corresponds to the standard zoom levels of Google Maps from 01 (world level) to 20 (street level)</remarks>
        public ZoomLevel ZoomLevel20
        {
            get
            {
                if (_zoomLevel20 == null)
                {
                    _zoomLevel20 = new ZoomLevel(1126.4644432067871);
                }
                return _zoomLevel20;
            }
        }

        /// <summary>Returns the active ZoomLevel based on an extent, a map unit and a screen width.</summary>
        /// <returns>Active ZoomLevel based on an extent, a map unit and a screen width.</returns>
        /// <param name="extent">World extent of the map.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="mapUnit">Unit of the map.</param>
        public ZoomLevel GetZoomLevel(RectangleShape extent, double screenWidth, GeographyUnit mapUnit)
        {
            Validators.CheckParameterIsNotNull(extent, "extent");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "width", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");

            return GetZoomLevel(extent, screenWidth, mapUnit, 96.0f);
        }

        /// <summary>Returns the active ZoomLevel based on an extent, a map unit and a screen width.</summary>
        /// <returns>Active ZoomLevel based on an extent, a map unit and a screen width.</returns>
        /// <param name="extent">World extent of the map.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="mapUnit">Unit of the map.</param>
        /// <param name="dpi">Dpi (Dot per inch)</param>
        public ZoomLevel GetZoomLevel(RectangleShape extent, double screenWidth, GeographyUnit mapUnit, float dpi)
        {
            Validators.CheckParameterIsNotNull(extent, "extent");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "width", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");

            var nearestDistance = double.MaxValue;
            ZoomLevel nearestZoomLevel = null;
            var currentScale = ExtentHelper.GetScale(extent, (float) screenWidth, mapUnit, dpi);

            var zoolLevelCount = 20;
            if (CustomZoomLevels.Count > 0)
            {
                zoolLevelCount = CustomZoomLevels.Count;
            }

            ZoomLevel zoomLevel;
            for (var i = zoolLevelCount - 1; i >= 0; i--)
            {
                if (CustomZoomLevels.Count > 0)
                {
                    zoomLevel = CustomZoomLevels[i];
                }
                else
                {
                    zoomLevel = SelectZoomLevelById(i + 1);
                    ;
                }

                var subScale = Math.Abs(currentScale - zoomLevel.Scale);
                if (nearestDistance > subScale)
                {
                    nearestDistance = subScale;
                    nearestZoomLevel = zoomLevel;
                }
            }

            return nearestZoomLevel;
        }

        /// <summary>Returns the active ZoomLevel based on an extent, a map unit and a canvas width.</summary>
        /// <returns>Active ZoomLevel based on an extent, map unit and a canvas width.</returns>
        /// <param name="extent">World extent of the map.</param>
        /// <param name="screenWidth">Width of the canvas in pixels.</param>
        /// <param name="mapUnit">Unit of the map.</param>
        /// <param name="dpi">Dpi (Dot per inch).</param>
        public ZoomLevel GetZoomLevelForDrawing(RectangleShape extent, double screenWidth, GeographyUnit mapUnit,
            float dpi)
        {
            var isGetResult = false;
            var currentScale = ExtentHelper.GetScale(extent, (float) screenWidth, mapUnit, dpi);
            ZoomLevel validateZoomLevel = null;

            var nearestDistance = double.MaxValue;
            ZoomLevel nearestZoomLevel = null;

            var zoolLevelCount = 20;
            if (CustomZoomLevels.Count > 0)
            {
                zoolLevelCount = CustomZoomLevels.Count;
            }

            ZoomLevel zoomLevel;
            for (var i = zoolLevelCount - 1; i >= 0; i--)
            {
                if (CustomZoomLevels.Count > 0)
                {
                    zoomLevel = CustomZoomLevels[i];
                }
                else
                {
                    zoomLevel = SelectZoomLevelById(i + 1);
                    ;
                }

                if (zoomLevel.IsActive)
                {
                    if (zoomLevel.ApplyUntilZoomLevel != ApplyUntilZoomLevel.None)
                    {
                        var upperThreshold = zoomLevel.Scale;
                        var lowerThreshold = GetLowerThreshold(zoomLevel);

                        if (upperThreshold >= currentScale && lowerThreshold <= currentScale)
                        {
                            validateZoomLevel = zoomLevel;
                            isGetResult = true;
                            break;
                        }

                        var subScale = Math.Abs(currentScale - upperThreshold);
                        if (nearestDistance >= subScale)
                        {
                            nearestDistance = subScale;
                            nearestZoomLevel = zoomLevel;
                        }
                        subScale = Math.Abs(currentScale - lowerThreshold);
                        if (nearestDistance >= subScale)
                        {
                            nearestDistance = subScale;
                            nearestZoomLevel = zoomLevel;
                        }
                    }
                    else
                    {
                        var subScale = Math.Abs(currentScale - zoomLevel.Scale);
                        if (nearestDistance > subScale)
                        {
                            nearestDistance = subScale;
                            nearestZoomLevel = zoomLevel;
                        }
                    }
                }
            }

            if (!isGetResult)
            {
                validateZoomLevel = nearestZoomLevel;
            }

            if (validateZoomLevel.IsDefault)
            {
                validateZoomLevel = null;
            }

            return validateZoomLevel;
        }


        /// <summary>Returns the active ZoomLevel based on an extent, a map unit and a canvas width.</summary>
        /// <returns>Active ZoomLevel based on an extent, map unit and a canvas width.</returns>
        /// <param name="extent">World extent of the map.</param>
        /// <param name="screenWidth">Width of the canvas in pixels.</param>
        /// <param name="mapUnit">Unit of the map.</param>
        public ZoomLevel GetZoomLevelForDrawing(RectangleShape extent, double screenWidth, GeographyUnit mapUnit)
        {
            Validators.CheckParameterIsNotNull(extent, "extent");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "width", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");

            return GetZoomLevelForDrawing(extent, screenWidth, mapUnit, 96f);
        }

        /// <summary>Returns collection of zoomLevels in the zoomLevelSet.</summary>
        /// <returns>Collection of zoomlevels in the zoomLevelSet.</returns>
        /// <remarks>None.</remarks>
        public Collection<ZoomLevel> GetZoomLevels()
        {
            var zoomLevels = new Collection<ZoomLevel>();
            if (CustomZoomLevels.Count > 0)
            {
                zoomLevels = CustomZoomLevels;
            }
            else
            {
                zoomLevels.Add(ZoomLevel01);
                zoomLevels.Add(ZoomLevel02);
                zoomLevels.Add(ZoomLevel03);
                zoomLevels.Add(ZoomLevel04);
                zoomLevels.Add(ZoomLevel05);
                zoomLevels.Add(ZoomLevel06);
                zoomLevels.Add(ZoomLevel07);
                zoomLevels.Add(ZoomLevel08);
                zoomLevels.Add(ZoomLevel09);
                zoomLevels.Add(ZoomLevel10);
                zoomLevels.Add(ZoomLevel11);
                zoomLevels.Add(ZoomLevel12);
                zoomLevels.Add(ZoomLevel13);
                zoomLevels.Add(ZoomLevel14);
                zoomLevels.Add(ZoomLevel15);
                zoomLevels.Add(ZoomLevel16);
                zoomLevels.Add(ZoomLevel17);
                zoomLevels.Add(ZoomLevel18);
                zoomLevels.Add(ZoomLevel19);
                zoomLevels.Add(ZoomLevel20);
            }

            return zoomLevels;
        }

        /// <summary>Gets the lower zoom level based on a scale.</summary>
        /// <param name="currentScale">Current scale.</param>
        /// <param name="zoomLevelSet">Zoom level set</param>
        /// <returns></returns>
        public static double GetLowerZoomLevelScale(double currentScale, ZoomLevelSet zoomLevelSet)
        {
            Validators.CheckParameterIsNotNull(zoomLevelSet, "zoomLevelSet");

            var nextScale = currentScale;

            var zoomLevels = zoomLevelSet.GetZoomLevels();
            for (var i = 0; i < zoomLevels.Count; i++)
            {
                if (zoomLevels[i].Scale + 0.000001 < currentScale || i == zoomLevels.Count)
                {
                    nextScale = zoomLevels[i].Scale;
                    break;
                }

               
            }

            return nextScale;
        }

        /// <summary>Gets the higher zoom level based on a scale.</summary>
        /// <param name="currentScale">Current scale.</param>
        /// <param name="zoomLevelSet">Zoom level set</param>
        /// <returns></returns>
        public static double GetHigherZoomLevelScale(double currentScale, ZoomLevelSet zoomLevelSet)
        {
            Validators.CheckParameterIsNotNull(zoomLevelSet, "zoomLevelSet");

            var nextScale = currentScale;

            var zoomLevels = zoomLevelSet.GetZoomLevels();
            for (var i = zoomLevels.Count - 1; i >= 0; i--)
            {
                if (zoomLevels[i].Scale > currentScale + 0.000001 || i == 0)
                {
                    nextScale = zoomLevels[i].Scale;
                    break;
                }

            }

            return nextScale;
        }

        internal ZoomLevel GetSnapedZoomLevel(double scale)
        {
            var zoolLevelCount = 20;
            if (CustomZoomLevels.Count > 0)
            {
                zoolLevelCount = CustomZoomLevels.Count;
            }

            ZoomLevel returnZoomLevel = null;
            ZoomLevel tempZoomLevel;
            for (var i = zoolLevelCount - 1; i >= 0; i--)
            {
                if (CustomZoomLevels.Count > 0)
                {
                    tempZoomLevel = CustomZoomLevels[i];
                }
                else
                {
                    tempZoomLevel = SelectZoomLevelById(i + 1);
                    ;
                }

                
                if (scale <= tempZoomLevel.Scale || Math.Abs(tempZoomLevel.Scale - scale)/scale < 10e-6)
                {
                    returnZoomLevel = tempZoomLevel;
                    break;
                }

                if (i == 0)
                {
                    returnZoomLevel = tempZoomLevel;
                }
            }

            return returnZoomLevel;
        }

        private double GetLowerThreshold(ZoomLevel zoomLevel)
        {
            switch (zoomLevel.ApplyUntilZoomLevel)
            {
                case ApplyUntilZoomLevel.None:
                    return zoomLevel.Scale;
                case ApplyUntilZoomLevel.Level01:
                    return ZoomLevel01.Scale;
                case ApplyUntilZoomLevel.Level02:
                    return ZoomLevel02.Scale;
                case ApplyUntilZoomLevel.Level03:
                    return ZoomLevel03.Scale;
                case ApplyUntilZoomLevel.Level04:
                    return ZoomLevel04.Scale;
                case ApplyUntilZoomLevel.Level05:
                    return ZoomLevel05.Scale;
                case ApplyUntilZoomLevel.Level06:
                    return ZoomLevel06.Scale;
                case ApplyUntilZoomLevel.Level07:
                    return ZoomLevel07.Scale;
                case ApplyUntilZoomLevel.Level08:
                    return ZoomLevel08.Scale;
                case ApplyUntilZoomLevel.Level09:
                    return ZoomLevel09.Scale;
                case ApplyUntilZoomLevel.Level10:
                    return ZoomLevel10.Scale;
                case ApplyUntilZoomLevel.Level11:
                    return ZoomLevel11.Scale;
                case ApplyUntilZoomLevel.Level12:
                    return ZoomLevel12.Scale;
                case ApplyUntilZoomLevel.Level13:
                    return ZoomLevel13.Scale;
                case ApplyUntilZoomLevel.Level14:
                    return ZoomLevel14.Scale;
                case ApplyUntilZoomLevel.Level15:
                    return ZoomLevel15.Scale;
                case ApplyUntilZoomLevel.Level16:
                    return ZoomLevel16.Scale;
                case ApplyUntilZoomLevel.Level17:
                    return ZoomLevel17.Scale;
                case ApplyUntilZoomLevel.Level18:
                    return ZoomLevel18.Scale;
                case ApplyUntilZoomLevel.Level19:
                    return ZoomLevel19.Scale;
                case ApplyUntilZoomLevel.Level20:
                    return ZoomLevel20.Scale;
            }

            return zoomLevel.Scale;
        }

        private ZoomLevel SelectZoomLevelById(int id)
        {
            switch (id)
            {
                case 1:
                    return ZoomLevel01;
                case 2:
                    return ZoomLevel02;
                case 3:
                    return ZoomLevel03;
                case 4:
                    return ZoomLevel04;
                case 5:
                    return ZoomLevel05;
                case 6:
                    return ZoomLevel06;
                case 7:
                    return ZoomLevel07;
                case 8:
                    return ZoomLevel08;
                case 9:
                    return ZoomLevel09;
                case 10:
                    return ZoomLevel10;
                case 11:
                    return ZoomLevel11;
                case 12:
                    return ZoomLevel12;
                case 13:
                    return ZoomLevel13;
                case 14:
                    return ZoomLevel14;
                case 15:
                    return ZoomLevel15;
                case 16:
                    return ZoomLevel16;
                case 17:
                    return ZoomLevel17;
                case 18:
                    return ZoomLevel18;
                case 19:
                    return ZoomLevel19;
                case 20:
                    return ZoomLevel20;
                default:
                    return null;
            }
        }

        
    }
}