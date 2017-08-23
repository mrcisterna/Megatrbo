using System;

namespace Mototrbo.Server.Types
{
    public interface IPayloadProcessor : IDisposable
    {
        Action ProcessorAction { get; }
    }
}
