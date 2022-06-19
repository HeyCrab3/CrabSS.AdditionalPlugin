// See https://aka.ms/new-console-template for more information
using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

namespace CrabSS.WebConsoleDaemon;
public partial class hi {
    public static void Main()
    {
        Console.Title = "正在初始化";
        Console.WriteLine("CrabSS Web 控制台 守护进程 v1");
        Console.WriteLine("> 请注意：这个程序需要 Token 才能运行。");
        if (File.Exists(@"config.json"))
        {
            Console.Clear();
            Console.Title = "CrabSS Web 控制台 守护进程 进程PID: " + Process.GetCurrentProcess().Id
+ " 守护密钥：null";
            using (StreamReader readFile = File.OpenText("config.json"))
            {
                using (JsonTextReader reader = new JsonTextReader(readFile)) //using Newtonsoft.Json
                {
                    JObject oJson = (JObject)JToken.ReadFrom(reader);
                    string userName = Settings.userName = oJson["userName"].ToString();
                    string passWord = Settings.passWord = oJson["passWord"].ToString();
                    reader.Close();
                }
            }
            using (StreamReader readFile = File.OpenText("crypto.json"))
            {
                using (JsonTextReader reader = new JsonTextReader(readFile)) //using Newtonsoft.Json
                {
                    JObject oJson = (JObject)JToken.ReadFrom(reader);
                    string confVersion = Crypto.confVersion = oJson["confVersion"].ToString();
                    string Encrypt = Crypto.Encrypt = oJson["Encrypt"].ToString();
                    string Token = Crypto.Token = oJson["Token"].ToString();
                    reader.Close();
                }
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("登录用户名：" + Settings.userName + "\n配置版本：" + Crypto.confVersion + "\n加密方式：" + Crypto.Encrypt + "\nAES 解密密钥：" + Crypto.Token);
            if (Crypto.confVersion != "v1")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("! 配置版本过低或损坏，请尝试重新生成！");
                Console.WriteLine("按任意键退出...");
                Console.ReadLine();
                Environment.Exit(1000);
            }
            if(Crypto.Encrypt == "Manual")
            {
                Console.WriteLine("？ 请输入您的AES加密密钥以解锁配置文件");
                string token = Console.ReadLine();
                try
                {
                    string passwd = AESDecrypt(Settings.passWord,token,"");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("√ 解密完成");
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("# 正在初始化 Http 服务器");
                } 
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("! 无效的解密密钥！请检查您的秘钥是否正确！\n" + ex);
                    Console.WriteLine("按任意键退出...");
                    Console.ReadLine();
                    Environment.Exit(1002);
                }
            }
        }
        else
        {
            Console.Title = "初次使用向导";
            Console.WriteLine("配置不存在，程序将引导您配置。");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("? 登录用户名：");
            string userName = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("> 登录用户名：" + userName);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("? 后台管理密码：");
            string passWord = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("> 登录用户名：" + userName);
            Console.WriteLine("> 后台管理密码：" + passWord);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("? 自定义密钥加密");
            Console.WriteLine("? 我们使用 AES 来加密您的用户数据。默认的密码是一串 UUID，写入到 crypto.json 中。自定义密码将不会储存，需要您每次开启程序后解密。");
            Console.WriteLine("输入 Y 以使用默认密钥，N 使用自定义密钥。如果不是Y/N，自动选择自定义密钥。");
            string isCustomEncrypt = Console.ReadLine();
            if (isCustomEncrypt == "Y")
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("> 使用默认密钥");
                string key = Guid.NewGuid().ToString();
                Console.WriteLine("> 加密使用的密钥：" + key);
                string encrypted_pwd = AESEncrypt(passWord,key,"");
                try
                {
                    Settings settings = new();
                    string json = JsonMapper.ToJson(settings); //using LitJson
                    StreamWriter sw = new StreamWriter(System.Environment.CurrentDirectory + "\\config.json");
                    sw.Write(json);
                    sw.Close();
                    string strJson = File.ReadAllText("config.json", Encoding.UTF8);
                    JObject oJson = JObject.Parse(strJson); 
                    oJson["userName"] = userName;
                    oJson["passWord"] = encrypted_pwd;
                    string strConvert = Convert.ToString(oJson); 
                    File.WriteAllText("config.json", strConvert); 
                    sw.Dispose();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("√ 写入完成 config.json");
                    Crypto crypto = new();
                    string json2 = JsonMapper.ToJson(crypto); //using LitJson
                    StreamWriter sw2 = new StreamWriter(System.Environment.CurrentDirectory + "\\crypto.json");
                    sw2.Write(json);
                    sw2.Close();
                    string strJson2 = File.ReadAllText("crypto.json", Encoding.UTF8);
                    JObject oJson2 = JObject.Parse(strJson2);
                    oJson2["confVersion"] = "v1";
                    oJson2["Encrypt"] = "System";
                    oJson2["Token"] = key;
                    string strConvert2 = Convert.ToString(oJson2);
                    File.WriteAllText("crypto.json", strConvert2);
                    sw2.Dispose();
                    Console.WriteLine("√ 写入完成 crypto.json");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine("! 发生错误，程序即将退出\n" + ex);
                }
            }
            else
            {
                Console.WriteLine("> 使用自定义密钥");
                Console.WriteLine("! 请注意，您必须保存好密钥，系统不会储存密钥。");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("? 请输入 AES 解密密钥：");
                string key = Console.ReadLine();
                Console.WriteLine("> 加密使用的密钥：" + key);
                string encrypted_pwd = AESEncrypt(passWord, key, "");
                try
                {
                    Settings settings = new();
                    string json = JsonMapper.ToJson(settings); //using LitJson
                    StreamWriter sw = new StreamWriter(System.Environment.CurrentDirectory + "\\config.json");
                    sw.Write(json);
                    sw.Close();
                    string strJson = File.ReadAllText("config.json", Encoding.UTF8);
                    JObject oJson = JObject.Parse(strJson);
                    oJson["userName"] = userName;
                    oJson["passWord"] = encrypted_pwd;
                    string strConvert = Convert.ToString(oJson);
                    File.WriteAllText("config.json", strConvert);
                    sw.Dispose();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("√ 写入完成 config.json");
                    Crypto crypto = new();
                    string json2 = JsonMapper.ToJson(crypto); //using LitJson
                    StreamWriter sw2 = new StreamWriter(System.Environment.CurrentDirectory + "\\crypto.json");
                    sw2.Write(json);
                    sw2.Close();
                    string strJson2 = File.ReadAllText("crypto.json", Encoding.UTF8);
                    JObject oJson2 = JObject.Parse(strJson2);
                    oJson2["confVersion"] = "v1";
                    oJson2["Encrypt"] = "Manual";
                    oJson2["Token"] = null;
                    string strConvert2 = Convert.ToString(oJson2);
                    File.WriteAllText("crypto.json", strConvert2);
                    sw2.Dispose();
                    Console.WriteLine("√ 写入完成 crypto.json");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine("! 发生错误，程序即将退出\n" + ex);
                }
            }
            Console.Title = "程序需要重启";
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Magenta;
            Console.Clear();
            Console.WriteLine("请等待程序重启，重启后就可以启动守护进程了！");
            Console.WriteLine("按任意键退出...");
            Console.ReadLine();
            Environment.Exit(3000);
        }
    }
    /// <summary>
    /// AES加密
    /// </summary>
    /// <param name="text">加密字符</param>
    /// <param name="password">加密的密码</param>
    /// <param name="iv">密钥</param>
    /// <returns></returns>
    public static string AESEncrypt(string text, string password, string iv)
    {
        RijndaelManaged rijndaelCipher = new()
        {
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7,
            KeySize = 128,
            BlockSize = 128
        };
        byte[] pwdBytes = System.Text.Encoding.UTF8.GetBytes(password);
        byte[] keyBytes = new byte[16];
        int len = pwdBytes.Length;
        if (len > keyBytes.Length) len = keyBytes.Length;
        System.Array.Copy(pwdBytes, keyBytes, len);
        rijndaelCipher.Key = keyBytes;
        byte[] ivBytes = System.Text.Encoding.UTF8.GetBytes(iv);
        rijndaelCipher.IV = new byte[16];
        ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
        byte[] plainText = Encoding.UTF8.GetBytes(text);
        byte[] cipherBytes = transform.TransformFinalBlock(plainText, 0, plainText.Length);
        return Convert.ToBase64String(cipherBytes);
    }
    /// <summary>
    /// AES解密
    /// </summary>
    /// <param name="text"></param>
    /// <param name="password"></param>
    /// <param name="iv"></param>
    /// <returns></returns>
    public static string AESDecrypt(string text, string password, string iv)
    {
        var rijndaelCipher = new RijndaelManaged
        {
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7,
            KeySize = 128,
            BlockSize = 128
        };
        byte[] encryptedData = Convert.FromBase64String(text);
        byte[] pwdBytes = System.Text.Encoding.UTF8.GetBytes(password);
        byte[] keyBytes = new byte[16];
        int len = pwdBytes.Length;
        if (len > keyBytes.Length) len = keyBytes.Length;
        System.Array.Copy(pwdBytes, keyBytes, len);
        rijndaelCipher.Key = keyBytes;
        byte[] ivBytes = System.Text.Encoding.UTF8.GetBytes(iv);
        rijndaelCipher.IV = ivBytes;
        ICryptoTransform transform = rijndaelCipher.CreateDecryptor();
        byte[] plainText = transform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        return Encoding.UTF8.GetString(plainText);
    }
}