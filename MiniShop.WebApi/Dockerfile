FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MiniShop.slnx ./
COPY MiniShop.WebApi/MiniShop.WebApi.csproj MiniShop.WebApi/
COPY MiniShop.ApplicationCore/MiniShop.ApplicationCore.csproj MiniShop.ApplicationCore/
COPY MiniShop.Infrastructure/MiniShop.Infrastructure.csproj MiniShop.Infrastructure/

RUN dotnet restore MiniShop.WebApi/MiniShop.WebApi.csproj 

COPY . .

RUN dotnet publish MiniShop.WebApi/MiniShop.WebApi.csproj \
                       -c Release \
                       -o /app/publish \
                       --no-restore 

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MiniShop.WebApi.dll"]