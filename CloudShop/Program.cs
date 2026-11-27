using Microsoft.EntityFrameworkCore;
using CloudShop.Data;
using CloudShop.Models;

var builder = WebApplication.CreateBuilder(args);

// Считываем строку подключения к БД из переменных окружения контейнера
var connString = builder.Configuration.GetConnectionString("DefaultConnection");

// Гибридная архитектура:
// 1. Если приложение запущено в связке Docker Compose — подключаем реальную БД PostgreSQL
if (!string.IsNullOrEmpty(connString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connString));
}
else
{
    // 2. Если контейнер запущен автономно без внешней БД — используем временный список в оперативной памяти
    builder.Services.AddSingleton<List<Product>>(new List<Product>
    {
        new Product { Id = 1, Name = "Игровой ноутбук (демо)", Price = 85000 },
        new Product { Id = 2, Name = "Беспроводная мышь (демо)", Price = 2500 }
    });
}

var app = builder.Build();

// При запуске с PostgreSQL автоматически создаём базу и таблицу Products, если их ещё нет
if (!string.IsNullOrEmpty(connString))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Включаем раздачу статических файлов (нашего index.html из папки wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

// REST API (GET): Возвращает список всех товаров в формате JSON
app.MapGet("/api/products", async (IServiceProvider sp) =>
{
    var db = sp.GetService<AppDbContext>();
    if (db != null) return Results.Ok(await db.Products.ToListAsync());
    return Results.Ok(sp.GetRequiredService<List<Product>>());
});

// REST API (POST): Принимает новый товар от фронтенда и сохраняет его
app.MapPost("/api/products", async (Product product, IServiceProvider sp) =>
{
    var db = sp.GetService<AppDbContext>();
    if (db != null)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return Results.Ok(product);
    }
    var list = sp.GetRequiredService<List<Product>>();
    product.Id = list.Count + 1;
    list.Add(product);
    return Results.Ok(product);
});

app.Run();