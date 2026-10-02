FROM mcr.microsoft.com/dotnet/sdk:10.0.401 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "src/SignalLab.Api/SignalLab.Api.csproj"

RUN dotnet publish "src/SignalLab.Api/SignalLab.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

RUN dotnet tool restore

RUN dotnet ef migrations bundle \
    --project "src/SignalLab.Infrastructure/SignalLab.Infrastructure.csproj" \
    --startup-project "src/SignalLab.Api/SignalLab.Api.csproj" \
    --configuration Release \
    --output /app/migrations/efbundle \
    --no-build

FROM mcr.microsoft.com/dotnet/aspnet:10.0.10 AS final

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

USER app

COPY --from=build /app/publish .

COPY --from=build /app/migrations/efbundle ./efbundle

ENTRYPOINT ["dotnet", "SignalLab.Api.dll"]