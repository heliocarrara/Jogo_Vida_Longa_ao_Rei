using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VLR.Models.ViewModels
{
    public class VMColuna
    {
        public char Rotulo { get; set; }

        public List<VMCasaTabuleiro> Casas { get; set; }

        public VMColuna()
        {
        }

    }
}