namespace MiniShop.ApplicationCore.Entities;

public class Brand
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Brand()
    {
        
    }

    public Brand(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Brand name is required.", nameof(name));

        Name = name;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Brand name is required.", nameof(newName));

        Name = newName;
    }
}