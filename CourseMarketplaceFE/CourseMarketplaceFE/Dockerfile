# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# corrected path to project's csproj
COPY CourseMarketplaceFE/CourseMarketplaceFE/CourseMarketplaceFE.csproj CourseMarketplaceFE/
RUN dotnet restore CourseMarketplaceFE/CourseMarketplaceFE.csproj

COPY . .
WORKDIR /src/CourseMarketplaceFE
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
# Provide a default backend URL; override in docker-compose if desired
ENV BackendApiUrl=http://backend:80/api/
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "CourseMarketplaceFE.dll"]