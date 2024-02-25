FROM mcr.microsoft.com/dotnet/sdk:7.0 as builder

WORKDIR /build/src

COPY QuantconnectLeanCoinbaseProTickersDownloader ./QuantconnectLeanCoinbaseProTickersDownloader
COPY QuantconnectLeanCoinbaseProTickersDownloader.sln .
COPY Lean.Brokerages.CoinbasePro ./Lean.Brokerages.CoinbasePro 

WORKDIR /build

RUN dotnet publish src/QuantconnectLeanCoinbaseProTickersDownloader.sln -c Release -o /output && \
    rm -rf src

FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine

ARG EXECUTABLE_PATH=/QuantconnectLeanCoinbaseProTickersDownloader
ARG TICKERS_FILE_NAME=tickers.txt
ARG ENTRY_POINT_FILE_NAME=entrypoint.sh

ENV SHARED_STORAGE_HOME=/Shared
ENV CONFIG_JSON_FILE=$EXECUTABLE_PATH/config.json
ENV TICKERS_FILE=$EXECUTABLE_PATH/$TICKERS_FILE_NAME
ENV DOWNLOADER_BIN=$EXECUTABLE_PATH/QuantconnectLeanCoinbaseProTickersDownloader.dll
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

#Alpine
RUN apk update
RUN apk add bash icu-libs

WORKDIR $EXECUTABLE_PATH    
COPY --from=builder /output .
COPY scripts/entrypoint.sh .
COPY $TICKERS_FILE_NAME .

# Set Crontab
RUN crontab -l | { cat; echo "30 23 * * * bash $EXECUTABLE_PATH/$ENTRY_POINT_FILE_NAME > $SHARED_STORAGE_HOME/DownloadDataUntilNow.log"; } | crontab -

CMD ["crond", "-f" ] 