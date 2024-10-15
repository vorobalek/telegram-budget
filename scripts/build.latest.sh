#!/bin/bash
docker build -f ./Dockerfile -t vorobalek/telegram-budget:latest . && docker push vorobalek/telegram-budget:latest