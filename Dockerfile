FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Coolzo.sln .
COPY Coolzo.Api/Coolzo.Api.csproj Coolzo.Api/
COPY Coolzo.Application/Coolzo.Application.csproj Coolzo.Application/
COPY Coolzo.Contracts/Coolzo.Contracts.csproj Coolzo.Contracts/
COPY Coolzo.Domain/Coolzo.Domain.csproj Coolzo.Domain/
COPY Coolzo.Infrastructure/Coolzo.Infrastructure.csproj Coolzo.Infrastructure/
COPY Coolzo.Persistence/Coolzo.Persistence.csproj Coolzo.Persistence/
COPY Coolzo.Shared/Coolzo.Shared.csproj Coolzo.Shared/
COPY Coolzo.Worker/Coolzo.Worker.csproj Coolzo.Worker/

RUN dotnet restore

COPY . .

RUN dotnet publish Coolzo.Api/Coolzo.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000
ENTRYPOINT ["dotnet", "Coolzo.Api.dll"]