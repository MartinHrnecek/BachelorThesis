//===========================================================================
// File:        OrderService.cs
// Project:     WebApi
// Author:      Martin Hrnecek <xhrnecm00>
// Description: In-memory order-processing service that validates product
//              availability and stock levels before creating an order.
//              Returns operation outcomes via the Result type rather
//              than by throwing exceptions.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using WebApi.Models;

namespace WebApi;

/// <summary>
/// Default in-memory implementation of <see cref="IOrderService"/>.
/// Validates each order request against the product catalogue and stores
/// successfully created orders in an internal list.
/// </summary>
public class OrderService : IOrderService
{
    private const string OutOfStockError = "Product is out of stock.";
    private const string NotFoundError = "Product not found.";
    private const string InsufficientStockError = "Insufficient stock for requested quantity.";

    private readonly IProductRepository _repository;
    private readonly List<Order> _orders = new();

    /// <summary>
    /// Initializes a new <see cref="OrderService"/> with its product
    /// repository dependency.
    /// </summary>
    /// <param name="repository">The product repository used for stock checks.</param>
    public OrderService(IProductRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Validates the request and creates a new order if the product
    /// exists and has sufficient stock.
    /// </summary>
    /// <param name="productId">The identifier of the product to order.</param>
    /// <param name="quantity">The number of units to order.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> wrapping the created order,
    /// or a failed result describing the validation error.
    /// </returns>
    public Result<Order> CreateOrder(int productId, int quantity)
    {
        var product = _repository.GetById(productId);
        if (product == null)
            return Result<Order>.Fail(NotFoundError);
        if (!product.IsAvailable)
            return Result<Order>.Fail(OutOfStockError);
        if (product.Stock < quantity)
            return Result<Order>.Fail(InsufficientStockError);

        var order = new Order
        {
            Id = _orders.Count + 1,
            ProductId = productId,
            Quantity = quantity,
            Status = OrderStatus.Processing
        };
        _orders.Add(order);
        return Result<Order>.Ok(order);
    }

    /// <inheritdoc/>
    public IEnumerable<Order> GetOrders() => _orders;
}