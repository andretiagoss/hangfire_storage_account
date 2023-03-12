using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Hangfire.API.Infrastructure.AzureStorage
{
    public static class AzureBlobHelper
    {
        public static string GenerateCsvString<T>(IEnumerable<T> list)
        {
            var sb = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties();

            string[] headers = properties.Select(p => p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? p.Name).OrderBy(x => Array.IndexOf(properties, typeof(T).GetProperty(x))).ToArray();
            sb.AppendLine(string.Join(",", headers));

            foreach (T obj in list)
            {
                object[] values = properties.Select(p => p.GetValue(obj)).Select(v => v != null ? Convert.ToString(v, CultureInfo.InvariantCulture) : string.Empty).ToArray();
                sb.AppendLine(string.Join(",", values));
            }

            return sb.ToString();
        }
    }
}
