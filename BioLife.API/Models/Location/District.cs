using Nest;

namespace HuloToys_Service.Models.Location
{
    public partial class District
    {
        [PropertyName("Id")]
        public int Id { get; set; }
        [PropertyName("DistrictId")]
        public string DistrictId { get; set; } = null!;
        [PropertyName("Name")]
        public string Name { get; set; } = null!;
        [PropertyName("NameNonUnicode")]
        public string? NameNonUnicode { get; set; }
        [PropertyName("Type")]
        public string Type { get; set; } = null!;
        [PropertyName("Location")]
        public string? Location { get; set; }
        [PropertyName("ProvinceId")]
        public string ProvinceId { get; set; } = null!;

    }
}
