FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

WORKDIR /app

RUN dotnet tool install --global dotnet-reportgenerator-globaltool

COPY src/HookHandler.Api/HookHandler.Api.csproj ./src/HookHandler.Api/HookHandler.Api.csproj
COPY HookHandler.sln ./

RUN dotnet restore

COPY . ./

# running build separate from publish so that when we run tests,
# then the libraries will be in the expected directories.
# If we only build as part of publish, the test dlls won't be built
# and the tests won't run.
RUN dotnet build --no-restore --configuration Release
RUN dotnet publish --no-build --configuration Release --output out src/HookHandler.Api

FROM mcr.microsoft.com/dotnet/aspnet:5.0

WORKDIR /app

COPY --from=build /app/out .

# Default to Development (aka "local") environment to load settings from the appsettings.Development.json
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:8000
EXPOSE 8000

# Setting the group id explicitly so we can use it in the deployment's pod's securityContext
# (see chart/templates/deployment.yaml)
RUN addgroup --gid 1000 dotnet && \
    adduser --gid 1000 --disabled-password --gecos '' dotnet

USER dotnet

ENTRYPOINT ["dotnet", "HookHandler.Api.dll"]
