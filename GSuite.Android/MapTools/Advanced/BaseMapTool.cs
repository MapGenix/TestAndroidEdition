using Android.Content;
using Android.Views;
using Android.Widget;

namespace Mapgenix.GSuite.Android
{
    public abstract class BaseMapTool : RelativeLayout
    {
        private Map _currentMap;
        
      
        protected BaseMapTool(Context context)
            : this(context, false)
        { }

        protected BaseMapTool (Context context, bool isEnabled)
            :base(context)
        {
            Enabled = Enabled;
        }

        public Map CurrentMap
        {
            get { return _currentMap; }
            set { _currentMap = value; }
        }

        public void Initialize(Map map)
        {
            InitializeCore(map);
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }

            set
            {
                base.Enabled = value;
                if(Enabled)
                {
                    EnabledChangedCore(Enabled);
                }
            }
        }

        protected virtual void InitializeCore(Map map)
        {
            _currentMap = map;
            if (_currentMap.ToolsGrid.IndexOfChild(this) == -1)
            {
                _currentMap.ToolsGrid.AddView(this);
            }
        }

        protected virtual void EnabledChangedCore(bool newValue)
        {
            Visibility = newValue ? ViewStates.Visible : ViewStates.Invisible;
        }
    }
}
