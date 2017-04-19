using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.Layers
{
    /// <summary>Layer to hide part of the map to restrict viewing.</summary>
    [Serializable]
    public class RestrictionLayer : BaseLayer
    {
        
        double _lowerScale;
        RestrictionMode _restrictionMode;
        RestrictionStyle _restrictionStyle;
        double _upperScale;
        
        public RestrictionLayer()
        {
            Zones = new SafeCollection<BaseAreaShape>();
        }

        public BaseStyle DefaultStyle { get; set; }

        public double UpperScale
        {
            get { return _upperScale; }
            set { _upperScale = value; }
        }

        public double LowerScale
        {
            get { return _lowerScale; }
            set { _lowerScale = value; }
        }

        public SafeCollection<BaseStyle> CustomStyles { get; set; }
        

        public RestrictionMode RestrictionMode
        {
            get { return _restrictionMode; }
            set { _restrictionMode = value; }
        }

        public SafeCollection<BaseAreaShape> Zones { get; set; }
        
        public RestrictionStyle RestrictionStyle
        {
            get { return _restrictionStyle; }
            set { _restrictionStyle = value; }
        }

        protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckLayerIsOpened(IsOpen);

            RectangleShape boundingBox = null;

            foreach (BaseAreaShape zone in Zones)
            {
                if (boundingBox == null)
                {
                    boundingBox = zone.GetBoundingBox();
                }
                else
                {
                    boundingBox.ExpandToInclude(zone.GetBoundingBox());
                }
            }

            return boundingBox;
        }

        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
			DrawHelper.DrawLayer(this, canvas, labelsInAllLayers);
        }
		
    }
}