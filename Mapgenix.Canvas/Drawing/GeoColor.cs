using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Mapgenix.Canvas.Properties;
using Mapgenix.Utils;

namespace Mapgenix.Canvas
{
    /// <summary>Represents a color for drawing on a BaseGeoCanvas.</summary>
    [Serializable]
    public struct GeoColor
    {
        private const float MaxLuminance = 0.9f;
        private const float MinSaturation = 0.1f;
        private const float MaxHue = 360.0f;
        private const float MinHue = 0.0f;

        private byte _alpha;
        private readonly byte _red;
        private readonly byte _green;
        private readonly byte _blue;

        private readonly string _name;
        private readonly ColorType _colorType;
        private bool _isTransparent;

        private static readonly StandardColors _standardColors = new StandardColors();
       
        private static readonly Random Random = new Random(DateTime.Now.Millisecond);

        /// <summary>Constructor to create a new color.</summary>
        /// <overloads>Red, green, and blue values from 0 to 255.</overloads>
        /// <remarks>None</remarks>
        /// <param name="red">Red value of the color (from 0 to 255).</param>
        /// <param name="green">Green value of the color (from 0 to 255).</param>
        /// <param name="blue">Blue value of the color (from 0 to 255).</param>
        public GeoColor(int red, int green, int blue)
            : this(255, red, green, blue)
        {
          
        }

        /// <summary>Constructor to create a new color.</summary>
        /// <overloads>Red, green, and blue values from 0 to 255.</overloads>
        /// <remarks>None</remarks>
        /// <param name="alpha">Alpha or transparency value of the color (from 0 to 255). 0 is totally transparent. 255 is totally opaque</param>
        /// <param name="red">Red value of the color (from 0 to 255).</param>
        /// <param name="green">Green value of the color (from 0 to 255).</param>
        /// <param name="blue">Blue value of the color (from 0 to 255).</param>
        public GeoColor(int alpha, int red, int green, int blue)
            : this("", ColorType.NonStandardColor, alpha, red, green, blue)
        {
        }

        /// <overloads>Creates a transparent version of a color.</overloads>
        /// <summary>Creates a color.</summary>
        /// <returns>None</returns>
        /// <remarks>Use this overload to create a transparent version of a color. </remarks>
        /// <param name="alpha">Alpha or transparency value from 0 to 255. 0 is totally transparent. 255 is totally opaque</param>
        /// <param name="color">This parameter specifies the base color.</param>
        public GeoColor(int alpha, GeoColor color) :
            this(alpha, color.RedComponent, color.GreenComponent, color.BlueComponent)
        {
        }

