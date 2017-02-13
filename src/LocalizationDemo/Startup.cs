using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace LocalizationDemo
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        protected IServiceProvider ServiceProvider { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddMvc().AddViewLocalization(
                LanguageViewLocationExpanderFormat.Suffix,
                opts => { opts.ResourcesPath = "Resources"; }).
                AddDataAnnotationsLocalization();

            //// Configure supported cultures and localization options
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("es"),
                };

                //// State what the default culture for your application is. This will be used if no specific culture
                //// can be determined for a given request.
                options.DefaultRequestCulture = new RequestCulture("en-US", "en-US");

                //// You must explicitly state which cultures your application supports.
                //// These are the cultures the app supports for formatting numbers, dates, etc.
                options.SupportedCultures = supportedCultures;

                //// These are the cultures the app supports for UI strings, i.e. we have localized resources for.
                options.SupportedUICultures = supportedCultures;
            });

            // Add framework services.
            var assembly = typeof(SampleResources.SampleResourcesClass).GetTypeInfo().Assembly;

            var fileProviders = new IFileProvider[] {
                this.ServiceProvider.GetService<IHostingEnvironment>().WebRootFileProvider
            };
            var providers= new CompositeFileProvider(fileProviders.Concat(new List<EmbeddedFileProvider> {
                new EmbeddedFileProvider(assembly, assembly.GetName().Name)
            }));
            this.ServiceProvider.GetService<IHostingEnvironment>().WebRootFileProvider = providers;


            var mvcBuilder = services.AddMvc();
            mvcBuilder.AddApplicationPart(assembly);
            mvcBuilder.AddRazorOptions(
             o =>
             {
                 o.FileProviders.Add(new EmbeddedFileProvider(assembly, assembly.GetName().Name));
             }
           );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

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
