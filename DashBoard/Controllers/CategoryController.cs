using DashBoard.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DashBoard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DbHelper _dbHelper;
        private readonly IWebHostEnvironment _env;


        public CategoryController(DbHelper dbHelper,IConfiguration configuration, IWebHostEnvironment env)
        {
            _dbHelper = dbHelper;
            _configuration = configuration;
            _env = env;
        }
        // INSERT
        [HttpPost("addCategory")]
        public IActionResult AddCategory([FromBody] Category category)
        {
            using var conn = _dbHelper.GetConnection();
            conn.Open();

            var cmd = new SqlCommand("INSERT INTO Category (Name) VALUES (@Name)", conn);
            cmd.Parameters.AddWithValue("@Name", category.Name);

            try
            {
                cmd.ExecuteNonQuery();
                return Ok(new { statuscode = 200, message = "Category added successfully." });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { statuscode = 400, message = ex.Message });
            }
        }

        // READ ALL
        [HttpGet("getAllCategories")]
        public IActionResult GetAllCategories()
        {
            try
            {
                using var conn = _dbHelper.GetConnection();
                conn.Open();

                var cmd = new SqlCommand("SELECT Id, Name FROM Category", conn);
                var reader = cmd.ExecuteReader();

                var categories = new List<Category>();

                while (reader.Read())
                {
                    categories.Add(new Category
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString()
                    });
                }

                if (categories.Count == 0)
                {
                    return Ok(new
                    {
                        statuscode = 404,
                        message = "No categories found.",
                        data = categories
                    });
                }

                return Ok(new
                {
                    statuscode = 200,
                    message = "Categories retrieved successfully.",
                    data = categories
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statuscode = 500,
                    message = "An error occurred while fetching categories.",
                    error = ex.Message
                });
            }
        }

        // UPDATE
        [HttpPut("updateCategory")]
        public IActionResult UpdateCategory([FromBody] Category category)
        {
            using var conn = _dbHelper.GetConnection();
            conn.Open();

            var cmd = new SqlCommand("UPDATE Category SET Name = @Name WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", category.Id);
            cmd.Parameters.AddWithValue("@Name", category.Name);

            try
            {
                var rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                    return Ok(new { statuscode = 200, message = "Category updated successfully." });
                else
                    return NotFound(new { statuscode = 404, message = "Category not found." });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { statuscode = 400, message = ex.Message });
            }
        }

        // DELETE
        [HttpDelete("deleteCategory/{id}")]
        public IActionResult DeleteCategory(int id)
        {
            using var conn = _dbHelper.GetConnection();
            conn.Open();

            var cmd = new SqlCommand("DELETE FROM Category WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            try
            {
                var rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                    return Ok(new { statuscode = 200, message = "Category deleted successfully." });
                else
                    return NotFound(new { statuscode = 404, message = "Category not found." });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { statuscode = 400, message = ex.Message });
            }
        }
        [HttpPost("insertSubCategory")]
        public async Task<IActionResult> InsertSubCategory(SubCategoryMaster model)
        {
            if (model.File == null || model.File.Length == 0)
                return BadRequest("File not selected.");

            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string filePath = Path.Combine(uploadFolder, model.File.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                model.File.CopyTo(stream);
            }

            // Your SQL logic here
            using var conn = _dbHelper.GetConnection();
            conn.Open();


            var cmd = new SqlCommand(@"INSERT INTO SubCategoryMaster (CategoryId, FileName, PaymentType, Amount, FilePath) 
                                   VALUES (@CategoryId, @FileName, @PaymentType, @Amount, @FilePath)", conn);
            cmd.Parameters.AddWithValue("@CategoryId", model.CategoryId);
            cmd.Parameters.AddWithValue("@FileName", model.FileName);
            cmd.Parameters.AddWithValue("@PaymentType", model.PaymentType);
            cmd.Parameters.AddWithValue("@Amount", model.Amount);
            cmd.Parameters.AddWithValue("@FilePath", filePath);

            try
            {
                cmd.ExecuteNonQuery();
                return Ok(new { statuscode = 200, message = "Subcategory inserted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statuscode = 500, message = ex.Message });
            }
        }

        // GET ALL
        [HttpGet("getAllSubCategory")]
        [HttpGet]
        public IActionResult GetAllSubCategory(int CategoryId)
        {
            try
            {
                using var conn = _dbHelper.GetConnection();
                conn.Open();

                var cmd = new SqlCommand("SELECT c.Name as CategoryName,scm.Id,scm.CategoryId,scm.FileName,scm.FilePath,scm.PaymentType,scm.Amount FROM SubCategoryMaster scm " +
                    "left join Category c on scm.CategoryId=c.Id where scm.CategoryId= @CategoryId", conn);
                cmd.Parameters.AddWithValue("@CategoryId", CategoryId);

                var reader = cmd.ExecuteReader();

                var list = new List<object>();

                while (reader.Read())
                {
                    list.Add(new
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        CategoryId = Convert.ToInt32(reader["CategoryId"]),
                        FileName = reader["FileName"]?.ToString(),
                        CategoryName = reader["CategoryName"]?.ToString(),
                        PaymentType = reader["PaymentType"]?.ToString(),
                        Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]) : 0,
                        FilePath = reader["FilePath"]?.ToString()
                    });
                }

                if (list.Count == 0)
                {
                    return Ok(new { statuscode = 404, message = "No sub-category records found.", data = new List<object>() });
                }

                return Ok(new { statuscode = 200, message = "Sub-category data fetched successfully.", data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statuscode = 500, message = "An error occurred while fetching data.", error = ex.Message });
            }
        }

        // UPDATE
        [HttpPut("updateSubCategory")]
        public IActionResult UpdateSubCategory(SubCategoryMaster model)
        {
            try
            {
                using var conn = _dbHelper.GetConnection();
                conn.Open();

                string? filePath = null;

                if (model.File != null && model.File.Length > 0)
                {
                    // Set upload folder path
                    string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

                    // Create directory if not exists
                    if (!Directory.Exists(uploadFolder))
                        Directory.CreateDirectory(uploadFolder);

                    // Generate unique file name to avoid conflicts
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.File.FileName;
                    filePath = Path.Combine(uploadFolder, uniqueFileName);

                    // Save file to server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        model.File.CopyTo(stream);
                    }
                }

                var cmd = new SqlCommand(@"
            UPDATE SubCategoryMaster SET 
                CategoryId = @CategoryId,
                FileName = @FileName,
                PaymentType = @PaymentType,
                Amount = @Amount
                " + (filePath != null ? ", FilePath = @FilePath " : "") + @"
            WHERE Id = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@CategoryId", model.CategoryId);
                cmd.Parameters.AddWithValue("@FileName", model.FileName ?? string.Empty);
                cmd.Parameters.AddWithValue("@PaymentType", model.PaymentType ?? string.Empty);
                cmd.Parameters.AddWithValue("@Amount", model.Amount);

                if (filePath != null)
                    cmd.Parameters.AddWithValue("@FilePath", filePath);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return Ok(new { statuscode = 200, message = "Subcategory updated successfully." });
                }
                else
                {
                    return NotFound(new { statuscode = 404, message = "Subcategory not found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statuscode = 500, message = "An error occurred.", error = ex.Message });
            }
        }


        // DELETE
        [HttpGet("deleteSubCategory/{id}")]
        public IActionResult DeleteSubCategory(int id)
        {
            using var conn = _dbHelper.GetConnection();
            conn.Open();

            var cmd = new SqlCommand("DELETE FROM SubCategoryMaster WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            try
            {
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                    return Ok(new { statuscode = 200, message = "Subcategory deleted successfully." });
                else
                    return NotFound(new { statuscode = 404, message = "Subcategory not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statuscode = 500, message = ex.Message });
            }
        }
		[HttpPost("insertOrder")]
		public async Task<IActionResult> InsertOrder(OrderDetails model)
		{
			using var conn = _dbHelper.GetConnection();

			if (model == null)
				return BadRequest("Invalid order details.");

			conn.Open();

			var cmd = new SqlCommand(@"INSERT INTO OrderDetails 
            (OrderPersonName, OrderPersonEmail, OrderPersonPhoneNo, PaymentId) 
            VALUES (@OrderPersonName, @OrderPersonEmail, @OrderPersonPhoneNo, @PaymentId)", conn);

			cmd.Parameters.AddWithValue("@OrderPersonName", model.OrderPersonName);
			cmd.Parameters.AddWithValue("@OrderPersonEmail", model.OrderPersonEmail);
			cmd.Parameters.AddWithValue("@OrderPersonPhoneNo", model.OrderPersonPhoneNo);
			cmd.Parameters.AddWithValue("@PaymentId", model.PaymentId);

			try
			{
				await cmd.ExecuteNonQueryAsync();
				return Ok(new { statuscode = 200, message = "Order inserted successfully." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { statuscode = 500, message = ex.Message });
			}
		}
	}
}
