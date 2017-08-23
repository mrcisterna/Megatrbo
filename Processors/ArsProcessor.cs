using System;
using System.Collections.Generic;
using System.Linq;
using Megatrbo.Types;
using Megatrbo.Domains;
using System.Threading.Tasks;
using Mototrbo.Server.Types;
using Megatrbo.CommonShared.Environment;
using log4net;

namespace Mototrbo.Server.CommServer
{
    public class ArsProcessor : PayloadProcessorBase
    {
        public ArsProcessor(IRadioObject payload, IEnvironment env, ILog log, IEnumerable<MototrboRadio> radios, ITransmitter transmitter, IUIDispatcher uiDispatcher)
            : base(payload, env, log, radios, transmitter, uiDispatcher)
        {
            _action = () =>
            {
                string radioIp = payload.Ip;

                if (_radio != null)
                {
                    var data = payload.Buffer;
                    var currentDate = DateTime.Now;

                    UIObject toFrontEnd = new UIObject();

                    int PayloadDecimal = 0;
                    if (data.Length > 3)
                    {
                        int a = data[2];
                        int b = data[3];
                        PayloadDecimal = a + b;
                    }
                    else
                    {
                        int a = data[1];
                        int b = data[2];
                        PayloadDecimal = a + b;
                    }

                    _radio.IsOn = true;
                    _radio.IsOff = false;
                    _radio.IsUnknow = false;

                    switch (PayloadDecimal)
                    {
                        case 272:
                            //eventoARS.EstadoARS = 1 'RADIO ON
                            _radio.evento = 1;
                            _radio.dates[0].date = currentDate;
                            toFrontEnd.MethodName = "arsOn";
                            //Ars Acknowledge transmit
                            _transmitter.Add(ARSAcknowledge());
                            if (_radio.gps_on)
                            {
                                //Gps Interval enabling                              
                                _transmitter.Add(GPSIntervalRequest());
                                //Emergency enabling
                                _transmitter.Add(GPSEmergencyEnabledRequest());
                            }

                            break;
                        case 64:
                            //eventoARS.EstadoARS = 2 'RADIO UPDATE   
                            RemoveTxData(_env.PortArs);

                            //Gps Interval enabling
                            if (_radio.gps_on)
                            {
                                //Gps Interval enabling                              
                                _transmitter.Add(GPSIntervalRequest());
                                //Emergency enabling
                                _transmitter.Add(GPSEmergencyEnabledRequest());
                            }
                            _radio.dates[0].date = currentDate;
                            toFrontEnd.MethodName = "arsOn";
                            _radio.evento = 3;
                            break;
                        case 304:
                            //eventoARS.EstadoARS = 2 'RADIO UPDATE
                            RemoveTxData(_env.PortArs);
                            _radio.dates[0].date = currentDate;
                            toFrontEnd.MethodName = "arsOn";
                            _radio.evento = 3;
                            break;
                        case 50:
                        case 81:
                            //eventoARS.EstadoARS = 3 'RADIO OFF
                            RemoveTxData(_env.PortArs);
                            _radio.IsOn = false;
                            _radio.IsOff = true;
                            _radio.hasGpsPosition = false;
                            _radio.dates[1].date = currentDate;
                            toFrontEnd.MethodName = "arsOff";
                            _radio.evento = 2;
                            break;
                    }

                    toFrontEnd.Id = payload.Id;
                    toFrontEnd.Radio = _radio;

                    _uiDispatcher.Add(toFrontEnd);

                    Task.Run(() =>
                    {
                        RadiosDomain.Save(_radio);
                    });

                    Task.Run(() =>
                    {
                        ArsDataDomain.Save(_radio);
                    });
                }
            };
        }
    }
}
