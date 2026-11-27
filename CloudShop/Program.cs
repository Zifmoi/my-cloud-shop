using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR; // Подключаем пространство имён SignalR
using CloudShop.Data;
using CloudShop.Models;
using CloudShop.Hubs;              // Подключаем наш созданный хаб

var builder = WebApplication.CreateBuilder(args);

// 1. Регистрируем службу SignalR в контейнере зависимостей
builder.Services.AddSignalR();

// Считываем строку подключения к PostgreSQL
var connString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connString));
}
else
{
    builder.Services.AddSingleton<List<Product>>(new List<Product>
    {
        new Product { Id = 1, Name = "Игровой ноутбук (демо)", Price = 85000 }
    });
}

var app = builder.Build();

if (!string.IsNullOrEmpty(connString))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseDefaultFiles();
app.UseStaticFiles();

// 2. Открываем точку подключения WebSockets по адресу /shophub
app.MapHub<ShopHub>("/shophub");

// REST API (GET): Получить список всех товаров
app.MapGet("/api/products", async (IServiceProvider sp) =>
{
    var db = sp.GetService<AppDbContext>();
    if (db != null) return Results.Ok(await db.Products.ToListAsync());
    return Results.Ok(sp.GetRequiredService<List<Product>>());
});

// REST API (POST): Добавить товар и МГНОВЕННО разослать его всем клиентам через SignalR
app.MapPost("/api/products", async (Product product, IServiceProvider sp, IHubContext<ShopHub> hub) =>
{
    var db = sp.GetService<AppDbContext>();
    if (db != null)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync();
    }
    else
    {
        var list = sp.GetRequiredService<List<Product>>();
        product.Id = list.Count + 1;
        list.Add(product);
    }

    // 3. Уведомляем клиентов после сохранения товара.
    // Сервер сам отправляет объект нового товара всем открытым вкладкам браузера!
    await hub.Clients.All.SendAsync("ProductAdded", product);

    return Results.Ok(product);
});

app.Run();