using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Просто встав свій довгий рядок сюди"
  });

  options.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference
        {
          Type = ReferenceType.SecurityScheme,
          Id= "Bearer"
        }
      },
      Array.Empty<string>()
    }
  });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer").AddJwtBearer(options =>
{  
      options.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = "MarkAuthServer",
          ValidAudience = "MarkAuthClient",
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySuperSecretKeyThatIsAtLeast32BytesLong123"))

      };

});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

using(var scope = app.Services.CreateScope())
{
  var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapPost("/register", (string username, string password, AppDbContext db) =>
{
    if(db.Users.Any(u => u.Username == username))
    {
      return Results.BadRequest("Цей логін вже зайнятий");
    }

    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

    var newUser = new User
    {
      Username = username,
      PasswordHash = hashedPassword
    };

    db.Users.Add(newUser);
    db.SaveChanges();

    return Results.Ok($"Створено юзера: {username}. Хеш пароля: {hashedPassword}");
});

app.MapPost("/login", (string username, string password, AppDbContext db) =>
{
    var user = db.Users.FirstOrDefault(u => u.Username == username);
    if(user == null)
      return Results.BadRequest($"Немає такого тіпа {username}");

    if(!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
      return Results.BadRequest("Не правильний пароль, спробуй ще раз");

    var claims = new[]
    {
      new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new Claim(ClaimTypes.Name, user.Username)
    };

    var secretKey = "MySuperSecretKeyThatIsAtLeast32BytesLong123";
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: "MarkAuthServer",
        audience: "MarkAuthClient",
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: creds
        );
    
    string jwt = new JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new { message = "Молодець залогінився", token = jwt});
});

app.MapGet("/vip", (ClaimsPrincipal user) =>
{
  return Results.Ok($"Ласкаво просимо до VIP зони, бро. Твій логін {user.Identity?.Name}");
})
.RequireAuthorization();

app.Run();

public class User
{
  public int Id{ get; set;}
  public string Username {get; set;}= String.Empty;
  public string PasswordHash {get; set;} = String.Empty;
}

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  public DbSet<User> Users => Set<User>();
}
