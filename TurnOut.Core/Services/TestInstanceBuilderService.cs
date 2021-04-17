using System.Collections.Generic;
using TurnOut.Core.Models;
using TurnOut.Core.Models.Entities;
using TurnOut.Core.Models.Entities.Units;

namespace TurnOut.Core.Services
{
    public class TestInstanceBuilderService
    {
        public GameInstance GetTestInstance()
        {
            Team teamAlpha = new Team
            {
                TeamName = "Team Alpha"
            };
            Team teamOmega = new Team
            {
                TeamName = "Team Omega"
            };
            teamAlpha.OpponentTeam = teamOmega;
            teamOmega.OpponentTeam = teamAlpha;

            Player alice = new Player
            {
                Team = teamAlpha,
                Name = "Spieler-A"
            };
            alice.Units = new List<UnitBase>
            {
                new RunnerUnit
                {
                    Player = alice,
                    Facing = UnitDirection.North
                },
                new DefenderUnit
                {
                    Player = alice,
                    Facing = UnitDirection.North
                },
                new DasherUnit
                {
                    Player = alice,
                    Facing = UnitDirection.West
                },
                new StrikerUnit
                {
                    Player = alice,
                    Facing = UnitDirection.West
                },
            };
            /*
            Player adam = new Player
            {
                Team = teamAlpha,
                Name = "Adam"
            };
            adam.Units = new List<UnitBase>
            {
                new DasherUnit
                {
                    Player = adam,
                    Facing = UnitDirection.West
                },
                new StrikerUnit
                {
                    Player = adam,
                    Facing = UnitDirection.West
                },
            };
            */
            teamAlpha.Players = new List<Player> { alice };



            Player bob = new Player
            {
                Team = teamOmega,
                Name = "Spieler-B"
            };
            bob.Units = new List<UnitBase>
            {
                new RunnerUnit
                {
                    Player = bob,
                    Facing = UnitDirection.South
                },
                new DefenderUnit
                {
                    Player = bob,
                    Facing = UnitDirection.South
                },
                new DasherUnit
                {
                    Player = bob,
                    Facing = UnitDirection.East
                },
                new StrikerUnit
                {
                    Player = bob,
                    Facing = UnitDirection.East
                },
            };
            /*
            Player berta = new Player
            {
                Team = teamOmega,
                Name = "Berta"
            };
            berta.Units = new List<UnitBase>
            {
                new DasherUnit
                {
                    Player = berta,
                    Facing = UnitDirection.East
                },
                new StrikerUnit
                {
                    Player = berta,
                    Facing = UnitDirection.East
                },
            };
            */
            teamOmega.Players = new List<Player> { bob };

            var gi = new GameInstance
            {
                GameWorld = new World(20, 15),
                TeamAlpha = teamAlpha,
                TeamOmega = teamOmega,
                IsInPlanningPhase = true,
                TurnPlanningCountdown = new CountdownState()
            };

            // Place units
            gi.GameWorld.Board[17, 14] = alice.Units[0];
            gi.GameWorld.Board[18, 14] = alice.Units[1];
            gi.GameWorld.Board[19, 13] = alice.Units[2];
            gi.GameWorld.Board[19, 12] = alice.Units[3];

            gi.GameWorld.Board[2, 0] = bob.Units[0];
            gi.GameWorld.Board[1, 0] = bob.Units[1];
            gi.GameWorld.Board[0, 1] = bob.Units[2];
            gi.GameWorld.Board[0, 2] = bob.Units[3];

            // Place obstacles
            List<(int x, int y)> obstacleMap = new List<(int x, int y)>
            {
                (4,1),(4,2),
                (1,4),(2,4),(3,4),(4,4),
                (0,8),(1,8),
                (3,8),(4,8),(5,8),(4,9),
                (4,12),(4,13),(4,14),
                (7,10),(7,11),(8,10),(8,11),(9,10),(9,11),(10,10),(9,9),(10,9),(9,8),(9,7),
                (9,3),(10,3),(11,3),(12,3),(13,3),(14,3),(11,2),(12,2),
                (17,1),(17,2),(17,3),
                (13,6),(14,6),(15,6),(16,6),(17,6),(18,6),(16,7),
                (15,10),(16,10),(17,10),(18,10),
                (15,12),(15,13)
            };
            foreach (var pos in obstacleMap)
            {
                gi.GameWorld.Board[pos.x, pos.y] = new Obstacle();
            }

            // Place flags
            gi.GameWorld.Board[17, 12] = gi.TeamAlpha.Flag;
            gi.GameWorld.Board[2, 2] = gi.TeamOmega.Flag;

            /*
            // Place units
            gi.GameWorld.Board[9, 1] = gi.TeamAlpha.Players[0].Units[0];
            gi.GameWorld.Board[7, 1] = gi.TeamAlpha.Players[0].Units[1];
            gi.GameWorld.Board[8, 2] = gi.TeamAlpha.Players[0].Units[2];

            gi.GameWorld.Board[10, 12] = gi.TeamOmega.Players[0].Units[0];
            gi.GameWorld.Board[12, 12] = gi.TeamOmega.Players[0].Units[1];
            gi.GameWorld.Board[11, 11] = gi.TeamOmega.Players[0].Units[2];

            // Place obstacles
            List<(int x, int y)> obstacleMap = new List<(int x, int y)>
            {
                (2,0), (2,1), (2,2), (3,2),
                (6,1), (6,2), (6,3),
                (15,1), (16,1),
                (13,4), (14,4), (13,5), (14,5),
                (8,4), (8,5), (8,6), (9,6), (7,6), (6,6), (5,6),
                (11,8), (12,8), (13,8), (14,8), (15,8),
                (1,9), (1,10), (2,9), (3,9), (4,9), (4,10), (4,11),
                (9,10), (9,11), (9,12), (9,13),
                (13,11), (13,12), (13,13)
            };
            foreach (var pos in obstacleMap)
            {
                gi.GameWorld.Board[pos.x, pos.y] = new Obstacle();
            }

            // Place flags
            gi.GameWorld.Board[8, 1] = gi.TeamAlpha.Flag;
            gi.GameWorld.Board[11, 12] = gi.TeamOmega.Flag;
            */

            return gi;
        }
    }
}
