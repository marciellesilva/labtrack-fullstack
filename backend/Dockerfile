FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restaura dependências primeiro para aproveitar o cache de camadas do Docker.
COPY LabTrack.sln ./
COPY src/LabTrack.Domain/LabTrack.Domain.csproj src/LabTrack.Domain/
COPY src/LabTrack.Application/LabTrack.Application.csproj src/LabTrack.Application/
COPY src/LabTrack.Infrastructure/LabTrack.Infrastructure.csproj src/LabTrack.Infrastructure/
COPY src/LabTrack.Api/LabTrack.Api.csproj src/LabTrack.Api/
RUN dotnet restore src/LabTrack.Api/LabTrack.Api.csproj

COPY src/ src/
RUN dotnet publish src/LabTrack.Api/LabTrack.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "LabTrack.Api.dll"]
