using Entities.ConfigModels;
using HuloToys_Service.DAL;
using HuloToys_Service.IRepositories;
using HuloToys_Service.Models.Models;
using Microsoft.Extensions.Options;

namespace HuloToys_Service.Repositories
{
    public class AllCodeRepository : IAllCodeRepository
    {
        private readonly AllCodeDAL _AllCodeDAL;
        public AllCodeRepository(IOptions<DataBaseConfig> dataBaseConfig, ILogger<AllCodeRepository> logger)
        {
         
            _AllCodeDAL = new AllCodeDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
          
        }
        public List<AllCode> GetListByType(string type)
        {
            return _AllCodeDAL.GetListByType(type);
        }
    }
}
