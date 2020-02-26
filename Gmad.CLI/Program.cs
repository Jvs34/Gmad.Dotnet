using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gmad.CLI
{
	internal static class Program
	{
		private static async Task<int> Main( string[] args )
		{
			Shared.Addon.DeserializeAddonInfoCallback = AddonHandling.DeserializeAddonInfo;
			Shared.Addon.SerializeAddonInfoToStringCallback = AddonHandling.SerializeAddonInfoToString;

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

			createCommand.Handler = CommandHandler.Create<DirectoryInfo , FileInfo , bool>( CreateAddonFileCommand );

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

			extractCommand.Handler = CommandHandler.Create<FileInfo , DirectoryInfo , bool>( ExtractAddonFileCommand );

			var rootCommand = new RootCommand()
			{
				createCommand,
				extractCommand,
				new Argument<FileSystemInfo>( "target" ) //TODO: the name might need to be removed, dunno
				{
					IsHidden = true
				}.ExistingOnly()
			};

			rootCommand.Handler = CommandHandler.Create<FileSystemInfo>( HandleDragAndDropCommand );
			return await rootCommand.InvokeAsync( args );
		}

		private static async Task<int> CreateAddonFileCommand( DirectoryInfo folder , FileInfo @out , bool warninvalid = false )
		{
			return await AddonHandling.CreateAddonFile( folder , @out , warninvalid );
		}

		private static async Task<int> ExtractAddonFileCommand( FileInfo file , DirectoryInfo @out , bool warninvalid = false )
		{
			return await AddonHandling.ExtractAddonFile( file , @out , warninvalid );
		}

		private static async Task<int> HandleDragAndDropCommand( FileSystemInfo target )
		{
			switch( target )
			{
				case FileInfo file:
				{
					return await AddonHandling.ExtractAddonFile( file , null );
				}
				case DirectoryInfo folder:
				{
					return await AddonHandling.CreateAddonFile( folder , null );
				}
			}

			Console.Error.WriteLine( "Cannot handle drag and drop action." );
			return 1;
		}
	}
}
