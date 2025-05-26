using APP_CHECKOUT.DAL;
using Azure.Core;
using DAL;
using Entities.ConfigModels;
using Entities.Models;
using HuloToys_Service.Models.APP;
using HuloToys_Service.Models.Models;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;

namespace Repositories.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDAL orderDAL;
        private readonly OrderDetailDAL orderDetailDAL;
        public OrderRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            orderDAL = new OrderDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            orderDetailDAL = new OrderDetailDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public async Task<long> CreateOrder(Order request)
        {
            return await orderDAL.CreateOrder(request);
        }
        public async Task<long> UpdateOrder(Order request)
        {
           return await orderDAL.UpdateOrder(request);
        }
        public async Task<long> CreateOrderDetail(OrderDetail request)
        {
            return await orderDetailDAL.CreateOrderDetail(request);
        }
        public async Task<long> UpdateOrderDetail(OrderDetail request)
        {
            return await orderDetailDAL.UpdateOrderDetail(request);
        }
        public async Task<long> CountOrderByYear()
        {
            return await orderDAL.CountOrderByYear();
        }



    }
}
