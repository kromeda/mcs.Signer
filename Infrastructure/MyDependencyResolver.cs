using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;

namespace Signer.Infrastructure
{
    public class MyDependencyResolver : System.Web.Mvc.IDependencyResolver, IDependencyResolver
    {
        protected IServiceProvider serviceProvider;
        protected IServiceScope scope = null;

        public MyDependencyResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public MyDependencyResolver(IServiceScope scope)
        {
            this.scope = scope;
            this.serviceProvider = scope.ServiceProvider;
        }

        public IDependencyScope BeginScope()
        {
            return new MyDependencyResolver(serviceProvider.CreateScope());
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            scope?.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return this.serviceProvider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return this.serviceProvider.GetServices(serviceType);
        }
    }
}