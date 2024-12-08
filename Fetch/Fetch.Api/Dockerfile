FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln ./
COPY Fetch.Api/*.csproj ./Fetch.Api/
COPY Fetch.Models/*.csproj ./Fetch.Models/
COPY Fetch.Consumer/*.csproj ./Fetch.Consumer/

RUN dotnet restore

COPY Fetch.Api/. ./Fetch.Api/
COPY Fetch.Models/. ./Fetch.Models/
COPY Fetch.Consumer/. ./Fetch.Consumer/

WORKDIR /app/Fetch.Api
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "Fetch.Api.dll"]
