FROM public.ecr.aws/lambda/dotnet:5.0 AS base

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim as build
WORKDIR /src
COPY ["Radar.Aws.CloudFormation.CustomResources.csproj", "Radar.Aws.CloudFormation.CustomResources/"]
RUN dotnet restore "Radar.Aws.CloudFormation.CustomResources/Radar.Aws.CloudFormation.CustomResources.csproj"

WORKDIR "/src/Radar.Aws.CloudFormation.CustomResources"
COPY . .
RUN dotnet build "Radar.Aws.CloudFormation.CustomResources.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "Radar.Aws.CloudFormation.CustomResources.csproj" \
            --configuration Release \ 
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .