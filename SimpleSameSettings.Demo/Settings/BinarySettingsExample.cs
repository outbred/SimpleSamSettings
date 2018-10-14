using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSameSettings.Demo.Settings
{
    /// <summary>
    /// Has [Serializable] attribute, so it'll be serialized as Binary
    /// </summary>
    [Serializable]
    internal class BinarySettingsExample : ExampleSettingsBase<BinarySettingsExample>
    {

    }
}
