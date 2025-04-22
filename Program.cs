
using HTTL_May_Xay_Dung.DataAccess;
using HTTL_May_Xay_Dung.DTO;
using HTTL_May_Xay_Dung.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace HTTL_May_Xay_Dung
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = false; // Optional for formatting
                    options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
                });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            // session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn của session
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddCors(opts =>
            {
                opts.AddPolicy("CORSPolicy", builder =>
                    builder.AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials()
                           .SetIsOriginAllowed((host) => true));
            });
            builder.Services.AddScoped<MailService>();

            // Đăng ký DbContext với MySQL sử dụng Pomelo.EntityFrameworkCore.MySql
            builder.Services.AddDbContext<BfhahziulzpihzqwnwfhContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("MyDatabase"),
                    // Sử dụng Pomelo để tự động phát hiện phiên bản MySQL
                    new MySqlServerVersion(new Version(8, 0, 0))  // Thay bằng phiên bản MySQL của bạn
                )
            );

            // Bind PaginationSettings to configuration
            builder.Services.Configure<PaginationSettings>(builder.Configuration.GetSection("PaginationSettings"));


            // Thêm cấu hình JWT từ appsettings.json
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            // Thêm Authentication với JWT Bearer
            builder.Services.AddAuthentication(options =>
            {
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
    };
});

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("1"));  // 1 = Admin
                options.AddPolicy("ManagerOnly", policy => policy.RequireRole("2")); // 2 = Manager
                options.AddPolicy("NormalUserOnly", policy => policy.RequireRole("0")); // 0 = Normal User
            });

            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter your Bearer token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
            });



            // ODATA
            //builder.Services.AddControllers().AddOData(option => option.Select().Filter().Count().OrderBy().Expand().SetMaxTop(100)
            //.AddRouteComponents("odata", GetEdmModel()));


            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            var app = builder.Build();
            app.UseMiddleware<SwaggerAuthMiddleware>();
            app.UseWebSockets();
            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            // cân nhắc sửa
            //if (!app.Environment.IsEnvironment("Testing"))
            //{
            //    app.UseStaticFiles();
            //}

            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseMiddleware<LoggingMiddleware>();
            app.UseHttpsRedirection();
            app.UseCors("CORSPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            // cái này nữa
            //app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images")),
                RequestPath = "/images"
            });
            app.MapControllers();
            app.UseODataBatching();
            app.UseSession();
            app.Run();
        }
    }
}
