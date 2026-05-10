#!/bin/bash
# Start Docker containers and open browser

echo "Starting Docker containers..."
docker-compose up -d

echo "Waiting for services to be ready..."
sleep 15

echo "Opening browser..."
start http://localhost:4200/
