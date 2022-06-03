using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using zad5.Models;

namespace zad5
{
    public interface IFileDbService
    {
       Task<int> AddOrderAsync(Product_Warehouse product_Warehouse);
       Task<int> AddOrder2Async(Product_Warehouse product_Warehouse);
    }
}
