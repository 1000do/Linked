# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# corrected path to project's csproj
COPY CourseMarketplaceBE/CourseMarketplaceBE/CourseMarketplaceBE.csproj CourseMarketplaceBE/
RUN dotnet restore CourseMarketplaceBE/CourseMarketplaceBE.csproj

COPY . .
WORKDIR /src/CourseMarketplaceBE
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "CourseMarketplaceBE.dll"]