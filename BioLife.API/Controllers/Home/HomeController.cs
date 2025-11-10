using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.RedisWorker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Utilities.Contants;
using Utilities;
using HuloToys_Service.Utilities.Lib;
using Newtonsoft.Json;
using HuloToys_Service.Models.Home;
using HuloToys_Service.IRepositories;
using Microsoft.AspNetCore.Authorization;

namespace HuloToys_Service.Controllers.Home
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HomeController : ControllerBase
    {
        private readonly RedisConn _redisService;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IConfiguration _configuration;
        public HomeController(RedisConn redisService, IAllCodeRepository allCodeRepository, IConfiguration configuration)
        {
            _redisService = redisService;
            _redisService.Connect();
            _allCodeRepository = allCodeRepository;
            _configuration = configuration;
        }
        [HttpPost("banner")]

        public async Task<ActionResult> GetBanner([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, _configuration["KEY:private_key"]))
                {
                    var cache_name = CacheType.OMORI_HOMEPAGE_SLIDE;
                    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                    HomepageBannerModel result = new HomepageBannerModel();
                    if (j_data != null && j_data.Trim() != "")
                    {
                        result = JsonConvert.DeserializeObject<HomepageBannerModel>(j_data);
                        if (result != null && result.main != null && result.main.Count > 0)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = "Success",
                                data = "",
                                main_slide = result.main.Select(x => new { x.Id, x.OrderNo, x.Description }),
                                sub_banner = result.sub.Select(x => new { x.Id, x.OrderNo, x.Description }),
                                trending_main = result.trending_main.Select(x => new { x.Id, x.OrderNo, x.Description }),
                            });
                        }
                    }
                    var slide = _allCodeRepository.GetListByType("HOMEPAGE_SLIDE");
                    var sub = _allCodeRepository.GetListByType("HOMEPAGE_SUBBANNER");
                    var trending_main = _allCodeRepository.GetListByType("HOMEPAGE_SUPPLIER");
                  
                    result = new HomepageBannerModel()
                    {
                        main = slide == null ? new List<Models.Models.AllCode>() : slide.Where(x => x.Description != null && x.Description.Trim() != "").ToList(),
                        sub = sub == null ? new List<Models.Models.AllCode>() : sub.Where(x => x.Description != null && x.Description.Trim() != "").ToList(),
                        trending_main = trending_main == null ? new List<Models.Models.AllCode>() : trending_main.Where(x => x.Description != null && x.Description.Trim() != "").ToList(),
                    };
                    if (slide != null && slide.Count > 0)
                    {

                        _redisService.Set(cache_name, JsonConvert.SerializeObject(result), Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                        string static_url = _configuration["config_value:ImageStatic"];

                        foreach (var item in result.main)
                        {
                            if (item.Description == null) continue;
                            item.Description = (!item.Description.Contains(static_url) && !item.Description.Contains("data:image") && !item.Description.Contains("http")) ? (static_url + item.Description) : item.Description;
                        }
                        foreach (var item in result.sub)
                        {
                            if (item.Description == null) continue;
                            item.Description = (!item.Description.Contains(static_url) && !item.Description.Contains("data:image") && !item.Description.Contains("http")) ? (static_url + item.Description) : item.Description;
                        }
                        foreach (var item in result.trending_main)
                        {
                            if (item.Description == null) continue;
                            item.Description = (!item.Description.Contains(static_url) && !item.Description.Contains("data:image") && !item.Description.Contains("http")) ? (static_url + item.Description) : item.Description;
                        }
                        
                    }

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = "",
                        main_slide = result.main.Where(x => x.Description != null && x.Description.Trim() != "").Select(x => new { x.Id, x.OrderNo, x.Description }),
                        sub_banner = result.sub.Where(x => x.Description != null && x.Description.Trim() != "").Select(x => new { x.Id, x.OrderNo, x.Description }),
                        trending_main = result.trending_main.Where(x => x.Description != null && x.Description.Trim() != "").Select(x => new { x.Id, x.OrderNo, x.Description }),
                    });

                }

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid
            });

        }
    }
}
