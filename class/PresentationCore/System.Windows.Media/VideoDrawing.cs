// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Chris Toshok (toshok@ximian.com)
//

using System.Windows;

namespace System.Windows.Media {

	public class VideoDrawing : Drawing
	{

		public VideoDrawing ()
		{
		}

		public new VideoDrawing Clone ()
		{
			throw new NotImplementedException ();
		}

		public new VideoDrawing CloneCurrentValue ()
		{
			throw new NotImplementedException ();
		}

		protected override Freezable CreateInstanceCore ()
		{
			return new VideoDrawing ();
		}

		public static readonly DependencyProperty PlayerProperty;
		public MediaPlayer Player {
		    get { return (MediaPlayer)GetValue (PlayerProperty); }
		    set { SetValue (PlayerProperty, value); }
		}
		
		public static readonly DependencyProperty RectProperty;
		public Rect Rect {
		    get { return (Rect)GetValue (RectProperty); }
		    set { SetValue (RectProperty, value); }
		}
		
	}

}