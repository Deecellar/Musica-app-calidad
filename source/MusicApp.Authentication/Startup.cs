using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace MusicApp.Authentication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddOpenIddict()
                .AddCore(options => { })
                .AddServer(options =>
                {
                    options
                        .SetAuthorizationEndpointUris("/authorize")
                        .SetTokenEndpointUris("/token")
                        .SetRevocationEndpointUris("/revoke")
                        .SetUserinfoEndpointUris("/user-info")
                        .SetIntrospectionEndpointUris("/introspect");
                    // Authorization Tokens to use
                    // We decide against the usage of username/password for the security
                    // Instead we use AuthorizationCodeFlow and RefreshTokens to access resources 
                    options
                        .AllowClientCredentialsFlow()
                        .AllowAuthorizationCodeFlow()
                        .AllowDeviceCodeFlow()
                        .AllowRefreshTokenFlow();

                    options
                        .AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    options
                        .UseAspNetCore()
                        .EnableTokenEndpointPassthrough();

                    options
                        .SetAccessTokenLifetime(TimeSpan.FromHours(12))
                        .SetRefreshTokenLifetime(TimeSpan.FromDays(1))
                        .SetDeviceCodeLifetime(TimeSpan.FromDays(31));
                    
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateAudience = true,
                        ValidateIssuer = false,
                        RequireExpirationTime = true,
                        ValidateLifetime = true,
                        RequireAudience = true
                    };
                });
            
                services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "MusicApp.Authentication",
                    Version = "v1",
                    Description = "SSO API for MusicApp Auth apps",
                    TermsOfService = new Uri("http//example.com/todo"),
                    Contact = new OpenApiContact()
                    {
                        Email = "19101das@gmail.com",
                        Name = "Dan Ellis Echavarria",
                        Url = new Uri("http://lldragon.net")
                    },
                    License = new OpenApiLicense()
                    {
                        Name = "Use under the MIT license",
                        Url = new Uri("/MIT")
                    }

                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MusicApp.Authentication v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthorization();


            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}