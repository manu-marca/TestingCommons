-- PostgreSQL Schema for TestingCommons Test Data
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
