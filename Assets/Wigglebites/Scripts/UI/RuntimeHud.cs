using UnityEngine;
using Wigglebites.Application;

namespace Wigglebites.UI
{
    public sealed class RuntimeHud : MonoBehaviour
    {
        private static readonly string[] HeadingArrows = { "→", "↗", "↑", "↖", "←", "↙", "↓", "↘" };
        private static readonly string[] HeadingLabels =
        {
            "STYR\n→", "STYR\n↗", "STYR\n↑", "STYR\n↖",
            "STYR\n←", "STYR\n↙", "STYR\n↓", "STYR\n↘"
        };
        private const string BoostIdleLabel = "TURBO\nHÅLL";
        private const string BoostPendingLabel = "TURBO\nSTARTAR";
        private const string BoostActiveLabel = "TURBO\nAKTIV";
        private const string BoostUnavailableLabel = "TURBO\nMER MASS";

        private ArenaBootstrap _game;
        private GUIStyle _brandStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _centerStyle;
        private GUIStyle _captionStyle;
        private GUIStyle _primaryButtonStyle;
        private GUIStyle _cyanButtonStyle;
        private GUIStyle _violetButtonStyle;
        private GUIStyle _smallButtonStyle;
        private GUIStyle _panelStyle;
        private GUIStyle _hudPanelStyle;
        private GUIStyle _controlStyle;
        private Texture2D _screenWash;

        public readonly struct TouchControlLayout
        {
            public TouchControlLayout(Rect steering, Rect boost)
            {
                Steering = steering;
                Boost = boost;
            }

            public Rect Steering { get; }
            public Rect Boost { get; }
        }

        public readonly struct MenuLayout
        {
            public MenuLayout(Rect panel, Rect play, Rect effects, Rect sound)
            {
                Panel = panel;
                Play = play;
                Effects = effects;
                Sound = sound;
            }

            public Rect Panel { get; }
            public Rect Play { get; }
            public Rect Effects { get; }
            public Rect Sound { get; }
        }

        public void Configure(ArenaBootstrap game)
        {
            _game = game;
        }

        private void OnGUI()
        {
            if (_game == null || _game.Match == null) return;
            EnsureStyles();

            var scale = Mathf.Max(0.6f, Screen.width / 1080f);
            var oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            var safeArea = Screen.safeArea;
            var width = safeArea.width / scale;
            var height = safeArea.height / scale;
            var left = safeArea.xMin / scale;
            var top = (Screen.height - safeArea.yMax) / scale;
            var touchHeight = MinimumTouchHeight(scale, Screen.dpi);
            GUI.BeginGroup(new Rect(left, top, width, height));

            DrawHud(width, height, touchHeight);
            if (_game.Match.Phase == MatchPhase.Menu) DrawMenu(width, height, touchHeight);
            else if (_game.Match.Phase == MatchPhase.Paused) DrawPaused(width, height, touchHeight);
            else if (_game.Match.Phase == MatchPhase.Results) DrawResults(width, height, touchHeight);

            GUI.EndGroup();
            GUI.matrix = oldMatrix;
        }

        public static float MinimumTouchHeight(float canvasScale, float dpi)
        {
            if (canvasScale <= 0f) return 82f;
            return dpi > 0f ? Mathf.Max(58f, (56f * dpi / 160f) / canvasScale) : 82f;
        }

        public static TouchControlLayout CalculateTouchControlLayout(float width, float height, float touchHeight)
        {
            const float horizontalMargin = 30f;
            const float bottomMargin = 26f;
            const float gap = 30f;
            var availableWidth = Mathf.Max(touchHeight, (width - (horizontalMargin * 2f) - gap) * 0.5f);
            var rightTouchZoneWidth = Mathf.Max(touchHeight, (width * 0.4f) - horizontalMargin);
            var size = Mathf.Min(
                Mathf.Max(150f, touchHeight * 1.65f),
                Mathf.Min(230f, Mathf.Min(availableWidth, rightTouchZoneWidth)));
            size = Mathf.Min(size, Mathf.Max(0f, height - bottomMargin));
            var top = Mathf.Max(0f, height - size - bottomMargin);
            return new TouchControlLayout(
                new Rect(horizontalMargin, top, size, size),
                new Rect(width - horizontalMargin - size, top, size, size));
        }

