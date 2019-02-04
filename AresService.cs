using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AresWebService.Ares;
using AresBasic = AresWebService.WS_ARES_BASIC;
using AresRzp = AresWebService.WS_ARES_RZP;
using AresStandard = AresWebService.WS_ARES_STANDARD;
using AresOr = AresWebService.WS_ARES_OR;

namespace AresWebService
{
	public class AresService : IDisposable
	{
		AresBasic.HttpSoapBasicClient wsAresBasicClient;
		AresOr.HttpSoapOrClient wsAresOrClient;
		AresRzp.HttpSoapRzpClient wsAresRzpClient;
		AresStandard.HttpSoapStandardClient wsAresStandardClient;

		const string URL = "http://wwwinfo.mfcr.cz/cgi-bin/ares/xar.cgi";
		static readonly Regex reIco = new Regex(@"^\d{8}$");
		static readonly Regex reSpace = new Regex(@"\s*");

		private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, AresServiceResponseError> invalidIcoList = new System.Collections.Concurrent.ConcurrentDictionary<string, AresServiceResponseError>();



		/// <summary>
		/// creates a new instance
		/// </summary>
		public AresService()
		{
		}

		public void Dispose()
		{
			CloseServiceClient(ref wsAresBasicClient);
			CloseServiceClient(ref wsAresOrClient);
			CloseServiceClient(ref wsAresRzpClient);
			CloseServiceClient(ref wsAresStandardClient);
		}

		private static void CloseServiceClient<T>(ref T service)
			where T : class, ICommunicationObject, IDisposable
		{
			if (service == null)
				return;
			switch (service.State)
			{
				case CommunicationState.Created:
					break;
				case CommunicationState.Closing:
					break;
				case CommunicationState.Closed:
					break;
				case CommunicationState.Faulted:
					throw new InvalidOperationException("AresService:Check4Connection. Service faulted after creating a new instance. Weird!");
				case CommunicationState.Opened:
					service.Close();
					break;
				case CommunicationState.Opening:
					service.Abort();
					break;
				default:
					throw new InvalidOperationException(string.Format("AresService:CloseServiceClient. Service in uknown state {0}. Weird!", service.State));
			}
			service = null;
		}

		/// <summary>
		/// returns a subject according to the ICO
		/// </summary>
		public bool FindByIco(string[] ico, out SubjectInfo[] subject, out int wsCalls)
		{
			//TODO
			subject = null;
			wsCalls = 0;
			return false;
		}/// <summary>
		/// returns a subject according to the ICO
		/// </summary>
		public async Task<AresRequest> FindByIcoAsync(string ico)
		{
			var requests = new AresRequest[] { new AresRequest() { FindIco = ico } };
			await FindAsync(requests);
			return requests[0];
		}

