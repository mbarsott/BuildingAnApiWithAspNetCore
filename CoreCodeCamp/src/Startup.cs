using AutoMapper;
using CoreCodeCamp.Controllers;
using CoreCodeCamp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace CoreCodeCamp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CampContext>();
            services.AddScoped<ICampRepository, CampRepository>();

            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddApiVersioning(opt =>
            {
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.DefaultApiVersion = new ApiVersion(1, 1);
                opt.ReportApiVersions = true;
                // the following aproach of using version in the url segment is not a godd idea
                // opt.ApiVersionReader = new UrlSegmentApiVersionReader(); // use .../v{version:apiVersion}/... in the routing
                opt.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader("X-Version"),
                    new QueryStringApiVersionReader("ver", "version") // default would be api-version on the query string
                );
                var conventionsController = opt.Conventions.Controller<TalksController>();
                conventionsController.HasApiVersion(new ApiVersion(1, 0));
                conventionsController.HasApiVersion(new ApiVersion(1, 1));
            });
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
