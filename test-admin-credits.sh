#!/bin/bash

# Admin Add Credits Endpoint Test Script
# Usage: ./test-admin-credits.sh

# Configuration
API_BASE_URL="http://localhost:5000/api"  # Update with your actual API URL
ADMIN_TOKEN=""  # Replace with actual admin JWT token
USER_ID=""      # Replace with target user's GUID

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}=== Admin Add Credits Test ===${NC}\n"

# Check if variables are set
if [ -z "$ADMIN_TOKEN" ]; then
    echo -e "${RED}Error: ADMIN_TOKEN is not set${NC}"
    echo "Please set the ADMIN_TOKEN variable with your admin JWT token"
    exit 1
fi

if [ -z "$USER_ID" ]; then
    echo -e "${RED}Error: USER_ID is not set${NC}"
    echo "Please set the USER_ID variable with the target user's GUID"
    exit 1
fi

# Test 1: Add credits successfully
echo -e "${YELLOW}Test 1: Add 100 credits to user${NC}"
RESPONSE=$(curl -s -X POST "${API_BASE_URL}/admin/credits/add-approved" \
  -H "Authorization: Bearer ${ADMIN_TOKEN}" \
  -H "Content-Type: application/json" \
  -d "{
    \"userId\": \"${USER_ID}\",
    \"amount\": 100.00,
    \"notes\": \"Test promotional credits\"
  }")

echo "$RESPONSE" | jq '.'

if echo "$RESPONSE" | jq -e '.success == true' > /dev/null; then
    echo -e "${GREEN}✓ Test 1 Passed${NC}\n"
else
    echo -e "${RED}✗ Test 1 Failed${NC}\n"
fi

# Test 2: Try with invalid amount (should fail)
echo -e "${YELLOW}Test 2: Try adding negative credits (should fail)${NC}"
RESPONSE=$(curl -s -X POST "${API_BASE_URL}/admin/credits/add-approved" \
  -H "Authorization: Bearer ${ADMIN_TOKEN}" \
  -H "Content-Type: application/json" \
  -d "{
    \"userId\": \"${USER_ID}\",
    \"amount\": -50.00
  }")

echo "$RESPONSE" | jq '.'

if echo "$RESPONSE" | jq -e '.success == false' > /dev/null; then
    echo -e "${GREEN}✓ Test 2 Passed (correctly rejected negative amount)${NC}\n"
else
    echo -e "${RED}✗ Test 2 Failed${NC}\n"
fi

# Test 3: Try with invalid user ID (should fail)
echo -e "${YELLOW}Test 3: Try with invalid user ID (should fail)${NC}"
RESPONSE=$(curl -s -X POST "${API_BASE_URL}/admin/credits/add-approved" \
  -H "Authorization: Bearer ${ADMIN_TOKEN}" \
  -H "Content-Type: application/json" \
  -d "{
    \"userId\": \"00000000-0000-0000-0000-000000000000\",
    \"amount\": 100.00
  }")

echo "$RESPONSE" | jq '.'

if echo "$RESPONSE" | jq -e '.success == false' > /dev/null; then
    echo -e "${GREEN}✓ Test 3 Passed (correctly rejected invalid user)${NC}\n"
else
    echo -e "${RED}✗ Test 3 Failed${NC}\n"
fi

# Test 4: Try without authorization (should fail)
echo -e "${YELLOW}Test 4: Try without authorization (should fail)${NC}"
RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "${API_BASE_URL}/admin/credits/add-approved" \
  -H "Content-Type: application/json" \
  -d "{
    \"userId\": \"${USER_ID}\",
    \"amount\": 100.00
  }")

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | head -n-1)

if [ "$HTTP_CODE" = "401" ] || [ "$HTTP_CODE" = "403" ]; then
    echo -e "${GREEN}✓ Test 4 Passed (correctly rejected unauthorized request)${NC}\n"
else
    echo -e "${RED}✗ Test 4 Failed (HTTP $HTTP_CODE)${NC}\n"
fi

echo -e "${YELLOW}=== Tests Complete ===${NC}"

