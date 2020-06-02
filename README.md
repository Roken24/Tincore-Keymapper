# Tincore-Keymapper--虚拟手柄
Through the mobile phone control computer racing game, using socket protocol to achieve a virtual handle.通过手机控制电脑上的赛车游戏，使用套接字协议实现的一款虚拟手柄。

一、基于socket实现虚拟手柄使用手机控制电脑游戏（上）-电脑服务端

这个是关于利用socket套接字实现手机控制电脑按键的一个实例，完成这个项目可以实现用手机控制狂野飙车等游戏，就是一个简易的手机虚拟手柄，该项目一共分为两个部分，一个是电脑的服务端，用来接收虚拟手柄发送来的消息并进行相应的按键处理；另一部分是手机虚拟手柄端，用来发送消息，这个是基于unity做的。
所有的代码已开源，GitHub链接：虚拟手柄GitHub地址
电脑服务端-Server
1、首先建立能够异步接收客户端消息的服务端

这个是比较好理解的，因为我们要不断地接收虚拟手柄发来的按键信息，并且玩家可能不是一个，就存在接入多个客户端，所以在接收客户端连接和接收客户端消息的时候采用异步的方式，实现代码如下：
建立服务端：

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

接收客户端：

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

接收客户端消息并处理成相应按键，关于按键处理代码后面介绍：

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

2、相应的按键处理部分

比如我们按下一个按钮发送的消息是“1”，我们要把这个“1”转换成键盘上某个键按下，这就需要引用C#的一个库函数

using System.Runtime.InteropServices;

相应的按键处理方法（及键值转换方法）：

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

我们可以看到这个方法里面有四个参数，我们只用到两个就够了，第一个参数代表的是键值，即哪个键的键值是多少，文章后面有相应的表，大家可以参考；第三个参数，如果是“0”代表着按键按下，如果是“2”代表着按键抬起。针对每个信息处理的代码上面有，也就是这些：

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

