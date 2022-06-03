
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using zad5.Models;

namespace zad5.Services
{
    public class FileDbService: IFileDbService
    {
        private readonly IConfiguration _configuration;
        public FileDbService(IConfiguration configuration) {
            _configuration = configuration;
        }

        public async Task<int> AddOrderAsync(Product_Warehouse product_Warehouse) {
            var newlyCreatedRowId = 0;

            // check if all needed values are set and if amount > 0

            if (String.IsNullOrEmpty(product_Warehouse.IdProduct.ToString()) || String.IsNullOrEmpty(product_Warehouse.IdWarehouse.ToString()) || String.IsNullOrEmpty(product_Warehouse.Amount.ToString()) || product_Warehouse.Amount <= 0 || String.IsNullOrEmpty(product_Warehouse.CreatedAt.ToString()))
            {
                return newlyCreatedRowId;
            }

            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultDbCon")))
                {

                //check if we have product with specified id
                var com = new SqlCommand();
                com.Connection = con;
                com.CommandText = "SELECT Count(*) FROM Product WHERE IdProduct=@param1";
                com.Parameters.AddWithValue("@param1", product_Warehouse.IdProduct);

                await con.OpenAsync();
                var tran = await con.BeginTransactionAsync();

                try
                {
                com.Transaction = (SqlTransaction)tran;

                var noOfRows = await com.ExecuteScalarAsync();
                    if (Convert.ToInt32(noOfRows) == 0) {
                        await tran.RollbackAsync();
                        return newlyCreatedRowId;
                    }

                    // get product price
                    var productPrice = new List<decimal>();
                    com.Parameters.Clear();
                    com.CommandText = "SELECT Price FROM Product WHERE IdProduct=@param1";
                    com.Parameters.AddWithValue("@param1", product_Warehouse.IdProduct);
                    SqlDataReader dr = await com.ExecuteReaderAsync();
                    while (dr.Read())
                    {
                        productPrice.Add((decimal)dr[0]);
                    }
                    dr.Close();

                    // check if we have warehouse with specified id
                    com.Parameters.Clear();
                    com.CommandText = "SELECT Count(*) FROM Warehouse WHERE IdWarehouse=@param1";
                    com.Parameters.AddWithValue("@param1", product_Warehouse.IdWarehouse);
                    noOfRows = await com.ExecuteScalarAsync();
                    if (Convert.ToInt32(noOfRows) == 0) {
                        await tran.RollbackAsync();
                        return newlyCreatedRowId;
                    }

                    // check if we have all the needed info in Order table and if order createdAt date is < than createdAt date from our request
                    var res = new List<DateTime>();

                    com.Parameters.Clear();
                    com.CommandText = "SELECT CreatedAt FROM \"Order\" WHERE IdProduct=@param1 AND Amount=@param2";
                    com.Parameters.AddWithValue("@param1", product_Warehouse.IdProduct);
                    com.Parameters.AddWithValue("@param2", product_Warehouse.Amount);

                    dr = await com.ExecuteReaderAsync();
                    while (dr.Read())
                    {
                        res.Add((DateTime)dr[0]);
                    }
                    dr.Close();

                    if ((res == null) || (res.Count != 1)) {
                        await tran.RollbackAsync();
                        return newlyCreatedRowId;
                    }
                    if (res[0] > product_Warehouse.CreatedAt) {
                        await tran.RollbackAsync();
                        return newlyCreatedRowId;
                    }

                    // get orderId
                    var IdList = new List<int>();
                    com.Parameters.Clear();
                    com.CommandText = "SELECT IdOrder FROM \"Order\" WHERE IdProduct=@param1 AND Amount=@param2";
                    com.Parameters.AddWithValue("@param1", product_Warehouse.IdProduct);
                    com.Parameters.AddWithValue("@param2", product_Warehouse.Amount);
                    dr = await com.ExecuteReaderAsync();
                    while (dr.Read())
                    {
                        IdList.Add((int)dr[0]);
                    }
                    dr.Close();

                    // check if order hasnt been realized
                    com.Parameters.Clear();
                    com.CommandText = "SELECT Count(*) FROM Product_Warehouse WHERE IdOrder = @param1";
                    com.Parameters.AddWithValue("@param1", IdList[0]);
                    noOfRows = await com.ExecuteScalarAsync();
                    if (Convert.ToInt32(noOfRows) != 0) {
                        await tran.RollbackAsync();
                        return newlyCreatedRowId;
                    }

                    // update Order table
                    com.Parameters.Clear();
                    com.CommandText = "UPDATE \"Order\" set FulfilledAt = @param1 WHERE IdOrder = @param2";
                    com.Parameters.AddWithValue("@param1", DateTime.Now);
                    com.Parameters.AddWithValue("@param2", IdList[0]);
                    await com.ExecuteNonQueryAsync();

                    // add
                    var finalPrice = product_Warehouse.Amount * productPrice[0];

                    com.Parameters.Clear();
                    com.CommandText = $"INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)" +
                    $"VALUES (@param1, @param2, @param3,  @param4, @param5, @param6)";
                    com.Parameters.AddWithValue("@param1", product_Warehouse.IdWarehouse);
                    com.Parameters.AddWithValue("@param2", product_Warehouse.IdProduct);
                    com.Parameters.AddWithValue("@param3", IdList[0]);
                    com.Parameters.AddWithValue("@param4", product_Warehouse.Amount);
                    com.Parameters.AddWithValue("@param5", finalPrice);
                    com.Parameters.AddWithValue("@param6", DateTime.Now);

                    await com.ExecuteNonQueryAsync();

                    await tran.CommitAsync();

                    // get newly created item id
                    com.Parameters.Clear();
                    com.CommandText = "SELECT IDENT_CURRENT('Product_Warehouse')";
                    newlyCreatedRowId = Convert.ToInt32(await com.ExecuteScalarAsync());
                }
                catch (SqlException exc)
                {
                    await tran.RollbackAsync();
            }
            }
            return newlyCreatedRowId;
        }

        public async Task<int> AddOrder2Async(Product_Warehouse product_Warehouse)
        {
            using (var con = new SqlConnection(_configuration.GetConnectionString("DefaultDbCon")))
            {

                var com = new SqlCommand("AddProductToWarehouse", con);
                com.Parameters.AddWithValue("@IdProduct", product_Warehouse.IdProduct);
                com.Parameters.AddWithValue("@IdWarehouse", product_Warehouse.IdWarehouse);
                com.Parameters.AddWithValue("@Amount", product_Warehouse.Amount);
                com.Parameters.AddWithValue("@CreatedAt", product_Warehouse.CreatedAt);
                com.Connection = con;
                com.CommandType = CommandType.StoredProcedure;

                await con.OpenAsync();
                var newlyCreatedRowId = await com.ExecuteScalarAsync();
                return Convert.ToInt32(newlyCreatedRowId);
            }
            }

    }
}
