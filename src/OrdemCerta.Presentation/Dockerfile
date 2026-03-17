# Etapa 1 - Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder
WORKDIR /app

# Copia os .csproj e restaura dependências (cache layer)
COPY ["src/OrdemCerta.Presentation/OrdemCerta.Presentation.csproj", "src/OrdemCerta.Presentation/"]
COPY ["src/OrdemCerta.Application/OrdemCerta.Application.csproj", "src/OrdemCerta.Application/"]
COPY ["src/OrdemCerta.Domain/OrdemCerta.Domain.csproj", "src/OrdemCerta.Domain/"]
COPY ["src/OrdemCerta.Shared/OrdemCerta.Shared.csproj", "src/OrdemCerta.Shared/"]
COPY ["src/OrdemCerta.Infrastructure/OrdemCerta.Infrastructure.csproj", "src/OrdemCerta.Infrastructure/"]

WORKDIR /app/src/OrdemCerta.Presentation
RUN dotnet restore

# Copia todo o código e publica
WORKDIR /app
COPY . .

WORKDIR /app/src/OrdemCerta.Presentation
RUN dotnet publish -c Release -o /app/publish --no-restore

# Etapa 2 - Runtime (imagem menor, sem SDK)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runner
WORKDIR /app

COPY --from=builder /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000

ENTRYPOINT ["dotnet", "OrdemCerta.Presentation.dll"]