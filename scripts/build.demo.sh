#!/bin/bash
docker build -f ./Dockerfile -t vorobalek/telegram-budget:demo . && docker push vorobalek/telegram-budget:demo