using System;

namespace Akeel.Utilities.ValidateResult
{
    public class ErrorDto
    {
        public string Message { get; set; }
        public int Code { get; set; }

        public string DetailsError { get; set; }

        public Exception Exception { get; set; }


        public static ErrorDto Factory(int code, string message, Exception exception = null)
        {
            return new ErrorDto()
            {
                Code = code,
                Message = message,
                Exception = exception
            };
        }
        public static ErrorDto Factory(Enum error, Exception exception = null)
        {
            return new ErrorDto()
            {
                Code = Convert.ToInt32(error),
                Message = error.ToString(),
                Exception = exception
            };
        }


        public ValidateResult<bool> Check()
        {
            return Factory(0, "");
        }
    }
}