//===========================================================================
// File:        Program.cs
// Project:     WebApi
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Entry point of the WebApi project. Configures the
//              ASP.NET Core host, registers the dependency-injection
//              services for the product catalogue and order processing,
//              and starts the HTTP request pipeline.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using WebApi;

var builder = WebApplication.CreateBuilder(args);

// Register MVC controllers and the application services in the DI container.
// IProductRepository is registered as a singleton because the in-memory
// catalogue must be shared across all HTTP requests, while IOrderService
// is registered per-request (scoped) to mirror typical request-scoped
// transactional semantics.
builder.Services.AddControllers();
builder.Services.AddSingleton<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.MapControllers();

app.Run();