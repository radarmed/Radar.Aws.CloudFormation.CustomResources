namespace Radar.Aws.CloudFormation.CustomResources.MultiResource
{
    public class MultiResourceRequest
    {
        public string RequestType { get; set; }
        public string ResponseURL { get; set; }
        public string StackId { get; set; }
        public string RequestId { get; set; }
        public string ResourceType { get; set; }
        public string LogicalResourceId { get; set; }
        public string PhysicalResourceId { get; set; }
        public dynamic ResourceProperties { get; set; }
        public dynamic OldResourceProperties { get; set; }
    }
   
}