using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedNativeWifi;

namespace iCJLU.Utils
{
    public class Wifi
    {
        public static string getWlanName()
        {
            foreach (string ssid in NativeWifi.EnumerateConnectedNetworkSsids()
               .Select(x => x.ToString())) return ssid;
            return null;
        }
    }
}
