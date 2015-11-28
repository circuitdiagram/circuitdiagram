using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using CircuitDiagram;
using System.Windows.Media;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Circuit Diagram")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Circuit Diagram")]
[assembly: AssemblyProduct("Circuit Diagram")]
[assembly: AssemblyCopyright("Copyright © Circut Diagram 2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

[assembly: NeutralResourcesLanguage("en-GB", UltimateResourceFallbackLocation.Satellite)]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
)]


// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("3.1.0.*")]

// Set build channel from environment variables
#if BUILD_CHANNEL_STABLE
[assembly: BuildChannel("", UpdateChannelType.Stable)]
#elif BUILD_CHANNEL_PRE
[assembly: BuildChannel("Pre", UpdateChannelType.Pre)]
#elif BUILD_CHANNEL_NIGHTLY
[assembly: BuildChannel("Nightly", UpdateChannelType.Nightly)]
#else
[assembly: BuildChannel("Dev", UpdateChannelType.Stable)]
#endif

// Per-monitor DPI scaling
[assembly: DisableDpiAwareness]

[assembly: InternalsVisibleTo("CircuitDiagram.Test")]
