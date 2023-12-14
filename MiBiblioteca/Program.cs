using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MiLibreria.Filters;
using MiLibreria.Hubs;
using MiLibreria.Middlewares;
using MiLibreria.Models;
using MiLibreria.Services;
using System.Drawing;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

#region SERVICES
builder.Services.AddHttpContextAccessor();
// Nuestro servicio de gesti�n de Archivos GestorArchivosLocal es un servicio que debemos incluir en el Program para que lo use
// cualquier controlador
builder.Services.AddTransient<GestorArchivosLocal>();
// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers(options =>
{
    // Integramos el filtro de excepci�n para todos los controladores
    options.Filters.Add(typeof(FiltroDeExcepcion));
}).AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
// Capturamos del app.settings la cadena de conexi�n a la base de datos
// Configuration.GetConnectionString va directamente a la propiedad ConnectionStrings y de ah� tomamos el valor de DefaultConnection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Nuestros servicios resolver�n dependencias de otras clases
// Registramos en el sistema de inyecci�n de dependencias de la aplicaci�n el ApplicationDbContext
// Conseguimos una instancia o configuraci�n global de la base de datos para todo el proyecto
builder.Services.AddDbContext<MiLibreriaContext>(options =>
{
    options.UseSqlServer(connectionString);
    // Esta opci�n deshabilita el tracking a nivel de proyecto (NoTracking).
    // Por defecto siempre hace el tracking. Con esta configuraci�n, no.
    // En cada operaci�n de modificaci�n de datos en los controladores, deberemos habilitar el tracking en cada operaci�n
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}
);

builder.Services.AddDbContext<MiLibreriaContext>(options => options.UseSqlServer(connectionString));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<OperacionesService>();
builder.Services.AddTransient<IGestorArchivos, GestorArchivosLocal>();
builder.Services.AddTransient<HashService>();
builder.Services.AddTransient<TokenService>();
builder.Services.AddDataProtection();
builder.Services.AddTransient<HashService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = false,
                   ValidateAudience = false,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(
                     Encoding.UTF8.GetBytes(builder.Configuration["ClaveJWT"]))
               });
//Este m�todo est� dise�ado espec�ficamente para servicios hospedados, como los que implementan la interfaz IHostedService.
//Crea una instancia del servicio cuando la aplicaci�n se inicia y la destruye cuando la aplicaci�n se detiene. Es decir, sigue el ciclo de vida del servicio hospedado.
//Adecuado para tareas de fondo y servicios que deben ejecutarse durante la duraci�n de la aplicaci�n.
builder.Services.AddHostedService<TareaProgramadaService>();

//AddSingleton<T>:
//Este m�todo crea una �nica instancia del servicio y la reutiliza en toda la aplicaci�n.
//La instancia se crea cuando se solicita por primera vez y se mantiene viva durante toda la vida de la aplicaci�n.
//Adecuado para servicios que deben compartirse entre diferentes partes de la aplicaci�n y que no necesitan ser recreados.
//builder.Services.AddSingleton<TareaProgramadaService>();
builder.Services.AddControllers(options =>
{
    // Integramos el filtro de excepci�n para todos los controladores
    options.Filters.Add<FiltroDeExcepcion>();
}).AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        //builder.WithOrigins("https://www.apirequest.io").AllowAnyMethod().AllowAnyHeader();
         builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
#endregion

var app = builder.Build();

#region MiMiddelware

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<LogFileIPMiddleware>();
app.UseHttpsRedirection();
app.UseCors();
app.UseFileServer();
app.UseStaticFiles();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chatHub"); // El acceso al hub ser�a v�a https://localhost:puerto/chatHub
});
app.UseAuthorization();
#endregion

app.MapControllers();

app.Run();
