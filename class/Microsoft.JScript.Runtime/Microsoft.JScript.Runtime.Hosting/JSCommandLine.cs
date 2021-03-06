//
// JSCommandLine.cs
//
// Author:
//   Olivier Dufour (olivier.duff@gmail.com)
//
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
using System.Text;
using System.IO;

namespace Microsoft.JScript.Runtime.Hosting
{
	public sealed class JSCommandLine : Microsoft.Scripting.Hosting.Shell.CommandLine
	{
		public JSCommandLine ()
		{
		}

		protected override int RunCommand (string command)
		{
			try {
				Engine.Execute (base.Module, Engine.CreateScriptSourceFromString(command));
			} catch (Exception e) {
				Console.Write (Engine.FormatException (e), Microsoft.Scripting.Shell.Style.Error);
				return -1;
			}
			Console.WriteLine ("commande succeed!", Microsoft.Scripting.Shell.Style.Out);
			return 0;
		}

		protected override int RunFile (string fileName)
		{
			if (!File.Exists(fileName))
			{
				Console.WriteLine ("File does not exist", Microsoft.Scripting.Shell.Style.Error);
				return -1;
			}
			throw new NotImplementedException ();
		}

		protected override string Logo {
			get { return "Mono Java Script Compiler for Moonlight 0.1 " + Environment.NewLine; } 
		}
		protected override string Prompt {
			get { return ">"; }
		}
		protected override string PromptContinuation {
			get { return " "; }
		}
	}

}
