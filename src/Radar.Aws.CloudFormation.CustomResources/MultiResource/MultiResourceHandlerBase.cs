namespace Radar.Aws.CloudFormation.CustomResources.MultiResource
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;

    // The interface the the MultiResourceRouterBase cares about
    public interface IResourceHandler
    {
        Task<bool> Create();
        Task<bool> Update();
        Task<bool> Delete();

        MultiResourceRequest Request { set; }
        MultiResourceResponse Response { set; }
        ILambdaContext Context { set; }
    }

    // Base class for resource handlers.
    public abstract class MultiResourceHandlerBase<TRequest, TResponse>: 
        MultiResourceBase, IResourceHandler
        where TRequest: class, new()
        where TResponse: class, new()
    {
        protected TRequest ResourceProperties { get; private set;}
        protected TRequest OldResourceProperties { get; private set; }
        protected TResponse ResponseData { get; private set; } = new TResponse();

        private MultiResourceRequest _request;
        public override MultiResourceRequest Request 
        { 
            get => _request;
            
            set
            {
                _request = value;
                if (value.ResourceProperties != null)
                {
                    ResourceProperties = FromDynamic<TRequest>(value.ResourceProperties);
                }
                if (value.OldResourceProperties != null)
                {
                    OldResourceProperties = FromDynamic<TRequest>(value.OldResourceProperties);
                }
            }
        }

        private static Task<bool> GetTrueTask()
        {
            var tcsTrue = new TaskCompletionSource<bool>();
            tcsTrue.SetResult(true);
            return tcsTrue.Task;
        }
        protected static readonly Task<bool> TrueTask = GetTrueTask();

        protected abstract Task<bool> OnCreate();
        protected virtual Task<bool> OnUpdate() { return TrueTask; }
        protected virtual Task<bool> OnDelete() { return TrueTask; }

        private async Task<bool> HandleAction(Func<Task<bool>> action)
        {
            bool sendResponse = await action();
            if (sendResponse)
            {
                Response.Data = ToDynamic(ResponseData);
            }
            return sendResponse;
        }

        public async Task<bool> Create()
        {
            return await HandleAction(() => OnCreate());
        }

        public async Task<bool> Update()
        {
            return await HandleAction(() => OnUpdate());
        }

        public async Task<bool> Delete()
        {
            return await HandleAction(() => OnDelete());
        }

    }
}