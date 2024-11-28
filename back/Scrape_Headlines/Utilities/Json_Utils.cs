using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scrape_Headlines.Utilities
{
    public static class JsonUtils
    {
        /// <summary>
        /// Procore does not like non-UTF-8 characters - there is probablty a better way to do this in the WebClient headers...
        /// </summary>
        /// <param name="item"></param>
        static public void RemoveUtf16(object item)
        {
            var properties = item.GetType().GetProperties();
            foreach (var pi in properties)
            {
                var objValue = pi.GetValue(item);
                if (pi.PropertyType == typeof(string) && objValue != null)
                {
                    var valueUtf16 = objValue as string;
                    // Convert a string to utf-8 bytes.
                    byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(valueUtf16);

                    // Convert utf-8 bytes to a string.
                    string valueUtf8 = System.Text.Encoding.UTF8.GetString(utf8Bytes);
                    pi.SetValue(item, valueUtf8);
                }
            }
        }

        /// <summary>
        /// Is this JSON valid?  (Newtonsoft probably has even better tools to do this check...
        /// </summary>
        /// <param name="inStringForm"></param>
        static public bool IsValidJson(string inStringForm)
        {
            try
            {
                var dataBack = JsonConvert.DeserializeObject(inStringForm);
            }
            catch (Exception ex)
            {
                Log.Error("Json is not valid: " + ex.Message);
                return false;
            }
            return true;
        }

        public static string ToJson(this object value)
        {
            if (value == null)
                return "";
            return ToJson(
                value,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    Formatting = Newtonsoft.Json.Formatting.Indented
                }
            );
        }

        //public static string ToJsonNoTypes(this object value)
        //{
        //	if (value == null) return "";
        //	return ToJson(value, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None });
        //}
        public static string ToJsonNoTypesWithNulls(this object value)
        {
            if (value == null)
                return "";
            var result = ToJson(
                value,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None,
                    NullValueHandling = NullValueHandling.Include
                }
            );
            return result;
        }

        public static string ToJson(this object value, JsonSerializerSettings settings)
        {
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return JsonConvert.SerializeObject(value, settings);
        }

        public static string ToJsonPretty(this object value)
        {
            var seetings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            return JsonConvert.SerializeObject(
                value,
                Newtonsoft.Json.Formatting.Indented,
                seetings
            );
        }

        public static string ToJsonSkippingNulls(this object value)
        {
            return JsonConvert.SerializeObject(
                value,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
            );
        }

        public static JObject ParseJson(this string value)
        {
            if (value.IsNullOrEmpty())
                return null;
            var result = JObject.Parse(value);
            return result;
        }

        public static T FromJsonAnonymous<T>(this string value, T template_type)
        {
            // https://www.newtonsoft.com/json/help/html/DeserializeAnonymousType.htm
            var result = JsonConvert.DeserializeAnonymousType(value, template_type);
            return result;
        }

        public static object FromJson(this string value)
        {
            var obj = JsonConvert.DeserializeObject(
                value,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }
            );
            return obj;
        }

        public static T FromJson<T>(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return default(T);

            var item = JsonConvert.DeserializeObject<T>(
                value,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    NullValueHandling = NullValueHandling.Ignore
                }
            );

            return item;
        }

        public static T FromJson<T>(this string value, JsonSerializerSettings settings)
        {
            if (string.IsNullOrEmpty(value))
                return default(T);
            return JsonConvert.DeserializeObject<T>(value, settings);
        }

        public static T ConvertUtf16ToAscii<T>(T item)
        {
            var data = Newtonsoft.Json.JsonConvert.SerializeObject(item);
            byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(data);
            var testBytes = System.Text.Encoding.Convert(Encoding.UTF8, Encoding.ASCII, utf8Bytes);
            var test = System.Text.Encoding.Default.GetString(testBytes);
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(test);
            return result;
        }

        /// <summary>
        /// Procore uses numeric values for internal IDs, but Dimension code prefers strings for all ID values.  Some Procore APIs can parse string values in JSON to read numeric IDs; others can't (or won't).
        /// Apply this converter to properties when the API returns an "object could not be found" error on IDs known to be valid (e.g., <c>cost_code.standard_cost_code_id</c>)
        /// </summary>
        /// <remarks><c>ReadJson()</c> must be implemented to extend JsonConverter; however, a false value for <c>CanRead</c> shifts responsibility for reading the value back to the default converter.
        /// I would prefer not to leave a 'throw exception' statement in here, but for now, implementing a custom read function is unneccessary.</remarks>
        public class ToNumericJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return true;
            }

            public override bool CanRead => false;

            public override void WriteJson(
                JsonWriter writer,
                object value,
                JsonSerializer serializer
            )
            {
                long.TryParse(value.ToString(), out long num);
                writer.WriteValue(num);
            }

            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer
            )
            {
                throw new NotImplementedException();
            }
        }
    }
}
