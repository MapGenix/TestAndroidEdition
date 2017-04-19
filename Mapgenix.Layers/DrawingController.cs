using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkGeo.MapSuite.Core;

namespace Core_Layers
{
    public class DrawingController
    {

        public event EventHandler<DrawingProgressChangedEventArgs> DrawingProgressChanged;
        /// <summary>This method draws the Layer.</summary>
        /// <returns>None</returns>
        /// <remarks>
        /// 	<para>This method is the concrete wrapper for the abstract method DrawCore. This
        ///     method draws the representation of the layer based on the extent you provided.<br/>
        /// 		<br/>
        ///     As this is a concrete public method that wraps a Core method, we reserve the right
        ///     to add events and other logic to pre- or post-process data returned by the Core version
        ///     of the method. In this way, we leave our framework open on our end, but also allow you
        ///     the developer to extend our logic to suit your needs. If you have questions about this,
        ///     please contact our support team as we would be happy to work with you on extending our
        ///     framework.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If you pass a null as the worldExtent, we will throw an ArgumentNullException.</exception>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If you pass a null as the geoImageOrNativeImage, we will throw an ArgumentNullException.</exception>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If you pass a null as the labeledInLayers, we will throw an ArgumentNullException.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException" caption="ArgumentOutOfRangeException">If you pass in a mapUnit that is not defined in the enumeration, it will throw a ArgumentOutOfRangeException.</exception>
        /// <param name="canvas">This parameter is the canvas object or a GeoImage to draw on.</param>
        /// <param name="labelsInAllLayers">
        /// This parameter represents the labels used for collision detection and duplication
        /// checking.
        /// </param>
        public void Draw(Layer self, GeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labeledInLayers");
            try
            {
                canvas.DrawingProgressChanged += new EventHandler<DrawingProgressChangedEventArgs>(canvas_ProgressDrawing);
                
                if (self.IsVisible)
                {
                    switch (self.DrawingExceptionMode)
                    {
                        case DrawingExceptionMode.DrawException:
                            try
                            {
                                self.DrawCore(canvas, labelsInAllLayers);
                            }
                            catch (Exception ex)
                            {
                                self.DrawException(canvas, ex);
                            }
                            break;
                        default:
                            self.DrawCore(canvas, labelsInAllLayers);
                            break;
                    }
                }

                
            }
            finally
            {
                canvas.DrawingProgressChanged -= new EventHandler<DrawingProgressChangedEventArgs>(canvas_ProgressDrawing);
            }
        }

        protected virtual void OnDrawingProgressChanged(DrawingProgressChangedEventArgs e)
        {
            EventHandler<DrawingProgressChangedEventArgs> handler = DrawingProgressChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void canvas_ProgressDrawing(object sender, DrawingProgressChangedEventArgs e)
        {
            OnDrawingProgressChanged(e);
        }

    }
}
