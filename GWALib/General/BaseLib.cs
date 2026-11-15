using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace GWALib.General
{
    public abstract class BaseLib
    {
        [JsonIgnore]
        [BindNever]
        [SwaggerSchema(ReadOnly = true)]
        public string EncDecrKey { get; set; } = null!;
    }
}
