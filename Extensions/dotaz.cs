using System;
using System.Linq;
using System.Xml.Serialization;

namespace AresWebService.WS_ARES_BASIC
{
	public partial class dotaz 
	{
		/// <summary>
		/// if definned overrides IsVatRegistered
		/// </summary>
		[XmlIgnore]
		public string FindIco { get; set; }
	}
}
