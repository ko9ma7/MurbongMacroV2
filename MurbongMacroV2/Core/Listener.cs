using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace MurbongMacroV2.Core
{

    /// <summary>
    /// 서버다. 모바일 앱과 통신한다.
    /// </summary>
    public class Listener
    {
        public delegate void Receiving(string str, bool send);
        public event Receiving receiving;


        public TcpListener server;
        public TcpClient connectedClient;
        private CancellationTokenSource cts;
        private bool isStarted;
        public bool isConnected;
        public Listener(int port)
        {
            server = new TcpListener(IPAddress.Any, port);
        }
        public void StartServer(int port)
        {
            if (isStarted == false)
            {
                isStarted = true;
                isConnected = true;
                cts = new CancellationTokenSource();
                server.Start();
                //BeginRead();
                Task.Run(() => StartAcceptClientAsync(), cts.Token);
            }
        }

        public void StopServer()
        {
            isStarted = false;
            cts.Cancel();

            cts.Dispose();
            if (connectedClient != null)
            {
                connectedClient.Close();
            }

            server.Stop();
        }


        private async Task StartAcceptClientAsync()
        {
            while (isStarted)
            {
                if (cts.Token.IsCancellationRequested)
                {
                    cts.Token.ThrowIfCancellationRequested();
                }
                connectedClient = await server.AcceptTcpClientAsync();
                IPEndPoint ipEndPoint = (IPEndPoint)connectedClient.Client.RemoteEndPoint;
                isConnected = true;
                receiving("클라이언트와 연결되었습니다.", false);
                BeginRead();
            }
            receiving("서버가 종료되었습니다.", false);
        }

        public void BeginRead()
        {
            if (isStarted == true && connectedClient != null)
            {
                try
                {
                    byte[] buffer = new byte[4096];
                    NetworkStream ns = connectedClient.GetStream();
                    ns.BeginRead(buffer, 0, buffer.Length, EndRead, buffer);
                }
                catch
                {

                }
            }
        }

        public void EndRead(IAsyncResult result)
        {
            try
            {
                byte[] buffer = (byte[])result.AsyncState;
                NetworkStream ns = connectedClient.GetStream();
                int bytesAvailable = ns.EndRead(result);
                string msg = Encoding.Unicode.GetString(buffer, 0, bytesAvailable);

                if (bytesAvailable > 0)
                {
                    receiving(msg, false);
                    BeginRead();
                }
                else
                {
                    throw new Exception();
                }
            }
            catch//연결 강제로 끊김.
            {
                receiving("클라이언트가 종료되었습니다.", false);
                connectedClient.Close();
                connectedClient.Dispose();
                isConnected = false;
            }
        }

        public void BeginSend(string xml)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(xml);
            NetworkStream ns = connectedClient.GetStream();
            ns.BeginWrite(bytes, 0, bytes.Length, EndSend, bytes);
        }

        public void EndSend(IAsyncResult result)
        {
            byte[] bytes = (byte[])result.AsyncState;
            Console.WriteLine("Sent  {0} bytes to server.", bytes.Length);
            Console.WriteLine("Sent: {0}", Encoding.Unicode.GetString(bytes));
        }
    }
}
