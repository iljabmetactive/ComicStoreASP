using ComicStoreASP.Data;
using Microsoft.EntityFrameworkCore;

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
var filePath = Path.Combine(AppContext.BaseDirectory, "names.csv");
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer(connectionString)
    .Options;


using var context = new ApplicationDbContext(options);

if (!context.DataComics.Any())
{
    Console.WriteLine("Database already seeded. Skipping...");
    return;
}

Console.WriteLine("Looking for file at: " + filePath);
Console.WriteLine("File exists: " + File.Exists(filePath));