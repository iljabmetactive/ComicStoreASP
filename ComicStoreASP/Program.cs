using ComicStoreASP.Data;
using ComicStoreASP.Models;
using ComicStoreASP.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace ComicStoreASP
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddTransient<CSVDataReader>(); // Register CsvService
            builder.Services.AddSingleton<ComicStore>();
            builder.Services.AddSingleton<ComicGenreFilter>();
            builder.Services.AddHostedService<DatatableUpdateService>();


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();


            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 128 * 1024 * 1024; // 128 MB
            });

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 128 * 1024 * 1024;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                await next();
            });

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                context.Database.Migrate();

                string[] roles = { "Staff", "PublicUser" };

                foreach (var role in roles)
                {
                    if (!roleManager.RoleExistsAsync(role).Result)
                    {
                        roleManager.CreateAsync(new IdentityRole(role)).Wait();
                    }
                }

                // Ensure default dataset version exists
                if (!context.DatatableVersions.Any())
                {
                    context.DatatableVersions.Add(new DatatableVersion
                    {
                        VersionName = "Initial",
                        ImportedAt = DateTime.UtcNow,
                        IsActive = true
                    });

                    context.SaveChanges();
                }
                app.Run();
            }
        }
    }
}
