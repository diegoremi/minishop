FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MiniShop.slnx ./
COPY MiniShop.PaymentWorker/MiniShop.PaymentWorker.csproj MiniShop.PaymentWorker/
COPY MiniShop.ApplicationCore/MiniShop.ApplicationCore.csproj MiniShop.ApplicationCore/

RUN dotnet restore MiniShop.PaymentWorker/MiniShop.PaymentWorker.csproj

COPY . .

RUN dotnet publish MiniShop.PaymentWorker/MiniShop.PaymentWorker.csproj \
    -c Release \
    -o /app/publish \
    --no-restore 

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MiniShop.PaymentWorker.dll"]