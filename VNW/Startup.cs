using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.EntityFrameworkCore; //::for sql
using VNW.Models;

namespace VNW
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        string CorsPolicyName = "_CorsPolicy";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            #region ::Connect string of Db WContext <Timmy>
            services.AddDbContext<VeganNewWorldContext>(op => op.UseSqlServer(
                Configuration.GetConnectionString("VeganNewWorldContext")));
            #endregion

            #region ::set session <Timmy>
            services.AddDistributedMemoryCache();
            services.AddSession(
                options =>
                {
                    options.Cookie.Name = ".AdventureWorks.Session";
                    //options.IdleTimeout = TimeSpan.FromSeconds(10);
                    options.IdleTimeout = TimeSpan.FromMinutes(15); //15 min
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });
            #endregion

            #region CORS <Timmy>
            services.AddCors(options => {
                options.AddPolicy(CorsPolicyName, policy =>
                {
                    ////::for all
                    //policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();                    
                    //::for webapi_reader in IIS
                    policy.WithOrigins("http://127.0.0.1").AllowAnyHeader().AllowAnyMethod().AllowCredentials();                    
                    //::for Azure
                    policy.WithOrigins("https://apird2024.azurewebsites.net").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    //::for IIS local
                    policy.WithOrigins("https://192.168.0.102").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    //::for IIS internet IP
                    policy.WithOrigins("https://203.67.107.76").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    //::for IIS internet (NoIP)
                    policy.WithOrigins("https://vnw.ddns.net").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
            });
            #endregion

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            #region ::use session <Timmy>
            app.UseSession();
            app.Use(async (context, next) =>
            {
                context.Session.SetString("SessionKey", "SessoinValue");
                await next.Invoke();
            });            
            #endregion

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
