using DashBoard.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Resources;
using System.Security.Cryptography;
using System.Text;

namespace DashBoard.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Consumes("application/json")]
	public class HomeController : ControllerBase
	{
		private readonly DbHelper _dbHelper;

		public HomeController(DbHelper dbHelper)
		{
			_dbHelper = dbHelper;
		}
		[HttpPost("register")]
		public IActionResult Register(RegisterRequest request)
		{
			using var conn = _dbHelper.GetConnection();
			conn.Open();

			var passwordHash = HashPassword(request.Password);

			var cmd = new SqlCommand("INSERT INTO UsersData (Username, PasswordHash, Email) VALUES (@Username, @PasswordHash, @Email)", conn);
			cmd.Parameters.AddWithValue("@Username", request.Username);
			cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
			cmd.Parameters.AddWithValue("@Email", request.Email);

			try
			{
				cmd.ExecuteNonQuery();
				return Ok(new { statuscode = 200, message = "User registered successfully." });
				//return StatusCode(200, "User registered successfully.");  // 201 Created

			}
			catch (SqlException ex)
			{
				return Ok(new { statuscode = 400, message = ex.Message});

				//return StatusCode(400, $"Error: {ex.Message}");            // 400 Bad Request
			}
		}

		[HttpPost("login")]
		public IActionResult Login(LoginRequest request)
		{
			using var conn = _dbHelper.GetConnection();
			conn.Open();

			var cmd = new SqlCommand("SELECT PasswordHash FROM UsersData WHERE Username = @Username", conn);
			cmd.Parameters.AddWithValue("@Username", request.Username);

			var reader = cmd.ExecuteReader();
			if (reader.Read())
			{
				var storedHash = reader.GetString(0);

				if (VerifyPassword(request.Password, storedHash))
					//return StatusCode(200, "Login successful."); // 200 OK
					return Ok(new { statuscode = 200, message = "Login successful." });

				else
					//return StatusCode(401, "Invalid credentials."); // 401 Unauthorized
				return Ok(new { statuscode = 401, message = "Invalid credentials." });

			}
			else
			{
				//return StatusCode(404, "User not found."); // 404 Not Found
				return Ok(new { statuscode = 404, message = "User not found." });

			}
		}
        [HttpPost("change-password")]
        public IActionResult ChangePassword(ChangePasswordRequest request)
        {
            using var conn = _dbHelper.GetConnection();
            conn.Open();

            // Verify old password
            var cmdCheck = new SqlCommand("SELECT PasswordHash FROM UsersData WHERE Username = @Username", conn);
            cmdCheck.Parameters.AddWithValue("@Username", request.Username);

            var reader = cmdCheck.ExecuteReader();
            if (!reader.Read())
            {
                return Ok(new { statuscode = 404, message = "User not found." });
            }

            var storedHash = reader.GetString(0);
            if (!VerifyPassword(request.OldPassword, storedHash))
            {
                return Ok(new { statuscode = 401, message = "Old password is incorrect." });
            }

            reader.Close();

            // Update new password
            var newPasswordHash = HashPassword(request.NewPassword);
            var cmdUpdate = new SqlCommand("UPDATE UsersData SET PasswordHash = @NewPassword WHERE Username = @Username", conn);
            cmdUpdate.Parameters.AddWithValue("@NewPassword", newPasswordHash);
            cmdUpdate.Parameters.AddWithValue("@Username", request.Username);

            cmdUpdate.ExecuteNonQuery();

            return Ok(new { statuscode = 200, message = "Password changed successfully." });
        }

        private static string HashPassword(string password)
		{
			using var sha256 = SHA256.Create();
			var bytes = Encoding.UTF8.GetBytes(password);
			var hash = sha256.ComputeHash(bytes);
			return Convert.ToBase64String(hash);
		}

		private static bool VerifyPassword(string password, string storedHash)
		{
			var hashOfInput = HashPassword(password);
			return hashOfInput == storedHash;
		}
	}
}
