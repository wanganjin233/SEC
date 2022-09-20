using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SEC.Util;

namespace SEC.Test
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent(); 
            Socket socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new UnixDomainSocketEndPoint(Application.StartupPath + "MesIP.sock"));
            socket.Listen(10);
            socket.BeginAccept(AcceptCallback, socket);


            Socket socket1 = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Tcp);
            socket1.Connect(new UnixDomainSocketEndPoint(Application.StartupPath + "MesIP.sock"));
            socket1.Send("ss".ToBytes());
        }


        /// <summary>
        /// 客户端连接回调
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallback(IAsyncResult ar)
        {
            Socket? _Socket = ar.AsyncState as Socket;
            if (_Socket != null)
            {
                var client = _Socket.EndAccept(ar);
                AddClient(client);
                _Socket.BeginAccept(AcceptCallback, _Socket);
            }
        }
        /// <summary>
        /// 新增会话方法
        /// </summary>
        /// <param name="client"></param>
        private void AddClient(Socket client)
        {
            ListenSocket listenSocket = new ListenSocket(client);
            if (string.IsNullOrEmpty(listenSocket.RemoteEndPoint) == false)
            {

                Receive(listenSocket);
            }
        }
        private void Receive(ListenSocket listenSocket)
        {
            _ = Task.Run(() =>
            {
                //缓存
                byte[] buffer = new byte[listenSocket.Socket.ReceiveBufferSize];
                //缓存下标
                int index = 0;
                while (true)
                {

                    //接收阻塞
                    int count = listenSocket.Socket.Receive(listenSocket.ReceiveBuffer);
                }
            });
            }
    }
}
