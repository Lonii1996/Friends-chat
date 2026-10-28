FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ChatServer/ChatServer/ChatServer.csproj", "ChatServer/"]
COPY ["ChatServer/ChatServer.Tests/ChatServer.Tests.csproj", "ChatServer.Tests/"]
RUN dotnet restore "ChatServer/ChatServer.csproj"


COPY ChatServer/ChatServer/ ChatServer/
COPY ChatServer/ChatServer.Tests/ ChatServer.Tests/

WORKDIR "/src/ChatServer"
RUN dotnet build "ChatServer.csproj" -c Release -o /app/build


FROM build AS testrunner
WORKDIR "/src/ChatServer.Tests"

RUN dotnet test "ChatServer.Tests.csproj" -c Release



FROM testrunner AS publish
WORKDIR "/src/ChatServer"
RUN dotnet publish "ChatServer.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ChatServer.dll"]
