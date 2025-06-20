using HuloToys_Service.Models;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Utilities.Common;
using HuloToys_Service.Utilities.Lib;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Payments;

namespace HuloToys_Service.Service.EMail
{
    public class MailService
    {
        private IConfiguration configuration;
        public MailService(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public bool sendMail(GoogleSheetsViewModel model)
        {
            bool ressult = true;
            try
            {
                MailMessage message = new MailMessage();

                var subject = "XÁC NHẬN ĐƠN HÀNG "+ model.Name+ DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                message.Subject = subject;
                var html = "<table style='border: 1px solid #b3c7db;color: #465869;border-collapse: collapse;'> <tbody>     <tr>         " +
                    "<th style='border: 1px solid #b3c7db;color: #465869;'>Mã đơn</th>" +
                    "<th style='border: 1px solid #b3c7db;color: #465869;'>Họ tên / Tên công ty</th>" +
                    "<th style='border: 1px solid #b3c7db;color: #465869;'>Số điện thoại</th>   " +
                    "<th style='border: 1px solid #b3c7db;color: #465869;'>Tên sản phẩm</th>" +
                    "<th style='border: 1px solid #b3c7db;color: #465869;'>số lượng</th>" +
                    "<th style='border: 1px solid #b3c7db;color: #465869;'>Tổng tiền</th> " +
                    "<th style='border: 1px solid #b3c7db;color: #465869;'>Lời nhắn</th> " +
                    "<th style='border: 1px solid #b3c7db;color: #465869;'>Tỉnh thành</th>  " +
                    "<th style='border: 1px solid #b3c7db;color: #465869;'>Khu vực hoạt động</th>  " +
                    "<th style='border: 1px solid #b3c7db;color: #465869;'>Ngày tạo</th> " +
                    "</tr><tr>" +
                    "<td style='border: 1px solid #b3c7db;color: #465869;'>" + model.OrderNo+ "</td>" +
                    "<td style='border: 1px solid #b3c7db;color: #465869;'>" + model.FullName + "</td>" +
                    "<td style='border: 1px solid #b3c7db;color: #465869;'>" + model.Phone + "</td>" +
                    "<td style='border: 1px solid #b3c7db;color: #465869;'>" + model.Name + "</td>" +
                    "<td style='border: 1px solid #b3c7db;color: #465869;'>" + model.Quantity + "</td>" +
                    "<td style='border: 1px solid #b3c7db;color: #465869;'>" + model.TotalAmount + "</td>" +
                    "<td style='border: 1px solid #b3c7db;color: #465869;'>" + model.Note + "</td>" +
                    "<td style='border: 1px solid #b3c7db;color: #465869;'>" + model.ProvinceName + "</td>" +
                    "<td style='border: 1px solid #b3c7db;color: #465869;'>" + model.DistrictName + "</td>" +
                    "<td style='border: 1px solid #b3c7db;color: #465869;'>" + model.CreatedDate + "</td>" +
                    "</tr>\r\n</tbody>\r\n</table>";
                //configsendemail
                string from_mail = configuration["MAIL_CONFIG:FROM_MAIL"];
                string account = configuration["MAIL_CONFIG:USERNAME"];
                string password = configuration["MAIL_CONFIG:PASSWORD"];
                string host = configuration["MAIL_CONFIG:HOST"];
                string port = configuration["MAIL_CONFIG:PORT"];
                message.IsBodyHtml = true;
                message.From = new MailAddress(from_mail);
                message.Body = html;
                string sendEmailsFrom = account;
                string sendEmailsFromPassword = password;
                SmtpClient smtp = new SmtpClient(host, Convert.ToInt32(port));
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = new NetworkCredential(sendEmailsFrom, sendEmailsFromPassword);
                smtp.Timeout = 20000;
                message.To.Add("happykids8386@gmail.com");
                message.CC.Add("happykids8386@gmail.com");
                message.CC.Add("Omoribaby@hotmail.com");
                smtp.Send(message);

            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], error_msg);
            }
            return ressult;
        }
    }
}
