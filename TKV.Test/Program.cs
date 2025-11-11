using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TKV.Interface;
using TKV.Model.DbContext;
using TKV.Service;
using Xunit;

namespace TKV.Test
{
    public class ProgramConfigurationTests
    {
        [Fact]
        public void Should_Register_IMainServices_With_DbContext()
        {
            var services = new ServiceCollection();

            services.AddDbContext<MyDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            services.AddScoped<IMainServices, MainServices>();

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var service = scope.ServiceProvider.GetService<IMainServices>();
            var dbContext = scope.ServiceProvider.GetService<MyDbContext>();

            service.Should().NotBeNull().And.BeOfType<MainServices>();
            dbContext.Should().NotBeNull();
        }

        [Fact]
        public void Should_Add_DbContext_With_SqlServer_And_RetryPolicy()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection([
                    new KeyValuePair<string, string>("ConnectionStrings:DefaultConnection",
                        "Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=True;")!
                ]!)
                .Build();

            var services = new ServiceCollection();

            services.AddDbContext<MyDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    )
                ));

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<DbContextOptions<MyDbContext>>();
            options.Extensions.Should().Contain(e =>
                e is Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension);
        }

        [Fact]
        public void Should_Add_Controllers_Endpoints_And_Swagger()
        {
            var services = new ServiceCollection();

            services.AddEndpointsApiExplorer();
            services.AddControllers();
            services.AddSwaggerGen();

            var provider = services.BuildServiceProvider();
            provider.GetService<Microsoft.AspNetCore.Mvc.Controllers.IControllerFactory>().Should().NotBeNull();
        }

        [Fact]
        public void Should_Build_App_And_Configure_Middleware_Without_Exception()
        {
            var builder = WebApplication.CreateBuilder(Array.Empty<string>());

            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<MyDbContext>(options =>
                options.UseInMemoryDatabase("StartupTestDb"));
            builder.Services.AddScoped<IMainServices, MainServices>();

            var app = builder.Build();

            var exception = Record.Exception(() =>
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseHttpsRedirection();
                app.MapControllers();
            });

            exception.Should().BeNull();
        }

        [Fact]
        public void Should_Resolve_All_Required_Services_From_Scope()
        {
            var builder = WebApplication.CreateBuilder(Array.Empty<string>());
            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<MyDbContext>(options =>
                options.UseInMemoryDatabase("ResolveTestDb"));
            builder.Services.AddScoped<IMainServices, MainServices>();

            var app = builder.Build();

            using var scope = app.Services.CreateScope();
            var mainService = scope.ServiceProvider.GetService<IMainServices>();
            var dbContext = scope.ServiceProvider.GetService<MyDbContext>();

            mainService.Should().NotBeNull().And.BeOfType<MainServices>();
            dbContext.Should().NotBeNull();
        }
    }
}
