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
    public class SmsProcessor : PayloadProcessorBase
    {
        public SmsProcessor(IRadioObject payload, IEnvironment env, ILog log, IEnumerable<MototrboRadio> radios, ITransmitter transmitter, IUIDispatcher uiDispatcher)
            : base(payload, env, log, radios, transmitter, uiDispatcher)
        {
            _action = () =>
            {
                string radioIp = payload.Ip;
                DateTime currentDate = DateTime.Now;

                if (_radio != null)
                {
                    byte[] Data = payload.Buffer;

                    UIObject toFrontEnd = new UIObject();

                    if (Data[2] == 191)
                    {
                        _radio.IsInsmsSending = false;
                        RemoveTxData(_env.PortSms);
                    }
                    if (Data[2] != 191)
                    {
                        //Sms acknowloedge
                        _transmitter.Add(SMSAcknowledge());
                        //

                        byte[] textB = Data.Skip(10 + Data[3]).ToArray();

                        string Text = System.Text.Encoding.Unicode.GetString(textB);

                        SmsDataTable dataTable = new SmsDataTable()
                        {
                            de = _radio.radio_id.ToString(),
                            evento = 9,
                            fecha = DateTime.Now,
                            para = _radio.despacho,
                            sms = Text,
                            sms_id = -1
                        };

                        _radio.message = Text;

                        _radio.dates[3].date = DateTime.Now;

                        toFrontEnd.MethodName = "smsIn";

                        toFrontEnd.Radio = _radio;

                        _uiDispatcher.Add(toFrontEnd);

                        Task.Run(() =>
                        {
                            RadiosDomain.Save(_radio);
                        });

                        Task.Run(() =>
                        {
                            SmsInDataDomain.Save(dataTable);
                        });
                    }
                }
            };
        }
    }
}
