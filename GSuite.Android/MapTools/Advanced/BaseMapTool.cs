using Android.Content;
using Android.Views;
using Android.Widget;

namespace Mapgenix.GSuite.Android
{
    public abstract class BaseMapTool : FrameLayout
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

        public void Initialize(Map wpfMap)
        {
            InitializeCore(wpfMap);
        }

        protected virtual void InitializeCore(Map wpfMap)
        {
            _currentMap = wpfMap;
            if (_currentMap.ToolsGrid.IndexOfChild(this) == -1)
            {
                _currentMap.ToolsGrid.AddView(this);
            }
        }

        protected virtual void EnabledChangedCore(bool newValue)
        {
            //Visibility = newValue ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            Visibility = newValue ? ViewStates.Visible : ViewStates.Invisible;
        }

        /*private void MapToolIsEnabledChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            EnabledChangedCore((bool)e.NewValue);
        }*/
    }
}
