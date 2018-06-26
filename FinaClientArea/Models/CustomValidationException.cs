using System;

namespace FinaClientArea.Models
{
    public class CustomValidationException : Exception
    {
        public CustomValidationException()
            : base()
        {

        }
        public CustomValidationException(string Message)
            : base(Message)
        {

        }
        public CustomValidationException(string Message, Exception e)
            : base(Message, e)
        {

        }
    }
}