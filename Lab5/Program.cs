using Kursach.Domain.Abstractions;
using Kursach.Infrastructure;
using Kursach.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System;

namespace Lab5
{
    public class Person(string email, string password)
    {
        public string Password = password;
        public string Email = email;

        public class Program
        {
            public static void Main(string[] args)
            {
                var people = new List<Person>
             {
                new Person("tom@gmail.com", "12345"),
                new Person("bob@gmail.com", "55555")
            };

                var builder = WebApplication.CreateBuilder(args);

                // Add services to the container.
                builder.Services.AddControllersWithViews();
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
                builder.Services.AddScoped<IClientRepository, ClientRepository>();
                builder.Services.AddRazorPages();


                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // �������� ��������
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,

                        // �������� ���������
                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,

                        // �������� ������� ����� ������
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero, // ���������� ��������� �����������

                        // �������� ����� �������
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),

                        // �������������� ���������
                        RequireExpirationTime = true, // ���������, ��� ����� ����� ���� ��������
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // ���������� ������ �� ����
                            context.Token = context.HttpContext.Request.Cookies["jwtToken"];
                            return Task.CompletedTask;
                        }
                    };
                });

                builder.Services.AddAuthorization();

                var app = builder.Build();


                // Configure the HTTP request pipeline.
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();
                app.MapRazorPages();

                app.MapStaticAssets();
                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                    .WithStaticAssets();

                app.Run();
            }
        }
    }
}
