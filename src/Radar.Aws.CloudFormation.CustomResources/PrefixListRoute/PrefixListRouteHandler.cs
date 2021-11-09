namespace Radar.Aws.CloudFormation.CustomResources.PrefixListRoute
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Radar.Aws.CloudFormation.CustomResources.MultiResource;
    using Amazon.EC2;
    using Amazon.EC2.Model;

    public class PrefixListRouteHandler:
        MultiResourceHandlerBase<
            PrefixListRouteResourceProperties, 
            PrefixListRouteResponseData>
    {
        public const string ResourceTypeId = "PrefixListRoute";

        protected override async Task<bool> OnCreate()
        {
            bool needCreate = true;
            if (ResourceProperties?.AllowFakeImport ?? false)
            {
                needCreate = await CheckForExistingRoute();
            }
            if (needCreate)
            {
                await CreateRoute();
            }
            return true;
        }

        private async Task<bool> CheckForExistingRoute()
        {
            using (var ec2 = new AmazonEC2Client())
            {
                var response = await  RequestResponse<DescribeRouteTablesRequest, DescribeRouteTablesResponse>(new DescribeRouteTablesRequest
                {
                    Filters = new List<Filter>
                    {
                        new Filter("route-table-id", new List<string> { ResourceProperties.RouteTableId }),
                        new Filter("route.destination-prefix-list-id", new List<string> { ResourceProperties.DestinationPrefixListId })
                    }
                }, ec2.DescribeRouteTablesAsync);

                var existing = response.RouteTables
                    .Where(rt => rt.RouteTableId == ResourceProperties.RouteTableId)
                    .SelectMany(rt => rt.Routes)
                    .Where(r => r.DestinationPrefixListId == ResourceProperties.DestinationPrefixListId
                                && (r.CarrierGatewayId ?? string.Empty) == (ResourceProperties.CarrierGatewayId ?? string.Empty)
                                && (r.EgressOnlyInternetGatewayId ?? string.Empty) == (ResourceProperties.EgressOnlyInternetGatewayId ?? string.Empty)
                                && (r.GatewayId ?? string.Empty) == (ResourceProperties.GatewayId ?? string.Empty)
                                && (r.InstanceId ?? string.Empty) == (ResourceProperties.InstanceId ?? string.Empty)
                                && (r.LocalGatewayId ?? string.Empty) == (ResourceProperties.LocalGatewayId ?? string.Empty)
                                && (r.NatGatewayId ?? string.Empty) == (ResourceProperties.NatGatewayId ?? string.Empty)
                                && (r.NetworkInterfaceId ?? string.Empty) == (ResourceProperties.NetworkInterfaceId ?? string.Empty)
                                && (r.TransitGatewayId ?? string.Empty) == (ResourceProperties.TransitGatewayId ?? string.Empty)
                                && (r.VpcPeeringConnectionId ?? string.Empty) == (ResourceProperties.VpcPeeringConnectionId ?? string.Empty)
                    );
                return existing != null;
            }
        }

        private async Task CreateRoute()
        {
            using (var ec2 = new AmazonEC2Client())
            {
                var response = await RequestResponse<CreateRouteRequest, CreateRouteResponse>(new CreateRouteRequest
                {
                    CarrierGatewayId = ResourceProperties.CarrierGatewayId,
                    DestinationPrefixListId = ResourceProperties.DestinationPrefixListId,
                    EgressOnlyInternetGatewayId = ResourceProperties.EgressOnlyInternetGatewayId,
                    GatewayId = ResourceProperties.GatewayId,
                    InstanceId = ResourceProperties.InstanceId,
                    LocalGatewayId = ResourceProperties.LocalGatewayId,
                    NatGatewayId = ResourceProperties.NatGatewayId,
                    NetworkInterfaceId = ResourceProperties.NetworkInterfaceId,
                    RouteTableId = ResourceProperties.RouteTableId,
                    TransitGatewayId = ResourceProperties.TransitGatewayId,
                    VpcEndpointId = ResourceProperties.VpcEndpointId,
                    VpcPeeringConnectionId = ResourceProperties.VpcPeeringConnectionId
                }, ec2.CreateRouteAsync);

                if (!response.Return)
                {
                    throw new ApplicationException($"CreateRoute failed, requestId {response.ResponseMetadata.RequestId}");
                }
            }
        }

        protected override Task<bool> OnUpdate()
        {
            Response.PhysicalResourceId = Guid.NewGuid().ToString();
            return OnCreate();
        }

        protected override async Task<bool> OnDelete()
        {
            using (var ec2 = new AmazonEC2Client())
            {
                var response = await RequestResponse<DeleteRouteRequest, DeleteRouteResponse>(new DeleteRouteRequest
                {
                    DestinationPrefixListId = ResourceProperties.DestinationPrefixListId,
                    RouteTableId = ResourceProperties.RouteTableId
                }, ec2.DeleteRouteAsync);
            }
            return true;
        }
    }
}