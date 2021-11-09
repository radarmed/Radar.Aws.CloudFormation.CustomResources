namespace Radar.Aws.CloudFormation.CustomResources.PrefixListRoute
{
    public class PrefixListRouteResourceProperties
    {
        public bool? AllowFakeImport { get; set; }
        public string RouteTableId { get; set; }
        public string DestinationPrefixListId { get; set; }
        public string CarrierGatewayId { get; set; }
        public string EgressOnlyInternetGatewayId { get; set; }
        public string GatewayId { get; set; }
        public string InstanceId { get; set; }
        public string LocalGatewayId { get; set; }
        public string NatGatewayId { get; set; }
        public string NetworkInterfaceId { get; set; }
        public string TransitGatewayId { get; set; }
        public string VpcEndpointId { get; set; }
        public string VpcPeeringConnectionId { get; set; }
    }
}
