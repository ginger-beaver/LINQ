FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["LabaLINQ/LabaLINQ.csproj", "LabaLINQ/"]
RUN dotnet restore "LabaLINQ/LabaLINQ.csproj"
COPY . .
WORKDIR "/src/LabaLINQ"
RUN dotnet build "LabaLINQ.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LabaLINQ.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LabaLINQ.dll"]
