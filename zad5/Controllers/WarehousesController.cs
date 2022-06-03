using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using zad5.Models;

namespace zad5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class WarehousesController : ControllerBase
    {
        private readonly IFileDbService _fileDbService;
        public WarehousesController(IFileDbService fileDbService)
        {
            _fileDbService = fileDbService;
        }

        [HttpPost]
        public async Task<IActionResult> AddAOrderAsync(Product_Warehouse product_Warehouse)
        {
            var addedProduct = await _fileDbService.AddOrderAsync(product_Warehouse);

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
