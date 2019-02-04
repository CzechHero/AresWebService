using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AresWebService.Ares
{
	/// <summary>
	/// pravni forma
	/// </summary>
	public struct SubjectLegalForm
	{
		/// <summary>
		/// kod
		/// </summary>
		public short Code { get; set; }
		/// <summary>
		/// nazev
		/// </summary>
		public string Name { get; set; }
	}
}
