FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.csproj .
COPY sportsbet-root-ca.pem .
COPY NuGet.config .
ENV SSL_CERT_FILE=/source/sportsbet-root-ca.pem
RUN dotnet restore --configfile NuGet.config --no-cache --ignore-failed-sources

# copy and publish app and libraries
COPY . .
RUN dotnet --info
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["/app/Sparql.Migrator"]
