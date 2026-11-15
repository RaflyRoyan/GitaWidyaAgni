using GWAShared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWAShared.Results
{
    public class Response
    {
        public ResponseStatus Status { get; init; }
        public string Message { get; init; } = string.Empty;
    }
    public class Response<T> : Response
    {
        public T? Data { get; init; }
    }
}
