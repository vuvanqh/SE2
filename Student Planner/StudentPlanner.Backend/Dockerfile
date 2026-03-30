#1 - build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY *.sln* .

COPY StudentPlanner.Core/*.csproj StudentPlanner.Core/
COPY StudentPlanner.Infrastructure/*.csproj StudentPlanner.Infrastructure/
COPY StudentPlanner.UI/*.csproj StudentPlanner.UI/
COPY StudentPlanner.Tests/*.csproj StudentPlanner.Tests/

RUN dotnet restore

COPY . .

#2 - publish
WORKDIR /src/StudentPlanner.UI
RUN dotnet publish -c Release -o /app/publish


#3 - runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .
EXPOSE 8080

ENTRYPOINT ["dotnet", "StudentPlanner.UI.dll"]