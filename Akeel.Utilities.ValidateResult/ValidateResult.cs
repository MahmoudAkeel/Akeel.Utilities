using System;
using System.Collections.Generic;
using System.Linq;

namespace Akeel.Utilities.ValidateResult
{
    public class ValidateResult
    {

        public bool IsSuccess { get; set; }
        public ErrorDto Error { get; set; }
        public bool IsFailure => !IsSuccess;

        protected ValidateResult()
        {

        }
        public static ValidateResult ValidateRange(params ValidateResult[] values)
        {
            var errors = values.Where(c => c.IsFailure).ToList();
            if (errors.Any())
            {
                return new ValidateResult()
                {
                    IsSuccess = false,
                    Error = errors.FirstOrDefault(c => c != null)?.Error
                };
            }

            return new ValidateResult()
            {
                IsSuccess = true
            };
        }

        public static ValidateResult ValidateRange(IEnumerable<ValidateResult> values)
        {
            var errors = values.Where(c => c.IsFailure).ToList();
            if (errors.Any())
            {
                return new ValidateResult()
                {
                    IsSuccess = false,
                    Error = errors.FirstOrDefault(c => c != null)?.Error
                };
            }

            return new ValidateResult()
            {
                IsSuccess = true
            };
        }

        public static ValidateResult WithError(ErrorDto error)
        {
            return new ValidateResult()
            {
                IsSuccess = false,
                Error = error
            };
        }

        public static implicit operator ValidateResult(ErrorDto error) => WithError(error);
    }
    public class ValidateResult<T> : ValidateResult
    {
        private ValidateResult()
        {

        }

        public T Data { get; set; }
        public static ValidateResult<T> WithSuccess(T input)
        {
            return new ValidateResult<T>()
            {
                IsSuccess = true,
                Data = input
            };
        }

        public static ValidateResult<T> WithError(Enum error)
        {
            return WithError(Convert.ToInt32(error), error.ToString(), null);
        }
        public static ValidateResult<T> WithError(Enum error, string errorMessage)
        {
            return WithError(Convert.ToInt32(error), errorMessage, null);
        }
        public static ValidateResult<T> WithError(Enum error, Exception exception)
        {
            return WithError(Convert.ToInt32(error), error.ToString(), exception);
        }

        public static ValidateResult<T> WithError(Enum error, string errorMessage, Exception exception)
        {
            return WithError(Convert.ToInt32(error), errorMessage, exception);
        }

        public static ValidateResult<T> WithError(int errorCode, string errorMessage)
        {
            return WithError(errorCode, errorMessage, null);
        }

        public static ValidateResult<T> WithError(ErrorDto error)
        {
            return new ValidateResult<T>()
            {
                IsSuccess = false,
                Error = error
            };
        }
        public static ValidateResult<T> WithError(int errorCode, string errorMessage, Exception exception)
        {
            return new ValidateResult<T>()
            {
                IsSuccess = false,
                Error = new ErrorDto()
                {
                    Code = errorCode,
                    Message = errorMessage,
                    Exception = exception
                }
            };
        }

        public static implicit operator ValidateResult<T>(ErrorDto error) => WithError(error);
        public static implicit operator ValidateResult<T>(T t) => WithSuccess(t);


    }

    public static class ValidateResultExtension
    {
        public static ValidateResult<T> ToSuccessResult<T>(this T t) => ValidateResult<T>.WithSuccess(t);
        public static ValidateResult<T> ToErrorResult<T>(this T t, ErrorDto error) => ValidateResult<T>.WithError(error);
        public static ValidateResult ToErrorResult<T>(this ErrorDto error, T t) => ValidateResult<T>.WithError(error);
    }



}
