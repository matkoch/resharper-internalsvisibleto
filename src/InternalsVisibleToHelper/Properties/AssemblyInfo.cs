using System.Reflection;
#if !RESHARPER9
using JetBrains.Application.PluginSupport;
#endif

[assembly: AssemblyTitle("InternalsVisibleTo.ReSharper")]
[assembly: AssemblyCompany("Igal Tabachnik")]
[assembly: AssemblyProduct("InternalsVisibleTo.ReSharper")]
[assembly: AssemblyCopyright("Copyright © Igal Tabachnik, 2013, 2014")]

[assembly: AssemblyVersion("0.1.5")]
[assembly: AssemblyFileVersion("0.1.5")]

#if !RESHARPER9
[assembly: PluginTitle("InternalsVisibleTo Helper for ReSharper")]
[assembly: PluginDescription("Provides IntelliSense completion for the InternalsVisibleTo attribute, including the public key value, when required.\r\n" +
                             "Copyright © Igal Tabachnik, 2013, 2014")]
[assembly: PluginVendor("Igal Tabachnik")]
#endif
