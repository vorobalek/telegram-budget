#!/bin/bash
docker build -f ./Dockerfile -t vorobalek/telegram-budget:staging . && docker push vorobalek/telegram-budget:staging