        public static MenuLayout CalculateMenuLayout(float width, float height, float touchHeight)
        {
            var panelWidth = Mathf.Min(620f, width - 32f);
            var desiredPanelHeight = Mathf.Max(700f, (touchHeight * 2f) + 520f);
            var panelHeight = Mathf.Min(height - 32f, desiredPanelHeight);
            var panel = new Rect((width - panelWidth) * 0.5f, (height - panelHeight) * 0.5f, panelWidth, panelHeight);
            var contentLeft = panel.x + 32f;
            var contentWidth = panel.width - 64f;
            var buttonHeight = Mathf.Max(touchHeight, 94f);
            var play = new Rect(contentLeft, panel.y + Mathf.Min(300f, panel.height * 0.42f), contentWidth, buttonHeight);
            var settingsTop = play.yMax + 78f;
            var gap = 18f;
            var settingWidth = (contentWidth - gap) * 0.5f;
            var settingsHeight = Mathf.Max(touchHeight, 96f);
            return new MenuLayout(
                panel,
                play,
                new Rect(contentLeft, settingsTop, settingWidth, settingsHeight),
                new Rect(contentLeft + settingWidth + gap, settingsTop, settingWidth, settingsHeight));
        }

        private void DrawHud(float width, float height, float touchHeight)
        {
            if (_game.Match.Phase == MatchPhase.Menu) return;
            var seconds = Mathf.CeilToInt(_game.Match.RemainingSeconds);
            var timerRect = new Rect((width * 0.5f) - 112f, 22f, 224f, 76f);
            GUI.Box(timerRect, GUIContent.none, _hudPanelStyle);
            GUI.Label(timerRect, $"{seconds / 60}:{seconds % 60:00}", _titleStyle);

            var player = _game.Player;
            if (player != null)
            {
                var statRect = new Rect(22f, 24f, 350f, 72f);
                GUI.Box(statRect, GUIContent.none, _hudPanelStyle);
                GUI.Label(new Rect(statRect.x + 20f, statRect.y, statRect.width - 40f, statRect.height), $"MASSA {player.Mass:0}   •   UTSLAGNA {player.Eliminations}", _labelStyle);
            }

            var pauseWidth = Mathf.Max(132f, touchHeight);
            if (_game.Match.Phase == MatchPhase.Playing &&
                GUI.Button(new Rect(width - pauseWidth - 24f, 25f, pauseWidth, Mathf.Max(68f, touchHeight)), "PAUS", _smallButtonStyle))
            {
                _game.TogglePause();
            }

            var leaderboardTop = Mathf.Max(118f, 44f + touchHeight);
            var leaderboardRect = new Rect(width - 324f, leaderboardTop, 300f, 270f);
            GUI.Box(leaderboardRect, GUIContent.none, _hudPanelStyle);
            GUI.Label(new Rect(leaderboardRect.x + 18f, leaderboardRect.y + 10f, leaderboardRect.width - 36f, 44f), "TOPP 5", _centerStyle);
            var count = Mathf.Min(5, _game.Match.Standings.Count);
            for (var index = 0; index < count; index++)
            {
                var standing = _game.Match.Standings[index];
                var name = standing.IsPlayer ? "DU" : $"BOT {standing.SnakeId - 1}";
                GUI.Label(
                    new Rect(leaderboardRect.x + 20f, leaderboardRect.y + 54f + (index * 39f), leaderboardRect.width - 40f, 38f),
                    $"{index + 1}.  {name}     {standing.Mass:0}",
                    _labelStyle);
            }

            if (_game.Match.Phase == MatchPhase.Playing) DrawTouchControls(width, height, touchHeight);
        }

