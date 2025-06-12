using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using HuloToys_Service.Models;
using HuloToys_Service.Utilities.Lib;
using System.Reflection;

namespace HuloToys_Service.Controllers.Order.Business
{
    public partial class GoogleSheetsWriter
    {
        private static readonly string SPREADSHEET_ID = "1wtiR77awxMSsXMR62ACYgcWC-Byda-sfa_PGq53StiE";
        private static readonly string SERVICE_ACCOUNT_KEY_FILE_PATH = "disco-horizon-462708-i0-cf23220db216.json";
        private readonly IConfiguration configuration;
        public GoogleSheetsWriter(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        /// <summary>
        /// Thêm dữ liệu vào cuối Google Sheet.
        /// </summary>
        /// <param name="service">Đối tượng SheetsService đã được xác thực.</param>
        /// <param name="spreadsheetId">ID của bảng tính.</param>
        /// <param name="range">Phạm vi để tìm dòng cuối cùng (ví dụ: "Sheet1!A:Z").</param>
        /// <param name="values">Dữ liệu để thêm.</param>
        private static async Task AppendDataToSheet(SheetsService service, string spreadsheetId, string range, IList<IList<object>> values)
        {
            try
            {
                var valueRange = new ValueRange { Values = values };
                var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS; // Thêm hàng mới

                var appendResponse = await appendRequest.ExecuteAsync();
                Console.WriteLine($"✔️ Thêm dữ liệu thành công! {appendResponse.Updates?.UpdatedCells ?? 0} ô đã được thêm.");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"✔️ Thêm dữ liệu thành công! ô đã được thêm.");
            }

        }
        /// <summary>
        /// Ghi (ghi đè) dữ liệu vào một phạm vi cụ thể trong Google Sheet.
        /// </summary>
        /// <param name="service">Đối tượng SheetsService đã được xác thực.</param>
        /// <param name="spreadsheetId">ID của bảng tính.</param>
        /// <param name="range">Phạm vi cell (ví dụ: "Sheet1!A1").</param>
        /// <param name="values">Dữ liệu để ghi.</param>
        private static async Task WriteDataToSheet(SheetsService service, string spreadsheetId, string range, IList<IList<object>> values)
        {
            var valueRange = new ValueRange { Values = values };
            var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

            var updateResponse = await updateRequest.ExecuteAsync();
            Console.WriteLine($"✔️ Ghi dữ liệu thành công! {updateResponse.UpdatedCells ?? 0} ô đã được cập nhật.");
        }
        public async Task<int> AppendData(GoogleSheetsViewModel data)
        {
            GoogleCredential credential;
            SheetsService sheetsService;

            try
            {
                var workingDirectory = Environment.CurrentDirectory;
                //  var currentDirectory = Directory.GetParent(workingDirectory);
                var link = workingDirectory + @"\json\" + SERVICE_ACCOUNT_KEY_FILE_PATH;
                // Bước 1: Xác thực với Google API bằng Service Account Key
                using (var stream = new FileStream(link, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(SheetsService.Scope.Spreadsheets); // Chỉ yêu cầu quyền truy cập vào Sheets
                }

                // Bước 2: Tạo đối tượng dịch vụ Google Sheets
                sheetsService = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Web Data", // Tên ứng dụng của bạn
                });


                // ============================ Ghi thêm dữ liệu vào cuối Sheet (Append) ============================
               
                var additionalData = new List<IList<object>>
                {
                    new List<object> { ""+data.OrderNo+"", "" + data.FullName + "",  ""+data.Phone+"", ""+data.Name+"", ""+data.Quantity+"",
                        ""+data.TotalAmount+"", ""+data.Note+"", ""+data.ProvinceName+"", ""+data.DistrictName+"",""+data.CreatedDate+""},
                    
                };


                // Range trống để API tự động tìm dòng cuối cùng có dữ liệu
                string appendRange = "Web Data!A2:Z2"; // Hoặc bất kỳ phạm vi đủ rộng nào
                Console.WriteLine($"🚀 Đang thêm dữ liệu vào cuối Sheet '{appendRange}'...");
                await AppendDataToSheet(sheetsService, SPREADSHEET_ID, appendRange, additionalData);
                // ==============================================================================

            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"❌ Lỗi: Không tìm thấy file JSON key của Service Account tại: {SERVICE_ACCOUNT_KEY_FILE_PATH}");
                Console.WriteLine("Vui lòng kiểm tra lại đường dẫn hoặc đảm bảo file đã được tải về.");
            }
            catch (Google.GoogleApiException ex)
            {
                Console.WriteLine($"❌ Lỗi Google API: {ex.Message}");
                Console.WriteLine($"   Chi tiết: {ex.Error?.Message ?? "Không có chi tiết lỗi."}");
                Console.WriteLine("   Gợi ý: Kiểm tra lại SPREADSHEET_ID, quyền của Service Account (Editor), và địa chỉ email của Service Account đã được chia sẻ với Sheet chưa.");
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegramByUrl(configuration["telegram:log_try_catch:bot_token"], configuration["telegram:log_try_catch:group_id"], MethodBase.GetCurrentMethod().Name + "=>" + ex.Message);
               
                return 0;
            }

            return 1;
        }
    }
}
