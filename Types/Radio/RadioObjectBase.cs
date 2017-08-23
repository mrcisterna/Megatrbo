using System;
using System.Net;

namespace Mototrbo.Server.Types
{
    public abstract class RadioObjectBase : IRadioObject
    {
        #region Private variables
        protected Guid _id;
        protected Guid? _parentId;
        protected byte[] _buffer;
        protected DateTime _date;
        protected int _port;
        protected string _ip;
        protected bool _enabled;
        #endregion

        #region Public properties
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public Guid? ParentId
        {
            get { return _parentId; }
            set { _parentId = value; }
        }
        public byte[] Buffer
        {
            get { return _buffer; }
            set { _buffer = value; }
        }
        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }
        public string Ip
        {
            get { return _ip; }
            set { _ip = value; }
        }
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }
        #endregion

        public T Clone<T>(T original) where T : IRadioObject, new()
        {
            var x = new T();

            foreach (var prop in typeof(T).GetProperties())
            {
                x.GetType().GetProperty(prop.Name).SetValue(x, prop.GetValue(original));
            }

            return x;
        }
    }
}
