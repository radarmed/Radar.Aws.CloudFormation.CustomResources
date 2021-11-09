namespace Radar.Aws.CloudFormation.CustomResources.MultiResource
{
    using System;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;

    public abstract class MultiResourceBase
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions();

        public virtual MultiResourceRequest Request { get; set; }
        public MultiResourceResponse Response { get; set; }
        public ILambdaContext Context { get; set; }

        protected string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, _jsonSerializerOptions);
        }

        protected dynamic ToDynamic(object obj)
        {
            string serialized = Serialize(obj);
            return JsonSerializer.Deserialize<dynamic>(serialized, _jsonSerializerOptions);
        }

        protected T FromDynamic<T>(dynamic obj)
        {
            string serialized = Serialize(obj);
            return JsonSerializer.Deserialize<T>(serialized, _jsonSerializerOptions);
        }

        protected void Log(string message)
        {
            Context.Logger.LogLine(message);
        }

        protected void LogJson(string message, object obj)
        {
            Log(string.Format(message, Serialize(obj)));
        }

        protected bool SendResponse()
        {
            try
            {
                string json = JsonSerializer.Serialize((object)Response, new JsonSerializerOptions());
                Log($"RESPONSE: {json}");

                var httpClient = new HttpClient();
                var httpRequest = new HttpRequestMessage(HttpMethod.Put, Request.ResponseURL)
                {
                    Content = new StringContent(json)
                };
                var httpResponse = httpClient.Send(httpRequest);
                string content = "";
                try
                {
                    var ctx = SynchronizationContext.Current;
                    SynchronizationContext.SetSynchronizationContext(null);
                    content = httpResponse.Content.ReadAsStringAsync().Result;
                    SynchronizationContext.SetSynchronizationContext(ctx);
                }
                catch {}
                Log($"PUT response: {httpResponse.StatusCode} - {httpResponse.ReasonPhrase}\n{content}");
                return true;
            }
            catch (Exception ex)
            {
                Log($"Exception: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        protected delegate Task<TResponse> RequestResponseDelegate<TRequest, TResponse>(TRequest request, CancellationToken ct);

        protected async Task<TResponse> RequestResponse<TRequest, TResponse>(TRequest request, RequestResponseDelegate<TRequest, TResponse> func)
        {
            LogJson(typeof(TRequest).Name, request);
            var response = await func(request, default(CancellationToken));
            LogJson(typeof(TResponse).Name, response);
            return response;
        }
 
    }
}