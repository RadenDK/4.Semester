
//connection to signalR hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/matchhub")
    .configureLogging(signalR.LogLevel.Information)
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

let matchTimer;
let matchStartTime;

connection.On("RecieveMatchStart", (isMatchStart, teamRed, teamBlue, redScore, blueScore) => {
    //start time on match
    if (isMatchStart) {
        matchStartTime = new Date();
        if (matchTimer) {
            clearInterval(matchTimer);
        }
        matchTimer = setInterval(updateMatchTime, 1000);
    }
    else {
        if (matchTimer) {
            clearInterval(matchTimer);
        }
    }

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

function updateMatchTime() {
    const now = new Date();
    const elapsedTime = Math.floor((now - matchStartTime) / 1000);
    const minutes = Math.floor(elapsedTime / 60);
    const seconds = elapsedTime % 60;
    document.querySelector(".match-time").textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
}


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

connection.on("RecieveEndMatch", (isMatchStart) => {
    //end match
    if (!isMatchStart) {
        if (matchTimer) {
            clearInterval(matchTimer);
        }
        document.querySelector(".match-time").textContent = "";

        // resets the scores and player information
        document.querySelector(".team-red").innerHTML = '<span class="team-red">Team Red</span>';
        document.querySelector(".team-blue").innerHTML = '<span class="team-blue">Team Blue</span>';
        document.querySelector(".match-score .score:nth-child(1)").textContent = "0";
        document.querySelector(".match-score .score:nth-child(3)").textContent = "0";
    }
})

