#
# Release Version
#

FROM mcr.microsoft.com/dotnet/sdk:7.0 as builder

WORKDIR /BUILD

COPY QuantconnectLeanCoinbaseProTickersDownloader .

RUN dotnet publish QuantconnectLeanCoinbaseProTickersDownloader.csproj -c Release -o /output && \
    rm -rf QuantconnectLeanCoinbaseProTickersDownloader

FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine

ENV EXECUTABLE_PATH=/QuantconnectLeanCoinbaseProTickersDownloader
ENV SHARED_STORAGE_HOME=/Shared
ENV CONFIG_JSON_FILENAME=config.json
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

#Alpine
RUN apk update
RUN apk add bash icu-libs
#Ubuntu/Debian
# RUN apt update
# RUN apt install -y nano

WORKDIR $EXECUTABLE_PATH    
COPY --from=builder /output .
COPY tickers.txt .

#ADD DockerBuilders/UtilitiesForDockerImage/config.json .
# create config file dynamically
RUN echo "{" > $CONFIG_JSON_FILENAME && \
    echo "\"data-folder\": \"$SHARED_STORAGE_HOME/Data/\"," >> $CONFIG_JSON_FILENAME && \
    echo "\"data-directory\": \"$SHARED_STORAGE_HOME/Data/\"" >> $CONFIG_JSON_FILENAME && \
    echo "}" >> $CONFIG_JSON_FILENAME

# Set Crontab
RUN crontab -l | { cat; echo "59 23 * * * dotnet $EXECUTABLE_PATH/QuantconnectLeanCoinbaseProTickersDownloader.dll $SHARED_STORAGE_HOME $EXECUTABLE_PATH/tickers.txt > $SHARED_STORAGE_HOME/DownloadDataUntilNow.log"; } | crontab -

# CMD ["/bin/bash", "-c", "$SCRIPTS_PATH/DownloadDataUntilNow.sh -p $DOWNLOADER_PATH -t $SCRIPTS_PATH/tickers -h $SHARED_STORAGE_HOME > $SHARED_STORAGE_HOME/DownloadDataUntilNow.log"]
#Alpine
CMD ["crond", "-f" ] 