# TestingCommons.Core

The Core project provides fundamental utilities, extensions, and abstractions used across testing scenarios. It includes common operations, date/time utilities, number extensions, and testable date-time providers.

## Key Components

### **Utilities (`Utils` namespace)**

**CommonOperations**
- `GetMaskedIban(string iban)` - Masks IBAN numbers for secure display, showing only the last 4 characters

**DateTimeExtensions**
- `StripMilliseconds(DateTime)` - Removes milliseconds from DateTime for comparison purposes
- `StripTime(DateTime)` - Removes time component, keeping only the date
- `ParseDateTimeWithFormat(string, string)` - Parses DateTime strings with custom formats
- `ShiftTo1StDayOfNextMonth(DateTime)` - Moves date to the first day of the next month
- `ShiftTo1StDayOfCurrentMonth(DateTime)` - Moves date to the first day of current month
- `ShiftToLastDayOfCurrentMonth(DateTime)` - Moves date to the last day of current month

**NumberExtensions**
- `GetNegativeFromPositive(int/decimal/double)` - Converts positive numbers to negative (leaves negatives unchanged)

### **DateTime Provider**

**UtcDateTimeProvider**
- Implements `IDateTimeProvider` for testable date/time operations
- Always returns UTC times to ensure consistency across time zones
- Properties: `Now` (DateTime.UtcNow), `NowOffset` (DateTimeOffset.UtcNow)

## Usage Examples

```csharp
// IBAN Masking
string iban = "DE1234567890123456";
string masked = CommonOperations.GetMaskedIban(iban);
// Result: "**************3456"

// DateTime Extensions
DateTime now = DateTime.Now;
DateTime dateOnly = now.StripTime();           // 2025-08-09 00:00:00
DateTime noMillis = now.StripMilliseconds();   // 2025-08-09 14:30:45
DateTime firstDay = now.ShiftTo1StDayOfCurrentMonth(); // 2025-08-01 14:30:45

// Number Extensions
decimal amount = 100.50m;
decimal negative = amount.GetNegativeFromPositive(); // -100.50

// Testable DateTime Provider
IDateTimeProvider dateProvider = new UtcDateTimeProvider();
DateTime utcNow = dateProvider.Now;
DateTimeOffset utcNowOffset = dateProvider.NowOffset;
```

## Key Features

- **IBAN Security**: Safe display of sensitive financial data
- **Date Manipulation**: Common date operations for testing scenarios
- **Number Utilities**: Consistent handling of positive/negative conversions
- **Testable Time**: Abstracted DateTime provider for unit testing
- **UTC Consistency**: Ensures all times are in UTC to avoid timezone issues
