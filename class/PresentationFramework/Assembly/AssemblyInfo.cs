//
// AssemblyInfo.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
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
using System.Reflection;
using System.Resources;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

// General Information about the PresentationFramework assembly
// v3.0 Assembly

//FIXME: Bug with mcs producing CS0433?
[assembly: AssemblyCompany (Consts.MonoCompany)]
[assembly: AssemblyProduct (Consts.MonoProduct)]
[assembly: AssemblyCopyright (Consts.MonoCopyright)]
[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: AssemblyFileVersion (Consts.WinFileVersion)]

[assembly: NeutralResourcesLanguage ("en")]
[assembly: CLSCompliant (true)]
[assembly: AssemblyDelaySign (true)]
[assembly: AssemblyKeyFile ("../winfx3.pub")]

[assembly: ComVisible (false)]
[assembly: AllowPartiallyTrustedCallers]

[assembly: CompilationRelaxations (CompilationRelaxations.NoStringInterning)]
[assembly: Debuggable (DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: RuntimeCompatibility (WrapNonExceptionThrows = true)]

[assembly: Dependency ("mscorlib,", LoadHint.Always)]
[assembly: Dependency ("System,", LoadHint.Always)]
[assembly: Dependency ("WindowsBase,", LoadHint.Always)]
[assembly: Dependency ("PresentationCore,", LoadHint.Always)]

[assembly: SecurityCritical]
[assembly: PermissionSet (SecurityAction.RequestMinimum, Name = "FullTrust")]
[assembly: SecurityPermission (SecurityAction.RequestMinimum, SkipVerification = true)]

[assembly: ThemeInfo (ResourceDictionaryLocation.ExternalAssembly, ResourceDictionaryLocation.None)]

[assembly: XmlnsPrefix ("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "wpf")]
[assembly: XmlnsPrefix ("http://schemas.microsoft.com/winfx/2006/xaml", "x")]
[assembly: XmlnsPrefix ("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "av")]
[assembly: XmlnsPrefix ("http://schemas.microsoft.com/xps/2005/06", "metro")]

[assembly: XmlnsDefinition ("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Windows")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Windows.Input")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Windows.Data")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Windows.Navigation")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Windows.Shapes")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Windows.Documents")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Windows.Controls")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Windows.Controls.Primitives")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Windows.Media.Animation")]

[assembly: XmlnsDefinition ("http://schemas.microsoft.com/winfx/2006/xaml", "System.Windows.Markup")]

[assembly: XmlnsDefinition ("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows.Input")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows.Data")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows.Navigation")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows.Shapes")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows.Documents")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows.Controls")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows.Controls.Primitives")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows.Media.Animation")]

[assembly: XmlnsDefinition ("http://schemas.microsoft.com/xps/2005/06", "System.Windows")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/xps/2005/06", "System.Windows.Input")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/xps/2005/06", "System.Windows.Data")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/xps/2005/06", "System.Windows.Navigation")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/xps/2005/06", "System.Windows.Shapes")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/xps/2005/06", "System.Windows.Documents")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/xps/2005/06", "System.Windows.Controls")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/xps/2005/06", "System.Windows.Controls.Primitives")]
[assembly: XmlnsDefinition ("http://schemas.microsoft.com/xps/2005/06", "System.Windows.Media.Animation")]

[assembly: XmlnsDefinition ("http://schemas.microsoft.com/xps/2005/06/documentstructure", "System.Windows.Documents.DocumentStructures")]
