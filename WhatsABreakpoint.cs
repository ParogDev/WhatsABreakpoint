using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Enums;
using SharpDX;
using System;
using System.Diagnostics;

namespace WhatsABreakpoint
{
    public class WhatsABreakpoint : BaseSettingsPlugin<WhatsABreakpointSettings>
    {
        public override void OnClose()
        {
            try { HudLog.Flush(); }
            finally { base.OnClose(); }
        }

        private TimeCache<KineticFusilladeStats> _statsCache;
        private readonly Stopwatch _animationTimer = Stopwatch.StartNew();
        private const float TOLERANCE = 0.05f; // 5% tolerance for "perfect" timing

        public override bool Initialise()
        {
            Name = "Whats A Breakpoint";
            _statsCache = new TimeCache<KineticFusilladeStats>(ReadPlayerStats, 500);
            return true;
        }

        public override void AreaChange(AreaInstance area)
        {
            // Reinitialize cache on zone change
            _statsCache = new TimeCache<KineticFusilladeStats>(ReadPlayerStats, 500);
        }

        public override void Render()
        {
            if (!Settings.Enable || !GameController.InGame)
                return;

            try
            {
                var stats = _statsCache.Value;
                if (stats == null)
                    return;

                if (Settings.ShowVisual)
                {
                    DrawAnimatedVisual(stats);
                }
            }
            catch (Exception ex)
            {
                HudLog.Fault($"Error in Render: {ex.GetType().Name}: {ex.Message}", "render");
            }
        }

        private KineticFusilladeStats ReadPlayerStats()
        {
            var stats = new KineticFusilladeStats();

            try
            {
                if (Settings.Manual.UseManualValues)
                {
                    // Use manual values from settings
                    stats.CurrentAttackSpeed = Settings.Manual.AttackSpeed;
                    stats.Projectiles = Settings.Manual.Projectiles;
                    stats.DurationModifier = 1 + (Settings.Manual.DurationReduction / 100f);
                }
                else
                {
                    // Try to read from player stats
                    var player = GameController?.Player;
                    if (player == null || !player.IsValid)
                    {
                        return null;
                    }

                    var statsComp = player.GetComponent<LocalStats>();
                    if (statsComp?.StatDictionary != null)
                    {
                        var statDict = statsComp.StatDictionary;

                        // Read attack speed percentage (default to 0 if not found)
                        statDict.TryGetValue(GameStat.AttackSpeedPct, out var attackSpeedPct);

                        // Read duration modifier (can be negative for reduced duration)
                        statDict.TryGetValue(GameStat.VirtualSkillEffectDurationPct, out var durationPct);

                        // Calculate duration modifier (1 + percentage/100)
                        stats.DurationModifier = 1 + (durationPct / 100f);

                        // Calculate attack speed
                        // Base attack time in milliseconds, convert to seconds and then to APS
                        float baseAttackTimeSeconds = Settings.Manual.BaseAttackTime / 1000f;
                        float attackSpeedMultiplier = 1 + (attackSpeedPct / 100f);
                        stats.CurrentAttackSpeed = attackSpeedMultiplier / baseAttackTimeSeconds;

                        // Default projectile count (would need skill gem reading for actual value)
                        stats.Projectiles = 5;  // Default for Kinetic Fusillade
                    }
                    else
                    {
                        // Fallback to manual if stats unavailable
                        return null;
                    }
                }

                // Calculate breakpoint
                CalculateBreakpoint(stats);
            }
            catch (Exception ex)
            {
                HudLog.Fault($"Error reading player stats: {ex.GetType().Name}: {ex.Message}", "stats");
                return null;
            }

            return stats;
        }

        private void CalculateBreakpoint(KineticFusilladeStats stats)
        {
            if (stats == null)
                return;

            // Kinetic Fusillade breakpoint formulas from web tool
            // hovering_time = 0.7 × duration_mod
            stats.HoveringTime = 0.7f * stats.DurationModifier;

            // firing_time = 0.05 × duration_mod × projectiles
            stats.FiringTime = 0.05f * stats.DurationModifier * stats.Projectiles;

            // salvo_time = firing_time + hovering_time
            stats.SalvoTime = stats.FiringTime + stats.HoveringTime;

            // target_speed = 1 / salvo_time (attacks per second)
            stats.TargetAttackSpeed = 1f / stats.SalvoTime;

            // Determine timing status
            float attackPeriod = stats.GetAttackPeriod();
            float difference = Math.Abs(attackPeriod - stats.SalvoTime);
            float toleranceSeconds = stats.SalvoTime * TOLERANCE;

            if (difference <= toleranceSeconds)
            {
                stats.Status = TimingStatus.Perfect;
            }
            else if (attackPeriod < stats.SalvoTime)
            {
                // Attacking faster than salvo completes
                stats.Status = TimingStatus.TooFast;
            }
            else
            {
                // Attacking slower than optimal
                stats.Status = TimingStatus.Suboptimal;
            }
        }

        private void DrawAnimatedVisual(KineticFusilladeStats stats)
        {
            if (stats == null)
                return;

            var windowRect = GameController.Window.GetWindowRectangleTimeCache;
            float scale = Settings.VisualScale;
            int barWidth = (int)(Settings.BarWidth * scale);
            int barHeight = (int)(Settings.BarHeight * scale);

            // Position
            int x = Settings.VisualX;
            int y = Settings.VisualY;

            // Background box
            var bgRect = new RectangleF(x - 10, y - 10, barWidth + 20, barHeight + 120);
            Graphics.DrawBox(bgRect, Settings.Colors.BackgroundColor);
            Graphics.DrawFrame(bgRect, Settings.Colors.TextColor, 2);

            // Title
            var titlePos = new Vector2(x + barWidth / 2 - 100, y);
            Graphics.DrawText("Kinetic Fusillade Breakpoint", titlePos, Settings.Colors.TextColor);

            // Draw gradient bar
            int barY = y + 25;
            DrawGradientBar(x, barY, barWidth, barHeight, stats);

            // Draw animated cursor
            DrawAttackCursor(x, barY, barWidth, barHeight, stats);

            // Draw status text
            int statusY = barY + barHeight + 10;
            DrawStatusText(x, statusY, barWidth, stats);

            // Draw numeric info
            int numericY = statusY + 25;
            DrawNumericInfo(x, numericY, barWidth, stats);
        }

