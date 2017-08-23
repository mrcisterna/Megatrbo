using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Mototrbo.Server.Types;
using log4net;

namespace Mototrbo.Server.CommServer
{
    public class Receiver<P> : IReceiver where P : IPayloadProcessor
    {
        #region Private variables
        private readonly int _port;
        private readonly Thread _receiverThread;
        private readonly ILog _log;
        private readonly ILog _consoleLog;
        private readonly IRxProcessor _rxProcessor;
        private readonly string _name;

        private bool _isAlive;
        #endregion

        #region Public properties       
        public bool IsAlive
        {
            get
            {
                return _isAlive;
            }
            set
            {
                _isAlive = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }
        #endregion

        #region Constructor
        public Receiver(int port, IRxProcessor rxProcessor, ILog log, ILog consoleLog, string name)
        {
            _port = port;
            _log = log;
            _consoleLog = consoleLog;
            _name = name;
            _rxProcessor = rxProcessor;

            _receiverThread = new Thread(new ThreadStart(ReceiverListener));
        }
        #endregion

        private void ReceiverListener()
        {
            while (true)
            {
                if (_isAlive)
                {
                    try
                    {
                        IPEndPoint rxEndPoint = null;

                        using (var _client = UdpFactory.CreateUdpRxClient(_port, ref rxEndPoint))
                        {
                            var receivedResults = _client.Receive(ref rxEndPoint);

                            var result = new RxObject()
                            {
                                Buffer = receivedResults,
                                Date = DateTime.Now,
                                Enabled = true,
                                Ip = rxEndPoint.Address.ToString(),
                                Port = rxEndPoint.Port,
                                ProcessorType = typeof(P)
                            };

                            _rxProcessor.Add(result);

                            _consoleLog.Debug($"RX | IP: {result.Ip} | {result.Date.ToString()} | port: {result.Port} | data: {BitConverter.ToString(result.Buffer)}");
                        }
                    }
                    catch (SocketException ex)
                    {
                        _log.Error(ex);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public void Start()
        {
            _isAlive = true;
            _receiverThread.Start();
        }

        public void Stop()
        {
            _isAlive = false;
        }
    }
}
