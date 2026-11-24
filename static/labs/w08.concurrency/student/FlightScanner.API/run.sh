#!/bin/sh
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0
./dotnet-install.sh --channel 9.0 --runtime aspnetcore
export PATH=$HOME/.dotnet:$PATH
dotnet run