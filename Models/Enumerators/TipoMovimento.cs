using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace VLR.Models.Enumerators
{
    public enum TipoMovimento
    {
        [Description("Horizontal")]
        Horizontal = 1,

        [Description("Vertical")]
        Vertical = 2
    }
}