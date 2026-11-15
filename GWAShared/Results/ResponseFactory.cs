using GWAShared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWAShared.Results
{
    public class ResponseFactory
    {
        public static Response<T> DataSaved<T>(T? data = default)
        {
            return new Response<T>
            {
                Status = ResponseStatus.Success,
                Message = ResponseMessages.DataSaved,
                Data = data
            };
        }
        public static Response<T> DataUpdated<T>(T? data = default)
        {
            return new Response<T>
            {
                Status = ResponseStatus.Success,
                Message = ResponseMessages.DataUpdated,
                Data = data
            };
        }
        public static Response<T> DataDeleted<T>(T? data = default)
        {
            return new Response<T>
            {
                Status = ResponseStatus.Success,
                Message = ResponseMessages.DataDeleted,
                Data = data
            };
        }
        public static Response<T> DataSaveFailed<T>(string message = "", T? data = default)
        {
            return new Response<T>
            {
                Status = ResponseStatus.Error,
                Message = $"{ResponseMessages.DataSaveFailed} {message}".Trim(),
                Data = data
            };
        }
        public static Response<T> DataUpdateFailed<T>(string message = "", T? data = default)
        {
            return new Response<T>
            {
                Status = ResponseStatus.Error,
                Message = $"{ResponseMessages.DataUpdateFailed} {message}".Trim(),
                Data = data
            };
        }
        public static Response<T> DataDeleteFailed<T>(string message = "", T? data = default)
        {
            return new Response<T>
            {
                Status = ResponseStatus.Error,
                Message = $"{ResponseMessages.DataDeleteFailed} {message}".Trim(),
                Data = data
            };
        }
        public static Response<T> InternalServerError<T>(string message = "", T? data = default)
        {
            return new Response<T>
            {
                Status = ResponseStatus.Error,
                Message = $"{ResponseMessages.InternalServerError} {message}".Trim(),
                Data = data
            };
        }
        public static Response<T> DataFound<T>(T? data = default)
        {
            return new Response<T>
            {
                Status = ResponseStatus.Success,
                Message = ResponseMessages.DataFound,
                Data = data
            };
        }
        public static Response<T> DataNotFound<T>(T? data = default)
        {
            return new Response<T>
            {
                Status = ResponseStatus.Error,
                Message = ResponseMessages.DataNotFound,
                Data = data
            };
        }
        public static Response<T> Success<T>(string message, T? data = default)
        {
            return new Response<T>
            {
                Status = ResponseStatus.Success,
                Message = message,
                Data = data
            };
        }
        public static Response<T> Error<T>(string message, T? data = default)
        {
            return new Response<T>
            {
                Status = ResponseStatus.Error,
                Message = message,
                Data = data
            };
        }
    }
}
