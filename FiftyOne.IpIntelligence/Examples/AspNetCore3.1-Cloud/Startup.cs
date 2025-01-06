/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2020 51 Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY.
 *
 * This Original Work is licensed under the European Union Public Licence (EUPL) 
 * v.1.2 and is subject to its terms as set out below.
 *
 * If a copy of the EUPL was not distributed with this file, You can obtain
 * one at https://opensource.org/licenses/EUPL-1.2.
 *
 * The 'Compatible Licences' set out in the Appendix to the EUPL (as may be
 * amended by the European Commission) shall be deemed incompatible for
 * the purposes of the Work and the provisions of the compatibility
 * clause in Article 5 of the EUPL shall not apply.
 * 
 * If using the Work as, or as part of, a network application, by 
 * including the attribution notice(s) required under Article 5 of the EUPL
 * in the end user terms of the application under an appropriate heading, 
 * such notice(s) shall fulfill the requirements of that article.
 * ********************************************************************* */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FiftyOne.IpIntelligence.Cloud.FlowElements;
using FiftyOne.Pipeline.CloudRequestEngine.FlowElements;
using FiftyOne.Pipeline.Core.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/// @example AspNetCore3.1-Cloud/Startup.cs
/// This example shows how to integrate the Pipeline API with a 
/// IP intelligence cloud engine into an ASP.NET Core web app.
/// 
/// The source code for this example is available in full on [GitHub](https://github.com/51Degrees/ip-intelligence-dotnet/tree/master/FiftyOne.IpIntelligence/Examples/AspNetCore3.1-Cloud). 
/// 
/// This example can be configured to use the 51Degrees cloud service or a local 
/// data file. If you don't already have data file you can obtain one from the 
/// [ip-intelligence-data](https://github.com/51Degrees/ip-intelligence-data) 
/// GitHub repository.
/// 
/// To use the cloud service you will need to create a **resource key**. 
/// The resource key is used as short-hand to store the particular set of 
/// properties you are interested in as well as any associated license keys 
/// that entitle you to increased request limits and/or paid-for properties.
/// 
/// You can create a resource key using the 51Degrees [Configurator](https://configure.51degrees.com).
/// 
/// Required NuGet Dependencies:
/// - [Microsoft.AspNetCore.App](https://www.nuget.org/packages/Microsoft.AspNetCore.App/)
/// - [FiftyOne.IpIntelligence](https://www.nuget.org/packages/FiftyOne.IpIntelligence/)
/// - [FiftyOne.Pipeline.Web](https://www.nuget.org/packages/FiftyOne.Pipeline.Web/)
/// - [FiftyOne.Interfaces](https://www.nuget.org/packages/FiftyOne.Interfaces/)
/// - [FiftyOne.NetworkIntelligence.TrueIp](https://www.nuget.org/packages/FiftyOne.NetworkIntelligence.TrueIp/)
///
/// 1. Add Pipeline configuration options to appsettings.json. 
/// (or a separate file if you prefer. Just don't forget to add that 
/// file to your startup.cs).
/// ```{json}
/// Example on-premise configuration:
/// {
///   "PipelineOptions": {
///     "Elements": [
///       {
///         "BuilderName": "IpiOnPremiseEngineBuilder",
///         "BuildParameters": {
///           "DataFile": "51Degrees-LiteV4.1.ipi",
///           "CreateTempDataCopy": false,
///           "AutoUpdate": false,
///           "PerformanceProfile": "LowMemory",
///           "DataFileSystemWatcher": false,
///           "DataUpdateOnStartUp": false
///         }
///       }
///     ]
///   }
/// }
/// 
/// Example cloud configuration:
/// {
///   "PipelineOptions": {
///     "Elements": [
///       {
///         "BuilderName": "CloudRequestEngineBuilder",
///         "BuildParameters": {
///           "ResourceKey": "YourKey" 
///         }
///       },
///       {
///         "BuilderName": "IpiCloudEngineBuilder"
///       }
///     ]
///   }
/// }
/// ```
/// 
/// 2. Add builders and the Pipeline to the server's services.
/// ```{cs}
/// public class Startup
/// {
///     ...
///     public void ConfigureServices(IServiceCollection services)
///     {
///         ...
///         services.AddSingleton<CloudRequestEngineBuilder>();
///         services.AddSingleton<IpiCloudEngineBuilder>();
///         services.AddFiftyOne(Configuration);
///         ...
/// ```
/// 
/// 3. Configure the server to use the Pipeline which has just been set up.
/// ```{cs}
/// public class Startup
/// {
///     ...
///     public void Configure(IApplicationBuilder app, IHostingEnvironment env)
///     {
///         app.UseFiftyOne();
///         ...
/// ```
/// 
/// 4. Inject the `IFlowDataProvider` into a controller.
/// ```{cs}
/// public class HomeController : Controller
/// {
///     private IFlowDataProvider _flow;
///     public HomeController(IFlowDataProvider flow)
///     {
///         _flow = flow;
///     }
///     ...
/// }
/// ```
/// 
/// 5. Pass the results contained in the flow data to the view.
/// ```{cs}
/// public class HomeController : Controller
/// {
///     ...
///     public IActionResult Index()
///     {
///         var data = _flow.GetFlowData().Get<IIpData>();
///         return View(data);
///     }
///     ...
/// ```
/// 
/// 6. Display IP details in the view.
/// ```{cs}
/// @model FiftyOne.IpIntelligence.IIpData
/// ...
/// var countries = Model.Countries;
/// ...
/// Countries:
/// @if(countries.HasValue)
/// {
///     < ol >
///         @foreach(WeightedValue<string> country in countries.Value)
///         {
///             < li > @country.Value, @country.Weight </ li >
///         }
///     </ ol >
/// }
/// ...
/// ```
/// 
/// ## Controller
/// @include Controllers/HomeController.cs
/// 
/// ## View
/// @include Views/Home/Index.cshtml
/// 
/// ## Startup
/// 
/// Expected output:
/// ```
/// ...
/// RangeStart: ::
/// RangeEnd: 2001:23f:ffff:ffff:ffff:ffff:ffff:ffff
/// CountryCodes:
///     ZZ, 1
/// Cities:
///     Unknown, 1
/// AverageLocation: 0,0
/// LocationBoundNorthWest: 0,0
/// LocationBoundSouthEast: 0,0
/// ...
/// ```
///

namespace IpIntelligenceWebDemoNetCore3_1Cloud
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
            // This section is not really necessary. We're just checking
            // if the resource key has been set to a new value.
            var pipelineConfig = new PipelineOptions();
            Configuration.Bind("PipelineOptions", pipelineConfig);
            var cloudConfig = pipelineConfig.Elements.Where(e =>
                e.BuilderName.Contains(nameof(CloudRequestEngine),
                    StringComparison.OrdinalIgnoreCase));
            if (cloudConfig.Count() > 0)
            {
                if (cloudConfig.Any(c => c.BuildParameters
                         .TryGetValue("ResourceKey", out var resourceKey) == true &&
                     resourceKey.ToString().StartsWith("!!")))
                {
                    throw new Exception("You need to create a resource key at " +
                        "https://configure.51degrees.com and paste it into the " +
                        "appsettings.json file in this example.");
                }
            }


            services.AddControllersWithViews();
            services.AddSingleton<HttpClient>();
            services.AddSingleton<CloudRequestEngineBuilder>();
            services.AddSingleton<IpiCloudEngineBuilder>();
            services.AddFiftyOne(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseFiftyOne();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
