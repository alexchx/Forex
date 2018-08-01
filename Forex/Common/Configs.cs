using System;
using System.IO;

namespace Forex
{
    public static class Configs
    {
        public const int PAGE_SIZE = 15;

#if DEBUG
        public static readonly string DATA_ROOT = Environment.CurrentDirectory;
# else
        public static readonly string DATA_ROOT = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ExRate");
#endif
    }
}
