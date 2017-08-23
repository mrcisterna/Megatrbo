using System;
using Megatrbo.Types;

namespace Mototrbo.Server.Types
{
    public interface IUIObject
    {
        Guid Id { get; set; }
        MototrboRadio Radio { get; set; }
        string MethodName { get; set; }
    }
}
