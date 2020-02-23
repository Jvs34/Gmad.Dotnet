using System;
using System.Collections.Generic;
using System.Text;

namespace Gmad.Shared
{
	public static partial class Addon
	{
		public static string Ident { get; } = "GMAD";
		public static int Version { get; } = 3;
		public static uint AppID { get; } = 4000;
		public static uint CompressionSignature { get; } = 0xBEEFCACE;
		public static uint TimestampOffset { get; } //= sizeof( Header ) + sizeof( UInt64 ); //TODO: gmad itself doesn't use it, maybe gmpublish does?

		public static IReadOnlyList<string> TypeTags { get; } = new List<string>()
		{
			"gamemode",
			"map",
			"weapon",
			"vehicle",
			"npc",
			"entity",
			"tool",
			"effects",
			"model",
			"servercontent"
		};

		public static IReadOnlyList<string> MiscTags { get; } = new List<string>()
		{
			"fun",
			"roleplay",
			"scenic",
			"movie",
			"realism",
			"cartoon",
			"water",
			"comic",
			"build"
		};
	}
}
