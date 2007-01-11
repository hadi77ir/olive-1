//
// WSSecurityTokenSerializer.cs
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
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel.Security.Tokens;
using System.Xml;

namespace System.ServiceModel.Security
{
	public class WSSecurityTokenSerializer : SecurityTokenSerializer
	{
		static WSSecurityTokenSerializer default_instance =
			new WSSecurityTokenSerializer ();

		public static WSSecurityTokenSerializer DefaultInstance {
			get { return default_instance; }
		}

		const int defaultOffset = 64,
			defaultLabelLength = 128,
			defaultNonceLength = 128;

		public WSSecurityTokenSerializer ()
			: this (false)
		{
		}

		public WSSecurityTokenSerializer (bool emitBspRequiredAttributes)
			: this (SecurityVersion.WSSecurity11, emitBspRequiredAttributes)
		{
		}

		public WSSecurityTokenSerializer (SecurityVersion securityVersion)
			: this (securityVersion, false)
		{
		}

		public WSSecurityTokenSerializer (SecurityVersion securityVersion, bool emitBspRequiredAttributes)
			: this (securityVersion, emitBspRequiredAttributes, new SamlSerializer ())
		{
		}

		public WSSecurityTokenSerializer (
			SecurityVersion securityVersion,
			bool emitBspRequiredAttributes,
			SamlSerializer samlSerializer)
			: this (securityVersion, emitBspRequiredAttributes, 
				samlSerializer, null, Type.EmptyTypes)
		{
		}

		public WSSecurityTokenSerializer (
			SecurityVersion securityVersion,
			bool emitBspRequiredAttributes,
			SamlSerializer samlSerializer,
			SecurityStateEncoder securityStateEncoder,
			IEnumerable<Type> knownTypes)
			: this (securityVersion, emitBspRequiredAttributes, 
				samlSerializer, securityStateEncoder,
				knownTypes, defaultOffset, defaultLabelLength,
				defaultNonceLength)
		{
		}
		
		public WSSecurityTokenSerializer (
			SecurityVersion securityVersion,
			bool emitBspRequiredAttributes,
			SamlSerializer samlSerializer,
			SecurityStateEncoder securityStateEncoder,
			IEnumerable<Type> knownTypes,
			int maximumKeyDerivationOffset,
			int maximumKeyDerivationLabelLength,
			int maximumKeyDerivationNonceLength)
		{
			security_version = securityVersion;
			emit_bsp = emitBspRequiredAttributes;
			saml_serializer = samlSerializer;
			encoder = securityStateEncoder;
			known_types = new List<Type> (knownTypes);
			max_offset = maximumKeyDerivationOffset;
			max_label_length = maximumKeyDerivationLabelLength;
			max_nonce_length = maximumKeyDerivationNonceLength;
		}

		SecurityVersion security_version;
		bool emit_bsp;
		SamlSerializer saml_serializer;
		SecurityStateEncoder encoder;
		List<Type> known_types;
		int max_offset, max_label_length, max_nonce_length;

		bool WSS1_0 {
			get { return SecurityVersion == SecurityVersion.WSSecurity10; }
		}

		[MonoTODO]
		public bool EmitBspRequiredAttributes {
			get { return emit_bsp; }
		}

		public SecurityVersion SecurityVersion {
			get { return security_version; }
		}

		[MonoTODO]
		public int MaximumKeyDerivationOffset {
			get { return max_offset; }
		}

		[MonoTODO]
		public int MaximumKeyDerivationLabelLength {
			get { return max_label_length; }
		}

		[MonoTODO]
		public int MaximumKeyDerivationNonceLength {
			get { return max_nonce_length; }
		}

		protected virtual string GetTokenTypeUri (Type tokenType)
		{
			if (tokenType == typeof (X509SecurityToken))
				return Constants.WSSX509Token;
//			if (tokenType == typeof (RsaSecurityToken))
//				return null;
			if (tokenType == typeof (SamlSecurityToken))
				return Constants.WSSSamlToken;
			if (tokenType == typeof (SecurityContextSecurityToken))
				return Constants.WsscContextToken;
//			if (tokenType == typeof (huh))
//				return ServiceModelSecurityTokenTypes.SecureConversation;
//			if (tokenType == typeof (hah))
//				return ServiceModelSecurityTokenTypes.MutualSslnego;
//			if (tokenType == typeof (whoa))
//				return ServiceModelSecurityTokenTypes.AnonymousSslnego;
			if (tokenType == typeof (UserNameSecurityToken))
				return Constants.WSSUserNameToken;
//			if (tokenType == typeof (uhoh))
//				return ServiceModelSecurityTokenTypes.Spnego;
//			if (tokenType == typeof (SspiSecurityToken))
//				return ServiceModelSecurityTokenTypes.SspiCredential;
			if (tokenType == typeof (KerberosRequestorSecurityToken))
				return Constants.WSSKerberosToken;
			return null;
		}