		/// <summary>
		/// returns a subject according to the ICO
		/// </summary>
		public async Task FindAsync(AresRequest[] requests)
		{
			if (requests == null)
				requests = new AresRequest[] { };

			var i = -1;
			var aresIcoRequests = new List<AresBasic.dotaz>();
			foreach (var request in requests)
			{
				i++;
                if (request.Error != null && request.Error.Type == AresServiceResponseError.ErrorType.InvalidIco)
                    continue;
                if (invalidIcoList.TryGetValue(request.FindIco, out AresServiceResponseError prevErr) && prevErr.IsErrorValid)
				{
					request.Error = prevErr;
					continue;
				}
				else
				{
                    var q1 = new AresBasic.dotaz
                    {
                        Items = new object[] { request.FindIco },
                        FindIco = request.FindIco,
                        Pomocne_ID = i,
                        ItemsElementName = new AresBasic.ItemsChoiceType[] { AresBasic.ItemsChoiceType.ICO }
                    };
                    aresIcoRequests.Add(q1);
				}
			}
			if (aresIcoRequests.Count <= 0)
				return;

			OpenService(ref wsAresBasicClient, (b, epa) => { return new AresBasic.HttpSoapBasicClient(b, epa); }, TimeSpan.FromSeconds(20 * requests.Length));
            var q = new AresBasic.Ares_dotazy
            {
                user_mail = "user@email.com",
                Dotaz = aresIcoRequests.ToArray(),
                dotaz_pocet = aresIcoRequests.Count,
                dotaz_typ = AresBasic.ares_dotaz_typ.Basic //zde MUSI byt BASIC...
            };
            AresBasic.BasicAnswer answers;
			try
			{
				answers = await wsAresBasicClient.GetXmlFileAsync(q);
			}
			catch (System.ServiceModel.Security.MessageSecurityException)
			{
				foreach (var r in aresIcoRequests)
					invalidIcoList.GetOrAdd(r.FindIco, AresServiceResponseError.TemporaryProblem);
				throw;
			}
			catch (Exception ex)
			{
				//probably System.ServiceModel.CommunicationException
				foreach (var r in aresIcoRequests)
					invalidIcoList.GetOrAdd(r.FindIco, AresServiceResponseError.TemporaryProblem);
				throw new InvalidOperationException(string.Format("AresService: failed to getXml for ICO [{0}], error: {1}",
					string.Join(",", aresIcoRequests.Select(p => p.FindIco)), ex), ex);
			}

			if (answers != null && answers.Ares_odpovedi != null && answers.Ares_odpovedi.Odpoved != null && answers.Ares_odpovedi.Odpoved.Length > 0)
			{
				Exception lastException = null;
				i = 0;
				foreach (var answer in answers.Ares_odpovedi.Odpoved)
				{
					if (answer == null)
					{
						lastException = new NullReferenceException("AresService: answers.Odpoved is null!!"); //should never happend...
						continue;
					}
					var origDotaz = i < aresIcoRequests.Count ? aresIcoRequests[i++] : null;
					if (origDotaz.Pomocne_ID != answer.PID)
						//we are out of original order! lets try to find the original request
						origDotaz = aresIcoRequests.FirstOrDefault((d) => d.Pomocne_ID == answer.PID);
					if (origDotaz == null)
					{
						lastException = new InvalidOperationException(string.Format("AresService: can not find original request with PID {0}", answer.PID));
						continue;
					}
					var originalRequest = origDotaz.Pomocne_ID >= 0 && origDotaz.Pomocne_ID < requests.Length ? requests[origDotaz.Pomocne_ID] : null;
					if (originalRequest == null)
						// we couldnt find the original request by ID
						originalRequest = requests.FirstOrDefault((ar) => ar.FindIco == origDotaz.FindIco);
					if (originalRequest == null)
					{
						lastException = new InvalidOperationException(string.Format("AresService: can not find original request in aresIcoRequests with ICO {0}", origDotaz.FindIco));
						continue;
					}
					if (answer.E != null && answer.VBAS == null)
					{
						if (answer.E.EK == 1)
							//mame tu chybu v navratu..subjekt PRAVDEPODOBNE neexistuje nebo zanikl... answer.E.ET bude zrejme "subjekt zanikl", EK bude 1
							continue;
						//mame tu chybu v navratu...ale neznamou. Zaevidujeme!
						invalidIcoList.GetOrAdd(originalRequest.FindIco, AresServiceResponseError.BasicResponseWrong);
						lastException = new InvalidOperationException(String.Format("IC {0}: ARESBasic ANSWER was good, but error returned was not NULL, in fact it was {1} ({2}). Wrong list so far: [{3}]",
							originalRequest.FindIco, answer.E.EK, answer.E.ET, string.Join(",", invalidIcoList.Keys)));
						continue;
					}
					if (answer.VBAS != null && answer.VBAS.Length == 1)
					{
						var basicInfoAnswer = answer.VBAS[0];
						originalRequest.Response = CreateSubjectFromBasicInfoAnswer(basicInfoAnswer);
						var callRzp = basicInfoAnswer.OF.zdroj == AresBasic.zdroj_type.RZP;
						switch (originalRequest.Response.LegalForm.Code) //see http://wwwinfo.mfcr.cz/ares/aresPrFor.html.cz
						{
							//nejsou v RZP... case 104: // Samostatně hospodařící rolník zapsaný v obchodním rejstříku ???
							//nejsou v RZP... case 107: // Zemědělský podnikatel - fyzická osoba nezapsaná v obchodním rejstříku
							//nejsou v RZP... case 108: // Zemědělský podnikatel - fyzická osoba zapsaná v obchodním rejstříku
							case 100: // Podnikající osoba tuzemská:
							case 101: // Fyzická osoba podnikající dle živnostenského zákona nezapsaná v obchodním rejstříku
							case 102: // Fyzická osoba podnikající dle živnostenského zákona zapsaná v obchodním rejstříku
							case 105: // Fyzická osoba podnikající dle jiných zákonů než živnostenského a zákona o zemědělství nezapsaná v obchodním rejstříku
							case 106: // Fyzická osoba podnikající dle jiných zákonů než živnostenského a zákona o zemědělství zapsaná v obchodním rejstříku

								callRzp = true;
								break;
						}
						if (callRzp)
						{
							//mame zde zivnostnika...musime zkontrolovat, zda neprerusil...
							var ex = await CallRzpService(originalRequest);
							if (ex != null)
								lastException = ex;
						}
						continue;
					}
					else
					{
						invalidIcoList.GetOrAdd(originalRequest.FindIco, AresServiceResponseError.BasicResponseWrong);
						lastException = new InvalidOperationException(String.Format("IC {0}: ARESBasic ANSWER was good, but VBAS returned NULL or wrong count. Wrong list so far: [{1}]",
							originalRequest.FindIco, string.Join(",", invalidIcoList.Keys)));
						continue;
					}
				}
				if (lastException != null)
					throw lastException;
			}
			else
			{
				foreach (var request in requests)
					if (request.Error != null && request.Error.Type == AresServiceResponseError.ErrorType.None) //v requestu neni chyba, ale ARES chybu vratil. Druha moznost je, ze je chyba v requestu = invalidICO...a to se kontroluje vzdy znovu...
						invalidIcoList.GetOrAdd(request.FindIco, AresServiceResponseError.BasicResponseWrong);
				throw new InvalidOperationException(String.Format("ARESBasic ANSWER returned NULL or wrong answer. Wrong list so far: [{0}]",
					string.Join(",", invalidIcoList.Keys)));
			}

		}

