namespace Radar.Aws.CloudFormation.CustomResources.MultiResource
{
    public class MultiResourceResponse
    {
        public string Status { get; set; }
        public string Reason { get; set; }
        public string PhysicalResourceId { get; set; }
        public string StackId { get; set; }
        public string RequestId { get; set; }
        public string LogicalResourceId { get; set; }
        public bool NoEcho { get; set; }
        public dynamic Data { get; set; }
    }

}