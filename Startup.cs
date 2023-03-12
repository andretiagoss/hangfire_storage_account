using Azure.Identity;
using Hangfire.API.Infrastructure.AzureStorage;
using Hangfire.API.Infrastructure.BackgroundTasks;
using Hangfire.API.Infrastructure.Configs;
using Hangfire.API.Infrastructure.Filters;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;

namespace Hangfire.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigurationServices(IServiceCollection services)
        {
            services.AddControllers();            
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();            
            services.AddCustomHangfire(_configuration);            
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(_configuration);

            services.AddCustomAzureStorage(_configuration);
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHangfireDashboard("/jobs", new DashboardOptions
                {
                    DashboardTitle = "Hangfire Jobs",
                    Authorization = new[] { new HangfireDashboardFilter() },
                    IsReadOnlyFunc = (DashboardContext context) =>
                    {
                        //var httpContext = context.GetHttpContext();
                        //if (httpContext.User.IsInRole("Admin") == false)
                        //    return true;        
                        //else        
                        //    return false;
                        return false;
                    }
                });

                endpoints.MapControllers();
            });            
        }
    }
}
