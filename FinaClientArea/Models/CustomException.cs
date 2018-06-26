using System;

namespace FinaClientArea.Models
{
    public class CustomException : Exception
    {
        public CustomException()
            : base()
        {

        }
        public CustomException(string Message, bool reload = false)
            : base(Message)
        {
            this.reload = reload;
        }
        public CustomException(string Message, int errorType, bool reload = false)
            : base(Message)
        {
            this.reload = reload;
            this.ErrorType = errorType;
        }
        public CustomException(string Message, Exception e, bool reload = false)
            : base(Message, e)
        {
            this.reload = reload;
        }
        public CustomException(string Message, Exception e, int errorType, bool reload = false)
            : base(Message, e)
        {
            this.reload = reload;
            this.ErrorType = errorType;
        }
        public bool NoDefaultMessage { get; set; }
        public int ErrorType { get; set; }
        public bool reload { get; set; }
    }
}