using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Daimler.Providence.Database;
using Daimler.Providence.Service.BusinessLogic;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.DAL;
using Daimler.Providence.Service.DAL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Daimler.Providence.Service.Utilities;
using System.Threading.Tasks;
using Daimler.Providence.Service.EventHub;
using Microsoft.ApplicationInsights.DataContracts;
using Daimler.Providence.Service.Scheduler.Jobs;
using Daimler.Providence.Service.Scheduler;
using System.Text.Json.Serialization;
using Daimler.Providence.Service.Controllers;
using Quartz;
using Quartz.Spi;
using Quartz.Impl;
using Daimler.Providence.Service.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Linq;
using Microsoft.OpenApi.Models;
using System.IO;
using Daimler.Providence.Service.Clients.Interfaces;
using Daimler.Providence.Service.Clients;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Daimler.Providence.Service.Authorization;

namespace Daimler.Providence.Service
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        /// <summary>
        /// Creates a new instance of <see cref="Startup"/>.
        /// </summary>
        /// <param name="env">The current hosting environment.</param>
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            //var builder = new ConfigurationBuilder()
            //    .SetBasePath(env.ContentRootPath)
            //    .AddJsonFile("appsettings.json", false, true)
            //    .AddEnvironmentVariables();
            //Configuration = builder.Build();
            Configuration = configuration;
            ProvidenceConfigurationManager.SetConfiguration(Configuration);
        }

        /// <summary>
        /// Gets the configurations.
        /// </summary>
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            string[] originURL;
            if (ProvidenceConfigurationManager.Region == "cn")
            {
                originURL = new string[] { "https://e2emonitoring." + ProvidenceConfigurationManager.Environment + ".csg.connectivity.fotondaimler.com" };
            }
            else
            {
                if (ProvidenceConfigurationManager.Environment == "prod")
                {
                    //e.g: https://e2emonitoring.jp.csg.daimler-truck.com/
                    originURL = new string[] { "https://e2emonitoring." + ProvidenceConfigurationManager.Region + ".csg.daimler-truck.com" };
                }
                else
                {
                    //e.g https://e2emonitoring.eu.staging.csg.daimler-truck.com/
                    originURL = new string[] { "https://e2emonitoring." + ProvidenceConfigurationManager.Region + "." + ProvidenceConfigurationManager.Environment + ".csg.daimler-truck.com" };

                }
            }

            services.AddCors(o => o.AddPolicy("corspolicy", builder =>
            {
                builder.WithOrigins(originURL)
                        .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            }));
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;

            }).AddJwtBearer(jwtOptions =>
            {
                jwtOptions.Authority = ProvidenceConfigurationManager.Instance + ProvidenceConfigurationManager.TenantId;
                jwtOptions.Audience = ProvidenceConfigurationManager.EnterpriseApplicationAppId;
                jwtOptions.RequireHttpsMetadata = false;

                // signalr authentication
                jwtOptions.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/signalr")))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = ProvidenceConfigurationManager.Instance + ProvidenceConfigurationManager.TenantId;
                options.ClientId = ProvidenceConfigurationManager.EnterpriseApplicationAppId;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = true;
            });

            services.AddAuthorization(options =>
            {
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
                defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
                options.AddPolicy("AdminPolicy", policy => policy.Requirements.Add(new Authorization.AdminRoleRequirement()));
                options.AddPolicy("ContributorPolicy", policy => policy.Requirements.Add(new Authorization.ContributorRoleRequirement()));
                options.AddPolicy("AdminOrContributorPolicy", policy => policy.Requirements.Add(new Authorization.AdminOrContributorRoleRequirement()));
                options.AddPolicy("WebApi", policy =>
                {
                    policy.AuthenticationSchemes.Add("Bearer");
                    policy.RequireAuthenticatedUser();
                });
            });

            services.AddSingleton<IAuthorizationHandler, Authorization.AdministratorAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, Authorization.ContributorAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, Authorization.AdminOrContributorAuthorizationHandler>();

            services.AddMvc().AddSessionStateTempDataProvider();

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.IgnoreNullValues = true;
            });

            services.AddSignalR(hubOptions => hubOptions.EnableDetailedErrors = true).AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.IgnoreNullValues = true;
                options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.PayloadSerializerOptions.PropertyNamingPolicy = null;
            });

            services.AddSession();
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                // Use the default property (Pascal) casing
                options.SerializerSettings.ContractResolver = new ProvidenceContractResolver();
            });

            // Add HttpContextAccessor for retrieving the User in ThreadContext
            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Add Singleton
            services.AddSingleton(typeof(IEnvironmentManager), typeof(EnvironmentManager));
            services.AddSingleton(typeof(IInternalJobManager), typeof(InternalJobManager));

            // Add Transient
            services.AddTransient(typeof(AlertCommentController));
            services.AddTransient(typeof(AlertController));
            services.AddTransient(typeof(AlertIgnoreController));
            services.AddTransient(typeof(ChangeLogController));
            services.AddTransient(typeof(ConfigurationController));
            services.AddTransient(typeof(SlaController));
            services.AddTransient(typeof(DeploymentController));
            services.AddTransient(typeof(EnvironmentController));
            services.AddTransient(typeof(HistoryController));
            services.AddTransient(typeof(ImportExportController));
            services.AddTransient(typeof(InternalJobController));
            services.AddTransient(typeof(LicenseInformationController));
            services.AddTransient(typeof(MaintenanceController));
            services.AddTransient(typeof(MasterdataController));
            services.AddTransient(typeof(NotificationRuleController));
            services.AddTransient(typeof(RefreshController));
            services.AddTransient(typeof(ResetController));
            services.AddTransient(typeof(StateIncreaseRuleController));
            services.AddTransient(typeof(TestController));
            services.AddTransient(typeof(IBlobStorageClient), typeof(BlobStorageClient));
            services.AddTransient(typeof(IAlertManager), typeof(AlertManager));
            services.AddTransient(typeof(IImportExportManager), typeof(ImportExportManager));
            services.AddTransient(typeof(ILicenseInformationManager), typeof(LicenseInformationManager));
            services.AddTransient(typeof(IMasterdataManager), typeof(MasterdataManager));
            services.AddTransient(typeof(IMaintenanceBusinessLogic), typeof(MaintenanceBusinessLogic));
            services.AddTransient(typeof(ISlaCalculationManager), typeof(SlaCalculationManager));

            //DAL
            services.AddSingleton(typeof(IStorageAbstraction), typeof(DataAccessLayer));
            services.AddSingleton(typeof(IDbContextFactory<MonitoringDB>), typeof(MonitoringDBFactory));

            // Add EventHub
            services.AddSingleton(typeof(EventHubMessageReceiver));
            //services.AddSingleton(typeof(EventProcessorFactory));

            // Client Repo for SignalR
            services.AddSingleton(typeof(ClientRepository));

            // Add Quartz services
            services.AddHostedService<JobScheduler>();
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            // Add cleanup jobs
            if (ProvidenceConfigurationManager.RunDeleteExpiredChangeLogs)
            {
                services.AddSingleton<DeleteExpiredChangeLogsJob>();
                services.AddSingleton(new JobSchedule(typeof(DeleteExpiredChangeLogsJob), "0 10 0/12 ? * * *")); // Run job every 12 hours
            }
            if (ProvidenceConfigurationManager.RunDeleteExpiredDeployments)
            {
                services.AddSingleton<DeleteExpiredDeploymentsJob>();
                services.AddSingleton(new JobSchedule(typeof(DeleteExpiredDeploymentsJob), $"0 20 0/12 ? * * *")); // Run job every 12 hours
            }
            if (ProvidenceConfigurationManager.RunDeleteExpiredStateTransitions)
            {
                services.AddSingleton<DeleteExpiredStateTransitionsJob>();
                services.AddSingleton(new JobSchedule(typeof(DeleteExpiredStateTransitionsJob), $"0 30 0/12 ? * * *")); // Run job every 12 hours
            }
            if (ProvidenceConfigurationManager.RunDeleteUnassignedComponents)
            {
                services.AddSingleton<DeleteUnassignedComponentsJob>();
                services.AddSingleton(new JobSchedule(typeof(DeleteUnassignedComponentsJob), $"0 40 0/12 ? * * *")); // Run job every 12 hours
            }
            if (ProvidenceConfigurationManager.RunDeleteExpiredInternalJobs)
            {
                services.AddSingleton<DeleteExpiredInternalJobsJob>();
                services.AddSingleton(new JobSchedule(typeof(DeleteExpiredInternalJobsJob), $"0 50 0/12 ? * * *")); // Run job every 12 hours
            }

            // Add environment maintainance jobs
            if (ProvidenceConfigurationManager.RunAutoRefresh)
            {
                services.AddSingleton<RefreshEnvironmentsJob>();
                var executeIntervalInMinutes = ProvidenceConfigurationManager.AutoRefreshJobIntervalInSeconds / 60;
                services.AddSingleton(new JobSchedule(typeof(RefreshEnvironmentsJob), $"0 */{executeIntervalInMinutes} * ? * * *"));
            }
            if (ProvidenceConfigurationManager.RunAutoReset)
            {
                services.AddSingleton<ResetEnvironmentsJob>();
                var executeIntervalInMinutes = ProvidenceConfigurationManager.AutoResetJobIntervalInSeconds / 60;
                services.AddSingleton(new JobSchedule(typeof(ResetEnvironmentsJob), $"0 */{executeIntervalInMinutes} * ? * * *"));
            }

            // Add additional feature jobs
            if (ProvidenceConfigurationManager.EmailNotificationJobIntervalInSeconds > -1)
            {
                services.AddSingleton<EmailNotificationJob>();
                var executeIntervalInMinutes = ProvidenceConfigurationManager.EmailNotificationJobIntervalInSeconds / 60;
                services.AddSingleton(new JobSchedule(typeof(EmailNotificationJob), $"0 */{executeIntervalInMinutes} * ? * * *"));
            }
            if (ProvidenceConfigurationManager.StateIncreaseJobIntervalInSeconds > -1)
            {
                services.AddSingleton<StateIncreaseJob>();
                var executeIntervalInMinutes = ProvidenceConfigurationManager.StateIncreaseJobIntervalInSeconds / 60;
                services.AddSingleton(new JobSchedule(typeof(StateIncreaseJob), $"0 */{executeIntervalInMinutes} * ? * * *"));
            }
            if (ProvidenceConfigurationManager.UpdateDeploymentsJobIntervalInSeconds > -1)
            {
                services.AddSingleton<UpdateDeploymentsJob>();
                var executeIntervalInMinutes = ProvidenceConfigurationManager.UpdateDeploymentsJobIntervalInSeconds / 60;
                services.AddSingleton(new JobSchedule(typeof(UpdateDeploymentsJob), $"0 */{executeIntervalInMinutes} * ? * * *"));
            }

            // Start the internal Job processing job
            services.AddSingleton<ProcessInternalJobsJob>();
            services.AddSingleton(new JobSchedule(typeof(ProcessInternalJobsJob), $"0 */1 * ? * * *"));

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Daimler.Providence.Service.xml"));
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.FirstOrDefault());
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Providence API", Version = "v1" });
                c.CustomSchemaIds(x => x.FullName);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                //context.Response.Headers.Append("Cache-Control", string.Format("public,max-age={0}", TimeSpan.FromHours(12).TotalSeconds));
                context.Response.Headers.Append("Cache-control", "no-cache, no-store, must-revalidate");
                await next();

                if (context.Response.StatusCode == 404 &&
                    !Path.HasExtension(context.Request.Path.Value) &&
                    !context.Request.Path.Value.StartsWith("/api") &&
                    !context.Request.Path.Value.StartsWith("/swagger") &&
                    !context.Request.Path.Value.StartsWith("/signalr") &&
                    !context.Request.Path.Value.StartsWith("/help")
                    )
                {
                    context.Request.Path = "/index.html";
                    context.Response.StatusCode = 200;
                    await next();
                }
            });

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseCors("corspolicy");
            app.UseAuthentication();
            app.UseSwaggerAuthorized();
            app.UseAuthorization();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
                endpoints.MapHub<DeviceEventHub>("signalr", options =>
                {
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets | Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                });
            });

            if (ProvidenceConfigurationManager.EnableEventHubReader)
            {
                ConfigureEventHub(app.ApplicationServices);
            }
        }

        private void ConfigureEventHub(IServiceProvider serviceProvider)
        {
            Task.Factory.StartNew(() =>
            {
                
                try
                {
                    var eventProcessorFactory = serviceProvider.GetRequiredService(typeof(EventHubMessageReceiver)) as EventHubMessageReceiver;
                    eventProcessorFactory.StartEventListenerAsync().GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    AILogger.Log(SeverityLevel.Error, $"Error setting up EventProcessor: {e.Message}", exception: e);
                }
            });
        }
    }

    public static class AppBuilderExtension
    {

        public static IApplicationBuilder UseSwaggerAuthorized(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SwaggerOAuthMiddleware>();
        }
    }
}
