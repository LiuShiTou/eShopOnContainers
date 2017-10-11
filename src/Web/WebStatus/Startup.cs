﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebStatus.Extensions;

namespace WebStatus
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
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddOptions();

            // Add framework services.
            services.AddHealthChecks(checks =>
            {
                var minutes = 1;
                if (int.TryParse(Configuration["HealthCheck:Timeout"], out var minutesParsed))
                {
                    minutes = minutesParsed;
                }

                checks.AddUrlCheckIfNotNull(Configuration["OrderingUrl"], TimeSpan.FromMinutes(minutes)); 
                checks.AddUrlCheckIfNotNull(Configuration["BasketUrl"], TimeSpan.Zero); //No cache for this HealthCheck, better just for demos                  
                checks.AddUrlCheckIfNotNull(Configuration["CatalogUrl"], TimeSpan.FromMinutes(minutes)); 
                checks.AddUrlCheckIfNotNull(Configuration["IdentityUrl"], TimeSpan.FromMinutes(minutes)); 
                checks.AddUrlCheckIfNotNull(Configuration["LocationsUrl"], TimeSpan.FromMinutes(minutes)); 
                checks.AddUrlCheckIfNotNull(Configuration["MarketingUrl"], TimeSpan.FromMinutes(minutes)); 
                checks.AddUrlCheckIfNotNull(Configuration["PaymentUrl"], TimeSpan.FromMinutes(minutes)); 
                checks.AddUrlCheckIfNotNull(Configuration["mvc"], TimeSpan.Zero); //No cache for this HealthCheck, better just for demos 
                checks.AddUrlCheckIfNotNull(Configuration["spa"], TimeSpan.Zero); //No cache for this HealthCheck, better just for demos 
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddAzureWebAppDiagnostics();
            loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Trace);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            var pathBase = Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }            

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
