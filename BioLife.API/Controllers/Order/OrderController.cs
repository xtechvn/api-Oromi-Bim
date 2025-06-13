using Caching.Elasticsearch;
using Models.APIRequest;
using HuloToys_Service.RabitMQ;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Utilities;
using Utilities.Contants;
using HuloToys_Service.MongoDb;
using Models.MongoDb;
using HuloToys_Service.Models.APP;
using HuloToys_Service.Utilities.constants.APP;
using HuloToys_Service.Controllers.Order.Business;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Models.Orders;
using HuloToys_Service.Models.APIRequest;
using Repositories.IRepositories;
using Repositories.Repositories;
using HuloToys_Service.Controllers.Orders.Business;
using HuloToys_Service.Models;
using HuloToys_Service.Controllers.Location.Business;
using Nest;

namespace HuloToys_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly WorkQueueClient workQueueClient;
        private readonly OrderESService orderESRepository;
        private readonly OrderMongodbService orderMongodbService;
        private readonly AccountClientESService accountClientESService;
        private readonly CartMongodbService _cartMongodbService;
        private readonly WorkQueueClient work_queue;
        private readonly IdentiferService identiferService;
        private readonly RedisConn _redisService;
        private readonly IProvinceRepository provinceRepository;
        private readonly IDistrictRepository districtRepository;
        private readonly IWardRepository wardRepository;
        private readonly OrderService orderService;
        private readonly IOrderRepository orderRepository;
        private readonly IAccountClientRepository accountClientRepository;
        private readonly ProductDetailMongoAccess productDetailMongoAccess;
        private readonly GoogleSheetsWriter googleSheetsWriter;
        private readonly LocationSevice locationSevice;
        public OrderController(IConfiguration _configuration, RedisConn redisService,
            IProvinceRepository _provinceRepository, IDistrictRepository _districtRepository, IWardRepository _wardRepository, IOrderRepository _orderRepository,
            IAccountClientRepository _accountClientRepository)
        {
            configuration = _configuration;

            workQueueClient = new WorkQueueClient(configuration);
            orderESRepository = new OrderESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            accountClientESService = new AccountClientESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
            orderMongodbService = new OrderMongodbService( configuration);
            _cartMongodbService = new CartMongodbService(configuration);
            work_queue = new WorkQueueClient(configuration);
            identiferService = new IdentiferService(_configuration);
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
            provinceRepository=_provinceRepository;
            districtRepository=_districtRepository;
            wardRepository=_wardRepository;
            orderRepository = _orderRepository;
            orderService =new OrderService(configuration, _redisService,provinceRepository,districtRepository,wardRepository, _orderRepository);
            productDetailMongoAccess=new ProductDetailMongoAccess(configuration);
            accountClientRepository = _accountClientRepository;
            googleSheetsWriter = new GoogleSheetsWriter(_configuration);
            locationSevice = new LocationSevice(_configuration, _redisService);
        }

        [HttpPost("history")]
        public async Task<ActionResult> OrderHistory([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrderHistoryRequestModel>(objParr[0].ToString());
                    if (request == null || request.client_id <= 0)
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var result=  orderESRepository.GetByClientID(request.client_id);

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data= result
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
        [HttpPost("fe-history")]
        public async Task<ActionResult> OrderFEHistory([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrderHistoryRequestModel>(objParr[0].ToString());
                    if (request == null || request.client_id <= 0
                        || request.page_index <= 0 || request.page_size <= 0
                        )
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    if (request.status == "-1") request.status = "";

                    var cache_name = CacheType.ORDER_DETAIL_FE + request.client_id+request.status+request.page_index+request.page_size;
                    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_search_result"]));
                    if (j_data != null && j_data.Trim() != "")
                    {
                        OrderFEResponseModel data = JsonConvert.DeserializeObject<OrderFEResponseModel>(j_data);
                        if (data != null && data.data != null&& data.data.Count>0)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = ResponseMessages.Success,
                                data = data
                            });
                        }
                    }
                    var account_client = accountClientESService.GetById(request.client_id);
                    var result = orderESRepository.GetFEByClientID((long)account_client.clientid, request.status, (request.page_index <= 0 ? 1 : request.page_index), (request.page_size <= 0 ? 10 : request.page_size));
                    if(result!=null && result.data!=null && result.data.Count > 0)
                    {
                        result.data_order = await orderMongodbService.GetListByOrderId(result.data.Select(x => x.orderid).ToList());
                    }
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = result
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
        [HttpPost("history-lastest")]
        public async Task<ActionResult> OrderHistoryLastestItem([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrderHistoryRequestModel>(objParr[0].ToString());
                    if (request == null|| request.client_id<=0)
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var result = orderESRepository.GetLastestClientID(request.client_id);


                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data=result
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
        [HttpPost("history-find")]
        public async Task<ActionResult> OrderHistoryFind([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrderHistoryRequestModel>(objParr[0].ToString());
                    if (request == null || request.client_id <= 0 || request.order_no ==null || request.order_no.Trim()=="")
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }

                    var order=orderESRepository.GetByOrderNo(request.order_no,request.client_id);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data= order
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
        [HttpPost("detail")]
        public async Task<ActionResult> Detail([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrdersGeneralRequestModel>(objParr[0].ToString());
                    if (request == null || request.id == null || request.id.Trim() == "")
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var result = await orderMongodbService.FindById(request.id);
                    if(result != null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success",
                            data = result
                        });

                    }
                    

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
        [HttpPost("history-detail")]
        public async Task<ActionResult> HistoryDetail([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<OrderHistoryDetailRequestModel>(objParr[0].ToString());
                    if (request == null || request.id <= 0)
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    OrderDetailResponseModel result = new OrderDetailResponseModel()
                    {
                        data_order = (await orderMongodbService.GetListByOrderId(new List<long>() { request.id })).FirstOrDefault(),
                        data = orderESRepository.GetByOrderId(request.id)
                    };
                    if (result != null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Success",
                            data = result
                        });

                    }


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
        [HttpPost("confirm")]
        public async Task<ActionResult> Confirm([FromBody] APIRequestGenericModel input)
        {
            try
            {


                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<CartConfirmRequestModel>(objParr[0].ToString());
                    if (request == null || request.account_client_id == null || request.account_client_id <= 0
                        || request.carts == null || request.carts.Count <= 0)
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var count = await orderESRepository.CountOrderByYear();
                    if (count < 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.FunctionExcutionFailed
                        });
                    }
                    var order_no = await identiferService.buildOrderNo(count);
                    var model = new OrderDetailMongoDbModel()
                    {
                        account_client_id = request.account_client_id,
                        carts = new List<CartItemMongoDbModel>(),
                        payment_type = request.payment_type,
                        delivery_type = request.delivery_type,
                        order_no = order_no,
                        total_amount=0
                    };
                    
                    foreach (var item in request.carts)
                    {
                        var cart = await _cartMongodbService.FindById(item.id);
                        if (cart == null
                            || cart._id == null || cart._id.Trim() == ""
                            || cart.account_client_id != request.account_client_id)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.FAILED,
                                msg = ResponseMessages.DataInvalid
                            });
                        }
                        else
                        {
                            cart.quanity = item.quanity;
                            model.carts.Add(cart);
                            model.total_amount += cart.total_amount;
                            await _cartMongodbService.Delete(item.id);
                        }

                    }
                    //-- Mongodb:
                    var result = await orderMongodbService.Insert(model);
                    //-- Insert Queue:
                    var queue_model = new CheckoutQueueModel() { event_id = (int)CheckoutEventID.CREATE_ORDER, order_mongo_id = result };
                    var pushed_queue=work_queue.InsertQueueSimpleDurable(JsonConvert.SerializeObject(queue_model) , QueueName.QUEUE_CHECKOUT);
                   
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = new OrderConfirmResponseModel { order_no = order_no, id = result, pushed= pushed_queue }
                    });
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = ResponseMessages.FunctionExcutionFailed
                });

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.FunctionExcutionFailed
            });
        }
        [HttpPost("confirm-order")]
        public async Task<ActionResult> ConfirmOrder([FromBody] APIRequestGenericModel input)
        {
            try
            {
                //CartConfirmRequestModel model_json = new CartConfirmRequestModel()
                //{
                //    account_client_id = 17,
                //    carts = new List<CartConfirmItemRequestModel>
                //    {
                //        new CartConfirmItemRequestModel()
                //        {
                //            id="682ef1aa0b7252f656aadb16",
                //            product_id="682ef1aa0b7252f656aadb16",
                //             quanity=2
                //        }
                //    },
                //    delivery_type = 0,
                //    payment_type = 0,
                //    address=new AddressClientFEModel()
                //    {
                //        address="So 001,ngo 00xx, duong XXX,",
                //        phone="0123456789",
                //        receivername="Nguyen van A",
                //        provinceid="01",
                //        districtid="001",
                //        wardid= "00001"
                //    },
                //    address_id=0,
                    
                    
                //};
                //input = new APIRequestGenericModel()
                //{
                //    token = CommonHelper.Encode(JsonConvert.SerializeObject(model_json), configuration["KEY:private_key"])
                //};

                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<CartConfirmRequestModel>(objParr[0].ToString());
                    if (request == null || request.carts == null || request.carts.Count <= 0)
                    {

                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.DataInvalid
                        });
                    }
                    var count = await orderRepository.CountOrderByYear();
                    if (count < 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.FunctionExcutionFailed
                        });
                    }
                    var order_no = await identiferService.buildOrderNo(count);
                    var model = new OrderDetailMongoDbModel()
                    {
                        account_client_id = request.account_client_id,
                        carts = new List<CartItemMongoDbModel>(),
                        payment_type = request.payment_type,
                        delivery_type = request.delivery_type,
                        order_no = order_no,
                        total_amount = 0,
                        address_id = request.address_id,
                        provinceid=request.address.provinceid,
                        districtid=request.address.districtid,
                        wardid=request.address.wardid,
                        receivername=request.receivername,
                        phone=request.phone,
                        address=request.address.address,
                        note=request.note,
                        
                    };

                    foreach (var item in request.carts)
                    {
                        var p = await productDetailMongoAccess.GetByID(item.product_id);
                        //if (cart == null
                        //    || cart._id == null || cart._id.Trim() == ""
                        //    || cart.account_client_id != request.account_client_id)
                        //{
                        //    return Ok(new
                        //    {
                        //        status = (int)ResponseType.FAILED,
                        //        msg = ResponseMessages.DataInvalid
                        //    });
                        //}
                        //else
                        //{
                        //    cart.quanity = item.quanity;
                        //    model.carts.Add(cart);
                        //    model.total_amount += cart.total_amount;
                        //    await _cartMongodbService.Delete(item.id);
                        //}
                        model.carts.Add(new CartItemMongoDbModel()
                        {
                            account_client_id= request.account_client_id,
                            created_date = DateTime.Now,
                            product=p,
                            quanity= (item.quanity <= 1 ? 1 : item.quanity),
                            total_amount=p.amount * (item.quanity<=1 ? 1: item.quanity),
                            total_discount=0,
                            total_price= p.price * (item.quanity <= 1 ? 1 : item.quanity),
                            total_profit= p.profit * (item.quanity <= 1 ? 1 : item.quanity),  
                        });
                        model.total_amount += p.amount * (item.quanity <= 1 ? 1 : item.quanity);
                    }
                    var account= accountClientRepository.GetById(request.account_client_id);
                    //-- save all at mongodb
                    await orderService.CreateOrder(model, account);
                    if (model.order == null || model.order.OrderId <= 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = ResponseMessages.FunctionExcutionFailed
                        });
                    }
                    //-- Mongodb:
                    var result = await orderMongodbService.Insert(model);
                  
                     ////-- Insert Queue:
                     //var queue_model = new CheckoutQueueModel() { event_id = (int)CheckoutEventID.CREATE_ORDER, order_mongo_id = result };
                     //var pushed_queue = work_queue.InsertQueueSimpleDurable(JsonConvert.SerializeObject(queue_model), QueueName.QUEUE_CHECKOUT);

                     //GoogleSheets
                     var Province_detail =await locationSevice.GetProvincesByProvinceId(request.address.provinceid.ToString());
                    var District_detail =await locationSevice.GetDistrictByDistrictId(request.address.districtid.ToString());
                    GoogleSheetsViewModel detail = new GoogleSheetsViewModel();
                    detail.CreatedDate = DateTime.Now.ToString("dd/MM/yyyy");
                    detail.OrderNo = model.order_no;
                    detail.Name = model.carts[0].product.name;
                    detail.Note = request.note;
                    detail.Quantity = request.carts[0].quanity.ToString();
                    detail.ProvinceName = Province_detail.Name;
                    detail.DistrictName = District_detail.Name;
                    detail.FullName = request.receivername;
                    detail.Phone = request.phone;
                    detail.TotalAmount = model.carts[0].total_amount.ToString("N0");
                   
                    googleSheetsWriter.AppendData(detail);

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Success",
                        data = new OrderConfirmResponseModel { order_no = order_no, id = result, pushed = true, order_id=model.order.OrderId }
                    });
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = ResponseMessages.FunctionExcutionFailed
                });

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = ResponseMessages.FunctionExcutionFailed
            });
        }
    }
}
