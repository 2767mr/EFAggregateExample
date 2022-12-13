using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

using var db = new BloggingContext();

#region Insert example data

await db.Database.MigrateAsync();

db.Posts.RemoveRange(db.Posts);
db.Blogs.RemoveRange(db.Blogs);
await db.SaveChangesAsync();


var blogA = new Blog() { Url = "blogA" };
var blogB = new Blog() { Url = "blogB" };

var postA = new Post() { Title = "postA", ContentLength = 1 };
var postB = new Post() { Title = "postB", ContentLength = 2 };

blogB.Posts.Add(postA);
blogB.Posts.Add(postB);


db.Blogs.Add(blogA);
db.Blogs.Add(blogB);

await db.SaveChangesAsync();

#endregion

Console.WriteLine("--- insert done --");

Console.WriteLine("Navigation Join - executed on client:");
var result = await db.Blogs.Select(b => new
    {
        Url = b.Url,
        PostTitles = b.Posts.IsNullOrEmpty() ? "_none_" : string.Join(", ", b.Posts.Select(p => p.Title))
    })
    .AsSingleQuery()
    .ToListAsync();

Console.WriteLine(JsonSerializer.Serialize(result));


Console.WriteLine("Navigation Concat - executed on client:");
result = await db.Blogs.Select(b => new
{
    Url = b.Url,
    PostTitles = b.Posts.IsNullOrEmpty() ? "_none_" : string.Concat(b.Posts.Select(p => p.Title))
})
    .AsSingleQuery()
    .ToListAsync();

Console.WriteLine(JsonSerializer.Serialize(result));


Console.WriteLine("GroupBy Union Join - executed on server:");

result = await db.Posts
    .GroupBy(p => p.Blog)
    .Select(g => new
{
    Url = g.Key.Url,
    PostTitles = string.Join(", ", g.Select(p => p.Title))
}).Union(
    db.Blogs
        .Where(b => b.Posts.Count == 0)
        .Select(b => new { Url = b.Url, PostTitles = "_none_" })
).ToListAsync();

Console.WriteLine(JsonSerializer.Serialize(result));

Console.WriteLine("Navigation sum - executed on server:");

var result2 = await db.Blogs.Select(b => new
{
    Url = b.Url,
    TotalLength = b.Posts.IsNullOrEmpty() ? 0 : b.Posts.Select(p => p.ContentLength).Sum()
})
    .AsSingleQuery()
    .ToListAsync();

Console.WriteLine(JsonSerializer.Serialize(result2));



Console.WriteLine("GroupBy distinct sum - executed on server:");
try
{
    result2 = await db.Posts
        .GroupBy(p => p.Blog)
        .Select(b => new
    {
        Url = b.Key.Url,
        TotalLength = b.Select(p => p.ContentLength).Distinct().Sum()
    })
        .AsSingleQuery()
        .ToListAsync();

    Console.WriteLine(JsonSerializer.Serialize(result2));
}
catch (Exception ex)
{
}


try
{
    Console.WriteLine("GroupBy Union distinct Join - exception:");

    result = await db.Posts
        .GroupBy(p => p.Blog)
        .Select(g => new
        {
            Url = g.Key.Url,
            PostTitles = string.Join(", ", g.Select(p => p.Title).Distinct())
        }).Union(
        db.Blogs
            .Where(b => b.Posts.Count == 0)
            .Select(b => new { Url = b.Url, PostTitles = "_none_" })
    ).ToListAsync();

    Console.WriteLine(JsonSerializer.Serialize(result));
}
catch (Exception ex)
{
}

public class BloggingContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }

    public string DbPath => Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../db.mdf"));
    public static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder => 
    {
        builder.AddConsole();
    });
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer($"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={DbPath};Integrated Security=True;Connect Timeout=30");
        options.UseLoggerFactory(loggerFactory);
    }
}

public class Blog
{
    public int BlogId { get; init; }
    public string Url { get; set; } = "";

    public List<Post> Posts { get; } = new();
}

public class Post
{
    public int PostId { get; set; }
    public string Title { get; set; } = "";
    public int ContentLength { get; set; } = 0;

    public int BlogId { get; set; }
    public Blog Blog { get; set; } = new();
}