FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
USER $APP_UID
WORKDIR /app
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MarketMonitor/MarketMonitor.csproj", "MarketMonitor/"]
RUN dotnet restore "MarketMonitor/MarketMonitor.csproj"
COPY . .
WORKDIR "/src/MarketMonitor"
RUN dotnet build "MarketMonitor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MarketMonitor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MarketMonitor.dll"]
