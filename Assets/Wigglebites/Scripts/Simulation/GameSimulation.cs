using System;
using System.Collections.Generic;

namespace Wigglebites.Simulation
{
    public sealed class GameSimulation
    {
        private readonly SimulationConfig _config;
        private readonly List<SnakeState> _snakes = new List<SnakeState>();
        private readonly List<FoodState> _food = new List<FoodState>();

        public GameSimulation(SimulationConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public IReadOnlyList<SnakeState> Snakes => _snakes;
        public IReadOnlyList<FoodState> Food => _food;

        public SnakeState AddSnake(int id, bool isPlayer, SimVector2 position, SimVector2 heading)
        {
            if (FindSnake(id) != null) throw new ArgumentException($"Snake id {id} already exists.", nameof(id));

            var snake = new SnakeState(
                id,
                isPlayer,
                position,
                heading,
                _config.BaseSpeed,
                _config.TurnRateRadians,
                _config.StartingMass,
                _config.BodySampleSpacing,
                _config.BaseBodySegments);
            _snakes.Add(snake);
            return snake;
        }

        public FoodState AddFood(int id, SimVector2 position, float massValue)
        {
            var food = new FoodState(id, position, massValue);
            _food.Add(food);
            return food;
        }

        public SnakeState FindSnake(int id)
        {
            for (var index = 0; index < _snakes.Count; index++)
            {
                if (_snakes[index].Id == id) return _snakes[index];
            }

            return null;
        }

        public void SetDesiredHeading(int snakeId, SimVector2 heading)
        {
            var snake = FindSnake(snakeId);
            if (snake == null) throw new ArgumentException($"Unknown snake id {snakeId}.", nameof(snakeId));
            snake.SetDesiredHeading(heading);
        }

        public void Tick(float deltaTime)
        {
            var halfExtent = (_config.ArenaSize * 0.5f) - _config.HeadRadius;
            for (var snakeIndex = 0; snakeIndex < _snakes.Count; snakeIndex++)
            {
                var snake = _snakes[snakeIndex];
                snake.Tick(deltaTime);
                snake.ConstrainToArena(halfExtent);
                ConsumeNearbyFood(snake);
            }
        }

        private void ConsumeNearbyFood(SnakeState snake)
        {
            var pickupDistance = _config.HeadRadius + _config.FoodPickupRadius;
            for (var foodIndex = 0; foodIndex < _food.Count; foodIndex++)
            {
                var item = _food[foodIndex];
                if (!item.IsActive) continue;
                if (SimVector2.Distance(snake.Position, item.Position) > pickupDistance) continue;

                item.Consume();
                snake.AddMass(item.MassValue);
            }
        }
    }
}
