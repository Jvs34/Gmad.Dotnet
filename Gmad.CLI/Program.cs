using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Gmad.Shared;

namespace Gmad.CLI
{
	internal static class Program
	{
		private static Command RootCommand { get; set; }
		private static Command CreateCommand { get; set; }
		private static Command ExtractCommand { get; set; }

		private static async Task<int> Main( string[] args )
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
			rootCommand.AddArgument( new Argument<FileInfo>( "the addon you want to extract" )
			{
				IsHidden = true
			} );
			rootCommand.AddArgument( new Argument<DirectoryInfo>( "the folder you turn into an addon" )
			{
				IsHidden = true
			} );

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
			await rootCommand.InvokeAsync( args );
			Console.ReadLine();
			return 0;
		}

		private static int CreateAddonFile( DirectoryInfo folder , FileInfo fileOutput , bool warninvalid = false )
		{
			if( fileOutput?.Exists != true )
			{
				//use the parent folder of the addon folder and create the gma there
				fileOutput = new FileInfo( folder.FullName + ".gma" );
			}

			//TODO: read addon.json, if it's not available, create it

			//open every file in the folder, then feed it as a string:stream dictionary
			Dictionary<string , Stream> files = new Dictionary<string , Stream>();

			foreach( var fileInput in folder.EnumerateFiles( "*" , SearchOption.AllDirectories ) )
			{
				//is this allowed by the wildcard? also the addon.json might have an ignore list already
			}

			//now open the stream for the output

			using var outputStream = fileOutput.OpenWrite();

			int successCode = Addon.Create( files , outputStream );

			foreach( var kv in files )
			{
				kv.Value.Dispose();
			}

			return successCode;
		}

		private static int ExtractAddonFile( FileInfo file , DirectoryInfo folderOutput )
		{
			if( folderOutput?.Exists != true )
			{
				folderOutput = new DirectoryInfo( Path.GetFileNameWithoutExtension( file.FullName ) );
			}

			using var inputStream = file.OpenRead();

			Dictionary<string , Stream> files = new Dictionary<string , Stream>();

			int successCode = Addon.Extract( inputStream , ( filePath ) =>
			{
				var fileStream = File.OpenWrite( Path.Combine( folderOutput.FullName , filePath ) );

				files.Add( filePath , fileStream );

				return fileStream;
			} );

			foreach( var kv in files )
			{
				kv.Value.Dispose();
			}

			return successCode;
		}


	}
}