		[MonoTODO]
		protected override bool CanReadKeyIdentifierClauseCore (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool CanReadKeyIdentifierCore (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool CanReadTokenCore (XmlReader reader)
		{
			switch (reader.NamespaceURI) {
			case Constants.WssNamespace:
				switch (reader.LocalName) {
				case "BinarySecurityToken":
				case "BinarySecret":
				case "UserNameToken":
					return true;
				}
				break;
			}
			return false;
		}

		[MonoTODO]
		protected override SecurityKeyIdentifier ReadKeyIdentifierCore (
			XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore (XmlReader reader)
		{
			reader.MoveToContent ();
			switch (reader.NamespaceURI) {
			case EncryptedXml.XmlEncNamespaceUrl:
				switch (reader.LocalName) {
				case "EncryptedKey":
					return ReadEncryptedKeyIdentifierClause (reader);
				}
				break;
			case Constants.WssNamespace:
				switch (reader.LocalName) {
				case "SecurityTokenReference":
					reader.ReadStartElement ();
					reader.MoveToContent ();
					if (reader.LocalName != "Reference" || reader.NamespaceURI != Constants.WssNamespace)
						throw new XmlException (String.Format ("Unexpected SecurityTokenReference content: expected local name 'Reference' and namespace URI '{0}' but found local name '{1}' and namespace '{2}'.", Constants.WssNamespace, reader.LocalName, reader.NamespaceURI));
					string uri = reader.GetAttribute ("URI");
					if (uri == null)
						uri = "#";
					LocalIdKeyIdentifierClause local = new LocalIdKeyIdentifierClause (uri.Substring (1));
					reader.Skip ();
					reader.MoveToContent ();
					reader.ReadEndElement ();
					return local;
				}
				break;
			}

			throw new NotImplementedException (String.Format ("Security key identifier clause element '{0}' in namespace '{1}' is either not implemented or not supported.", reader.LocalName, reader.NamespaceURI));
		}

		EncryptedKeyIdentifierClause ReadEncryptedKeyIdentifierClause (
			XmlReader reader)
		{
			string encNS = EncryptedXml.XmlEncNamespaceUrl;

			string id = reader.GetAttribute ("Id", Constants.WsuNamespace);
			reader.Read ();
			reader.MoveToContent ();
			string encMethod = reader.GetAttribute ("Algorithm");
			bool isEmpty = reader.IsEmptyElement;
			reader.ReadStartElement ("EncryptionMethod", encNS);
			string digMethod = null;
			if (!isEmpty) {
				reader.MoveToContent ();
				if (reader.LocalName == "DigestMethod" && reader.NamespaceURI == SignedXml.XmlDsigNamespaceUrl)
					digMethod = reader.GetAttribute ("Algorithm");
				while (reader.NodeType != XmlNodeType.EndElement) {
					reader.Skip ();
					reader.MoveToContent ();
				}
				reader.ReadEndElement ();
			}
			reader.MoveToContent ();
			SecurityKeyIdentifier ki = null;
			if (!reader.IsEmptyElement) {
				reader.ReadStartElement ("KeyInfo", SignedXml.XmlDsigNamespaceUrl);
				reader.MoveToContent ();
				SecurityKeyIdentifierClause kic = ReadKeyIdentifierClauseCore (reader);
				ki = new SecurityKeyIdentifier ();
				ki.Add (kic);
				reader.MoveToContent ();
				reader.ReadEndElement (); // </ds:KeyInfo>
				reader.MoveToContent ();
			}
			byte [] keyValue = null;
			if (!reader.IsEmptyElement) {
				reader.ReadStartElement ("CipherData", encNS);
				reader.MoveToContent ();
				keyValue = Convert.FromBase64String (reader.ReadElementContentAsString ("CipherValue", encNS));
				reader.MoveToContent ();
				reader.ReadEndElement (); // CipherData
			}
			string carriedKeyName = null;
			if (!reader.IsEmptyElement && reader.LocalName == "CarriedKeyName" && reader.NamespaceURI == encNS) {
				carriedKeyName = reader.ReadElementContentAsString ();
				reader.MoveToContent ();
			}
			// FIXME: handle derived keys??
			return new EncryptedKeyIdentifierClause (keyValue, encMethod, ki, carriedKeyName);
		}

		[MonoTODO]
		protected override SecurityToken ReadTokenCore (
			XmlReader reader,
			SecurityTokenResolver tokenResolver)
		{
			if (!CanReadToken (reader))
				throw new InvalidOperationException (String.Format ("Cannot read security token from {0} node of name '{1}' and namespace URI '{2}'", reader.NodeType, reader.LocalName, reader.NamespaceURI));

			switch (reader.NamespaceURI) {
			case Constants.WssNamespace:
				switch (reader.LocalName) {
				case "BinarySecurityToken":
					return ReadX509TokenCore (reader, tokenResolver);
				case "BinarySecret":
					return ReadBinarySecretTokenCore (reader, tokenResolver);
				case "UserNameToken":
					return ReadUserNameTokenCore (reader, tokenResolver);
				}
				break;
			}

			throw new NotImplementedException ();
		}

		X509SecurityToken ReadX509TokenCore (
			XmlReader reader, SecurityTokenResolver resolver)
		{
			string id = reader.GetAttribute ("Id", Constants.WsuNamespace);
			byte [] raw = Convert.FromBase64String (reader.ReadElementContentAsString ());
			return new X509SecurityToken (new X509Certificate2 (raw), id);
		}

		UserNameSecurityToken ReadUserNameTokenCore (
			XmlReader reader, SecurityTokenResolver resolver)
		{
			throw new NotImplementedException ();
		}

		BinarySecretSecurityToken ReadBinarySecretTokenCore (
			XmlReader reader, SecurityTokenResolver resolver)
		{
			string id = reader.GetAttribute ("Id", Constants.WsuNamespace);
			byte [] data = Convert.FromBase64String (reader.ReadElementContentAsString ());
			return new BinarySecretSecurityToken (id, data);
		}

		[MonoTODO]
		protected override bool CanWriteKeyIdentifierCore (
			SecurityKeyIdentifier keyIdentifier)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool CanWriteKeyIdentifierClauseCore (
			SecurityKeyIdentifierClause keyIdentifierClause)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool CanWriteTokenCore (SecurityToken token)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void WriteKeyIdentifierCore (
			XmlWriter writer,
			SecurityKeyIdentifier keyIdentifier)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void WriteKeyIdentifierClauseCore (
			XmlWriter writer,
			SecurityKeyIdentifierClause keyIdentifierClause)
		{
			string errorReason = null;

			if (keyIdentifierClause == null)
				throw new ArgumentNullException ("keyIdentifierClause");
			if (keyIdentifierClause is LocalIdKeyIdentifierClause)
				WriteLocalIdKeyIdentifierClause (writer, (LocalIdKeyIdentifierClause) keyIdentifierClause);
			else if (keyIdentifierClause is X509IssuerSerialKeyIdentifierClause)
				WriteX509IssuerSerialKeyIdentifierClause (writer, (X509IssuerSerialKeyIdentifierClause) keyIdentifierClause);
			else if (keyIdentifierClause is X509ThumbprintKeyIdentifierClause) {
				if (WSS1_0)
					errorReason = String.Format ("Security key identifier clause '{0}' is not supported in this serializer.", keyIdentifierClause.GetType ());
				else
					WriteX509ThumbprintKeyIdentifierClause (writer, (X509ThumbprintKeyIdentifierClause) keyIdentifierClause);
			}
			else if (keyIdentifierClause is EncryptedKeyIdentifierClause)
				WriteEncryptedKeyIdentifierClause (writer, (EncryptedKeyIdentifierClause) keyIdentifierClause);
			else
				throw new NotImplementedException (String.Format ("Security key identifier clause '{0}' is not either implemented or supported.", keyIdentifierClause.GetType ()));

			if (errorReason != null)
				throw new InvalidOperationException (errorReason);
		}

		void WriteX509IssuerSerialKeyIdentifierClause (
			XmlWriter w, X509IssuerSerialKeyIdentifierClause ic)
		{
			w.WriteStartElement ("o", "SecurityTokenReference", Constants.WssNamespace);
			w.WriteStartElement ("X509Data", Constants.XmlDsig);
			w.WriteStartElement ("X509IssuerSerial", Constants.XmlDsig);
			w.WriteStartElement ("X509IssuerName", Constants.XmlDsig);
			w.WriteString (ic.IssuerName);
			w.WriteEndElement ();
			w.WriteStartElement ("X509SerialNumber", Constants.XmlDsig);
			w.WriteString (ic.IssuerSerialNumber);
			w.WriteEndElement ();
			w.WriteEndElement ();
			w.WriteEndElement ();
			w.WriteEndElement ();
		}

		void WriteX509ThumbprintKeyIdentifierClause (
			XmlWriter w, X509ThumbprintKeyIdentifierClause ic)
		{
			w.WriteStartElement ("o", "SecurityTokenReference", Constants.WssNamespace);
			w.WriteStartElement ("o", "KeyIdentifier", Constants.WssNamespace);
			w.WriteAttributeString ("ValueType", Constants.WssX509ThumbptintValueType);
			// FIXME: in some cases it seems to write this attribute...
			//w.WriteAttributeString ("EncodingType", Constants.WssBase64BinaryEncodingType);
			w.WriteString (Convert.ToBase64String (ic.GetX509Thumbprint ()));
			w.WriteEndElement ();
			w.WriteEndElement ();
		}

		void WriteLocalIdKeyIdentifierClause (
			XmlWriter w, LocalIdKeyIdentifierClause ic)
		{
			w.WriteStartElement ("o", "SecurityTokenReference", Constants.WssNamespace);
			w.WriteStartElement ("o", "Reference", Constants.WssNamespace);
			w.WriteAttributeString ("URI", "#" + ic.LocalId);
			w.WriteEndElement ();
			w.WriteEndElement ();
		}

		void WriteEncryptedKeyIdentifierClause (
			XmlWriter w, EncryptedKeyIdentifierClause ic)
		{
			w.WriteStartElement ("e", "EncryptedKey", EncryptedXml.XmlEncNamespaceUrl);
			w.WriteStartElement ("EncryptionMethod", EncryptedXml.XmlEncNamespaceUrl);
			w.WriteAttributeString ("Algorithm", ic.EncryptionMethod);
			w.WriteEndElement ();
			if (ic.EncryptingKeyIdentifier != null) {
				w.WriteStartElement ("KeyInfo", SignedXml.XmlDsigNamespaceUrl);
				foreach (SecurityKeyIdentifierClause ckic in ic.EncryptingKeyIdentifier)
					WriteKeyIdentifierClause (w, ckic);
				w.WriteEndElement ();
			}
			w.WriteStartElement ("CipherData", EncryptedXml.XmlEncNamespaceUrl);
			w.WriteStartElement ("CipherValue", EncryptedXml.XmlEncNamespaceUrl);
			w.WriteString (Convert.ToBase64String (ic.GetEncryptedKey ()));
			w.WriteEndElement ();
			w.WriteEndElement ();
			if (ic.CarriedKeyName != null)
				w.WriteElementString ("CarriedKeyName", EncryptedXml.XmlEncNamespaceUrl, ic.CarriedKeyName);
			w.WriteEndElement ();
		}

		[MonoTODO]
		protected override void WriteTokenCore (
			XmlWriter writer, SecurityToken token)
		{
			// WSSecurity supports:
			//	- UsernameToken : S.IM.T.UserNameSecurityToken
			//	- X509SecurityToken : S.IM.T.X509SecurityToken
			//	- SAML Assertion : S.IM.T.SamlSecurityToken
			//	- Kerberos : S.IM.T.KerberosRequestorSecurityToken
			//	- Rights Expression Language (REL) : N/A
			//	- SOAP with Attachments : N/A
			// additionally there are extra token types in WCF:
			//	- GenericXml
			//	- Windows
			//	- BinarySecret
			//	- SecurityContext
			//	- Sspi
			//	- WrappedKey
			// not supported in this class:
			//	- Rsa

			if (token is UserNameSecurityToken)
				WriteUserNameSecurityToken (writer, ((UserNameSecurityToken) token));
			else if (token is X509SecurityToken)
				WriteX509SecurityToken (writer, ((X509SecurityToken) token));
			else if (token is BinarySecretSecurityToken)
				WriteBinarySecretSecurityToken (writer, ((BinarySecretSecurityToken) token));
			else if (token is SamlSecurityToken)
				throw new NotImplementedException ("WriteTokenCore() is not implemented for " + token);
			else if (token is GenericXmlSecurityToken)
				throw new NotImplementedException ("WriteTokenCore() is not implemented for " + token);
			else if (token is WrappedKeySecurityToken)
				WriteWrappedKeySecurityToken (writer, (WrappedKeySecurityToken) token);
			else if (token is SecurityContextSecurityToken)
				throw new NotImplementedException ("WriteTokenCore() is not implemented for " + token);
			else if (token is SspiSecurityToken)
				throw new NotImplementedException ("WriteTokenCore() is not implemented for " + token);
			else if (token is KerberosRequestorSecurityToken)
				throw new NotImplementedException ("WriteTokenCore() is not implemented for " + token);
			else if (token is WindowsSecurityToken)
				throw new NotImplementedException ("WriteTokenCore() is not implemented for " + token);
			else
				throw new InvalidOperationException (String.Format ("This SecurityTokenSerializer does not support security token '{0}'.", token));
		}

		void WriteUserNameSecurityToken (XmlWriter w, UserNameSecurityToken token)
		{
			w.WriteStartElement ("o", "UsernameToken", Constants.WssNamespace);
			w.WriteAttributeString ("u", "Id", Constants.WsuNamespace, token.Id);
			w.WriteStartElement ("o", "Username", Constants.WssNamespace);
			w.WriteString (token.UserName);
			w.WriteEndElement ();
			w.WriteStartElement ("o", "Password", Constants.WssNamespace);
			w.WriteString (token.Password);
			w.WriteEndElement ();
			w.WriteEndElement ();
		}

		void WriteX509SecurityToken (XmlWriter w, X509SecurityToken token)
		{
			w.WriteStartElement ("o", "BinarySecurityToken", Constants.WssNamespace);
			w.WriteAttributeString ("u", "Id", Constants.WsuNamespace, token.Id);
			w.WriteAttributeString ("ValueType", Constants.WSSX509Token);
			w.WriteString (Convert.ToBase64String (token.Certificate.RawData));
			w.WriteEndElement ();
		}

		void WriteBinarySecretSecurityToken (XmlWriter w, BinarySecretSecurityToken token)
		{
			w.WriteStartElement ("t", "BinarySecret", Constants.WstNamespace);
			w.WriteAttributeString ("u", "Id", Constants.WsuNamespace, token.Id);
			w.WriteString (Convert.ToBase64String (token.GetKeyBytes ()));
			w.WriteEndElement ();
		}

		void WriteWrappedKeySecurityToken (XmlWriter w, WrappedKeySecurityToken token)
		{
			string encNS = EncryptedXml.XmlEncNamespaceUrl;
			w.WriteStartElement ("e", "EncryptedKey", encNS);
			w.WriteAttributeString ("Id", token.Id);
			w.WriteStartElement ("EncryptionMethod", encNS);
			w.WriteAttributeString ("Algorithm", token.WrappingAlgorithm);
			w.WriteStartElement ("DigestMethod", SignedXml.XmlDsigNamespaceUrl);
			w.WriteAttributeString ("Algorithm", SignedXml.XmlDsigSHA1Url);
			w.WriteEndElement ();
			w.WriteEndElement ();

			w.WriteStartElement ("KeyInfo", SignedXml.XmlDsigNamespaceUrl);
			if (token.WrappingTokenReference != null)
				foreach (SecurityKeyIdentifierClause kic in token.WrappingTokenReference)
					WriteKeyIdentifierClause (w, kic);
			w.WriteEndElement ();
			w.WriteStartElement ("CipherData", encNS);
			w.WriteStartElement ("CipherValue", encNS);
			w.WriteString (Convert.ToBase64String (token.GetWrappedKey ()));
			w.WriteEndElement ();
			w.WriteEndElement ();
			if (token.ReferenceList != null) {
				w.WriteStartElement ("e", "ReferenceList", encNS);
				foreach (DataReference er in token.ReferenceList) {
					w.WriteStartElement ("DataReference", encNS);
					w.WriteAttributeString ("URI", er.Uri);
					w.WriteEndElement ();
				}
				w.WriteEndElement ();
			}
			w.WriteEndElement ();
		}
	}
}
