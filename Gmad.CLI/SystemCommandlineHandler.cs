using System;
using System.Collections.Generic;
#if !MONOOPTIONS
using System.CommandLine;
using System.CommandLine.Invocation;
#else
using Mono.Options;
#endif
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Gmad.CLI
{
	internal static class SystemCommandlineHandler
	{
#if !MONOOPTIONS
		public static async Task<int> ExecuteAsync( string[] args )
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
#else
		public static async Task<int> ExecuteAsync( string[] args )
		{
			await Task.CompletedTask;

			Task<int> asyncCommand = null;

			Action<Task<int>> assignAsyncCommand = ( calledCommand ) =>
			{
				asyncCommand = calledCommand;
			};
			var monoCommands = new CommandSet( string.Empty )
			{
				new CreateAddonFileCommand( assignAsyncCommand ),
				new ExtractAddonFileCommand( assignAsyncCommand ),
				new DragAndDropCommand( assignAsyncCommand ),
			};

			monoCommands.Run( args );

			//try to see if there's a drag and drop command here
			if( asyncCommand is null )
			{
				var newArgs = new List<string>( args );
				newArgs.Insert( 0 , "draganddrop" );
				newArgs.Insert( 1 , "-input" );

				monoCommands.Run( newArgs );
			}


			if( asyncCommand is null )
			{
				monoCommands.Run( new string[] { "-help" } );
			}

			return asyncCommand != null ? await asyncCommand : 1;
		}

		private class AsyncReturnCommand : Command
		{
			protected Action<Task<int>> AsyncCommandSet { get; }

			public AsyncReturnCommand( Action<Task<int>> asyncCommandSet , string commandName , string helptext = null ) : base( commandName , helptext )
			{
				AsyncCommandSet = asyncCommandSet;
				Options = new OptionSet();
			}
		}

		private class CreateAddonFileCommand : AsyncReturnCommand
		{
			private DirectoryInfo InputFolder { get; set; }
			private FileInfo OutputFile { get; set; }
			private bool WarnInvalid { get; set; }

			public CreateAddonFileCommand( Action<Task<int>> asyncCommandSet ) : base( asyncCommandSet , "create" )
			{
				Options.Add( "f|folder=" , "the folder to turn into an addon" , ( inputPath ) => InputFolder = new DirectoryInfo( inputPath ) );
				Options.Add( "o|out=" , "the file output ending in .gma" , ( outputPath ) => OutputFile = new FileInfo( outputPath ) );
				Options.Add( "w|warninvalid" , "automatically skip invalid files" , ( warninvalid ) => WarnInvalid = true );

				Run = ( args ) =>
				{
					if( InputFolder == null || !InputFolder.Exists )
					{
						CommandSet.Out.WriteLine( "Invalid path for -folder" );
						return;
					}

					AsyncCommandSet( AddonHandling.CreateAddonFile( InputFolder , OutputFile , WarnInvalid ) );
				};
			}
		}

		private class ExtractAddonFileCommand : AsyncReturnCommand
		{
			private FileInfo InputFile { get; set; }
			private DirectoryInfo OutputFolder { get; set; }
			private bool WarnInvalid { get; set; }

			public ExtractAddonFileCommand( Action<Task<int>> asyncCommandSet ) : base( asyncCommandSet , "extract" )
			{
				Options.Add( "f|file=" , "the addon you want to extract" , ( inputPath ) => InputFile = new FileInfo( inputPath ) );
				Options.Add( "o|out=" , "the directory output" , ( outputPath ) => OutputFolder = new DirectoryInfo( outputPath ) );
				Options.Add( "w|warninvalid" , "automatically skip invalid files" , ( warninvalid ) => WarnInvalid = true );

				Run = ( args ) =>
				{
					if( InputFile == null || !InputFile.Exists )
					{
						CommandSet.Out.WriteLine( "Invalid path for -file" );
						return;
					}

					AsyncCommandSet( AddonHandling.ExtractAddonFile( InputFile , OutputFolder , WarnInvalid ) );
				};
			}
		}

		private class DragAndDropCommand : AsyncReturnCommand
		{
			private FileSystemInfo InputFilesystem { get; set; }

			public DragAndDropCommand( Action<Task<int>> asyncCommandSet ) : base( asyncCommandSet , "draganddrop" )
			{
				Options.Add( "i|input=" , "the addon you want to extract" , ( inputPath ) =>
					{
						if( Directory.Exists( inputPath ) )
						{
							InputFilesystem = new DirectoryInfo( inputPath );
							return;
						}

						if( File.Exists( inputPath ) )
						{
							InputFilesystem = new FileInfo( inputPath );
						}
					} );

				Run = ( args ) =>
				{
					switch( InputFilesystem )
					{
						case FileInfo file:
						{
							AsyncCommandSet( AddonHandling.ExtractAddonFile( file , null ) );
							break;
						}
						case DirectoryInfo folder:
						{
							AsyncCommandSet( AddonHandling.CreateAddonFile( folder , null ) );
							break;
						}
					}
				};
			}
		}
	}
#endif


}