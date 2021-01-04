FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /app


# Copy csproj and restore as distinct layers
COPY consumer/consumer.csproj ./consumer/
COPY dal/dal.csproj ./dal/
RUN dotnet restore consumer/consumer.csproj

# Copy everything else and build
COPY ./consumer ./consumer
COPY ./dal ./dal
RUN dotnet publish consumer/consumer.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "consumer.dll"]