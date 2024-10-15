#!/bin/bash
docker build -f ./demo.Dockerfile -t vorobalek/telegram-budget:demo-latest . && docker push vorobalek/telegram-budget:demo-latest