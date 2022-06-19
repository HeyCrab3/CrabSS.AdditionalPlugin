using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrabSS.WebConsoleDaemon
{
    internal class Settings
    {
        public static string userName { get; set; }
        public static string passWord { get; set; }
    }
    internal class Crypto
    {
        public static string confVersion { get; set; }
        public static string Encrypt { get; set; }
        public static string Token { get; set; }
    }
}
