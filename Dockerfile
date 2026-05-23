FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Backend/src/MSMS.Domain/MSMS.Domain.csproj Backend/src/MSMS.Domain/
COPY Backend/src/MSMS.Application/MSMS.Application.csproj Backend/src/MSMS.Application/
COPY Backend/src/MSMS.Infrastructure/MSMS.Infrastructure.csproj Backend/src/MSMS.Infrastructure/
COPY Backend/src/MSMS.Api/MSMS.Api.csproj Backend/src/MSMS.Api/

RUN dotnet restore Backend/src/MSMS.Api/MSMS.Api.csproj

COPY Backend/src/ Backend/src/
RUN dotnet publish Backend/src/MSMS.Api/MSMS.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "MSMS.Api.dll"]
