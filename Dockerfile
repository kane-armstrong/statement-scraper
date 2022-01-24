FROM mcr.microsoft.com/dotnet/core/6.0.100-alpine3.14 AS build
ARG Configuration=Release
WORKDIR /app

COPY . ./
RUN dotnet publish src/WorkerService -c $Configuration -o ../../publish -r alpine-x64

FROM mcr.microsoft.com/dotnet/core/runtime-deps:6.0.1-bullseye-slim
# To make talking to SQL Server work
RUN apk add --no-cache icu-libs
# Makes PuppeteerSharp work in Docker
RUN apt-get update && apt-get install -y libx11-6 libx11-xcb1 libatk1.0-0 \
	libgtk-3-0 libcups2 libdrm2 libxkbcommon0 libxcomposite1 libxdamage1 \
	libxrandr2 libgbm1 libpango-1.0-0 libcairo2 libasound2 libxshmfence1 libnss3
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
COPY --from=build /publish ./
ENTRYPOINT ["./WorkerService"]
