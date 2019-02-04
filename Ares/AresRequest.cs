using System;
using System.Linq;

namespace AresWebService.Ares
{
	/// <summary>
	/// Requst to the ARES service
	/// </summary>
	public class AresRequest
	{
        private string findIcoStored;

        /// <summary>
        /// Find by ICO
        /// </summary>
        public string FindIco
		{
			get { return findIcoStored; }
			set
			{
				findIcoStored = value;
				if (!AresService.CheckValidIco(ref findIcoStored))
					Error = AresServiceResponseError.InvalidIco;
			}
		}

		/// <summary>
		/// Stores the response
		/// </summary>
		public SubjectInfo Response { get; set; }

		/// <summary>
		/// Was the response succesful?
		/// </summary>
		public bool Succesful
		{
			get
			{
				return Response != null && (Error == null || Error == AresServiceResponseError.None);
			}
		}

		/// <summary>
		/// Error from WS service
		/// </summary>
		public AresServiceResponseError Error { get; set; }
	}
}
