using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using CadetTest.Entities;
using CadetTest.Helpers;
using CadetTest.Services;
using Microsoft.EntityFrameworkCore;

namespace CadetTest
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
            // in memory database used for simplicity, change to a real db for production applications
            services.AddDbContext<DataContext>(x => x.UseInMemoryDatabase("CadetTestDb"));

            services.AddCors();
            services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.IgnoreNullValues = true);

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // configure DI for application services
            services.AddScoped<IDataService, DataService>();
            services.AddScoped<IContextPersistency, ContextPersistency>();
            services.AddScoped<IUserService, UserService>();

            services.AddRazorPages();

            services.AddSwaggerGen();

            //services.AddMvc();
            services.AddControllersWithViews();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DataContext context, IContextPersistency persistentContext, IDataService dataService)
        {
            context.Users.Add(new User { Username = "User-5E638711-2D64-47B0-A8F5-1C5A9EADA966", Password = "Pass-E4A679C7-2F4B-40AE-9DC8-C967EF7215AE" });
            context.Users.Add(new User { Username = "User-1A30567E-C121-44DF-8008-BCEBEC01BF0A", Password = "Pass-095584F8-0EFB-4F21-B3FB-7DB0899AE4A7" });
            context.Users.Add(new User { Username = "User-58076950-C311-4DD1-88DE-E682C164E3BE", Password = "Pass-15D6617D-721B-46D6-AC9C-B50E8EA207E0" });
            context.Users.Add(new User { Username = "User-DB42737F-6AAA-453C-A5E8-72EB5229C18C", Password = "Pass-5DD8829F-0DCA-4140-A022-048CB5964D36" });

            for (int i = 1; i <= 1000; i++)
            {
                var consent = new Consent
                {
                    Recipient = $"{dataService.GetRandomString(10)}_{i}@ornek.com",
                    RecipientType = "BIREYSEL",
                    Status = "ONAY",
                    Type = "EPOSTA"
                };

                context.Consents.Add(consent);
            }

            context.SaveChanges();

            //persistentContext.InitConnectTssServiceUser();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bilmed Cadet Test API");
            });

            app.UseRouting();

            //app.UseCors(x => x
            //    .SetIsOriginAllowed(origin => true)
            //    .AllowAnyMethod()
            //    .AllowAnyHeader()
            //    .AllowCredentials());

            app.UseAuthentication();
            app.UseAuthorization();
            //app.UseMvcWithDefaultRoute();
            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
