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
			//bool b = Addon.IsPathAllowed( "lua/effects/soccerball_explode.lua" , "lua/*.lua" );
			//return 0;

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
					Console.Error.WriteLine( "Cannot handle drag and drop action." );
					return 1;
				}
			}
		}

		private static int CreateAddonFile( DirectoryInfo folder , FileInfo @out , bool warninvalid = false )
		{
			//unfortunately, the binding for the function in system.commandline is based on names of the Options stripped from dashes and case sensitive stuff
			//so to keep my sanity I'm going to rename this one
			var fileOutput = @out ?? new FileInfo( folder.FullName + ".gma" );
			//TODO: read addon.json, if it's not available, create it

			var jsonFileInfo = new FileInfo( Path.Combine( folder.FullName , "addon.json" ) );

			AddonInfo addonInfo = OpenJSON( jsonFileInfo );

			//open every file in the folder, then feed it as a string:stream dictionary
			Dictionary<string , Stream> files = new Dictionary<string , Stream>();

			foreach( var fileInput in folder.EnumerateFiles( "*" , SearchOption.AllDirectories ) )
			{
				//turn the file paths into relatives and also lowercase
				string relativeFilePath = Path.GetRelativePath( folder.FullName , fileInput.FullName ).ToLower();

				relativeFilePath = relativeFilePath.Replace( "\\" , "/" );

				
				//this could PROBABLY be streamlined in a Addon.IsIgnoreMatching function but for now I just want this to work

				if( Addon.IsWildcardMatching( relativeFilePath , Addon.DefaultIgnores ) )
				{
					continue;
				}

				if( addonInfo.IgnoreWildcard != null && Addon.IsWildcardMatching( relativeFilePath , addonInfo.IgnoreWildcard ) )
				{
					continue;
				}

				//if it's not ignored and still not allowed, throw out an error

				if( !Addon.IsPathAllowed( relativeFilePath ) )
				{
					if( warninvalid )
					{
						Console.Error.WriteLine( $"{relativeFilePath} \t\t[Not allowed by whitelist]" );
					}
					return 1;
				}

				var fileInputStream = fileInput.OpenRead();

				files.Add( relativeFilePath , fileInputStream );
			}

			if( files.Count == 0 )
			{
				Console.Error.WriteLine( "No files found, can't continue!" );
				return 1;
			}

			//now open the stream for the output
			bool success;

			using( var outputStream = fileOutput.OpenWrite() )
			{
				success = Addon.Create( files , outputStream , addonInfo );
			}

			foreach( var kv in files )
			{
				kv.Value?.Dispose();
			}

			if( success )
			{
				SaveJSON( jsonFileInfo , addonInfo );
				Console.WriteLine( $"Successfully saved to {fileOutput.FullName}" );
			}

			return Convert.ToInt32( !success );
		}

		private static int ExtractAddonFile( FileInfo file , DirectoryInfo @out , bool warninvalid = false )
		{
			//see CreateAddonFile above for the name reason
			var folderOutput = @out;

			if( folderOutput == null )
			{
				folderOutput = new DirectoryInfo( Path.GetFileNameWithoutExtension( file.FullName ) );
			}

			if( !folderOutput.Exists )
			{
				folderOutput.Create();
			}

			using var gmadFileStream = file.OpenRead();

			Dictionary<string , Stream> files = new Dictionary<string , Stream>();

			var jsonFileInfo = new FileInfo( Path.Combine( folderOutput.FullName , "addon.json" ) );

			//in case of re-extraction, we don't want to overwrite a manually written json for whatever reason
			AddonInfo addonInfo = OpenJSON( jsonFileInfo ) ?? new AddonInfo();

			bool success = Addon.Extract( gmadFileStream , ( filePath ) =>
			{
				var outputFileInfo = new FileInfo( Path.Combine( folderOutput.FullName , filePath ) );

				//create the subfolder first

				outputFileInfo.Directory.Create();

				if( !outputFileInfo.FullName.StartsWith( folderOutput.FullName ) )
				{
					throw new IOException( $"Addon extraction somehow ended up outside main folder {outputFileInfo.FullName}" );
				}

				var fileStream = outputFileInfo.OpenWrite();

				files.Add( filePath , fileStream );

				return fileStream;
			} , addonInfo );

			foreach( var kv in files )
			{
				kv.Value.Dispose();
			}

			if( success )
			{
				SaveJSON( jsonFileInfo , addonInfo );
			}

			return Convert.ToInt32( !success );
		}


		private static AddonInfo OpenJSON( FileInfo jsonFile )
		{
			if( !jsonFile.Exists )
			{
				return null;
			}

			using var fileStream = jsonFile.OpenRead();
			return Addon.LoadAddonInfo( fileStream );
		}

		private static void SaveJSON( FileInfo jsonFile , AddonInfo addonInfo )
		{
			using var fileStream = jsonFile.CreateText();

			Addon.SaveAddonInfo( addonInfo , fileStream );
		}
	}
}
