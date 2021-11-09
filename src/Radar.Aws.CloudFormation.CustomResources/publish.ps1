[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $Version,

    [Parameter()]
    [string]
    $Image = "radar-aws-cloudformation-customresources",

    [Parameter()]
    [bool]
    $Latest = $true,

    [Parameter()]
    [string]
    $PublicRepo = "public.ecr.aws/k0z1u8d8/radarmed/"

)

$localVersionTag = "${Image}:${Version}" 
$localLatestTag = "${Image}:latest" 
$publicVersionTag = "${PublicRepo}${Image}:${Version}" 
$publicLatestTag = "${PublicRepo}${Image}:latest" 

Write-Host "--- localVersionTag: ${localVersionTag}"
Write-Host "--- localLatestTag: ${localLatestTag}"
Write-Host "--- publicVersionTag: ${publicVersionTag}"
Write-Host "--- publicLatestTag: ${publicLatestTag}"

Write-Host "--- Docker Build"
docker build . -t $localVersionTag
if ($null -ne $PublicRepo)
{
    Write-Host "--- Docker Tag ${localVersionTag} ${publicVersionTag}"
    docker tag $localVersionTag $publicVersionTag
}
if ($Latest)
{
    Write-Host "--- Docker Tag ${localVersionTag} ${localLatestTag}"
    docker tag $localVersionTag $localLatestTag
    if ($null -ne $PublicRepo)
    {
        Write-Host "--- Docker Tag ${localVersionTag} ${publicLatestTag}"
        docker tag $localVersionTag $publicLatestTag
    }
}

if ($null -ne $PublicRepo)
{
    Write-Host "--- AWS ECR-Public Login"
    aws ecr-public get-login-password | docker login -u AWS --password-stdin public.ecr.aws
    Write-Host "--- Docker push ${publicVersionTag}"
    docker push $publicVersionTag
    if ($Latest)
    {
        Write-Host "--- Docker push ${publicLatestTag}"
        docker push $publicLatestTag
    }
}