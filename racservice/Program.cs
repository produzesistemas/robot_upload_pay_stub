using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace racservice
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            
            IConfiguration config = new ConfigurationBuilder()
.AddJsonFile(@"C:\robot_rac\appsettings.json", true, true)
.Build();

            using (var service = new racservice(config))
            {
                ServiceBase.Run(service);
            }

        }


    }
}
