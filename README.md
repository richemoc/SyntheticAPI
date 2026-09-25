# SyntheticApi

A synthetic .NET 8 Web API demonstrating clean architecture patterns with Products and Orders.

## Tech Stack

- **.NET 8** — ASP.NET Core Web API
- **Entity Framework Core 8** — ORM with SQLite (configure another relational provider for production)
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
dotnet run --environment Development
# Open: https://localhost:5001/swagger
```

Set `Jwt:Key` through an environment variable or another secure configuration provider before starting the API. The default database connection is `ConnectionStrings:Default`; override it for deployment rather than committing credentials.

Run the security regression tests with:

```bash
dotnet test SyntheticAPI.Tests/SyntheticAPI.Tests.csproj
```

## API Endpoints

### Products
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/products` | List active products (filter by `?category=`, `?page=`, `?pageSize=`) |
| GET | `/api/products/{id}` | Get by ID |
| POST | `/api/products` | Create product |
| PATCH | `/api/products/{id}` | Partial update |
| DELETE | `/api/products/{id}` | Soft delete |
| POST | `/api/products/{id}/restore` | Restore (admin only) |

### Orders
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/orders` | List orders (use `?page=` and `?pageSize=`) |
| GET | `/api/orders/{id}` | Get by ID |
| POST | `/api/orders` | Place order (validates stock) |
| PATCH | `/api/orders/{id}/status` | Update status (admin only) |

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
# SyntheticAPI
