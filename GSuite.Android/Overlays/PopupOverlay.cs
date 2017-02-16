using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Overlay for storing popups.
    /// </summary>
    [Serializable]
    public class PopupOverlay : BaseOverlay
    {
        private SafeCollection<Popup> _popups;

        public PopupOverlay()
            : this(new Collection<Popup>() { })
        { }

        public PopupOverlay(IEnumerable<Popup> popups)
        {
            OverlayCanvas.SetValue(System.Windows.Controls.Panel.ZIndexProperty, ZIndexes.PopupOverlay);
            this._popups = new SafeCollection<Popup>();
            foreach (Popup popup in popups)
            {
                this._popups.Add(popup);
            }
        }

        public SafeCollection<Popup> Popups { get { return _popups; } }

        protected override RectangleShape GetBoundingBoxCore()
        {
            if (_popups.Count > 0)
            {
                MultipointShape multipoint = new MultipointShape();
                foreach (Popup popup in _popups)
                {
                    multipoint.Points.Add(new PointShape(popup.Position.X, popup.Position.Y));
                }

                return multipoint.GetBoundingBox();
            }
            else
            {
                return base.GetBoundingBoxCore();
            }
        }

        protected override void DrawCore(RectangleShape targetExtent, OverlayRefreshType overlayRefreshType)
        {
          
            if (overlayRefreshType == OverlayRefreshType.Pan)
            {
                foreach (Popup popup in OverlayCanvas.Children)
                {
                    popup.Draw(targetExtent, MapArguments.ActualWidth, MapArguments.ActualHeight);
                }
            }
            else
            {
                OverlayCanvas.Children.Clear();
                foreach (Popup popup in _popups)
                {
                    OverlayCanvas.Children.Add(popup);
                    popup.Draw(targetExtent, MapArguments.ActualWidth, MapArguments.ActualHeight);
                }
                OnDrawn(new ExtentEventArgs(targetExtent));
            }
        }
    }
}
