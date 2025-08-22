# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["GameplaysApi.csproj", "./"]
RUN dotnet restore "GameplaysApi.csproj"

COPY . .
RUN dotnet publish "GameplaysApi.csproj" -c Release -r linux-x64 \
    --self-contained true --output /app/publish /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true

# Runtime stage using runtime-deps for smaller image
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["./GameplaysApi"]
