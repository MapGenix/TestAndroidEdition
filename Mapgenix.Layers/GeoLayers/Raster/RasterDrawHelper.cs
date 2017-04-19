using System.Collections.ObjectModel;
using System.Drawing;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using System.IO;
using Mapgenix.RasterSource;

namespace Mapgenix.Layers
{
    public static class RasterDrawHelper
    {
        public static void DrawRaster(RasterLayer self, BaseGeoCanvas canvas)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckLayerIsOpened(self.IsOpen);
            Validators.CheckRasterSourceIsOpen(self.ImageSource.IsOpen);
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            GeoColor tempKeyColor = GeoColor.StandardColors.Transparent;
            Collection<GeoColor> tempKeyColors = new Collection<GeoColor>();
            if (canvas.HasKeyColor)
            {
                tempKeyColor = canvas.KeyColor;

                tempKeyColors = canvas.KeyColors;
                canvas.KeyColor = self.KeyColor;

                canvas.KeyColors.Clear();
                foreach (GeoColor oneKeyColor in self.KeyColors)
                {
                    canvas.KeyColors.Add(oneKeyColor);
                }
            }


            int canvasWidth = (int)canvas.Width;
            int canvasHeight = (int)canvas.Height;

            bool isInThreshold = RasterLayerHelper.IsExtentWithinThreshold(canvas.CurrentWorldExtent, self.UpperThreshold, self.LowerThreshold, canvasWidth, canvas.MapUnit, canvas.Dpi);
            if (isInThreshold)
            {
                PointShape centerPointShape;
                Rectangle smallImageRectange = new Rectangle();
                if (self.ImageSource is WmsRasterSource)
                {
                    centerPointShape = canvas.CurrentWorldExtent.GetCenterPoint();
                    smallImageRectange = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent, canvas.CurrentWorldExtent, canvasWidth, canvasHeight);
                }
                else
                {
                    RectangleShape imageExtent = self.GetBoundingBox();
                    bool isOverloap = imageExtent.Intersects(canvas.CurrentWorldExtent);
                    if (!isOverloap)
                    {
                        return;
                    }
                    RectangleShape overlapShape = imageExtent.GetIntersection(canvas.CurrentWorldExtent);
                    centerPointShape = overlapShape.GetCenterPoint();
                    smallImageRectange = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent, overlapShape, canvasWidth, canvasHeight);
                }

                using (GeoImage tempImage = self.ImageSource.GetImage(canvas.CurrentWorldExtent, canvasWidth, canvasHeight))
                {
                    if (tempImage != null)
                    {
                        if (canvas is GdiPlusGeoCanvas)
                        {
                            float widthInScreen = smallImageRectange.Width + 2;
                            float heightInScreen = smallImageRectange.Height + 2;
                            if (self.ImageSource is WmsRasterSource)
                            {
                                widthInScreen = smallImageRectange.Width;
                                heightInScreen = smallImageRectange.Height;
                            }
                            Stream sourceStream = tempImage.GetImageStream(canvas);
                            Bitmap bitmap = new Bitmap(sourceStream);
                           

                            canvas.DrawWorldImage(tempImage, centerPointShape.X, centerPointShape.Y, widthInScreen, heightInScreen, DrawingLevel.LevelOne);
                        }
                        else
                        {
                            canvas.DrawWorldImageWithoutScaling(tempImage, centerPointShape.X, centerPointShape.Y, DrawingLevel.LevelOne);
                        }
                    }
                }

            }

            if (canvas.HasKeyColor)
            {
                canvas.KeyColor = tempKeyColor;
                canvas.KeyColors.Clear();
                foreach (GeoColor oneKeyColor in tempKeyColors)
                {
                    canvas.KeyColors.Add(oneKeyColor);
                }
            }
        }

    }
}
