FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY "duckduckimagedownloader.csproj" .
RUN dotnet restore "duckduckimagedownloader.csproj"
COPY . .
RUN dotnet build "duckduckimagedownloader.csproj" -c Release -o /app/build
RUN dotnet publish "duckduckimagedownloader.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "duckduckimagedownloader.dll"]