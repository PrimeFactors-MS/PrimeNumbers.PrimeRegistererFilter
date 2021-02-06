#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["PrimeNumbers.PrimeRegistererFilter.Worker/PrimeNumbers.PrimeRegistererFilter.Worker.csproj", "PrimeNumbers.PrimeRegistererFilter.Worker/"]
COPY ["PrimeNumbers.PrimeRegistererFilter.Core/PrimeNumbers.PrimeRegistererFilter.Core.csproj", "PrimeNumbers.PrimeRegistererFilter.Core/"]
RUN dotnet restore "PrimeNumbers.PrimeRegistererFilter.Worker/PrimeNumbers.PrimeRegistererFilter.Worker.csproj"
COPY . .
WORKDIR "/src/PrimeNumbers.PrimeRegistererFilter.Worker"
RUN dotnet build "PrimeNumbers.PrimeRegistererFilter.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PrimeNumbers.PrimeRegistererFilter.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PrimeNumbers.PrimeRegistererFilter.Worker.dll"]