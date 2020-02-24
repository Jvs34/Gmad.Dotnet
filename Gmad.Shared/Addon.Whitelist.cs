using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gmad.Shared
{
	public static partial class Addon
	{
		/// <summary>
		/// Check this before the addon's ignore list, because gmad's code does it in a hardcoded way
		/// </summary>
		public static IReadOnlyList<string> DefaultIgnores { get; } = new List<string>()
		{
			"addon.json",
			"*thumbs.db",
			"*desktop.ini",
			".DS_Store",
			"*/.DS_Store"
		};

		public static IReadOnlyList<string> Wildcards { get; } = new List<string>()
		{
			"lua/*.lua",
			"scenes/*.vcd",
			"particles/*.pcf",
			"resource/fonts/*.ttf",
			"scripts/vehicles/*.txt",
			"resource/localization/*/*.properties",
			"maps/*.bsp",
			"maps/*.nav",
			"maps/*.ain",
			"maps/thumb/*.png",
			"sound/*.wav",
			"sound/*.mp3",
			"sound/*.ogg",
			"materials/*.vmt",
			"materials/*.vtf",
			"materials/*.png",
			"materials/*.jpg",
			"materials/*.jpeg",
			"models/*.mdl",
			"models/*.vtx",
			"models/*.phy",
			"models/*.ani",
			"models/*.vvd",
			"gamemodes/*/*.txt",
			"gamemodes/*/*.fgd",
			"gamemodes/*/logo.png",
			"gamemodes/*/icon24.png",
			"gamemodes/*/gamemode/*.lua",
			"gamemodes/*/entities/effects/*.lua",
			"gamemodes/*/entities/weapons/*.lua",
			"gamemodes/*/entities/entities/*.lua",
			"gamemodes/*/backgrounds/*.png",
			"gamemodes/*/backgrounds/*.jpg",
			"gamemodes/*/backgrounds/*.jpeg",
			"gamemodes/*/content/models/*.mdl",
			"gamemodes/*/content/models/*.vtx",
			"gamemodes/*/content/models/*.phy",
			"gamemodes/*/content/models/*.ani",
			"gamemodes/*/content/models/*.vvd",
			"gamemodes/*/content/materials/*.vmt",
			"gamemodes/*/content/materials/*.vtf",
			"gamemodes/*/content/materials/*.png",
			"gamemodes/*/content/materials/*.jpg",
			"gamemodes/*/content/materials/*.jpeg",
			"gamemodes/*/content/scenes/*.vcd",
			"gamemodes/*/content/particles/*.pcf",
			"gamemodes/*/content/resource/fonts/*.ttf",
			"gamemodes/*/content/scripts/vehicles/*.txt",
			"gamemodes/*/content/resource/localization/*/*.properties",
			"gamemodes/*/content/maps/*.bsp",
			"gamemodes/*/content/maps/*.nav",
			"gamemodes/*/content/maps/*.ain",
			"gamemodes/*/content/maps/thumb/*.png",
			"gamemodes/*/content/sound/*.wav",
			"gamemodes/*/content/sound/*.mp3",
			"gamemodes/*/content/sound/*.ogg",
		};

		/// <summary>
		/// Calls <see cref="IsWildcardMatching"/> with Addon.Wildcards to check if the path is allowed
		/// </summary>
		/// <param name="relativePath">the filesystem path relative to the addon folder </param>
		/// <returns></returns>
		public static bool IsPathAllowed( string relativePath )
		{
			return IsWildcardMatching( relativePath , Wildcards );
		}

		public static bool IsWildcardMatching( string relativePath , IEnumerable<string> wildcardList )
		{
			return wildcardList?.Any( wildcard => IsWildcardMatching( relativePath , wildcard ) ) == true;
		}

		/// <summary>
		/// Uses the same code behind GMAD's c++ implementation (a third party function called globber)
		/// </summary>
		/// <param name="str">preferably a relative path</param>
		/// <param name="wildcard">the wildcard to match to, can contain * and ?</param>
		/// <returns></returns>
		public static bool IsWildcardMatching( string str , string wildcard )
		{
			int strIndex = 0;
			int wildIndex = 0;

			while( strIndex < str.Length && wildcard[wildIndex] != '*' )
			{
				if( wildcard[wildIndex] != str[strIndex] && wildcard[wildIndex] != '?' )
				{
					return false;
				}
				wildIndex++;
				strIndex++;
			}

			int cp = 0;
			int mp = 0;

			while( strIndex < str.Length )
			{
				if( wildcard[wildIndex] == '*' )
				{
					if( ++wildIndex >= wildcard.Length )
					{
						return true;
					}
					mp = wildIndex;
					cp = strIndex + 1;
				}
				else if( wildcard[wildIndex] == str[strIndex] || wildcard[wildIndex] == '?' )
				{
					wildIndex++;
					strIndex++;
				}
				else
				{
					wildIndex = mp;
					strIndex = cp++;
				}
			}

			while( wildIndex < wildcard.Length && wildcard[wildIndex] == '*' )
			{
				wildIndex++;
			}

			return wildIndex >= wildcard.Length;
		}
	}
}
