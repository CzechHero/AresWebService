using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AresWebService.Ares
{
	/// <summary>
	/// Info about the subject
	/// </summary>
	public class SubjectInfo
	{
        /*
		* @subject_ICO NVARCHAR(20),			-- ico for testing
		* @subject_NotFound BIT = 0,			-- set to 1 if the subject was not found!
		* @subject_DV DATETIME = NULL,			-- datum vzniku
		* @subject_DZ DATETIME = NULL,			-- datum zaniku
		* @subject_OF NVARCHAR(96) = NULL,		-- Obchodni nazev
		* @subject_PF_KOD INT = NULL,			-- pravni forma...kod
		* @subject_PF_NAZEV NVARCHAR(100) = NULL,		-- pravni forma...nazev
		* @subject_AA_NU NVARCHAR(64) = NULL,		-- adresa: nazev ulice
		* @subject_AA_CD NVARCHAR(10) = NULL,		-- adresa: Cislo popis
		* @subject_AA_CO NVARCHAR(10) = NULL,		-- adresa: cislo orient.
		* @subject_AA_N NVARCHAR(50) = NULL,		-- adresa: nazev obec asi...
		* @subject_AA_NMC NVARCHAR(50) = NULL,		-- adresa: nazev mistni casti
		* @subject_AA_KS NVARCHAR(255) = NULL,		-- adresa: kod statu
		* @subject_AA_NS NVARCHAR(50) = NULL,		-- adresa: nazev statu
		* @subject_AA_PSC NVARCHAR(20) = NULL,		-- adresa: PSC
		* @subject_IsVatRegistered BIT = NULL,		-- isVATRegistered...
		* @subject_IsProblematicSubject BIT = NULL,	-- isProblemnaticSubject...
		* @subject_TradeRegister NVARCHAR(200) = NULL,	-- registrace u soudu/sro atp.
		* @subject_KPP NVARCHAR(50) = NULL,		-- kategorie poctu pracovniku
		* @subject_DIC NVARCHAR(20) = NULL		-- DIC
		* 
		*/
        /// <summary>
        /// DIČ
        /// </summary>
        public string Dic { get; set; }

        /// <summary>
        /// Počet pracovníků
        /// </summary>
        public string AresAlias_KPP { get; set; }

        /// <summary>
        /// Obchodní rejstřík
        /// </summary>
        public string TradeRegister { get; set; }

        /// <summary>
        /// Insolvence/úpadek
        /// </summary>
        public bool IsProblematicSubject { get; set; }

        /// <summary>
        /// DIČ ověřeno
        /// </summary>
        public bool IsVatRegistered { get; set; }

        /// <summary>
        /// Adresa subjektu
        /// </summary>
        public SubjectAddress Address { get; set; }

        /// <summary>
        /// Právní forma
        /// </summary>
        public SubjectLegalForm AresAlias_PF
		{
			get
			{
				return LegalForm;
			}
		}
		/// <summary>
		/// Legal form
		/// </summary>
		public SubjectLegalForm LegalForm { get; set; }

		/// <summary>
		/// Obchodní název
		/// </summary>
		public string AresAlias_OF
		{
			get
			{
				return CompanyName;
			}
		}
		/// <summary>
		/// Company name
		/// </summary>
		public string CompanyName{get;set;}

		/// <summary>
		/// Datum zániku / přerušení činnosti subjektu
		/// </summary>
		public DateTime ? AresAlias_DZ
		{
			get
			{
				return DateTerminated;
			}
		}
		/// <summary>
		/// Date created
		/// </summary>
		public DateTime ? DateCreated{get;set;}

		/// <summary>
		/// Datum vzniku
		/// </summary>
		public DateTime? AresAlias_DV
		{
			get
			{
				return DateCreated;
			}
		}
		/// <summary>
		/// Date terminated
		/// </summary>
		public DateTime? DateTerminated { get; set; }

        /// <summary>
        /// IČO
        /// </summary>
        public string Ico { get; set; }
    }
}