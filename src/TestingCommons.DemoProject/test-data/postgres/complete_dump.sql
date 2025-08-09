-- Complete PostgreSQL dump for TestingCommons
-- This file combines schema and data for easy import

-- Set connection encoding and other settings
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

-- Create database (uncomment if you want to create a new database)
-- CREATE DATABASE testingcommons_db WITH ENCODING 'UTF8';
-- \c testingcommons_db;

-- Enable UUID extension if not already enabled
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Drop tables if they exist (for clean re-runs)
DROP TABLE IF EXISTS order_items CASCADE;
DROP TABLE IF EXISTS orders CASCADE;
DROP TABLE IF EXISTS product_reviews CASCADE;
DROP TABLE IF EXISTS products CASCADE;
DROP TABLE IF EXISTS employee_projects CASCADE;
DROP TABLE IF EXISTS employees CASCADE;

-- Drop custom types if they exist
DROP TYPE IF EXISTS order_status_enum CASCADE;
DROP TYPE IF EXISTS payment_method_enum CASCADE;
DROP TYPE IF EXISTS payment_status_enum CASCADE;
DROP TYPE IF EXISTS shipping_method_enum CASCADE;

-- Create custom enum types
CREATE TYPE order_status_enum AS ENUM ('processing', 'shipped', 'delivered', 'cancelled');
CREATE TYPE payment_method_enum AS ENUM ('credit_card', 'paypal', 'bank_transfer', 'cash');
CREATE TYPE payment_status_enum AS ENUM ('pending', 'paid', 'failed', 'refunded');
CREATE TYPE shipping_method_enum AS ENUM ('standard', 'express', 'overnight', 'pickup');

