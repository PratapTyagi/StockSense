using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Infrastructure.Extentions;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder = builder.AddWebApplicationBuilderConfiguration();
    var app = builder.Build();
    var log = app.Services.GetRequiredService<ILogger<Program>>();
    log.LogInformation("Starting application 🚀");
    app.MapControllers();
    app.UseCors("AllowAll");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Critical error: {ex}");
}

