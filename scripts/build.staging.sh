#!/bin/bash
docker build -f ./staging.Dockerfile -t vorobalek/telegram-budget:staging . && docker push vorobalek/telegram-budget:staging