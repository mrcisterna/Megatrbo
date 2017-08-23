using System;

namespace Mototrbo.Server.Types
{
    public class RxObject :RadioObjectBase
    {
        #region Private variables      
        private Type _processorType;
        #endregion

        #region Public properties     
        public Type ProcessorType
        {
            get
            {
                return _processorType;
            }
            set
            {
                _processorType = value;
            }
        }
        #endregion       
    }
}
