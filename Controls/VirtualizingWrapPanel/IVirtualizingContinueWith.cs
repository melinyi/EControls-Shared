using System;
using System.Collections.Generic;
using System.Text;

namespace EControl.Controls
{
    internal interface IVirtualizingContinueWith
    {
        public Action AddContinueWith { get; }
        public Action RemoveContinueWith { get; }
    }
}

