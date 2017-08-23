using System.Collections.Generic;
using System.Threading;
using Megatrbo.Helpers;
using System.Collections.Concurrent;
using Mototrbo.Server.Types;
using System;
using Megatrbo.Types;
using Megatrbo.CommonShared.Environment;
using log4net;
using Microsoft.Practices.Composite.Events;

namespace Mototrbo.Server.CommServer
{
    public class RxProcessor : IRxProcessor
    {
        #region Private variables
        private readonly ConcurrentQueue<RxObject> _receiverQueue;
        private readonly Thread _rxPoolThread;
        private readonly IEnumerable<MototrboRadio> _radios;
        private readonly IEventAggregator _aggregator;
        private readonly IEnvironment _env;
        private readonly ILog _log;
        private readonly ITransmitter _transmitter;
        private readonly IUIDispatcher _uiDispatcher;

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
        #endregion

        #region Constructor
        public RxProcessor(IEnvironment env, ILog log, IEventAggregator aggregator, IEnumerable<MototrboRadio> radios, ITransmitter transmitter, IUIDispatcher uiDispatcher)
        {
            _env = env;
            _log = log;
            _aggregator = aggregator;
            _radios = radios;
            _transmitter = transmitter;
            _uiDispatcher = uiDispatcher;

            _receiverQueue = new ConcurrentQueue<RxObject>();
            _rxPoolThread = new Thread(new ThreadStart(Process));
        }
        #endregion

        public void Add(RxObject payload)
        {
            _receiverQueue.Enqueue(payload);
        }

        public void Start()
        {
            _isAlive = true;

            _rxPoolThread.Start();
        }

        private void Process()
        {
            while (true)
            {
                if (_isAlive)
                {
                    try
                    {
                        RxObject receivedDataToProcess = null;

                        if (_receiverQueue.TryDequeue(out receivedDataToProcess))
                        {
                            using (var currentProcessor = Activator.CreateInstance(receivedDataToProcess.ProcessorType,
                                new object[] { receivedDataToProcess, _env, _log, _radios, _transmitter, _uiDispatcher }) as IPayloadProcessor)
                            {
                                currentProcessor.ProcessorAction.Invoke();
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Logger.LogToConsole($"class ReceivePoolData: {ex.Message}");
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public void Stop()
        {
            _isAlive = false;
        }
    }
}

