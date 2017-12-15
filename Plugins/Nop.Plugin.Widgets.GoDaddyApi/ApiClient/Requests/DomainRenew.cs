﻿using GodaddyWrapper.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GodaddyWrapper.Requests
{
    public class DomainRenew
    {
        [Range(1, 10)]
        public int? period { get; set; }
    }
}
