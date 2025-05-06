namespace DashBoard.Model
{
	public class RegisterRequest
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
	}

	public class LoginRequest
	{
		public string Username { get; set; }
		public string Password { get; set; }
	}

	public class ChangePasswordRequest
	{
		public string Username { get; set; }
		public string OldPassword { get; set; }
		public string NewPassword { get; set; }
	}

}
