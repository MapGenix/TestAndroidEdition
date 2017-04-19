package md5c5236f64892ab177e164889723751dea;


public class MapPinchGestureManager
	extends android.view.ScaleGestureDetector.SimpleOnScaleGestureListener
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onScale:(Landroid/view/ScaleGestureDetector;)Z:GetOnScale_Landroid_view_ScaleGestureDetector_Handler\n" +
			"";
		mono.android.Runtime.register ("Mapgenix.GSuite.Android.MapPinchGestureManager, Mapgenix.GSuite.Android, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", MapPinchGestureManager.class, __md_methods);
	}


	public MapPinchGestureManager () throws java.lang.Throwable
	{
		super ();
		if (getClass () == MapPinchGestureManager.class)
			mono.android.TypeManager.Activate ("Mapgenix.GSuite.Android.MapPinchGestureManager, Mapgenix.GSuite.Android, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public boolean onScale (android.view.ScaleGestureDetector p0)
	{
		return n_onScale (p0);
	}

	private native boolean n_onScale (android.view.ScaleGestureDetector p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
