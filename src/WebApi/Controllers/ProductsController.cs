//===========================================================================
// File:        ProductsController.cs
// Project:     WebApi
// Author:      Martin Hrnecek <xhrnecm00>
// Description: ASP.NET Core controller exposing the product catalogue
//              and order-creation endpoints. Demonstrates dependency
//              injection, attribute-based routing and HTTP verb mapping
//              whose framework metadata must be preserved by the
//              tested obfuscators.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers;

/// <summary>
/// REST controller exposing CRUD-like operations over the product
/// catalogue together with an order-creation endpoint.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly IOrderService _orderService;

    /// <summary>
    /// Initializes a new <see cref="ProductsController"/> with its
    /// dependencies provided by the DI container.
    /// </summary>
    /// <param name="repository">The product repository.</param>
    /// <param name="orderService">The order-processing service.</param>
    public ProductsController(IProductRepository repository, IOrderService orderService)
    {
        _repository = repository;
        _orderService = orderService;
    }

    /// <summary>Returns all products in the catalogue.</summary>
    /// <returns><c>200 OK</c> with the full product list.</returns>
    [HttpGet]
    public IActionResult GetAll() => Ok(_repository.GetAll());

    /// <summary>Returns the product with the specified identifier.</summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns><c>200 OK</c> with the product, or <c>404 Not Found</c> if no such product exists.</returns>
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var product = _repository.GetById(id);
        return product == null ? NotFound($"Product {id} not found.") : Ok(product);
    }

    /// <summary>Returns all products belonging to the specified category.</summary>
    /// <param name="category">The category name to filter by.</param>
    /// <returns><c>200 OK</c> with the matching products (possibly empty).</returns>
    [HttpGet("category/{category}")]
    public IActionResult GetByCategory(string category)
        => Ok(_repository.GetByCategory(category));

    /// <summary>Creates a new product in the catalogue.</summary>
    /// <param name="product">The product payload to create.</param>
    /// <returns>
    /// <c>201 Created</c> with the new product on success, or
    /// <c>400 Bad Request</c> if the name is missing.
    /// </returns>
    [HttpPost]
    public IActionResult Create([FromBody] Product product)
    {
        if (string.IsNullOrEmpty(product.Name))
            return BadRequest("Product name is required.");

        var created = _repository.Add(product);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Deletes the product with the specified identifier.</summary>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <returns><c>204 No Content</c> on success, or <c>404 Not Found</c> if no such product exists.</returns>
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var deleted = _repository.Delete(id);
        return deleted ? NoContent() : NotFound($"Product {id} not found.");
    }

    /// <summary>
    /// Creates a new order for the specified product and quantity.
    /// </summary>
    /// <param name="id">The identifier of the product to order.</param>
    /// <param name="quantity">The number of units to order (defaults to 1).</param>
    /// <returns>
    /// <c>200 OK</c> with the created order on success, or
    /// <c>400 Bad Request</c> with an error description on failure.
    /// </returns>
    [HttpPost("{id}/order")]
    public IActionResult CreateOrder(int id, [FromQuery] int quantity = 1)
    {
        var result = _orderService.CreateOrder(id, quantity);
        return result.Success ? Ok(result.Value) : BadRequest(result.Error);
    }
}