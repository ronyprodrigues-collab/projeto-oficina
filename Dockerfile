FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["projetos.csproj", "./"]
RUN dotnet restore "projetos.csproj"

COPY . .
RUN dotnet publish "projetos.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
COPY render-entrypoint.sh /app/render-entrypoint.sh

RUN chmod +x /app/render-entrypoint.sh
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0
EXPOSE 8080

ENTRYPOINT ["/app/render-entrypoint.sh"]
