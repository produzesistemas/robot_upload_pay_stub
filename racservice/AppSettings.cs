
using Microsoft.Extensions.Configuration;

namespace racservice
{
    public static class AppSettings
    {
        public static IConfiguration Configuration { get; set; }
        public static string ConnectionString { get; set; }
    }
}
