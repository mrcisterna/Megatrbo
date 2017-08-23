using System;

namespace Mototrbo.Server.Types
{
    public interface IWorker
    {
        void Start();
        void Stop();
        bool IsAlive { get; set; }
    }

    public interface IWorker<T> : IWorker
    {
        void Add(T payload);
    }
}
