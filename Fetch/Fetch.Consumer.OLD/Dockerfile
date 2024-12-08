FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln ./
COPY Fetch.Api/*.csproj ./Fetch.Api/
COPY Fetch.Models/*.csproj ./Fetch.Models/
COPY Fetch.Consumer/*.csproj ./Fetch.Consumer/

RUN dotnet restore

COPY . ./

WORKDIR /src/Fetch.Consumer
RUN dotnet publish "Fetch.Consumer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Fetch.Consumer.dll"]
