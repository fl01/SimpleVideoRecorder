using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVideoRecorder.Client.Common
{
    public interface IWindowView
    {
        object DataContext { get; set; }

        bool? ShowDialog();

        bool Activate();
    }
}
