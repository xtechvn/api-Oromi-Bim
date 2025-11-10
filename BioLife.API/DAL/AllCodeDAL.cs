using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HuloToys_Service.DAL
{
    public class AllCodeDAL : GenericService<AllCode>
    {
        private static DbWorker _DbWorker;
        public AllCodeDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public List<AllCode> GetListByType(string type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Set<AllCode>().Where(n => n.Type == type).ToList();
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                
                return null;
            }
        }

       
    }
}
