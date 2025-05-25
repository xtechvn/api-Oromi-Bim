using HuloToys_Service.ElasticSearch;
using HuloToys_Service.Models;
using HuloToys_Service.Models.Location;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using System.Reflection;

namespace HuloToys_Service.Controllers.Location.Business
{
    public partial class LocationSevice
    {
        public LocationESService _locationESService;
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public LocationSevice(IConfiguration _configuration, RedisConn _redisService)
        {
            configuration = _configuration;
            redisService = _redisService;
            _locationESService = new LocationESService(_configuration["DataBaseConfig:Elastic:Host"], configuration);
        }
        public async Task<List<Province>> GetAllProvinces()
        {
            try
            {
                var result= _locationESService.GetAllProvinces();
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], MethodBase.GetCurrentMethod().Name + "=>" + ex.Message);
                return null;
            }
        }
        public async Task<List<District>> GetAllDistrict()
        {
            try
            {
                var result = _locationESService.GetAllDistrict();
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], MethodBase.GetCurrentMethod().Name + "=>" + ex.Message);
                return null;
            }
        }
        public async Task<List<Ward>> GetAllWards()
        {
            try
            {
                var result = _locationESService.GetAllWards();
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], MethodBase.GetCurrentMethod().Name + "=>" + ex.Message);
                return null;
            }
        }
    }
}
