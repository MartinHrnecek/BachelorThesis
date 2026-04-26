//===========================================================================
// File:        IOrderService.cs
// Project:     WebApi
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Order-service contract and a generic Result record used
//              to convey success or failure of order operations without
//              exception-based error propagation.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using WebApi.Models;

namespace WebApi;

/// <summary>
/// Contract for the order-processing service consumed by the
/// <see cref="Controllers.ProductsController"/>.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Creates a new order for the specified product and quantity.
    /// </summary>
    /// <param name="productId">The identifier of the product to order.</param>
    /// <param name="quantity">The number of units to order.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the created order,
    /// or a failed result describing why the order could not be created.
    /// </returns>
    Result<Order> CreateOrder(int productId, int quantity);

    /// <summary>Returns all orders currently known to the service.</summary>
    /// <returns>An enumeration of all stored orders.</returns>
    IEnumerable<Order> GetOrders();
}

/// <summary>
/// Generic discriminated result type wrapping either a successful value
/// or an error message.
/// </summary>
/// <typeparam name="T">The type of the wrapped success value.</typeparam>
/// <param name="Success">Indicates whether the operation succeeded.</param>
/// <param name="Value">The wrapped value on success; otherwise <c>default</c>.</param>
/// <param name="Error">The error description on failure; otherwise <c>null</c>.</param>
public record Result<T>(bool Success, T? Value, string? Error)
{
    /// <summary>Creates a successful result wrapping the specified value.</summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A successful <see cref="Result{T}"/> containing <paramref name="value"/>.</returns>
    public static Result<T> Ok(T value) => new(true, value, null);

    /// <summary>Creates a failed result with the specified error message.</summary>
    /// <param name="error">A description of the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying <paramref name="error"/>.</returns>
    public static Result<T> Fail(string error) => new(false, default, error);
}