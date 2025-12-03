#!/bin/bash

# CORS Fix - Backend Restart Script
# This script will restart the backend server with the updated CORS configuration

echo "🔄 Restarting Backend Server with CORS Fix..."
echo ""

# Navigate to backend directory
cd /Users/edward/Documents/StudyHubAPI/Study-Hub

# Kill any existing dotnet processes for Study-Hub
echo "📌 Stopping existing backend server..."
pkill -f "dotnet.*Study-Hub" 2>/dev/null

# Wait a moment
sleep 2

# Build the project
echo "🔨 Building project..."
dotnet build

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Build successful!"
    echo ""
    echo "🚀 Starting backend server with CORS fix..."
    echo "   Backend will be available at: https://3qrbqpcx-5212.asse.devtunnels.ms/api"
    echo "   CORS will allow: http://localhost:5173"
    echo ""
    echo "📊 Watch for CORS debug messages below:"
    echo "   🔍 CORS Request from origin: ..."
    echo "   ✅ CORS: Allowed localhost origin: ..."
    echo ""
    echo "----------------------------------------"
    echo ""
    
    # Start the server
    dotnet run
else
    echo ""
    echo "❌ Build failed! Please check the errors above."
    exit 1
fi

