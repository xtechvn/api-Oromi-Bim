using APP_CHECKOUT.DAL;
using Entities.Models;
using HuloToys_Service.Models.APP;
using HuloToys_Service.Models.Models;

namespace Repositories.IRepositories
{
    public interface IOrderRepository
    {
        public Task<long> CreateOrder(Order request);
        public  Task<long> UpdateOrder(Order request);
        public  Task<long> CreateOrderDetail(OrderDetail request);
        public  Task<long> UpdateOrderDetail(OrderDetail request);
        public  Task<long> CountOrderByYear();
    }
}
