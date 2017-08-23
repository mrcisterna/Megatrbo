using System;
using System.Collections.Generic;
using System.Linq;
using Megatrbo.Types;
using Mototrbo.Server.Types;
using Megatrbo.CommonShared.Environment;
using log4net;

namespace Mototrbo.Server.CommServer
{
    public abstract class PayloadProcessorBase : IPayloadProcessor
    {
        #region Protected variables
        protected Action _action;
        protected readonly IRadioObject rxObject;
        protected readonly IEnumerable<MototrboRadio> _radios;
        protected readonly IEnvironment _env;
        protected readonly ILog _log;
        protected readonly ITransmitter _transmitter;
        protected readonly IUIDispatcher _uiDispatcher;
        protected readonly MototrboRadio _radio;
        #endregion

        #region Public properties
        public Action ProcessorAction { get { return _action; } }
        #endregion

        #region Constructor
        public PayloadProcessorBase(IRadioObject payload, IEnvironment env, ILog log, IEnumerable<MototrboRadio> radios, ITransmitter transmitter, IUIDispatcher uiDispatcher)
        {
            _env = env;
            _log = log;
            _radios = radios;
            _transmitter = transmitter;
            _uiDispatcher = uiDispatcher;
            rxObject = payload;

            _radio = GetRadio(payload.Ip);
        }
        #endregion

        protected TxObject CreateNewTransmitObject(byte[] data, int port, bool resend)
        {
            return new TxObject()
            {
                Buffer = data,
                Date = DateTime.Now,
                Id = Guid.NewGuid(),
                ParentId = rxObject.Id,
                Ip = rxObject.Ip,
                Port = port,
                Resend = resend
            };
        }

        protected MototrboRadio GetRadio(string ip)
        {
            MototrboRadio radio = null;

            if (_radios != null)
            {
                for (int i = 0; i < _radios.Count(); i++)
                {
                    if (_radios.ElementAt(i).IP == ip)
                    {
                        radio = _radios.ElementAt(i);
                        break;
                    }
                }                
            }

            return radio;
        }

        protected TxObject GPSIntervalRequest()
        {
            var data = DataToSend.GPS_INTERVAL_ON;
            data[11] = (byte)_radio.GPSInterval;

            return CreateRequest(data, _env.PortGps, true);
        }

        protected TxObject GPSEmergencyEnabledRequest()
        {
            var data = DataToSend.EMERGENCY_ON;

            return CreateRequest(data, _env.PortGps, true);
        }

        protected TxObject ARSAcknowledge()
        {
            var data = DataToSend.ARS_ACKNOWLEDGE;

            return CreateRequest(data, _env.PortArs, false);
        }

        protected TxObject SMSAcknowledge()
        {
            var data = new byte[] { 0, 1, 31 };

            return CreateRequest(data, _env.PortSms, true);
        }

        private TxObject CreateRequest(byte[] data, int port, bool retry)
        {
            return new TxObject()
            {
                Buffer = data,
                Date = DateTime.Now,
                Disposed = false,
                Enabled = true,
                Ip = _radio.IP,
                Port = port,
                Resend = retry,
                Retries = 0
            };
        }

        protected void RemoveTxData(int port, byte[] data)
        {
            _transmitter.RemoveData(_radio.IP, port, data);
        }

        protected void RemoveTxData(int port)
        {
            _transmitter.RemoveData(_radio.IP, port, null);
        }

        public void Dispose()
        {

        }
    }
}
