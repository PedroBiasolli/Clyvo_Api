using Clyvo.PetCare.Application.Services;
using Clyvo.PetCare.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Clyvo.PetCare.Application;

/// <summary>Registro dos casos de uso no contêiner de injeção de dependência.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITutorService, TutorService>();
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<IVeterinarioService, VeterinarioService>();
        services.AddScoped<IConsultaService, ConsultaService>();
        services.AddScoped<IVacinaService, VacinaService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuditoriaRegistrador, AuditoriaRegistrador>();
        services.AddScoped<IAuditoriaConsultaService, AuditoriaConsultaService>();

        return services;
    }
}
