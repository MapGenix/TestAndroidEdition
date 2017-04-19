using System.Threading;
using Mapgenix.Shapes;

namespace Mapgenix.FeatureSource
{
    public class CalculateGridCellStateInfo
    {
        public long IndexX { get; set; }
        public long IndexY { get; set; }
        public GridCell[,] Cells { get; set; }
        public BaseGridInterpolationModel Model { get; set; }
        public RectangleShape CellExtent { get; set; }
        public GridDefinition GridDefinition { get; set; }
        public ManualResetEvent Finished { get; set; }

        public CalculateGridCellStateInfo() { }

        public CalculateGridCellStateInfo(long x, long y, RectangleShape cellExtent, GridCell[,] cells, BaseGridInterpolationModel model, GridDefinition definition, ManualResetEvent finished)
        {
            IndexX = x;
            IndexY = y;
            CellExtent = cellExtent;
            Cells = cells;
            Model = model;
            GridDefinition = definition;
            Finished = finished;
        }
    }
}
