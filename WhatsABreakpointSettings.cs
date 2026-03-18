using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace WhatsABreakpoint
{
    public class WhatsABreakpointSettings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(false);

        [Menu("Show Visual")]
        public ToggleNode ShowVisual { get; set; } = new ToggleNode(true);

        [Menu("Visual Position X")]
        public RangeNode<int> VisualX { get; set; } = new RangeNode<int>(100, 0, 3840);

        [Menu("Visual Position Y")]
        public RangeNode<int> VisualY { get; set; } = new RangeNode<int>(100, 0, 2160);

        [Menu("Visual Scale")]
        public RangeNode<float> VisualScale { get; set; } = new RangeNode<float>(1.0f, 0.5f, 2.0f);

        [Menu("Bar Width")]
        public RangeNode<int> BarWidth { get; set; } = new RangeNode<int>(400, 200, 800);

        [Menu("Bar Height")]
        public RangeNode<int> BarHeight { get; set; } = new RangeNode<int>(30, 20, 100);

        public ColorSettings Colors { get; set; } = new ColorSettings();

        public ManualOverrides Manual { get; set; } = new ManualOverrides();
    }

    [Submenu]
    public class ColorSettings
    {
        [Menu("Perfect Color")]
        public ColorNode PerfectColor { get; set; } = new ColorNode(new Color(0, 255, 0, 255));

        [Menu("Warning Color")]
        public ColorNode WarningColor { get; set; } = new ColorNode(new Color(255, 255, 0, 255));

        [Menu("Danger Color")]
        public ColorNode DangerColor { get; set; } = new ColorNode(new Color(255, 0, 0, 255));

        [Menu("Hovering Zone Color")]
        public ColorNode HoveringColor { get; set; } = new ColorNode(new Color(255, 165, 0, 200)); // Orange

        [Menu("Firing Zone Color")]
        public ColorNode FiringColor { get; set; } = new ColorNode(new Color(255, 69, 0, 200)); // Red-orange

        [Menu("Buffer Zone Color")]
        public ColorNode BufferColor { get; set; } = new ColorNode(new Color(144, 238, 144, 150)); // Light green

        [Menu("Background Color")]
        public ColorNode BackgroundColor { get; set; } = new ColorNode(new Color(0, 0, 0, 180));

        [Menu("Text Color")]
        public ColorNode TextColor { get; set; } = new ColorNode(Color.White);
    }

    [Submenu]
    public class ManualOverrides
    {
        [Menu("Use Manual Values")]
        public ToggleNode UseManualValues { get; set; } = new ToggleNode(false);

        [Menu("Projectiles")]
        public RangeNode<int> Projectiles { get; set; } = new RangeNode<int>(5, 1, 20);

        [Menu("Duration Reduction %")]
        public RangeNode<float> DurationReduction { get; set; } = new RangeNode<float>(0f, -90f, 50f);

        [Menu("Attack Speed")]
        public RangeNode<float> AttackSpeed { get; set; } = new RangeNode<float>(5.0f, 1.0f, 15.0f);

        [Menu("Base Attack Time (ms)")]
        public RangeNode<int> BaseAttackTime { get; set; } = new RangeNode<int>(600, 100, 2000);
    }
}
