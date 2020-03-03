using Core.Model.Settings;
using Injection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Api
{
    public class Startup
    {
        public AppSettings AppSettings { get; set; }
        private const string SWAGGER_JSON = "/swagger/v1/swagger.json";
        private const string AUTHORIZATION = "Authorization";
        private const string OAUTH2 = "oauth2";

        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;
            string appsettingsPath;

#if DEBUG
            appsettingsPath = $"{AppDomain.CurrentDomain.BaseDirectory}appsettings.{env.EnvironmentName}.json";
#else
			appsettingsPath = $"{AppDomain.CurrentDomain.BaseDirectory}appsettings.json";
#endif

            AppSettings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(appsettingsPath, Encoding.UTF8));

            // Create a random Issuer Signing Key to Bearer Token
            AppSettings.TokenConfiguration.IssuerSigningKey = Guid.NewGuid().ToString();

        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddControllers();

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions =>
            {
                var paramsValidation = bearerOptions.TokenValidationParameters;
                paramsValidation.IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AppSettings.TokenConfiguration.IssuerSigningKey));
                paramsValidation.ValidAudience = AppSettings.TokenConfiguration.Audience;
                paramsValidation.ValidIssuer = AppSettings.TokenConfiguration.Issuer;

                // Validate token
                paramsValidation.ValidateIssuerSigningKey = true;
                paramsValidation.ValidateAudience = true;
                paramsValidation.ValidateIssuer = true;
                paramsValidation.ValidateLifetime = true;
                paramsValidation.RequireExpirationTime = true;
                paramsValidation.ClockSkew = TimeSpan.Zero;
            });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy(AppSettings.TokenConfiguration.TokenType, new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build());
            });

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition(AppSettings.TokenConfiguration.TokenType, new OpenApiSecurityScheme
                {
                    Description = AppSettings.TokenConfiguration.TokenDescription,
                    Name = AUTHORIZATION,
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = AppSettings.TokenConfiguration.TokenType
                }) ;
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = AppSettings.TokenConfiguration.TokenType
                            },
                            Scheme = OAUTH2,
                            Name = AppSettings.TokenConfiguration.TokenType,
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
                c.SwaggerDoc(AppSettings.Api.Version, new OpenApiInfo { Title = AppSettings.Api.Name, Version = AppSettings.Api.Version });
            });

            services.AddSingleton(AppSettings);

            services.AddInjectionsBll();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(SWAGGER_JSON, $"{AppSettings.Api.Name} {AppSettings.Api.Version}");
                c.RoutePrefix = string.Empty;
                c.DocExpansion(DocExpansion.None);
            });

            app.UseCors(c =>
            {
                c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseResponseCompression();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
