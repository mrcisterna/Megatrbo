using System;

namespace Mototrbo.Server.Types
{
    public interface IRadioObject
    {
        Guid Id { get; set; }
        Guid? ParentId { get; set; }
        byte[] Buffer { get; set; }
        DateTime Date { get; set; }
        int Port { get; set; }
        string Ip { get; set; }
        bool Enabled { get; set; }        
    }
}
