using DAL;
using HuloToys_Service.Models;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace HuloToys_Service.Controllers.ProductRegistration.Business
{
    public class ProductRegistrationService
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        private readonly ProductRegistrationDAL _productRegistrationDAL;
        public ProductRegistrationService(IConfiguration _configuration, RedisConn _redisService)
        {
            configuration = _configuration;
            redisService = _redisService;
            _productRegistrationDAL = new ProductRegistrationDAL(configuration["DataBaseConfig:SqlServer:ConnectionString"]);
            
        }
        public async Task<int> InsertProductRegistration(Models.ProductRegistration.ProductRegistration model)
        {
            try
            {
                var Insert =await _productRegistrationDAL.InsertProductRegistration(model);
                return Insert;


            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], MethodBase.GetCurrentMethod().Name + "=>" + ex.Message);
                return 0;
            }
            return 0;
        }
    }
}
