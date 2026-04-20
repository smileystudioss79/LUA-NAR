using UnityEngine;
using LUNAR.Logging;

namespace LUNAR.Startup
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class LuaNarStartup : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            string root = KSPUtil.ApplicationRootPath + "GameData";
            LuaNarLog.Initialize(root);
            LuaNarLog.AppendInfo($"LUA-NAR v{LuaNarLog.Version} initialised.");
            LuaNarLog.AppendInfo($"Root path: {root}");
        }
    }
}
