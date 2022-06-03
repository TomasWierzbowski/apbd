using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using zad5.Models;

namespace zad5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class Warehouses2Controller : ControllerBase
    {

        private readonly IFileDbService _fileDbService;
        public Warehouses2Controller(IFileDbService fileDbService)
        {
            _fileDbService = fileDbService;
        }

        [HttpPost]
        public async Task<IActionResult> AddAOrder2Async(Product_Warehouse product_Warehouse)
        {
            var addedProduct = await _fileDbService.AddOrder2Async(product_Warehouse);

            if (addedProduct != 0)
            {
                return Created("Zarejestrowano w hurtowni produkt o ID: ", addedProduct);
            }
            else
            {
                // dodaj poszczegolne inne komunikaty
                return BadRequest("Nie udało się zarejestrować produktu w hurtowni");
            }
        }

    }
}
