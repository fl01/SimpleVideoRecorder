using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SimpleVideoRecorder.Core.ScreenDetails
{
    public class WinFormsScreenMetadataService : IScreenMetadataService
    {
        public IReadOnlyCollection<ScreenMetadata> GetActiveScreens()
        {
            return Screen.AllScreens
                .Select(m => new ScreenMetadata(m.DeviceName, m.Bounds.X, m.Bounds.Y, m.Bounds.Width, m.Bounds.Height, m.Primary))
                .ToList()
                .AsReadOnly();
        }
    }
}
