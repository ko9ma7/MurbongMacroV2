using System.Threading;
using WpfApplicationHotKey.WinApi;

namespace MurbongMacroV2.Core
{
    /// <summary>
    /// 독립형 매크로, 메인 탭과 별도로 실행된다.(제보자 TALBAE)
    /// </summary>
    public class Independent
    {

        public HotKey Hotkey { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string HotkeyName { get; set; }
        public bool Activation { get; set; }
        public Preset Preset { get; set; }

        public CancellationTokenSource cts;

        public Independent(HotKey hot, Preset preset, string hotkeyName)
        {
            Preset = preset;
            Hotkey = hot;
            HotkeyName = hotkeyName;
            Name = preset.Name;
            Description = preset.Description;
            Activation = false;

        }

        public override string ToString()
        {
            return HotkeyName + " , " + Name;
        }


    }
}
