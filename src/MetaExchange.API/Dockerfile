FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/MetaExchange.API/MetaExchange.API.csproj", "src/MetaExchange.API/"]
COPY ["src/MetaExchange.Common/MetaExchange.Common.csproj", "src/MetaExchange.Common/"]
RUN dotnet restore "src/MetaExchange.API/MetaExchange.API.csproj"
COPY . .
WORKDIR "/src/src/MetaExchange.API"
RUN dotnet build "MetaExchange.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MetaExchange.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MetaExchange.API.dll"]