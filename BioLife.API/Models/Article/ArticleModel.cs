using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Nest;

namespace HuloToys_Service.Models.Article
{
    public class ArticleModel:CategoryArticleModel // Kế thừa thuộc tính của box tin
    {
        [PropertyName("Body")]
        public string body { get; set; }
    }
    public class ArticleRelationModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public DateTime publish_date { get; set; }
        public string category_name { get; set; }
        public string? Lead { get; set; }

    }
    public class ArticleFeModel
    {
        public long id { get; set; }
        public string category_name { get; set; }
        public string title { get; set; }
        public string lead { get; set; }
        public string image_169 { get; set; }
        public string image_43 { get; set; }
        public string image_11 { get; set; }
        public string body { get; set; }
        public DateTime publish_date { get; set; }
        public DateTime update_last { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public int article_type { get; set; }
        public short? position { get; set; }
        public int status { get; set; }
        public string category_id { get; set; }
    }
    public class NewsViewCount
    {
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public void GenID()
        {
            _id = ObjectId.GenerateNewId().ToString();
        }
        public long articleID { get; set; }
        public long pageview { get; set; }
    }
    public class NewsESView
    {
    
        public string _id { get; set; }
       
        public long articleID { get; set; }
        public long pageview { get; set; }
    }
}
