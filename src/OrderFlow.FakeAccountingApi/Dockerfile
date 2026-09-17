# Build context: repository root.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/OrderFlow.FakeAccountingApi/*.csproj src/OrderFlow.FakeAccountingApi/
RUN dotnet restore src/OrderFlow.FakeAccountingApi/OrderFlow.FakeAccountingApi.csproj

COPY src/OrderFlow.FakeAccountingApi/ src/OrderFlow.FakeAccountingApi/
RUN dotnet publish src/OrderFlow.FakeAccountingApi/OrderFlow.FakeAccountingApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 5099
ENV ASPNETCORE_HTTP_PORTS=5099
COPY --from=build /app/publish ./
USER app
ENTRYPOINT ["dotnet", "OrderFlow.FakeAccountingApi.dll"]
