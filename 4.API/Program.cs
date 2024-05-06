using _2.Core;
using _2.Core.Extensions;
using _3.Infra;
using _3.Infra.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Configure IOption Pattern

// builder.Services.ConfigureInfraOptions();

builder.Services.Configure<MailGunEmailProviderOptions>(
    builder.Configuration.GetSection(MailGunEmailProviderOptions.ConfigItem));

builder.Services.Configure<SendgridEmailProviderOptions>(
     builder.Configuration.GetSection(SendgridEmailProviderOptions.ConfigItem));

#endregion

#region Configure DI Container - Service Lifetimes - Infra

builder.Services.AddInfraDependencyInjection(builder.Configuration);

builder.Services.AddTransient<ITransientService, TransientService>();
builder.Services.AddScoped<IScopedService, ScopedService>();
builder.Services.AddSingleton<ISingletonService, SingletonService>();

// builder.Services.AddTransient<IEmailProvider, SendgridEmailProvider>();
builder.Services.AddTransient<IEmailProvider, MailGunEmailProvider>();

#endregion

#region Configure DI Container - Service Lifetimes - Business Services

builder.Services.AddCoreDependencyInjection();

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await SeedDatabase();

app.Run();

async Task SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbcontext =
          scope.ServiceProvider.GetRequiredService<DataContext>();

        // Run migration scripts
        await dbcontext.Database.MigrateAsync();

        // Seed data to the project
        await _3.Infra.Seed.SeedData(dbcontext);
    }
}