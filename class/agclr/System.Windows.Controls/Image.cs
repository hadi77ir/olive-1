//
// System.Windows.Controls.Image
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
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
using System.Windows;
using Mono;

namespace System.Windows.Controls {

	public class Image : MediaBase {

		public static readonly DependencyProperty DownloadProgressProperty =
			DependencyProperty.Lookup (Kind.IMAGE, "DownloadProgress", typeof (double));


		public Image ()  : base (NativeMethods.image_new ())
		{
			NativeMethods.base_ref (native);
		}

		internal Image (IntPtr raw) : base (raw)
		{
		}

		public double DownloadProgress {
			get { return (double) GetValue (DownloadProgressProperty); }
			set { SetValue (DownloadProgressProperty, value); }
		}

		public void SetSource (DependencyObject Downloader, string PartName)
		{
			if (Downloader == null)
				throw new ArgumentNullException ("Downloader");

			if (PartName == null) {
				// FIXME: this means we must transfer data from Downloader object to this image
				// hopefully using an API saner than returning a string
			} else {
				Downloader dl = (Downloader as Downloader);
				if (dl != null) {
					dl.Completed += delegate {
						End ();
					};

					NativeMethods.image_set_source (native, dl.native, PartName);
				}
				// FIXME: else not sure how to (or if) handle non-Downloader objects
			}
		}

		public event ErrorEventHandler ImageFailed;


		protected internal override Kind GetKind ()
		{
			return Kind.IMAGE;
		}
	}
}