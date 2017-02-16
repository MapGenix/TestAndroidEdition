using System;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Overlay containing markers.
    /// </summary>
    [Serializable]
    public abstract class BaseMarkerOverlay : BaseOverlay
    {
        protected BaseMarkerOverlay()
        {
            OverlayCanvas.SetValue(System.Windows.Controls.Panel.ZIndexProperty, ZIndexes.MarkerOverlay);
        }

        protected abstract SafeCollection<Marker> GetMarkersForDrawing(RectangleShape boundingBox);

        protected override void DrawCore(RectangleShape targetExtent, OverlayRefreshType overlayRefreshType)
        {
            
            if (overlayRefreshType == OverlayRefreshType.Pan)
            {
                foreach (Marker marker in OverlayCanvas.Children)
                {
                    marker.Draw(targetExtent, MapArguments.ActualWidth, MapArguments.ActualHeight);
                }
            }
            else
            {
                OverlayCanvas.Children.Clear();
                SafeCollection<Marker> drawingMarkers = GetMarkersForDrawing(targetExtent);
                foreach (Marker marker in drawingMarkers)
                {
                    if (targetExtent.Contains(new PointShape(marker.Position.X, marker.Position.Y)))
                    {
                        OverlayCanvas.Children.Add(marker);
                        marker.Draw(targetExtent, MapArguments.ActualWidth, MapArguments.ActualHeight);
                    }
                }

                OnDrawn(new ExtentEventArgs(targetExtent));
            }
        }
    }
}
