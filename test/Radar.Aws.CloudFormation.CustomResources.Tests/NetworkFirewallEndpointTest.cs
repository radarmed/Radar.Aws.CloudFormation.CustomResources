using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Amazon.Lambda.TestUtilities;
using Radar.Aws.CloudFormation.CustomResources.MultiResource;
using Radar.Aws.CloudFormation.CustomResources.NetworkFirewallEndpoint;

namespace Radar.Aws.CloudFormation.CustomResources.Tests
{
    public class NetworkFirewallEndpointTest
    {
        [Fact]
        public async Task TestNetworkFirewallEndpoint()
        {
   
            // Invoke the lambda function and confirm the string was upper cased.
            RequestRouter router = new RequestRouter();
            var context = new TestLambdaContext();
            var request = new MultiResourceRequest
            {
                RequestType = "Create",
                LogicalResourceId = "NwFwEp",
                RequestId = Guid.NewGuid().ToString(),
                ResourceType = "Custom::NetworkFirewallEndpoint",
                StackId = "unit-test",
                ResponseURL = "https://webhook.site/247891d4-94d3-4604-aff0-01f7ae0a3733",
                ResourceProperties = new NetworkFirewallEndpointResourceProperties
                {
                    EndpointIds = new string[]
                    { 
                        "us-west-1:vpce-1234567890", 
                        "us-west-2:vpce-0987654321",
                        "us-east-1:vpce-0000000000", 
                        "us-east-2:vpce-1111111111" 
                    },
                    AvailabilityZone = "us-east-1"
                }
            };
            await router.Route(request, context);
        }

    }
}
