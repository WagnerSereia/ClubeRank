using ClubeRank.Application;
using ClubeRank.Api.Extensions;
using ClubeRank.Api.Middlewares;
using ClubeRank.Infrastructure;
using ClubeRank.Infrastructure.Data;
using ClubeRank.Infrastructure.Identity.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerUI;
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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("acesso", new OpenApiInfo
    {
        Title = "ClubeRank API - Acesso",
        Version = "v1"
    });
    options.SwaggerDoc("organizacoes", new OpenApiInfo
    {
        Title = "ClubeRank API - Organizacoes",
        Version = "v1"
    });
    options.SwaggerDoc("atletas", new OpenApiInfo
    {
        Title = "ClubeRank API - Atletas",
        Version = "v1"
    });
    options.SwaggerDoc("competicoes", new OpenApiInfo
    {
        Title = "ClubeRank API - Competicoes",
        Version = "v1"
    });
    options.SwaggerDoc("auditoria", new OpenApiInfo
    {
        Title = "ClubeRank API - Auditoria",
        Version = "v1"
    });

    options.DocInclusionPredicate((documentName, apiDescription) =>
    {
        var groupName = apiDescription.GroupName;
        return string.Equals(groupName, documentName, StringComparison.OrdinalIgnoreCase);
    });

    options.TagActionsBy(api => [api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] ?? "Default"]);
    options.OrderActionsBy(api => $"{api.GroupName}_{api.RelativePath}");

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o token JWT. Exemplo: eyJhbGciOi..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var clubeRankDbContext = services.GetRequiredService<ClubeRankDbContext>();
    var identityDbContext = services.GetRequiredService<IdentityDbContext>();

    await clubeRankDbContext.Database.MigrateAsync();
    await identityDbContext.Database.EnsureCreatedAsync();
    await IdentityInitializationExtensions.EnsureRolesCreatedAsync(services);
    await IdentityInitializationExtensions.EnsureSeedDataAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/acesso/swagger.json", "Acesso");
        options.SwaggerEndpoint("/swagger/organizacoes/swagger.json", "Organizacoes");
        options.SwaggerEndpoint("/swagger/atletas/swagger.json", "Atletas");
        options.SwaggerEndpoint("/swagger/competicoes/swagger.json", "Competicoes");
        options.SwaggerEndpoint("/swagger/auditoria/swagger.json", "Auditoria");
        options.ConfigObject.Urls = new List<UrlDescriptor>
        {
            new() { Name = "Acesso", Url = "/swagger/acesso/swagger.json" },
            new() { Name = "Organizacoes", Url = "/swagger/organizacoes/swagger.json" },
            new() { Name = "Atletas", Url = "/swagger/atletas/swagger.json" },
            new() { Name = "Competicoes", Url = "/swagger/competicoes/swagger.json" },
            new() { Name = "Auditoria", Url = "/swagger/auditoria/swagger.json" }
        };
        options.ConfigObject.DisplayOperationId = false;
        options.ConfigObject.DisplayRequestDuration = true;
        options.ConfigObject.DefaultModelsExpandDepth = 1;
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
