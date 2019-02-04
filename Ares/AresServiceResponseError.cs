using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AresWebService.Ares
{
	/// <summary>
	/// Error type when returned from WS call
	/// </summary>
	public class AresServiceResponseError
	{
		#region Error types new instances
		public static AresServiceResponseError None 
		{
			get
			{
				return new AresServiceResponseError(ErrorType.None);
			}
		}
		public static AresServiceResponseError InvalidIco		 
		{
			get
			{
				return new AresServiceResponseError(ErrorType.InvalidIco);
			}
		}
		public static AresServiceResponseError BasicResponseWrong	 
		{
			get
			{
				return new AresServiceResponseError(ErrorType.BasicResponseWrong);
			}
		}
		public static AresServiceResponseError RzpResponseWrong		 
		{
			get
			{
				return new AresServiceResponseError(ErrorType.RzpResponseWrong);
			}
		}
		public static AresServiceResponseError TemporaryProblem		 
		{
			get
			{
				return new AresServiceResponseError(ErrorType.TemporaryProblem);
			}
		} 
	#endregion

		#region public ErrorType Type
		/// <summary>
		/// error type
		/// </summary>
		public ErrorType Type
		{
			get
			{
				return this.typeStored;
			}
		}
		private readonly ErrorType typeStored; 
		#endregion

		#region public DateTime Occured
		/// <summary>
		/// when occured
		/// </summary>
		public DateTime Occured
		{
			get
			{
				return this.occuredStored;
			}
		}
		private readonly DateTime occuredStored; 
		#endregion

		#region #Ctor
		private AresServiceResponseError(ErrorType type)
		{
			typeStored = type;
			occuredStored = DateTime.UtcNow;
		} 
		#endregion

		public bool IsErrorValid
		{
			get
			{
				switch (typeStored)
				{
					case ErrorType.None:
						return false;
					case ErrorType.TemporaryProblem:
						return (DateTime.UtcNow - occuredStored).TotalMinutes > 135; //temp error is valid only for 135 minutes
					default:
						return true;
				}
			}
		}

		public override bool Equals(object obj)
		{
			AresServiceResponseError temp = obj as AresServiceResponseError;
			if (temp == null)
				return false;
			return this.Equals(temp);
		}

		public bool Equals(AresServiceResponseError value)
		{
			if (object.ReferenceEquals(null, value))
			{
				return false;
			}
			if (object.ReferenceEquals(this, value))
			{
				return true;
			}
			return this.typeStored == value.typeStored;
		}

		/// <summary>
		/// Error type
		/// </summary>
		public enum ErrorType
		{
			None,
			InvalidIco,
			BasicResponseWrong,
			RzpResponseWrong,
			TemporaryProblem
		}

		public override int GetHashCode()
		{
			return (int)typeStored;
		}

		public static bool operator == (AresServiceResponseError a, AresServiceResponseError b)
		{
			if (object.ReferenceEquals(null, a))
				return object.ReferenceEquals(null, b);
			return a.Equals(b);
		}
		public static bool operator !=(AresServiceResponseError a, AresServiceResponseError b)
		{
			if (object.ReferenceEquals(null, a))
				return !object.ReferenceEquals(null, b);
			return !a.Equals(b);
		}
	}
}
