FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /app
# Copy csproj and restore as distinct layers
COPY dal/dal.csproj ./dal/
COPY source/source.csproj ./source/
RUN dotnet restore source/source.csproj

# Copy everything else and build
COPY ./source ./source
COPY ./dal ./dal
RUN dotnet publish source/source.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim 
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS="http://0.0.0.0:5000"
EXPOSE 5000
ENTRYPOINT ["dotnet", "source.dll"]