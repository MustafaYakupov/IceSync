using IceSync.Common.Options;
using IceSync.Data;
using IceSync.Data.Repositories;
using IceSync.Data.Repositories.Contracts;
using IceSync.Infrastructure.Background;
using IceSync.Infrastructure.UniversalLoader;
using IceSync.Infrastructure.UniversalLoader.Contracts;
using IceSync.Services;
using IceSync.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace IceSync.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        // MVC
        builder.Services.AddControllersWithViews();

        // Options
        builder.Services.Configure<UniversalLoaderOptions>(
            builder.Configuration.GetSection("UniversalLoader"));

        string? connectionString = builder.Configuration
                .GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // EF Core
        builder.Services.AddDbContext<IceSyncDbContext>(options =>
        options.UseSqlServer(connectionString));

        // JWT caching
        builder.Services.AddMemoryCache();

        // HttpClients
        builder.Services.AddHttpClient("UniversalLoaderAuth", (sp, http) =>
        {
            var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<UniversalLoaderOptions>>().Value;
            http.BaseAddress = new Uri(opt.BaseApiUrl);
        });

        builder.Services.AddHttpClient<IUniversalLoaderClient, UniversalLoaderClient>((sp, http) =>
        {
            var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<UniversalLoaderOptions>>().Value;
            http.BaseAddress = new Uri(opt.BaseApiUrl);
        });

        // Infrastructure services
        builder.Services.AddSingleton<IUniversalLoaderTokenProvider, UniversalLoaderTokenProvider>();
        builder.Services.AddMemoryCache();

        // Repositories + UoW
        builder.Services.AddScoped<IWorkflowRepository, WorkflowRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // App services
        builder.Services.AddScoped<IWorkflowService, WorkflowService>();

        // Background job
        builder.Services.AddHostedService<WorkflowSyncHostedService>();

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
