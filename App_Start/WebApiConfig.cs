using Microsoft.Extensions.DependencyInjection;
using Signer.Controllers;
using Signer.Infrastructure;
using Signer.Models;
using Signer.Services;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace Signer
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Formatters.Add(new JsonMediaTypeFormatter());

            var services = new ServiceCollection();
            services.AddTransient<IFileSigner, CryptoProSigner>();
            services.AddTransient<SignController>();

            var resolver = new MyDependencyResolver(services.BuildServiceProvider());
            config.DependencyResolver = resolver;
        }
    }
}