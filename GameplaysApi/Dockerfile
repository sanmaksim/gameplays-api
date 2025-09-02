# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /src

COPY ["GameplaysApi.csproj", "./"]
RUN dotnet restore "GameplaysApi.csproj"

COPY . .
# Build for the local architecture
RUN if [ "$TARGETARCH" = "arm64" ]; then \
        dotnet publish "GameplaysApi.csproj" -c Release -r linux-arm64 --self-contained true --output /app/publish /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true; \
    else \
        dotnet publish "GameplaysApi.csproj" -c Release -r linux-x64 --self-contained true --output /app/publish /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true; \
    fi

# Runtime stage using runtime-deps for smaller image
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["./GameplaysApi"]
