using System;
using System.Collections.Generic;
using Wigglebites.Simulation;

namespace Wigglebites.Application
{
    public enum MatchPhase
    {
        Menu,
        Playing,
        Paused,
        Results
    }

    public readonly struct MatchStanding
    {
        public MatchStanding(int snakeId, bool isPlayer, float mass, int eliminations, bool isAlive)
        {
            SnakeId = snakeId;
            IsPlayer = isPlayer;
            Mass = mass;
            Eliminations = eliminations;
            IsAlive = isAlive;
        }

        public int SnakeId { get; }
        public bool IsPlayer { get; }
        public float Mass { get; }
        public int Eliminations { get; }
        public bool IsAlive { get; }
    }

    public sealed class MatchController
    {
        private readonly GameSimulation _simulation;
        private readonly int _playerSnakeId;
        private readonly List<MatchStanding> _standings = new List<MatchStanding>(20);

        public MatchController(GameSimulation simulation, int playerSnakeId, float durationSeconds)
        {
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            if (_simulation.FindSnake(playerSnakeId) == null) throw new ArgumentException("Player snake is missing.", nameof(playerSnakeId));
            if (durationSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            _playerSnakeId = playerSnakeId;
            RemainingSeconds = durationSeconds;
            Phase = MatchPhase.Menu;
            RefreshStandings();
        }

        public MatchPhase Phase { get; private set; }
        public float RemainingSeconds { get; private set; }
        public IReadOnlyList<MatchStanding> Standings => _standings;
        public int PlayerRank { get; private set; }
        public float FinalPlayerMass { get; private set; }
        public int FinalPlayerEliminations { get; private set; }

        public void Start()
        {
            if (Phase != MatchPhase.Menu) throw new InvalidOperationException("Only a menu match can be started.");
            Phase = MatchPhase.Playing;
        }

        public void Advance(float deltaTime)
        {
            if (deltaTime <= 0f) throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (Phase != MatchPhase.Playing) return;

            RemainingSeconds = Math.Max(0f, RemainingSeconds - deltaTime);
            if (RemainingSeconds <= 0f)
            {
                Finish();
                return;
            }
            EvaluateCurrentState();
        }

        public void EvaluateCurrentState()
        {
            if (Phase != MatchPhase.Playing) return;
            RefreshStandings();
            var player = _simulation.FindSnake(_playerSnakeId);
            if (player == null || !player.IsAlive) Finish();
        }

        public void Pause()
        {
            if (Phase == MatchPhase.Playing) Phase = MatchPhase.Paused;
        }

        public void Resume()
        {
            if (Phase == MatchPhase.Paused) Phase = MatchPhase.Playing;
        }

        private void Finish()
        {
            RefreshStandings();
            Phase = MatchPhase.Results;
            for (var index = 0; index < _standings.Count; index++)
            {
                if (!_standings[index].IsPlayer) continue;
                PlayerRank = index + 1;
                FinalPlayerMass = _standings[index].Mass;
                FinalPlayerEliminations = _standings[index].Eliminations;
                break;
            }
        }

        private void RefreshStandings()
        {
            _standings.Clear();
            for (var index = 0; index < _simulation.Snakes.Count; index++)
            {
                var snake = _simulation.Snakes[index];
                _standings.Add(new MatchStanding(
                    snake.Id,
                    snake.IsPlayer,
                    snake.Mass,
                    snake.Eliminations,
                    snake.IsAlive));
            }
            _standings.Sort(CompareStandings);
        }

        private static int CompareStandings(MatchStanding left, MatchStanding right)
        {
            var massOrder = right.Mass.CompareTo(left.Mass);
            if (massOrder != 0) return massOrder;
            var eliminationOrder = right.Eliminations.CompareTo(left.Eliminations);
            if (eliminationOrder != 0) return eliminationOrder;
            return left.SnakeId.CompareTo(right.SnakeId);
        }
    }
}
