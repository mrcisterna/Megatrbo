using System;
using System.Collections.Generic;
using System.Linq;
using Megatrbo.Types;
using Megatrbo.VbHelper;
using Megatrbo.Domains;
using System.Threading.Tasks;
using Mototrbo.Server.Types;
using Megatrbo.CommonShared.Environment;
using log4net;

namespace Mototrbo.Server.CommServer
{
    public class GpsProcessor : PayloadProcessorBase
    {
        public GpsProcessor(IRadioObject payload, IEnvironment env, ILog log, IEnumerable<MototrboRadio> radios, ITransmitter transmitter, IUIDispatcher uiDispatcher)
            : base(payload, env, log, radios, transmitter, uiDispatcher)
        {
            _action = () =>
            {
                string radioIp = payload.Ip;

                int DatumIndexStart = 9;
                bool HasGpsData = false;

                if (_radio != null)
                {
                    byte[] Data = payload.Buffer;
                    UIObject toFrontEnd = new UIObject();

                    _radio.emerg_on = false;

                    if (!_radio.IsOn)
                    {
                        _radio.IsOn = true;
                        _radio.IsOff = false;
                        _radio.IsUnknow = false;
                    }

                    //Localizacion instantanea correcta                  

                    if (Data[0] == 7 & !(Data[1] == 0xc))
                    {
                        HasGpsData = true;
                        _radio.evento = 6;
                        DatumIndexStart = 15;

                        RemoveTxData(_env.PortGps, new byte[] { 0x05, 0x08, 0x22, 0x04, 0x24, 0x68, 0xAC, 0xE0, 0x51, 0x62 });
                    }

                    //GPS instantaneo respuesta acknowledge.
                    if (Data[0] == 0xb)
                    {
                        _radio.IsInIntervalLocationSending = false;

                        RemoveTxData(_env.PortGps, new byte[] { 0x05, 0x08, 0x22, 0x04, 0x24, 0x68, 0xAC, 0xE0, 0x51, 0x62 });
                    }

                    //GPS localizacion intervalo correcta
                    if (Data[0] == 0xd & !(Data[1] == 0xc))
                    {
                        if (string.IsNullOrEmpty(_radio.latitud))
                            RemoveTxData(_env.PortGps, DataToSend.GPS_INTERVAL_ON);

                        _radio.evento = 7;
                        HasGpsData = true;
                    }

                    //GPS localizacion intervalo stop
                    if (Data[0] == 0xb && Data[1] == 0x07)
                    {
                        RemoveTxData(_env.PortGps, DataToSend.GPS_INTERVAL_OFF);
                    }

                    if ((Data[0] == 0xb & Data[7] == 0xe1))
                    {
                        _radio.IsInEmergencySending = false;
                        RemoveTxData(_env.PortGps, DataToSend.EMERGENCY_ON);
                    }

                    if ((Data[0] == 0xd & Data[7] == 0xe1))
                    {                        
                        _transmitter.Add(GPSEmergencyEnabledRequest());

                        _radio.latitud = "0";
                        _radio.longitud = "0";
                        _radio.velocidad = "0";
                        _radio.dates[2].date = DateTime.Now;
                        _radio.emerg_on = true;
                        _radio.evento = 8;
                        if (!(Data[8] == 0x37 || Data[8] == 0x38))
                        {
                            HasGpsData = true;
                        }

                        toFrontEnd.MethodName = "emergency";
                    }

                    if (HasGpsData)
                    {
                        _radio.IsInInstantLocationSending = false;
                        //List<double> GpsDatum = ResolveDatum(DatumIndexStart, DatumIndexStart + 4, Data);
                        Helpers.GpsDatum Datum = new Helpers.GpsDatum(DatumIndexStart, DatumIndexStart + 4, Data);

                        _radio.latitud = Datum.latitude.ToString().Replace(",", ".");
                        _radio.longitud = Datum.longitude.ToString().Replace(",", ".");
                        _radio.velocidad = Datum.speed.ToString().Replace(",", ".");

                        _radio.hasGpsPosition = true;

                        _radio.dates[2].date = DateTime.Now;

                        if (!_radio.emerg_on)
                            toFrontEnd.MethodName = "gpsData";
                    }

                    toFrontEnd.Radio = _radio;

                    _uiDispatcher.Add(toFrontEnd);

                    Task.Run(() =>
                    {
                        GpsDataDomain.Save(_radio);
                    });

                    Task.Run(() =>
                    {
                        RadiosDomain.Save(_radio);
                    });
                }
            };
        }
    }
}
