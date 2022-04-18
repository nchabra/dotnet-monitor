FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
COPY --from=ochinchina/supervisord:latest /usr/local/bin/supervisord /usr/local/bin/supervisord
ARG BUST_CACHE=$RANDOM
COPY src/supervisord.conf /app/supervisord.conf
COPY /artifacts/bin/dotnet-monitor/Release/net6.0/publish /app
ENV ASPNETCORE_URLS=
ENV COMPlus_EnableDiagnostics=0
ENV DotnetMonitor_DefaultProcess__Filters__0__Key=ProcessId
ENV DotnetMonitor_DefaultProcess__Filters__0__Value=1
ENV Logging__Console__FormatterName=json
ENV Logging__Console__FormatterOptions__TimestampFormat=yyyy-MM-ddTHH:mm:ss.fffffffZ
ENV Logging__Console__FormatterOptions__UseUtcTimestamp=true
# ENTRYPOINT [ "dotnet", "exec", "/app/dotnet-monitor.dll", "collect"]
WORKDIR /app
ENTRYPOINT [ "supervisord" ]
