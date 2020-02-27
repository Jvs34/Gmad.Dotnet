using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace Gmad.CLI
{
	internal static class Program
	{
		private static async Task<int> Main( string[] args )
		{
			Shared.Addon.DeserializeAddonInfoCallback = AddonHandling.DeserializeAddonInfo;
			Shared.Addon.SerializeAddonInfoToStringCallback = AddonHandling.SerializeAddonInfoToString;

			return await SystemCommandlineHandler.ExecuteAsync( args );
		}
	}
}
