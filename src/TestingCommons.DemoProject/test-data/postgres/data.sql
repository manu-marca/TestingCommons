-- Insert test data for PostgreSQL TestingCommons database

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

-- Verify data insertion with some sample queries
-- Show employee count by department
SELECT 
    department, 
    COUNT(*) as employee_count,
    COUNT(*) FILTER (WHERE is_active = true) as active_count
FROM employees 
GROUP BY department 
ORDER BY department;

-- Show product inventory summary
SELECT 
    category,
    COUNT(*) as product_count,
    SUM(quantity) as total_inventory,
    AVG(price) as avg_price
FROM products 
GROUP BY category 
ORDER BY category;

-- Show order summary by status
SELECT 
    status,
    COUNT(*) as order_count,
    SUM(total_amount) as total_revenue
FROM orders 
GROUP BY status 
ORDER BY status;
