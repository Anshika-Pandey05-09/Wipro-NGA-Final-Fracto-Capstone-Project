using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Fracto.Api.Data;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder webBuilder)
    {
        webBuilder.ConfigureServices((context, services) =>
        {
            // Remove app's DbContext registrations
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                         || d.ServiceType == typeof(IDbContextFactory<ApplicationDbContext>))
                .ToList();
            foreach (var d in toRemove) services.Remove(d);

            // Build a unique SQL Server DB for each run
            var baseConn = context.Configuration.GetConnectionString("DefaultConnection");
            string testConn;
            if (!string.IsNullOrWhiteSpace(baseConn))
            {
                try
                {
                    var csb = new SqlConnectionStringBuilder(baseConn);
                    csb.InitialCatalog = "FractoTest_" + Guid.NewGuid().ToString("N");
                    testConn = csb.ToString();
                }
                catch
                {
                    testConn =
                        $"Server=(localdb)\\MSSQLLocalDB;Database=FractoTest_{Guid.NewGuid():N};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
                }
            }
            else
            {
                testConn =
                    $"Server=(localdb)\\MSSQLLocalDB;Database=FractoTest_{Guid.NewGuid():N};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
            }

            services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(testConn));

            // Create fresh schema
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }
            catch
            {
                db.Database.Migrate();
            }
        });
    }
}