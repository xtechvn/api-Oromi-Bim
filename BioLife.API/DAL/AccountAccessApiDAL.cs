using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using HuloToys_Service.Models.Models;
using HuloToys_Service.Utilities.Lib;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
   public class AccountAccessApiDAL : GenericService<AccountAccessApi>
    {
        private static DbWorker _DbWorker;
        public AccountAccessApiDAL(string connection) : base(connection) {
            _DbWorker = new DbWorker(connection);
        }
        public AccountAccessApi GetByUsername(string user_name)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    user_name = user_name.Trim().ToLower();
                    var main_account = _DbContext.AccountAccessApis.FirstOrDefault(x => x.UserName == user_name);
                    if (main_account != null)
                    {
                        return main_account;
                    }


                }
            }
            catch (Exception ex)
            {

            }
            return null;

        }
        
    }
}
