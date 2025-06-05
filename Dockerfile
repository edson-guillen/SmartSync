# Estágio de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar os arquivos de projeto
COPY ["SmartSync.API/SmartSync.API.csproj", "SmartSync.API/"]
COPY ["SmartSync.Application/SmartSync.Application.csproj", "SmartSync.Application/"]
COPY ["SmartSync.Domain/SmartSync.Domain.csproj", "SmartSync.Domain/"]
COPY ["SmartSync.Infraestructure/SmartSync.Infraestructure.csproj", "SmartSync.Infraestructure/"]

# Restaurar dependências
RUN dotnet restore "SmartSync.API/SmartSync.API.csproj"

# Copiar todo o código fonte
COPY . .

# Publicar a aplicação
RUN dotnet publish "SmartSync.API/SmartSync.API.csproj" -c Release -o /app/publish

# Estágio final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copiar os arquivos publicados
COPY --from=build /app/publish .

# Configurar variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV TZ=America/Sao_Paulo

# Expor a porta 80
EXPOSE 80

# Healthcheck
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Definir o ponto de entrada
ENTRYPOINT ["dotnet", "SmartSync.API.dll"]