using HuloToys_Service.Models.Models;

namespace HuloToys_Service.IRepositories
{
    public interface IAllCodeRepository
    {
        List<AllCode> GetListByType(string type);
    }
}
