using System;

namespace MyException
{
    //  解码错误
    public class DecodeException : ApplicationException
    {
        public DecodeException(string message) : base(message) { }
    }
}
