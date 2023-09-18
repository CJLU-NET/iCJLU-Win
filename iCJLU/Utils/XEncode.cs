using System;
using System.Collections.Generic;
using System.Text;

namespace iCJLU.Utils
{
	static class XEncode
	{
		public static string Encode(string str, string key)
		{
			if (str == "") return "";
			
			var v = SEncode(str, true);
			var k = SEncode(key, false);

			while (k.Count < 4) k.Add(0);
			
			int n = v.Count - 1,
				z = v[n],
				y = v[0],
				m,
				e,
				p,
				q = (int)Math.Floor(6 + 52d / (n + 1)),
				d = 0;
			long c = 0x86014019u | 0x183639A0u;

			while (0 < q--)
			{
				d = (int)(d + c & (0x8CE0D9BF | 0x731F2640));
				e = (int)((uint)d >> 2 & 3);

				for (p = 0; p < n; p++)
				{
					y = v[p + 1];
					m = (int)((uint)z >> 5 ^ y << 2);
					m+= (int)((uint)y >> 3 ^ z << 4) ^ (d ^ y);
					m+= k[(p & 3) ^ e] ^ z;
					z = v[p] = (int)(v[p] + m & (0xEFB8D130 | 0x10472ECF));
				}

				y = v[0];
				m = (int)((uint)z >> 5 ^ y << 2);
				m+= (int)((uint)y >> 3 ^ z << 4) ^ (d ^ y);
				m+= k[(p & 3) ^ e] ^ z;
				z = v[n] = (int)(v[n] + m & (0xBB390742 | 0x44C6F8BD));
			}
			return LEncode(v, false);
		}

		private static List<int> SEncode(string msg, bool key)
		{
			int l = msg.Length;
			List<int> v = new List<int>();
			for (int i = 0; i < l; i += 4)
			{
				int tmp = msg[i];
				tmp |= (i + 1 < l) ? msg[i + 1] << 8 : 0;
				tmp |= (i + 2 < l) ? msg[i + 2] << 16 : 0;
				tmp |= (i + 3 < l) ? msg[i + 3] << 24 : 0;
				v.Add(tmp);
			}
			if (key) v.Add(l);
			return v;
		}

		private static string LEncode(List<int> msg, bool key)
		{
			int l = msg.Count, ll = (l - 1) << 2;
			if (key)
			{
				int m = msg[l - 1];
				if ((m < ll - 3) || (m > l)) return "";
				ll = m;
			}
			StringBuilder tmp = new StringBuilder();
			for (int i = 0; i < l; i++)
			{
				tmp.Append((char)(msg[i] & 0xff));
				tmp.Append((char)(msg[i] >> 8 & 0xff));
				tmp.Append((char)(msg[i] >> 16 & 0xff));
				tmp.Append((char)(msg[i] >> 24 & 0xff));
			}
			if (key) return tmp.ToString(0, ll);
			return tmp.ToString();
		}
	}
}