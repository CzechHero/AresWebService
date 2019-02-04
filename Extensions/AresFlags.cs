using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AresWebService.Extensions
{
	/// <summary>
	/// flags on subjects
	/// </summary>
	internal static class AresFlags
	{
		/*
		 * priznaky v this.Priznaky_subjektu
		 * 
		 * 1	rezervováno
		 * 2	příznak existence subjektu v Obchodním rejstříku
		 * 3	příznak existence subjektu ve statistickém Registru ekonomických subjektů
		 * 4	příznak existence subjektu v Registru živnostenského podnikání
		 * 5	příznak existence subjektu v Registru zdravotnických zařízení
		 * 6	příznak existence subjektu v Registru plátců daně z přidané hodnoty
		 * 7	příznak existence subjektu v Registru plátců spotřební daně
		 * 8	rezervováno
		 * 9	příznak existence subjektu v registru Centrální evidence úpadců - konkurz
		 * 10	příznak existence subjektu v registru Centrální evidence úpadců - vyrovnání
		 * 11	příznak existence subjektu v registru Centrální evidence dotací z rozpočtu
		 * 12	příznak existence subjektu v účelovém registru organizací systému ARIS
		 * 13	rezervováno
		 * 14	příznak existence subjektu v Registru církví a náboženských společností
		 * 15	příznak existence subjektu v Seznamu politických stran a hnutí
		 * 16	rezervováno
		 * 17	rezervováno
		 * 18	rezervováno
		 * 19	rezervováno
		 * 20	příznak existence subjektu v seznamu Občanských sdružení a spolků
		 * 21	příznak existence subjektu v Zemědělském registru
		 * 22	příznak existence subjektu v Insolvenčním rejstříku
		 * 23	příznak existence subjektu v Rejstříku škol a školských zařízení
		 * 24 - 30	rezervováno
		 * */


		/// <summary>
		/// returns true if the subject is VAT registered
		/// </summary>
		public static bool IsVatRegistered(string subjectFlags)
		{
			return ReturnPriznak(subjectFlags, 5, 'A', 'a', 'S', 's');
		}

		/// <summary>
		/// returns true if the subject is in bankruptcy process or insolvent
		/// </summary>
		public static bool IsProblematicSubject(string subjectFlags)
		{
			return ReturnPriznak(subjectFlags, 8) || ReturnPriznak(subjectFlags, 9) || ReturnPriznak(subjectFlags, 21, 'A', 'a', 'E', 'e');
		}

		private static bool ReturnPriznak(string subjectFlags, int pos, params char[] testChar)
		{
			if (subjectFlags == null || subjectFlags.Length < pos + 1)
				return false;
			if (testChar == null || testChar.Length <= 0)
				testChar = new char[] { 'A', 'a' };
			foreach (var c in testChar)
				if (subjectFlags[pos] == c)
					return true;
			return false;
		}
	}
}