		private SubjectInfo CreateSubjectFromBasicInfoAnswer(AresBasic.vypis_basic basicInfoAnswer)
		{
			if (basicInfoAnswer == null)
				throw new InvalidOperationException("AresService: answer.VBAS[0] is null");
			var addr = basicInfoAnswer.AA ?? new AresBasic.adresa_ARES();
			return new SubjectInfo()
			{
				Address = new SubjectAddress()
				{
					City = addr.N,
					CityPart = addr.NMC,
					Country = addr.NS,
					CountryCode = addr.KS,
					HomeNr1 = addr.CD,
					HomeNr2 = addr.CO,
					Street = addr.NU,
					ZIP = addr.PSC
				},
				AresAlias_KPP = (basicInfoAnswer.KPP ?? new AresBasic.kategorie_poctu_pracovniku() { Value = "(null)" }).Value,
				CompanyName = (basicInfoAnswer.OF ?? new AresBasic.obchodni_firma()).Value,
				DateCreated = basicInfoAnswer.DVSpecified ? (DateTime?)basicInfoAnswer.DV : null,
				DateTerminated = basicInfoAnswer.DZSpecified ? (DateTime?)basicInfoAnswer.DZ : null,
				Dic = (basicInfoAnswer.DIC ?? new AresBasic.dic()).Value,
				Ico = (basicInfoAnswer.ICO ?? new AresBasic.ico()).Value,
				IsProblematicSubject = basicInfoAnswer.IsProblematicSubject,
				IsVatRegistered = basicInfoAnswer.IsVatRegistered,
				LegalForm = basicInfoAnswer.PF != null ? new SubjectLegalForm()
				{
					Code = basicInfoAnswer.PF.KPF,
					Name = basicInfoAnswer.PF.NPF
				} : new SubjectLegalForm() { },
				TradeRegister = basicInfoAnswer.ROR != null && basicInfoAnswer.ROR.Length > 0 && basicInfoAnswer.ROR[0].SZ != null && basicInfoAnswer.ROR[0].SZ.Length > 0 && basicInfoAnswer.ROR[0].SZ[0].SD != null ?
							basicInfoAnswer.ROR[0].SZ[0].SD.T + " " + basicInfoAnswer.ROR[0].SZ[0].OV : null
			};
		}

