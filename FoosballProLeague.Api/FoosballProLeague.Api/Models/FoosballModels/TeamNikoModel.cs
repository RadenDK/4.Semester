namespace FoosballProLeague.Api.Models.FoosballModels
{
    public class TeamNikoModel
    {
        public int Id { get; set; }
        public List<UserModel> teamRed { get; set; }
        public List<UserModel> teamBlue { get; set; }
    }
}
