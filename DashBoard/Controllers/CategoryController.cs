using DashBoard.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DashBoard.Controllers
{
    public class CategoryController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DbHelper _dbHelper;

        public CategoryController(DbHelper dbHelper,IConfiguration configuration)
        {
            _dbHelper = dbHelper;
            _configuration = configuration;
        }
        // INSERT
        [HttpPost("add")]
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
        [HttpGet("list")]
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
        [HttpPut("update")]
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
        [HttpDelete("delete/{id}")]
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
    }
}
