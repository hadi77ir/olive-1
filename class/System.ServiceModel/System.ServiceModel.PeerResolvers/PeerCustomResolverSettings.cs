﻿// 
// PeerCustomResolverSettings.cs
// 
// Author: 
//     Marcos Cobena (marcoscobena@gmail.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// 

using System;
using System.ServiceModel.Channels;

namespace System.ServiceModel.PeerResolvers
{
	public class PeerCustomResolverSettings
	{
		EndpointAddress address;
		Binding binding;
		PeerResolver resolver;
		
		public PeerCustomResolverSettings()
		{
		}
		
		public EndpointAddress Address {
			get { return address; }
			set { address = value; }
		}
		
		public Binding Binding {
			get { return binding; }
			set { binding = value; }
		}
		
		public PeerResolver Resolver {
			get { return resolver; }
			set { resolver = value; }
		}
	}
}
