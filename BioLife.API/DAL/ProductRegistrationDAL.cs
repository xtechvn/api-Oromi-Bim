using DAL.StoreProcedure;
using HuloToys_Service.Models.ProductRegistration;
using Microsoft.Data.SqlClient;

namespace DAL
{
    public class ProductRegistrationDAL
    {
        private static DbWorker _DbWorker;
        public ProductRegistrationDAL(string connection) 
        {
            _DbWorker = new DbWorker(connection);
        }

        public async Task<int> InsertProductRegistration(ProductRegistration model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[8];
                objParam[0] = new SqlParameter("@ProductId", model.ProductId);
                objParam[1] = new SqlParameter("@DistrictId", model.DistrictId);
                objParam[2] = new SqlParameter("@ProvinceId", model.ProvinceId);
                objParam[3] = new SqlParameter("@Phone", model.Phone);
                objParam[4] = new SqlParameter("@FullName", model.FullName);
                objParam[5] = new SqlParameter("@Note", model.Note);
                objParam[6] = new SqlParameter("@CreatedBy", 1);
                objParam[7] = new SqlParameter("@CreatedDate", DateTime.Now);
                
                return _DbWorker.ExecuteNonQuery(ProcedureConstants.sp_InsertProductRegistration, objParam);
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("GetDetailOrderServiceByOrderId - OrderDal: " + ex);
            }
            return 0;
        }

    }
}