//
// wpfp -- a semi-clone of monop
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	John Luke  (john.luke@gmail.com)
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2004 Ben Maurer
// (C) 2004 John Luke
// (C) 2007 Chris Toshok

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
using System.CodeDom.Compiler;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

class Wpfp {
	static string assembly;
	
	// namespaces of interest
	static readonly string [] common_ns = {
		"System.Windows",
		"System.Windows.Annotations",
		"System.Windows.Annotations.Storage",
		"System.Windows.Automation",
		"System.Windows.Automation.Peers",
		"System.Windows.Controls",
		"System.Windows.Controls.Primitive",
		"System.Windows.Controls.Primitives",
		"System.Windows.Converters",
		"System.Windows.Data",
		"System.Windows.Documents",
		"System.Windows.Documents.DocumentStructures",
		"System.Windows.Documents.Serialization",
		"System.Windows.Ink",
		"System.Windows.Input",
		"System.Windows.Input.StylusPlugIns",
		"System.Windows.Interop",
		"System.Windows.Markup",
		"System.Windows.Markup.Localizer",
		"System.Windows.Markup.Primitives",
		"System.Windows.Media",
		"System.Windows.Media.Animation",
		"System.Windows.Media.Converters",
		"System.Windows.Media.Effects",
		"System.Windows.Media.Imaging",
		"System.Windows.Media.Media3D",
		"System.Windows.Media.Media3D.Converters",
		"System.Windows.Media.TextFormatting",
		"System.Windows.Navigation",
		"System.Windows.Resources",
		"System.Windows.Serialization",
		"System.Windows.Shapes",
		"System.Windows.Threading"
	};
	
	static readonly string [] common_assemblies = {
		"WindowsBase.dll",
		"PresentationCore.dll",
		"PresentationFramework.dll"
	};
	
	static Type GetType (string tname, bool ignoreCase)
	{
		Type t;
		if (assembly != null) {
			Assembly a = GetAssembly (assembly, true);
			t = a.GetType (tname, false, ignoreCase);
		} else 
			t = Type.GetType (tname, false, ignoreCase);

		return t;
	}
	
	static string SearchTypes (string name, ref Type retval, out int count)
	{
		StringBuilder sb = new StringBuilder ();
		Type current = null;
		count = 0;

		string [] assemblies = GetKnownAssemblyNames ();
		for (int i = 0; i < assemblies.Length; i++) {
			Assembly a = GetAssembly (assemblies [i], false);
			if (a == null)
				continue;

			Type [] types = a.GetTypes ();
			for (int j = 0; j < types.Length; j++) {
				Type t = types [j];
				if (t.IsPublic == false)
					continue;
				
				if (t.Name == name || t.Name.ToLower ().IndexOf (name.ToLower ()) > 0) {
					current = t;
					count ++;
					sb.Append (t.FullName + " from " + a.Location + "\n");
				}
			}
		}

		if (count == 0) 
			return null;

		if (count == 1) {
			retval = current;
			return String.Empty;
		}
		
		return sb.ToString ();
	}

	static string [] GetKnownAssemblyNames ()
	{
		Process p = new Process ();
		p.StartInfo.UseShellExecute = false;
		p.StartInfo.RedirectStandardOutput = true;
		p.StartInfo.FileName = "gacutil";
		p.StartInfo.Arguments = "-l";
		try {
			p.Start ();
		}
		catch {
			Console.WriteLine ("WARNING: gacutil could not be found.");
			return new string[0];
		}

		string s;
		ArrayList names = new ArrayList ();
		StreamReader output = p.StandardOutput;

		while ((s = output.ReadLine ()) != null)
			names.Add (s);

		p.WaitForExit ();
		
		int length = names.Count - 1;
		string [] retval = new string [length];
		retval [0] = typeof (Object).Assembly.FullName;		
		names.CopyTo (1, retval, 1, length - 1); // skip the first and last line
		return retval;
	}

