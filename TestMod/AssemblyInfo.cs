using System.Reflection;
using CumLoader;

[assembly: AssemblyTitle(TestMod.BuildInfo.Description)]
[assembly: AssemblyDescription(TestMod.BuildInfo.Description)]
[assembly: AssemblyCompany(TestMod.BuildInfo.Company)]
[assembly: AssemblyProduct(TestMod.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + TestMod.BuildInfo.Author)]
[assembly: AssemblyTrademark(TestMod.BuildInfo.Company)]
[assembly: AssemblyVersion(TestMod.BuildInfo.Version)]
[assembly: AssemblyFileVersion(TestMod.BuildInfo.Version)]
[assembly: CumInfo(typeof(TestMod.TestMod), TestMod.BuildInfo.Name, TestMod.BuildInfo.Version, TestMod.BuildInfo.Author, TestMod.BuildInfo.DownloadLink)]


// Create and Setup a CumGame to mark a Mod as Universal or Compatible with specific Games.
// If no CumGameAttribute is found or any of the Values for any CumGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for CumMame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: CumGame(null, null)]