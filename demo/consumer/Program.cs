using System;
using System.Collections.Generic;
using Confluent.Kafka;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using dal.Data;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace consumer
{
    class Program
    {

        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                
                try
                {
                    var context = services.GetRequiredService<SchoolContext>();
                    if (context.Database.EnsureCreated())
                    {
                        Console.WriteLine("Database Created");
                    } else
                    {
                        Console.WriteLine("Database Exists");
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
               Host.CreateDefaultBuilder(args)
                    .ConfigureLogging((context, logging) => {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                        logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning);
                    })
                   .ConfigureServices((hostContext, services) =>
                   {
                       var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
                       var configuration = builder.Build();
                       string conString = ConfigurationExtensions.GetConnectionString(configuration, "DefaultConnection");
                       var settings = configuration.GetSection("KafkaSettings").Get<KafkaSettings>();
                       services.AddSingleton(settings);
                       services.AddDbContext<SchoolContext>(options => options.UseNpgsql(conString));
                       services.AddHostedService<Worker>();
                   });
    }
}
