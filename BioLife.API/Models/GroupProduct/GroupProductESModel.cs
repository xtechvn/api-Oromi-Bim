using Nest;

namespace HuloToys_Service.Models.ElasticSearch
{
    public partial class GroupProductModel
    {
        [PropertyName("id")]
        public int id { get; set; }
        [PropertyName("ParentId")]
        public int parentid { get; set; }
        [PropertyName("PositionId")]
        public int? positionid { get; set; }
        [PropertyName("Name")]
        public string name { get; set; } = null!;
        [PropertyName("ImagePath")]
        public string? imagepath { get; set; }
        [PropertyName("OrderNo")]
        public int? orderno { get; set; }
        [PropertyName("Path")]
        public string? path { get; set; }
        [PropertyName("Status")]
        public int? status { get; set; }
        [PropertyName("CreatedOn")]
        public DateTime? createdon { get; set; }
        [PropertyName("ModifiedOn")]
        public DateTime? modifiedon { get; set; }

        [PropertyName("Description")]
        public string? description { get; set; }
        [PropertyName("IsShowHeader")]
        public bool isshowheader { get; set; }
        [PropertyName("IsShowFooter")]
        public bool isshowfooter { get; set; }

        public List<GroupProductModel> group_product_child { get; set; }
    }
    public partial class GroupProductESModel
    {
        [PropertyName("Id")]

        public int Id { get; set; }
        [PropertyName("ParentId")]

        public int ParentId { get; set; }
        [PropertyName("PositionId")]

        public int? PositionId { get; set; }
        [PropertyName("Name")]

        public string Name { get; set; }
        [PropertyName("ImagePath")]

        public string ImagePath { get; set; }
        [PropertyName("OrderNo")]

        public int? OrderNo { get; set; }
        [PropertyName("Path")]

        public string Path { get; set; }
        [PropertyName("Status")]

        public int? Status { get; set; }
        [PropertyName("CreatedOn")]

        public DateTime? CreatedOn { get; set; }
        [PropertyName("ModifiedOn")]

        public DateTime? ModifiedOn { get; set; }
        [PropertyName("Priority")]

        public int? Priority { get; set; }
        [PropertyName("Description")]

        public string Description { get; set; }
        [PropertyName("IsShowHeader")]


        public bool IsShowHeader { get; set; }
        [PropertyName("IsShowFooter")]

        public bool IsShowFooter { get; set; }
        [PropertyName("Code")]

        public string? Code { get; set; }

        public List<GroupProductESModel> group_product_child { get; set; }

    }
}
