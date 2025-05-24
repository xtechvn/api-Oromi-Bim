using Caching.Elasticsearch;
using DAL;
using HuloToys_Service.Controllers.Order.Business;
using HuloToys_Service.Controllers.ProductRegistration.Business;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.Orders;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RabitMQ;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Utilities;
using Utilities.Contants;

namespace HuloToys_Service.Controllers.ProductRegistration
{
    [ApiController]
    [Route("api/[controller]")]

    public class ProductRegistrationController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ProductRegistrationService productRegistrationService;
        private readonly RedisConn _redisService;
        public ProductRegistrationController(IConfiguration _configuration, RedisConn redisService)
        {
            configuration = _configuration;
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
            productRegistrationService = new ProductRegistrationService(configuration, _redisService);

        }
        [HttpPost("SeverProductRegistration.json")]
        public async Task<ActionResult> SeverProductRegistration([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<Models.ProductRegistration.ProductRegistration>(objParr[0].ToString());
                   var Service = await productRegistrationService.InsertProductRegistration(request);
                    if(Service >0)
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                      
                    });
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.DataInvalid
            });


        }
    }
}
