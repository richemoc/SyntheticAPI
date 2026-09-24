# SyntheticApi

A synthetic .NET 8 Web API demonstrating clean architecture patterns with Products and Orders.

## Tech Stack

- **.NET 8** — ASP.NET Core Web API
- **Entity Framework Core 8** — ORM with In-Memory provider (swap for SQL Server/Postgres easily)
- **Swagger / OpenAPI** — auto-generated docs at `/swagger`

## Project Structure

```
SyntheticApi/
├── Controllers/
│   ├── ProductsController.cs   # CRUD for products
│   └── OrdersController.cs     # Order placement & status
├── Models/
│   ├── Product.cs
│   └── Order.cs                # Order + OrderItem entities
├── DTOs/
│   ├── ProductDtos.cs          # Create / Update / Read records
│   └── OrderDtos.cs
├── Services/
│   ├── ProductService.cs       # Business logic + interface
│   └── OrderService.cs         # Stock validation, order creation
├── Data/
│   └── AppDbContext.cs         # EF Core context + seed data
├── Program.cs
└── SyntheticApi.csproj
```

## Running

```bash
dotnet restore
dotnet run
# Open: https://localhost:5001/swagger
```

## API Endpoints

### Products
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/products` | List all (filter by `?category=`) |
| GET | `/api/products/{id}` | Get by ID |
| POST | `/api/products` | Create product |
| PATCH | `/api/products/{id}` | Partial update |
| DELETE | `/api/products/{id}` | Soft delete |

### Orders
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/orders` | List all orders |
| GET | `/api/orders/{id}` | Get by ID |
| POST | `/api/orders` | Place order (validates stock) |
| PATCH | `/api/orders/{id}/status` | Update status |

## Sample Payloads

**Create Order:**
```json
{
  "customerName": "Jane Doe",
  "customerEmail": "jane@example.com",
  "items": [
    { "productId": 1, "quantity": 1 },
    { "productId": 2, "quantity": 2 }
  ]
}
```

**Update Product:**
```json
{
  "price": 1199.99,
  "stock": 45
}
```
