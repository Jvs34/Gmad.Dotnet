﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Gmad.Shared
{
	public class AddonInfo
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string WorkshopType { get; set; }

		/// <summary>
		/// To remain 0 until gmpublish gives you an id, not sure why this is saved as a float in the file
		/// </summary>
		public uint WorkshopID { get; set; }

		/// <summary>
		/// The logo image, this is a relative path
		/// </summary>
		public string LogoImage { get; set; }
		public List<string> Tags { get; set; }

		/// <summary>
		/// Only used by Falco's gmosh I think
		/// </summary>
		public List<string> IgnoreWildcard { get; set; }
	}
}