		private async Task<Exception> CallRzpService(AresRequest request)
		{
			if (request.Response.DateTerminated.HasValue)
			{
				return null; //jedine, co nyni aktualizujeme z RZP je datum preruseni zivnosti. Pokud je jiz nastaveno z BASIC dotazu, neni treba jiz volat ARES...
			}

            if (invalidIcoList.TryGetValue(request.FindIco, out AresServiceResponseError prevErr)) //predchozi volani v teto web app session nebylo uspesne..proc volat znovu? Nevolame, vracime se!
            {
                request.Error = prevErr;
                return null;
            }


            //mame zde zivnostnika...musime zkontrolovat, zda neprerusil...
            OpenService(ref wsAresRzpClient, (b, epa) => { return new AresRzp.HttpSoapRzpClient(b, epa); });
            var q = new AresRzp.Ares_dotazy
            {
                user_mail = "user@email.com"
            };
            var q1 = new AresRzp.dotaz
            {
                ICO = request.FindIco
            };
            q.Dotaz = new AresRzp.dotaz[] { q1 };
			q.dotaz_pocet = 1;
			q.vystup_format = AresRzp.vystup_format.XML;
			q.dotaz_typ = AresRzp.ares_dotaz_typ.Vypis_RZP;
			q.answerNamespaceRequired = "http://wwwinfo.mfcr.cz/ares/xml_doc/schemas/ares/ares_answer_rzp/v_1.0.4";
			AresRzp.RzpAnswer answers;
			try
			{
				answers = await wsAresRzpClient.GetXmlFileAsync(q);
			}
			catch (System.ServiceModel.Security.MessageSecurityException ex)
			{
				invalidIcoList.GetOrAdd(request.FindIco, AresServiceResponseError.TemporaryProblem);
				return ex;
			}
			catch (Exception ex)
			{	//probably System.ServiceModel.CommunicationException
				invalidIcoList.GetOrAdd(request.FindIco, AresServiceResponseError.TemporaryProblem);
				return new InvalidOperationException(string.Format("AresService: failed to getXml (RZP) for ICO [{0}], error: {1}",
					request.FindIco, ex), ex);
			}
			if (answers != null && answers.Ares_odpovedi != null && answers.Ares_odpovedi.Odpoved != null && answers.Ares_odpovedi.Odpoved.Length > 0)
			{
				var answer = answers.Ares_odpovedi.Odpoved[0];
				if (answer != null && answer.Pocet_zaznamu >= 1 && answer.Vypis_RZP != null)
				{
					var alesponJednaJeAktivni = false;
					DateTime datumPreruseni = new DateTime(1900, 1, 1);
					foreach (var vypis in answer.Vypis_RZP)
					{
						if (vypis.Zakladni_udaje == null || vypis.Zakladni_udaje.Stav != AresRzp.stav_rzp.A)
							continue;
						foreach (var zivnost in vypis.Zivnosti)
						{
							if (zivnost.Preruseni_odSpecified && zivnost.Preruseni_od > datumPreruseni && (!zivnost.Preruseni_doSpecified || zivnost.Preruseni_do <= DateTime.Now))
							{
								datumPreruseni = zivnost.Preruseni_od;
								continue;
							}
							if (zivnost.Pozastaveni_odSpecified && zivnost.Pozastaveni_od > datumPreruseni && (!zivnost.Pozastaveni_doSpecified || zivnost.Pozastaveni_do <= DateTime.Now))
							{
								datumPreruseni = zivnost.Pozastaveni_od;
								continue;
							}
							if (zivnost.ZanikSpecified && zivnost.Zanik > datumPreruseni && (!zivnost.ZahajeniSpecified || zivnost.Zahajeni <= DateTime.Now))
							{
								datumPreruseni = zivnost.Zanik;
								continue;
							}
							switch (zivnost.Stav)
							{
								case AresRzp.stav_zivnosti.Z: //zanikla...
								case AresRzp.stav_zivnosti.P: //pozastavena...
								case AresRzp.stav_zivnosti.N: //neumime zjistit...
									continue;
								case AresRzp.stav_zivnosti.A:
									alesponJednaJeAktivni = true;
									break;
								default:
									//unknown state...a new one? ignoring for now...
									continue;
							}
							break;
						}
					}
					if (!alesponJednaJeAktivni)
						request.Response.DateTerminated = datumPreruseni;
				}
				else
				{
					invalidIcoList.GetOrAdd(request.FindIco, AresServiceResponseError.RzpResponseWrong);
					return new InvalidOperationException(String.Format("IC {0}: Basic response indicated we have RZP subject, but RZP service returned Vypis_RZP = NULL or wrong count. Wrong list so far: [{1}]",
						request.FindIco, string.Join(",", invalidIcoList.Keys)));
				}
			}
			else
			{
				invalidIcoList.GetOrAdd(request.FindIco, AresServiceResponseError.RzpResponseWrong);
				return new InvalidOperationException(String.Format("IC {0}: Basic response indicated we have RZP subject, but RZP service returned ANSWER = NULL or wrong count. Wrong list so far: [{1}]",
						request.FindIco, string.Join(",", invalidIcoList.Keys)));
			}
			return null;
		}

