using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;
using triliza.Data;
using triliza.Models;
using triliza.Controllers;
using triliza;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("FinalProject")));

        builder.Services.AddScoped<SessionService>();
      
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
         builder => builder.WithOrigins("https://localhost:52384")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials());
        });

        builder.Services.AddSignalR();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "MyAppAuthCookie";
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        });

        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
        builder.Services.AddAuthentication(options=> {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })

            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Cookies["authToken"];

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

  
        builder.Services.AddAuthorization();

        builder.Services.AddControllers();
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();


        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        

        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Marker"))
                await roleManager.CreateAsync(new IdentityRole("Marker"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser { UserName = "admin", Email = "admin@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(adminUser, "password1234@ABC");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        app.UseRouting();
        app.UseDeveloperExceptionPage();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors("AllowFrontend");
        app.MapHub<RealTimeHub>("/realtimehub").RequireCors("AllowFrontend");

      
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        
        app.MapControllers();
        app.MapFallbackToFile("/index.html");

        app.Run();
    }
}