using Microsoft.Extensions.Configuration;
using Models;
using System;
using System.Linq;

namespace racservice.Services
{
    public class DbHelper
    {
        private AppDbContext dbContext { get; set; }
        private IConfiguration _configuration;
        public DbHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void CreateDocument(Document document)
        {
            using (dbContext = new AppDbContext(_configuration))
            {
                try
                {
                    dbContext.Add(document);
                    dbContext.SaveChanges();
                }
                catch (Exception)
                {
                    throw;
                }

            }
        }
        public int CreateLog(Log log)
        {
            using (dbContext = new AppDbContext(_configuration))
            {
                try
                {
                    dbContext.Add(log);
                    return dbContext.SaveChanges();
                }
                catch (Exception)
                {
                    throw;
                }

            }
        }

        public Establishment GetEstablishment(int code)
        {
            using (dbContext = new AppDbContext(_configuration))
            {
                try
                {
                    return dbContext.Establishment.FirstOrDefault(x => x.Code == code);
                }
                catch (Exception)
                {
                    throw;
                }

            }
        }
        public AspNetUsers GetAspNetUsers(int registration)
        {
            using (dbContext = new AppDbContext(_configuration))
            {
                try
                {
                    return dbContext.AspNetUsers.FirstOrDefault(x => x.Registration == registration);
                }
                catch (Exception)
                {
                    throw;
                }

            }
        }
    }


}
