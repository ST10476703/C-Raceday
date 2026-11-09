# Build only MVC. The API runs separately and is reached over HTTP.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY RaceDayMVC/RaceDayMVC.csproj RaceDayMVC/
RUN dotnet restore RaceDayMVC/RaceDayMVC.csproj
COPY RaceDayMVC/ RaceDayMVC/
RUN dotnet publish RaceDayMVC/RaceDayMVC.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "RaceDayMVC.dll"]
