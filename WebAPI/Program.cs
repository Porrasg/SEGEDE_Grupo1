using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.WebAPI.BackgroundServices;
using SEGEDE_Grupo1.WebAPI.Middleware;
using SEGEDE_Grupo1.CoreApp.Managers;

var builder = WebApplication.CreateBuilder(args);

// Configuración de cadena de conexión a base de datos relacional para ADO.NET sin ORM (§11.1).
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=tcp:segede-sql-server.database.windows.net,1433;Initial Catalog=SEGEDE_DB;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
SqlDao.Configure(connectionString);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuración de CORS para permitir peticiones AJAX desde la WebApp y otros clientes locales.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Registro de servicios en segundo plano para simulación de energía, notificaciones y auditoría WORM.
builder.Services.AddHostedService<EnergySimulationJob>();
builder.Services.AddHostedService<NotificationProcessingJob>();
builder.Services.AddHostedService<AuditArchiveJob>();

var app = builder.Build();

// Intercepción global de excepciones para convertir errores de negocio en respuestas HTTP estandarizadas.
app.UseExceptionHandlingMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    try
    {
        new UserManager().SeedDevUsers();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DEV SEED ERROR] {ex.Message}");
    }
}

// Redirección automática de la ruta raíz hacia la interfaz visual Swagger para facilitar inspección en el navegador.
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
