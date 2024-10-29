﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace FoosballProLeague.Api.Models.FoosballModels
{
    public class MatchModel
    {
        public int Id { get; set; }

        public int TableId { get; set; }

        public int RedTeamId { get; set; }

        public int BlueTeamId { get; set; }

        public int TeamRedScore { get; set; }

        public int TeamBlueScore { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}
