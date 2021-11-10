# RADAR AWS CloudFormation Custom Resources

## Credits

Work done here has been performed as part of my daily job at [RADAR Medical Systems](https://www.radarmed.com/), which is a part of [RadNet, Inc.](https://www.radnet.com/).  All of RADAR's systems have been hosted on AWS for since 2012.  This work is the result of me working towards putting all of our resources into CloudFormation Stacks.

## Overview
As I have been putting our existing AWS infrastructure into CloudFormation Stacks, I have found a few pieces to be missing, such as routes with prefix list destinations or getting the endpoint ID for a network firewall for a specific availability zone -- and likely others.

After a lot of research and trying different techniques and experiments, I settled on creating custom resources to handle the missing pieces.  This repository is the result of that.

Currently, all of the custom resources are handled in a single lambda function, differentiated by the specific `Custom::ResourceType` used in the template.

The lambda function is written in .NET 5 and is packaged in a linux container.  The container image is published to a public ECR repository and scripts and templates are provided to import the image into a private ECR repository (which is required for Lambda) and create the Lambda function to be referenced in the custom resources.

The source code is also available if you want to see how it functions or extend it yourself.

## Tools
You will need the following tools installed locally to configure your AWS Account to use these custom resources.  The same tools can be used to build and deploy the image, though the default settings currently point to the public ECR repository owned by RADAR.

* AWS CLI v2 (configured with a default profile that has permissions to manage resources)
* Docker
* PowerShell

## Installing RADAR Custom Resources to your AWS Region

If you have the required tools installed and configured as described above:

1. Clone this repo
1. From PowerShell, change to the `/aws` folder
1. Execute `.\radar-aws-cloudformation-customresources-init.ps1`

This will do the following:

1. Create a temporary S3 bucket for CloudFormation templates
1. Upload `*.yaml` files to the bucket
1. Create a CloudFormation Stack called `radar-aws-cloudformation-customresources-1` which creates a private ECR repository named `radar-aws-cloudformation-customresources`
1. Pulls the container image from the public ECR repository and pushes it to the newly created private repository
1. Creates another CloudFormation Stack called `radar-aws-cloudformation-customresources-2` which creates an IAM role for the Lambda function and the Lambda function itself, referencing the "latest" tag for the container image in the private repository
1. Deletes the temporary S3 bucket

**NOTE**: The docker image is left locally.  Delete that manually (the local and remote tags) if you want to save space.

The second stack group exports an Output with the name `RADAR-RadarCustomResourcesServiceToken` that is used for the `ServiceToken` property of a custom resource.  For example:

```yaml
Resources:
    MyPrefixListRoute:
        Type: Custom::PrefixListRoute
        Properties:
            ServiceToken: !ImportValue RADAR-RadarCustomResourcesServiceToken
            RouteTableId: !Ref Rtb
            DestinationPrefixListId: !GetAtt PrefixList.PrefixListId
            GatewayId: !Ref Igw
```

## Implemented Custom Resource

The following custom resources are currently implemented.  I have written a framework that makes it fairly easy to add new custom resources.  This will be explained in a later section.

### NetworkFirewallEndpoint

When a `AWS::NetworkFirewall::Firewall` is created, you specify subnet mappings for different Availability Zones.  In order to create routes to the specific endpoints in each AZ, you need a way to access them. They are listed in the `EndpointIds` attribute of the Firewall, but not in any way usable in CloudFormation.

Create a `Custom::NetworkFirewallEndpoint` for each availability zone to get the endpoint Id.

```yaml
Resources:

  # EndpointId for 1st AZ specified in firewall
  FirewallEndpoint1:
    Type: Custom::NetworkFirewallEndpoint
    Properties:
      ServiceToken: !GetAtt !ImportValue RADAR-RadarCustomResourcesServiceToken
      EndpointIds: !GetAtt Firewall.EndpointIds
      AvailabilityZone: !Sub '${AWS::Region}${AZ1}'

  # EndpointId for 2nd AZ specified in firewall
  FirewallEndpoint2:
    Type: Custom::NetworkFirewallEndpoint
    Properties:
      ServiceToken: !GetAtt !ImportValue RADAR-RadarCustomResourcesServiceToken
      EndpointIds: !GetAtt Firewall.EndpointIds
      AvailabilityZone: !Sub '${AWS::Region}${AZ2}'

  # Using the EndpointId in a route
  RouteIgwIngress1:
    Type: AWS::EC2::Route
    Properties:
      RouteTableId: !Ref RouteTableIgw
      DestinationCidrBlock: !Sub "10.${VpcOctet}.${SubnetIngress1Octet}.0/24"
      VpcEndpointId: !GetAtt FirewallEndpoint1.AzEndpointId
```

### PrefixListRoute

`AWS::EC2::Route` does not support `DestinationPrefixListId`.  Boom.  Here it is.  Use `Custom::PrefixListRoute` for a route with prefix list destinations.

```yaml
Resources:

  RtPrefixTest:
    Type: Custom::PrefixListRoute
    Properties:
      ServiceToken: !ImportValue RADAR-RadarCustomResourcesServiceToken
      RouteTableId: !Ref Rtb
      DestinationPrefixListId: !GetAtt PrefixList.PrefixListId
      GatewayId: !Ref Igw
```

## More to come

Just wanted to get the initial stuff documented.