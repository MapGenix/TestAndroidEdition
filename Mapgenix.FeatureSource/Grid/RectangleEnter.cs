
namespace Mapgenix.FeatureSource
{
    public class RectangleEnter
    {
        private const double Revise = 0.00001;
        private double pointValue;
        private GridCell pointResult;

        public RectangleEnter(double oldLevel)
        {
            pointValue = oldLevel;
        }

        public double PointValue
        {
            get { return pointValue; }
        }

        public GridCell PointResult
        {
            get { return pointResult; }
        }
        public bool JudgeLine(GridCell startPoint, GridCell endPoint)
        {
            if (startPoint != null && endPoint != null)
            {
                if (startPoint.Value > endPoint.Value)
                {
                    if (pointValue < startPoint.Value && pointValue > endPoint.Value)
                    {
                        GetEnterPoint(startPoint, endPoint, true);
                        return true;
                    }
                    else if (startPoint.Value.Equals(pointValue))
                    {
                        if (!startPoint.Value.Equals(0.0))
                        {
                            pointValue -= pointValue * Revise;
                        }
                        else
                        {
                            pointValue = pointValue - 0.000000000001;
                        }
                        GetEnterPoint(startPoint, endPoint, true);
                        return true;
                    }
                    else if (endPoint.Value.Equals(pointValue))
                    {
                        if (!endPoint.Value.Equals(0.0))
                        {
                            pointValue += pointValue * Revise;
                        }
                        else
                        {
                            pointValue = pointValue + 0.000000000001;
                        }
                        GetEnterPoint(startPoint, endPoint, true);
                        return true;
                    }
                    return false;
                }
                else if (startPoint.Value < endPoint.Value)
                {
                    if (pointValue > startPoint.Value && pointValue < endPoint.Value)
                    {
                        GetEnterPoint(startPoint, endPoint, false);
                        return true;
                    }
                    else if (startPoint.Value.Equals(pointValue))
                    {
                        if (!startPoint.Value.Equals(0.0))
                        {
                            pointValue += pointValue * Revise;
                        }
                        else
                        {
                            pointValue = pointValue + 0.000000000001;
                        }
                        GetEnterPoint(startPoint, endPoint, false);
                        return true;
                    }
                    else if (endPoint.Value.Equals(pointValue))
                    {
                        if (!endPoint.Value.Equals(0.0))
                        {
                            pointValue -= pointValue * Revise;
                        }
                        else
                        {
                            pointValue = pointValue - 0.000000000001;
                        }
                        GetEnterPoint(startPoint, endPoint, false);
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        public bool JudgeLine(double startValue, double endValue)
        {
            if (startValue > endValue)
            {
                if (pointValue < startValue && pointValue > endValue)
                {
                    return true;
                }
                else if (startValue.Equals(pointValue))
                {
                    if (!startValue.Equals(0.0))
                    {
                        pointValue -= pointValue * Revise;
                    }
                    else
                    {
                        pointValue = pointValue - 0.00000000000001;
                    }
                    return true;
                }
                else if (endValue.Equals(pointValue))
                {
                    if (!endValue.Equals(0.0))
                    {
                        pointValue += pointValue * Revise;
                    }
                    else
                    {
                        pointValue = pointValue + 0.00000000000001;
                    }
                    return true;
                }
                return false;
            }
            else if (startValue < endValue)
            {
                if (pointValue > startValue && pointValue < endValue)
                {
                    return true;
                }
                else if (startValue.Equals(pointValue))
                {
                    if (!startValue.Equals(0.0))
                    {
                        pointValue += pointValue * Revise;
                    }
                    else
                    {
                        pointValue = pointValue + 0.00000000000001;
                    }
                    return true;
                }
                else if (endValue.Equals(pointValue))
                {
                    if (!endValue.Equals(0.0))
                    {
                        pointValue -= pointValue * Revise;
                    }
                    else
                    {
                        pointValue = pointValue - 0.00000000000001;
                    }
                    return true;
                }
                return false;
            }
            return false;
        }

        private void GetEnterPoint(GridCell startPoint, GridCell endPoint, bool isStartLarge)
        {
            double PositionX;
            double PositionY;
            double Radio;
            double DvalueX = endPoint.CenterX - startPoint.CenterX;

            if (DvalueX.Equals(0))
            {
                PositionX = endPoint.CenterX;
            }
            else
            {
                if (isStartLarge.Equals(true))
                {
                    Radio = (pointValue - endPoint.Value) / (startPoint.Value - endPoint.Value);
                    PositionX = (1 - Radio) * endPoint.CenterX + Radio * startPoint.CenterX;
                }
                else
                {
                    Radio = (pointValue - startPoint.Value) / (endPoint.Value - startPoint.Value);
                    PositionX = (1 - Radio) * startPoint.CenterX + Radio * endPoint.CenterX;
                }
            }

            double DvalueY = endPoint.CenterY - startPoint.CenterY;
            if (DvalueY.Equals(0))
            {
                PositionY = endPoint.CenterY;
            }
            else
            {
                if (isStartLarge.Equals(true))
                {
                    Radio = (pointValue - endPoint.Value) / (startPoint.Value - endPoint.Value);
                    PositionY = (1 - Radio) * endPoint.CenterY + Radio * startPoint.CenterY;
                }
                else
                {
                    Radio = (pointValue - startPoint.Value) / (endPoint.Value - startPoint.Value);
                    PositionY = (1 - Radio) * startPoint.CenterY + Radio * endPoint.CenterY;
                }
            }
            pointResult = new GridCell(PositionX, PositionY, pointValue);
        }
    }
}
