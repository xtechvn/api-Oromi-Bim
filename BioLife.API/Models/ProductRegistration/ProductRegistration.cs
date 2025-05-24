namespace HuloToys_Service.Models.ProductRegistration
{
    public class ProductRegistration
    {
        public int id { get; set; }
        public long ProductId { get; set; }
        public long DistrictId { get; set; }
        public long ProvinceId { get; set; }
        public string Phone { get; set; }
        public string FullName { get; set; }
        public string Note { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long UpdatedBy { get; set; }
    }
}
