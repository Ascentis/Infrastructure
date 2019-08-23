using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.EnterpriseServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Ascentis.ExternalCacheItem")]
[assembly: AssemblyDescription("COM class used to transfer objects between a client application and MemoryCache residing in out of process COM Server")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Ascentis")]
[assembly: AssemblyProduct("Ascentis.ExternalCacheItem")]
[assembly: AssemblyCopyright("Copyright ©  2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("b1d90b19-60bd-4959-acaa-bfe4f567df7c")]

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
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]

[assembly: ApplicationName("Ascentis.ExternalCacheItem")]
[assembly: ApplicationActivation(ActivationOption.Library)]
[assembly: ApplicationAccessControl(false, Authentication = AuthenticationOption.None)]
[assembly: AssemblyKeyFileAttribute("Ascentis.ExternalCacheItem.snk")]