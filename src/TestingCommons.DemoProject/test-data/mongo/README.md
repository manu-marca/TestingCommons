# MongoDB Test Data

This directory contains sample JSON files that can be used for testing MongoDB functionality with the TestingCommons.MongoDb package.

## Files

### employees.json
Contains sample employee data with the following structure:
- Employee information (name, email, age, department, salary)
- Address details
- Skills array
- Projects with roles and dates
- Mix of active and inactive employees

### products.json  
Contains sample product catalog data:
- Product details (name, category, price, stock status)
- Manufacturer information
- Technical specifications
- Customer reviews
- Various product categories (Electronics, Wearables, Computer Accessories)

### orders.json
Contains sample order data:
- Order information with customer references
- Multiple order statuses (delivered, shipped, processing)
- Item details with pricing
- Payment information
- Shipping details with tracking

## Usage

### Importing into MongoDB

You can import these files into MongoDB using `mongoimport`:

```bash
# Import employees collection
mongoimport --db testdb --collection employees --file employees.json --jsonArray

# Import products collection  
mongoimport --db testdb --collection products --file products.json --jsonArray

# Import orders collection
mongoimport --db testdb --collection orders --file orders.json --jsonArray
```

### Using with TestingCommons.MongoDb

These files are perfect for testing the MongoDB client functionality:

```csharp
// Example usage with TestingCommons.MongoDb
var mongoClient = new MongoDbClientBase(connectionString, databaseName);

// Test querying employees
var activeEmployees = await mongoClient.GetCollectionAsync<Employee>("employees")
    .Find(e => e.IsActive == true)
    .ToListAsync();

// Test product searches
var electronicsProducts = await mongoClient.GetCollectionAsync<Product>("products")
    .Find(p => p.Category == "Electronics")
    .ToListAsync();

// Test order queries
var shippedOrders = await mongoClient.GetCollectionAsync<Order>("orders")
    .Find(o => o.Status == "shipped")
    .ToListAsync();
```

## Data Relationships

The test data includes logical relationships:
- Orders reference customers by `customerId` (matching employee `_id`)
- Orders reference products by `productId` (matching product `_id`)
- Reviews in products reference users by `userId` (matching employee `_id`)

This allows for testing complex queries and aggregation scenarios.

## Notes

- All dates use MongoDB's `$date` format for proper DateTime handling
- ObjectIds use the `$oid` format for proper ObjectId handling
- Data includes various scenarios: active/inactive records, in-stock/out-of-stock products, different order statuses
- Realistic data volumes suitable for unit testing and integration testing
