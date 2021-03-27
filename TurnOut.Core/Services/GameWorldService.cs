using System;
using TurnOut.Core.Models;
using TurnOut.Core.Models.Entities;

namespace TurnOut.Core.Services
{
    public class GameWorldService
    {
        private readonly GameInstanceService _gameInstanceService;

        private readonly World world;

        public GameWorldService(GameInstanceService gameInstanceService)
        {
            _gameInstanceService = gameInstanceService;

            world = _gameInstanceService.GameInstance.GameWorld;
        }

        public (int x, int y)? GetEntityPosition(EntityBase entity)
        {
            for (int x = 0; x < world.BoardDimensions.w; ++x)
            {
                for (int y = 0; y < world.BoardDimensions.h; ++y)
                {
                    if (entity.Equals(world.Board[x, y]))
                        return (x, y);
                }
            }
            return null;
        }

        public bool IsValidPosition((int x, int y) position)
        {
            return (position.x >= 0 && position.y >= 0 && position.x < world.BoardDimensions.w && position.y < world.BoardDimensions.h);
        }

        public EntityBase GetEntityAt((int x, int y) position, Predicate<EntityBase> criteria)
        {
            var e = IsValidPosition(position) ? world.Board[position.x, position.y] : null;
            if (e is not null && criteria.Invoke(e)) return e;
            return null;
        }

        public EntityBase GetEntityAt((int x, int y) position)
        {
            return IsValidPosition(position) ? world.Board[position.x, position.y] : null;
        }

        public EntityBase RemoveEntityAt((int x, int y) position) => ReplaceEntityAt(position, null);

        public EntityBase ReplaceEntityAt((int x, int y) position, EntityBase newEntity)
        {
            var oldEntity = GetEntityAt(position);
            world.Board[position.x, position.y] = newEntity;
            return oldEntity;
        }

        public void DisplayAnimation((int x, int y) position, string animation)
        {
            if (!IsValidPosition(position)) return;
            world.Animations[position.x, position.y] = animation;
        }

        public void ClearAnimations()
        {
            world.Animations = new string[world.BoardDimensions.w, world.BoardDimensions.h];
        }
    }
}