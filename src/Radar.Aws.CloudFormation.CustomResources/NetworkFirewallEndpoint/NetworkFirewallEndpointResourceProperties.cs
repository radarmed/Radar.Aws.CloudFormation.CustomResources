namespace Radar.Aws.CloudFormation.CustomResources.NetworkFirewallEndpoint
{
    public class NetworkFirewallEndpointResourceProperties
    {
        public string[] EndpointIds { get; set; }
        public string AvailabilityZone { get; set;}
    }
}