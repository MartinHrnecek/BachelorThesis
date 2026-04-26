//===========================================================================
// File:        Product.cs
// Project:     WebApi
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Domain model definitions for the WebApi project: the
//              Product entity, the Order entity and the OrderStatus
//              enumeration. Properties are exposed via auto-implemented
//              accessors so that ASP.NET Core model binding and JSON
//              serialization can populate them from HTTP requests.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

namespace WebApi.Models;

/// <summary>
/// Represents a product in the catalogue, including pricing, stock
/// information and an availability indicator derived from the stock count.
/// </summary>
public class Product
{
    /// <summary>Gets or sets the unique product identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the display name of the product.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the category to which the product belongs.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the unit price of the product.</summary>
    public decimal Price { get; set; }

    /// <summary>Gets or sets the number of units currently in stock.</summary>
    public int Stock { get; set; }

    /// <summary>Gets a value indicating whether the product has any units in stock.</summary>
    public bool IsAvailable => Stock > 0;
}

/// <summary>
/// Represents an order placed for a specific product and quantity.
/// </summary>
public class Order
{
    /// <summary>Gets or sets the unique order identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the identifier of the ordered product.</summary>
    public int ProductId { get; set; }

    /// <summary>Gets or sets the number of units ordered.</summary>
    public int Quantity { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the order was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the current processing status of the order.</summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
}

/// <summary>
/// Enumerates the possible processing states of an <see cref="Order"/>.
/// </summary>
public enum OrderStatus
{
    /// <summary>The order has been created but not yet processed.</summary>
    Pending,

    /// <summary>The order is currently being processed.</summary>
    Processing,

    /// <summary>The order has been successfully completed.</summary>
    Completed,

    /// <summary>The order has been cancelled.</summary>
    Cancelled
}