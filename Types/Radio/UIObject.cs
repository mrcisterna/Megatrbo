using System;
using Megatrbo.Types;

namespace Mototrbo.Server.Types
{
    public class UIObject : IUIObject
    {
        public Guid Id { get; set; }
        public MototrboRadio Radio { get; set; }
        public string MethodName { get; set; }
    }
}
