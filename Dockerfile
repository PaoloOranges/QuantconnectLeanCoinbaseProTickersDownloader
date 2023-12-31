FROM mcr.microsoft.com/dotnet/sdk:7.0 as builder

WORKDIR /BUILD

COPY QuantconnectLeanCoinbaseProTickersDownloader .

RUN dotnet publish QuantconnectLeanCoinbaseProTickersDownloader.csproj -c Release -o /output && \
    rm -rf QuantconnectLeanCoinbaseProTickersDownloader

FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine

ENV EXECUTABLE_PATH=/QuantconnectLeanCoinbaseProTickersDownloader
ENV SHARED_STORAGE_HOME=/Shared
ENV CONFIG_JSON_FILENAME=config.json
ENV DOWNLOADER_BIN=$EXECUTABLE_PATH/QuantconnectLeanCoinbaseProTickersDownloader.dll
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

#Alpine
RUN apk update
RUN apk add bash icu-libs

WORKDIR $EXECUTABLE_PATH    
COPY --from=builder /output .
COPY tickers.txt .

# create config file dynamically
RUN echo "{" > $CONFIG_JSON_FILENAME && \
    echo "\"data-folder\": \"$SHARED_STORAGE_HOME/Data/\"," >> $CONFIG_JSON_FILENAME && \
    echo "\"data-directory\": \"$SHARED_STORAGE_HOME/Data/\"" >> $CONFIG_JSON_FILENAME && \
    echo "}" >> $CONFIG_JSON_FILENAME

# Set Crontab
RUN crontab -l | { cat; echo "30 23 * * * dotnet $DOWNLOADER_BIN $EXECUTABLE_PATH/$CONFIG_JSON_FILENAME $SHARED_STORAGE_HOME $EXECUTABLE_PATH/tickers.txt > $SHARED_STORAGE_HOME/DownloadDataUntilNow.log"; } | crontab -

CMD ["crond", "-f" ] 