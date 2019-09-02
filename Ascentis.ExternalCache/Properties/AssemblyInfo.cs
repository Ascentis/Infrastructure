using System.EnterpriseServices;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Ascentis.ExternalCache")]
[assembly: AssemblyDescription("Out of process cache operating as a COM+ server based on MemoryCache class")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Ascentis")]
[assembly: AssemblyProduct("Ascentis.ExternalCache")]
[assembly: AssemblyCopyright("Copyright ©  2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("35d66854-b6a0-4155-9302-ecebddf171e8")]

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
[assembly: AssemblyVersion("1.2.0.2")]
[assembly: AssemblyFileVersion("1.2.0.2")]

[assembly: ApplicationName("Ascentis.ExternalCache")]
[assembly: ApplicationActivation(ActivationOption.Server)]
[assembly: ApplicationAccessControl(false, Authentication = AuthenticationOption.None)]
[assembly: AssemblyKeyFileAttribute("Ascentis.ExternalCache.snk")]
