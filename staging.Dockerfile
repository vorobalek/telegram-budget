FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet build "TelegramBudget.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelegramBudget.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ARG OTEL_VERSION=1.4.0
ADD https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/download/v${OTEL_VERSION}/otel-dotnet-auto-install.sh otel-dotnet-auto-install.sh
RUN apt-get update && apt-get install -y curl unzip && \
    OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto" sh otel-dotnet-auto-install.sh && \
    chmod +x /otel-dotnet-auto/instrument.sh
ENV OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto"
ENV OTEL_EXPORTER_OTLP_ENDPOINT=http://grafana-alloy:4318
ENV OTEL_SERVICE_NAME=telegram-budget-staging
ENV OTEL_RESOURCE_ATTRIBUTES="service.namespace=telegram-budget,deployment.environment=staging,service.instance.id=telegram-budget@ubuntu-0,service.version=staging"

ENTRYPOINT ["/otel-dotnet-auto/instrument.sh", "dotnet", "TelegramBudget.dll"]