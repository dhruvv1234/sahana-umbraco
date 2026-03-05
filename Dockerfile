# -------- BUILD STAGE --------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish


# -------- RUNTIME STAGE --------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

# Create required Umbraco directories
RUN mkdir -p /app/wwwroot/media \
    && mkdir -p /app/umbraco/Data \
    && mkdir -p /app/umbraco/Logs

ENTRYPOINT ["dotnet", "sahanaweb.dll"]