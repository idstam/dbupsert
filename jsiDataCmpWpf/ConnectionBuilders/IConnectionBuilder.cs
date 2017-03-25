using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jsiDataCmpCore;

namespace jsiDataCmpWpf.ConnectionBuilders
{
    public interface IConnectionBuilder
    {
        IDatabaseManager Manager { get; set; }
        bool? ShowDialog();
    }
}
