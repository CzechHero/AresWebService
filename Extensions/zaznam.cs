using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AresWebService.Extensions;

namespace AresWebService.WS_ARES_STANDARD
{
	public partial class zaznam
	{
		/// <summary>
		/// returns true if the subject is VAT registered
		/// </summary>
		public bool IsVatRegistered
		{
			get
			{
				return AresFlags.IsVatRegistered(Priznaky_subjektu);
			}
		}

		/// <summary>
		/// returns true if the subject is in bankruptcy process or insolvent
		/// </summary>
		public bool IsProblematicSubject
		{
			get
			{
				return AresFlags.IsProblematicSubject(Priznaky_subjektu);
			}
		}
	}
}