	static Assembly GetAssembly (string assembly, bool exit)
	{
		Assembly a = null;

		// if -r:~/foo.dll syntax is used the shell misses it
		if (assembly.StartsWith ("~/"))
			assembly = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), assembly.Substring (2));

		try {
			// if it exists try to use LoadFrom
			if (File.Exists (assembly))
				a = Assembly.LoadFrom (assembly);
			// if it looks like a fullname try that
			else if (assembly.Split (',').Length == 4)
				a = Assembly.Load (assembly);
			// see if MONO_PATH has it
			else
				a = LoadFromMonoPath (assembly);
		} catch {
			// ignore exception it gets handled below
		}

		// last try partial name
		// this (apparently) is exception safe
		if (a == null)
			a = Assembly.LoadWithPartialName (assembly);

		if (a == null && exit) {
			Console.WriteLine ("Could not load {0}", Wpfp.assembly);
			Environment.Exit (1);
		}

		return a;
	}

	static Assembly LoadFromMonoPath (string assembly)
	{
		// ; on win32, : everywhere else
		char sep = (Path.DirectorySeparatorChar == '/' ? ':' : ';');
		string[] paths = Environment.GetEnvironmentVariable ("MONO_PATH").Split (sep);
		foreach (string path in paths) {	
			string apath = Path.Combine (path, assembly);
			if (File.Exists (apath))
				return Assembly.LoadFrom (apath);	
		}
		return null;
	}

	static Type GetType (string tname)
	{
		return GetType (tname, false);
	}

	static void PrintRefs (string assembly)
	{
		Assembly a = GetAssembly (assembly, true);
		foreach (AssemblyName an in a.GetReferencedAssemblies ())
			Console.WriteLine (an);
	}

	static void PrintTypes (string assembly, bool show_private, bool filter_obsolete)
	{
		Assembly a = GetAssembly (assembly, true);

		Console.WriteLine ();
		Console.WriteLine ("Assembly Information:");

		object[] cls = a.GetCustomAttributes (typeof (CLSCompliantAttribute), false);
		if (cls.Length > 0) {
			CLSCompliantAttribute cca = cls[0] as CLSCompliantAttribute;
			if (cca.IsCompliant)
				Console.WriteLine ("[CLSCompliant]");
		}

		foreach (string ai in a.ToString ().Split (','))
			Console.WriteLine (ai.Trim ());
			
		Console.WriteLine ();
		Type [] types = show_private ? a.GetTypes () : a.GetExportedTypes ();
		Array.Sort (types, new TypeSorter ());

		int obsolete_count = 0;
		foreach (Type t in types) {
			if (filter_obsolete && t.IsDefined (typeof (ObsoleteAttribute), false))
				obsolete_count ++;
			else
				Console.WriteLine (t.FullName);
		}

		Console.WriteLine ("\nTotal: {0} types.", types.Length - obsolete_count);
	}
	
	internal static void Completion (string prefix)
	{
		foreach (string assm in common_assemblies) {
			try {
				
				Assembly a = GetAssembly (assm, true);
				foreach (Type t in a.GetExportedTypes ()) {
					
					if (t.Name.StartsWith (prefix)) {
						if (Array.IndexOf (common_ns, t.Namespace) != -1) {
							Console.WriteLine (t.Name);
							return;
						}
					}
					
					if (t.FullName.StartsWith (prefix)) {
						Console.WriteLine (t.FullName);
					}
				}
				
			} catch {
			}
		}
		
	}

	static void Main (string [] args)
	{
		Options options = new Options ();
		if (!options.ProcessArgs (args))
			return;
		
		if (options.AssemblyReference != null) {
			assembly = options.AssemblyReference;
			
			if (options.Type == null) {
				if (options.PrintRefs)
					PrintRefs (assembly);
				else
					PrintTypes (assembly, options.ShowPrivate, options.FilterObsolete);
				return;
			}
		}

		string message = null;
		string tname = options.Type;
		Type t = null;
		int count;

		if (options.Search) {
			string matches = SearchTypes (tname, ref t, out count);

			if (count == 0)
				goto notfound;

			if (count == 1)
				goto found;
			
			if (count > 1){
				Console.WriteLine ("Found " + count + " types that match:");
				Console.WriteLine (matches);
				return;
			}
		}
			
		t = GetType (tname);

		if (t == null) {
			foreach (string assm in GetKnownAssemblyNames ()) {
				try {
					Assembly a = GetAssembly (assm, false);
					t = a.GetType (tname, false, true);
					if (t != null) {
						message = String.Format ("{0} is included in the {1} assembly.",
								t.FullName, 
								t.Assembly.GetName ().Name);
						goto found;
					}
					foreach (string ns in common_ns) {
						t = a.GetType (ns + "." + tname, false, true);
						if (t != null) {
							message = String.Format ("{0} is included in the {1} assembly.",
									t.FullName, 
									t.Assembly.GetName ().Name);
							goto found;
						}
					}
				} catch {
				}
			}
		}

	notfound:
		if (t == null) {
			Console.WriteLine ("Could not find {0}", tname);
			return;
		}
	found:
		//
		// This gets us nice buffering
		//
		StreamWriter sw = new StreamWriter (Console.OpenStandardOutput (), Console.Out.Encoding);				
		new Outline (t, sw, options).OutlineType ();
		sw.Flush ();

		if (message != null)
			Console.WriteLine (message);
	}
}

