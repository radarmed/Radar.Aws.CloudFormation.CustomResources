namespace Radar.Aws.CloudFormation.CustomResources.NetworkFirewallEndpoint
{
    using System.Linq;
    using System.Threading.Tasks;
    using Radar.Aws.CloudFormation.CustomResources.MultiResource;

    public class NetworkFirewallEndpointHandler:
        MultiResourceHandlerBase<
            NetworkFirewallEndpointResourceProperties, 
            NetworkFirewallEndpointResponseData>
    {
        public const string ResourceTypeId = "NetworkFirewallEndpoint";

        protected override Task<bool> OnCreate()
        {
            var endpointId = ResourceProperties.EndpointIds
                .Select(x => x.Split(':'))
                .Where(x => x.Length == 2 && x[0] == ResourceProperties.AvailabilityZone)
                .Select(x => x[1])
                .FirstOrDefault();
            Log( $"Selected endpoint: {endpointId}");

            if (endpointId == null)
            {
                Response.Status = "FAILED";
                Response.Reason = $"EndpointId not found for Availability Zone '{ResourceProperties.AvailabilityZone}'.";
            }
            else
            {
                ResponseData.AzEndpointId = endpointId;
            }
            return TrueTask;
        }

        protected override Task<bool> OnUpdate()
        {
            return OnCreate();
        }
    }
}