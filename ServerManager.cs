using System;
using System.Threading;
using System.Collections.Generic;
using Megatrbo.Types;
using log4net;
using Megatrbo.CommonShared.Environment;
using Mototrbo.Server.CommServer;
using Mototrbo.Server.Types;
using Microsoft.Practices.Composite.Events;

namespace Mototrbo.Server
{
    public class ServerManager : IServerManager
    {
        #region Private variables
        private readonly IEnvironment _env;
        private readonly ILog _log;
        private readonly IDictionary<int, IReceiver> _serviceList;
        private readonly IEventAggregator _aggregator;
        private readonly ITransmitter _transmitter;
        private readonly IUIDispatcher _uiDispatcher;
        private readonly IRxProcessor _rxProcessor;
        private readonly ILog _consoleLog;

        private IReceiver _ARSServer;
        private IReceiver _GPSServer;
        private IReceiver _SMSServer;
        private IReceiver _TMTServer;

        private IEnumerable<MototrboRadio> _radios;
        #endregion

        #region Public properties
        public IEnumerable<MototrboRadio> Radios
        {
            get
            {
                return _radios;
            }

            set
            {
                _radios = value;
            }
        }
        #endregion

        #region Constructor
        public ServerManager(IEnvironment env, ILog logger, IEventAggregator aggregator, IEnumerable<MototrboRadio> radios, ILog consoleLog)
        {
            _env = env;
            _log = logger;
            _aggregator = aggregator;
            _radios = radios;
            _consoleLog = consoleLog;

            _transmitter = new Transmiter(_env, _log, _aggregator, _consoleLog);
            _uiDispatcher = new UIDispatcher(_env, _log, _aggregator);
            _rxProcessor = new RxProcessor(_env, _log, _aggregator, _radios, _transmitter, _uiDispatcher);

            _serviceList = new Dictionary<int, IReceiver>();
        }
        #endregion

        public void Initialize()
        {
            _log.Info("Preparando servicios");

            try
            {
                if (_env.PortArs > 0)
                {
                    _log.Info("Instanciando servicio ARS");
                    _ARSServer = new Receiver<ArsProcessor>(_env.PortArs, _rxProcessor, _log, _consoleLog, "ARS");
                    _serviceList.Add(_env.PortArs, _ARSServer);
                    _log.Info("Servicio ARS instaciado correctamente");
                }

                if (_env.PortGps > 0)
                {
                    _log.Info("Instanciando servicio GPS");
                    _GPSServer = new Receiver<GpsProcessor>(_env.PortGps, _rxProcessor, _log, _consoleLog, "GPS");
                    _serviceList.Add(_env.PortGps, _GPSServer);
                    _log.Info("Servicio GPS instaciado correctamente");
                }

                if (_env.PortSms > 0)
                {
                    _log.Info("Instanciando servicio SMS");
                    _SMSServer = new Receiver<SmsProcessor>(_env.PortSms, _rxProcessor, _log, _consoleLog, "SMS");
                    _serviceList.Add(_env.PortSms, _SMSServer);
                    _log.Info("Servicio SMS instaciado correctamente");
                }

                if (_env.PortTmt > 0)
                {
                    _log.Info("Instanciando servicio TMT");
                    //to-do
                    _log.Info("Servicio TMT instaciado correctamente");
                }

                _log.Info("CommServerManager inicializado correctamente");
            }
            catch (Exception)
            {
                _log.Info("CommServerManager no pudo iniciar");
            }
        }

        public void Start()
        {
            try
            {
                _log.Info("Iniciando workers");

                _transmitter.Start();
                Thread.Sleep(50);
                _log.Info("Tx worker iniciado");
                _uiDispatcher.Start();
                Thread.Sleep(50);
                _log.Info("UI worker iniciado");
                _rxProcessor.Start();
                Thread.Sleep(50);
                _log.Info("Rx worker iniciado");

                _log.Info("Iniciando servicios");

                foreach (KeyValuePair<int, IReceiver> server in _serviceList)
                {
                    server.Value.IsAlive = true;
                    server.Value.Start();

                    Thread.Sleep(50);

                    _log.Info(string.Format("Servicio {0} iniciado correctamente", server.Value.Name));
                }

                _log.Info("CommServerManager iniciado correctamente");
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public void Stop()
        {
            foreach (KeyValuePair<int, IReceiver> service in _serviceList)
            {
                service.Value.Stop();

                _log.Info(string.Format("Servicio {0} detenido correctamente", service.Value.Name));
            }

            _rxProcessor.Stop();
            _transmitter.Stop();
            _uiDispatcher.Stop();
        }
    }
}
