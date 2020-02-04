using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace Gmad.CLI
{
	internal static class Program
	{
		private static Command RootCommand { get; set; }
		private static Command CreateCommand { get; set; }
		private static Command ExtractCommand { get; set; }

		private static async Task<int> Main( string [] args )
		{
			var createCommand = new Command( "create" )
			{
				new Option( "-folder", "the folder to turn into an addon" )
				{
					Argument = new Argument<DirectoryInfo>()
					{
						Arity = ArgumentArity.ExactlyOne
					} ,
					Required = true,
				},
				new Option( "-out" , "the file output ending in .gma" )
				{
					Argument = new Argument<FileInfo>()
					{
						Arity = ArgumentArity.ZeroOrOne
					}
				},
				new Option( "-warninvalid" , "automatically skip invalid files" )
				{
					Argument = new Argument()
					{
						Arity = ArgumentArity.ZeroOrOne
					} ,
				},
			};

			createCommand.Handler = CommandHandler.Create<DirectoryInfo , FileInfo , bool>( ( folder , file , warninvalid ) =>
			{
				if( !folder.Exists )
				{
					Console.WriteLine( "Missing -folder (the folder to turn into an addon)" );
					return 0;
				}

				return CreateAddonFile( folder , file , warninvalid );
			} );

			var extractCommand = new Command( "extract" )
			{
				new Option( "-file" , "the addon you want to extract" )
				{
					Argument = new Argument<FileInfo>()
				},
				new Option( "-out" , "the directory output" )
				{
					Argument = new Argument<DirectoryInfo>()
				},
			};


			extractCommand.Handler = CommandHandler.Create<FileInfo , DirectoryInfo>( ( file , folder ) =>
			{
				if( !file.Exists )
				{
					Console.WriteLine( "Missing -file (the addon you want to extract)" );
					return 0;
				}

				return ExtractAddonFile( file , folder );
			} );

			var rootCommand = new RootCommand();

			//TODO: drag and drop feeds the path of the file/folder as one of the arguments, need to validate it with the rootcommand system somehow
			//is this gonna require a handler?
			rootCommand.AddArgument( new Argument<FileInfo>( "the addon you want to extract" ) );
			rootCommand.AddArgument( new Argument<DirectoryInfo>( "the folder you turn into an addon" ) );
			rootCommand.AddCommand( createCommand );
			rootCommand.AddCommand( extractCommand );

			rootCommand.Handler = CommandHandler.Create<FileInfo , DirectoryInfo>( ( file , folder ) =>
			{
				if( file.Exists )
				{
					return ExtractAddonFile( file , new DirectoryInfo( "" ) );
				}

				if( folder.Exists )
				{
					return CreateAddonFile( folder , new FileInfo( "" ) );
				}

				return 0;
			} );


			RootCommand = rootCommand;
			CreateCommand = createCommand;
			ExtractCommand = extractCommand;


			//return await rootCommand.InvokeAsync( args );
			await rootCommand.InvokeAsync( "create" );
			Console.ReadLine();
			return 0;
		}

		private static int CreateAddonFile( DirectoryInfo folder , FileInfo file , bool warninvalid = false )
		{
			return 0;
		}

		private static int ExtractAddonFile( FileInfo file , DirectoryInfo folder )
		{
			return 0;
		}


	}
}
