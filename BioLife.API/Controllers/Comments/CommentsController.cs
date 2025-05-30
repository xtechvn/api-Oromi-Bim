﻿using HuloToys_Service.Models;
using HuloToys_Service.Models.Address;
using HuloToys_Service.Models.APIRequest;
using HuloToys_Service.Models.Queue;
using HuloToys_Service.MongoDb;
using HuloToys_Service.RabitMQ;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.MongoDb;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Utilities;
using Utilities.Contants;

namespace HuloToys_Service.Controllers.Comments
{
    [Route("api")]
    [ApiController]

    public class CommentsController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        private readonly WorkQueueClient work_queue;
        private readonly CommentsMongodbService _commentsMongodbService;
        public CommentsController(IConfiguration _configuration, RedisConn _redisService)
        {
            configuration = _configuration;
            redisService = _redisService;
            work_queue = new WorkQueueClient(configuration);

            _commentsMongodbService = new CommentsMongodbService(configuration);

        }
        [HttpPost("push-comments")]
        public async Task<IActionResult> insertComments([FromBody] APIRequestGenericModel input)
        {
            try
            {
                JArray objParr = null;
                if (input != null && input.token != null && CommonHelper.GetParamWithKey(input.token, out objParr, configuration["KEY:private_key"]))
                {
                    var request = JsonConvert.DeserializeObject<CommentsModel>(objParr[0].ToString());
                    bool response_queue = false;

                    var comment_model = JsonConvert.SerializeObject(request);
                    if (comment_model != null)
                    {

                        // Execute Push mongo
                        var id = await _commentsMongodbService.Insert(request);
                        if (id != null)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = "Success"
                            });
                        }
                        else
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.FAILED,
                                msg = "FAILED"
                            });
                        }
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
    }
}
