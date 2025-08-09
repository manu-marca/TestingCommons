# PostgreSQL Test Data

This directory contains PostgreSQL SQL dump files that mirror the MongoDB test dataset for comprehensive database testing with TestingCommons.

## Files

### complete_dump.sql
A complete PostgreSQL dump that includes:
- Schema creation with proper constraints and indexes
- All test data insertion
- Useful views for common queries
- Summary statistics and verification queries

### schema.sql
Database schema definition including:
- Custom enum types for order status, payment methods, etc.
- Normalized table structure with proper foreign keys
- Indexes for optimal query performance
- Triggers for automatic timestamp updates

### data.sql
Test data insertion statements with:
- 5 employees with complete profile information
- 4 products across different categories
- 3 orders with various statuses
- Product reviews and employee project assignments

## Database Design

### Key Features
- **Normalized Design**: Proper relational structure with foreign keys
- **UUID Primary Keys**: Using PostgreSQL's gen_random_uuid() for unique identifiers
- **MongoDB Compatibility**: Preserves original MongoDB ObjectIds for reference
- **Modern PostgreSQL**: Uses JSONB for flexible specifications, arrays for tags/skills
- **Data Integrity**: Check constraints, foreign keys, and proper data types
- **Performance**: Strategic indexes on commonly queried columns

### Tables
- `employees` - Employee information with address and skills
- `employee_projects` - Project assignments (normalized many-to-many)
- `products` - Product catalog with JSONB specifications
- `product_reviews` - Customer reviews (normalized one-to-many)
- `orders` - Order headers with payment and shipping info
- `order_items` - Order line items (normalized one-to-many)

### Views
- `employee_summary` - Employees with aggregated project counts
- `product_with_reviews` - Products with review statistics
- `order_summary` - Orders with item counts and product lists

## Usage

### Import into PostgreSQL

```bash
# Create database
createdb testingcommons_db

# Import complete dump
psql -d testingcommons_db -f complete_dump.sql

# Or import schema and data separately
psql -d testingcommons_db -f schema.sql
psql -d testingcommons_db -f data.sql
```

### Using with .NET/Entity Framework

```csharp
// Example Entity Framework models
public class Employee
{
    public Guid Id { get; set; }
    public string MongoId { get; set; } // For MongoDB compatibility
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public string Department { get; set; }
    public decimal Salary { get; set; }
    public DateTime JoinDate { get; set; }
    public string[] Skills { get; set; } // PostgreSQL array
    
    // Navigation properties
    public List<EmployeeProject> Projects { get; set; }
}

public class Product
{
    public Guid Id { get; set; }
    public string MongoId { get; set; }
    public string ProductName { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public bool InStock { get; set; }
    public int Quantity { get; set; }
    public string[] Tags { get; set; } // PostgreSQL array
    public JsonDocument Specifications { get; set; } // JSONB
    
    // Navigation properties
    public List<ProductReview> Reviews { get; set; }
}
```

### Sample Queries

```sql
-- Find employees by department with project counts
SELECT * FROM employee_summary WHERE department = 'Engineering';

-- Get products with high ratings
SELECT * FROM product_with_reviews WHERE avg_rating >= 4.5;

-- Find orders by status with details
SELECT * FROM order_summary WHERE status = 'delivered';

-- Complex join query: Orders with customer and product details
SELECT 
    o.order_id,
    e.name as customer_name,
    e.email as customer_email,
    o.total_amount,
    o.status,
    STRING_AGG(p.product_name, ', ') as ordered_products
FROM orders o
JOIN employees e ON o.customer_mongo_id = e.mongo_id
JOIN order_items oi ON o.id = oi.order_id
JOIN products p ON oi.product_mongo_id = p.mongo_id
GROUP BY o.id, o.order_id, e.name, e.email, o.total_amount, o.status
ORDER BY o.order_date DESC;

-- PostgreSQL-specific array and JSONB queries
SELECT name, skills FROM employees WHERE 'C#' = ANY(skills);
SELECT product_name, specifications->'batteryLife' as battery 
FROM products WHERE specifications ? 'batteryLife';
```

## Docker Setup

You can use this with Docker Compose for testing:

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: testingcommons_db
      POSTGRES_USER: testuser
      POSTGRES_PASSWORD: testpass
    ports:
      - "5432:5432"
    volumes:
      - ./complete_dump.sql:/docker-entrypoint-initdb.d/init.sql
```

## Testing Scenarios

This dataset supports testing:
- **CRUD Operations**: Create, read, update, delete on all entities
- **Complex Queries**: Joins across multiple tables with aggregations
- **PostgreSQL Features**: Array operations, JSONB queries, full-text search
- **Data Relationships**: Foreign key constraints and referential integrity
- **Performance**: Index usage and query optimization
- **Transactions**: Multi-table operations with rollback scenarios

## Compatibility Notes

- **MongoDB Mapping**: Original MongoDB ObjectIds preserved for cross-database testing
- **Date/Time**: Uses PostgreSQL's TIMESTAMP WITH TIME ZONE for proper timezone handling
- **JSON Data**: Product specifications stored as JSONB for flexible querying
- **Arrays**: Skills and tags use PostgreSQL native array types
- **Enums**: Order status and payment methods use PostgreSQL custom enums

This PostgreSQL dataset provides the same logical data as the MongoDB version while leveraging PostgreSQL's relational strengths and advanced features.
