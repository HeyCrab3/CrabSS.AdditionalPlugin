using Microsoft.AspNetCore.Http;
using System.Net.Sockets;
using System.Text;

namespace CrabSS.WebConsoleDaemon
{
    internal class HttpServer
    {
        private static TcpListener CrabSrv;
        private static int port = 22102;
        private static HttpRequest Request;
        public static void UpdateLog(string content, ConsoleColor alertColor)
        {
            string time = "[" + DateTime.Now.ToString() + "] ";
            Console.ForegroundColor = alertColor;
            Console.WriteLine(time + content);
        }
        public static void StartSrv()
        {
            try
            {
                CrabSrv = new TcpListener(port);
                CrabSrv.Start();
                UpdateLog("已监听在端口 " + port, ConsoleColor.Cyan);
                Thread th = new Thread(new ThreadStart(StartListen));
                th.Start();
            }
            catch (Exception ex)
            {
                UpdateLog("寄了，端口被占用 检查22102端口上的占用应用！\n" + ex, ConsoleColor.Red);
                Console.WriteLine("按任意键退出...");
                Console.Read();
                Environment.Exit(1003);
            }
        }
        public static void SendHeader(string sHttpVersion, string sMIMEHeader, int iTotBytes, string sStatusCode, ref Socket mySocket)
        {
            String sBuffer = "";
            if (sMIMEHeader.Length == 0)
            {
                sMIMEHeader = "application/json";
            }
            var sBuffersBuffer = sBuffer + sHttpVersion + sStatusCode + "\r\n";
            sBuffersBuffer = sBuffer + "Server: CrabSS-Daemon\r\n";
            sBuffersBuffer = sBuffer + "Content-Type: " + sMIMEHeader + "\r\n";
            sBuffersBuffer = sBuffer + "Accept-Ranges: bytes\r\n";
            sBuffersBuffer = sBuffer + "Content-Length: " + iTotBytes + "\r\n\r\n";
            Byte[] bSendData = Encoding.ASCII.GetBytes(sBuffer);
            SendToBrowser(bSendData, ref mySocket);
            Console.WriteLine("Total Bytes : " + iTotBytes.ToString());
        }
        public static void SendToBrowser(Byte[] bSendData, ref Socket mySocket)
        {
            int numBytes = 0;

            try
            {
                if (mySocket.Connected)
                {
                    if ((numBytes = mySocket.Send(bSendData, bSendData.Length, 0)) == -1)
                        UpdateLog(mySocket.RemoteEndPoint + "=> 连接失败: 发包失败 Socket Error", ConsoleColor.Red);
                    else
                    {
                        UpdateLog(mySocket.RemoteEndPoint + "=> 数据包发送 [" + numBytes + "]", ConsoleColor.Cyan);
                    }
                }
                else
                    UpdateLog(mySocket.RemoteEndPoint + "=> 连接失败: null", ConsoleColor.Red);
            }
            catch (Exception e)
            {
                UpdateLog("发生了错误，请检查...\n" + e, ConsoleColor.Red);
            }
        }
        public static void StartListen()
        {

            int iStartPos = 0;
            string sRequest;
            string sDirName;
            string sRequestedFile;
            string sErrorMessage;
            string sPhysicalFilePath = "";
            string sFormattedMessage = "";
            string sResponse = "";

            while (true)
            {
                Socket mySocket = CrabSrv.AcceptSocket();

                Console.WriteLine("Socket Type " + mySocket.SocketType);
                if (mySocket.Connected)
                {
                    UpdateLog("新客户端已建立连接 => " + mySocket.RemoteEndPoint, ConsoleColor.Cyan);
                    Byte[] bReceive = new Byte[1024];
                    int i = mySocket.Receive(bReceive, bReceive.Length, 0);
                    string sBuffer = Encoding.ASCII.GetString(bReceive); 
                    iStartPos = sBuffer.IndexOf("HTTP", 1);
                    string sHttpVersion = sBuffer.Substring(iStartPos, 8);
                    sRequest = sBuffer.Substring(0, iStartPos - 1);
                    sRequest.Replace("\\", "/");
                    iStartPos = sRequest.LastIndexOf("/") + 1;
                    sDirName = sRequest.Substring(sRequest.IndexOf("/"), sRequest.LastIndexOf("/") - 3);
                    UpdateLog(mySocket.RemoteEndPoint + " => Requesting DICT " + sDirName, ConsoleColor.Cyan);
                    if (sDirName == "/v1/api/login")
                    {
                        string userName = Request.Form["userName"];
                        string passWord = Request.Form["passWord"];
                    }
                    String sMimeType = "text/html";

                    sPhysicalFilePath = sLocalDir + sRequestedFile;
                    Console.WriteLine("请求文件: " + sPhysicalFilePath);

                    if (File.Exists(sPhysicalFilePath) == false)
                    {

                        sErrorMessage = "<H2>404 Error! File Does Not Exists...</H2>";
                        SendHeader(sHttpVersion, "", sErrorMessage.Length, " 404 Not Found", ref mySocket);
                        SendToBrowser(sErrorMessage, ref mySocket);

                        Console.WriteLine(sFormattedMessage);
                    }

                    else
                    {
                        int iTotBytes = 0;

                        sResponse = "";

                        FileStream fs = new FileStream(sPhysicalFilePath,
                        FileMode.Open, FileAccess.Read, FileShare.Read);

                        BinaryReader reader = new BinaryReader(fs);
                        byte[] bytes = new byte[fs.Length];
                        int read;
                        while ((read = reader.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            sResponsesResponse = sResponse + Encoding.ASCII.GetString(bytes, 0, read);

                            iTotBytesiTotBytes = iTotBytes + read;

                        }
                        reader.Close();
                        fs.Close();

                        SendHeader(sHttpVersion, sMimeType, iTotBytes, " 200 OK", ref mySocket);
                        SendToBrowser(bytes, ref mySocket);
                        //mySocket.Send(bytes, bytes.Length,0);  
                    }
                    mySocket.Close();
                }
            }
        }
    }
}
