namespace DashBoard.Model
{
    public class Category
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
    }
    public class SubCategoryMaster
    {
        public int? Id { get; set; }
        public int CategoryId { get; set; }
        public string FileName { get; set; }
        public string PaymentType { get; set; }
        public decimal? Amount { get; set; }
        public IFormFile File { get; set; }
    }

}
