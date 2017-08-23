using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Practices.Composite.Events;
using Mototrbo.Server.Types;
using Megatrbo.CommonShared.Environment;
using log4net;

namespace Mototrbo.Server.CommServer
{
    public class UIDispatcher : IUIDispatcher
    {
        #region Private variables
        private readonly ConcurrentQueue<UIObject> _uiQueue;
        private readonly Thread _publishThread;
        private readonly IEventAggregator _aggregator;
        private readonly IEnvironment _env;
        private readonly ILog _log;

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
        public UIDispatcher(IEnvironment env, ILog log, IEventAggregator aggregator)
        {
            _env = env;
            _log = log;
            _aggregator = aggregator;
            _uiQueue = new ConcurrentQueue<UIObject>();
            _publishThread = new Thread(new ThreadStart(Dispatcher));
        }
        #endregion

        public void Add(UIObject frontEndData)
        {
            _uiQueue.Enqueue(frontEndData);
        }

        private void Dispatcher()
        {
            while (true)
            {
                if (_isAlive)
                {
                    UIObject o = null;

                    if (_uiQueue.TryDequeue(out o))
                    {
                        _aggregator.GetEvent<UIEvent>().Publish(o);
                    }

                    Thread.Sleep(50);
                }
                else
                {
                    break;
                }
            }
        }

        public void Start()
        {
            IsAlive = true;
            _publishThread.Start();
        }

        public void Stop()
        {
            _isAlive = false;
        }
    }
}
