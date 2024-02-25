#! /bin/bash

while getopts j:c:s:p:t:h: flag
do
    case "${flag}" in
        j) CONFIG_JSON_FILE=${OPTARG};;
        c) COINBASE_API_KEY=${OPTARG};;
        s) COINBASE_API_SECRET=${OPTARG};;
        p) DOWNLOADER_BIN=${OPTARG};;
        t) TICKERS_FILE=${OPTARG};;
        h) SHARED_STORAGE_HOME=${OPTARG};;
    esac
done

if [[ -z "${CONFIG_JSON_FILE}" ]]; then
  echo "No CONFIG_JSON_FILE defined, exiting"
  exit -1
fi

if [[ -z "${COINBASE_API_KEY}" ]]; then
  echo "No COINBASE_API_KEY defined, exiting"
  exit -1
fi

if [[ -z "${COINBASE_API_SECRET}" ]]; then
  echo "No COINBASE_API_SECRET defined, exiting"
  exit -1
fi

if [[ -z "${SHARED_STORAGE_HOME}" ]]; then
  echo "No SHARED_STORAGE_HOME defined, exiting"
  exit -1
fi

if [[ -z "${DOWNLOADER_BIN}" ]]; then
  echo "No DOWNLOADER_BIN defined, exiting"
  exit -1
fi

if [[ -z "${TICKERS_FILE}" ]]; then
  echo "No TICKERS_FILE defined, exiting"
  exit -1
fi

# generate config file 
cat >$CONFIG_JSON_FILE <<EOF
{
  "coinbase-api-key": "$COINBASE_API_KEY",
  "coinbase-api-secret": "$COINBASE_API_SECRET",
  "data-folder": "$SHARED_STORAGE_HOME/Data/",
  "data-directory": "$SHARED_STORAGE_HOME/Data/"
}
EOF

dotnet $DOWNLOADER_BIN $CONFIG_JSON_FILE $SHARED_STORAGE_HOME $TICKERS_FILE > $SHARED_STORAGE_HOME/DownloadDataUntilNow.log
