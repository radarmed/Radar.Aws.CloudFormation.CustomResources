namespace Radar.Aws.CloudFormation.CustomResources.MultiResource
{
    using System;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;
    using Util;

    public abstract class MultiResourceBase
    {
        public virtual MultiResourceRequest Request { get; set; }
        public MultiResourceResponse Response { get; set; }
        public ILambdaContext Context { get; set; }

        protected void Log(string message)
        {
            Context.Logger.LogLine(message);
        }

        protected void LogJson(string message, object obj)
        {
            Log(string.Format(message, SerializeUtil.Serialize(obj)));
        }

        protected bool SendResponse()
        {
            try
            {
                string json = SerializeUtil.Serialize((object)Response);
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