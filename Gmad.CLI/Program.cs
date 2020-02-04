using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

namespace Gmad.CLI
{
	internal static class Program
	{
		private static async Task<int> Main( string [] args )
		{
			var createCommand = new Command( "create" )
			{
				new Option( "-folder", "the folder to turn into an addon" )
				{
					Argument = new Argument<DirectoryInfo>(),
				},
				new Option( "-out" , "the file output ending in .gma" )
				{
					Argument = new Argument<FileInfo>()
				},
				new Option( "-warninvalid" , "automatically skip invalid files" )
			};

			var extractCommand = new Command( "extract" )
			{

			};


			var rootCommand = new RootCommand();

			rootCommand.AddCommand( createCommand );
			rootCommand.AddCommand( extractCommand );

			return await rootCommand.InvokeAsync( args );
		}
	}
}