        private void DrawGradientBar(int x, int y, int width, int height, KineticFusilladeStats stats)
        {
            // Calculate zone widths based on timing
            float hoveringRatio = stats.HoveringTime / stats.SalvoTime;
            float firingRatio = stats.FiringTime / stats.SalvoTime;

            int hoveringWidth = (int)(width * hoveringRatio);
            int firingWidth = (int)(width * firingRatio);
            int bufferWidth = width - hoveringWidth - firingWidth;

            // Draw hovering zone (orange)
            var hoveringRect = new RectangleF(x, y, hoveringWidth, height);
            Graphics.DrawBox(hoveringRect, Settings.Colors.HoveringColor);

            // Draw firing zone (red-orange)
            var firingRect = new RectangleF(x + hoveringWidth, y, firingWidth, height);
            Graphics.DrawBox(firingRect, Settings.Colors.FiringColor);

            // Draw buffer zone (green)
            if (bufferWidth > 0)
            {
                var bufferRect = new RectangleF(x + hoveringWidth + firingWidth, y, bufferWidth, height);
                Graphics.DrawBox(bufferRect, Settings.Colors.BufferColor);
            }

            // Draw border
            var borderRect = new RectangleF(x, y, width, height);
            Graphics.DrawFrame(borderRect, Settings.Colors.TextColor, 2);

            // Draw zone labels
            using (Graphics.SetTextScale(0.8f * Settings.VisualScale))
            {
                if (hoveringWidth > 60)
                {
                    var hoverLabel = new Vector2(x + hoveringWidth / 2 - 30, y + height / 2 - 5);
                    Graphics.DrawText("HOVER", hoverLabel, Color.Black);
                }

                if (firingWidth > 40)
                {
                    var fireLabel = new Vector2(x + hoveringWidth + firingWidth / 2 - 20, y + height / 2 - 5);
                    Graphics.DrawText("FIRE", fireLabel, Color.Black);
                }
            }
        }

        private void DrawAttackCursor(int x, int y, int width, int height, KineticFusilladeStats stats)
        {
            // Calculate cursor position based on attack timing
            float attackPeriodMs = stats.GetAttackPeriod() * 1000f;  // Convert to milliseconds
            float elapsed = _animationTimer.ElapsedMilliseconds % attackPeriodMs;
            float progress = elapsed / attackPeriodMs;
            int cursorX = x + (int)(width * progress);

            // Choose cursor color based on status
            Color cursorColor = stats.Status switch
            {
                TimingStatus.Perfect => Settings.Colors.PerfectColor,
                TimingStatus.TooFast => Settings.Colors.DangerColor,
                TimingStatus.Suboptimal => Settings.Colors.WarningColor,
                _ => Color.White
            };

            // Draw cursor line
            var cursorTop = new Vector2(cursorX, y - 5);
            var cursorBottom = new Vector2(cursorX, y + height + 5);
            Graphics.DrawLine(cursorTop, cursorBottom, 3, cursorColor);

            // Draw arrow at top
            Graphics.DrawText("▼", new Vector2(cursorX - 5, y - 15), cursorColor);
        }

        private void DrawStatusText(int x, int y, int width, KineticFusilladeStats stats)
        {
            string statusText = stats.Status switch
            {
                TimingStatus.Perfect => "✓ PERFECT",
                TimingStatus.TooFast => "⚠ TOO FAST - Losing Projectiles!",
                TimingStatus.Suboptimal => "⚠ SUBOPTIMAL - Wasting Potential",
                _ => "UNKNOWN"
            };

            Color statusColor = stats.Status switch
            {
                TimingStatus.Perfect => Settings.Colors.PerfectColor,
                TimingStatus.TooFast => Settings.Colors.DangerColor,
                TimingStatus.Suboptimal => Settings.Colors.WarningColor,
                _ => Color.White
            };

            var textPos = new Vector2(x + width / 2 - 80, y);
            using (Graphics.SetTextScale(0.9f * Settings.VisualScale))
            {
                Graphics.DrawText(statusText, textPos, statusColor);
            }
        }

        private void DrawNumericInfo(int x, int y, int width, KineticFusilladeStats stats)
        {
            using (Graphics.SetTextScale(0.8f * Settings.VisualScale))
            {
                var targetText = $"Target: {stats.TargetAttackSpeed:F2} APS";
                var currentText = $"Current: {stats.CurrentAttackSpeed:F2} APS";
                var diffText = $"Difference: {stats.GetDifference():+0.00;-0.00} APS";

                Graphics.DrawText(targetText, new Vector2(x, y), Settings.Colors.TextColor);
                Graphics.DrawText(currentText, new Vector2(x, y + 15), Settings.Colors.TextColor);
                Graphics.DrawText(diffText, new Vector2(x, y + 30), Settings.Colors.TextColor);

                // Display timing breakdown
                var hoverText = $"Hover: {stats.HoveringTime:F2}s | Fire: {stats.FiringTime:F2}s | Total: {stats.SalvoTime:F2}s";
                Graphics.DrawText(hoverText, new Vector2(x + 150, y + 15), Color.Gray);
            }
        }
    }
}
