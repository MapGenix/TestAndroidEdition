
namespace Mapgenix.FeatureSource
{
    public class FunctionThrough
    {
        private bool top;
        private bool left;
        private bool right;
        private bool bottom;

        public bool Top
        {
            get { return top; }
        }

        public bool Right
        {
            get { return right; }
        }

        public bool Bottom
        {
            get { return bottom; }
        }

        public bool Left
        {
            get { return left; }
        }
        

        public virtual void SetTop()
        {
            top = true;
            right = false;
            bottom = false;
            left = false;
        }

        public virtual void SetRight()
        {
            top = false;
            right = true;
            bottom = false;
            left = false;
        }

        public virtual void SetLeft()
        {
            top = false;
            right = false;
            bottom = false;
            left = true;
        }

        public virtual void SetBottom()
        {
            top = false;
            right = false;
            bottom = true;
            left = false;
        }
    }
}
