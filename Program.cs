using Microsoft.EntityFrameworkCore;
using TBLApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Добавляем строку подключения
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Обработка статических файлов
app.UseAuthorization();
app.MapControllers();

// Добавляем корневой маршрут
app.MapGet("/", () => "TBLApi is running");

app.Run();