        private void DrawTouchControls(float width, float height, float touchHeight)
        {
            var layout = CalculateTouchControlLayout(width, height, touchHeight);
            var oldColor = GUI.color;
            GUI.color = new Color(0.35f, 0.88f, 1f, 0.88f);
            GUI.Box(layout.Steering, HeadingLabel(_game.DesiredHeading), _controlStyle);

            GUI.color = BoostColor(_game.IsBoostHeld, _game.IsPlayerBoosting, _game.CanPlayerBoost);
            GUI.Box(
                layout.Boost,
                BoostLabel(_game.IsBoostHeld, _game.IsPlayerBoosting, _game.CanPlayerBoost),
                _controlStyle);
            GUI.color = oldColor;
        }

        public static string HeadingArrow(Wigglebites.Simulation.SimVector2 heading)
        {
            return HeadingArrows[HeadingDirection(heading)];
        }

        private static string HeadingLabel(Wigglebites.Simulation.SimVector2 heading)
        {
            return HeadingLabels[HeadingDirection(heading)];
        }

        private static int HeadingDirection(Wigglebites.Simulation.SimVector2 heading)
        {
            var angle = Mathf.Atan2(heading.Y, heading.X) * Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;
            return Mathf.RoundToInt(angle / 45f) % 8;
        }

        public static string BoostLabel(bool held, bool boosting, bool canBoost)
        {
            if (boosting) return BoostActiveLabel;
            if (!held) return BoostIdleLabel;
            return canBoost ? BoostPendingLabel : BoostUnavailableLabel;
        }

        private static Color BoostColor(bool held, bool boosting, bool canBoost)
        {
            if (boosting) return new Color(1f, 0.86f, 0.25f, 0.98f);
            if (held && !canBoost) return new Color(0.65f, 0.68f, 0.76f, 0.86f);
            return held
                ? new Color(1f, 0.58f, 0.18f, 0.94f)
                : new Color(1f, 0.34f, 0.65f, 0.88f);
        }

        private void DrawMenu(float width, float height, float touchHeight)
        {
            GUI.DrawTexture(new Rect(0f, 0f, width, height), _screenWash, ScaleMode.StretchToFill);
            var layout = CalculateMenuLayout(width, height, touchHeight);
            DrawPanel(layout.Panel);

            GUI.Label(new Rect(layout.Panel.x + 28f, layout.Panel.y + 42f, layout.Panel.width - 56f, 40f), "NEON SNAKE ARENA", _captionStyle);
            GUI.Label(new Rect(layout.Panel.x + 28f, layout.Panel.y + 78f, layout.Panel.width - 56f, 96f), "WIGGLEBITES", _brandStyle);
            GUI.Label(
                new Rect(layout.Panel.x + 42f, layout.Panel.y + 174f, layout.Panel.width - 84f, 84f),
                "Samla godis. Väx dig stor.\nBit svansen — huvudet lever vidare!",
                _centerStyle);

            if (GUI.Button(layout.Play, "SPELA NU", _primaryButtonStyle)) _game.StartMatch();

            GUI.Label(
                new Rect(layout.Panel.x + 32f, layout.Play.yMax + 28f, layout.Panel.width - 64f, 42f),
                "SNABBA INSTÄLLNINGAR",
                _captionStyle);
            if (GUI.Button(
                    layout.Effects,
                    _game.ReducedEffects ? "EFFEKTER\nMINSKADE" : "EFFEKTER\nFULLA",
                    _cyanButtonStyle))
            {
                _game.ToggleReducedEffects();
            }
            if (GUI.Button(
                    layout.Sound,
                    _game.SoundEnabled ? "LJUD\nPÅ" : "LJUD\nAV",
                    _violetButtonStyle))
            {
                _game.ToggleSound();
            }

            GUI.Label(
                new Rect(layout.Panel.x + 38f, layout.Panel.yMax - 72f, layout.Panel.width - 76f, 44f),
                "3 MINUTER  •  OFFLINE  •  BARNVÄNLIGT",
                _captionStyle);
        }

