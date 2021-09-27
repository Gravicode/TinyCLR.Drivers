using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Maple.Server_UDP_Listener
{
    class Program
    {
        const int MAPLE_SERVER_BROADCASTPORT = 17756;
        static void Main()
        {
            Thread th1 = new Thread(new ThreadStart(Start));
            th1.Start();
            Thread.Sleep(Timeout.Infinite);
            //UdpClient udpClient = new UdpClient(MAPLE_SERVER_BROADCASTPORT);
            //IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            //while (true)
            //{
            //    Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
            //    string returnData = Encoding.ASCII.GetString(receiveBytes);
            //    Console.WriteLine(returnData);
            //}
        }

        static void Start()
        {
            var socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //var ip = new IPAddress(new byte[] { 192, 168, 1, 87 });
            var endPoint = new IPEndPoint(IPAddress.Any, MAPLE_SERVER_BROADCASTPORT);

            socket.Connect(endPoint);

            //byte[] bytesToSend = Encoding.UTF8.GetBytes(msg);

            while (true)
            {
                //socket.SendTo(bytesToSend, bytesToSend.Length, SocketFlags.None, endPoint);
                while (socket.Poll(2000000, SelectMode.SelectRead))
                {
                    if (socket.Available > 0)
                    {
                        byte[] inBuf = new byte[socket.Available];
                        EndPoint recEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        socket.ReceiveFrom(inBuf, ref recEndPoint);
                        //if (!recEndPoint.Equals(endPoint))// Check if the received packet is from the 192.168.0.2
                        //    continue;
                        Debug.WriteLine(new String(Encoding.UTF8.GetChars(inBuf)));
                    }
                }
                Thread.Sleep(100);
            }
        }
    }
}
