namespace Radar.Aws.CloudFormation.CustomResources.MultiResource
{
    using System;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;

    public abstract class MultiResourceRouterBase: MultiResourceBase
    {
        protected abstract IResourceHandler GetHandler(string resourceType);

        public async Task Route(MultiResourceRequest request, ILambdaContext context)
        {
            const string CUSTOM_PREFIX = "Custom::";

            Context = context;
            Request = request;
            Response = new MultiResourceResponse
            {
                StackId = request.StackId,
                RequestId = request.RequestId,
                LogicalResourceId = request.LogicalResourceId,
                PhysicalResourceId = request.PhysicalResourceId ?? Guid.NewGuid().ToString(),
                Reason = string.Empty,
                NoEcho = false,
                Status = "SUCCESS"
            };
            LogJson("REQUEST:\n{0}", request);

            try
            {
                if (!request.ResourceType.StartsWith(CUSTOM_PREFIX))
                {
                    throw new ApplicationException($"ResourceType ({request.ResourceType}) must start with '{CUSTOM_PREFIX}'.");
                }

                string resourceType = request.ResourceType.Substring(CUSTOM_PREFIX.Length);
                IResourceHandler handler = GetHandler(resourceType);

                if (handler == null)
                {
                    throw new ApplicationException($"ResourceType ({request.ResourceType}) is not handled.");
                }

                handler.Request = Request;
                handler.Response = Response;
                handler.Context = Context;

                bool sendResponse = true;

                switch (request.RequestType.ToLowerInvariant())
                {
                    case "create":
                        sendResponse = await handler.Create();
                        break;
                    case "update":
                        sendResponse = await handler.Update();
                        break;
                    case "delete":
                        sendResponse = await handler.Delete();
                        break;
                    default:
                        Response.Status = "FAILED";
                        Response.Reason = $"RequestType '{Request.RequestType}' not implemented.";
                        break;
                }
                if (sendResponse)
                {
                    SendResponse();
                }
            }
            catch (Exception ex)
            {
                Log($"Exception: {ex.Message}\n{ex.StackTrace}");
                Response.Status = "FAILED";
                Response.Reason = ex.Message;
                SendResponse();
            }            
        }

   }
}