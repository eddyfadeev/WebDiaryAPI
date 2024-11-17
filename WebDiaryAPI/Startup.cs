using Microsoft.EntityFrameworkCore;
using WebDiaryAPI.Data;
using WebDiaryAPI.Extensions;

namespace WebDiaryAPI;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration) =>
        _configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                _configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        services.AddRepository();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "WebDiaryAPI v1");
                options.RoutePrefix = string.Empty;
            });
        }

        app.UseRouting();
        
        app.UseHttpsRedirection();
        
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}