using UnityEngine;
using Wigglebites.Application;

namespace Wigglebites.UI
{
    public sealed class RuntimeHud : MonoBehaviour
    {
        private ArenaBootstrap _game;
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _centerStyle;
        private GUIStyle _buttonStyle;

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
            GUI.BeginGroup(new Rect(left, top, width, height));

            DrawHud(width, height);
            if (_game.Match.Phase == MatchPhase.Menu) DrawMenu(width, height);
            else if (_game.Match.Phase == MatchPhase.Paused) DrawPaused(width, height);
            else if (_game.Match.Phase == MatchPhase.Results) DrawResults(width, height);

            GUI.EndGroup();
            GUI.matrix = oldMatrix;
        }

        private void DrawHud(float width, float height)
        {
            if (_game.Match.Phase == MatchPhase.Menu) return;
            var seconds = Mathf.CeilToInt(_game.Match.RemainingSeconds);
            GUI.Label(new Rect((width * 0.5f) - 100f, 28f, 200f, 64f), $"{seconds / 60}:{seconds % 60:00}", _titleStyle);

            var player = _game.Player;
            if (player != null)
            {
                GUI.Label(new Rect(30f, 34f, 260f, 52f), $"Mass {player.Mass:0}  •  KOs {player.Eliminations}", _labelStyle);
            }

            GUI.Label(new Rect(width - 290f, 94f, 250f, 44f), "TOP 5", _centerStyle);
            var count = Mathf.Min(5, _game.Match.Standings.Count);
            for (var index = 0; index < count; index++)
            {
                var standing = _game.Match.Standings[index];
                var name = standing.IsPlayer ? "YOU" : $"BOT {standing.SnakeId - 1}";
                GUI.Label(
                    new Rect(width - 310f, 138f + (index * 40f), 270f, 38f),
                    $"{index + 1}. {name}   {standing.Mass:0}",
                    _labelStyle);
            }

            if (_game.Match.Phase == MatchPhase.Playing &&
                GUI.Button(new Rect(width - 150f, 26f, 120f, 58f), "Pause", _buttonStyle))
            {
                _game.TogglePause();
            }

            if (_game.Match.Phase == MatchPhase.Playing)
            {
                GUI.Label(new Rect(30f, height - 110f, 250f, 58f), "DRAG TO STEER", _centerStyle);
                GUI.Label(new Rect(width - 280f, height - 110f, 250f, 58f), "HOLD TO BOOST", _centerStyle);
            }
        }

        private void DrawMenu(float width, float height)
        {
            DrawPanel(width, height, 470f, 360f);
            GUI.Label(new Rect((width * 0.5f) - 200f, (height * 0.5f) - 130f, 400f, 70f), "WIGGLEBITES", _titleStyle);
            GUI.Label(new Rect((width * 0.5f) - 190f, (height * 0.5f) - 55f, 380f, 60f), "Grow big. Stay wiggly.\nThree-minute offline arena.", _centerStyle);
            if (GUI.Button(new Rect((width * 0.5f) - 140f, (height * 0.5f) + 45f, 280f, 82f), "PLAY", _buttonStyle))
            {
                _game.StartMatch();
            }
        }

        private void DrawPaused(float width, float height)
        {
            DrawPanel(width, height, 430f, 330f);
            GUI.Label(new Rect((width * 0.5f) - 180f, (height * 0.5f) - 125f, 360f, 70f), "PAUSED", _titleStyle);
            if (GUI.Button(new Rect((width * 0.5f) - 140f, (height * 0.5f) - 30f, 280f, 72f), "RESUME", _buttonStyle))
            {
                _game.TogglePause();
            }
            if (GUI.Button(new Rect((width * 0.5f) - 140f, (height * 0.5f) + 62f, 280f, 72f), "RESTART", _buttonStyle))
            {
                _game.Replay();
            }
        }

        private void DrawResults(float width, float height)
        {
            DrawPanel(width, height, 510f, 490f);
            GUI.Label(new Rect((width * 0.5f) - 210f, (height * 0.5f) - 205f, 420f, 70f), "ROUND COMPLETE", _titleStyle);
            GUI.Label(
                new Rect((width * 0.5f) - 200f, (height * 0.5f) - 115f, 400f, 180f),
                $"Rank  #{_game.Match.PlayerRank}\nMass  {_game.Match.FinalPlayerMass:0}\nKOs  {_game.Match.FinalPlayerEliminations}\nBest mass  {_game.BestMass:0}",
                _centerStyle);
            if (GUI.Button(new Rect((width * 0.5f) - 150f, (height * 0.5f) + 95f, 300f, 82f), "PLAY AGAIN", _buttonStyle))
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
        }
    }
}
