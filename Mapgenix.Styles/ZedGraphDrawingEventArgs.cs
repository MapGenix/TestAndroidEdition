using System;
using System.Collections.ObjectModel;
using System.Drawing;
using Mapgenix.Shapes;
using Mapgenix.Canvas;

namespace Mapgenix.Styles
{
    public class ZedGraphDrawingEventArgs : EventArgs
    {
        private Feature feature;
        private Collection<SimpleCandidate> labeledInLayers;
        private Collection<SimpleCandidate> labeledInLayer;
        private Bitmap bitmap;

        public ZedGraphDrawingEventArgs()
            : this(new Feature(), null, new Collection<SimpleCandidate>(), new Collection<SimpleCandidate>())
        { }

        public ZedGraphDrawingEventArgs(Feature feature, Bitmap bitmap, Collection<SimpleCandidate> labeledInLayer, Collection<SimpleCandidate> labeledInLayers)
        {
            this.feature = feature;
            this.bitmap = bitmap;
            this.labeledInLayer = labeledInLayer;
            this.labeledInLayers = labeledInLayers;
        }

        public Feature Feature
        {
            get { return feature; }
            set { feature = value; }
        }

        public Bitmap Bitmap
        {
            get { return bitmap; }
            set { bitmap = value; }
        }

        public Collection<SimpleCandidate> LabeledInLayer
        {
            get { return labeledInLayer; }
            set { labeledInLayer = value; }
        }

        public Collection<SimpleCandidate> LabeledInLayers
        {
            get { return labeledInLayers; }
            set { labeledInLayers = value; }
        }
    }
}
