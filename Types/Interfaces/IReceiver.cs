using System;

namespace Mototrbo.Server.Types
{
    public interface IReceiver : IWorker
    {
        string Name { get; }
    }
}
