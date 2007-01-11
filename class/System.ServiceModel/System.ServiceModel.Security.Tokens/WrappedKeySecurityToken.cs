//
// WrappedKeySecurityToken.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;

namespace System.ServiceModel.Security.Tokens
{
	public class WrappedKeySecurityToken : SecurityToken
	{
		string id;
		byte [] key;
		string wrap_alg;
		SecurityToken wrap_token;
		SecurityKeyIdentifier wrap_token_ref;
		DateTime valid_from = DateTime.Now.ToUniversalTime ();
		ReadOnlyCollection<SecurityKey> keys;
		ReferenceList reference_list;

		public WrappedKeySecurityToken (
			string id,
			byte [] keyToWrap,
			string wrappingAlgorithm,
			SecurityToken wrappingToken,
			SecurityKeyIdentifier wrappingTokenReference)
		{
			if (id == null)
				throw new ArgumentNullException ("id");
			if (keyToWrap == null)
				throw new ArgumentNullException ("keyToWrap");
			if (wrappingAlgorithm == null)
				throw new ArgumentNullException ("wrappingAlgorithm");
			if (wrappingToken == null)
				throw new ArgumentNullException ("wrappingToken");

			this.id = id;
			wrap_alg = wrappingAlgorithm;
			wrap_token = wrappingToken;
			wrap_token_ref = wrappingTokenReference;
			Collection<SecurityKey> l = new Collection<SecurityKey> ();
			foreach (SecurityKey sk in wrappingToken.SecurityKeys) {
				if (sk.IsSupportedAlgorithm (wrappingAlgorithm)) {
					key = sk.EncryptKey (wrappingAlgorithm, keyToWrap);
					l.Add (new InMemorySymmetricSecurityKey (key));
					break;
				}
			}
			keys = new ReadOnlyCollection<SecurityKey> (l);
			if (key == null)
				throw new ArgumentException (String.Format ("None of the security keys in the argument token supports specified wrapping algorithm '{0}'", wrappingAlgorithm));
		}

		// FIXME: it is kind of compromised solution to output
		// ReferenceList inside e:EncryptedKey and might disappear
		// when non-wrapped key is represented by another token type.
		internal ReferenceList ReferenceList {
			get { return reference_list; }
			set { reference_list = value; }
		}

		public override DateTime ValidFrom {
			get { return valid_from; }
		}

		public override DateTime ValidTo {
			get { return DateTime.MaxValue.AddDays (-1); }
		}

		public override string Id {
			get { return id; }
		}

		public override ReadOnlyCollection<SecurityKey> SecurityKeys {
			get { return keys; }
		}

		public string WrappingAlgorithm {
			get { return wrap_alg; }
		}

		public SecurityToken WrappingToken {
			get { return wrap_token; }
		}

		public SecurityKeyIdentifier WrappingTokenReference {
			get { return wrap_token_ref; }
		}

		public byte [] GetWrappedKey ()
		{
			return (byte []) key.Clone ();
		}

		public override bool CanCreateKeyIdentifierClause<T> ()
		{
			foreach (SecurityKeyIdentifierClause k in WrappingTokenReference) {
				Type t = k.GetType ();
				if (t == typeof (T) || t.IsSubclassOf (typeof (T)))
					return true;
			}
			return false;
		}

		public override T CreateKeyIdentifierClause<T> ()
		{
			foreach (SecurityKeyIdentifierClause k in WrappingTokenReference) {
				Type t = k.GetType ();
				if (t == typeof (T) || t.IsSubclassOf (typeof (T)))
					return (T) k;
			}
			throw new InvalidOperationException (String.Format ("WrappedKeySecurityToken cannot create '{0}'. The WrappingTokenReference type is '{1}'.", typeof (T), WrappingTokenReference.GetType ()));
		}

		[MonoTODO]
		public override bool MatchesKeyIdentifierClause (SecurityKeyIdentifierClause keyIdentifierClause)
		{
			if (keyIdentifierClause is LocalIdKeyIdentifierClause &&
			    ((LocalIdKeyIdentifierClause) keyIdentifierClause).LocalId == Id)
				return true;
			// FIXME: no other possibilities to match?
			return false;
		}
	}
}
