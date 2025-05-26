using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class ProductRegistration
{
    public int Id { get; set; }

    public int? DistrictId { get; set; }

    public int? ProvinceId { get; set; }

    public string? Phone { get; set; }

    public string? FullName { get; set; }

    public string? Note { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? ProductId { get; set; }
}
