using Address2Map.BusinessController;
using Address2Map.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<VersionRepository>();
builder.Services.AddSingleton<RuianRepository>();
builder.Services.AddScoped<AddressBusinessController>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Address2Map Service API",
        Version = "v1",
        Description = "Api providing address point helper service"
    });

    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Bearer token.",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.OperationFilter<Swashbuckle.AspNetCore.Filters.SecurityRequirementsOperationFilter>();
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First()); //This line

    var xmlFile = $"doc/documentation.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("cors",
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173",
                                              "https://address2map.h1.scholtz.sk/",
                                              "https://address2mapfe.h1.scholtz.sk/")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseCors("cors");

var repo = app.Services.GetService<RuianRepository>();
repo?.ProcessCSV(File.ReadAllBytes("Data/20220930_OB_554782_ADR.csv"));
System.GC.Collect();
app.Run();
