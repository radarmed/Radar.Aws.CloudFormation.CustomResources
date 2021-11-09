[CmdletBinding()]
param (
    [Parameter()]
    [bool]
    $UpdateFunc = $false
)

if ($null -eq $stack1)
{
    ./radar-aws-cloudformation-customresources-init-vars.ps1
}

Write-Host "***** Getting repo URI"
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

if ($UpdateFunc)
{
    $funcId = aws cloudformation describe-stacks --stack-name $stack2 --query "Stacks[0].Outputs[?OutputKey=='ServiceToken'].OutputValue" --output text
    aws lambda update-function-code --function-name $funcId --image-uri $repoUriTagged
}

