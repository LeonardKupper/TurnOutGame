using System;
using System.Collections.Generic;
using System.Linq;
using TurnOut.Core.Models;
using TurnOut.Core.Models.Entities;

namespace TurnOut.Core.Services
{
    public class FieldOfViewService
    {
        const double CONV_DEG = (180 / Math.PI);
        private readonly GameInstanceService _gameInstanceService;
        private readonly GameWorldService _gameWorldService;
        private readonly UnitMoveService _unitMoveService;

        public FieldOfViewService(GameInstanceService gameInstanceService, GameWorldService gameWorldService, UnitMoveService unitMoveService)
        {
            _gameInstanceService = gameInstanceService;
            _gameWorldService = gameWorldService;
            _unitMoveService = unitMoveService;
        }

        /// <summary>
        /// For a vector with x and y components this computes an absolute angle in degrees, where
        /// NORTH (x=0, y=-1) becomes 0°, EAST (x=1, y=0) becomes 90°, ...
        /// </summary>
        /// <param name="vector">The input vector.</param>
        /// <returns>An angle in degrees</returns>
        private double GetNorthBasedAbsDeg((float x, float y) vector)
        {
            // special case of x = 0, prevent division by zero
            if (vector.x == 0)
            {
                if (vector.y <= 0) return 0;
                else return 180;
            }
            return (CONV_DEG * Math.Atan(vector.y / vector.x)) + ((vector.x < 0) ? 270 : 90);
        }

        /// <summary>
        /// Takes two angles given in degrees and computes the deviation , where clockwise deviation
        /// ranges from 0° to +180° and counter-clockwise deviation from 0° to -180°
        /// </summary>
        /// <param name="theta">The deviating angle in degrees.</param>
        /// <param name="phi">The reference angle in degrees.</param>
        /// <returns>An angle between minus and plus 180°.</returns>
        private double GetRelativeDeg(double theta, double phi)
        {
            double rho = theta - phi;
            if (rho <= -180)
                return rho + 360;
            if (rho > 180)
                return rho - 360;
            return rho;
        }

        private double GetEuclidDist(float dx, float dy)
        {
            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        private bool CheckCoordinateVisibility((float x, float y) coords, FieldOfView observer, List<(int x, int y)> blockedPositions)
        {
            (float x, float y) viewVector = (coords.x - observer.Position.x, coords.y - observer.Position.y);
            
            // Check by distance
            double distance = GetEuclidDist(viewVector.x, viewVector.y);
            if (distance > observer.MaxViewDistance) return false;

            // Check by angle
            double absTargetAngle = GetNorthBasedAbsDeg(viewVector);
            double absObserverAngle = GetNorthBasedAbsDeg(observer.FacingDirection);
            double relTargetAngle = GetRelativeDeg(absTargetAngle, absObserverAngle);
            if (Math.Abs(relTargetAngle) > (observer.FovAngle * 0.5))
                return false;

            // Check by obstacle blocking
            foreach (var block in blockedPositions)
            {
                var blockCoords = (x: block.x + 0.5f, y: block.y + 0.5f);
                var blockVector = (x: blockCoords.x - observer.Position.x, y: blockCoords.y - observer.Position.y);
                var blockDistance = GetEuclidDist(blockVector.x, blockVector.y);
                if (blockDistance < distance)
                {
                    // could be in front of target, compute blocked angle range from all 4 corners
                    (double lower, double upper) blockedAngleRange = (180, -180);
                    for (int i = 0; (i < 4); i++)
                    {
                        float x = block.x + (i < 2 ? 0.0f : 1.0f);
                        float y = block.y + ((i % 2 == 0) ? 0.0f : 1.0f);
                        var cornerVector = (x: x - observer.Position.x, y: y - observer.Position.y);
                        var absCornerAngle = GetNorthBasedAbsDeg(cornerVector);
                        var relCornerAngle = GetRelativeDeg(absCornerAngle, absObserverAngle);
                        // Set min and max for range accordingly
                        blockedAngleRange.lower = Math.Min(blockedAngleRange.lower, relCornerAngle);
                        blockedAngleRange.upper = Math.Max(blockedAngleRange.upper, relCornerAngle);
                    }
                    if ((blockedAngleRange.lower < -90.1f) && (blockedAngleRange.upper > 90.1f))
                    {
                        // obstacle is actually behind observer.
                        continue;
                    }
                    if (relTargetAngle > blockedAngleRange.lower && relTargetAngle < blockedAngleRange.upper)
                    {
                        // The coordinate is in the blocked angle range, hence not visible
                        return false;
                    }
                }
            }

            return true;
        }

        private bool CheckMapPositionVisibility((int x, int y) mapPos, List<FieldOfView> observers, List<(int x, int y)> blockedPositions)
        {
            // Test if at least one test coordinate is visible
            for (int i = 0; (i < 4); i++)
            {
                float x = mapPos.x + (i < 2 ? 0.25f : 0.75f);
                float y = mapPos.y + ((i % 2 == 0) ? 0.25f : 0.75f);
                foreach (var observer in observers)
                {
                    if (CheckCoordinateVisibility((x, y), observer, blockedPositions)) return true;
                }
            }
            return false;
        }

        private List<FieldOfView> GetTeamFOVs(Team team)
        {
            var units = team.Players.SelectMany(p => p.Units);
            var fovs = new List<FieldOfView>();
            foreach (var unit in units)
            {
                var pos = _gameWorldService.GetEntityPosition(unit);
                if (!pos.HasValue) continue;
                var fovCoords = (pos.Value.x + 0.5f, pos.Value.y + 0.5f);
                var dirVec = _unitMoveService.GetDirectionVector(unit.Facing);
                fovs.Add(new FieldOfView
                {
                    Position = fovCoords,
                    FacingDirection = dirVec,
                    FovAngle = 120,
                    MaxViewDistance = 7.5f
                });
                fovs.Add(new FieldOfView
                {
                    Position = fovCoords,
                    FacingDirection = dirVec,
                    FovAngle = 360,
                    MaxViewDistance = 1.4f
                });
            }
            return fovs;
        }

        public List<(int x, int y)> ComputeVisiblePositionsForTeam(Team team)
        {
            var visiblePositions = new List<(int x, int y)>();
            var teamFOVs = GetTeamFOVs(team);
            var blockedPositions = _gameWorldService.GetPositionsByType<Obstacle>();
            var dimensons = _gameInstanceService.GameInstance.GameWorld.BoardDimensions;
            for (int x = 0; x < dimensons.w; x++)
            {
                for (int y = 0; y < dimensons.h; y++)
                {
                    bool visible = CheckMapPositionVisibility((x, y), teamFOVs, blockedPositions);
                    if (visible) visiblePositions.Add((x, y));
                }
            }
            return visiblePositions;
        }
    }
}
