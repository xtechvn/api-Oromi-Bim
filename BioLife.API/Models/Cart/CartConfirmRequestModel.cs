using Entities.Models;
using HuloToys_Service.Models.APP;
using HuloToys_Service.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.APIRequest
{
    public class CartConfirmRequestModel
    {
        public long account_client_id { get; set; }
        public int payment_type { get; set; }
        public int delivery_type { get; set; }
        public List<CartConfirmItemRequestModel> carts { get; set; }
        public AddressClientFEModel address { get; set; }
        public long address_id { get; set; }
    }
    public class CartConfirmItemRequestModel
    {
        public string id { get; set; }
        public string product_id { get; set; }
        public int quanity { get; set; }
    }
    public partial class AddressClientFEModel : AddressClientESModel
    {
        public Province province_detail { get; set; }
        public District district_detail { get; set; }
        public Ward ward_detail { get; set; }

    }
}
