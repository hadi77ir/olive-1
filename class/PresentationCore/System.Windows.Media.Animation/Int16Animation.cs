
/* this file is generated by gen-animation-types.cs.  do not modify */

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;

namespace System.Windows.Media.Animation {


public class Int16Animation : Int16AnimationBase
{
	public static readonly DependencyProperty ByProperty; /* XXX initialize */
	public static readonly DependencyProperty FromProperty; /* XXX initialize */
	public static readonly DependencyProperty ToProperty; /* XXX initialize */

	public Int16Animation ()
	{
	}

	public Int16Animation (short toValue, Duration duration)
	{
	}

	public Int16Animation (short toValue, Duration duration, FillBehavior fillBehavior)
	{
	}

	public Int16Animation (short fromValue, short toValue, Duration duration)
	{
	}

	public Int16Animation (short fromValue, short tovalue, Duration duration, FillBehavior fillBehavior)
	{
	}

	public short? By {
		get { return (short?) GetValue (ByProperty); }
		set { SetValue (ByProperty, value); }
	}

	public short? From {
		get { return (short?) GetValue (FromProperty); }
		set { SetValue (FromProperty, value); }
	}

	public short? To {
		get { return (short?) GetValue (ToProperty); }
		set { SetValue (ToProperty, value); }
	}

	public bool IsAdditive {
		get { return (bool) GetValue (AnimationTimeline.IsAdditiveProperty); }
		set { SetValue (AnimationTimeline.IsAdditiveProperty, value); }
	}

	public bool IsCumulative {
		get { return (bool) GetValue (AnimationTimeline.IsCumulativeProperty); }
		set { SetValue (AnimationTimeline.IsCumulativeProperty, value); }
	}

	public new Int16Animation Clone ()
	{
		throw new NotImplementedException ();
	}

	protected override Freezable CreateInstanceCore ()
	{
		throw new NotImplementedException ();
	}

	protected override short GetCurrentValueCore (short defaultOriginValue, short defaultDestinationValue, AnimationClock animationClock)
	{
		throw new NotImplementedException ();
	}
}


}