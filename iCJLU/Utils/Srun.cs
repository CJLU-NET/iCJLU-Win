using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace iCJLU.Utils
{
    public class Srun
    {
        private static readonly HttpClient client = new HttpClient();

        private static string host = "10.253.0.100";
        private static string baseUri = "http://10.253.0.100/";
        private static string getChallengeApi = "http://10.253.0.100/cgi-bin/get_challenge";
        private static string srunPortalApi = "http://10.253.0.100/cgi-bin/srun_portal";
        private static string srunPortalPhone = "http://10.253.0.100/srun_portal_phone.php";
        private static string srunPortalPC = "http://10.253.0.100/srun_portal_pc.php";
        private static string userInfoApi = "http://10.253.0.100/cgi-bin/rad_user_info";
        private static string detectApi = "http://10.253.0.100/v1/srun_portal_detect";

        /*phone_data = {
            'action': 'login',
            'username': 'xxxxx',
            'password': 'xxxxx',
            'ac_id': '1',
        }*/

        private static readonly int n = 200;
        private static readonly int type = 1;
        private static readonly string enc = "srun_bx1";

        private static string challenge = "";
        private static string hmd5 = "";
        private static string ac_id = "";
        private static string ip = "";
        private static string i = "";
        public static async Task<bool> Login(string username, string password)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "text/javascript, application/javascript, application/ecmascript, application/x-ecmascript, */*; q=0.01");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.8,zh-TW;q=0.7,zh-HK;q=0.5,en-US;q=0.3,en;q=0.2");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Cookie", "lang=zh-CN");
            client.DefaultRequestHeaders.Add("Host", host);
            client.DefaultRequestHeaders.Add("Referer", srunPortalPC + "?ac_id=1&theme=basic1");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36 Edg/110.0.1587.69");
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

            await getIp();
            await getChallenge(username);
            // await getAcId();
            ac_id = "5";
            i = "{SRBX1}" + Base64.Encode(XEncode.Encode(getInfo(username, password), challenge));
            hmd5 = calcPwd(password, challenge);
            string pass = "{MD5}" + hmd5;

            string param = $"callback=jQuery112405953212365516434_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"
                + "&action=login"
                + $"&username={username}"
                + $"&password={pass.Replace("{", "%7B").Replace("}", "%7D")}"
                + $"&ac_id={ac_id}"
                + $"&ip={ip}"
                + $"&chksum={calcChksum(getChksum(username))}"
                + $"&info={i.Replace("{", "%7B").Replace("}", "%7D").Replace("+", "%2B").Replace("/", "%2F").Replace("=", "%3D")}"
                + $"&n={n}"
                + $"&type={type}"
                + $"&os=Windows+10"
                + $"&name=Windows"
                + $"&double_stack=0"
                + $"&_={DateTimeOffset.Now.ToUnixTimeMilliseconds()}";

            string responseBody = await client.GetStringAsync(srunPortalApi + "?" + param);

            if (responseBody.Contains("\"error\":\"ok\",\"error_msg\":\"\"")) return true;

            return false;
        }

        public static async Task<bool> isLogin()
        {
            string param = $"callback=jQuery112405953212365516434_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"
                + $"&_={DateTimeOffset.Now.ToUnixTimeMilliseconds()}";
            string responseBody = await client.GetStringAsync(userInfoApi + "?" + param);

            if (responseBody.Contains("\"error\":\"ok\",\"error_msg\":\"\"")) return true;
            return false;
        }

        public static async Task<bool> Logout(string username)
        {
            string param = $"callback=jQuery112405953212365516434_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"
                + $"&action=logout"
                + $"&ac_id={ac_id}"
                + $"&ip={ip}"
                + $"&username={username}"
                + $"&_={DateTimeOffset.Now.ToUnixTimeMilliseconds()}";
            await client.GetStringAsync(srunPortalApi + "?" + param);
            return true;
        }

        private static string getChksum(string username) {
            string chkstr = challenge + username;
            chkstr += challenge + hmd5;
            chkstr += challenge + ac_id;
            chkstr += challenge + ip;
            chkstr += challenge + n;
            chkstr += challenge + type;
            chkstr += challenge + i;
            return chkstr;
        }

        private static string getInfo(string username, string password)
        {
            string info_temp = "{\"username\":\"";
            info_temp += username + "\",\"password\":\"";
            info_temp += password + "\",\"ip\":\"";
            info_temp += ip + "\",\"acid\":\"";
            info_temp += ac_id + "\",\"enc_ver\":\"";
            info_temp += enc + "\"}";

            File.WriteAllText("info.txt", info_temp);

            return info_temp.Replace(" ", "").Replace("'", "\"");
        }

        private static async Task getIp()
        {
            string res = await client.GetStringAsync(baseUri);
            Regex regex = new Regex("id=\"user_ip\" value=\"(.*?)\"");
            Match match = regex.Match(res);
            string value = match.Groups[1].Value;
            ip = value;
        }

        private static async Task getAcId()
        {
            string res = await client.GetStringAsync(baseUri);
            Regex regex = new Regex("id=\"ac_id\" value=\"(.*?)\"");
            Match match = regex.Match(res);
            string value = match.Groups[1].Value;
            ac_id = value;
        }

        private static async Task getChallenge(string username) {
            string time = (DateTimeOffset.Now.ToUnixTimeSeconds() * 1000).ToString(); ;
            string callback = "jQuery112406728703022454459_" + time;
            string param = "callback=" + callback + "&username=" + username + "&ip=" + ip + "&_=" + time;
            string res = await client.GetStringAsync(getChallengeApi + "?" + param);
            Regex regex = new Regex("\"challenge\":\"(.*?)\"");
            Match match = regex.Match(res);
            string value = match.Groups[1].Value;
            challenge = value;
        }

        private static string calcPwd(string data, string key)
        {
            byte[] hash;
            using (HMAC hmac = new HMACMD5(Encoding.Default.GetBytes(key)))
                hash = hmac.ComputeHash(Encoding.Default.GetBytes(data));

            var res = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
                res.Append(hash[i].ToString("x2"));

            return res.ToString();
        }

        private static string calcChksum(string data)
        {
            byte[] hash;
            using (SHA1 sha1 = SHA1.Create())
                hash = sha1.ComputeHash(Encoding.Default.GetBytes(data));

            var res = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
                res.Append(hash[i].ToString("x2"));
            File.WriteAllText("chksum.txt", res.ToString());
            return res.ToString();
        }
    }
}
