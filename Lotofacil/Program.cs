using Hangfire;
using Lotofacil.Application.BackgroundJobs;
using Lotofacil.Application.Common;
using Lotofacil.Infra.Data.Context;
using Lotofacil.Infra.IoC;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog usando o método de extensão
builder.Host.ConfigureSerilog();

// Garante que o banco de dados exista e esteja com as migrations aplicadas ANTES de
// AddInfrastructure() configurar o Hangfire, logo abaixo. GlobalConfiguration.Configuration
// .UseSqlServerStorage(...)/services.AddHangfire(...) preparam o schema do Hangfire de forma
// síncrona e eager assim que são configurados (PrepareSchemaIfNecessary=true por padrão) — sem
// isso aqui antes, essa preparação falha com "Cannot open database ... requested by login" contra
// um banco que ainda não existe. Usa um ApplicationDbContext avulso porque o container de DI
// (que resolveria um ApplicationDbContext normalmente) só existe depois de builder.Build().
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
using (var startupDbContext = new ApplicationDbContext(
    new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connectionString).Options))
{
    startupDbContext.Database.Migrate();
}

builder.Services.AddControllersWithViews();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMemoryCache();

var app = builder.Build();

// Middleware do Serilog para logging de requisições
app.UseSerilogRequestLogging();

// Inicialização do banco de dados
using (var scope = app.Services.CreateScope())
{
    var initService = scope.ServiceProvider.GetRequiredService<IInitializationDbService>();
    initService.Initialize();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Configuração do dashboard do Hangfire — restrito a Development (sem DashboardOptions.Authorization,
// a rota não pode ficar exposta publicamente em produção).
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}

RecurringJob.AddOrUpdate<MainJobHandler>(
    "main-job",
    x => x.ExecuteAsync(),
    "*/4 * * * *");

RecurringJob.AddOrUpdate<TopTenJobHandler>(
    "Top-ten-job",
    service => service.ExecuteAsync(),
    "*/9 * * * *"); 

app.Run();
