#-------------------------------------------------------------------------
# Must have AWS CLI installed and configured for active profile having 
#  permissions to create all resources including IAM
# Docker must be installed with linux containers active
#-------------------------------------------------------------------------

Write-Host "***** Creating local ECR repo for RADAR cfn custom resources lambda container image"

Write-Host "***** Setting up"
$x = (New-Guid).Guid
$bucket = "radarmed-cfn-$x"
./radar-aws-cloudformation-customresources-init.ps1

Write-Host "***** Create temporary s3 bucket $bucket for cfn template"
aws s3api create-bucket --bucket $bucket --acl private
Write-Host "***** Copy cfn template *.yaml to s3"
aws s3 sync . "s3://$bucket" --exclude "*" --include "*.yaml"

Write-Host "***** Create stack $stack1 which creates repo."
aws cloudformation create-stack --stack-name $stack1 --template-url "https://$bucket.s3.amazonaws.com/$file1" --capabilities CAPABILITY_IAM
Write-Host "***** Waiting for stack to complete.  This might take a while."
aws cloudformation wait stack-create-complete --stack-name $stack1

./radar-aws-cloudformation-customresources-repo.ps1

Write-Host "***** Create stack $stack2 which creates lambda function."
aws cloudformation create-stack --stack-name $stack2 --template-url "https://$bucket.s3.amazonaws.com/$file2" --capabilities CAPABILITY_IAM
Write-Host "***** Waiting for stack to complete.  This might take a while."
aws cloudformation wait stack-create-complete --stack-name $stack2

Write-Host "***** Cleaning up s3 and local docker"
aws s3 rb s3://$bucket --force
