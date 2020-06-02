using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Runtime.InteropServices;

namespace Server
{
    class Server
    {
        static int state = 0;
        static ArrayList al = new ArrayList(0);
        static byte[] buffer = new byte[1024];
        private static int count = 0;

        //主函数
        static void Main(string[] args)
        {
            RunSocket();
            //al.Clear();
            //state = -1;
            //while (true)
            //{
                
            //}
        }
        //键值转换
        [DllImport("USER32.DLL")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        static void pressKey(byte keycode)
        {
            if (!al.Contains(keycode))
            {
                al.Add(keycode);
                keybd_event(keycode, 0, 0, 0);
            }
        }

        #region 客户端连接成功
        /// <summary>
        /// 客户端连接成功
        /// </summary>
        /// <param name="ar"></param>
        public static void RunSocket()
        {
            WriteLine("服务端启动成功，IP：192.168.8.128", ConsoleColor.Green); //绿色

           
            //①创建一个新的Socket,这里我们使用最常用的基于TCP的Stream Socket（流式套接字）
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //②将该socket绑定到主机上面的某个端口
            socket.Bind(new IPEndPoint(IPAddress.Parse("192.168.8.128"), 8088));
            //③启动监听，并且设置一个最大的队列长度
            socket.Listen(0);
            //④开始接受客户端连接请求
            //异步接收 同步接受用accept
            socket.BeginAccept(new AsyncCallback(ClientAccepted), socket);
            Console.ReadLine();
            
        }
        public static void ClientAccepted(IAsyncResult ar)
        {
            #region
            //设置计数器
            count++;
            var socket = ar.AsyncState as Socket;
            //这就是客户端的Socket实例，我们后续可以将其保存起来
            var client = socket.EndAccept(ar);
            //客户端IP地址和端口信息
            IPEndPoint clientipe = (IPEndPoint)client.RemoteEndPoint;

            WriteLine(clientipe + " 接入连接，客户端总数： " + count, ConsoleColor.Yellow);

            //接收客户端的消息(这个和在客户端实现的方式是一样的）异步
            client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), client);
            //准备接受下一个客户端请求(异步)
            socket.BeginAccept(new AsyncCallback(ClientAccepted), socket);
            #endregion
        }
        #endregion

        #region 接收客户端的信息
        /// <summary>
        /// 接收某一个客户端的消息
        /// </summary>
        /// <param name="ar"></param>
        public static void ReceiveMessage(IAsyncResult ar)
        {
            int length = 0;
            string message = "";
            var socket = ar.AsyncState as Socket;
            //客户端IP地址和端口信息
            IPEndPoint clientipe = (IPEndPoint)socket.RemoteEndPoint;
            try
            {
                #region
                length = socket.EndReceive(ar);
                //读取出来消息内容
                message = Encoding.UTF8.GetString(buffer, 0, length);
                //输出接收信息
                WriteLine(clientipe + " ：" + message, ConsoleColor.White);
                //将内容转换成int
                int option = -1;
                try
                {
                    option = int.Parse(message);
                }
                catch
                {

                }
                // Console.WriteLine("option = " + option);
                state = option;
                al.Clear();
                if (state != -1)
                {

                    switch (state)
                    {
                        case 1: // UP = 38
                            //pressKey(38);
                            keybd_event(38, 0, 0, 0);
                            break;
                        case -1: // DOWN = 40
                            //pressKey(40);
                            keybd_event(38, 0, 2, 0);
                            break;
                        case 2: // LEFT = 37
                            keybd_event(40, 0, 0, 0);
                            break;
                        case -2: // RIGHT = 39
                            keybd_event(40, 0, 2, 0);
                            break;
                        case 3: // K = 75
                            keybd_event(37, 0, 0, 0);

                            break;
                        case -3: // L = 76
                            keybd_event(37, 0, 2, 0);
                            break;
                        case 4: // U = 85
                            keybd_event(39, 0, 0, 0);
                            break;
                        case -4: // I = 73
                            keybd_event(39, 0, 2, 0);
                            break;
                        case 5: //SPACE = 32
                            keybd_event(32, 0, 0, 0);
                            break;
                        case -5: // O = 79
                            keybd_event(32, 0, 2, 0);
                            break;
                    }
                }
                else
                {
                    foreach (byte key in al)
                    {
                        keybd_event(key, 0, 2, 0);
                    }
                    al.Clear();
                }
                //服务器发送消息
                socket.Send(Encoding.UTF8.GetBytes("server received data")); //默认Unicode
                //接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息）异步
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
                #endregion
            }
            catch (Exception ex)
            {
                //设置计数器
                count--;
                //断开连接
                WriteLine(clientipe + " 断开连接，客户端总数： " + (count), ConsoleColor.Red);
            }
        }
        #endregion

        #region 扩展方法
        public static void WriteLine(string str, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("[{0}] {1}", DateTime.Now.ToString("MM-dd HH:mm:ss"), str);
        }
        #endregion
    }
}
