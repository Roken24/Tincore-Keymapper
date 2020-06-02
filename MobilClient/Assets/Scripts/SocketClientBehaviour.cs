using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SocketClientBehaviour : MonoBehaviour
{
    //string sendMsg;
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

    //up-1,down-2,left-3,right-4;

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
}