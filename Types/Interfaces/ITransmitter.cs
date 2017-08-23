using System;

namespace Mototrbo.Server.Types
{
    public interface ITransmitter : IWorker<TxObject>
    {
        void RemoveData(string ip, int port, byte[] data);
    }
}
