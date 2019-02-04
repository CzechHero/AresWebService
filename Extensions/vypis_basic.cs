using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using AresWebService.Extensions;

namespace AresWebService.WS_ARES_BASIC
{
	public partial class vypis_basic
	{
		/// <summary>
		/// returns true if the subject is VAT registered
		/// </summary>
		public bool IsVatRegistered
		{
			get
			{
				if (OverrideIsVatRegistered.HasValue)
					return OverrideIsVatRegistered.Value;
				return AresFlags.IsVatRegistered(this.PSU);
			}
		}

		/// <summary>
		/// returns true if the subject is in bankruptcy process or insolvent
		/// </summary>
		public bool IsProblematicSubject
		{
			get
			{
				if (OverrideIsProblematicSubject.HasValue)
					return OverrideIsProblematicSubject.Value;
				return AresFlags.IsProblematicSubject(this.PSU);
			}
		}
		/// <summary>
		/// if definned overrides IsVatRegistered
		/// </summary>
		[XmlIgnore]
		internal bool? OverrideIsVatRegistered { get; set; }
		/// <summary>
		/// if defined overrides IsProblematicSubject
		/// </summary>
		[XmlIgnore]
		internal bool? OverrideIsProblematicSubject { get; set; }
	}
}
