using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GWALib.General;
using GWAShared.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GitaWidyaAgniWeb.Infrastructure
{
    /// <summary>
    /// Aturan JSON untuk objek GWALib yang dikirim ke halaman:
    /// - setiap turunan BaseLib mendapat properti <c>Key</c> (EncDecrKey, yang di library diberi [JsonIgnore]);
    /// - setiap properti enum mendapat pasangan <c>&lt;Nama&gt;Text</c> berisi [Description]-nya;
    /// - properti byte[] (foto / berkas) tidak ikut dikirim; berkas diambil lewat endpoint unduh masing-masing.
    /// </summary>
    public static class LibJson
    {
        public static void Configure(JsonSerializerOptions options)
        {
            options.PropertyNamingPolicy = null;
            options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { Modify } };
        }

        private static void Modify(JsonTypeInfo info)
        {
            if (info.Kind != JsonTypeInfoKind.Object)
            {
                return;
            }
            for (var i = info.Properties.Count - 1; i >= 0; i--)
            {
                if (info.Properties[i].PropertyType == typeof(byte[]))
                {
                    info.Properties.RemoveAt(i);
                }
            }
            var names = info.Properties.Select(p => p.Name).ToHashSet();
            foreach (var property in info.Properties.ToList())
            {
                var enumType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                var getter = property.Get;
                if (!enumType.IsEnum || getter == null || !names.Add(property.Name + "Text"))
                {
                    continue;
                }
                var text = info.CreateJsonPropertyInfo(typeof(string), property.Name + "Text");
                text.Get = obj => getter(obj) is Enum value ? value.GetDescription() : null;
                info.Properties.Add(text);
            }
            if (typeof(BaseLib).IsAssignableFrom(info.Type) && names.Add("Key"))
            {
                var key = info.CreateJsonPropertyInfo(typeof(string), "Key");
                key.Get = obj => ((BaseLib)obj).EncDecrKey;
                info.Properties.Add(key);
            }
        }
    }

    /// <summary>
    /// Nilai form dibaca dengan InvariantCulture (desimal memakai titik, tanggal ISO), apa pun culture server.
    /// Menggantikan FormValueProviderFactory bawaan.
    /// </summary>
    public class InvariantFormValueProviderFactory : IValueProviderFactory
    {
        public async Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            var request = context.ActionContext.HttpContext.Request;
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();
                context.ValueProviders.Add(new FormValueProvider(BindingSource.Form, form, CultureInfo.InvariantCulture));
            }
        }
    }

    /// <summary>Pilihan dropdown dari enum (value = angka, text = [Description]).</summary>
    public static class EnumHelper
    {
        public record Option(object Value, string Text);

        public static List<Option> Options<T>(params T[] exclude) where T : struct, Enum
        {
            return Enum.GetValues<T>().Where(v => !exclude.Contains(v))
                .Select(v => new Option(Convert.ToInt32(v), v.GetDescription())).ToList();
        }
        public static string Text(Enum value) => value.GetDescription();
    }
}