        internal GeoColor(string name, ColorType colorType, int alpha, int red, int green, int blue)
        {
            Validators.CheckIfInputValueIsInRange(alpha, "alpha", 0, RangeCheckingInclusion.IncludeValue, 255,
                RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(red, "red", 0, RangeCheckingInclusion.IncludeValue, 255,
                RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(green, "green", 0, RangeCheckingInclusion.IncludeValue, 255,
                RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(blue, "blue", 0, RangeCheckingInclusion.IncludeValue, 255,
                RangeCheckingInclusion.IncludeValue);

            _alpha = (byte) alpha;
            _red = (byte) red;
            _green = (byte) green;
            _blue = (byte) blue;
            _colorType = colorType;

            if (alpha == 0)
            {
                _isTransparent = true;
            }
            else
            {
                _isTransparent = false;
            }

            if (colorType == ColorType.StandardColor)
            {
                _name = name;
            }
            else
            {
                _name = alpha.ToString("X2", null) + red.ToString("X2", null) + green.ToString("X2", null) +
                        blue.ToString("X2", null);
            }
        }

        /// <summary>Checks if GeoColor is totally transparent (alpha value = 0).
        /// </summary>
        public bool IsTransparent
        {
            get
            {
                if (_alpha == 0)
                {
                    _isTransparent = true;
                }
                else
                {
                    _isTransparent = false;
                }
                return _isTransparent;
            }
        }

        /// <summary>Gets alpha value of the GeoColor.</summary>
        public byte AlphaComponent
        {
            get { return _alpha; }
        }

        /// <summary>Gets red value of the GeoColor.</summary>
        public byte RedComponent
        {
            get { return _red; }
        }

        /// <summary>Gets green value of the GeoColor.</summary>
        public byte GreenComponent
        {
            get { return _green; }
        }

        /// <summary>Gets blue value of the GeoColor.</summary>
        public byte BlueComponent
        {
            get { return _blue; }
        }

        /// <summary>Gets the hue value of the GeoColor.</summary>
        public float Hue
        {
            get
            {
                float hue, saturation, luminance;
                RgbToHsl(this, out hue, out saturation, out luminance);

                return hue;
            }
        }

        /// <summary>Gets the saturation value of the GeoColor.</summary>
        public float Saturation
        {
            get
            {
                float hue, saturation, luminance;
                RgbToHsl(this, out hue, out saturation, out luminance);

                return saturation;
            }
        }

        /// <summary>Gets the luminance value of the GeoColor.</summary>
        public float Luminance
        {
            get
            {
                float hue, saturation, luminance;
                RgbToHsl(this, out hue, out saturation, out luminance);

                return luminance;
            }
        }

        /// <summary>Returns a list of commonly-used predefined colors.</summary>
        public static StandardColors StandardColors
        {
            get { return _standardColors; }
        }

        /// <summary>Returns a collection of GeoColors based on a hue of a color.</summary>
        /// <remarks>To get a number of colors with the same hue. For example, all the reds such as light red, dark red, pastel red etc.</remarks>
        /// <returns>Collection of GeoColors based on the same hue.</returns>
        /// <param name="baseColor">Color to base the hue from.</param>
        /// <param name="numbersOfColors">Number of colors to return.</param>
        public static Collection<GeoColor> GetColorsInHueFamily(GeoColor baseColor, int numbersOfColors)
        {
            Validators.CheckIfInputValueIsInRange(numbersOfColors, "numbersOfColors", 1, 255);

            var returnGeoColors = new Collection<GeoColor>();

            float hue, saturation, luminance;
            RgbToHsl(baseColor, out hue, out saturation, out luminance);

            var saturationRatio = (saturation - MinSaturation)/(numbersOfColors - 1);
            var luminanceRatio = (MaxLuminance - luminance)/(numbersOfColors - 1);

            returnGeoColors.Add(baseColor);
            for (var i = 1; i < numbersOfColors; i++)
            {
                saturation = DecreseSaturation(saturation, saturationRatio);
                luminance = IncreseLuminance(luminance, luminanceRatio);
                var geoColor = HslToRgb(hue, saturation, luminance);
                geoColor._alpha = baseColor.AlphaComponent;
                returnGeoColors.Add(geoColor);
            }

            return returnGeoColors;
        }

        private static float DecreseSaturation(float saturation, float ratio)
        {
            var rtnSaturation = saturation - ratio;
            if (rtnSaturation < MinSaturation)
            {
                rtnSaturation = MinSaturation;
            }

            return rtnSaturation;
        }

        private static float IncreseLuminance(float luminance, float ratio)
        {
            var rtnLuminance = luminance + ratio;
            if (rtnLuminance > MaxLuminance)
            {
                rtnLuminance = MaxLuminance;
            }
            return rtnLuminance;
        }

        /// <summary>Returns a collection of GeoColors based on the quality (luminosity and saturation) of a reference color.</summary>
        /// <returns>Collection of GeoColors based on the quality (luminosity and saturation) of a reference color. </returns>
        /// <remarks>Useful to get different colors with the same quality. 
        /// For example, with a dark red as reference color, it gets dark blue, dark green, etc.</remarks>
        /// <param name="baseColor">Color to base the quality from.</param>
        /// <param name="numberOfColors">Number of colors to return.</param>
        public static Collection<GeoColor> GetColorsInQualityFamily(GeoColor baseColor, int numberOfColors)
        {
            Validators.CheckIfInputValueIsInRange(numberOfColors, "numberOfColors", 1, 255);

            var returnGeoColors = new Collection<GeoColor>();

            float hue, luminance, saturation;
            RgbToHsl(baseColor, out hue, out saturation, out luminance);

            var hueRatio = (MaxHue - MinHue)/(numberOfColors);

            returnGeoColors.Add(baseColor);
            for (var i = 1; i < numberOfColors; i++)
            {
                hue = IncreseHue(hue, hueRatio);
                var geoColor = HslToRgb(hue, saturation, luminance);
                geoColor._alpha = baseColor._alpha;
                returnGeoColors.Add(geoColor);
            }

            return returnGeoColors;
        }

        /// <summary>Returns a collection of GeoColors based on the quality (luminosity and saturation) of two reference colors.</summary>
        /// <returns>Collection of GeoColors based on the quality (luminosity and saturation) of two reference colors. </returns>
        /// <remarks>Useful to get different colors with the same quality. 
        /// For example, with a dark red as reference color, it gets dark blue, dark green, etc.</remarks>
        /// <param name="fromColor">First color to base the quality from.</param>
        /// <param name="toColor">Second color to base the quality from.</param>
        /// <param name="numberOfColors">Number of colors to return.</param>
        public static Collection<GeoColor> GetColorsInQualityFamily(GeoColor fromColor, GeoColor toColor,
            int numberOfColors, ColorWheelDirection colorWheelDirection)
        {
            Validators.CheckIfInputValueIsInRange(numberOfColors, "numberOfColors", 1, 255);

            float fromHue, fromLuminance, fromSaturation;
            RgbToHsl(fromColor, out fromHue, out fromSaturation, out fromLuminance);

            float toHue, toLuminance, toSaturation;
            RgbToHsl(toColor, out toHue, out toSaturation, out toLuminance);

            float hueRatio;
            if (colorWheelDirection == ColorWheelDirection.Clockwise)
            {
                hueRatio = (toHue - fromHue - MaxHue)/numberOfColors;
            }
            else
            {
                hueRatio = (toHue - fromHue)/numberOfColors;
            }

            var returnGeoColors = new Collection<GeoColor>();
            for (var i = 1; i < numberOfColors + 1; i++)
            {
                var geoColor = HslToRgb(fromHue + i*hueRatio, fromSaturation, fromLuminance);
                geoColor._alpha = fromColor._alpha;
                returnGeoColors.Add(geoColor);
            }

            return returnGeoColors;
        }

        private static float IncreseHue(float hue, float ratio)
        {
            var rtnHue = hue + ratio;
            if (rtnHue > MaxHue)
            {
                rtnHue = rtnHue - MaxHue;
            }
            return rtnHue;
        }

        /// <summary>Returns a GeoColor based on the Alpha, Red, Green, and Blue values.</summary>
        /// <returns>GeoColor based on the Alpha, Red, Green, and Blue values.</returns>
        /// <remarks>None</remarks>
        /// <param name="alpha">Alpha, or transparent value of the color.</param>
        /// <param name="red">Red value of the color.</param>
        /// <param name="green">Green value of the color.</param>
        /// <param name="blue">Blue value of the color.</param>
        public static GeoColor FromArgb(int alpha, int red, int green, int blue)
        {
            Validators.CheckIfInputValueIsInRange(alpha, "alpha", 0, RangeCheckingInclusion.IncludeValue, 255,
                RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(red, "red", 0, RangeCheckingInclusion.IncludeValue, 255,
                RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(green, "green", 0, RangeCheckingInclusion.IncludeValue, 255,
                RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(blue, "blue", 0, RangeCheckingInclusion.IncludeValue, 255,
                RangeCheckingInclusion.IncludeValue);

            return new GeoColor(alpha, red, green, blue);
        }

        /// <summary>Returns a transparent version of another color.</summary>
        /// <returns>Transparent version of another color.</returns>
        /// <param name="alpha">Transparent value for the color from 0 to 255. 
        /// 0 = fully transparent. 255 = fully opaque.</param>
        /// <param name="baseColor">Color to apply the transparency to.</param>
        public static GeoColor FromArgb(int alpha, GeoColor baseColor)
        {
            Validators.CheckIfInputValueIsInRange(alpha, "alpha", 0, RangeCheckingInclusion.IncludeValue, 255,
                RangeCheckingInclusion.IncludeValue);

            return new GeoColor(alpha, baseColor._red, baseColor._green, baseColor._blue);
        }

        /// <summary>Returns a GeoColor based on the Alpha, Hue, Saturation, and Luminosity values.</summary>
        /// <remarks>None</remarks>
        /// <param name="alpha">Alpha, or transparent, value of the color.</param>
        /// <param name="hue">Hue value of the color.</param>
        /// <param name="saturation">Saturation value of the color.</param>
        /// <param name="luminance">Luminance value of the color.</param>
        public static GeoColor FromAhsl(int alpha, float hue, float saturation, float luminance)
        {
            Validators.CheckIfInputValueIsInRange(alpha, "alpha", 0, RangeCheckingInclusion.IncludeValue, 255,
                RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(hue, "hue", 0, RangeCheckingInclusion.IncludeValue, 360,
                RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(saturation, "saturation", 0, RangeCheckingInclusion.IncludeValue, 100,
                RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(luminance, "luminance", 0, RangeCheckingInclusion.IncludeValue, 100,
                RangeCheckingInclusion.IncludeValue);

            return HslToRgb(hue, saturation, luminance);
        }

        /// <summary>Returns a GeoColor from an OLE color.</summary>
        /// <returns>GeoColor from an OLE color.</returns>
        /// <remarks>None</remarks>
        /// <param name="oleColor">OLE color to convert to GeoColor.</param>
        public static GeoColor FromOle(int oleColor)
        {
            if (((oleColor & unchecked((int) 0xff000000L)) == -2147483648) && ((oleColor & 0xffffff) <= 0x18))
            {
                #region StandardColors switch

                switch (oleColor)
                {
                    case -2147483648:
                        return _standardColors.ScrollBar;
                    case -2147483647:
                        return _standardColors.Desktop;
                    case -2147483646:
                        return _standardColors.ActiveCaption;
                    case -2147483645:
                        return _standardColors.InactiveCaption;
                    case -2147483644:
                        return _standardColors.Menu;
                    case -2147483643:
                        return _standardColors.Window;
                    case -2147483642:
                        return _standardColors.WindowFrame;
                    case -2147483641:
                        return _standardColors.MenuText;
                    case -2147483640:
                        return _standardColors.WindowText;
                    case -2147483639:
                        return _standardColors.ActiveCaptionText;
                    case -2147483638:
                        return _standardColors.ActiveBorder;
                    case -2147483637:
                        return _standardColors.InactiveBorder;
                    case -2147483636:
                        return _standardColors.AppWorkspace;
                    case -2147483635:
                        return _standardColors.Highlight;
                    case -2147483634:
                        return _standardColors.HighlightText;
                    case -2147483633:
                        return _standardColors.Control;
                    case -2147483632:
                        return _standardColors.ControlDark;
                    case -2147483631:
                        return _standardColors.GrayText;
                    case -2147483630:
                        return _standardColors.ControlText;
                    case -2147483629:
                        return _standardColors.InactiveCaptionText;
                    case -2147483628:
                        return _standardColors.ControlLightLight;
                    case -2147483627:
                        return _standardColors.ControlDarkDark;
                    case -2147483626:
                        return _standardColors.ControlLight;
                    case -2147483625:
                        return _standardColors.InfoText;
                    case -2147483624:
                        return _standardColors.Info;
                    case -2147483621:
                        return _standardColors.GradientActiveCaption;
                    case -2147483620:
                        return _standardColors.GradientInactiveCaption;
                    case -2147483619:
                        return _standardColors.MenuHighlight;
                    case -2147483618:
                        return _standardColors.MenuBar;
                }

                #endregion
            }
            var r = Convert.ToByte(oleColor & 0xff);
            var g = Convert.ToByte((oleColor >> 8) & 0xff);
            var b = Convert.ToByte((oleColor >> 0x10) & 0xff);

            return new GeoColor(r, g, b);
        }

        /// <summary>Returns a GeoColor from an HTML color (either in hexadecimal or a named color).</summary>
        /// <returns>GeoColor from an HTML color (either in hexadecimal or a named color).</returns>
        /// <remarks>None</remarks>
        /// <param name="htmlColor">HTML color to convert to GeoColor.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"),
         SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static GeoColor FromHtml(string htmlColor)
        {
            Validators.CheckHtmlColorIsValid(htmlColor, "htmlColor");

            if ((htmlColor[0] == '#') && ((htmlColor.Length == 7) || (htmlColor.Length == 4)))
            {
                byte r, g, b;
                if (htmlColor.Length == 7)
                {
                    r = Convert.ToByte(htmlColor.Substring(1, 2), 0x10);
                    g = Convert.ToByte(htmlColor.Substring(3, 2), 0x10);
                    b = Convert.ToByte(htmlColor.Substring(5, 2), 0x10);
                }
                else
                {
                    var str = char.ToString(htmlColor[1]);
                    var str2 = char.ToString(htmlColor[2]);
                    var str3 = char.ToString(htmlColor[3]);

                    r = Convert.ToByte(str + str, 0x10);
                    g = Convert.ToByte(str2 + str2, 0x10);
                    b = Convert.ToByte(str3 + str3, 0x10);
                }

                return new GeoColor(255, r, g, b);
            }

            htmlColor = ConvertSomeHtmlColor(htmlColor);
            return GetStandardColorFromHtml(htmlColor);
        }

        private static string ConvertSomeHtmlColor(string htmlColor)
        {
            switch (htmlColor)
            {
                case "captiontext":
                    htmlColor = "ActiveCaptionText";
                    break;
                case "buttonshadow":
                    htmlColor = "ControlDark";
                    break;
                case "threeddarkshadow":
                    htmlColor = "ControlDarkDark";
                    break;
                case "buttontext":
                    htmlColor = "ControlText";
                    break;
                case "background":
                    htmlColor = "Desktop";
                    break;
                case "infobackground":
                    htmlColor = "Info";
                    break;
            }
            return htmlColor;
        }

        private static GeoColor GetStandardColorFromHtml(string htmlColor)
        {
            switch (htmlColor.ToLowerInvariant())
            {
                case "activeborder":
                    return _standardColors.ActiveBorder;
                case "activecaption":
                    return _standardColors.ActiveCaption;
                case "activecaptiontext":
                    return _standardColors.ActiveCaptionText;
                case "appworkspace":
                    return _standardColors.AppWorkspace;
                case "control":
                    return _standardColors.Control;
                case "controldark":
                    return _standardColors.ControlDark;
                case "controldarkdark":
                    return _standardColors.ControlDarkDark;
                case "controllight":
                    return _standardColors.ControlLight;
                case "controllightlight":
                    return _standardColors.ControlLightLight;
                case "controltext":
                    return _standardColors.ControlText;
                case "desktop":
                    return _standardColors.Desktop;
                case "graytext":
                    return _standardColors.GrayText;
                case "highlight":
                    return _standardColors.Highlight;
                case "highlighttext":
                    return _standardColors.HighlightText;
                case "hottrack":
                    return _standardColors.HotTrack;
                case "inactiveborder":
                    return _standardColors.InactiveBorder;
                case "inactivecaption":
                    return _standardColors.InactiveCaption;
                case "inactivecaptiontext":
                    return _standardColors.InactiveCaptionText;
                case "info":
                    return _standardColors.Info;
                case "infotext":
                    return _standardColors.InfoText;
                case "menu":
                    return _standardColors.Menu;
                case "menutext":
                    return _standardColors.MenuText;
                case "scrollbar":
                    return _standardColors.ScrollBar;
                case "window":
                    return _standardColors.Window;
                case "windowframe":
                    return _standardColors.WindowFrame;
                case "windowtext":
                    return _standardColors.WindowText;
                case "transparent":
                    return _standardColors.Transparent;
                case "aliceblue":
                    return _standardColors.AliceBlue;
                case "antiquewhite":
                    return _standardColors.AntiqueWhite;
                case "aqua":
                    return _standardColors.Aqua;
                case "aquamarine":
                    return _standardColors.Aquamarine;
                case "azure":
                    return _standardColors.Azure;
                case "beige":
                    return _standardColors.Beige;
                case "bisque":
                    return _standardColors.Bisque;
                case "black":
                    return _standardColors.Black;
                case "blanchedalmond":
                    return _standardColors.BlanchedAlmond;
                case "blue":
                    return _standardColors.Blue;
                case "blueviolet":
                    return _standardColors.BlueViolet;
                case "brown":
                    return _standardColors.Brown;
                case "burlywood":
                    return _standardColors.BurlyWood;
                case "cadetblue":
                    return _standardColors.CadetBlue;
                case "chartreuse":
                    return _standardColors.Chartreuse;
                case "chocolate":
                    return _standardColors.Chocolate;
                case "coral":
                    return _standardColors.Coral;
                case "cornflowerblue":
                    return _standardColors.CornflowerBlue;
                case "cornsilk":
                    return _standardColors.Cornsilk;
                case "crimson":
                    return _standardColors.Crimson;
                case "cyan":
                    return _standardColors.Cyan;
                case "darkblue":
                    return _standardColors.DarkBlue;
                case "darkcyan":
                    return _standardColors.DarkCyan;
                case "darkgoldenrod":
                    return _standardColors.DarkGoldenrod;
                case "darkgray":
                    return _standardColors.DarkGray;
                case "darkgreen":
                    return _standardColors.DarkGreen;
                case "darkkhaki":
                    return _standardColors.DarkKhaki;
                case "darkmagenta":
                    return _standardColors.DarkMagenta;
                case "darkolivegreen":
                    return _standardColors.DarkOliveGreen;
                case "darkorange":
                    return _standardColors.DarkOrange;
                case "darkorchid":
                    return _standardColors.DarkOrchid;
                case "darkred":
                    return _standardColors.DarkRed;
                case "darksalmon":
                    return _standardColors.DarkSalmon;
                case "darkseagreen":
                    return _standardColors.DarkSeaGreen;
                case "darkslateblue":
                    return _standardColors.DarkSlateBlue;
                case "darkslategray":
                    return _standardColors.DarkSlateGray;
                case "darkturquoise":
                    return _standardColors.DarkTurquoise;
                case "darkviolet":
                    return _standardColors.DarkViolet;
                case "deeppink":
                    return _standardColors.DeepPink;
                case "deepskyblue":
                    return _standardColors.DeepSkyBlue;
                case "dimgray":
                    return _standardColors.DimGray;
                case "dodgerblue":
                    return _standardColors.DodgerBlue;
                case "firebrick":
                    return _standardColors.Firebrick;
                case "floralwhite":
                    return _standardColors.FloralWhite;
                case "forestgreen":
                    return _standardColors.ForestGreen;
                case "fuchsia":
                    return _standardColors.Fuchsia;
                case "gainsboro":
                    return _standardColors.Gainsboro;
                case "ghostwhite":
                    return _standardColors.GhostWhite;
                case "gold":
                    return _standardColors.Gold;
                case "goldenrod":
                    return _standardColors.Goldenrod;
                case "gray":
                    return _standardColors.Gray;
                case "green":
                    return _standardColors.Green;
                case "greenyellow":
                    return _standardColors.GreenYellow;
                case "honeydew":
                    return _standardColors.Honeydew;
                case "hotpink":
                    return _standardColors.HotPink;
                case "indianred":
                    return _standardColors.IndianRed;
                case "indigo":
                    return _standardColors.Indigo;
                case "ivory":
                    return _standardColors.Ivory;
                case "khaki":
                    return _standardColors.Khaki;
                case "lavender":
                    return _standardColors.Lavender;
                case "lavenderblush":
                    return _standardColors.LavenderBlush;
                case "lawngreen":
                    return _standardColors.LawnGreen;
                case "lemonchiffon":
                    return _standardColors.LemonChiffon;
                case "lightblue":
                    return _standardColors.LightBlue;
                case "lightcoral":
                    return _standardColors.LightCoral;
                case "lightcyan":
                    return _standardColors.LightCyan;
                case "lightgoldenrodyellow":
                    return _standardColors.LightGoldenrodYellow;
                case "lightgrey":
                case "lightgray":
                    return _standardColors.LightGray;
                case "lightgreen":
                    return _standardColors.LightGreen;
                case "lightpink":
                    return _standardColors.LightPink;
                case "lightsalmon":
                    return _standardColors.LightSalmon;
                case "lightseagreen":
                    return _standardColors.LightSeaGreen;
                case "lightskyblue":
                    return _standardColors.LightSkyBlue;
                case "lightslategray":
                    return _standardColors.LightSlateGray;
                case "lightsteelblue":
                    return _standardColors.LightSteelBlue;
                case "lightyellow":
                    return _standardColors.LightYellow;
                case "lime":
                    return _standardColors.Lime;
                case "limegreen":
                    return _standardColors.LimeGreen;
                case "linen":
                    return _standardColors.Linen;
                case "magenta":
                    return _standardColors.Magenta;
                case "maroon":
                    return _standardColors.Maroon;
                case "mediumaquamarine":
                    return _standardColors.MediumAquamarine;
                case "mediumblue":
                    return _standardColors.MediumBlue;
                case "mediumorchid":
                    return _standardColors.MediumOrchid;
                case "mediumpurple":
                    return _standardColors.MediumPurple;
                case "mediumseagreen":
                    return _standardColors.MediumSeaGreen;
                case "mediumslateblue":
                    return _standardColors.MediumSlateBlue;
                case "mediumspringgreen":
                    return _standardColors.MediumSpringGreen;
                case "mediumturquoise":
                    return _standardColors.MediumTurquoise;
                case "mediumvioletred":
                    return _standardColors.MediumVioletRed;
                case "midnightblue":
                    return _standardColors.MidnightBlue;
                case "mintcream":
                    return _standardColors.MintCream;
                case "mistyrose":
                    return _standardColors.MistyRose;
                case "moccasin":
                    return _standardColors.Moccasin;
                case "navajowhite":
                    return _standardColors.NavajoWhite;
                case "navy":
                    return _standardColors.Navy;
                case "oldlace":
                    return _standardColors.OldLace;
                case "olive":
                    return _standardColors.Olive;
                case "olivedrab":
                    return _standardColors.OliveDrab;
                case "orange":
                    return _standardColors.Orange;
                case "orangered":
                    return _standardColors.OrangeRed;
                case "orchid":
                    return _standardColors.Orchid;
                case "palegoldenrod":
                    return _standardColors.PaleGoldenrod;
                case "palegreen":
                    return _standardColors.PaleGreen;
                case "paleturquoise":
                    return _standardColors.PaleTurquoise;
                case "palevioletred":
                    return _standardColors.PaleVioletRed;
                case "papayawhip":
                    return _standardColors.PapayaWhip;
                case "peachpuff":
                    return _standardColors.PeachPuff;
                case "peru":
                    return _standardColors.Peru;
                case "pink":
                    return _standardColors.Pink;
                case "plum":
                    return _standardColors.Plum;
                case "powderblue":
                    return _standardColors.PowderBlue;
                case "purple":
                    return _standardColors.Purple;
                case "red":
                    return _standardColors.Red;
                case "rosybrown":
                    return _standardColors.RosyBrown;
                case "royalblue":
                    return _standardColors.RoyalBlue;
                case "saddlebrown":

                    return _standardColors.SaddleBrown;
                case "salmon":
                    return _standardColors.Salmon;
                case "sandybrown":
                    return _standardColors.SandyBrown;
                case "seagreen":
                    return _standardColors.SeaGreen;
                case "seashell":
                    return _standardColors.SeaShell;
                case "sienna":
                    return _standardColors.Sienna;
                case "silver":
                    return _standardColors.Silver;
                case "skyblue":
                    return _standardColors.SkyBlue;
                case "slateblue":
                    return _standardColors.SlateBlue;
                case "slategray":
                    return _standardColors.SlateGray;
                case "snow":
                    return _standardColors.Snow;
                case "springgreen":
                    return _standardColors.SpringGreen;
                case "steelblue":
                    return _standardColors.SteelBlue;
                case "tan":
                    return _standardColors.Tan;
                case "teal":
                    return _standardColors.Teal;
                case "thistle":
                    return _standardColors.Thistle;
                case "tomato":
                    return _standardColors.Tomato;
                case "turquoise":
                    return _standardColors.Turquoise;
                case "violet":
                    return _standardColors.Violet;
                case "wheat":
                    return _standardColors.Wheat;
                case "white":
                    return _standardColors.White;
                case "whitesmoke":
                    return _standardColors.WhiteSmoke;
                case "yellow":
                    return _standardColors.Yellow;
                case "yellowgreen":
                    return _standardColors.YellowGreen;
                case "buttonface":
                    return _standardColors.ButtonFace;
                case "buttonhighlight":
                    return _standardColors.ButtonHighlight;
                case "buttonshadow":
                    return _standardColors.ButtonShadow;
                case "gradientactivecaption":
                    return _standardColors.GradientActiveCaption;
                case "gradientinactivecaption":
                    return _standardColors.GradientInactiveCaption;
                case "menubar":
                    return _standardColors.MenuBar;
                case "menuhighlight":
                    return _standardColors.MenuHighlight;
                default:
                    throw new ArgumentOutOfRangeException(ExceptionDescription.ColorDoesNotExist, "htmlColor");
            }
        }

        
        public static GeoColor GetRandomGeoColor(RandomColorType colorType)
        {
            Validators.CheckRandomColorTypeIsValid(colorType, "colorType");

            return GetRandomGeoColor(255, colorType);
        }

        /// <summary>Creates a random <strong>GeoColor</strong> structure</summary>
        /// <overloads>Creates a <strong>GeoColor</strong> structure with random color</overloads>
        /// <returns>A <strong>GeoColor</strong> structure.</returns>
        /// <param name="alpha"><para>The alpha component. Valid values are 0 to 255.</para></param>
        /// <param name="colorType">A <strong>ColorType</strong> defining the type of color.</param>
        public static GeoColor GetRandomGeoColor(int alpha, RandomColorType colorType)
        {
            Validators.CheckRandomColorTypeIsValid(colorType, "colorType");
            Validators.CheckNumberIsByte(alpha, "alpha");

            var number = Random.Next(0, 10);
            var returnColor = new GeoColor();

            var colors = new GeoColor[10];
            switch (colorType)
            {
                case RandomColorType.All:
                    var r = Random.Next(0, 255);
                    var g = Random.Next(0, 255);
                    var b = Random.Next(0, 255);
                    returnColor = FromArgb(255, r, g, b);
                    break;
                case RandomColorType.Bright:
                    colors[0] = StandardColors.Blue;
                    colors[1] = StandardColors.Green;
                    colors[2] = StandardColors.Red;
                    colors[3] = StandardColors.Orange;
                    colors[4] = FromArgb(255, 255, 0, 255);
                    colors[5] = FromArgb(255, 50, 205, 50);
                    colors[6] = FromArgb(255, 165, 42, 42);
                    colors[7] = StandardColors.Yellow;
                    colors[8] = FromArgb(255, 128, 0, 128);
                    colors[9] = FromArgb(255, 128, 0, 0);
                    returnColor = colors[number];
                    break;
                case RandomColorType.Pastel:
                    colors[0] = StandardColors.LightBlue;
                    colors[1] = StandardColors.LightCoral;
                    colors[2] = StandardColors.LightCyan;
                    colors[3] = StandardColors.LightGreen;
                    colors[4] = StandardColors.LightGoldenrodYellow;
                    colors[5] = StandardColors.LightSkyBlue;
                    colors[6] = StandardColors.LightSteelBlue;
                    colors[7] = StandardColors.LightPink;
                    colors[8] = StandardColors.LightSalmon;
                    colors[9] = StandardColors.LightSeaGreen;
                    returnColor = colors[number];
                    break;
                default:
                    break;
            }
            returnColor = new GeoColor(alpha, returnColor);

            return returnColor;
        }

        /// <summary>Returns a GeoColor from a Win32 color.</summary>
        /// <returns>GeoColor from a Win32 color.</returns>
        /// <remarks>None</remarks>
        /// <param name="win32Color">Win32 color to convert to GeoColor.</param>
        public static GeoColor FromWin32(int win32Color)
        {
            return FromOle(win32Color);
        }

        /// <summary>Returns an OLE color from a GeoColor.</summary>
        /// <remarks>None</remarks>
        /// <returns>OLE color from a GeoColor.</returns>
        /// <param name="color">GeoColor to convert to GeoColor.</param>
        public static int ToOle(GeoColor color)
        {
            Validators.CheckParameterIsNotNull(color, "drawingColor");

            if (color._colorType == ColorType.StandardColor)
            {
                #region StandardColors switch

                switch (color._name.ToLowerInvariant())
                {
                    case "activeborder":
                        return -2147483638;
                    case "activecaption":
                        return -2147483646;
                    case "activecaptiontext":
                        return -2147483639;
                    case "appworkspace":
                        return -2147483636;
                    case "control":
                        return -2147483633;
                    case "controldark":
                        return -2147483632;
                    case "controldarkdark":
                        return -2147483627;
                    case "controllight":
                        return -2147483626;
                    case "controllightlight":
                        return -2147483628;
                    case "controltext":
                        return -2147483630;
                    case "desktop":
                        return -2147483647;
                    case "graytext":
                        return -2147483631;
                    case "highlight":
                        return -2147483635;
                    case "highlighttext":
                        return -2147483634;
                    case "hottrack":
                        return -2147483635;
                    case "inactiveborder":
                        return -2147483637;
                    case "inactivecaption":
                        return -2147483645;
                    case "inactivecaptiontext":
                        return -2147483629;
                    case "info":
                        return -2147483624;
                    case "infotext":
                        return -2147483625;
                    case "menu":
                        return -2147483644;
                    case "menutext":
                        return -2147483641;
                    case "scrollbar":
                        return -2147483648;
                    case "window":
                        return -2147483643;
                    case "windowframe":
                        return -2147483642;
                    case "windowtext":
                        return -2147483640;
                    case "buttonface":
                        return -2147483633;
                    case "buttonhighlight":
                        return -2147483628;
                    case "buttonshadow":
                        return -2147483632;
                    case "gradientactivecaption":
                        return -2147483621;
                    case "gradientinactivecaption":
                        return -2147483620;
                    case "menubar":
                        return -2147483618;
                    case "menuhighlight":
                        return -2147483619;
                }

                #endregion
            }

            return ToWin32(color);
        }

        /// <summary>Returns an HTML color from a GeoColor.</summary>
        /// <returns>HTML color from a GeoColor.</returns>
        /// <param name="color">GeoColor to convert to GoeColor.</param>
        public static string ToHtml(GeoColor color)
        {
            Validators.CheckParameterIsNotNull(color, "drawingColor");

            if (color._colorType == ColorType.StandardColor)
            {
                #region standardColors switch

                switch (color._name)
                {
                    case "ActiveBorder":
                        return "activeborder";
                    case "ActiveCaption":
                    case "GradientActiveCaption":
                        return "activecaption";
                    case "ActiveCaptionText":
                        return "captiontext";
                    case "AppWorkspace":
                        return "appworkspace";
                    case "Control":
                        return "buttonface";
                    case "ControlDark":
                        return "buttonshadow";
                    case "ControlDarkDark":
                        return "threeddarkshadow";
                    case "ControlLight":
                        return "buttonface";
                    case "ControlLightLight":
                        return "buttonhighlight";
                    case "ControlText":
                        return "buttontext";
                    case "Desktop":
                        return "background";
                    case "GrayText":
                        return "graytext";
                    case "Highlight":
                    case "HotTrack":
                        return "highlight";
                    case "HighlightText":
                    case "MenuHighlight":
                        return "highlighttext";
                    case "InactiveBorder":
                        return "inactiveborder";
                    case "InactiveCaption":
                    case "GradientInactiveCaption":
                        return "inactivecaption";
                    case "InactiveCaptionText":
                        return "inactivecaptiontext";
                    case "Info":
                        return "infobackground";
                    case "InfoText":
                        return "infotext";
                    case "Menu":
                    case "MenuBar":
                        return "menu";
                    case "MenuText":
                        return "menutext";
                    case "ScrollBar":
                        return "scrollbar";
                    case "Window":
                        return "window";
                    case "WindowFrame":
                        return "windowframe";
                    case "WindowText":
                        return "windowtext";
                    case "ButtonFace":
                        return "buttonface";
                    case "ButtonHighlight":
                        return "buttonhighlight";
                    case "ButtonShadow":
                        return "buttonshadow";
                    case "LightGray":
                        return "lightgrey";

                    default:
                        return color._name;
                }

                #endregion
            }

            return ("#" + color._red.ToString("X2", null) + color._green.ToString("X2", null) +
                    color._blue.ToString("X2", null));
        }

        /// <summary>Returns a Win32 color from a GeoColor.</summary>
        /// <returns>Win32 color from a GeoColor.</returns>
        /// <param name="color">GeoColor to convert Win32 color.</param>
        public static int ToWin32(GeoColor color)
        {
            Validators.CheckParameterIsNotNull(color, "drawingColor");

            return ((color._red | (color._green << 8)) | (color._blue << 0x10));
        }

        /// <summary>Override of the == operator.</summary>
        /// <remarks>None</remarks>
        /// <returns>Equality of the two instances.</returns>
        /// <param name="geoColor1">First GeoColor to compare.</param>
        /// <param name="geoColor2">Second GeoColor to compare.</param>
        public static bool operator ==(GeoColor geoColor1, GeoColor geoColor2)
        {
            Validators.CheckParameterIsNotNull(geoColor1, "sourceGeoColor");
            Validators.CheckParameterIsNotNull(geoColor2, "targetGeoColor");

            return geoColor1.Equals(geoColor2);
        }

        /// <summary>Override of the != operator.</summary>
        /// <remarks>None</remarks>
        /// <returns>Inequality of the two instances.</returns>
        /// <param name="geoColor1">First GeoColor to compare.</param>
        /// <param name="geoColor2">Second GeoColor to compare.</param>
        public static bool operator !=(GeoColor geoColor1, GeoColor geoColor2)
        {
            Validators.CheckParameterIsNotNull(geoColor1, "sourceGeoColor");
            Validators.CheckParameterIsNotNull(geoColor2, "targetGeoColor");

            return !(geoColor1.Equals(geoColor2));
        }

        /// <summary>Override of the Equals method.</summary>
        /// <returns>Equals of an object.</returns>
        /// <remarks>None</remarks>
        /// <param name="obj">Object to check equality to the current instance.</param>
        public override bool Equals(object obj)
        {
            Validators.CheckParameterIsNotNull(obj, "obj");

            if (!(obj is GeoColor)) return false;
            return Equals((GeoColor) obj);
        }

        private bool Equals(GeoColor compareObj)
        {
            if (AlphaComponent != compareObj.AlphaComponent) return false;
            if (GreenComponent != compareObj.GreenComponent) return false;
            if (BlueComponent != compareObj.BlueComponent) return false;
            if (RedComponent != compareObj.RedComponent) return false;

            return true;
        }

        /// <summary>Override of the GetHashCode method.</summary>
        /// <returns>Hash code.</returns>
        /// <remarks>None</remarks>
        public override int GetHashCode()
        {
            return _alpha.GetHashCode() ^ _red.GetHashCode() ^ _green.GetHashCode() ^ _blue.GetHashCode();
        }

        private static GeoColor HslToRgb(float hue, float saturation, float luminance)
        {
            byte red;
            byte green;
            byte blue;
            if (saturation == 0.0)
            {
                red = Convert.ToByte(luminance*255f);
                green = red;
                blue = red;
            }
            else
            {
                float rm1;
                float rm2;

                if (luminance <= 0.5f)
                {
                    rm2 = luminance + luminance*saturation;
                }
                else
                {
                    rm2 = luminance + saturation - luminance*saturation;
                }
                rm1 = 2f*luminance - rm2;
                red = ToRgb(rm1, rm2, hue + 120f);
                green = ToRgb(rm1, rm2, hue);
                blue = ToRgb(rm1, rm2, hue - 120f);
            }

            return FromArgb(255, red, green, blue);
        }

        private static byte ToRgb(float rm1, float rm2, float rh)
        {
            if (rh > 360f)
            {
                rh -= 360f;
            }
            else if (rh < 0f)
            {
                rh += 360f;
            }

            if (rh < 60f)
            {
                rm1 = rm1 + (rm2 - rm1)*rh/60f;
            }
            else if (rh < 180f)
            {
                rm1 = rm2;
            }
            else if (rh < 240f)
            {
                rm1 = rm1 + (rm2 - rm1)*(240f - rh)/60f;
            }

            return Convert.ToByte(rm1*255);
        }

        private static void RgbToHsl(GeoColor geoColor, out float hue, out float saturation, out float luminance)
        {
            var red = geoColor._red;
            var green = geoColor._green;
            var blue = geoColor._blue;

            var minValue = Math.Min(red, Math.Min(green, blue));
            var maxValue = Math.Max(red, Math.Max(green, blue));
            var mdiff = (float) maxValue - minValue;
            var msum = (float) maxValue + minValue;
            luminance = msum/510f;
            if (maxValue == minValue)
            {
                saturation = 0f;
                hue = 0f;
            }
            else
            {
                var rnorm = (maxValue - red)/mdiff;
                var gnorm = (maxValue - green)/mdiff;
                var bnorm = (maxValue - blue)/mdiff;

                if (luminance <= 0.5f)
                {
                    saturation = mdiff/msum;
                }
                else
                {
                    saturation = (mdiff/(510f - msum));
                }

                hue = 0;
                if (red == maxValue)
                {
                    hue = 60f*(6f + bnorm - gnorm);
                }
                if (green == maxValue)
                {
                    hue = 60f*(2f + rnorm - bnorm);
                }
                if (blue == maxValue)
                {
                    hue = 60f*(4f + gnorm - rnorm);
                }
                if (hue > 360f)
                {
                    hue = hue - 360f;
                }
            }
        }
    }
}