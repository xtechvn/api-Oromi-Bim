using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.StoreProcedure
{
    public class SPName
    {
       public const string CREATE_ORDER = "SP_InsertOrder";
       public const string UPDATE_ORDER = "sp_UpdateOrder";
        public const string CREATE_ORDER_DEATIL = "sp_InsertOrderDetail";
        public const string UPDATE_ORDER_DEATIL = "sp_UpdateOrderDetail";
        public static string sp_InsertAddressClient = "sp_InsertAddressClient";
        public static string sp_InsertAccountClient = "sp_InsertAccountClient";
        public static string sp_UpdateAccountClient = "sp_UpdateAccountClient";

    }

}
