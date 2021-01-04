FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /app
# Copy csproj and restore as distinct layers
COPY dal/dal.csproj ./dal/
COPY proxy/proxy.csproj ./proxy/
RUN dotnet restore proxy/proxy.csproj

# Copy everything else and build
COPY ./proxy ./proxy
COPY ./dal ./dal
RUN dotnet publish proxy/proxy.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim 
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS="http://0.0.0.0:5000"
EXPOSE 5000
ENTRYPOINT ["dotnet", "proxy.dll"]