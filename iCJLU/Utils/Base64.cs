using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace iCJLU.Utils
{
	public class Base64
	{
		private static readonly string altAlphabet = "LVoJPiCN2R8G90yg+hmFHuacZ1OWMnrsSTXkYpUq/3dlbfKwv6xztjI7DeBE45QA";
		private static readonly string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

		public static string Encode(string msg)
		{
			byte[] bytes = new byte[msg.Length];
			for (int i = 0; i < msg.Length; i++)
			{
				bytes[i] = (byte)msg[i];
			}
			string tmp = Convert.ToBase64String(bytes);
			StringBuilder result = new();
			for (int i = 0; i < tmp.Length; i++)
			{
				if (alphabet.Contains(tmp[i])) result.Append(altAlphabet[alphabet.IndexOf(tmp[i])]);
				else result.Append(tmp[i]);
			}
			return result.ToString();
		}
	}
}
