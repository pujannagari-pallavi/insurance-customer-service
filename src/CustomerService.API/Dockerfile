FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/CustomerService.API/CustomerService.API.csproj src/CustomerService.API/
COPY src/CustomerService.Application/CustomerService.Application.csproj src/CustomerService.Application/
COPY src/CustomerService.Domain/CustomerService.Domain.csproj src/CustomerService.Domain/
COPY src/CustomerService.Infrastructure/CustomerService.Infrastructure.csproj src/CustomerService.Infrastructure/
RUN dotnet restore src/CustomerService.API/CustomerService.API.csproj

COPY src/ src/
RUN dotnet publish src/CustomerService.API/CustomerService.API.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["sh", "-c", "if [ -n \"$PORT\" ]; then exec dotnet CustomerService.API.dll --urls http://0.0.0.0:$PORT; else exec dotnet CustomerService.API.dll; fi"]