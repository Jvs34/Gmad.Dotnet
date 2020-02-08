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
		private static async Task<int> Main( string[] args )
		{
			var warnInvalidOption = new Option( "-warninvalid" , "automatically skip invalid files" )
			{
				Argument = new Argument<bool>()
				{
					Arity = ArgumentArity.ZeroOrOne
				} ,
			};

			var createCommand = new Command( "create" )
			{
				new Option( "-folder", "the folder to turn into an addon" )
				{
					Argument = new Argument<DirectoryInfo>( "folder" )
					{
						Arity = ArgumentArity.ExactlyOne
					}.ExistingOnly(),
					Required = true,
				},
				new Option( "-out" , "the file output ending in .gma" )
				{
					Argument = new Argument<FileInfo>()
					{
						Arity = ArgumentArity.ZeroOrOne
					},
				},
				warnInvalidOption
			};

			createCommand.Handler = CommandHandler.Create<DirectoryInfo , FileInfo , bool>( CreateAddonFile );

			var extractCommand = new Command( "extract" )
			{
				new Option( "-file" , "the addon you want to extract" )
				{
					Argument = new Argument<FileInfo>().ExistingOnly()
				},
				new Option( "-out" , "the directory output" )
				{
					Argument = new Argument<DirectoryInfo>()
				},
				warnInvalidOption
			};

			extractCommand.Handler = CommandHandler.Create<FileInfo , DirectoryInfo , bool>( ExtractAddonFile );

			var rootCommand = new RootCommand()
			{
				createCommand,
				extractCommand,
				new Argument<FileSystemInfo>( "target" ) //TODO: the name might need to be removed, dunno
				{
					IsHidden = true
				}.ExistingOnly()
			};

			//TODO: drag and drop feeds the path of the file/folder as one of the arguments, need to validate it with the rootcommand system somehow
			/*
			rootCommand.AddArgument( new Argument<FileSystemInfo>( "target" ) //TODO: the name might need to be removed, dunno
			{
				IsHidden = true
			}.ExistingOnly() );

			rootCommand.AddCommand( createCommand );
			rootCommand.AddCommand( extractCommand );
			*/

			rootCommand.Handler = CommandHandler.Create<FileSystemInfo>( HandleDragAndDrop );
			return await rootCommand.InvokeAsync( args );
		}

		private static int HandleDragAndDrop( FileSystemInfo target )
		{
			switch( target )
			{
				case FileInfo file:
				{
					return ExtractAddonFile( file , new DirectoryInfo( "" ) );
				}
				case DirectoryInfo folder:
				{
					return CreateAddonFile( folder , new FileInfo( "" ) );
				}
				default:
				{
					return 0;
				}
			}
		}

		private static int CreateAddonFile( DirectoryInfo folder , FileInfo @out , bool warninvalid = false )
		{
			//unfortunately, the binding for the function in system.commandline is based on names of the Options stripped from dashes and case sensitive stuff
			//so to keep my sanity I'm going to rename this one
			var fileOutput = @out;

			if( fileOutput is null )
			{
				//use the parent folder of the addon folder and create the gma there
				fileOutput = new FileInfo( folder.FullName + ".gma" );
			}

			//TODO: read addon.json, if it's not available, create it

			AddonInfo addonInfo = new AddonInfo();

			//open every file in the folder, then feed it as a string:stream dictionary
			Dictionary<string , Stream> files = new Dictionary<string , Stream>();

			foreach( var fileInput in folder.EnumerateFiles( "*" , SearchOption.AllDirectories ) )
			{
				//turn the file paths into relatives
				string relativeFilePath = Path.GetRelativePath( folder.FullName , fileInput.FullName );

				//is this allowed by the wildcard? also the addon.json might have an ignore list already

				if( !Addon.IsPathAllowed( relativeFilePath , addonInfo.IgnoreWildcard ) )
				{
					if( warninvalid )
					{
						Console.WriteLine( $"WAH {relativeFilePath} is BAD PATH" );
					}
					continue;
				}

				var fileInputStream = fileInput.OpenRead();

				//TODO: change the slashes of the relativeFilePath to whatever gmad uses internally?

				files.Add( relativeFilePath , fileInputStream );
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

		private static int ExtractAddonFile( FileInfo file , DirectoryInfo @out , bool warninvalid = false )
		{
			//see CreateAddonFile above for the name reason
			var folderOutput = @out;

			if( folderOutput?.Exists != true )
			{
				folderOutput = new DirectoryInfo( Path.GetFileNameWithoutExtension( file.FullName ) );

				if( !folderOutput.Exists )
				{
					folderOutput.Create();
				}
			}

			using var inputStream = file.OpenRead();

			Dictionary<string , Stream> files = new Dictionary<string , Stream>();

			int successCode = Addon.Extract( inputStream , ( filePath ) =>
			{
				var outputFileInfo = new FileInfo( Path.Combine( folderOutput.FullName , filePath ) );

				if( !outputFileInfo.FullName.Contains( folderOutput.FullName ) )
				{
					throw new Exception( $"Addon extraction somehow ended up outside main folder {outputFileInfo.FullName}" );
				}

				var fileStream = outputFileInfo.OpenWrite();

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
