using Microsoft.Data.SqlClient;

namespace DashBoard.Model
{
	public class DbHelper
	{
		private readonly string _connectionString;

		public DbHelper(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection");
		}

		public SqlConnection GetConnection()
		{
			return new SqlConnection(_connectionString);
		}
	}
}
