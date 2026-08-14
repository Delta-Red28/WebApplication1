using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class PasswordController : Controller
    {
        public IActionResult Index()
        {
            var hashes = new Dictionary<string, string>
            {
                { "admin", PasswordService.Hash("Admin@123") },
                { "gerente", PasswordService.Hash("Gerente@123") },
                { "cajero", PasswordService.Hash("Cajero@123") },
                { "mesero", PasswordService.Hash("Mesero@123") },
                { "cocina", PasswordService.Hash("Cocina@123") }
            };

            return Content(
                string.Join(
                    Environment.NewLine + Environment.NewLine,
                    hashes.Select(x => $"{x.Key} = {x.Value}")
                )
            );
        }
    }
}