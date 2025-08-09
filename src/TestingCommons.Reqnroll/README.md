# TestingCommons.Reqnroll

The Reqnroll project provides extensions and utilities for behavior-driven development (BDD) testing using Reqnroll (formerly SpecFlow). It includes data table helpers and relative date/time parsing for more expressive test scenarios.

## Key Components

### **DataTableExtensions**
- `GetVerticalTableData()` - Converts Reqnroll DataTable to Dictionary<string, string>
- `GetValueFromVerticalTableByName()` - Extracts specific values from vertical data tables
- Case-insensitive key lookup for flexible test data access

### **RelativeDateValueRetriever**
- Custom value retriever for Reqnroll.Assist that handles relative dates
- Supports natural language date expressions in test scenarios
- Configurable test moment for consistent test execution
- Handles both past and future relative dates

## Usage Examples

### Data Table Extensions

**Feature File Example:**
```gherkin
Scenario: User registration with vertical data table
    Given I have the following user data:
      | Field     | Value              |
      | Name      | John Doe           |
      | Email     | john@example.com   |
      | Age       | 30                 |
      | IsActive  | true               |
    When I register the user
    Then the user should be created successfully
```

**Step Definition:**
```csharp
[Given(@"I have the following user data:")]
public void GivenIHaveTheFollowingUserData(DataTable table)
{
    // Convert to dictionary for easy access
    var userData = table.GetVerticalTableData();
    
    var user = new User
    {
        Name = userData["Name"],
        Email = userData["Email"],
        Age = int.Parse(userData["Age"]),
        IsActive = bool.Parse(userData["IsActive"])
    };
    
    _scenarioContext["User"] = user;
}
```

### Relative Date Value Retriever

**Setup in Test Hooks:**
```csharp
[BeforeTestRun]
public static void BeforeTestRun()
{
    // Set a fixed test moment for consistent date calculations
    RelativeDateValueRetriever.TestMoment = new DateTime(2025, 8, 9, 14, 30, 0);
    
    // Register the custom value retriever
    Service.Instance.ValueRetrievers.Register(new RelativeDateValueRetriever());
}
```

**Feature File with Relative Dates:**
```gherkin
Scenario: Order processing with relative dates
    Given I have an order with the following details:
      | Field        | Value           |
      | OrderDate    | 2 days ago      |
      | DeliveryDate | in 5 days       |
      | CreatedAt    | 1 hour ago      |
      | UpdatedAt    | now             |
      | CancelledAt  | null date       |
    When I process the order
    Then the dates should be calculated correctly
```

### Supported Date Formats

- **Past Dates**: `1 day ago`, `2 weeks ago`, `3 months ago`, `5 years ago`
- **Future Dates**: `in 1 day`, `in 2 weeks`, `in 3 months`, `in 5 years`
- **Current Time**: `now`
- **Null Values**: `null date`
- **Time Units**: `second(s)`, `minute(s)`, `hour(s)`, `day(s)`, `month(s)`, `year(s)`

## Key Features

- **Natural Language Dates**: Express dates in human-readable format in test scenarios
- **Consistent Test Execution**: Fixed test moment ensures reproducible test results
- **Flexible Data Access**: Easy conversion between Reqnroll DataTables and .NET objects
- **Case-Insensitive Lookup**: Robust data table value retrieval
- **Null Date Support**: Proper handling of optional date fields
- **Multiple Time Units**: Support for years, months, days, hours, minutes, seconds
- **Past and Future**: Handle both historical and future relative dates
- **Reqnroll.Assist Integration**: Seamless integration with object creation from tables
