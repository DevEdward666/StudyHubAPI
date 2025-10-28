#!/bin/bash

# Promo System Migration Script
# This script creates the database migration for the promo system

echo "Creating migration for Promo System..."

cd "$(dirname "$0")/Study-Hub"

# Create the migration
dotnet ef migrations add AddPromoSystem --context ApplicationDbContext

echo ""
echo "Migration created successfully!"
echo ""
echo "To apply the migration, run:"
echo "  cd Study-Hub"
echo "  dotnet ef database update"

