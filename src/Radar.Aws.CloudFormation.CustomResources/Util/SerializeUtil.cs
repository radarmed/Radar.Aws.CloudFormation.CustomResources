namespace Radar.Aws.CloudFormation.CustomResources.Util
{
    using System.Text.Json;

    public static class SerializeUtil
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions();

        public static string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, _jsonSerializerOptions);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }

    }
}