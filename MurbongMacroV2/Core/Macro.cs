using System.Windows;

namespace MurbongMacroV2.Core
{


    /// <summary>
    /// 모든 매크로의 주인 컨트롤, 액션(텍스트만), 설명이 있다.
    /// </summary>
    public class Macro
    {
        public string Control { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public override string ToString()
        {
            return Control + Action + Description;
        }
    }

    /// <summary>
    /// 키보드 매크로, Vk코드와 Press Pull 결정
    /// </summary>
    public class KeyboardMacro : Macro
    {
        public int Code { get; set; }
        public int Btn { get; set; }

    }
    /// <summary>
    /// 마우스 매크로, 이동인지, 클릭인지 확인한다.
    /// </summary>
    public class MouseMacro : Macro
    {
        public int Status { get; set; }
        public Point Coordinate { get; set; }
        public int RandStatus { get; set; }
        public Point Parameter { get; set; }
        public int MouseButton { get; set; }
    }

    public class DelayMacro : Macro
    {
        public int MS { get; set; }
        public int RandParam1 { get; set; }
        public int RandParam2 { get; set; }
    }

    public class ImageSearchMacro : Macro
    {
        public string ImageName { get; set; }
        public Point Position { get; set; }
        public Point Area { get; set; }
        public int Tolerance { get; set; }
        public bool MouseMode { get; set; }
        public string SuccessPreset { get; set; }
        public string FailPreset { get; set; }
    }

    public class OCRMacro : Macro
    {
        public Point Position { get; set; }
        public Point Area { get; set; }
        public bool EngKor { get; set; }
        public string RGX { get; set; }
        public string SuccessPreset { get; set; }
        public string FailPreset { get; set; }
    }

    public class ControlMacro : Macro
    {
        public int Signal { get; set; }
        public int Parameter { get; set; }
    }

    public class PresetMacro : Macro
    {
        public string Name { get; set; }
    }

}
