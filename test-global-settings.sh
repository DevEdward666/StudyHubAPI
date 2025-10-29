#!/bin/bash

# Global Settings Test Script
# Tests the Global Settings API endpoints

BASE_URL="https://3qrbqpcx-5212.asse.devtunnels.ms"
ADMIN_TOKEN="YOUR_ADMIN_JWT_TOKEN_HERE"

echo "================================"
echo "Global Settings API Test"
echo "================================"
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Function to test endpoint
test_endpoint() {
    local method=$1
    local endpoint=$2
    local description=$3
    local data=$4
    
    echo "Testing: $description"
    echo "Endpoint: $method $endpoint"
    
    if [ "$method" = "GET" ]; then
        response=$(curl -s -w "\n%{http_code}" -X GET \
            -H "Authorization: Bearer $ADMIN_TOKEN" \
            -H "Content-Type: application/json" \
            "$BASE_URL$endpoint")
    else
        response=$(curl -s -w "\n%{http_code}" -X $method \
            -H "Authorization: Bearer $ADMIN_TOKEN" \
            -H "Content-Type: application/json" \
            -d "$data" \
            "$BASE_URL$endpoint")
    fi
    
    http_code=$(echo "$response" | tail -n 1)
    body=$(echo "$response" | sed '$d')
    
    if [ "$http_code" -ge 200 ] && [ "$http_code" -lt 300 ]; then
        echo -e "${GREEN}✓ Success (HTTP $http_code)${NC}"
        echo "$body" | jq '.' 2>/dev/null || echo "$body"
    else
        echo -e "${RED}✗ Failed (HTTP $http_code)${NC}"
        echo "$body" | jq '.' 2>/dev/null || echo "$body"
    fi
    echo ""
}

# 1. Get all settings
test_endpoint "GET" "/admin/settings" "Get all global settings"

# 2. Get settings by category
test_endpoint "GET" "/admin/settings/category/general" "Get settings by category (general)"

# 3. Get specific setting by key
test_endpoint "GET" "/admin/settings/key/payment.fixed_rate" "Get setting by key (payment.fixed_rate)"

# 4. Create a new setting
CREATE_DATA='{
  "key": "test.sample_setting",
  "value": "test_value",
  "description": "A test setting created by script",
  "dataType": "string",
  "category": "general",
  "isPublic": false,
  "isEncrypted": false
}'
test_endpoint "POST" "/admin/settings/create" "Create new setting" "$CREATE_DATA"

# 5. Update a setting (you'll need to replace the ID)
# First, get the setting we just created to get its ID
echo "Note: To test update, first get the setting ID from the create response above"
echo ""

# 6. Validate a setting
VALIDATE_DATA='{
  "key": "payment.fixed_rate",
  "value": "50"
}'
test_endpoint "POST" "/admin/settings/validate" "Validate setting value" "$VALIDATE_DATA"

# 7. Get recent changes
test_endpoint "GET" "/admin/settings/changes/recent?count=10" "Get recent changes (last 10)"

echo "================================"
echo "Test Complete"
echo "================================"
echo ""
echo "Instructions:"
echo "1. Replace 'YOUR_ADMIN_JWT_TOKEN_HERE' with your actual admin JWT token"
echo "2. Update BASE_URL if needed"
echo "3. Run: chmod +x test-global-settings.sh && ./test-global-settings.sh"
echo ""
echo "To get your admin token:"
echo "1. Login as admin in the app"
echo "2. Open browser DevTools > Application/Storage > Local Storage"
echo "3. Look for 'auth_token' key"
echo ""

