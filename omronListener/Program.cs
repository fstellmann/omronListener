using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mobile.Communication.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace omronListener
{
    public class Program
    {
        public static readonly string reportOutput = "o5";
        public static readonly string robotPassword = "admin";
        public static readonly IO.Switch desiredOutput = IO.Switch.Off;

        public static void Main(string[] args)
        {
            internalSQL.connectionString = @"Server=localhost;Port=3306;Database=data;Uid=admin;Pwd=;";
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }
}
