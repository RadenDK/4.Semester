
//connection to signalR hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl(/goalhub)
    .configurationLogging(signalR.LogLevel.Information)
    .build()

var Start = async () => {
    try {
        await connection.start();
        console.log("SignlaR Connected");
    }
    catch (error){
        console.log(error);
        setTimeout(Start, 5000);
    }
}

Start();

connection.onclose(async () => {
    await Start();
})


connection.on("RecieveGoalUpdate", (teamRed, teamBlue, redScore, blueScore) => {

    //update team red players
    const teamRedContainer = document.querySelector("team-red");
    teamRedContainer.innerHTML = '<span class="team-red">Team Red</span>';
    teamRed.forEach(user => {
        const userElement = document.createElement("p");
        userElement.textContent = `${user.FirstName} ${user.LastName}`;
        teamRedContainer.appendChild(userElement);
    });

    //update team blue players
    const teamBlueContainer = document.querySelector("team-blue");
    teamBlueContainer.innerHTML = '<span class="team-blue">Team Blue</span>';
    teamBlue.forEach(user => {
        const userElement = document.createElement("p");
        userElement.textContent = `${user.FirstName} ${user.LastName}`;
        teamBlueContainer.appendChild(userElement);
    });

    //update scores
    document.querySelector(".match-score .score:nth-child(1)").textContent = redScore;
    document.querySelector(".match-score .score:nth-child(3)").textContent = blueScore;
})
