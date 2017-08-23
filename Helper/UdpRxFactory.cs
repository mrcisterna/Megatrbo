using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Configuration;

namespace Mototrbo.Server
{
    public static class UdpFactory
    {
        private static readonly string _hostAddress = ConfigurationManager.AppSettings["udpHost"];

        public static UdpClient CreateUdpRxClient(int port, ref IPEndPoint endPoint)
        {
            endPoint = new IPEndPoint(IPAddress.Parse(_hostAddress), port);

            var client = new UdpClient();
            client.ExclusiveAddressUse = false;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, port));

            return client;
        }
    }
}
