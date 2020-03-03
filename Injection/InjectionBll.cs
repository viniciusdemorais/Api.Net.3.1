using Business;
using Core.Contract.Bll;
using Microsoft.Extensions.DependencyInjection;

namespace Injection
{
    public static class InjectionBll
    {
        public static IServiceCollection AddInjectionsBll(this IServiceCollection services)
        {
            services.AddSingleton<IAuthenticationBll, AuthenticationBll>();
            return services;
        }
    }
}
