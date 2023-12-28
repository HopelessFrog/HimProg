using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm;

namespace HimProg
{
    public class ErrorRequest : ViewModelBase
    {
        public double InitialStep { get; set; }
        public double MinStep { get; set; }

        public double MaxError { get; set; }
    }
}
