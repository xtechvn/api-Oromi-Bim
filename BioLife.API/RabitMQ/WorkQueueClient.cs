using HuloToys_Service.Models.Queue;
using HuloToys_Service.RedisWorker;
using HuloToys_Service.Utilities.Lib;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Reflection;
using System.Text;

namespace HuloToys_Service.RabitMQ
{
    public class WorkQueueClient
    {
        private readonly IConfiguration configuration;
        private readonly QueueSettingViewModel queue_setting;
        private readonly ConnectionFactory factory;
        
        public WorkQueueClient(IConfiguration _configuration)
        {
            configuration = _configuration;
            queue_setting = new QueueSettingViewModel()
            {
                host = _configuration["Queue:Host"],
                port = Convert.ToInt32(_configuration["Queue:Port"]),
                v_host = _configuration["Queue:V_Host"],
                username = _configuration["Queue:Username"],
                password = _configuration["Queue:Password"],
            };
            factory = new ConnectionFactory()
            {
                HostName = queue_setting.host,
                UserName = queue_setting.username,
                Password = queue_setting.password,
                VirtualHost = queue_setting.v_host,
                Port = Protocols.DefaultProtocol.DefaultPort
            };
        }
        public bool InsertQueueSimple(string message, string queueName)
        {            
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                try
                {
                    channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: queueName,
                                         basicProperties: null,
                                         body: body);
                    return true;

                }
                catch (Exception ex)
                {
                    string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                    LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                    return false;
                }
            }
        }
        public bool InsertQueueSimpleDurable( string message, string queueName)
        {
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                try
                {
                    channel.QueueDeclare(queue: queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: queueName,
                                         basicProperties: null,
                                         body: body);
                    return true;

                }
                catch (Exception ex)
                {
                    string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                    LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
                    return false;
                }
            }
        }
        public bool SyncES(long id, string store_procedure, string index_es, short project_id)
        {
            try
            {
                var j_param = new Dictionary<string, object>
                              {
                              { "store_name", store_procedure },
                              { "index_es", index_es },
                              {"project_type", project_id },
                              {"id" , id }

                              };
                var _data_push = JsonConvert.SerializeObject(j_param);
                // Push message vào queue
                var response_queue = InsertQueueSyncES(_data_push);

                return true;
            }
            catch (Exception ex)
            {

            }
            return false;
        }
        public bool InsertQueueSyncES(string message)
        {
            var queue_setting_sync_es = new QueueSettingViewModel()
            {
                host = configuration["Queue:Host"],
                port = Convert.ToInt32(configuration["Queue:Port"]),
                v_host = configuration["Queue:Sync_V_Host"],
                username = configuration["Queue:Sync_Username"],
                password = configuration["Queue:Sync_Password"],
            };
            var factory_es = new ConnectionFactory()
            {
                HostName = queue_setting_sync_es.host,
                UserName = queue_setting_sync_es.username,
                Password = queue_setting_sync_es.password,
                VirtualHost = queue_setting_sync_es.v_host,
                Port = Protocols.DefaultProtocol.DefaultPort
            };
            using (var connection = factory_es.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                try
                {
                    channel.QueueDeclare(queue: configuration["Queue:Sync_QueueName"],
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: configuration["Queue:Sync_QueueName"],
                                         basicProperties: null,
                                         body: body);
                    return true;

                }
                catch (Exception ex)
                {

                    return false;
                }
            }
        }
    }
}
