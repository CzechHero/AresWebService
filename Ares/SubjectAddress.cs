using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AresWebService.Ares
{
	public struct SubjectAddress
	{
		/// <summary>
		/// nazev ulice
		/// </summary>
		public string AresAlias_NU { get { return this.Street; } }
		/// <summary>
		/// adresa: Cislo popis
		/// </summary>
		public int AresAlias_CD { get { return this.HomeNr1; } } // NVARCHAR(10) = NULL,		-- 
		/// <summary>
		/// adresa: cislo orient.
		/// </summary>
		public string AresAlias_CO { get { return this.HomeNr2; } } // NVARCHAR(10) = NULL,		-- 
		/// <summary>
		/// adresa: nazev obec asi...
		/// </summary>
		public string AresAlias_N { get { return this.City; } } // NVARCHAR(50) = NULL,		-- 
		/// <summary>
		/// adresa: nazev mistni casti
		/// </summary>
		public string AresAlias_NMC { get { return this.CityPart; } }  // NVARCHAR(50) = NULL,		-- 
		/// <summary>
		/// adresa: kod statu
		/// </summary>
		public string AresAlias_KS { get { return this.CountryCode; } }  // NVARCHAR(255) = NULL,		-- 
		/// <summary>
		/// adresa: nazev statu
		/// </summary>
		public string AresAlias_NS { get { return this.Country; } }  //NVARCHAR(50) = NULL,		-- 
		/// <summary>
		/// PSC
		/// </summary>
		public string AresAlias_PSC { get { return this.ZIP; } } //NVARCHAR(20) = NULL,		-- adresa: 

		/// <summary>
		/// Street name
		/// </summary>
		public string Street { get; set; }

		/// <summary>
		/// Home number 01
		/// </summary>
		public int HomeNr1 { get; set; }
		/// <summary>
		/// Home number 02
		/// </summary>
		public string HomeNr2 { get; set; }
		/// <summary>
		/// City
		/// </summary>
		public string City { get; set; }
		/// <summary>
		/// CityPart
		/// </summary>
		public string CityPart { get; set; }
		/// <summary>
		/// CountryCode
		/// </summary>
		public string CountryCode { get; set; }
		/// <summary>
		/// Country
		/// </summary>
		public string Country { get; set; }
		/// <summary>
		/// ZIP
		/// </summary>
		public string ZIP { get; set; }
	}
}
