using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.IdentityModel.Tokens;
using WebApi.CustomEvents;
using System;

namespace WebApi
{
    public class Startup
    {        
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddCors(setup => {
                setup.AddPolicy("open", policyBuilder =>
                {
                    policyBuilder.AllowAnyHeader();
                    policyBuilder.AllowAnyMethod();
                    policyBuilder.AllowAnyOrigin();
                });
            });

            // Add framework services.
            services.AddMvc(options =>
            {
                options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var clientId = Configuration["AzureAd:ClientId"];
            var tenantId = Configuration["AzureAd:TenantId"];            
            var issuer = $"https://sts.windows.net/{tenantId}/";

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {                
                Authority = "https://login.microsoftonline.com/common/",
                TokenValidationParameters =                
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,                       
                    ValidAudiences = new string[] {clientId },
                    ValidateLifetime = true
                },
                Events = new MyJwtBearerEvents(loggerFactory.CreateLogger<MyJwtBearerEvents>())
            });

            app.UseCors("open");
            app.UseMvc();
        }
    }
}
