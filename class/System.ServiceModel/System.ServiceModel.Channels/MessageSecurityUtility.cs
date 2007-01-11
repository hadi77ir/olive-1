//
// MessageSecurityUtility.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Collections.ObjectModel;
using System.Globalization;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace System.ServiceModel.Channels
{
	internal static class MessageSecurityUtility
	{
		static SecurityKey ResolveKey (SecurityToken token, SecurityTokenParameters p)
		{
			SecurityKeyIdentifierClause clause =
				p.CallCreateKeyIdentifierClause (token, p.ReferenceStyle);
			return token.ResolveKeyIdentifierClause (clause);
		}

		public static Message SecureMessage (
			Message msg,
			ISecurityChannelSource security,
			EndpointAddress remoteAddress)
		{
			// FIXME: I believe it should be done at channel
			// creation phase, but WinFX does not.
			if (!security.Support.InitiatorParameters.InternalHasAsymmetricKey)
				throw new InvalidOperationException ("Wrong security token parameters: it must have an asymmetric key (HasAsymmetricKey). There is likely a misconfiguration in the custom security binding element.");

			return SecureMessage (
				msg,
				remoteAddress,
				security.Support,
				MessageDirection.Input);
		}

		public static Message SecureMessage (
			Message msg,
			EndpointAddress messageTo,
			MessageSecurityBindingSupport security,
			MessageDirection direction)
		{
			SecurityTokenParameters initiatorParams =
				security.InitiatorParameters;
			SecurityTokenParameters recipientParams =
				security.RecipientParameters;
			SecurityToken encToken =
				security.EncryptionToken;
			SecurityToken signToken =
				security.SigningToken;
			MessageProtectionOrder protectionOrder =
				security.MessageProtectionOrder;
			SecurityTokenSerializer serializer =
				security.TokenSerializer;
			SecurityBindingElement element =
				security.Element;
			ScopedMessagePartSpecification spec =
				direction == MessageDirection.Input ?
				security.ChannelRequirements.IncomingSignatureParts :
				security.ChannelRequirements.OutgoingSignatureParts;
			SecurityTokenParameters securingParams =
				direction == MessageDirection.Input ?
				initiatorParams : recipientParams;

			SupportingTokenInfoCollection tokens =
				security.CollectInitiatorSupportingTokens (msg.Headers.Action, messageTo);


			// SecurityTokenInclusionMode
			// - Initiator or Recipient
			// - done or notyet. FIXME: not implemented yet
			// It also affects on key reference output

			SecurityTokenInclusionMode appliedMode =
				securingParams.InclusionMode;
			bool msgIncludesToken = ShouldIncludeToken (appliedMode, direction, false);

			SecurityKeyIdentifierClause encClause =
				initiatorParams.CallCreateKeyIdentifierClause (encToken, msgIncludesToken ? initiatorParams.ReferenceStyle : SecurityTokenReferenceStyle.External);
			SecurityKeyIdentifierClause signClause =
				recipientParams.CallCreateKeyIdentifierClause (signToken, msgIncludesToken ? recipientParams.ReferenceStyle : SecurityTokenReferenceStyle.External);

			AsymmetricSecurityKey encKey = (AsymmetricSecurityKey) 
				encToken.ResolveKeyIdentifierClause (encClause);
			AsymmetricSecurityKey signKey = (AsymmetricSecurityKey) 
				signToken.ResolveKeyIdentifierClause (signClause);
			string messageId = "uuid:" + Guid.NewGuid ();
			int identForMessageId = 1;

			msg.Headers.Add (MessageHeader.CreateHeader ("MessageID", msg.Version.Addressing.Namespace, messageId));

			// FIXME: get correct ReplyTo value
			msg.Headers.Add (MessageHeader.CreateHeader ("ReplyTo", msg.Version.Addressing.Namespace, EndpointAddress10.FromEndpointAddress (new EndpointAddress (Constants.WsaAnonymousUri))));

			if (messageTo != null)
				msg.Headers.Add (MessageHeader.CreateHeader ("To", msg.Version.Addressing.Namespace, messageTo.Uri.AbsoluteUri, true));

			// addition except for wsse:Security is done. Start securing...

			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			XPathNavigator nav = doc.CreateNavigator ();
			using (XmlWriter w = nav.AppendChild ()) {
				msg.WriteMessage (w);
			}
			XmlNamespaceManager nsmgr = new XmlNamespaceManager (doc.NameTable);
			nsmgr.AddNamespace ("s", msg.Version.Envelope.Namespace);

			WrappedKeySecurityToken ekey = null;
			ReferenceList encRefList = null;
			Signature sig = null;
			EncryptedData sigenc = null;

			SecurityAlgorithmSuite suite = element.DefaultAlgorithmSuite;

			// wss:Security
			WSSecurityMessageHeader header =
				new WSSecurityMessageHeader (serializer);
			// wss:Security/wsu:Timestamp
			if (element.IncludeTimestamp) {
				WsuTimestamp timestamp = new WsuTimestamp ();
				timestamp.Id = messageId + "-" + identForMessageId++;
				timestamp.Created = DateTime.Now;
				// FIXME: on service side, use element.LocalServiceSettings.TimestampValidityDuration
				timestamp.Expires = timestamp.Created.Add (element.LocalClientSettings.TimestampValidityDuration);
				header.Contents.Add (timestamp);
			}
			msg.Headers.Add (header);

			// FIXME: it needs to choose message parts by 
			// ChannelProtectionRequirements.
			XmlElement body = doc.SelectSingleNode ("/s:Envelope/s:Body/*", nsmgr) as XmlElement;

			switch (protectionOrder) {
			case MessageProtectionOrder.EncryptBeforeSign:
				// FIXME: implement
				throw new NotImplementedException ();
			case MessageProtectionOrder.SignBeforeEncrypt:
			case MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature:

				MessagePartSpecification sigSpec;
				if (!spec.TryGetParts (GetAction (msg), false, out sigSpec))
					sigSpec = spec.ChannelParts;

				// encryption key (possibly also used for signing)
				// FIXME: get correct SymmetricAlgorithm according to the algorithm suite
				RijndaelManaged aes = new RijndaelManaged ();
				aes.KeySize = 256;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.ISO10126;

				// sign
				SignedXml sxml = new SignedXml (body);

				HMACSHA1 sigHash = null;
				if (security.DefaultSignatureAlgorithm == SignedXml.XmlDsigHMACSHA1Url)
					sigHash = new HMACSHA1 (aes.Key);
				else
					sxml.SigningKey = signKey.GetAsymmetricAlgorithm (security.DefaultSignatureAlgorithm, true);

				sig = sxml.Signature;
				sig.SignedInfo.CanonicalizationMethod =
					suite.DefaultCanonicalizationAlgorithm;
				foreach (XmlNode n in doc.SelectNodes ("/s:Envelope/s:Header", nsmgr)) {
					XmlElement el = n as XmlElement;
					if (el == null)
						continue;
					// FIXME: check sigSpec.HeaderTypes and skip it if not included.
					sig.SignedInfo.AddReference (CreateReference (el, suite));
				}
				foreach (XmlNode n in body.ChildNodes) {
					XmlElement el = n as XmlElement;
					if (el == null)
						continue;
					sig.SignedInfo.AddReference (CreateReference (el, suite));
				}
				if (sigHash != null)
					sxml.ComputeSignature (sigHash);
				else
					sxml.ComputeSignature ();
				sxml.KeyInfo = new KeyInfo ();
				SecurityTokenReferenceKeyInfo sigKeyInfo = new SecurityTokenReferenceKeyInfo (signClause, serializer, doc);
				sxml.KeyInfo.AddClause (sigKeyInfo);

				// encrypt
				string ekeyId = messageId + "-" + identForMessageId++;

				EncryptedXml exml = new EncryptedXml ();
				ekey = new WrappedKeySecurityToken (ekeyId,
					aes.Key,
					suite.DefaultAsymmetricKeyWrapAlgorithm,
					encToken,
					new SecurityKeyIdentifier (encClause));
				encRefList = new ReferenceList ();
				if (!initiatorParams.RequireDerivedKeys)
					ekey.ReferenceList = encRefList;

				EncryptedData edata = Encrypt (body, aes, suite, ekeyId, encRefList, encClause, serializer, exml, doc);
				edata.KeyInfo = null;
				EncryptedXml.ReplaceElement (body, edata, false);

				// encrypt signature
				if (protectionOrder == MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature) {
					XmlElement sigxml = sig.GetXml ();
					sigenc = Encrypt (sigxml, aes, suite, ekeyId, encRefList, encClause, serializer, exml, doc);
				}
				break;
			}

			Message ret = Message.CreateMessage (msg.Version, msg.Headers.Action, new XmlNodeReader (doc.SelectSingleNode ("/s:Envelope/s:Body/*", nsmgr) as XmlElement));

			ret.Headers.Clear ();
			ret.Headers.CopyHeadersFrom (msg);

			if (sig != null && msgIncludesToken)
				header.Contents.Add (signToken);
			if (signToken != encToken && msgIncludesToken)
				header.Contents.Add (encToken);

			if (ekey != null)
				header.Contents.Add (ekey);

			// generate derived key if needed
			if (initiatorParams.RequireDerivedKeys) {
				RijndaelManaged deriv = new RijndaelManaged ();
				deriv.KeySize = 128;
				deriv.Mode = CipherMode.CBC;
				deriv.Padding = PaddingMode.ISO10126;
				deriv.GenerateKey ();
				WsscDerivedKeyToken derivedKey =
					new WsscDerivedKeyToken ();
				derivedKey.Id = GenerateId (doc);
				derivedKey.Offset = 0;
				derivedKey.Nonce = deriv.Key;
				derivedKey.Length = derivedKey.Nonce.Length;
				if (ekey != null)
					derivedKey.SecurityTokenReference =
						new LocalIdKeyIdentifierClause (ekey.Id);
				header.Contents.Add (derivedKey);
				if (encRefList != null)
					header.Contents.Add (encRefList);
			}

			if (sigenc != null)
				header.Contents.Add (sigenc);
			else if (sig != null)
				header.Contents.Add (sig);

			return ret;
		}

		static bool ShouldIncludeToken (SecurityTokenInclusionMode mode,
			MessageDirection direction, bool isInitialized)
		{
			switch (mode) {
			case SecurityTokenInclusionMode.Never:
				return false;
			case SecurityTokenInclusionMode.AlwaysToInitiator:
				return direction == MessageDirection.Output;
			case SecurityTokenInclusionMode.AlwaysToRecipient:
				return direction == MessageDirection.Input;
			case SecurityTokenInclusionMode.Once:
				return !isInitialized;
			}
			throw new Exception ("Internal Error: should not happen.");
		}

		static Reference CreateReference (XmlElement el, SecurityAlgorithmSuite suite)
		{
			if (el.GetAttribute ("Id") == String.Empty)
				el.SetAttribute ("Id", GenerateId (el.OwnerDocument));
			Reference r = new Reference ("#" + el.GetAttribute ("Id"));
			r.AddTransform (CreateTransform (suite.DefaultCanonicalizationAlgorithm));
			r.DigestMethod = suite.DefaultDigestAlgorithm;
			return r;
		}

		static Transform CreateTransform (string url)
		{
			switch (url) {
			case SignedXml.XmlDsigC14NTransformUrl:
				return new XmlDsigC14NTransform ();
			case SignedXml.XmlDsigC14NWithCommentsTransformUrl:
				return new XmlDsigC14NWithCommentsTransform ();
			case SignedXml.XmlDsigExcC14NTransformUrl:
				return new XmlDsigExcC14NTransform ();
			case SignedXml.XmlDsigExcC14NWithCommentsTransformUrl:
				return new XmlDsigExcC14NWithCommentsTransform ();
			}
			throw new Exception (String.Format ("INTERNAL ERROR: Invalid canonicalization URL: {0}", url));
		}

		static EncryptedData Encrypt (XmlElement target, SymmetricAlgorithm aes, SecurityAlgorithmSuite suite, string ekeyId, ReferenceList refList, SecurityKeyIdentifierClause encClause, SecurityTokenSerializer serializer, EncryptedXml exml, XmlDocument doc)
		{
			byte [] encrypted = exml.EncryptData (target, aes, false);
			EncryptedData edata = new EncryptedData ();
			edata.Id = GenerateId (doc);
			edata.Type = EncryptedXml.XmlEncElementUrl;
			edata.EncryptionMethod = new EncryptionMethod (suite.DefaultEncryptionAlgorithm);
			// FIXME: here wsse:DigestMethod should be embedded 
			// inside EncryptionMethod. Since it is not possible 
			// with S.S.C.Xml.EncryptionMethod, we will have to
			// build our own XML encryption classes.

			edata.KeyInfo = new KeyInfo ();
			LocalIdKeyIdentifierClause ident =
				new LocalIdKeyIdentifierClause (ekeyId);
			KeyInfoClause kic = new SecurityTokenReferenceKeyInfo (ident, serializer, doc);
			edata.KeyInfo.AddClause (kic);
			edata.CipherData.CipherValue = encrypted;

			DataReference dr = new DataReference ();
			dr.Uri = "#" + edata.Id;
			refList.Add (dr);
			return edata;
		}

		static string GenerateId (XmlDocument doc)
		{
			int i = 1;
			while (doc.SelectSingleNode (String.Format (CultureInfo.InvariantCulture, "//*[@Id = '_{0}']", i)) != null ||
			       doc.SelectSingleNode (String.Format (CultureInfo.InvariantCulture, "//*[@Id = '_{0}']", i)) != null)
				i++;
			return "_" + i;
		}

		// Decryption

		public static Message DecryptMessage (Message inputMessage,
			MessageSecurityBindingSupport security)
		{
			SecurityBindingElement element =
				security.Element;
			SecurityTokenSerializer serializer =
				security.TokenSerializer;
			SecurityTokenResolver resolver =
				security.OutOfBandTokenResolver;
			SecurityToken token =
				security.EncryptionToken;
			SecurityTokenParameters parameters =
				security.RecipientParameters;
			MessageProtectionOrder protectionOrder =
				security.MessageProtectionOrder;

			MessageBuffer buf = inputMessage.CreateBufferedCopy (int.MaxValue);

Console.WriteLine (buf.CreateMessage ());

			SecurityMessageProperty secProp =
				new SecurityMessageProperty ();

			Message srcmsg = buf.CreateMessage ();
			if (srcmsg.Version.Envelope == EnvelopeVersion.None)
				throw new ArgumentException ("The request is not a SOAP envelope.");

			string action = GetAction (srcmsg);
			if (action == null)
				throw new ArgumentException ("SOAP action could not be retrieved from the request message.");

			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			XPathNavigator nav = doc.CreateNavigator ();
			using (XmlWriter writer = nav.AppendChild ()) {
				buf.CreateMessage ().WriteMessage (writer);
			}

			DecryptDocument (doc, token, element, parameters, protectionOrder, secProp, serializer);

			Message msg = Message.CreateMessage (new XmlNodeReader (doc), srcmsg.Headers.Count, srcmsg.Version);

			// FIXME: when Local[Client|Service]SecuritySettings.DetectReplays
			// is true, reject such messages which don't have <wsu:Timestamp>

			WSSecurityMessageHeader sheader = null;

			for (int i = 0; i < srcmsg.Headers.Count; i++) {
				MessageHeaderInfo header = srcmsg.Headers [i];
				if (header.Namespace == Constants.WssNamespace &&
				    header.Name == "Security") {
					sheader = WSSecurityMessageHeader.Read (
						srcmsg.Headers.GetReaderAtHeader (i),
						serializer, resolver);
					msg.Headers.Add (sheader);
				}
				else
					msg.Headers.CopyHeaderFrom (srcmsg.Headers, i);
			}

			if (sheader == null)
				throw new InvalidOperationException ("Message security header was not found in the request message.");
			return msg;
		}

		static void DecryptDocument (XmlDocument doc, SecurityToken token, SecurityBindingElement element, SecurityTokenParameters parameters, MessageProtectionOrder protectionOrder, SecurityMessageProperty secProp, SecurityTokenSerializer serializer)
		{
			SecurityKey securityKey = MessageSecurityUtility.ResolveKey (token, parameters);

			XmlNamespaceManager nsmgr = new XmlNamespaceManager (doc.NameTable);
			nsmgr.AddNamespace ("s", "http://www.w3.org/2003/05/soap-envelope");
			nsmgr.AddNamespace ("c", Constants.WsscNamespace);
			nsmgr.AddNamespace ("o", Constants.WssNamespace);
			nsmgr.AddNamespace ("e", EncryptedXml.XmlEncNamespaceUrl);
			nsmgr.AddNamespace ("u", Constants.WsuNamespace);
			nsmgr.AddNamespace ("dsig", SignedXml.XmlDsigNamespaceUrl);

			XmlNodeList securityHeaders = doc.SelectNodes ("/s:Envelope/s:Header/o:Security", nsmgr);
			if (securityHeaders.Count == 0)
				throw new MessageSecurityException ("In this service that contains the security binding element, a security header is required in the reply message.");

			foreach (XmlElement security in securityHeaders)
				DecryptSingleSecurity (security, token, element, parameters, protectionOrder, secProp, securityKey, nsmgr, serializer);
		}

		static void DecryptSingleSecurity (XmlElement security, SecurityToken token, SecurityBindingElement element, SecurityTokenParameters parameters, MessageProtectionOrder protectionOrder, SecurityMessageProperty secProp, SecurityKey securityKey, XmlNamespaceManager nsmgr, SecurityTokenSerializer serializer)
		{
			XmlDocument doc = security.OwnerDocument;
			// decrypt the key with service certificate privkey
			EncryptedXml encXml = new EncryptedXml (doc);

			if (protectionOrder == MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature &&
			    security.SelectSingleNode ("dsig:Signature", nsmgr) != null)
				throw new MessageSecurityException ("The security binding element expects that the message signature is encrypted, while it isn't.");

			XmlElement keyElem = security.SelectSingleNode ("e:EncryptedKey", nsmgr) as XmlElement;
			EncryptedKey encryptedKey = new EncryptedKey ();
			encryptedKey.LoadXml (keyElem);

			byte [] decryptedKey = securityKey.DecryptKey (
				encryptedKey.EncryptionMethod.KeyAlgorithm,
				encryptedKey.CipherData.CipherValue);
			string id = encryptedKey.Id;

			// create derived keys
			// FIXME: an alternative approach is to make use of
			// EncryptedXml.AddKeyNameMapping().
			Dictionary<string,byte[]> map = ResolveDerivedKeys (security, nsmgr, decryptedKey);
			if (encryptedKey.Id != null)
				map [encryptedKey.Id] = decryptedKey;
			Rijndael aes = RijndaelManaged.Create (); // it is reused with every key
			aes.Key = map [String.Empty];
			aes.Mode = CipherMode.CBC;

			// decrypt the body with the decrypted key
			Collection<string> references = new Collection<string> ();
			foreach (XmlElement rlist in security.SelectNodes ("e:ReferenceList", nsmgr))
				foreach (XmlElement encref in rlist.SelectNodes ("e:DataReference | e:KeyReference", nsmgr))
					references.Add (StripUri (encref.GetAttribute ("URI")));
			foreach (EncryptedReference er in encryptedKey.ReferenceList)
				references.Add (StripUri (er.Uri));

			Collection<XmlElement> list = new Collection<XmlElement> ();
			foreach (string uri in references) {
				XmlElement el = doc.SelectSingleNode ("//e:EncryptedData [@Id='" + uri + "' or @u:Id='" + uri + "']", nsmgr) as XmlElement;
				if (el != null)
					list.Add (el);
				else
					throw new MessageSecurityException (String.Format ("On decryption, EncryptedData with Id '{0}', referenced by ReferenceData, was not found.", uri));
			}

			foreach (XmlElement el in list) {
				EncryptedData ed2 = new EncryptedData ();
				ed2.LoadXml (el);
				byte [] key = GetEncryptionKeyForData (ed2, encXml, map, serializer);
				aes.Key = key != null ? key : decryptedKey;
				if (ed2.GetXml () == null) throw new Exception ("Gyabo");
				encXml.ReplaceData (el, DecryptLax (encXml, ed2, aes));
			}
/*
Console.WriteLine ("======== Decrypted Document ========");
doc.PreserveWhitespace = false;
doc.Save (Console.Out);
doc.PreserveWhitespace = true;
*/
			if (security.SelectSingleNode ("dsig:Signature", nsmgr) == null)
				throw new MessageSecurityException ("The the message signature is expected but not found.");
		}

		static byte [] GetEncryptionKeyForData (EncryptedData ed2, EncryptedXml encXml, Dictionary<string,byte[]> map, SecurityTokenSerializer serializer)
		{
			if (ed2.KeyInfo == null)
				return null;
			foreach (KeyInfoClause kic in ed2.KeyInfo) {
				KeyInfoNode n = kic as KeyInfoNode;
				if (n == null)
					continue; // FIXME: probably other kinds of KeyInfoClause could be used.
				if (n.Value == null || n.Value.LocalName != "SecurityTokenReference" || n.Value.NamespaceURI != Constants.WssNamespace)
					continue; // FIXME: probably other kinds of KeyInfoClause could be used.

				SecurityKeyIdentifierClause skic = serializer.ReadKeyIdentifierClause (new XmlNodeReader (n.Value));
				LocalIdKeyIdentifierClause lskic = skic as LocalIdKeyIdentifierClause;
				string keyUri = (lskic != null) ?
					lskic.LocalId : String.Empty;
				if (lskic != null && map.ContainsKey (keyUri))
					return map [keyUri];
				else
					throw new XmlException (String.Format ("Encryption key for '{0}' was not found. URI is '{1}'", ed2.Id, keyUri));
			}
			return null; // no applicable key info clause.
		}

		// FIXME: this should consider the referent SecurityToken of
		// each DerivedKeyToken element.
		static Dictionary<string,byte[]> ResolveDerivedKeys (XmlElement security, XmlNamespaceManager nsmgr, byte [] decryptedKey)
		{
			XmlDocument doc = security.OwnerDocument;

			// create mapping from Id to derived keys
			Dictionary<string,byte[]> keys = new Dictionary<string,byte[]> ();
			// default, unless overriden by the default DerivedKeyToken.
			keys [String.Empty] = decryptedKey;

			InMemorySymmetricSecurityKey skey =
				new InMemorySymmetricSecurityKey (decryptedKey);

			byte [] currentKey = decryptedKey;
			foreach (XmlNode n in security.ChildNodes) {
				XmlElement el = n as XmlElement;
				if (el == null)
					continue;
				if (el.LocalName == "DerivedKeyToken" &&
				    el.NamespaceURI == Constants.WsscNamespace) {
					byte [] key = GetDerivedKeyBytes (el, skey, nsmgr);
					string id = el.GetAttribute ("Id", Constants.WsuNamespace);
					id = (id == null) ? String.Empty : id;
					keys [id] = key;
				}
			}

			return keys;
		}

		static string StripUri (string src)
		{
			if (src == null || src.Length == 0)
				return String.Empty;
			if (src [0] != '#')
				throw new NotSupportedException (String.Format ("Non-fragment URI in DataReference and KeyReference is not supported: '{0}'", src));
			return src.Substring (1);
		}

		static byte [] GetDerivedKeyBytes (XmlElement el, InMemorySymmetricSecurityKey skey, XmlNamespaceManager nsmgr)
		{
			XmlNode n = el.SelectSingleNode ("c:Offset", nsmgr);
			int offset = (n == null) ? 0 :
				int.Parse (n.InnerText, CultureInfo.InvariantCulture);
			n = el.SelectSingleNode ("c:Length", nsmgr);
			int length = (n == null) ? 32 :
				int.Parse (n.InnerText, CultureInfo.InvariantCulture);
			n = el.SelectSingleNode ("c:Label", nsmgr);
			string inLabel = n == null ? "WS-SecureConversation" : n.InnerText;
			byte [] label = Encoding.UTF8.GetBytes (
				inLabel + "WS-SecureConversation");
			n = el.SelectSingleNode ("c:Nonce", nsmgr);
			byte [] nonce = (n == null) ? new byte [0] :
				Convert.FromBase64String (n.InnerText);
			return skey.GenerateDerivedKey (
				SecurityAlgorithms.Psha1KeyDerivation,
				label, nonce, length * 8, offset);
		}

		// Probably it is a bug in .NET, but sometimes it does not contain
		// proper padding bytes. For such cases, use PaddingMode.None
		// instead. It must not be done in EncryptedXml class as it
		// correctly rejects improper ISO10126 padding.
		static byte [] DecryptLax (EncryptedXml encXml, EncryptedData ed, SymmetricAlgorithm symAlg)
		{
			PaddingMode bak = symAlg.Padding;
			try {
				byte [] bytes = ed.CipherData.CipherValue;

				if (encXml.Padding != PaddingMode.None &&
				    encXml.Padding != PaddingMode.Zeros &&
				    bytes [bytes.Length - 1] > symAlg.BlockSize / 8)
					symAlg.Padding = PaddingMode.None;
				return encXml.DecryptData (ed, symAlg);
			} finally {
				symAlg.Padding = bak;
			}
		}

		static string GetAction (Message msg)
		{
			string ret = msg.Headers.Action;
			if (ret == null) {
				HttpRequestMessageProperty reqprop =
					msg.Properties ["Action"] as HttpRequestMessageProperty;
				if (reqprop != null)
					ret = reqprop.Headers ["Action"];
			}
			return ret;
		}
	}
}
