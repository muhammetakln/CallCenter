using Business.IServices;
using Core.Abstracts;
using Core.Abstracts.IServices;
using Core.Concretes.Entities;
using Data;
using Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utilities.Helpers;
using Utilities.Models;

namespace Business
{
    public static class  IOC
    {
        public static IServiceCollection AddCustomerServices(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddDbContext<ApplicationContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("app_db")));
            services.AddIdentity<ApplicationUser, ApplicationUserRole>()
                .AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultTokenProviders();
            services.AddAutoMapper(config =>
            {

            });

            services.AddScoped<IAuthService, AuthService>();
            //Scoped başta oluşur arada değişebilir oldupu için scoped kullanıyoruz prototip yapı.Sadece veri tabaı işlemlerinde kullanılır.Son ana kadaara değişime açık.
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.Configure<EmailSettings>(
             configuration.GetSection("EmailSettings"));


            services.AddScoped<IEmailSender, EmailSender>();
            

            return services;
        }
    }
}
