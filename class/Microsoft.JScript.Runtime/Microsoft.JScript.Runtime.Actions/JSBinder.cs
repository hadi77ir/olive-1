// JSBinder.cs
//
// Authors:
//   Olivier Dufour <olivier.duff@gmail.com>
//
// Copyright (C) 2008 Olivier Dufour
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
//

using System;
using System.Collections.Generic;
using Microsoft.JScript.Runtime.Conversions;
using Microsoft.JScript.Runtime.Operations;
using Microsoft.JScript.Runtime.Types;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;
using System.Linq.Expressions;
using System.Scripting.Actions;
using System.Scripting;
using Microsoft.Scripting.Generation;

namespace Microsoft.JScript.Runtime.Actions {

	public class JSBinder : ActionBinder {

		public JSBinder (ScriptDomainManager manager)
			: base (manager)
		{
		}
		
		public override MemberGroup GetMember (OldDynamicAction action, Type type, string name)
		{
			throw new NotImplementedException ();
		}
		
		public override bool CanConvertFrom (Type fromType, Type toType, NarrowingLevel level)
		{
			throw new NotImplementedException ();
		}

		public override object Convert (object obj, Type toType)
		{
			throw new NotImplementedException ();
		}

		public override ErrorInfo MakeMissingMemberError (Type type, string name)
		{
			throw new NotImplementedException ();
		}

		public override Expression ConvertExpression (Expression expr, Type toType, ConversionResultKind kind, Expression context)
		{
			throw new NotImplementedException ();
		}

		protected override RuleBuilder<T> MakeRule<T> (OldDynamicAction action, object [] args)
		{
			throw new NotImplementedException ();
		}

		public override bool PreferConvert (Type t1, Type t2)
		{
			throw new NotImplementedException ();
		}

		public override Type SelectBestConversionFor (Type actualType, Type candidateOne, Type candidateTwo, NarrowingLevel level)
		{
			throw new NotImplementedException ();
		}
		
		public override IList<Type> GetExtensionTypes (Type t)
		{
			throw new NotImplementedException ();
		}
	}
}
