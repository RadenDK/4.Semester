namespace FoosballProLeague.Api.Models.FoosballModels
{
    public class TeamModel
    {
        public int Id { get; set; }
        public UserModel User1 { get; set; }
        public UserModel? User2 { get; set; }

        public int GetTeamEloAverage()
        {
            // if there is no User2, return User1's Elo1v1
            if (User2 == null)
            {
                return User1.Elo1v1;
            }
            // If the User2 is not null then there are two players on the match and we should return their 2vs2 Elo
            else
            {
                return (User1.Elo2v2 + User2.Elo2v2) / 2;
            }
        }
    }
}