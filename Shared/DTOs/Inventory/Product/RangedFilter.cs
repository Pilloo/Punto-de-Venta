namespace DTOs.Inventory.Product;

public class RangeFilter<T>
{
    public T? Min { get; set; }
    public T? Max { get; set; }
}