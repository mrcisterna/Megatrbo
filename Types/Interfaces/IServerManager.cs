using System;
using System.Collections.Generic;
using Megatrbo.Types;

namespace Mototrbo.Server.Types
{
    public interface IServerManager
    {
        void Initialize();
        void Start();
        void Stop();
        IEnumerable<MototrboRadio> Radios { get; set; }
    }
}
