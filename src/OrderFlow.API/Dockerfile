# Build context: repository root.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first for layer caching; only project files are needed here.
# Restoring the API project pulls its referenced projects transitively.
COPY src/OrderFlow.API/*.csproj src/OrderFlow.API/
COPY src/OrderFlow.Application/*.csproj src/OrderFlow.Application/
COPY src/OrderFlow.Domain/*.csproj src/OrderFlow.Domain/
COPY src/OrderFlow.Infrastructure/*.csproj src/OrderFlow.Infrastructure/
RUN dotnet restore src/OrderFlow.API/OrderFlow.API.csproj

COPY . ./
RUN dotnet publish src/OrderFlow.API/OrderFlow.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app/publish ./
USER app
ENTRYPOINT ["dotnet", "OrderFlow.API.dll"]
