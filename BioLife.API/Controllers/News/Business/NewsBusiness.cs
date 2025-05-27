using HuloToys_Service.ElasticSearch;
using HuloToys_Service.Models.Article;
using HuloToys_Service.Models.ElasticSearch;
using HuloToys_Service.Utilities.Lib;
using MongoDB.Driver;

namespace HuloToys_Service.Controllers.News.Business
{
    public partial class NewsBusiness
    {
        public IConfiguration configuration;
        public ArticleService article_service;
        
        public ArticleCategoryService articleCategoryESService;
        
        public GroupProductESService groupProductESService;
        public NewsBusiness(IConfiguration _configuration)
        {

            configuration = _configuration;
            article_service = new ArticleService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
        
            articleCategoryESService = new ArticleCategoryService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
            
            groupProductESService = new GroupProductESService(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
        }

        public async Task<ArticleModel> getArticleDetail(long article_id)
        {
            try
            {
                var article_list = article_service.GetArticleDetailById(article_id);
                return article_list;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetArticleCategoryByParentID -GroupProductRepository : " + ex);
            }
            return null;
        }


        /// <summary>
        /// cuonglv
        /// Lấy ra danh sách các bài thuộc 1 chuyên mục
        /// </summary>
        /// <param name="cate_id"></param>
        /// <returns></returns>
        //public async Task<List<CategoryArticleModel>> getArticleListByCategoryId(int cate_id,int size)
        //{
        //    var list_article = new List<CategoryArticleModel>();
        //    try
        //    {  
        //        // Lấy ra danh sách id các bài viết theo chuyên mục
        //        var List_articleCategory = articleCategoryESService.GetArticleIdByCategoryId(cate_id, size);
        //        if (List_articleCategory != null && List_articleCategory.Count > 0)
        //        {
        //            foreach (var item in List_articleCategory)
        //            {
        //                // Chi tiết bài viết
        //                var article = articleESService.GetArticleDetailForCategoryById((long)item.articleid);
        //                if (article == null)
        //                {
        //                    continue;
        //                }
        //                else
        //                {
        //                    list_article.Add(article);
        //                }
        //            }                    
        //        }
        //        return list_article;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "getArticleListByCategoryId - ArticleDAL: " + ex);
        //        return list_article;
        //    }
        //}


        /// <summary>
        /// cuonglv
        /// Lấy ra bài mới nhất
        /// </summary>
        /// <param name="category_parent_id">Là id cha của các chuyên mục  tin tức</param>
        /// <returns></returns>
        public async Task<List<CategoryArticleModel>> getListNews(int category_id, int take)
        {
            var list_article = new List<CategoryArticleModel>();
            try
            {
                // Lấy ra danh sách id các bài viết mới nhất
                var obj_top_story = article_service.getListNews( category_id,take);

                return obj_top_story;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "getArticleListByCategoryId - ArticleDAL: " + ex);
                return list_article;
            }
        }

        public List<GroupProductModel> GetByParentId(long parent_id)
        {
            try
            {
                var obj_cate = new List<GroupProductModel>();
                // List chuyên mục cha
                var obj_cate_parent = groupProductESService.GetListGroupProductByParentId(parent_id);

                foreach (var item in obj_cate_parent)
                {                  
                    var obj_cate_child = new List<GroupProductModel>();
                    var cate_child_detail =  groupProductESService.GetListGroupProductByParentId(item.id);
                    if (cate_child_detail.Count > 0)
                    {
                        foreach (var item_child in cate_child_detail)
                        {
                            var cate_child = new GroupProductModel
                            {
                                parentid = item.id,
                                id = item_child.id,
                                name = item_child.name,                                
                                imagepath = item_child.imagepath,
                                path = item_child.path,
                            };
                            obj_cate_child.Add(cate_child);
                        }                        
                    }
                    var cate_parent = new GroupProductModel
                    {
                        id = item.id,
                        name = item.name,
                        imagepath = item.imagepath,
                        path = item.path,
                        group_product_child = obj_cate_child
                    };
                    obj_cate.Add(cate_parent);
                }

                return obj_cate;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetByParentId-GroupProductDAL" + ex);
            }
            return null;
        }

        public GroupProductModel GetById(long id)
        {
            try
            {
                var data = groupProductESService.GetDetailGroupProductById(id);
                return data;

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetById-GroupProductDAL" + ex);

            }
            return null;
        }
       
        public async Task<List<GroupProductModel>> GetArticleCategoryByParentID(long parent_id)
        {
            try
            {
                var article_list = GetByParentId(parent_id).ToList();                
                return article_list;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetArticleCategoryByParentID -GroupProductRepository : " + ex);
            }
            return null;
        }
        //public async Task<List<GroupProductModel>> GetFooterCategoryByParentID(long parent_id)
        //{
        //    try
        //    {
        //        //var group = GetByParentId(parent_id);
        //        //group = group.Where(x => x.isshowfooter == true).ToList();
        //        //var list = new List<GroupProductModel>();