        private void DrawPaused(float width, float height, float touchHeight)
        {
            GUI.DrawTexture(new Rect(0f, 0f, width, height), _screenWash, ScaleMode.StretchToFill);
            var panelWidth = Mathf.Min(520f, width - 32f);
            var panelHeight = Mathf.Min(height - 32f, Mathf.Max(700f, (touchHeight * 4f) + 300f));
            var panel = new Rect((width - panelWidth) * 0.5f, (height - panelHeight) * 0.5f, panelWidth, panelHeight);
            DrawPanel(panel);
            GUI.Label(new Rect(panel.x + 24f, panel.y + 42f, panel.width - 48f, 76f), "PAUS", _brandStyle);
            GUI.Label(new Rect(panel.x + 32f, panel.y + 118f, panel.width - 64f, 56f), "Ta en paus. Din orm väntar här.", _centerStyle);

            var buttonHeight = Mathf.Max(touchHeight, 86f);
            var buttonRect = new Rect(panel.x + 32f, panel.y + 210f, panel.width - 64f, buttonHeight);
            if (GUI.Button(buttonRect, "FORTSÄTT SPELA", _primaryButtonStyle)) _game.TogglePause();
            buttonRect.y += buttonHeight + 18f;
            if (GUI.Button(buttonRect, "STARTA OM", _cyanButtonStyle)) _game.Replay();
            buttonRect.y += buttonHeight + 18f;
            if (GUI.Button(buttonRect, _game.ReducedEffects ? "EFFEKTER: MINSKADE" : "EFFEKTER: FULLA", _cyanButtonStyle)) _game.ToggleReducedEffects();
            buttonRect.y += buttonHeight + 18f;
            if (GUI.Button(buttonRect, _game.SoundEnabled ? "LJUD: PÅ" : "LJUD: AV", _violetButtonStyle)) _game.ToggleSound();
        }

        private void DrawResults(float width, float height, float touchHeight)
        {
            GUI.DrawTexture(new Rect(0f, 0f, width, height), _screenWash, ScaleMode.StretchToFill);
            var panelWidth = Mathf.Min(560f, width - 32f);
            var panelHeight = Mathf.Min(height - 32f, Mathf.Max(650f, touchHeight + 520f));
            var panel = new Rect((width - panelWidth) * 0.5f, (height - panelHeight) * 0.5f, panelWidth, panelHeight);
            DrawPanel(panel);
            GUI.Label(new Rect(panel.x + 24f, panel.y + 38f, panel.width - 48f, 42f), "RUNDAN ÄR KLAR", _captionStyle);
            GUI.Label(new Rect(panel.x + 24f, panel.y + 78f, panel.width - 48f, 86f), $"PLATS #{_game.Match.PlayerRank}", _brandStyle);
            GUI.Label(
                new Rect(panel.x + 42f, panel.y + 190f, panel.width - 84f, 230f),
                $"SLUTMASSA   {_game.Match.FinalPlayerMass:0}\n\nUTSLAGNA   {_game.Match.FinalPlayerEliminations}\n\nBÄSTA MASSA   {_game.BestMass:0}",
                _centerStyle);
            var replay = new Rect(panel.x + 42f, panel.yMax - Mathf.Max(touchHeight, 96f) - 42f, panel.width - 84f, Mathf.Max(touchHeight, 96f));
            if (GUI.Button(replay, "SPELA IGEN", _primaryButtonStyle)) _game.Replay();
        }

        private void DrawPanel(Rect panel)
        {
            var oldColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.38f);
            GUI.Box(new Rect(panel.x + 9f, panel.y + 12f, panel.width, panel.height), GUIContent.none, _panelStyle);
            GUI.color = Color.white;
            GUI.Box(panel, GUIContent.none, _panelStyle);
            GUI.color = oldColor;
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null) return;

