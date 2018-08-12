using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcmeCorporation.Raffle.Domain.Interfaces;
using AcmeCorporation.Raffle.Infrastructure.Services;
using AcmeCorporation.Raffle.Infrastructure.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AcmeCorporation.Raffle.WebApi
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
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            
            var connectionString = @"";
            services
                .AddDbContext<RaffleDbContext>(contextOptions =>
                {
                    contextOptions.UseSqlServer(connectionString,
                        sqloptions => sqloptions
                            .MigrationsAssembly("AcmeCorporation.Raffle.Infrastructure"));
                });


            services.AddTransient<ISerialNumberRepository, SerialNumberRespository>();
            services.AddTransient<IRaffleSubmissionService, RaffleSubmissionService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();

            using (var dbContext = app.ApplicationServices.GetService<RaffleDbContext>())
            {
                dbContext.Database.Migrate();
                dbContext.Database.EnsureCreated();
            }
        }
    }
}
