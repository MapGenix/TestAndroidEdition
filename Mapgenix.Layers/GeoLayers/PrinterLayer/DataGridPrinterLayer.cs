using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    public class DataGridPrinterLayer : BasePrinterLayer
    {
        public GeoPen CellBorderPen { get; set; }

        public BaseGeoBrush FontBrush { get; set; }

        public DataTable DataTable { get; set; }

        public GeoFont TextFont { get; set; }

        public LabelTextJustification CellTextJustification { get; set; }

        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {

            PrinterDrawHelper.DrawPrinterCore(this,canvas, labelsInAllLayers);
            PrinterDrawHelper.DrawGridPrinterCore(this,canvas);

        }

        public Dictionary<RectangleShape, string> GetCells(DataTable drawingData, double cellWidth, double cellHeight)
        {
            Dictionary<RectangleShape, string> rtn = new Dictionary<RectangleShape, string>();

            RectangleShape currentBoundingBox = GetBoundingBox();

            for (int r = 0; r < drawingData.Rows.Count; r++)
            {
                for (int c = 0; c < drawingData.Columns.Count; c++)
                {
                    PointShape ul = new PointShape(currentBoundingBox.UpperLeftPoint.X + c * cellWidth, currentBoundingBox.UpperLeftPoint.Y - r * cellHeight);
                    PointShape lr = new PointShape(currentBoundingBox.UpperLeftPoint.X + (c + 1) * cellWidth, currentBoundingBox.UpperLeftPoint.Y - (r + 1) * cellHeight);
                    RectangleShape cellRect = new RectangleShape(ul, lr);
                    rtn.Add(cellRect, drawingData.Rows[r][c].ToString());
                }
            }

            return rtn;
        }

        public MultilineShape GetCellBorder(DataTable drawingData, double cellWidth, double cellHeight)
        {
            MultilineShape rtn = new MultilineShape();
            RectangleShape currentBoundingBox = GetBoundingBox();

            for (int r = 0; r < drawingData.Rows.Count; r++)
            {
                LineShape hline = new LineShape();
                hline.Vertices.Add(new Vertex(currentBoundingBox.UpperLeftPoint.X, currentBoundingBox.UpperLeftPoint.Y - r * cellHeight));
                hline.Vertices.Add(new Vertex(currentBoundingBox.UpperRightPoint.X, currentBoundingBox.UpperLeftPoint.Y - r * cellHeight));
                rtn.Lines.Add(hline);

                if (r == drawingData.Rows.Count - 1)
                {
                    r++;
                    hline = new LineShape();
                    hline.Vertices.Add(new Vertex(currentBoundingBox.UpperLeftPoint.X, currentBoundingBox.UpperLeftPoint.Y - r * cellHeight));
                    hline.Vertices.Add(new Vertex(currentBoundingBox.UpperRightPoint.X, currentBoundingBox.UpperLeftPoint.Y - r * cellHeight));
                    rtn.Lines.Add(hline);
                }
            }
           
            for (int c = 0; c < drawingData.Columns.Count; c++)
            {
                LineShape hline = new LineShape();
                hline.Vertices.Add(new Vertex(currentBoundingBox.UpperLeftPoint.X + c * cellWidth, currentBoundingBox.UpperLeftPoint.Y));
                hline.Vertices.Add(new Vertex(currentBoundingBox.LowerLeftPoint.X + c * cellWidth, currentBoundingBox.LowerLeftPoint.Y));
                rtn.Lines.Add(hline);

                if (c == drawingData.Columns.Count - 1)
                {
                    c++;
                    hline = new LineShape();
                    hline.Vertices.Add(new Vertex(currentBoundingBox.UpperLeftPoint.X + c * cellWidth, currentBoundingBox.UpperLeftPoint.Y));
                    hline.Vertices.Add(new Vertex(currentBoundingBox.LowerLeftPoint.X + c * cellWidth, currentBoundingBox.LowerLeftPoint.Y));
                    rtn.Lines.Add(hline);
                }
            }

            return rtn;
        }

        public string TruncateString(string cellText, double scaledStringLength)
        {
            string cellDrawingText = cellText;
            int drawingStringLength = (int)scaledStringLength;
            int cellLength = cellText.Length;
            switch (CellTextJustification)
            {
                case LabelTextJustification.Left:
                    cellDrawingText = cellText.Remove(drawingStringLength);
                    break;
                case LabelTextJustification.Center:
                    int truncatedStringLength = cellLength - drawingStringLength;
                    int endTruncateLength = truncatedStringLength / 2;

                    int startTruncateLength = endTruncateLength;
                    if (truncatedStringLength % 2 != 0)
                    {
                        startTruncateLength = endTruncateLength + 1;
                    }

                    cellDrawingText = cellText.Remove(cellLength - endTruncateLength);
                    cellDrawingText = cellDrawingText.Remove(0, startTruncateLength);
                    break;
                case LabelTextJustification.Right:
                    cellDrawingText = cellText.Remove(0, cellLength - drawingStringLength);
                    break;
                default:
                    break;
            }
            return cellDrawingText;
        }

        public DataTable GetDataWithTitle()
        {
            DataTable rtn = new DataTable();
            foreach (DataColumn column in DataTable.Columns)
            {
                rtn.Columns.Add(column.ColumnName);
            }

            DataRow dataRow = rtn.NewRow();
            for (int i = 0; i < rtn.Columns.Count; i++)
            {
                dataRow[i] = rtn.Columns[i].ColumnName;
            }
            rtn.Rows.Add(dataRow);

            foreach (DataRow dr in DataTable.Rows)
            {
                rtn.Rows.Add(dr.ItemArray);
            }

            return rtn;
        }


    }
}
