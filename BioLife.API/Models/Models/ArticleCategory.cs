﻿using System;
using System.Collections.Generic;

namespace HuloToys_Service.Models.Models;

public partial class ArticleCategory
{
    public long Id { get; set; }

    public int? CategoryId { get; set; }

    public long? ArticleId { get; set; }

    public bool? IsMain { get; set; }

    public DateTime? UpdateLast { get; set; }
}
