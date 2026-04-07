using ClubeRank.Application;
using ClubeRank.Api.Extensions;
using ClubeRank.Api.Middlewares;
using ClubeRank.Infrastructure;
using ClubeRank.Infrastructure.Data;
using ClubeRank.Infrastructure.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var clubeRankDbContext = services.GetRequiredService<ClubeRankDbContext>();
    var identityDbContext = services.GetRequiredService<IdentityDbContext>();

    await clubeRankDbContext.Database.MigrateAsync();
    await identityDbContext.Database.EnsureCreatedAsync();
    await IdentityInitializationExtensions.EnsureRolesCreatedAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
