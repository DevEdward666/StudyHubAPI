#!/bin/bash

# Subscription Package Seed Script
# This script creates initial subscription packages in the database

echo "🌱 Seeding Subscription Packages..."

# Set your database connection details
DB_HOST="localhost"
DB_PORT="5432"
DB_NAME="studyhub"
DB_USER="postgres"

# Generate UUIDs for packages
PACKAGE1_ID=$(uuidgen)
PACKAGE2_ID=$(uuidgen)
PACKAGE3_ID=$(uuidgen)
PACKAGE4_ID=$(uuidgen)
PACKAGE5_ID=$(uuidgen)

# SQL to insert packages
SQL=$(cat <<EOF
-- Clear existing packages (optional, comment out if you want to keep existing)
-- TRUNCATE TABLE subscription_packages CASCADE;

-- Insert Subscription Packages
INSERT INTO subscription_packages 
  (id, name, package_type, duration_value, total_hours, price, description, is_active, display_order, created_at, updated_at)
VALUES 
  -- Hourly Package
  (
    '$PACKAGE1_ID',
    '10 Hours Starter',
    'Hourly',
    10,
    10.00,
    500.00,
    'Perfect for occasional study sessions and trying out our service',
    true,
    1,
    NOW(),
    NOW()
  ),
  
  -- Daily Package
  (
    '$PACKAGE2_ID',
    '1 Day Pass',
    'Daily',
    1,
    24.00,
    1000.00,
    'Full day access - perfect for exam preparation',
    true,
    2,
    NOW(),
    NOW()
  ),
  
  -- Weekly Package (Most Popular)
  (
    '$PACKAGE3_ID',
    '1 Week Unlimited',
    'Weekly',
    1,
    168.00,
    5000.00,
    '⭐ Most Popular! One week of study time - great value',
    true,
    3,
    NOW(),
    NOW()
  ),
  
  -- Monthly Package (Best Value)
  (
    '$PACKAGE4_ID',
    '1 Month Premium',
    'Monthly',
    1,
    720.00,
    15000.00,
    '💎 Best Value! One month of unlimited access for serious students',
    true,
    4,
    NOW(),
    NOW()
  ),
  
  -- Extended Package
  (
    '$PACKAGE5_ID',
    '3 Months Elite',
    'Monthly',
    3,
    2160.00,
    40000.00,
    '👑 Elite Package! Three months with 10% discount - perfect for long-term projects',
    true,
    5,
    NOW(),
    NOW()
  )

ON CONFLICT (id) DO NOTHING;

-- Verify insertion
SELECT 
  name, 
  package_type, 
  total_hours, 
  price,
  is_active
FROM subscription_packages
ORDER BY display_order;
EOF
)

# Execute SQL
echo "Connecting to database: $DB_NAME@$DB_HOST:$DB_PORT"
echo "$SQL" | PGPASSWORD=${DB_PASSWORD:-postgres} psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME

if [ $? -eq 0 ]; then
    echo "✅ Subscription packages seeded successfully!"
    echo ""
    echo "📦 Created Packages:"
    echo "  1. 10 Hours Starter - ₱500"
    echo "  2. 1 Day Pass - ₱1,000"
    echo "  3. 1 Week Unlimited - ₱5,000 ⭐"
    echo "  4. 1 Month Premium - ₱15,000 💎"
    echo "  5. 3 Months Elite - ₱40,000 👑"
else
    echo "❌ Error seeding packages. Please check your database connection."
    exit 1
fi

