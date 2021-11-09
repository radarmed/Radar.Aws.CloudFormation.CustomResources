#-------------------------------------------------------------------------
# Must have AWS CLI installed and configured for active profile having 
#  permissions to create all resources including IAM
# Docker must be installed with linux containers active
#-------------------------------------------------------------------------

Write-Host "***** Creating local ECR repo for RADAR cfn custom resources lambda container image"

Write-Host "***** Setting up"
$publicImageUri = 'public.ecr.aws/k0z1u8d8/radarmed/radar-aws-cloudformation-customresources:latest'
$x = (New-Guid).Guid
$bucket = "radarmed-cfn-$x"
$file1 = "radar-aws-cloudformation-customresources-init-1.yaml"
$file2 = "radar-aws-cloudformation-customresources-init-2.yaml"
$stack = "radar-aws-cloudformation-customresources"
$stack1 = "${stack}-1"
$stack2 = "${stack}-2"

Write-Host "***** Create temporary s3 bucket $bucket for cfn template"
aws s3api create-bucket --bucket $bucket --acl private
Write-Host "***** Copy cfn template *.yaml to s3"
aws s3 sync . "s3://$bucket" --exclude "*" --include "*.yaml"

Write-Host "***** Create stack $stack1 which creates repo."
aws cloudformation create-stack --stack-name $stack1 --template-url "https://$bucket.s3.amazonaws.com/$file1" --capabilities CAPABILITY_IAM
Write-Host "***** Waiting for stack to complete.  This might take a while."
aws cloudformation wait stack-create-complete --stack-name $stack1
Write-Host "***** Getting newly created repo URI"
$repoUri = aws cloudformation describe-stacks --stack-name $stack1 --query "Stacks[0].Outputs[?OutputKey=='PrivateRepoUri'].OutputValue" --output text
$repoUriTagged = "${repoUri}:latest"

Write-Host "***** Pulling public docker image from $publicImageUri"
docker image pull $publicImageUri
Write-Host "***** Logging in to ECR"
$ecrEndpoint = aws ecr get-authorization-token --query "authorizationData[0].proxyEndpoint" --output text
aws ecr get-login-password | docker login --username AWS --password-stdin $ecrEndpoint
Write-Host "***** Tagging public image for private repo"
docker image tag $publicImageUri $repoUriTagged
Write-Host "***** Pushing image to private repo"
docker image push $repoUriTagged

Write-Host "***** Create stack $stack2 which creates lambda function."
aws cloudformation create-stack --stack-name $stack2 --template-url "https://$bucket.s3.amazonaws.com/$file2" --capabilities CAPABILITY_IAM
Write-Host "***** Waiting for stack to complete.  This might take a while."
aws cloudformation wait stack-create-complete --stack-name $stack2

Write-Host "***** Cleaning up s3 and local docker"
aws s3 rb s3://$bucket --force
