using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;

namespace Mapgenix.Layers
{
    /// <summary>
    /// Base class for adorment layers.
    /// <strong>ScaleLineAdornmentLayer</strong> and
    /// <strong>ScaleBarAdornmentLayer</strong>.
    /// </summary>
    [Serializable]
    public abstract class BaseAdornmentLayer : BaseLayer
    {
        float _xOffsetInPixel;
        float _yOffsetInPixel;
        AdornmentLocation _location;

        float _width;
        float _height;

        float _minimumWidth;
        float _minimumHeight;

        float _actualWidth;
        float _actualHeight;

        AreaStyle _backgroundMask;

        protected BaseAdornmentLayer()
        {
            _location = AdornmentLocation.UseOffsets;
            BackgroundMask = new AreaStyle();
            IsVisible = true;
        }

        public float Width
        {
            get { return _width; }
            set
            {
                _width = value;
                ActualWidth = _width;
            }
        }

        public float Height
        {
            get { return _height; }
            set
            {
                _height = value;
                _actualHeight = _height;
            }
        }

        internal float MinimumWidth
        {
            get { return _minimumWidth; }
            set
            {
                _minimumWidth = value;
                if (Width < _minimumWidth)
                {
                    Width = _minimumWidth;
                }
            }
        }

        internal float MinimumHeight
        {
            get { return _minimumHeight; }
            set
            {
                _minimumHeight = value;
                if (Height < MinimumHeight)
                {
                    Height = MinimumHeight;
                }
            }
        }

        internal float ActualWidth
        {
            get { return _actualWidth; }
            set { _actualWidth = value; }
        }

        internal float ActualHeight
        {
            get { return _actualHeight; }
            set { _actualHeight = value; }
        }

        public AreaStyle BackgroundMask
        {
            get { return _backgroundMask; }
            set { _backgroundMask = value; }
        }

        public AdornmentLocation Location
        {
            get { return _location; }
            set { _location = value; }
        }

        public float XOffsetInPixel
        {
            get { return _xOffsetInPixel; }
            set { _xOffsetInPixel = value; }
        }

        public float YOffsetInPixel
        {
            get { return _yOffsetInPixel; }
            set { _yOffsetInPixel = value; }
        }

        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {

            if (!BackgroundMask.OutlinePen.Color.IsTransparent || !BackgroundMask.FillSolidBrush.Color.IsTransparent)
            {
                Validators.CheckParameterIsNotNull(canvas, "canvas");
                Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

                ScreenPointF drawingLocation = GetDrawingLocation(canvas, Width, Height);
                ScreenPointF[] screenPoints = new ScreenPointF[5];
                screenPoints[0] = drawingLocation;
                screenPoints[1] = new ScreenPointF(drawingLocation.X + Width, drawingLocation.Y);
                screenPoints[2] = new ScreenPointF(drawingLocation.X + Width, drawingLocation.Y + Height);
                screenPoints[3] = new ScreenPointF(drawingLocation.X, drawingLocation.Y + Height);
                screenPoints[4] = new ScreenPointF(drawingLocation.X, drawingLocation.Y);

                Collection<ScreenPointF[]> screenPointsF = new Collection<ScreenPointF[]>();
                screenPointsF.Add(screenPoints);

                canvas.DrawArea(screenPointsF, BackgroundMask.OutlinePen, BackgroundMask.FillSolidBrush, DrawingLevel.LevelOne, 0, 0, PenBrushDrawingOrder.BrushFirst);
            }
        }

        public virtual ScreenPointF GetDrawingLocation(BaseGeoCanvas canvas, float adornmentWidth, float adornmentHeight)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");

            ScreenPointF result;

            switch (_location)
            {
                case AdornmentLocation.UseOffsets:
                case AdornmentLocation.UpperLeft:
                    result = new ScreenPointF(0, 0);
                    break;
                case AdornmentLocation.UpperCenter:
                    result = new ScreenPointF(Convert.ToSingle(canvas.Width * 0.5), 0);
                    break;
                case AdornmentLocation.UpperRight:
                    result = new ScreenPointF(canvas.Width - adornmentWidth, 0);
                    break;
                case AdornmentLocation.CenterLeft:
                    result = new ScreenPointF(0, Convert.ToSingle(canvas.Height * 0.5));
                    break;
                case AdornmentLocation.Center:
                    result = new ScreenPointF(Convert.ToSingle(canvas.Width * 0.5), Convert.ToSingle(canvas.Height * 0.5));
                    break;
                case AdornmentLocation.CenterRight:
                    result = new ScreenPointF(canvas.Width - adornmentWidth, Convert.ToSingle(canvas.Height * 0.5));
                    break;
                case AdornmentLocation.LowerLeft:
                    result = new ScreenPointF(0, canvas.Height - adornmentHeight);
                    break;
                case AdornmentLocation.LowerCenter:
                    result = new ScreenPointF(Convert.ToSingle(canvas.Width * 0.5), canvas.Height - adornmentHeight);
                    break;
                case AdornmentLocation.LowerRight:
                    result = new ScreenPointF(canvas.Width - adornmentWidth, canvas.Height - adornmentHeight);
                    break;
                default:
                    result = new ScreenPointF();
                    break;
            }

            return new ScreenPointF(result.X + _xOffsetInPixel, result.Y + _yOffsetInPixel);
        }
    }
}
