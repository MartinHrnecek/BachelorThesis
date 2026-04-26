//===========================================================================
// File:        ProductRepository.cs
// Project:     WebApi
// Author:      Martin Hrnecek <xhrnecm00>
// Description: In-memory implementation of IProductRepository seeded
//              with a fixed sample catalogue. Used as the default DI
//              registration for the WebApi project.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using WebApi.Models;

namespace WebApi;

/// <summary>
/// Default in-memory implementation of <see cref="IProductRepository"/>.
/// The catalogue is pre-populated with a fixed set of products so that
/// the API responses are deterministic across test runs.
/// </summary>
public class ProductRepository : IProductRepository
{
    private const string DefaultCategory = "General";

    private readonly List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Laptop",  Category = "Electronics", Price = 999.99m, Stock = 10 },
        new Product { Id = 2, Name = "Mouse",   Category = "Electronics", Price =  29.99m, Stock = 50 },
        new Product { Id = 3, Name = "Desk",    Category = "Furniture",   Price = 299.99m, Stock =  5 },
        new Product { Id = 4, Name = "Chair",   Category = "Furniture",   Price = 199.99m, Stock =  8 },
        new Product { Id = 5, Name = "Monitor", Category = "Electronics", Price = 399.99m, Stock =  0 },
    };

    /// <inheritdoc/>
    public IEnumerable<Product> GetAll() => _products;

    /// <inheritdoc/>
    public Product? GetById(int id)
        => _products.FirstOrDefault(p => p.Id == id);

    /// <inheritdoc/>
    public Product Add(Product product)
    {
        product.Id = _products.Max(p => p.Id) + 1;
        product.Category = string.IsNullOrEmpty(product.Category)
            ? DefaultCategory
            : product.Category;
        _products.Add(product);
        return product;
    }

    /// <inheritdoc/>
    public bool Delete(int id)
    {
        var product = GetById(id);
        if (product == null) return false;
        _products.Remove(product);
        return true;
    }

    /// <inheritdoc/>
    public IEnumerable<Product> GetByCategory(string category)
        => _products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
}