-- Employees table
CREATE TABLE employees (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    mongo_id VARCHAR(24) UNIQUE NOT NULL, -- Original MongoDB ObjectId as string
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    age INTEGER CHECK (age > 0 AND age < 120),
    is_active BOOLEAN DEFAULT true,
    department VARCHAR(100),
    salary DECIMAL(10,2),
    join_date TIMESTAMP WITH TIME ZONE,
    street VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(50),
    zip_code VARCHAR(20),
    skills TEXT[], -- PostgreSQL array type
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Employee projects (separate table for normalized design)
CREATE TABLE employee_projects (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_id UUID REFERENCES employees(id) ON DELETE CASCADE,
    project_name VARCHAR(255) NOT NULL,
    role VARCHAR(100),
    start_date TIMESTAMP WITH TIME ZONE,
    end_date TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Products table
CREATE TABLE products (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    mongo_id VARCHAR(24) UNIQUE NOT NULL, -- Original MongoDB ObjectId as string
    product_name VARCHAR(255) NOT NULL,
    category VARCHAR(100),
    price DECIMAL(10,2) NOT NULL CHECK (price >= 0),
    in_stock BOOLEAN DEFAULT true,
    quantity INTEGER DEFAULT 0 CHECK (quantity >= 0),
    description TEXT,
    manufacturer VARCHAR(255),
    release_date TIMESTAMP WITH TIME ZONE,
    tags TEXT[], -- PostgreSQL array type
    -- Specifications as JSONB for flexible schema
    specifications JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Product reviews (separate table for normalized design)
CREATE TABLE product_reviews (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID REFERENCES products(id) ON DELETE CASCADE,
    user_mongo_id VARCHAR(24) NOT NULL, -- Reference to MongoDB employee ID
    rating INTEGER CHECK (rating >= 1 AND rating <= 5),
    comment TEXT,
    review_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Orders table
CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    mongo_id VARCHAR(24) UNIQUE NOT NULL, -- Original MongoDB ObjectId as string
    order_id VARCHAR(50) UNIQUE NOT NULL,
    customer_mongo_id VARCHAR(24) NOT NULL, -- Reference to MongoDB employee ID
    order_date TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    status order_status_enum DEFAULT 'processing',
    total_amount DECIMAL(10,2) NOT NULL CHECK (total_amount >= 0),
    -- Shipping address
    shipping_street VARCHAR(255),
    shipping_city VARCHAR(100),
    shipping_state VARCHAR(50),
    shipping_zip_code VARCHAR(20),
    shipping_country VARCHAR(100) DEFAULT 'USA',
    -- Payment information
    payment_method payment_method_enum,
    payment_status payment_status_enum DEFAULT 'pending',
    transaction_id VARCHAR(255),
    paid_date TIMESTAMP WITH TIME ZONE,
    -- Shipping information
    shipping_method shipping_method_enum DEFAULT 'standard',
    shipping_cost DECIMAL(8,2) DEFAULT 0,
    estimated_delivery TIMESTAMP WITH TIME ZONE,
    actual_delivery TIMESTAMP WITH TIME ZONE,
    tracking_number VARCHAR(255),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Order items (separate table for normalized design)
CREATE TABLE order_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID REFERENCES orders(id) ON DELETE CASCADE,
    product_mongo_id VARCHAR(24) NOT NULL, -- Reference to MongoDB product ID
    product_name VARCHAR(255) NOT NULL, -- Denormalized for convenience
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    unit_price DECIMAL(10,2) NOT NULL CHECK (unit_price >= 0),
    total_price DECIMAL(10,2) NOT NULL CHECK (total_price >= 0),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for better performance
CREATE INDEX idx_employees_email ON employees(email);
CREATE INDEX idx_employees_department ON employees(department);
CREATE INDEX idx_employees_is_active ON employees(is_active);
CREATE INDEX idx_employees_mongo_id ON employees(mongo_id);

CREATE INDEX idx_products_category ON products(category);
CREATE INDEX idx_products_in_stock ON products(in_stock);
CREATE INDEX idx_products_mongo_id ON products(mongo_id);
CREATE INDEX idx_products_price ON products(price);

CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_orders_customer_mongo_id ON orders(customer_mongo_id);
CREATE INDEX idx_orders_order_date ON orders(order_date);
CREATE INDEX idx_orders_mongo_id ON orders(mongo_id);

CREATE INDEX idx_product_reviews_product_id ON product_reviews(product_id);
CREATE INDEX idx_product_reviews_rating ON product_reviews(rating);

CREATE INDEX idx_employee_projects_employee_id ON employee_projects(employee_id);
CREATE INDEX idx_order_items_order_id ON order_items(order_id);

-- Create updated_at trigger function
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create triggers for updated_at
CREATE TRIGGER update_employees_updated_at BEFORE UPDATE ON employees
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_products_updated_at BEFORE UPDATE ON products
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_orders_updated_at BEFORE UPDATE ON orders
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Insert test data

-- Insert employees
INSERT INTO employees (mongo_id, name, email, age, is_active, department, salary, join_date, street, city, state, zip_code, skills) VALUES
('507f1f77bcf86cd799439011', 'John Doe', 'john.doe@example.com', 29, true, 'Engineering', 75000.00, '2023-01-15 09:00:00+00', '123 Main St', 'New York', 'NY', '10001', ARRAY['C#', '.NET', 'MongoDB', 'Azure']),
('507f1f77bcf86cd799439012', 'Jane Smith', 'jane.smith@example.com', 32, true, 'Product', 85000.00, '2022-08-20 09:00:00+00', '456 Oak Ave', 'San Francisco', 'CA', '94102', ARRAY['Product Management', 'Agile', 'SQL', 'Analytics']),
('507f1f77bcf86cd799439013', 'Mike Johnson', 'mike.johnson@example.com', 27, false, 'Engineering', 68000.00, '2023-06-10 09:00:00+00', '789 Pine Rd', 'Austin', 'TX', '73301', ARRAY['JavaScript', 'React', 'Node.js', 'Docker']),
('507f1f77bcf86cd799439014', 'Sarah Wilson', 'sarah.wilson@example.com', 35, true, 'DevOps', 95000.00, '2021-11-05 09:00:00+00', '321 Elm St', 'Seattle', 'WA', '98101', ARRAY['AWS', 'Kubernetes', 'Terraform', 'CI/CD', 'MongoDB']),
('507f1f77bcf86cd799439015', 'David Brown', 'david.brown@example.com', 28, true, 'QA', 62000.00, '2023-03-01 09:00:00+00', '654 Maple Dr', 'Denver', 'CO', '80201', ARRAY['Test Automation', 'Selenium', 'C#', 'API Testing']);

-- Insert employee projects
INSERT INTO employee_projects (employee_id, project_name, role, start_date) VALUES
((SELECT id FROM employees WHERE mongo_id = '507f1f77bcf86cd799439011'), 'TestingCommons', 'Lead Developer', '2023-02-01 00:00:00+00'),
((SELECT id FROM employees WHERE mongo_id = '507f1f77bcf86cd799439012'), 'User Dashboard', 'Product Owner', '2022-09-01 00:00:00+00'),
((SELECT id FROM employees WHERE mongo_id = '507f1f77bcf86cd799439012'), 'API Gateway', 'Stakeholder', '2023-03-15 00:00:00+00'),
((SELECT id FROM employees WHERE mongo_id = '507f1f77bcf86cd799439013'), 'Frontend Redesign', 'Frontend Developer', '2023-07-01 00:00:00+00'),
((SELECT id FROM employees WHERE mongo_id = '507f1f77bcf86cd799439014'), 'Infrastructure Migration', 'DevOps Lead', '2022-01-01 00:00:00+00'),
((SELECT id FROM employees WHERE mongo_id = '507f1f77bcf86cd799439014'), 'Monitoring Setup', 'Technical Lead', '2023-04-01 00:00:00+00'),
((SELECT id FROM employees WHERE mongo_id = '507f1f77bcf86cd799439015'), 'Test Automation Framework', 'QA Engineer', '2023-03-15 00:00:00+00');

-- Insert products
INSERT INTO products (mongo_id, product_name, category, price, in_stock, quantity, description, manufacturer, release_date, tags, specifications) VALUES
('60d5ec49f1b2c72e4c8b4567', 'Wireless Headphones', 'Electronics', 99.99, true, 150, 'High-quality wireless headphones with noise cancellation', 'TechCorp', '2023-01-10 00:00:00+00', ARRAY['wireless', 'audio', 'bluetooth', 'noise-cancelling'], '{"batteryLife": "24 hours", "range": "30 feet", "weight": "250g", "color": "Black"}'),
('60d5ec49f1b2c72e4c8b4568', 'Smart Watch', 'Wearables', 249.99, true, 75, 'Feature-rich smartwatch with health monitoring', 'WearTech', '2023-03-05 00:00:00+00', ARRAY['smartwatch', 'fitness', 'health', 'notifications'], '{"batteryLife": "7 days", "display": "1.4 inch AMOLED", "waterResistant": true, "heartRateMonitor": true}'),
('60d5ec49f1b2c72e4c8b4569', 'Gaming Keyboard', 'Computer Accessories', 129.99, false, 0, 'Mechanical gaming keyboard with RGB lighting', 'GameGear', '2022-11-20 00:00:00+00', ARRAY['gaming', 'mechanical', 'rgb', 'keyboard'], '{"switchType": "Cherry MX Blue", "backlight": "RGB", "connectivity": "USB-C", "keyLayout": "Full Size"}'),
('60d5ec49f1b2c72e4c8b456a', 'Bluetooth Speaker', 'Electronics', 79.99, true, 200, 'Portable Bluetooth speaker with excellent bass', 'SoundWave', '2023-02-28 00:00:00+00', ARRAY['bluetooth', 'portable', 'speaker', 'bass'], '{"batteryLife": "12 hours", "range": "33 feet", "waterproof": "IPX7", "power": "20W"}');

-- Insert product reviews
INSERT INTO product_reviews (product_id, user_mongo_id, rating, comment, review_date) VALUES
((SELECT id FROM products WHERE mongo_id = '60d5ec49f1b2c72e4c8b4567'), '507f1f77bcf86cd799439011', 5, 'Excellent sound quality!', '2023-02-15 10:30:00+00'),
((SELECT id FROM products WHERE mongo_id = '60d5ec49f1b2c72e4c8b4568'), '507f1f77bcf86cd799439012', 4, 'Great features, good battery life', '2023-04-01 14:20:00+00'),
((SELECT id FROM products WHERE mongo_id = '60d5ec49f1b2c72e4c8b456a'), '507f1f77bcf86cd799439014', 5, 'Amazing sound for the price!', '2023-03-20 09:15:00+00'),
((SELECT id FROM products WHERE mongo_id = '60d5ec49f1b2c72e4c8b456a'), '507f1f77bcf86cd799439015', 4, 'Very portable and good quality', '2023-04-10 16:45:00+00');

-- Insert orders
INSERT INTO orders (mongo_id, order_id, customer_mongo_id, order_date, status, total_amount, shipping_street, shipping_city, shipping_state, shipping_zip_code, shipping_country, payment_method, payment_status, transaction_id, paid_date, shipping_method, shipping_cost, estimated_delivery, actual_delivery, tracking_number) VALUES
('64a1b2c3d4e5f6789abc0001', 'ORD-2023-001', '507f1f77bcf86cd799439011', '2023-05-15 14:30:00+00', 'delivered', 179.98, '123 Main St', 'New York', 'NY', '10001', 'USA', 'credit_card', 'paid', 'txn_abc123def456', '2023-05-15 14:32:00+00', 'standard', 9.99, '2023-05-20 00:00:00+00', '2023-05-19 16:45:00+00', '1Z999AA1234567890'),
('64a1b2c3d4e5f6789abc0002', 'ORD-2023-002', '507f1f77bcf86cd799439012', '2023-06-02 10:15:00+00', 'shipped', 259.98, '456 Oak Ave', 'San Francisco', 'CA', '94102', 'USA', 'paypal', 'paid', 'txn_xyz789ghi012', '2023-06-02 10:17:00+00', 'express', 19.99, '2023-06-05 00:00:00+00', NULL, '1Z999BB9876543210'),
('64a1b2c3d4e5f6789abc0003', 'ORD-2023-003', '507f1f77bcf86cd799439015', '2023-06-10 09:00:00+00', 'processing', 89.98, '654 Maple Dr', 'Denver', 'CO', '80201', 'USA', 'credit_card', 'paid', 'txn_mno345pqr678', '2023-06-10 09:02:00+00', 'standard', 9.99, '2023-06-15 00:00:00+00', NULL, NULL);

-- Insert order items
INSERT INTO order_items (order_id, product_mongo_id, product_name, quantity, unit_price, total_price) VALUES
-- Order 1 items
((SELECT id FROM orders WHERE mongo_id = '64a1b2c3d4e5f6789abc0001'), '60d5ec49f1b2c72e4c8b4567', 'Wireless Headphones', 1, 99.99, 99.99),
((SELECT id FROM orders WHERE mongo_id = '64a1b2c3d4e5f6789abc0001'), '60d5ec49f1b2c72e4c8b456a', 'Bluetooth Speaker', 1, 79.99, 79.99),
-- Order 2 items
((SELECT id FROM orders WHERE mongo_id = '64a1b2c3d4e5f6789abc0002'), '60d5ec49f1b2c72e4c8b4568', 'Smart Watch', 1, 249.99, 249.99),
-- Order 3 items
((SELECT id FROM orders WHERE mongo_id = '64a1b2c3d4e5f6789abc0003'), '60d5ec49f1b2c72e4c8b456a', 'Bluetooth Speaker', 1, 79.99, 79.99);

-- Create some useful views for common queries

-- Employee summary view
CREATE VIEW employee_summary AS
SELECT 
    e.id,
    e.mongo_id,
    e.name,
    e.email,
    e.department,
    e.salary,
    e.is_active,
    COUNT(ep.id) as project_count,
    ARRAY_AGG(ep.project_name ORDER BY ep.start_date DESC) FILTER (WHERE ep.project_name IS NOT NULL) as projects
FROM employees e
LEFT JOIN employee_projects ep ON e.id = ep.employee_id
GROUP BY e.id, e.mongo_id, e.name, e.email, e.department, e.salary, e.is_active;

-- Product with reviews view
CREATE VIEW product_with_reviews AS
SELECT 
    p.id,
    p.mongo_id,
    p.product_name,
    p.category,
    p.price,
    p.in_stock,
    p.quantity,
    p.manufacturer,
    COUNT(pr.id) as review_count,
    AVG(pr.rating) as avg_rating,
    ARRAY_AGG(pr.comment ORDER BY pr.review_date DESC) FILTER (WHERE pr.comment IS NOT NULL) as recent_comments
FROM products p
LEFT JOIN product_reviews pr ON p.id = pr.product_id
GROUP BY p.id, p.mongo_id, p.product_name, p.category, p.price, p.in_stock, p.quantity, p.manufacturer;

-- Order summary view
CREATE VIEW order_summary AS
SELECT 
    o.id,
    o.mongo_id,
    o.order_id,
    o.customer_mongo_id,
    o.order_date,
    o.status,
    o.total_amount,
    o.payment_method,
    o.payment_status,
    COUNT(oi.id) as item_count,
    ARRAY_AGG(oi.product_name ORDER BY oi.product_name) as products
FROM orders o
LEFT JOIN order_items oi ON o.id = oi.order_id
GROUP BY o.id, o.mongo_id, o.order_id, o.customer_mongo_id, o.order_date, o.status, o.total_amount, o.payment_method, o.payment_status;

-- Display summary statistics
SELECT 'Data Import Summary' as info;

SELECT 'Employees' as table_name, COUNT(*) as record_count FROM employees
UNION ALL
SELECT 'Employee Projects', COUNT(*) FROM employee_projects
UNION ALL
SELECT 'Products', COUNT(*) FROM products
UNION ALL
SELECT 'Product Reviews', COUNT(*) FROM product_reviews
UNION ALL
SELECT 'Orders', COUNT(*) FROM orders
UNION ALL
SELECT 'Order Items', COUNT(*) FROM order_items;

-- Show some sample data verification queries
SELECT 'Employee Department Summary' as info;
SELECT 
    department, 
    COUNT(*) as employee_count,
    COUNT(*) FILTER (WHERE is_active = true) as active_count,
    AVG(salary) as avg_salary
FROM employees 
GROUP BY department 
ORDER BY department;

SELECT 'Product Category Summary' as info;
SELECT 
    category,
    COUNT(*) as product_count,
    SUM(quantity) as total_inventory,
    ROUND(AVG(price), 2) as avg_price
FROM products 
GROUP BY category 
ORDER BY category;

SELECT 'Order Status Summary' as info;
SELECT 
    status,
    COUNT(*) as order_count,
    ROUND(SUM(total_amount), 2) as total_revenue
FROM orders 
GROUP BY status 
ORDER BY status;
