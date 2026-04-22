using System.Text;
using Dalba.Financeiro.Application.Abstractions.Audit;
using Dalba.Financeiro.Application.Abstractions.Notifications;
using Dalba.Financeiro.Application.Abstractions.Persistence;
using Dalba.Financeiro.Application.Abstractions.Security;
using Dalba.Financeiro.Application.Abstractions.Storage;
using Dalba.Financeiro.Application.Services;
using Dalba.Financeiro.Infrastructure.Audit;
using Dalba.Financeiro.Infrastructure.Configuration;
using Dalba.Financeiro.Infrastructure.Notifications;
using Dalba.Financeiro.Infrastructure.Persistence;
using Dalba.Financeiro.Infrastructure.Security;
using Dalba.Financeiro.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Dalba.Financeiro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.Configure<SmsSettings>(configuration.GetSection(SmsSettings.SectionName));
        services.PostConfigure<SmsSettings>(settings =>
        {
            settings.Provider = configuration["SMS_PROVIDER"] ?? settings.Provider;
            settings.Password = configuration["SMS_PASSWORD"] ?? configuration["SMS_PASSOWORD"] ?? settings.Password;
            settings.Sender = configuration["SMS_PHONE"] ?? settings.Sender;
            settings.Account = configuration["SMS_ACCOUNT"] ?? settings.Account;
            settings.Token = configuration["SMS_TOKEN"] ?? settings.Token;
        });
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddHttpContextAccessor();
        services.AddHttpClient();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<INotificationDispatcher, SmtpNotificationDispatcher>();
        services.AddScoped<IAuditService, AuditService>();

        services.AddScoped<AuthService>();
        services.AddScoped<UsuarioService>();
        services.AddScoped<CategoriaService>();
        services.AddScoped<FornecedorService>();
        services.AddScoped<ContratoService>();
        services.AddScoped<DocumentoCatalogService>();
        services.AddScoped<SupplierPortalService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<AdminValidationService>();
        services.AddScoped<FinanceiroService>();
        services.AddScoped<DashboardService>();

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("FinanceiroOnly", policy => policy.RequireRole("Financeiro"));
            options.AddPolicy("FornecedorOnly", policy => policy.RequireRole("Fornecedor"));
            options.AddPolicy("AdminOrFinanceiro", policy => policy.RequireRole("Admin", "Financeiro"));
        });

        return services;
    }
}
