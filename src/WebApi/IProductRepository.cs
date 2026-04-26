//===========================================================================
// File:        IProductRepository.cs
// Project:     WebApi
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Contract for the product repository consumed by the
//              ProductsController. Provides CRUD-like operations over
//              the in-memory product catalogue.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using WebApi.Models;

namespace WebApi;

/// <summary>
/// Contract for accessing and modifying the product catalogue.
/// </summary>
public interface IProductRepository
{
    /// <summary>Returns all products in the catalogue.</summary>
    /// <returns>An enumeration of all stored products.</returns>
    IEnumerable<Product> GetAll();

    /// <summary>Returns the product with the specified identifier.</summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>The product, or <c>null</c> if no such product exists.</returns>
    Product? GetById(int id);

    /// <summary>Adds a new product to the catalogue, assigning it an identifier.</summary>
    /// <param name="product">The product to add.</param>
    /// <returns>The added product with its assigned identifier.</returns>
    Product Add(Product product);

    /// <summary>Removes the product with the specified identifier.</summary>
    /// <param name="id">The unique identifier of the product to remove.</param>
    /// <returns><c>true</c> if the product was found and removed; otherwise <c>false</c>.</returns>
    bool Delete(int id);

    /// <summary>Returns all products belonging to the specified category.</summary>
    /// <param name="category">The category name to filter by.</param>
    /// <returns>An enumeration of matching products (possibly empty).</returns>
    IEnumerable<Product> GetByCategory(string category);
}