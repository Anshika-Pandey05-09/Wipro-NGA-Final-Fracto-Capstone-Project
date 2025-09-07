// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models;
// using Newtonsoft.Json.Serialization;
// using System.Text;
// using Fracto.Api.Data;
// using Fracto.Api.Services;
// using Fracto.Api.Models; //  needed for AppUser

// var builder = WebApplication.CreateBuilder(args);

// // MVC + Newtonsoft
// builder.Services.AddControllers()
//     .AddNewtonsoftJson(o =>
//     {
//         o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
//     });

// // EF Core
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// // Core services
// builder.Services.AddScoped<ITokenService, TokenService>();
// builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>(); //  DI hasher

// // JWT
// var jwtSettings = builder.Configuration.GetSection("JwtSettings");
// var secret = jwtSettings.GetValue<string>("Secret") ?? throw new Exception("JWT secret missing");
// var keyBytes = Encoding.UTF8.GetBytes(secret);

// builder.Services
//     .AddAuthentication(options =>
//     {
//         options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//         options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//     })
//     .AddJwtBearer(options =>
//     {
//         options.RequireHttpsMetadata = false;   // dev only
//         options.SaveToken = true;
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuerSigningKey = true,
//             IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
//             ValidateIssuer = true,
//             ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
//             ValidateAudience = true,
//             ValidAudience = jwtSettings.GetValue<string>("Audience"),
//             ValidateLifetime = true,
//             ClockSkew = TimeSpan.Zero
//         };
//     });

// // CORS
// var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
//               ?? new[] { "http://localhost:4200" };

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAngular", policy =>
//     {
//         policy.WithOrigins(allowed)
//               .AllowAnyHeader()
//               .AllowAnyMethod();
//     });
// });

// // Swagger + bearer
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fracto API", Version = "v1" });

//     var securityScheme = new OpenApiSecurityScheme
//     {
//         Name = "Authorization",
//         Description = "Bearer {token}",
//         In = ParameterLocation.Header,
//         Type = SecuritySchemeType.Http,
//         Scheme = "bearer",
//         BearerFormat = "JWT",
//         Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
//     };
//     c.AddSecurityDefinition("Bearer", securityScheme);
//     c.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });
// });

// var app = builder.Build();

// // migrate + seed
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     db.Database.Migrate();
//     SeedData.Initialize(db);
// }

// app.UseSwagger();
// app.UseSwaggerUI(c =>
// {
//     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fracto API v1");
//     c.RoutePrefix = "swagger";
// });

// app.UseHttpsRedirection();
// app.UseStaticFiles();      // serves wwwroot (images/uploads)
// app.UseRouting();
// app.UseCors("AllowAngular");
// app.UseAuthentication();
// app.UseAuthorization();
// app.MapControllers();
// app.Run();

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Fracto.Api.Data;
using Fracto.Api.Services;
using Fracto.Api.Models; // AppUser

var builder = WebApplication.CreateBuilder(args);

// ---------- MVC + Newtonsoft ----------
builder.Services.AddControllers()
    .AddNewtonsoftJson(o =>
    {
        o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        // If you hit reference loops with EF entities, uncomment:
        // o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// ---------- EF Core ----------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- Core services ----------
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

// ---------- JWT ----------
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings.GetValue<string>("Secret") ?? throw new Exception("JWT secret missing");
var keyBytes = Encoding.UTF8.GetBytes(secret);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;   // dev only
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
            ValidateAudience = true,
            ValidAudience = jwtSettings.GetValue<string>("Audience"),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// ---------- CORS ----------
var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
              ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(allowed)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ---------- Swagger (with bearer) ----------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fracto API", Version = "v1" });

    // Prevent conflicts & schema id collisions
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.CustomSchemaIds(t => t.FullName);

    // Map DateOnly/TimeOnly if used
    c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
    c.MapType<TimeOnly>(() => new OpenApiSchema { Type = "string", Format = "time" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// ---------- migrate + seed ----------
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        SeedData.Initialize(db);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database migration/seed failed");
    }
}

// ---------- pipeline ----------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fracto API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseStaticFiles();      // serves wwwroot (images/uploads)
app.UseRouting();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();