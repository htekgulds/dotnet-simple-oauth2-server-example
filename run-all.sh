#!/bin/bash

# Simple OAuth2 Server - Run All Services
echo "Starting Simple OAuth2 Server Demo..."

# Function to run a service in background
run_service() {
    local service_name=$1
    local service_path=$2
    local port=$3
    
    echo "Starting $service_name on port $port..."
    cd "$service_path"
    dotnet run &
    local pid=$!
    echo "$service_name PID: $pid"
    cd - > /dev/null
    return $pid
}

# Store PIDs for cleanup
pids=()

# Start OAuth2 Server
run_service "OAuth2 Server" "OAuth2.Server" "7000"
pids+=($!)

# Start User Service
run_service "User Service" "UserService.Mock" "7002"
pids+=($!)

# Start SMS Service
run_service "SMS Service" "SmsService.Mock" "7003"
pids+=($!)

# Start Demo App
run_service "Demo App" "DemoApp" "7001"
pids+=($!)

echo ""
echo "All services started!"
echo "Access the demo app at: https://localhost:7001"
echo ""
echo "Press Ctrl+C to stop all services..."

# Function to cleanup on exit
cleanup() {
    echo ""
    echo "Stopping all services..."
    for pid in "${pids[@]}"; do
        kill $pid 2>/dev/null
    done
    echo "All services stopped."
    exit 0
}

# Set trap for cleanup
trap cleanup SIGINT SIGTERM

# Wait for all background processes
wait
