using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using Aws.Dynamo.Exercise.Models;

namespace Aws.Dynamo.Exercise.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDynamoDBContext DbContext;

        public HomeController(IDynamoDBContext dbContext)
        {
            this.DbContext = dbContext;
        }

        public async Task<IActionResult> AddBook(int id)
        {
            Book newBook = new Book
            {
                Id = id,
                Title = "化物語 1（首刷限定版）",
                ISBN = "4710945559346",
                BookAuthors = new List<string> {"西尾 維新", "大暮 維人"}
            };

            await this.DbContext.SaveAsync(newBook);

            Book savedBook = await this.DbContext.LoadAsync<Book>(id);

            return this.Json(savedBook);
        }

        public async Task<IActionResult> GetBook(int id)
        {
            Book book = await this.DbContext.LoadAsync<Book>(id);

            return this.Json(book);
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}