using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Sample.ActiveMQ.MySql.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly ICapPublisher _capBus;

        public ValuesController(ICapPublisher capPublisher)
        {
            _capBus = capPublisher;
        }

        #region pubblish

        [Route("~/without/transaction")]
        public async Task<IActionResult> WithoutTransaction(string name = "sample.activemq.mysql")
        {

            await _capBus.PublishAsync(name, DateTime.Now);

            return Ok();
        }

        [Route("~/adonet/transaction")]
        public IActionResult AdonetWithTransaction()
        {
            using (var connection = new MySqlConnection(AppDbContext.ConnectionString))
            {
                using (var transaction = connection.BeginTransaction(_capBus, autoCommit: false))
                {
                    //your business code
                    connection.Execute("insert into test(name) values('test')", transaction: (IDbTransaction)transaction.DbTransaction);

                    for (int i = 0; i < 5; i++)
                    {
                        _capBus.Publish("sample.activemq.mysql", DateTime.Now);
                    }

                    transaction.Commit();
                }
            }

            return Ok();
        }

        [Route("~/ef/transaction")]
        public IActionResult EntityFrameworkWithTransaction([FromServices]AppDbContext dbContext)
        {
            using (var trans = dbContext.Database.BeginTransaction(_capBus, autoCommit: false))
            {
                dbContext.Persons.Add(new Person() { Name = "ef.transaction" });

                for (int i = 0; i < 5; i++)
                {
                    _capBus.Publish("sample.activemq.mysql", DateTime.Now);
                }

                dbContext.SaveChanges();

                trans.Commit();
            }
            return Ok();
        }

        #endregion

        #region subscribers

        [NonAction]
        [CapSubscribe("sample.activemq.mysql")]
        public bool Subscriber(DateTime time)
        {
            Console.WriteLine($@"{DateTime.Now}, Subscriber invoked, Sent time:{time}");
            return true;
        }

        [NonAction]
        [CapSubscribe("sample.activemq.mysql")]
        public bool Subscriber2(DateTime time)
        {
            Console.WriteLine($@"{DateTime.Now}, Subscriber2 invoked, Sent time:{time}");
            return true;
        }

        [NonAction]
        [CapSubscribe("#.activemq.mysql")]
        public bool Subscriber3(DateTime time)
        {
            Console.WriteLine($@"{DateTime.Now}, Subscriber3 invoked, Sent time:{time}");
            return true;
        }

        [NonAction]
        [CapSubscribe("sample.activemq.mysql", Group = "another")]
        public bool Subscriber4(DateTime time)
        {
            Console.WriteLine($@"{DateTime.Now}, Subscriber4 invoked, Sent time:{time}");
            return true;
        }

        [NonAction]
        [CapSubscribe("a.activemq.mysql", Group = "another")]
        public bool Subscriber5(DateTime time)
        {
            Console.WriteLine($@"{DateTime.Now}, Subscriber5 invoked, Sent time:{time}");
            return true;
        }

        [NonAction]
        [CapSubscribe("a.activemq.mysql.1", Group = "another")]
        public bool Subscriber6(DateTime time)
        {
            Console.WriteLine($@"{DateTime.Now}, Subscriber6 invoked, Sent time:{time}");
            return true;
        }

        #endregion
    }
}
