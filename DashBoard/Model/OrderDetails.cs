namespace DashBoard.Model
{
	public class OrderDetails
	{
		public int? Id { get; set; } // Auto-increment primary key
		public string OrderPersonName { get; set; }
		public string OrderPersonEmail { get; set; }
		public string OrderPersonPhoneNo { get; set; }
		public string PaymentId { get; set; }
	}
}
