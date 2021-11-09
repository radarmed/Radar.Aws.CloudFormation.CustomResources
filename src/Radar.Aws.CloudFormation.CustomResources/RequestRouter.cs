// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: Amazon.Lambda.Core.LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Radar.Aws.CloudFormation.CustomResources
{
    using System;
    using System.Linq;

    using MultiResource;
    using NetworkFirewallEndpoint;
    using PrefixListRoute;

    public class RequestRouter: MultiResourceRouterBase
    {
        protected override IResourceHandler GetHandler(string resourceType)
        {
            switch (resourceType)
            {
                case NetworkFirewallEndpointHandler.ResourceTypeId:
                    return new NetworkFirewallEndpointHandler();
                case PrefixListRouteHandler.ResourceTypeId:
                    return new PrefixListRouteHandler();
            }
            return null;
        }
    }
   
}
