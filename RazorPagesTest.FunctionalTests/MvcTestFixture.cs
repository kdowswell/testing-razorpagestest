using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.TestHost;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace RazorPagesTest.FunctionalTests
{
    public class MvcTestFixture<TStartup> : IDisposable
    {
        private readonly TestServer _server;

        public MvcTestFixture()
            : this(Path.Combine(""))
        {
        }

        protected MvcTestFixture(string solutionRelativePath)
        {
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;
            var contentRoot = SolutionPathUtility.GetProjectPath(solutionRelativePath, startupAssembly);

            //var builder = new WebHostBuilder()
            //    .UseContentRoot(contentRoot)
            //    .ConfigureServices(InitializeServices)
            //    .UseStartup(typeof(TStartup));

            var builder = new WebHostBuilder()
                .UseContentRoot(contentRoot)
                .ConfigureLogging(factory =>
                {
                    //factory.AddConsole();
                })
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    services.Configure((RazorViewEngineOptions options) =>
                    {
                        var previous = options.CompilationCallback;
                        options.CompilationCallback = (context) =>
                        {
                            previous?.Invoke(context);

                            var assembly = typeof(Startup).GetTypeInfo().Assembly;
                            var assemblies = assembly.GetReferencedAssemblies().Select(x => MetadataReference.CreateFromFile(Assembly.Load(x).Location))
                                .ToList();

                            //assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Linq")).Location));
                            assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Microsoft.AspNetCore.Razor")).Location));
                            assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("mscorlib")).Location));
                            assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Microsoft.AspNetCore.Razor.Runtime")).Location));
                            assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Microsoft.AspNetCore.Mvc")).Location));
                            assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Microsoft.AspNetCore.Razor")).Location));
                            assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Microsoft.AspNetCore.Mvc.Razor")).Location));
                            assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Microsoft.AspNetCore.Html.Abstractions")).Location));
                            assemblies.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Text.Encodings.Web")).Location));

                            context.Compilation = context.Compilation.AddReferences(assemblies);
                        };
                    });
                });

            _server = new TestServer(builder);

            Client = _server.CreateClient();
            Client.BaseAddress = new Uri("http://localhost");
        }

        public HttpClient Client { get; }

        public void Dispose()
        {
            Client.Dispose();
            _server.Dispose();
        }

        protected virtual void InitializeServices(IServiceCollection services)
        {
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;

            // Inject a custom application part manager. Overrides AddMvcCore() because that uses TryAdd().
            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new AssemblyPart(startupAssembly));

            manager.FeatureProviders.Add(new ControllerFeatureProvider());
            manager.FeatureProviders.Add(new ViewComponentFeatureProvider());

            services.AddSingleton(manager);
        }
    }
}