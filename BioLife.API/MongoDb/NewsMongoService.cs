using HuloToys_Service.Models.Article;
using HuloToys_Service.Utilities.Lib;
using MongoDB.Driver;

namespace HuloToys_Service.MongoDb
{
    public class NewsMongoService
    {
        private readonly IConfiguration _configuration;
        private IMongoCollection<NewsESView> _news_collection;

        public NewsMongoService(IConfiguration _configuration)
        {
            _configuration = _configuration;
            //mongodb://adavigolog_writer:adavigolog_2022@103.163.216.42:27017/?authSource=HoanBds
            string _connection = "mongodb://" + _configuration["DataBaseConfig:MongoServer:user"]
                    + ":" + _configuration["DataBaseConfig:MongoServer:pwd"]
                    + "@" + _configuration["DataBaseConfig:MongoServer:Host"]
                    + ":" + _configuration["DataBaseConfig:MongoServer:Port"]
                    + "/?authSource=" + _configuration["DataBaseConfig:MongoServer:catalog_log"];
            var booking = new MongoClient(_connection);
            IMongoDatabase db = booking.GetDatabase(_configuration["DataBaseConfig:MongoServer:catalog_log"]);
            _news_collection = db.GetCollection<NewsESView>("ArticlePageView");
        }
        public async Task<string> AddNewOrReplace(NewsViewCount model)
        {
            try
            {
                var filter = Builders<NewsESView>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<NewsESView>.Filter.Eq(x => x.articleID, model.articleID);
                var exists_model = await _news_collection.Find(filterDefinition).FirstOrDefaultAsync();
                if (exists_model != null && exists_model.articleID == model.articleID)
                {
                    var NewsES=new NewsESView();
                    NewsES._id = exists_model._id;
                    NewsES.pageview = exists_model.pageview + model.pageview;
                    NewsES.articleID = exists_model.articleID ;
                    await _news_collection.FindOneAndReplaceAsync(filterDefinition, NewsES);
                    return exists_model._id;
                }
                else
                {
                    model.GenID();
                    var NewsES = new NewsESView();
                    NewsES._id = model._id;
                    NewsES.pageview =  model.pageview;
                    NewsES.articleID = model.articleID;
                    await _news_collection.InsertOneAsync(NewsES);
                    return model._id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], "AddNewOrReplace - NewsMongoService: " + ex);
                return null;
            }
        }
        public async Task<List<NewsESView>> GetMostViewedArticle()
        {
            try
            {
                var filter = Builders<NewsESView>.Filter;
                var filterDefinition = filter.Empty;
                var list = await _news_collection.Find(filterDefinition).SortByDescending(x => x.pageview).ToListAsync();
                if (list != null && list.Count > 0)
                {
                    if (list.Count < 10) return list;
                    else return list.Skip(0).Take(10).ToList();
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], "GetMostViewedArticle - NewsMongoService: " + ex);
            }
            return null;
        }

    }
}