            _screenWash = CreateSolidTexture(new Color(0.015f, 0.025f, 0.08f, 0.62f));
            _brandStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 52,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.86f, 0.2f) }
            };
            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            _centerStyle = new GUIStyle(_labelStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = new Color(0.9f, 0.95f, 1f) }
            };
            _captionStyle = new GUIStyle(_labelStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 19,
                normal = { textColor = new Color(0.38f, 0.9f, 1f) }
            };
            _primaryButtonStyle = CreateButtonStyle(
                new Color(1f, 0.68f, 0.05f),
                new Color(1f, 0.91f, 0.35f),
                new Color(1f, 0.79f, 0.12f),
                new Color(0.95f, 0.49f, 0.03f));
            _cyanButtonStyle = CreateButtonStyle(
                new Color(0.03f, 0.49f, 0.83f),
                new Color(0.15f, 0.87f, 1f),
                new Color(0.04f, 0.63f, 0.94f),
                new Color(0.03f, 0.36f, 0.66f));
            _violetButtonStyle = CreateButtonStyle(
                new Color(0.48f, 0.16f, 0.82f),
                new Color(0.95f, 0.28f, 0.91f),
                new Color(0.62f, 0.21f, 0.94f),
                new Color(0.34f, 0.1f, 0.65f));
            _smallButtonStyle = new GUIStyle(_violetButtonStyle) { fontSize = 20 };
            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal =
                {
                    background = CreateRoundedTexture(
                        192,
                        192,
                        28,
                        new Color(0.035f, 0.065f, 0.18f, 0.97f),
                        new Color(0.1f, 0.76f, 1f, 0.92f),
                        4)
                },
                border = new RectOffset(32, 32, 32, 32)
            };
            _hudPanelStyle = new GUIStyle(GUI.skin.box)
            {
                normal =
                {
                    background = CreateRoundedTexture(
                        160,
                        96,
                        22,
                        new Color(0.025f, 0.055f, 0.16f, 0.9f),
                        new Color(0.13f, 0.65f, 1f, 0.8f),
                        3)
                },
                border = new RectOffset(24, 24, 24, 24)
            };
            _controlStyle = new GUIStyle(_hudPanelStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
        }

        private static GUIStyle CreateButtonStyle(Color fill, Color border, Color hover, Color pressed)
        {
            return new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                border = new RectOffset(28, 28, 28, 28),
                normal =
                {
                    background = CreateRoundedTexture(192, 96, 28, fill, border, 4),
                    textColor = Color.white
                },
                hover =
                {
                    background = CreateRoundedTexture(192, 96, 28, hover, border, 4),
                    textColor = Color.white
                },
                active =
                {
                    background = CreateRoundedTexture(192, 96, 28, pressed, border, 4),
                    textColor = Color.white
                },
                focused =
                {
                    background = CreateRoundedTexture(192, 96, 28, hover, Color.white, 5),
                    textColor = Color.white
                }
            };
        }

        private static Texture2D CreateSolidTexture(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixel(0, 0, color);
            texture.Apply(false, true);
            return texture;
        }

        private static Texture2D CreateRoundedTexture(
            int width,
            int height,
            int radius,
            Color fill,
            Color border,
            int borderWidth)
        {
            var pixels = new Color[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (!InsideRoundedRect(x, y, width, height, radius))
                    {
                        pixels[(y * width) + x] = Color.clear;
                        continue;
                    }

                    var insideFill = InsideRoundedRect(
                        x - borderWidth,
                        y - borderWidth,
                        width - (borderWidth * 2),
                        height - (borderWidth * 2),
                        Mathf.Max(1, radius - borderWidth));
                    pixels[(y * width) + x] = insideFill ? fill : border;
                }
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static bool InsideRoundedRect(int x, int y, int width, int height, int radius)
        {
            if (x < 0 || y < 0 || x >= width || y >= height) return false;
            var closestX = Mathf.Clamp(x + 0.5f, radius, width - radius);
            var closestY = Mathf.Clamp(y + 0.5f, radius, height - radius);
            var deltaX = (x + 0.5f) - closestX;
            var deltaY = (y + 0.5f) - closestY;
            return (deltaX * deltaX) + (deltaY * deltaY) <= radius * radius;
        }
    }
}
