using ApiTrader.OtherClasses;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using spw;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton(new SpWorlds("f4323357-1d13-40a2-b1b6-f27cbb11425f", "iqvk7yPaYm/SwNU3F6fNEntEp79wHWVG"));


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
                      policy =>
                      {
                          policy.AllowAnyOrigin().AllowAnyHeader()
                                                  .AllowAnyMethod(); 
                      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors();

app.MapControllers();

app.Run();
