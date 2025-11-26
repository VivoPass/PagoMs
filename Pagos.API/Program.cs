using dotenv.net;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using log4net;
using log4net.Config;
using MassTransit;
using Microsoft.OpenApi.Models;
using Pagos.API.Controllers;
using Pagos.Application.Commands.CommandHandlers;
using Pagos.Application.Interfaces;
using Pagos.Application.Validations;
using Pagos.Domain.Factories;
using Pagos.Domain.Interfaces;
using Pagos.Infrastructure.Configurations;
using Pagos.Infrastructure.Interfaces;
using Pagos.Infrastructure.Persistences.Repositories.MongoDB;
using Pagos.Infrastructure.Queries.QueryHandlers;
using Pagos.Infrastructure.Services;
using RestSharp;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configurar log4net
XmlConfigurator.Configure(new FileInfo("log4net.config"));
builder.Services.AddSingleton<ILog>(provider => LogManager.GetLogger(typeof(PagosController)));

Env.Load();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pagos API",
        Version = "v1",
        Description = "API del Microservicio de Pagos que gestiona la información de pagos y métodos de pago.",
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.IncludeXmlComments(xmlPath);
});

// Registrar configuración de MongoDB
builder.Services.AddSingleton<PagoDbConfig>();
builder.Services.AddSingleton<AuditoriaDbConfig>();
builder.Services.AddSingleton<IRestClient>(new RestClient());

// REGISTRA EL REPOSITORIO ANTES DE MediatR
builder.Services.AddScoped<IMPagoRepository, MPagoRepository>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<ITarjetaCreditoFactory, TarjetaCreditoFactory>();
builder.Services.AddScoped<IPagoFactory, PagoFactory>();

// REGISTRA MediatR PARA TODOS LOS HANDLERS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AgregarMPagoCommandHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EliminarMPagoCommandHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MPagoPredeterminadoCommandHandler).Assembly));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AgregarPagoCommandHandler).Assembly));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetTodosMPagoQueryHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetMPagoPorIdQueryHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetMPagoPorIdUsuarioQueryHandler).Assembly));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetPagoPorIdQueryHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetPagosByIdEventoQueryHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetPagosByIdUsuarioQueryHandler).Assembly));

builder.Services.AddValidatorsFromAssemblyContaining<AgregarMPagoDTOValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<MPagoDTOValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AgregarPagoDTOValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PagoDTOValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddMassTransit(busConfigurator =>
{
    //busConfigurator.AddConsumer<AgregarMPagoConsumer>();

    busConfigurator.SetKebabCaseEndpointNameFormatter();
    busConfigurator.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(new Uri(Environment.GetEnvironmentVariable("RABBIT_URL")), h =>
        {
            h.Username(Environment.GetEnvironmentVariable("RABBIT_USERNAME"));
            h.Password(Environment.GetEnvironmentVariable("RABBIT_PASSWORD"));
        });

        //configurator.ReceiveEndpoint(Environment.GetEnvironmentVariable("RABBIT_QUEUE_AgregarMPago"), e => {
        //    e.ConfigureConsumer<AgregarMPagoConsumer>(context);
        //});

        configurator.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        configurator.ConfigureEndpoints(context);
    });
});
//EndpointConvention.Map<MPagoPredeterminadoEvent>(new Uri("queue:" + Environment.GetEnvironmentVariable("RABBIT_QUEUE_ActualizarPredeterminadoMPago")));

// Configuración CORS permisiva (¡Solo para desarrollo!)
builder.Services.AddCors(options =>
{
    // Define la política "AllowAll" para desarrollo
    options.AddPolicy("AllowAll",
        builder =>
        {
            // Permite cualquier origen (dominio), método (GET, POST, etc.) y encabezado.
            // Esto es crucial para que el entorno de Canvas (iframe) pueda conectarse a localhost.
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pagos API v1");
    //c.RoutePrefix = string.Empty;
});

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