当我们启动服务端的时候就可以接收消息并进行处理了，大家不要照搬这篇文章代码，仅提供思路，想要代码去GitHub即可。
下面是按键与键值对应表：

    虚拟键码 对应值 对应键
    VK_LBUTTON 1 鼠标左键
    VK_RBUTTON 2 鼠标右键
    VK_CANCEL 3 Cancel
    VK_MBUTTON 4 鼠标中键
    VK_XBUTTON1 5
    VK_XBUTTON2 6
    VK_BACK 8 Backspace
    VK_TAB 9 Tab
    VK_CLEAR 12 Clear
    VK_RETURN 13 Enter
    VK_SHIFT 16 Shift
    VK_CONTROL 17 Ctrl
    VK_MENU 18 Alt
    VK_PAUSE 19 Pause
    VK_CAPITAL 20 Caps Lock
    VK_KANA 21
    VK_HANGUL 21
    VK_JUNJA 23
    VK_FINAL 24
    VK_HANJA 25
    VK_KANJI 25*
    VK_ESCAPE 27 Esc
    VK_CONVERT 28
    VK_NONCONVERT 29
    VK_ACCEPT 30
    VK_MODECHANGE 31
    VK_SPACE 32 Space
    VK_PRIOR 33 Page Up
    VK_NEXT 34 Page Down
    VK_END 35 End
    VK_HOME 36 Home
    VK_LEFT 37 Left Arrow
    VK_UP 38 Up Arrow
    VK_RIGHT 39 Right Arrow
    VK_DOWN 40 Down Arrow
    VK_SELECT 41 Select
    VK_PRINT 42 Print
    VK_EXECUTE 43 Execute
    VK_SNAPSHOT 44 Snapshot
    VK_INSERT 45 Insert
    VK_DELETE 46 Delete
    VK_HELP 47 Help
    48 0
    49 1
    50 2
    51 3
    52 4
    53 5
    54 6
    55 7
    56 8
    57 9
    65 A
    66 B
    67 C
    68 D
    69 E
    70 F
    71 G
    72 H
    73 I
    74 J
    75 K
    76 L
    77 M
    78 N
    79 O
    80 P
    81 Q
    82 R
    83 S
    84 T
    85 U
    86 V
    87 W
    88 X
    89 Y
    90 Z
    VK_LWIN 91
    VK_RWIN 92
    VK_APPS 93
    VK_SLEEP 95
    VK_NUMPAD0 96 小键盘 0
    VK_NUMPAD1 97 小键盘 1
    VK_NUMPAD2 98 小键盘 2
    VK_NUMPAD3 99 小键盘 3
    VK_NUMPAD4 100 小键盘 4
    VK_NUMPAD5 101 小键盘 5
    VK_NUMPAD6 102 小键盘 6
    VK_NUMPAD7 103 小键盘 7
    VK_NUMPAD8 104 小键盘 8
    VK_NUMPAD9 105 小键盘 9
    VK_MULTIPLY 106 小键盘 *
    VK_ADD 107 小键盘 +
    VK_SEPARATOR 108 小键盘 Enter
    VK_SUBTRACT 109 小键盘 -
    VK_DECIMAL 110 小键盘 .
    VK_DIVIDE 111 小键盘 /
    VK_F1 112 F1
    VK_F2 113 F2
    VK_F3 114 F3
    VK_F4 115 F4
    VK_F5 116 F5
    VK_F6 117 F6
    VK_F7 118 F7
    VK_F8 119 F8
    VK_F9 120 F9
    VK_F10 121 F10
    VK_F11 122 F11
    VK_F12 123 F12
    VK_F13 124
    VK_F14 125
    VK_F15 126
    VK_F16 127
    VK_F17 128
    VK_F18 129
    VK_F19 130
    VK_F20 131
    VK_F21 132
    VK_F22 133
    VK_F23 134
    VK_F24 135
    VK_NUMLOCK 144 Num Lock
    VK_SCROLL 145 Scroll
    VK_LSHIFT 160
    VK_RSHIFT 161
    VK_LCONTROL 162
    VK_RCONTROL 163
    VK_LMENU 164
    VK_RMENU 165
    VK_BROWSER_BACK 166
    VK_BROWSER_FORWARD 167
    VK_BROWSER_REFRESH 168
    VK_BROWSER_STOP 169
    VK_BROWSER_SEARCH 170
    VK_BROWSER_FAVORITES 171
    VK_BROWSER_HOME 172
    VK_VOLUME_MUTE 173 VolumeMute
    VK_VOLUME_DOWN 174 VolumeDown
    VK_VOLUME_UP 175 VolumeUp
    VK_MEDIA_NEXT_TRACK 176
    VK_MEDIA_PREV_TRACK 177
    VK_MEDIA_STOP 178
    VK_MEDIA_PLAY_PAUSE 179
    VK_LAUNCH_MAIL 180
    VK_LAUNCH_MEDIA_SELECT 181
    VK_LAUNCH_APP1 182
    VK_LAUNCH_APP2 183
    VK_OEM_1 186 ; :
    VK_OEM_PLUS 187 = +
    VK_OEM_COMMA 188
    VK_OEM_MINUS 189 - _
    VK_OEM_PERIOD 190
    VK_OEM_2 191 / ?
    VK_OEM_3 192 ` ~
    VK_OEM_4 219 [ {
    VK_OEM_5 220 \ |
    VK_OEM_6 221 ] }
    VK_OEM_7 222 ’ "
    VK_OEM_8 223
    VK_OEM_102 226
    VK_PACKET 231
    VK_PROCESSKEY 229
    VK_ATTN 246
    VK_CRSEL 247
    VK_EXSEL 248
    VK_EREOF 249
    VK_PLAY 250
    VK_ZOOM 251
    VK_NONAME 252
    VK_PA1 253
    VK_OEM_CLEAR 254

二、基于socket实现虚拟手柄使用手机控制电脑游戏（下）-手机端虚拟手柄

这个是关于利用socket套接字实现手机控制电脑按键的一个实例，完成这个项目可以实现用手机控制狂野飙车等游戏，就是一个简易的手机虚拟手柄，该项目一共分为两个部分，一个是电脑的服务端，用来接收虚拟手柄发送来的消息并进行相应的按键处理；另一部分是手机虚拟手柄端，用来发送消息，这个是基于unity做的。
所有的代码已开源，GitHub链接：虚拟手柄GitHub地址

1、客户端部分

建立一个客户端能够发送消息就可以了，这里是比较基础的东西，没什么好说明的，代码：

private static SocketClientBehaviour _singleton;
    public static SocketClientBehaviour Singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindObjectOfType<SocketClientBehaviour>();
            }
            return _singleton;
        }
    }
    private const int BUFFER_SIZE = 1024;

    public InputField myIPv4;
    public InputField myPort;
    public GameObject ConPanel;

    //public string host = "192.168.43.177";
    //public int port = 8088;
    string host;
    int port;

    private byte[] buffer;

    private Socket socket;
    // Use this for initialization
    void Start()
    {
        
        //sendMsg = "Hello server";
        //socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //Connect();
    }

    public void PressToConnect()
    {
        Destroy(ConPanel);
        host = myIPv4.text;
        port = int.Parse(myPort.text);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Connect();
    }

    private void Connect()
    {
        try
        {
            socket.Connect(host, port);
        }
        catch (Exception e)
        {
            print(e.Message);
        }

        if (socket.Connected)
        {
            print("Connected");
            Receive();
        }
        else
        {
            print("Connect fail");
        }
    }

    private void Receive()
    {
        if (!socket.Connected)
            return;

        buffer = new byte[BUFFER_SIZE];

        try
        {
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback(Receive_Callback), socket);
        }
        catch (Exception e)
        {
            print(e.Message);
        }
    }

    private void Receive_Callback(IAsyncResult ar)
    {
        if (!socket.Connected)
        {
            return;
        }

        int read = socket.EndReceive(ar);

        if (read > 0)
        {
            print(Encoding.UTF8.GetString(buffer));

            Receive();
        }
    }

    public void Send(string message)
    {
        if (!socket.Connected)
            return;

        byte[] msg = Encoding.UTF8.GetBytes(message);
        try
        {
            socket.Send(msg);
        }
        catch(Exception e)
        {
            print(e.Message);
        }
    }

2、按钮部分

其实手机端最主要的就是按钮部分的制作，我们可以参照键盘按键的工作原理，当按键按下之后电流是通的，我们仿照这个当某个按钮按下之后发送数字“1”，抬起时发送“-1”，按照这个模式来进行数据传输，达到相应的目的，下面是按钮部分的代码：

public void UpKeyDown()
    {
        //print("now down 1");
        Send("1");
    }

    public void UpKeyUp()
    {
        //print("now up 1");
        Send("-1");
    }

    public void DownKeyDown()
    {
        Send("2");
    }

    public void DownKeyUp()
    {
        Send("-2");
    }

    public void LeftKeyDown()
    {
        Send("3");
    }

    public void LeftKeyUp()
    {
        Send("-3");
    }

    public void RightKeyDown()
    {
        Send("4");
    }

    public void RightKeyUp()
    {
        Send("-4");
    }
    public void SpaceKeyDown()
    {
        Send("5");
    }

    public void SpaceKeyUp()
    {
        Send("-5");
    }
    public void PressToQuit()
    {
        Application.Quit();
    }

    private void OnDisable()
    {
        if (socket.Connected)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }

