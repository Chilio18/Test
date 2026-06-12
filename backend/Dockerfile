FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/UnameIT.RevenueIntelligence.API/UnameIT.RevenueIntelligence.API.csproj", "src/UnameIT.RevenueIntelligence.API/"]
COPY ["src/UnameIT.RevenueIntelligence.Application/UnameIT.RevenueIntelligence.Application.csproj", "src/UnameIT.RevenueIntelligence.Application/"]
COPY ["src/UnameIT.RevenueIntelligence.Infrastructure/UnameIT.RevenueIntelligence.Infrastructure.csproj", "src/UnameIT.RevenueIntelligence.Infrastructure/"]
COPY ["src/UnameIT.RevenueIntelligence.Domain/UnameIT.RevenueIntelligence.Domain.csproj", "src/UnameIT.RevenueIntelligence.Domain/"]

RUN dotnet restore "src/UnameIT.RevenueIntelligence.API/UnameIT.RevenueIntelligence.API.csproj"

COPY . .

RUN dotnet build "src/UnameIT.RevenueIntelligence.API/UnameIT.RevenueIntelligence.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/UnameIT.RevenueIntelligence.API/UnameIT.RevenueIntelligence.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Run as non-root
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "UnameIT.RevenueIntelligence.API.dll"]
