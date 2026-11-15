using GWAShared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.General
{
    internal static class ServiceHandler
    {
        internal static Response<T> Handle<T>(Func<T> process, Func<T, Response<T>> onSuccess, Func<string, Response<T>> onFailed)
        {
            try
            {
                return onSuccess(process());
            }
            catch (ArgumentException ex)
            {
                return onFailed(ex.Message);
            }
            catch (Exception ex)
            {
                return ResponseFactory.InternalServerError<T>(ex.GetBaseException().Message);
            }
        }
        internal static async Task<Response<T>> HandleAsync<T>(Func<Task<T>> process, Func<T, Response<T>> onSuccess, Func<string, Response<T>> onFailed)
        {
            try
            {
                return onSuccess(await process());
            }
            catch (ArgumentException ex)
            {
                return onFailed(ex.Message);
            }
            catch (Exception ex)
            {
                return ResponseFactory.InternalServerError<T>(ex.GetBaseException().Message);
            }
        }
    }
}
