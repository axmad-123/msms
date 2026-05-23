FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/MSMS.Domain/MSMS.Domain.csproj src/MSMS.Domain/
COPY src/MSMS.Application/MSMS.Application.csproj src/MSMS.Application/
COPY src/MSMS.Infrastructure/MSMS.Infrastructure.csproj src/MSMS.Infrastructure/
COPY src/MSMS.Api/MSMS.Api.csproj src/MSMS.Api/

RUN dotnet restore src/MSMS.Api/MSMS.Api.csproj

COPY src/ src/
RUN dotnet publish src/MSMS.Api/MSMS.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
EXPOSE 8080

ENTRYPOINT ["dotnet", "MSMS.Api.dll"]
