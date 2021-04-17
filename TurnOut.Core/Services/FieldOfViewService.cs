using System;
using System.Collections.Generic;
using System.Linq;
using Turnout.Core.Utility;
using TurnOut.Core.Models;
using TurnOut.Core.Models.Entities;
using TurnOut.Core.Models.Entities.Units;
using TurnOut.Core.Models.EntityExtensions;

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

        private bool TestObserverAngleRangeBlocking(List<double> blockingRangeAngles, double testAngle)
        {
            // Calculate the range bounds
            (double lower, double upper) blockedAngleRange = (180, -180);
            // First simple approach is to assume the minimum angle to be the left bound
            // and the maximum angle as right bound
            blockedAngleRange.lower = blockingRangeAngles.Min();
            blockedAngleRange.upper = blockingRangeAngles.Max();
            // Check the resulting ranges size
            var blockedAngleRangeSize = blockedAngleRange.upper - blockedAngleRange.lower;
            bool behindObserver = blockedAngleRangeSize > 180;
            if (behindObserver)
            {
                // Here the obstacle is behind the observer plane
                // This is a special case, where the range actually needs to be "inverted"
                // To achieve this, the left bound is determined by the highest negative
                // and the right bound by the smallest positive angle value.
                blockedAngleRange.lower = blockingRangeAngles.Where(a => a < 0).Max();
                blockedAngleRange.upper = blockingRangeAngles.Where(a => a >= 0).Min();
            }
            // Check if the target angle falls between the ranges left and right angle limits
            bool inAngleRange = (testAngle > blockedAngleRange.lower) && (testAngle < blockedAngleRange.upper);
            // Use XOR of behindObserver and inAngleRange for the actual blocking test:
            // That's because in the case of the obstacle being located behind the observer plane, left and right
            // bounds need to be switched, therefore negating inAngleRange, which describes the target angle being
            // inside the range from a front-facing perspective.
            return (behindObserver ^ inAngleRange);
        }

        private bool CheckCoordinateVisibility((float x, float y) coords, GlobalFieldOfView observer, List<(int x, int y)> blockedPositions)
        {
            (float x, float y) viewVector = (coords.x - observer.Position.x, coords.y - observer.Position.y);
            
            // Check by distance
            double distance = GetEuclidDist(viewVector.x, viewVector.y);
            if (distance > observer.ViewDepth) return false;

            // Check by angle
            double absTargetAngle = GetNorthBasedAbsDeg(viewVector);
            double relTargetAngle = GetRelativeDeg(absTargetAngle, observer.DirectionAngle);
            if (Math.Abs(relTargetAngle) > (observer.ViewAngle * 0.5))
                return false;

            // Check by obstacle blocking
            foreach (var block in blockedPositions)
            {
                var blockCoords = (x: block.x + 0.5f, y: block.y + 0.5f);
                var blockVector = (x: blockCoords.x - observer.Position.x, y: blockCoords.y - observer.Position.y);
                var blockDistance = GetEuclidDist(blockVector.x, blockVector.y);
                if (blockDistance < distance)
                {
                    var relCornerAngles = new List<double>();
                    // could be in front of target, compute relative angles of all 4 obstacle corners
                    for (int i = 0; (i < 4); i++)
                    {
                        // move corners out by a small notch to prevent seeing through infinitesimal gaps between diagonal obstacles
                        float x = block.x + (i < 2 ? -0.01f : 1.01f);
                        float y = block.y + ((i % 2 == 0) ? -0.01f : 1.01f);
                        var cornerVector = (x: x - observer.Position.x, y: y - observer.Position.y);
                        var absCornerAngle = GetNorthBasedAbsDeg(cornerVector);
                        var relCornerAngle = GetRelativeDeg(absCornerAngle, observer.DirectionAngle);
                        relCornerAngles.Add(relCornerAngle);
                    }

                    bool isBlocked = TestObserverAngleRangeBlocking(relCornerAngles, relTargetAngle);
                    if (isBlocked) return false;
                }
            }

            return true;
        }

        private bool CheckMapPositionVisibility((int x, int y) mapPos, List<GlobalFieldOfView> observers, List<(int x, int y)> blockedPositions)
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

        private List<GlobalFieldOfView> GetGlobalFieldsOfView(UnitBase unit)
        {
            var globalFOVs = new List<GlobalFieldOfView>();
            if (!unit.Has<VisionExtension>(out var unitVision)) return globalFOVs;

            var pos = _gameWorldService.GetEntityPosition(unit);
            if (!pos.HasValue) return globalFOVs;
            var unitCenter = (x: pos.Value.x + 0.5f, y: pos.Value.y + 0.5f);
            var frontVector = _unitMoveService.GetDirectionVector(unit.Facing);
            var rightVector = _unitMoveService.GetDirectionVector(unit.Facing.RotateClockWise(1));
            var unitFacingAngle = GetNorthBasedAbsDeg(frontVector);

            foreach (var relativeFieldOfView in unitVision.RelativeFOVs)
            {
                var offset = (x: relativeFieldOfView.PostionOffset.front * frontVector.x
                                 + relativeFieldOfView.PostionOffset.right * rightVector.x,
                              y: relativeFieldOfView.PostionOffset.front * frontVector.y
                                 + relativeFieldOfView.PostionOffset.right * rightVector.y);
                globalFOVs.Add(new GlobalFieldOfView
                {
                    Position = (unitCenter.x + offset.x, unitCenter.y + offset.y),
                    DirectionAngle = unitFacingAngle + relativeFieldOfView.AngleOffset,
                    ViewAngle = relativeFieldOfView.ViewAngle,
                    ViewDepth = relativeFieldOfView.ViewDepth
                });
            }
            return globalFOVs;
        }

        private List<GlobalFieldOfView> GetTeamFOVs(Team team)
        {
            var units = team.Players.SelectMany(p => p.Units);
            var fovs = new List<GlobalFieldOfView>();
            foreach (var unit in units)
            {
                fovs.AddRange(GetGlobalFieldsOfView(unit));
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
