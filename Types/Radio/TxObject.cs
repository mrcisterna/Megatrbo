using System;
using System.Threading;
using System.Configuration;

namespace Mototrbo.Server.Types
{
    public class TxObject : RadioObjectBase
    {
        #region Private variables
        private readonly static int _time = int.Parse(ConfigurationManager.AppSettings["TxRetryTime"]) * 1000;
        private readonly static int _maxRetries = int.Parse(ConfigurationManager.AppSettings["TxRetryCount"]);

        private Timer _timer;
        private object _state;
        private bool _resend;
        private bool _disposed;
        private int _retries;
        #endregion

        #region Public properties      
        public bool Resend
        {
            get
            {
                return _resend;
            }
            set
            {
                _resend = value;
            }
        }
        public int Retries
        {
            get
            {
                return _retries;
            }
            set
            {
                if (_retries == 0)
                {                    
                    StartResendProcess();
                }

                _retries = value;

                if(_retries == _maxRetries)
                {
                    _disposed = true;
                    _enabled = false;
                    _timer.Dispose();
                }
            }
        }
        public bool Disposed
        {
            get
            {
                return _disposed;
            }
            set
            {
                _disposed = value;
            }
        }
        #endregion

        #region Constructor
        public TxObject()
        {
            _enabled = true;
            _state = new object();
            _retries = 0;
        }
        #endregion

        private void StartResendProcess()
        {
            if (_resend)
            {
                var time = (uint)(_time);
                _timer = new Timer(EnableResend, _state, time, time);
            }
        }

        private void EnableResend(object state)
        {
            _enabled = true;
        }
    }
}
