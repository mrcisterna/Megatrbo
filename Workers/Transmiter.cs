using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;
using Mototrbo.Server.Types;
using Megatrbo.CommonShared.Environment;
using log4net;
using Microsoft.Practices.Composite.Events;

namespace Mototrbo.Server.CommServer
{
    public class Transmiter : ITransmitter
    {
        #region Private variables
        private readonly ConcurrentQueue<TxObject> _transmiterQueue;
        private readonly Thread _txThread;        
        private readonly IEventAggregator _aggregator;
        private readonly IEnvironment _env;
        private readonly ILog _log;
        private readonly ILog _consoleLog;

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
                IsAlive = value;
            }
        }
        #endregion

        #region Constructor
        public Transmiter(IEnvironment env, ILog log, IEventAggregator aggregator, ILog consoleLog)
        {
            _env = env;
            _log = log;
            _consoleLog = consoleLog;
            _aggregator = aggregator;
            _transmiterQueue = new ConcurrentQueue<TxObject>();
            _txThread = new Thread(new ThreadStart(Process));

            _aggregator.GetEvent<RadioTxEvent>().Subscribe((Action<RxObject>)((e) =>
            {
                var toTx = new TxObject()
                {
                    Enabled = true,
                    Buffer = e.Buffer,
                    Date = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Ip = e.Ip,
                    Port = e.Port,
                    Resend = true,
                    Disposed = false,
                    Retries = 0
                };

                Add(toTx);
            }));
        }
        #endregion

        public void Start()
        {
            _isAlive = true;
            _txThread.Start();
        }

        public void Add(TxObject toTransmit)
        {
            if (ValidateAdd(toTransmit))
            {
                _transmiterQueue.Enqueue(toTransmit);
            }
        }

        public void Add(string ip, int port, byte[] buffer, bool retry)
        {
            var tx = new TxObject()
            {
                Enabled = true,
                Buffer = buffer,
                Date = DateTime.Now,
                Disposed = false,
                Ip = ip,
                Port = port,
                Resend = retry,
                Retries = 0
            };

            if (ValidateAdd(tx))
            {
                _transmiterQueue.Enqueue(tx);
            }
        }

        private bool ValidateAdd(TxObject txObject)
        {
            bool valid = true;
            var txHash = GetHashForValidator(txObject);

            for (int i = 0; i < _transmiterQueue.Count; i++)
            {
                var qTx = _transmiterQueue.ElementAt(i);

                if (!qTx.Disposed)
                {
                    if (GetHashForValidator(qTx) == txHash)
                    {
                        valid = false;
                        break;
                    }
                }
            }            

            return valid;
        }

        private string GetHashForValidator(TxObject tx)
        {
            var hash = new StringBuilder();
            hash.Append(tx.Ip).GetHashCode();
            hash.Append(tx.Port).GetHashCode();
            hash.Append(tx.Buffer[1]).GetHashCode();
            hash.Append(tx.Buffer[2]).GetHashCode();

            return hash.ToString();
        }

        private void Process()
        {
            while (true)
            {
                if (_isAlive)
                {
                    TxObject toTx = null;

                    if (_transmiterQueue.TryDequeue(out toTx))
                    {
                        if (!toTx.Disposed)
                        {
                            if (toTx.Enabled)
                            {
                                Send(toTx);
                                toTx.Enabled = false;

                                if (toTx.Resend)
                                {
                                    toTx.Retries++;
                                }
                            }

                            if (toTx.Resend)
                                _transmiterQueue.Enqueue(toTx);
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void Send(TxObject tx)
        {
            try
            {
                IPEndPoint endPoint = null;

                using (var client = UdpFactory.CreateUdpRxClient(tx.Port, ref endPoint))
                {
                    endPoint = new IPEndPoint(IPAddress.Parse(tx.Ip), tx.Port);

                    client.Send(tx.Buffer, tx.Buffer.Length, endPoint);

                    Thread.Sleep(tx.Buffer.Length * 50);

                  _consoleLog.Warn($"TX | IP: {tx.Ip} | {tx.Date.ToString()} | port: {tx.Port} | data: {BitConverter.ToString(tx.Buffer)}");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public void RemoveData(string ip, int port, byte[] data)
        {
            if (data == null)
            {
                RemoveData(ip, port);
            }
            else
            {
                foreach (var item in _transmiterQueue.Where(t => t.Ip == ip && t.Port == port && t.Buffer[1] == data[1]))
                {
                    item.Disposed = true;
                }
            }
        }

        private void RemoveData(string ip, int port)
        {
            foreach (var item in _transmiterQueue.Where(t => t.Ip == ip && t.Port == port))
            {
                item.Disposed = true;
            }
        }

        private void RemoveData(string ip)
        {
            foreach (var item in _transmiterQueue.Where(t => t.Ip == ip))
            {
                item.Disposed = true;
            }
        }

        public void Stop()
        {
            _isAlive = false;
        }
    }
}
