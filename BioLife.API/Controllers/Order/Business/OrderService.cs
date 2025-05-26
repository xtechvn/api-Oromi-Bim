using Caching.Elasticsearch;
using Entities.Models;
using HuloToys_Service.Models;
using HuloToys_Service.Models.APP;
using HuloToys_Service.Models.Models;
using HuloToys_Service.RabitMQ;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Models.MongoDb;
using Repositories.IRepositories;
using System.Reflection;
using Utilities;

namespace HuloToys_Service.Controllers.Order.Business
{

    public partial class OrderService
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        private readonly AccountClientESService accountClientESService;
        private readonly ClientESService clientESService;
        private readonly AddressClientESService addressClientESService;
        private readonly IProvinceRepository provinceRepository;
        private readonly IDistrictRepository districtRepository;
        private readonly IWardRepository wardRepository;
        private readonly WorkQueueClient workQueueClient;
        private readonly IOrderRepository orderRepository;

        public OrderService(IConfiguration _configuration, RedisConn _redisService,
            IProvinceRepository _provinceRepository, IDistrictRepository _districtRepository, IWardRepository _wardRepository, IOrderRepository orderRepository)
        {
            configuration = _configuration;
            redisService = _redisService;
            accountClientESService = new AccountClientESService(_configuration["Elastic:Host"], _configuration);
            clientESService = new ClientESService(_configuration["Elastic:Host"], _configuration);
            addressClientESService = new AddressClientESService(_configuration["Elastic:Host"], _configuration);
            provinceRepository = _provinceRepository;
            districtRepository = _districtRepository;
            wardRepository = _wardRepository;
            workQueueClient = new WorkQueueClient(configuration);
            this.orderRepository = orderRepository;
        }

        public async Task<List<UserLoginModel>> getOrderList()
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], MethodBase.GetCurrentMethod().Name + "=>" + ex.Message);
                return null;
            }
        }
        public async Task CreateOrder(OrderDetailMongoDbModel order, AccountClient account)
        {
            try
            {
                HuloToys_Service.Models.Models.Order order_summit = new HuloToys_Service.Models.Models.Order();
                List<OrderDetail> details = new List<OrderDetail>();
                double total_price = 0;
                double total_profit = 0;
                double total_discount = 0;
                double total_amount = 0;
                float total_weight = 0;
                foreach (var cart in order.carts)
                {
                    string name_url = CommonHelper.RemoveUnicode(cart.product.name);
                    name_url = CommonHelper.RemoveSpecialCharacters(name_url);
                    name_url = name_url.Replace(" ", "-").Trim();
                    details.Add(new OrderDetail()
                    {
                        CreatedDate = DateTime.Now,
                        Discount = cart.product.discount,
                        OrderDetailId = 0,
                        OrderId = 0,
                        Price = cart.product.price,
                        Profit = cart.product.profit,
                        Quantity = cart.quanity,
                        Amount = cart.product.amount,
                        ProductCode = cart.product.code,
                        ProductId = cart.product._id,
                        ProductLink = "",
                        TotalPrice = cart.product.price * cart.quanity,
                        TotalProfit = cart.product.profit * cart.quanity,
                        TotalAmount = cart.product.amount * cart.quanity,
                        TotalDiscount = cart.product.discount * cart.quanity,
                        UpdatedDate = DateTime.Now,
                        UserCreate = 1,
                        UserUpdated = 1
                    });
                    total_price += (cart.product.price * cart.quanity);
                    total_profit += (cart.product.profit * cart.quanity);
                    total_discount += (cart.product.discount * cart.quanity);
                    total_amount += (cart.product.amount * cart.quanity);
                    cart.total_price = cart.product.price * cart.quanity;
                    cart.total_discount = cart.product.discount * cart.quanity;
                    cart.total_profit = cart.product.profit * cart.quanity;
                    cart.total_amount = cart.product.amount * cart.quanity;
                    total_weight += 0;

                }
              //  var account_client = accountClientESService.GetById(order.account_client_id);
                //logging_service.InsertLogTelegramDirect(" accountClientESService.GetById("+ order.account_client_id + ") : "+ (account_client == null ? "NULL" : JsonConvert.SerializeObject(account_client)));

               // var client = clientESService.GetById((long)account_client.clientid);
                // logging_service.InsertLogTelegramDirect(" clientESService.GetById(" + (long)account_client.ClientId + ") : " + (client == null ? "NULL" : JsonConvert.SerializeObject(client)));

               // AddressClientESModel address_client = addressClientESService.GetById(order.ad, client.Id);
                // logging_service.InsertLogTelegramDirect(" addressClientESService.GetById(" + order.address_id + "," + client.Id + ") : " + (address_client == null ? "NULL" : JsonConvert.SerializeObject(address_client)));

                order_summit = new HuloToys_Service.Models.Models.Order()
                {
                    Amount = total_amount,
                    ClientId = account!=null && account.ClientId > 0? (long)account.ClientId :0,
                    CreatedDate = DateTime.Now,
                    Discount = total_discount,
                    IsDelete = 0,
                    Note = "",
                    OrderId = 0,
                    OrderNo = order.order_no,
                    PaymentStatus = 0,
                    PaymentType = Convert.ToInt16(order.payment_type),
                    Price = total_price,
                    Profit = total_profit,
                    OrderStatus = 0,
                    UpdateLast = DateTime.Now,
                    UserGroupIds = "",
                    UserId =1,
                    UtmMedium = order.utm_medium,
                    UtmSource = order.utm_source,
                    VoucherId = order.voucher_id,
                    CreatedBy = 1,
                    UserUpdateId = 1,

                    Address = order.address,
                    ReceiverName = order.receivername,
                    Phone = order.phone,
                    //ShippingFee = order.shipping_fee,
                    //CarrierId = order.delivery_detail.carrier_id,
                    //ShippingCode = "",
                    //ShippingType = order.delivery_detail.shipping_type,
                    //ShippingStatus = 0,
                    //PackageWeight = total_weight

                };
                List<Province> provinces = await provinceRepository.GetProvincesList();
                List<District> districts = await districtRepository.GetDistrictList();
                List<Ward> wards = await wardRepository.GetWardList();
                var province = provinces.FirstOrDefault(x => x.ProvinceId == order.provinceid);
                order_summit.ProvinceId = province != null ? province.Id : null;
                var district = districts.FirstOrDefault(x => x.DistrictId == order.districtid);
                order_summit.DistrictId = district != null ? district.Id : null;
                var ward = wards.FirstOrDefault(x => x.WardId == order.wardid);
                order_summit.WardId = ward != null ? ward.Id : null;
                order_summit.ReceiverName = order.receivername;
                order_summit.Phone = order.phone;
                order_summit.Address = order.address;

                var order_id = await orderRepository.CreateOrder(order_summit);
                if (order_id > 0)
                {
                    foreach (var detail in details)
                    {
                        detail.OrderId = order_id;
                        await orderRepository.CreateOrderDetail(detail);
                    }
                }
                workQueueClient.SyncES(order_id, "SP_GetOrder", "biolife_sp_getorder", 0);
                order.order = order_summit;
                order.order_detail = details;
                return;
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }

        }


    }
}
