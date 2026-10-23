using UnityEngine;
using Wigglebites.Application;

namespace Wigglebites.UI
{
    public sealed class RuntimeHud : MonoBehaviour
    {
        private static readonly string[] HeadingArrows = { "→", "↗", "↑", "↖", "←", "↙", "↓", "↘" };

        private ArenaBootstrap _game;
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _centerStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _controlStyle;

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
            var size = Mathf.Min(Mathf.Max(150f, touchHeight * 1.65f), Mathf.Min(230f, availableWidth));
            var top = Mathf.Max(0f, height - size - bottomMargin);
            return new TouchControlLayout(
                new Rect(horizontalMargin, top, size, size),
                new Rect(width - horizontalMargin - size, top, size, size));
        }

        private void DrawHud(float width, float height, float touchHeight)
        {
            if (_game.Match.Phase == MatchPhase.Menu) return;
            var seconds = Mathf.CeilToInt(_game.Match.RemainingSeconds);
            GUI.Label(new Rect((width * 0.5f) - 100f, 28f, 200f, 64f), $"{seconds / 60}:{seconds % 60:00}", _titleStyle);

            var player = _game.Player;
            if (player != null)
            {
                GUI.Label(new Rect(30f, 34f, 280f, 52f), $"Massa {player.Mass:0}  •  Utslagna {player.Eliminations}", _labelStyle);
            }

            var leaderboardTop = Mathf.Max(94f, 46f + touchHeight);
            GUI.Label(new Rect(width - 290f, leaderboardTop, 250f, 44f), "TOPP 5", _centerStyle);
            var count = Mathf.Min(5, _game.Match.Standings.Count);
            for (var index = 0; index < count; index++)
            {
                var standing = _game.Match.Standings[index];
                var name = standing.IsPlayer ? "DU" : $"BOT {standing.SnakeId - 1}";
                GUI.Label(
                    new Rect(width - 310f, leaderboardTop + 44f + (index * 40f), 270f, 38f),
                    $"{index + 1}. {name}   {standing.Mass:0}",
                    _labelStyle);
            }

            var pauseWidth = Mathf.Max(120f, touchHeight);
            if (_game.Match.Phase == MatchPhase.Playing &&
                GUI.Button(new Rect(width - pauseWidth - 30f, 26f, pauseWidth, touchHeight), "Paus", _buttonStyle))
            {
                _game.TogglePause();
            }

            if (_game.Match.Phase == MatchPhase.Playing)
            {
                DrawTouchControls(width, height, touchHeight);
            }
        }

        private void DrawTouchControls(float width, float height, float touchHeight)
        {
            var layout = CalculateTouchControlLayout(width, height, touchHeight);
            var oldColor = GUI.color;

            GUI.color = new Color(0.15f, 0.75f, 1f, 0.72f);
            GUI.Box(layout.Steering, $"STYR\n{HeadingArrow(_game.DesiredHeading)}", _controlStyle);

            GUI.color = _game.IsBoostHeld
                ? new Color(1f, 0.78f, 0.15f, 0.92f)
                : new Color(1f, 0.45f, 0.2f, 0.72f);
            GUI.Box(layout.Boost, _game.IsBoostHeld ? "TURBO\nAKTIV" : "TURBO\nHÅLL", _controlStyle);

            GUI.color = oldColor;
        }

        public static string HeadingArrow(Wigglebites.Simulation.SimVector2 heading)
        {
            var angle = Mathf.Atan2(heading.Y, heading.X) * Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;
            var direction = Mathf.RoundToInt(angle / 45f) % 8;
            return HeadingArrows[direction];
        }

        private void DrawMenu(float width, float height, float touchHeight)
        {
            var panelWidth = Mathf.Min(470f, width - 30f);
            var panelHeight = Mathf.Max(570f, (touchHeight * 3f) + 300f);
            var buttonWidth = Mathf.Min(420f, panelWidth - 40f);
            DrawPanel(width, height, panelWidth, panelHeight);
            var panelTop = (height - panelHeight) * 0.5f;
            var firstButtonY = panelTop + 200f;
            GUI.Label(new Rect((width * 0.5f) - 200f, panelTop + 30f, 400f, 70f), "WIGGLEBITES", _titleStyle);
            GUI.Label(new Rect((width * 0.5f) - 190f, panelTop + 110f, 380f, 60f), "Väx dig stor. Fortsätt slingra.\nTre minuter, helt offline.", _centerStyle);
            if (GUI.Button(new Rect((width - buttonWidth) * 0.5f, firstButtonY, buttonWidth, touchHeight), _game.ReducedEffects ? "[FX] EFFEKTER: MINSKADE" : "[FX] EFFEKTER: FULLA", _buttonStyle))
            {
                _game.ToggleReducedEffects();
            }
            if (GUI.Button(new Rect((width - buttonWidth) * 0.5f, firstButtonY + touchHeight + 18f, buttonWidth, touchHeight), _game.SoundEnabled ? "[LJUD] PÅ" : "[LJUD] AV", _buttonStyle))
            {
                _game.ToggleSound();
            }
            if (GUI.Button(new Rect((width - buttonWidth) * 0.5f, firstButtonY + ((touchHeight + 18f) * 2f), buttonWidth, touchHeight), "SPELA", _buttonStyle))
            {
                _game.StartMatch();
            }
        }

