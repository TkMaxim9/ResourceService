FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ResourceService.Api/ResourceService.Api.csproj ResourceService.Api/
RUN dotnet restore ResourceService.Api/ResourceService.Api.csproj

COPY ResourceService.Api/. ResourceService.Api/
RUN dotnet publish ResourceService.Api/ResourceService.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ResourceService.Api.dll"]
