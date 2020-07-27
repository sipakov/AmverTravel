using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Amver.Api.Implementations;
using Amver.Api.Implementations.Services;
using Amver.Api.Implementations.Storages;
using Amver.Api.Interfaces;
using Amver.Api.Interfaces.Services;
using Amver.Api.Interfaces.Storages;
using Amver.Api.RealTimeCommunication.SignalR;
using Amver.Domain.Constants;
using Amver.EfCli;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amver.Api.CustomExceptionMiddleware.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using CustomRequestCultureProvider = Amver.Api.Implementations.CustomRequestCultureProvider;

namespace Amver.Api
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddTransient<ITripService, TripService>();
            services.TryAddTransient<ITripStorage, TripStorage>();
            services.TryAddTransient<IUserService, UserService>();
            services.TryAddTransient<IUserStorage, UserStorage>();
            services.TryAddTransient<IConversationService, ConversationService>();
            services.TryAddTransient<IConversationStorage, ConversationStorage>();
            services.TryAddTransient<IMessageService, MessageService>();
            services.TryAddTransient<IMessageStorage, MessageStorage>();
            services.TryAddTransient<ICityService, CityService>();
            services.TryAddTransient<ICountryService, CountryService>();
            services.TryAddTransient<ICountryStorage, CountryStorage>();
            services.TryAddTransient<IFavouriteTripService, FavouriteTripService>();
            services.TryAddTransient<IFavouriteTripStorage, FavouriteTripStorage>();
            services.TryAddTransient<IAuthService, AuthService>();
            services.TryAddTransient<IAuthStorage, AuthStorage>();
            services.TryAddTransient<IPasswordEncryptor, PasswordEncryptor>();
            services.TryAddTransient<IAccountService, AccountService>();
            services.TryAddTransient<IProfileService, ProfileService>();
            services.TryAddTransient<ICustomRequestCultureProvider, Implementations.CustomRequestCultureProvider>();

            services.TryAddTransient<IContextFactory<ApplicationContext>, ApplicationContextFactory>();
            
            var supportedCultures = new[]
            {
                new CultureInfo("ru"),
                new CultureInfo("en")
            };

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new AcceptLanguageHeaderRequestCultureProvider()
                };
            });

            services.AddLocalization(options => options.ResourcesPath = "Localization");

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.KnownProxies.Add(IPAddress.Parse("84.201.184.247"));
            });
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.Issuer,
                        ValidateAudience = true,
                        ValidAudience = AuthOptions.Audience,
                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.SymmetricSecurityKey,
                        ValidateIssuerSigningKey = true
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/chat")))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            
            services.AddSignalR();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRequestLocalization();
            app.UseCustomExceptionMiddleware();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();               
                endpoints.MapHub<ChatHub>("/chat", options =>
                {
                    options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
                });
            });
        }
    }
}
