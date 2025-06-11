FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src

# Copy all contents of the current folder into the container
COPY . .

RUN dotnet restore "./Z1R_SignalRHost.csproj"
RUN dotnet publish "./Z1R_SignalRHost.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Z1R_SignalRHost.dll"]
