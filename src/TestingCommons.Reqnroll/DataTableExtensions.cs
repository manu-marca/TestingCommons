using Reqnroll;
namespace TestingCommons.Reqnroll;

public static class DataTableExtensions
{
    public static Dictionary<string, string> GetVerticalTableData(this DataTable table)
    {
        var result = new Dictionary<string, string>(table.Rows.Count);
        foreach (var row in table.Rows)
        {
            var nameValue = row.Values.ToArray();
            result.Add(nameValue[0], nameValue[1]);
        }
        return result;
    }

    public static string? GetValueFromVerticalTableByName(this DataTable table, string name)
    {
        return (from field in table.GetVerticalTableData()
                where field.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                select field.Value)
            .FirstOrDefault();
    }
}
