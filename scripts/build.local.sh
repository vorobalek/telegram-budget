#!/bin/bash
docker build -f ./Dockerfile -t vorobalek/telegram-budget:local . && docker push vorobalek/telegram-budget:local