//
// WebServiceHost.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

// This class does:
// - manual addressing support (with ChannelFactory, client will fail with
//   InvalidOperationException that claims missing manual addressing) in every
//   messages.

namespace System.ServiceModel.Web
{
	public class WebServiceHost : ServiceHost
	{
		Type serviceType = null;
		public WebServiceHost ()
			: base ()
		{
		}

		public WebServiceHost (object singletonInstance, params Uri [] baseAddresses)
			: base (singletonInstance, baseAddresses)
		{
		}

		public WebServiceHost (Type serviceType, params Uri [] baseAddresses)
			: base (serviceType, baseAddresses)
		{
			this.serviceType = serviceType;
		}

		protected override void OnOpening ()
		{
			foreach (Uri baseAddress in BaseAddresses) {
				bool found = false;
				foreach (ServiceEndpoint se in Description.Endpoints)
					if (se.Address.Uri == baseAddress)
						found = true;
				if (!found)
					if (SingletonInstance != null)
						AddServiceEndpoint (SingletonInstance.GetType(), new WebHttpBinding (), baseAddress.ToString());
					else
						AddServiceEndpoint (serviceType, new WebHttpBinding (), baseAddress);
			}

			// disable help page.
			ServiceDebugBehavior serviceDebugBehavior = Description.Behaviors.Find<ServiceDebugBehavior> ();
			if (serviceDebugBehavior != null) {
				serviceDebugBehavior.HttpHelpPageEnabled = false;
				serviceDebugBehavior.HttpsHelpPageEnabled = false;
			}
		}
	}
}