        //        // list.AddRange(group.Select(x => new ArticleGroupViewModel() { id = x.Id, image_path = x.ImagePath, name = x.Name, order_no = (int)x.OrderNo, url_path = x.Path }).OrderBy(x => x.order_no).ToList());
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "GetFooterCategoryByParentID -GroupProductRepository : " + ex);
        //    }
        //    return null;
        //}

        /// <summary>
        /// cuonglv
        /// Lấy ra tổng tin theo chuyên mục
        /// </summary>
        /// <param name="cate_id"></param>
        /// <returns></returns>
        public async Task<int> getTotalItemNewsByCategoryId(int cate_id)
        {
            var list_article = new List<CategoryArticleModel>();
            try
            {
                // Lấy ra danh sách id các bài viết theo chuyên mục
                var total = article_service.getTotalItemNewsByCategoryId(cate_id);
                
                return total;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], "getArticleListByCategoryId - ArticleDAL: " + ex);
                return 0;
            }
        }
        /// <summary>
        /// cuonglv
        /// Lọc những bài faq theo title
        /// </summary>
        /// <param name="article_id"></param>
        /// <returns></returns>
        public async Task<List<ArticleRelationModel>> FindArticleByTitle(string title, int parent_cate_faq_id)
        {
            try
            {
                var list_article = new List<ArticleRelationModel>();

                try
                {
                    var arr_cate_child_help_id = new List<int>();
                    var group_product_detail = groupProductESService.GetDetailGroupProductById(parent_cate_faq_id);
                    if (group_product_detail == null)
                    {
                        var group_product_list = groupProductESService.GetListGroupProductByParentId(parent_cate_faq_id);
                        arr_cate_child_help_id = group_product_list.Select(x => x.id).ToList();
                    }
                    arr_cate_child_help_id.Add(group_product_detail.id);



                    if (arr_cate_child_help_id.Count() > 0)
                    {
                        foreach (var item in arr_cate_child_help_id)
                        {
                            var groupProductName = string.Empty;
                            var DetailGroupProductById = groupProductESService.GetDetailGroupProductById(item);
                            if (DetailGroupProductById.isshowheader == true)
                            {
                                groupProductName += DetailGroupProductById.name + ",";
                            }
                            var List_articleCategory = articleCategoryESService.GetByCategoryId(DetailGroupProductById.id);
                            if (List_articleCategory != null && List_articleCategory.Count > 0)
                            {
                                foreach (var item2 in List_articleCategory)
                                {
                                    var detail_article = article_service.GetDetailById((long)item2.articleid);
                                    if (detail_article != null)
                                    {
                                        var ArticleRelation = new ArticleRelationModel
                                        {
                                            Id = detail_article.Id,
                                            Lead = detail_article.Lead,
                                            Image = detail_article.Image169 ?? detail_article.Image43 ?? detail_article.Image11,
                                            Title = detail_article.Title,
                                            publish_date = detail_article.PublishDate ?? DateTime.Now,
                                            category_name = groupProductName ?? "Tin tức"
                                        };
                                        list_article.Add(ArticleRelation);
                                    }

                                }
                            }

                        }
                        if (list_article.Count > 0)
                            list_article = list_article.Where(s => s.Title.ToUpper().Contains(title.ToUpper())).GroupBy(x => x.Id).Select(x => x.First()).OrderByDescending(x => x.publish_date).ToList();

                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception ex)
                {

                    LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], "[title = " + title + "]FindArticleByTitle - ArticleDAL: transaction.Commit " + ex);
                    return null;
                }

                return list_article;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], "[title = " + title + "]FindArticleByTitle - ArticleDAL:" + ex);

                return null;
            }
        }
        public async Task<ArticleFeModel> GetMostViewedArticle(long article_id)
        {
            try
            {

                var detail = await getArticleDetail(article_id);

                if (detail != null)
                {
                    //var group = GetById(detail.category_id);
                    //if (group != null && !group.isshowheader) return null;
                    var fe_detail = new ArticleFeModel()
                    {
                        id = detail.id,
                        lead = detail.lead,
                        publish_date = detail.publish_date,
                        title = detail.title,
                        image_11 = detail.image_11,
                        image_43 = detail.image_43,
                        image_169 = detail.image_169,
                        //article_type = detail.article_type,
                        category_name = detail.list_category_name,

                    };
                    return fe_detail;
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], "[API]ArticleRepository - GetMostViewedArticle: " + ex);
            }
            return null;
        }
    }
}