		public static bool CheckValidIco(ref string ico)
		{
			ico = reSpace.Replace(ico ?? "", "");
			if (string.IsNullOrEmpty(ico) || !reIco.IsMatch(ico))
				return false;
			int checknum = ico[ico.Length - 1] - 48;
			int sum = 0;
			int result;

			for (int index = ico.Length - 2, multiplier = 2; index >= 0; index--,
										multiplier++)
			{
				int num = ico[index] - 48;
				sum += num * multiplier;
			}

			result = 11 - (sum % 11);
			if (result >= 10)
				result -= 10;

			return checknum == result;
		}

		///// <summary>
		///// resets the channel
		///// </summary>
		//public void Reset()
		//{
		//	if (wsAresStandardClient != null)
		//	{
		//		CloseServiceClient(wsAresStandardClient);
		//	}
		//}
        
		private static void OpenService<T>(ref T service, Func<BasicHttpBinding, EndpointAddress, T> createInstanceDelegate, TimeSpan? sendTimeout = null)
			where T : class, ICommunicationObject
        {
			if (createInstanceDelegate == null)
				throw new ArgumentNullException("createInstanceDelegate", "AresService:Check4Connection");
			if (service != null && service.State == CommunicationState.Faulted)
				service = null;
			if (service == null)
			{
                var b = new BasicHttpBinding
                {
                    MaxReceivedMessageSize = 524288
                };
                if (sendTimeout.HasValue)
					b.SendTimeout = sendTimeout.Value;
				var epa = new EndpointAddress(URL);
				service = createInstanceDelegate(b, epa);
			}
			switch (service.State)
			{
				case CommunicationState.Created:
					service.Open();
					break;
				case CommunicationState.Opened:
					break;
				case CommunicationState.Closing:
					service.Abort();
					service.Open();
					break;
				case CommunicationState.Closed:
					service.Open();
					break;
				case CommunicationState.Faulted:
					throw new InvalidOperationException("AresService:Check4Connection. Service faulted after creating a new instance. Weird!");
				default:
					break;
			}
		}
	}
}
