
/* this file is generated by gen-animation-types.cs.  do not modify */

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using NUnit.Framework;

namespace MonoTests.System.Windows.Media.Animation {


[TestFixture]
public class RectAnimationBaseTest
{
	class RectAnimationBasePoker : RectAnimationBase
	{
		protected override Rect GetCurrentValueCore (Rect defaultOriginValue, Rect defaultDestinationValue,
							    AnimationClock animationClock)
		{
			throw new NotImplementedException ();
		}

		protected override Freezable CreateInstanceCore ()
		{
			throw new NotImplementedException ();
		}
	}

	[Test]
	public void Properties ()
	{
		RectAnimationBasePoker poker = new RectAnimationBasePoker ();
		Assert.AreEqual (typeof (Rect), poker.TargetPropertyType);
	}
}


}