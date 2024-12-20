using Microsoft.EntityFrameworkCore;
using TBLApi.Data;
using TBLApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ��������� ��������� ������������
builder.Services.AddControllers();

// ��������� Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ������ ���������� SMTP �� ������������
var smtpSettings = builder.Configuration.GetSection("SmtpSettings");
builder.Services.AddSingleton<IEmailSender>(provider =>
    new SmtpEmailService(
        smtpSettings["Server"],
        int.Parse(smtpSettings["Port"]),
        smtpSettings["User"],
        smtpSettings["Password"]
    ));

// ����������� � ���� ������
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ��������� CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.Use(async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR]: {ex.Message}");
            throw;
        }
    });

}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.Run();