        private void DrawPaused(float width, float height, float touchHeight)
        {
            var panelWidth = Mathf.Min(430f, width - 30f);
            var panelHeight = Mathf.Max(590f, (touchHeight * 4f) + 220f);
            var buttonWidth = Mathf.Min(390f, panelWidth - 40f);
            DrawPanel(width, height, panelWidth, panelHeight);
            var panelTop = (height - panelHeight) * 0.5f;
            GUI.Label(new Rect((width * 0.5f) - 180f, panelTop + 30f, 360f, 70f), "PAUS", _titleStyle);
            var firstButtonY = panelTop + 120f;
            if (GUI.Button(new Rect((width - buttonWidth) * 0.5f, firstButtonY, buttonWidth, touchHeight), "FORTSÄTT", _buttonStyle))
            {
                _game.TogglePause();
            }
            if (GUI.Button(new Rect((width - buttonWidth) * 0.5f, firstButtonY + touchHeight + 18f, buttonWidth, touchHeight), "STARTA OM", _buttonStyle))
            {
                _game.Replay();
            }
            if (GUI.Button(new Rect((width - buttonWidth) * 0.5f, firstButtonY + ((touchHeight + 18f) * 2f), buttonWidth, touchHeight), _game.ReducedEffects ? "[FX] EFFEKTER: MINSKADE" : "[FX] EFFEKTER: FULLA", _buttonStyle))
            {
                _game.ToggleReducedEffects();
            }
            if (GUI.Button(new Rect((width - buttonWidth) * 0.5f, firstButtonY + ((touchHeight + 18f) * 3f), buttonWidth, touchHeight), _game.SoundEnabled ? "[LJUD] PÅ" : "[LJUD] AV", _buttonStyle))
            {
                _game.ToggleSound();
            }
        }

        private void DrawResults(float width, float height, float touchHeight)
        {
            var panelWidth = Mathf.Min(510f, width - 30f);
            var panelHeight = Mathf.Max(490f, touchHeight + 410f);
            var buttonWidth = Mathf.Min(300f, panelWidth - 60f);
            DrawPanel(width, height, panelWidth, panelHeight);
            GUI.Label(new Rect((width * 0.5f) - 210f, (height * 0.5f) - (panelHeight * 0.5f) + 30f, 420f, 70f), "RUNDAN ÄR KLAR", _titleStyle);
            GUI.Label(
                new Rect((width * 0.5f) - 200f, (height * 0.5f) - 115f, 400f, 180f),
                $"Placering  #{_game.Match.PlayerRank}\nMassa  {_game.Match.FinalPlayerMass:0}\nUtslagna  {_game.Match.FinalPlayerEliminations}\nBästa massa  {_game.BestMass:0}",
                _centerStyle);
            if (GUI.Button(new Rect((width - buttonWidth) * 0.5f, (height * 0.5f) + 95f, buttonWidth, touchHeight), "SPELA IGEN", _buttonStyle))
            {
                _game.Replay();
            }
        }

        private static void DrawPanel(float width, float height, float panelWidth, float panelHeight)
        {
            var oldColor = GUI.color;
            GUI.color = new Color(0.04f, 0.08f, 0.18f, 0.94f);
            GUI.Box(new Rect((width - panelWidth) * 0.5f, (height - panelHeight) * 0.5f, panelWidth, panelHeight), GUIContent.none);
            GUI.color = oldColor;
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null) return;
            _titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 42, fontStyle = FontStyle.Bold };
            _labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontSize = 24, fontStyle = FontStyle.Bold };
            _centerStyle = new GUIStyle(_labelStyle) { alignment = TextAnchor.MiddleCenter };
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 28, fontStyle = FontStyle.Bold };
            _controlStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
        }
    }
}
