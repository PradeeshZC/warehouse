#nullable enable
namespace Warehouse.Validators
{
    public static class PlaceholderValidator
    {
        public static bool IsValidString(string? value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}
