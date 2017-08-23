using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mototrbo.Server.CommServer
{
   internal static class DataToSend
    {
        internal static readonly byte[] ARS_ACKNOWLEDGE = new byte[] { 0x0, 0x02, 0xBF, 0x01 };
        internal static readonly byte[] GPS_INTERVAL_ON = new byte[] { 0x09, 10, 0x22, 0x04, 0x24, 0x68, 0xAC, 0xE0, 0x74, 0x34, 0x31, 0 };
        internal static readonly byte[] EMERGENCY_ON = new byte[] { 0x09, 0x09, 0x22, 0x04, 0x24, 0x68, 0xAC, 0xE1, 0x33, 0x4A, 0x02 };
        internal static readonly byte[] GPS_INTERVAL_OFF = new byte[] { 0x0F, 0x06, 0x22, 0x04, 0x24, 0x68, 0xAC, 0xE0 };
    }
}
