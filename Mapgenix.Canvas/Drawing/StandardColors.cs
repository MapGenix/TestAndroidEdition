using System;

namespace Mapgenix.Canvas
{
    /// <summary>
    /// Predefined set of standard colors.
    /// </summary>
    [Serializable]
    public class StandardColors
    {
        internal StandardColors()
        {
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor ActiveBorder
        {
            get { return new GeoColor("ActiveBorder", ColorType.StandardColor, 255, 212, 208, 200); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor ActiveCaption
        {
            get { return new GeoColor("ActiveCaption", ColorType.StandardColor, 255, 0, 84, 227); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor ActiveCaptionText
        {
            get { return new GeoColor("ActiveCaptionText", ColorType.StandardColor, 255, 255, 255, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor AppWorkspace
        {
            get { return new GeoColor("AppWorkspace", ColorType.StandardColor, 255, 128, 128, 128); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Control
        {
            get { return new GeoColor("Control", ColorType.StandardColor, 255, 236, 233, 216); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor ControlDark
        {
            get { return new GeoColor("ControlDark", ColorType.StandardColor, 255, 172, 168, 153); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor ControlDarkDark
        {
            get { return new GeoColor("ControlDarkDark", ColorType.StandardColor, 255, 113, 111, 100); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor ControlLight
        {
            get { return new GeoColor("ControlLight", ColorType.StandardColor, 255, 241, 239, 226); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor ControlLightLight
        {
            get { return new GeoColor("ControlLightLight", ColorType.StandardColor, 255, 255, 255, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor ControlText
        {
            get { return new GeoColor("ControlText", ColorType.StandardColor, 255, 0, 0, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Desktop
        {
            get { return new GeoColor("Desktop", ColorType.StandardColor, 255, 0, 78, 152); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor GrayText
        {
            get { return new GeoColor("GrayText", ColorType.StandardColor, 255, 172, 168, 153); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Highlight
        {
            get { return new GeoColor("Highlight", ColorType.StandardColor, 255, 49, 106, 197); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor HighlightText
        {
            get { return new GeoColor("HighlightText", ColorType.StandardColor, 255, 255, 255, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor HotTrack
        {
            get { return new GeoColor("HotTrack", ColorType.StandardColor, 255, 0, 0, 128); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor InactiveBorder
        {
            get { return new GeoColor("InactiveBorder", ColorType.StandardColor, 255, 212, 208, 200); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor InactiveCaption
        {
            get { return new GeoColor("InactiveCaption", ColorType.StandardColor, 255, 122, 150, 223); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor InactiveCaptionText
        {
            get { return new GeoColor("InactiveCaptionText", ColorType.StandardColor, 255, 216, 228, 248); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Info
        {
            get { return new GeoColor("Info", ColorType.StandardColor, 255, 255, 255, 225); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor InfoText
        {
            get { return new GeoColor("InfoText", ColorType.StandardColor, 255, 0, 0, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Menu
        {
            get { return new GeoColor("Menu", ColorType.StandardColor, 255, 255, 255, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor MenuText
        {
            get { return new GeoColor("MenuText", ColorType.StandardColor, 255, 0, 0, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor ScrollBar
        {
            get { return new GeoColor("ScrollBar", ColorType.StandardColor, 255, 212, 208, 200); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Window
        {
            get { return new GeoColor("Window", ColorType.StandardColor, 255, 182, 222, 187); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor WindowFrame
        {
            get { return new GeoColor("WindowFrame", ColorType.StandardColor, 255, 0, 0, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor WindowText
        {
            get { return new GeoColor("WindowText", ColorType.StandardColor, 255, 0, 0, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Transparent
        {
            get { return new GeoColor("Transparent", ColorType.StandardColor, 0, 255, 255, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor AliceBlue
        {
            get { return new GeoColor("AliceBlue", ColorType.StandardColor, 255, 240, 248, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor AntiqueWhite
        {
            get { return new GeoColor("AntiqueWhite", ColorType.StandardColor, 255, 250, 235, 215); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Aqua
        {
            get { return new GeoColor("Aqua", ColorType.StandardColor, 255, 0, 255, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Aquamarine
        {
            get { return new GeoColor("Aquamarine", ColorType.StandardColor, 255, 127, 255, 212); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Azure
        {
            get { return new GeoColor("Azure", ColorType.StandardColor, 255, 240, 255, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Beige
        {
            get { return new GeoColor("Beige", ColorType.StandardColor, 255, 245, 245, 220); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Bisque
        {
            get { return new GeoColor("Bisque", ColorType.StandardColor, 255, 255, 228, 196); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Black
        {
            get { return new GeoColor("Black", ColorType.StandardColor, 255, 0, 0, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor BlanchedAlmond
        {
            get { return new GeoColor("BlanchedAlmond", ColorType.StandardColor, 255, 255, 235, 205); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Blue
        {
            get { return new GeoColor("Blue", ColorType.StandardColor, 255, 0, 0, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor BlueViolet
        {
            get { return new GeoColor("BlueViolet", ColorType.StandardColor, 255, 138, 43, 226); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Brown
        {
            get { return new GeoColor("Brown", ColorType.StandardColor, 255, 165, 42, 42); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor BurlyWood
        {
            get { return new GeoColor("BurlyWood", ColorType.StandardColor, 255, 222, 184, 135); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor CadetBlue
        {
            get { return new GeoColor("CadetBlue", ColorType.StandardColor, 255, 95, 158, 160); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Chartreuse
        {
            get { return new GeoColor("Chartreuse", ColorType.StandardColor, 255, 127, 255, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Chocolate
        {
            get { return new GeoColor("Chocolate", ColorType.StandardColor, 255, 210, 105, 30); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Coral
        {
            get { return new GeoColor("Coral", ColorType.StandardColor, 255, 255, 127, 80); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor CornflowerBlue
        {
            get { return new GeoColor("CornflowerBlue", ColorType.StandardColor, 255, 100, 149, 237); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Cornsilk
        {
            get { return new GeoColor("Cornsilk", ColorType.StandardColor, 255, 255, 248, 220); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Crimson
        {
            get { return new GeoColor("Crimson", ColorType.StandardColor, 255, 220, 20, 60); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Cyan
        {
            get { return new GeoColor("Cyan", ColorType.StandardColor, 255, 0, 255, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkBlue
        {
            get { return new GeoColor("DarkBlue", ColorType.StandardColor, 255, 0, 0, 139); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkCyan
        {
            get { return new GeoColor("DarkCyan", ColorType.StandardColor, 255, 0, 139, 139); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkGoldenrod
        {
            get { return new GeoColor("DarkGoldenrod", ColorType.StandardColor, 255, 184, 134, 11); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkGray
        {
            get { return new GeoColor("DarkGray", ColorType.StandardColor, 255, 169, 169, 169); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkGreen
        {
            get { return new GeoColor("DarkGreen", ColorType.StandardColor, 255, 0, 100, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkKhaki
        {
            get { return new GeoColor("DarkKhaki", ColorType.StandardColor, 255, 189, 183, 107); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkMagenta
        {
            get { return new GeoColor("DarkMagenta", ColorType.StandardColor, 255, 139, 0, 139); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkOliveGreen
        {
            get { return new GeoColor("DarkOliveGreen", ColorType.StandardColor, 255, 85, 107, 47); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkOrange
        {
            get { return new GeoColor("DarkOrange", ColorType.StandardColor, 255, 255, 140, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkOrchid
        {
            get { return new GeoColor("DarkOrchid", ColorType.StandardColor, 255, 153, 50, 204); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkRed
        {
            get { return new GeoColor("DarkRed", ColorType.StandardColor, 255, 139, 0, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
       public GeoColor DarkSalmon
        {
            get { return new GeoColor("DarkSalmon", ColorType.StandardColor, 255, 233, 150, 122); }
        }

       /// <summary>Gets the color based on the GDI+ version of the color.</summary>
       /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkSeaGreen
        {
            get { return new GeoColor("DarkSeaGreen", ColorType.StandardColor, 255, 143, 188, 139); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkSlateBlue
        {
            get { return new GeoColor("DarkSlateBlue", ColorType.StandardColor, 255, 72, 61, 139); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkSlateGray
        {
            get { return new GeoColor("DarkSlateGray", ColorType.StandardColor, 255, 47, 79, 79); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkTurquoise
        {
            get { return new GeoColor("DarkTurquoise", ColorType.StandardColor, 255, 0, 206, 209); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DarkViolet
        {
            get { return new GeoColor("DarkViolet", ColorType.StandardColor, 255, 148, 0, 211); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
         public GeoColor DeepPink
        {
            get { return new GeoColor("DeepPink", ColorType.StandardColor, 255, 255, 20, 147); }
        }

         /// <summary>Gets the color based on the GDI+ version of the color.</summary>
         /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DeepSkyBlue
        {
            get { return new GeoColor("DeepSkyBlue", ColorType.StandardColor, 255, 0, 191, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DimGray
        {
            get { return new GeoColor("DimGray", ColorType.StandardColor, 255, 105, 105, 105); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor DodgerBlue
        {
            get { return new GeoColor("DodgerBlue", ColorType.StandardColor, 255, 30, 144, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Firebrick
        {
            get { return new GeoColor("Firebrick", ColorType.StandardColor, 255, 178, 34, 34); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor FloralWhite
        {
            get { return new GeoColor("FloralWhite", ColorType.StandardColor, 255, 255, 250, 240); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor ForestGreen
        {
            get { return new GeoColor("ForestGreen", ColorType.StandardColor, 255, 34, 139, 34); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Fuchsia
        {
            get { return new GeoColor("Fuchsia", ColorType.StandardColor, 255, 255, 0, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Gainsboro
        {
            get { return new GeoColor("Gainsboro", ColorType.StandardColor, 255, 220, 220, 220); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor GhostWhite
        {
            get { return new GeoColor("GhostWhite", ColorType.StandardColor, 255, 248, 248, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Gold
        {
            get { return new GeoColor("Gold", ColorType.StandardColor, 255, 255, 215, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Goldenrod
        {
            get { return new GeoColor("Goldenrod", ColorType.StandardColor, 255, 218, 165, 32); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Gray
        {
            get { return new GeoColor("Gray", ColorType.StandardColor, 255, 128, 128, 128); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Green
        {
            get { return new GeoColor("Green", ColorType.StandardColor, 255, 0, 128, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor GreenYellow
        {
            get { return new GeoColor("GreenYellow", ColorType.StandardColor, 255, 173, 255, 47); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Honeydew
        {
            get { return new GeoColor("Honeydew", ColorType.StandardColor, 255, 240, 255, 240); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor HotPink
        {
            get { return new GeoColor("HotPink", ColorType.StandardColor, 255, 255, 105, 180); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor IndianRed
        {
            get { return new GeoColor("IndianRed", ColorType.StandardColor, 255, 205, 92, 92); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Indigo
        {
            get { return new GeoColor("Indigo", ColorType.StandardColor, 255, 75, 0, 130); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Ivory
        {
            get { return new GeoColor("Ivory", ColorType.StandardColor, 255, 255, 255, 240); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Khaki
        {
            get { return new GeoColor("Khaki", ColorType.StandardColor, 255, 240, 230, 140); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Lavender
        {
            get { return new GeoColor("Lavender", ColorType.StandardColor, 255, 230, 230, 250); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor LavenderBlush
        {
            get { return new GeoColor("LavenderBlush", ColorType.StandardColor, 255, 255, 240, 245); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor LawnGreen
        {
            get { return new GeoColor("LawnGreen", ColorType.StandardColor, 255, 124, 252, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor LemonChiffon
        {
            get { return new GeoColor("LemonChiffon", ColorType.StandardColor, 255, 255, 250, 205); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor LightBlue
        {
            get { return new GeoColor("LightBlue", ColorType.StandardColor, 255, 173, 216, 230); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
       public GeoColor LightCoral
        {
            get { return new GeoColor("LightCoral", ColorType.StandardColor, 255, 240, 128, 128); }
        }

       /// <summary>Gets the color based on the GDI+ version of the color.</summary>
       /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor LightCyan
        {
            get { return new GeoColor("LightCyan", ColorType.StandardColor, 255, 224, 255, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor LightGoldenrodYellow
        {
            get { return new GeoColor("LightGoldenrodYellow", ColorType.StandardColor, 255, 250, 250, 210); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor LightGray
        {
            get { return new GeoColor("LightGray", ColorType.StandardColor, 255, 211, 211, 211); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor LightGreen
        {
            get { return new GeoColor("LightGreen", ColorType.StandardColor, 255, 144, 238, 144); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor LightPink
        {
            get { return new GeoColor("LightPink", ColorType.StandardColor, 255, 255, 182, 193); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor LightSalmon
        {
            get { return new GeoColor("LightSalmon", ColorType.StandardColor, 255, 255, 160, 122); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor LightSeaGreen
        {
            get { return new GeoColor("LightSeaGreen", ColorType.StandardColor, 255, 32, 178, 170); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor LightSkyBlue
        {
            get { return new GeoColor("LightSkyBlue", ColorType.StandardColor, 255, 135, 206, 250); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor LightSlateGray
        {
            get { return new GeoColor("LightSlateGray", ColorType.StandardColor, 255, 119, 136, 153); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
       public GeoColor LightSteelBlue
        {
            get { return new GeoColor("LightSteelBlue", ColorType.StandardColor, 255, 176, 196, 222); }
        }

       /// <summary>Gets the color based on the GDI+ version of the color.</summary>
       /// <remarks>Exposes the colors shipping with GDI+.</remarks>
       public GeoColor LightYellow
        {
            get { return new GeoColor("LightYellow", ColorType.StandardColor, 255, 255, 255, 224); }
        }

       /// <summary>Gets the color based on the GDI+ version of the color.</summary>
       /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Lime
        {
            get { return new GeoColor("Lime", ColorType.StandardColor, 255, 0, 255, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor LimeGreen
        {
            get { return new GeoColor("LimeGreen", ColorType.StandardColor, 255, 50, 205, 50); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Linen
        {
            get { return new GeoColor("Linen", ColorType.StandardColor, 255, 250, 240, 230); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Magenta
        {
            get { return new GeoColor("Magenta", ColorType.StandardColor, 255, 255, 0, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Maroon
        {
            get { return new GeoColor("Maroon", ColorType.StandardColor, 255, 128, 0, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor MediumAquamarine
        {
            get { return new GeoColor("MediumAquamarine", ColorType.StandardColor, 255, 102, 205, 170); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor MediumBlue
        {
            get { return new GeoColor("MediumBlue", ColorType.StandardColor, 255, 0, 0, 205); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor MediumOrchid
        {
            get { return new GeoColor("MediumOrchid", ColorType.StandardColor, 255, 186, 85, 211); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor MediumPurple
        {
            get { return new GeoColor("MediumPurple", ColorType.StandardColor, 255, 147, 112, 219); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor MediumSeaGreen
        {
            get { return new GeoColor("MediumSeaGreen", ColorType.StandardColor, 255, 60, 179, 113); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor MediumSlateBlue
        {
            get { return new GeoColor("MediumSlateBlue", ColorType.StandardColor, 255, 123, 104, 238); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor MediumSpringGreen
        {
            get { return new GeoColor("MediumSpringGreen", ColorType.StandardColor, 255, 0, 250, 154); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor MediumTurquoise
        {
            get { return new GeoColor("MediumTurquoise", ColorType.StandardColor, 255, 72, 209, 204); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor MediumVioletRed
        {
            get { return new GeoColor("MediumVioletRed", ColorType.StandardColor, 255, 199, 21, 133); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor MidnightBlue
        {
            get { return new GeoColor("MidnightBlue", ColorType.StandardColor, 255, 25, 25, 112); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor MintCream
        {
            get { return new GeoColor("MintCream", ColorType.StandardColor, 255, 245, 255, 250); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor MistyRose
        {
            get { return new GeoColor("MistyRose", ColorType.StandardColor, 255, 255, 228, 225); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Moccasin
        {
            get { return new GeoColor("Moccasin", ColorType.StandardColor, 255, 255, 228, 181); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor NavajoWhite
        {
            get { return new GeoColor("NavajoWhite", ColorType.StandardColor, 255, 255, 222, 173); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Navy
        {
            get { return new GeoColor("Navy", ColorType.StandardColor, 255, 0, 0, 128); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor OldLace
        {
            get { return new GeoColor("OldLace", ColorType.StandardColor, 255, 253, 245, 230); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Olive
        {
            get { return new GeoColor("Olive", ColorType.StandardColor, 255, 128, 128, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor OliveDrab
        {
            get { return new GeoColor("OliveDrab", ColorType.StandardColor, 255, 107, 142, 35); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Orange
        {
            get { return new GeoColor("Orange", ColorType.StandardColor, 255, 255, 165, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor OrangeRed
        {
            get { return new GeoColor("OrangeRed", ColorType.StandardColor, 255, 255, 69, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Orchid
        {
            get { return new GeoColor("Orchid", ColorType.StandardColor, 255, 218, 112, 214); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor PaleGoldenrod
        {
            get { return new GeoColor("PaleGoldenrod", ColorType.StandardColor, 255, 238, 232, 170); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor PaleGreen
        {
            get { return new GeoColor("PaleGreen", ColorType.StandardColor, 255, 152, 251, 152); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor PaleTurquoise
        {
            get { return new GeoColor("PaleTurquoise", ColorType.StandardColor, 255, 175, 238, 238); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor PaleVioletRed
        {
            get { return new GeoColor("PaleVioletRed", ColorType.StandardColor, 255, 219, 112, 147); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor PapayaWhip
        {
            get { return new GeoColor("PapayaWhip", ColorType.StandardColor, 255, 255, 239, 213); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor PeachPuff
        {
            get { return new GeoColor("PeachPuff", ColorType.StandardColor, 255, 255, 218, 185); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Peru
        {
            get { return new GeoColor("Peru", ColorType.StandardColor, 255, 205, 133, 63); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Pink
        {
            get { return new GeoColor("Pink", ColorType.StandardColor, 255, 255, 192, 203); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Plum
        {
            get { return new GeoColor("Plum", ColorType.StandardColor, 255, 221, 160, 221); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor PowderBlue
        {
            get { return new GeoColor("PowderBlue", ColorType.StandardColor, 255, 176, 224, 230); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Purple
        {
            get { return new GeoColor("Purple", ColorType.StandardColor, 255, 128, 0, 128); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Red
        {
            get { return new GeoColor("Red", ColorType.StandardColor, 255, 255, 0, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor RosyBrown
        {
            get { return new GeoColor("RosyBrown", ColorType.StandardColor, 255, 188, 143, 143); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor RoyalBlue
        {
            get { return new GeoColor("RoyalBlue", ColorType.StandardColor, 255, 65, 105, 225); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor SaddleBrown
        {
            get { return new GeoColor("SaddleBrown", ColorType.StandardColor, 255, 139, 69, 19); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Salmon
        {
            get { return new GeoColor("Salmon", ColorType.StandardColor, 255, 250, 128, 114); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor SandyBrown
        {
            get { return new GeoColor("SandyBrown", ColorType.StandardColor, 255, 244, 164, 96); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor SeaGreen
        {
            get { return new GeoColor("SeaGreen", ColorType.StandardColor, 255, 46, 139, 87); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor SeaShell
        {
            get { return new GeoColor("SeaShell", ColorType.StandardColor, 255, 255, 245, 238); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Sienna
        {
            get { return new GeoColor("Sienna", ColorType.StandardColor, 255, 160, 82, 45); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Silver
        {
            get { return new GeoColor("Silver", ColorType.StandardColor, 255, 192, 192, 192); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor SkyBlue
        {
            get { return new GeoColor("SkyBlue", ColorType.StandardColor, 255, 135, 206, 235); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor SlateBlue
        {
            get { return new GeoColor("SlateBlue", ColorType.StandardColor, 255, 106, 90, 205); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor SlateGray
        {
            get { return new GeoColor("SlateGray", ColorType.StandardColor, 255, 112, 128, 144); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Snow
        {
            get { return new GeoColor("Snow", ColorType.StandardColor, 255, 255, 250, 250); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor SpringGreen
        {
            get { return new GeoColor("SpringGreen", ColorType.StandardColor, 255, 0, 255, 127); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor SteelBlue
        {
            get { return new GeoColor("SteelBlue", ColorType.StandardColor, 255, 70, 130, 180); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
       public GeoColor Tan
        {
            get { return new GeoColor("Tan", ColorType.StandardColor, 255, 210, 180, 140); }
        }

       /// <summary>Gets the color based on the GDI+ version of the color.</summary>
       /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Teal
        {
            get { return new GeoColor("Teal", ColorType.StandardColor, 255, 0, 128, 128); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Thistle
        {
            get { return new GeoColor("Thistle", ColorType.StandardColor, 255, 216, 191, 216); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Tomato
        {
            get { return new GeoColor("Tomato", ColorType.StandardColor, 255, 255, 99, 71); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Turquoise
        {
            get { return new GeoColor("Turquoise", ColorType.StandardColor, 255, 64, 224, 208); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Violet
        {
            get { return new GeoColor("Violet", ColorType.StandardColor, 255, 238, 130, 238); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Wheat
        {
            get { return new GeoColor("Wheat", ColorType.StandardColor, 255, 245, 222, 179); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor White
        {
            get { return new GeoColor("White", ColorType.StandardColor, 255, 255, 255, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor WhiteSmoke
        {
            get { return new GeoColor("WhiteSmoke", ColorType.StandardColor, 255, 245, 245, 245); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor Yellow
        {
            get { return new GeoColor("Yellow", ColorType.StandardColor, 255, 255, 255, 0); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor YellowGreen
        {
            get { return new GeoColor("YellowGreen", ColorType.StandardColor, 255, 154, 205, 50); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor ButtonFace
        {
            get { return new GeoColor("ButtonFace", ColorType.StandardColor, 255, 236, 233, 216); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor ButtonHighlight
        {
            get { return new GeoColor("ButtonHighlight", ColorType.StandardColor, 255, 255, 255, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor ButtonShadow
        {
            get { return new GeoColor("ButtonShadow", ColorType.StandardColor, 255, 172, 168, 153); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor GradientActiveCaption
        {
            get { return new GeoColor("GradientActiveCaption", ColorType.StandardColor, 255, 61, 149, 255); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor GradientInactiveCaption
        {
            get { return new GeoColor("GradientInactiveCaption", ColorType.StandardColor, 255, 157, 185, 235); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor MenuBar
        {
            get { return new GeoColor("MenuBar", ColorType.StandardColor, 255, 236, 233, 216); }
        }

        /// <summary>Gets the color based on the GDI+ version of the color.</summary>
        /// <remarks>Exposes the colors shipping with GDI+.</remarks>
        public GeoColor MenuHighlight
        {
            get { return new GeoColor("MenuHighlight", ColorType.StandardColor, 255, 49, 106, 197); }
        }
    }
}