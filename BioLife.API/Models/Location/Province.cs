using MongoDB.Bson.Serialization.Attributes;
using Nest;

namespace HuloToys_Service.Models.Location
{
    public partial class Province
    {
        [PropertyName("Id")]
        public long Id { get; set; }
        [PropertyName("ProvinceId")]
        public string ProvinceId { get; set; } = null!;
        [PropertyName("Name")]
        public string Name { get; set; } = null!;
        [PropertyName("NameNonUnicode")]
        public string NameNonUnicode { get; set; }
        [PropertyName("Type")]
        public string Type { get; set; } = null!;

    }
}
