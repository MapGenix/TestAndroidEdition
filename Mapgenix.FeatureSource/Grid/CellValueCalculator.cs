using System.Threading;

namespace Mapgenix.FeatureSource
{
    public class CellValueCalculator
    {
        public long CellsToCalculate;

        public void CalculatGridCell(object objStateInfo)
        {
            CalculateGridCellStateInfo info = objStateInfo as CalculateGridCellStateInfo;
            double cellValue = info.Model.Interpolate(info.CellExtent, info.GridDefinition);
            Monitor.Enter(info.Cells);

            info.Cells[info.IndexY, info.IndexX] = new GridCell(info.CellExtent.GetCenterPoint().X, info.CellExtent.GetCenterPoint().Y, cellValue);

            Monitor.Exit(info.Cells);

            if (Interlocked.Decrement(ref CellsToCalculate) == 0)
            {
                info.Finished.Set();
            }
        }
    }
}