class FoosballProLeague {
    constructor(apiUrl) {
        this.apiUrl = apiUrl;
        this.homepageConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${this.apiUrl}homepageHub`, {
                transport: signalR.HttpTransportType.WebSockets,
                withCredentials: true
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.matchTimer = null;
        this.matchStartTime = null;
        this.currentPageNumber = 1; // Store the current page number
        this.leaderboardData = []; // Store the leaderboard data

        this.initializeConnections();
    }

    async initializeConnections() {
        this.homepageConnection.on("ReceiveLeaderboardUpdate", (leaderboard) => {
            this.leaderboardData = leaderboard; // Store the leaderboard data
            this.updateLeaderboard(this.currentPageNumber); // Use the stored page number
        });

        this.homepageConnection.on("ReceiveMatchStart", (isMatchStart, teamRed, teamBlue, redScore, blueScore) => {
            this.handleMatchStart(isMatchStart, teamRed, teamBlue, redScore, blueScore);
        });

        this.homepageConnection.on("ReceiveGoalUpdate", (teamRed, teamBlue, redScore, blueScore) => {
            this.updateMatchInfo(teamRed, teamBlue, redScore, blueScore);
        });

        this.homepageConnection.on("ReceiveMatchEnd", (isMatchStart) => {
            this.handleMatchEnd(isMatchStart);
        });

        this.homepageConnection.onclose(() => this.startConnection(this.homepageConnection));

        await this.startConnection(this.homepageConnection);
    }

    async startConnection(connection) {
        try {
            await connection.start();
            console.log("SignalR Connected.");
        } catch (err) {
            console.error("Error establishing SignalR connection: ", err);
            setTimeout(() => this.startConnection(connection), 5000);
        }
    }

    updateLeaderboard(pageNumber = 1) {
        this.currentPageNumber = pageNumber; // Update the stored page number
        const leaderboardBody = document.querySelector('#leaderboardBody');
        leaderboardBody.innerHTML = '';

        const pageSize = 10; // Number of items per page

        // Sort the leaderboard data by elo1v1 in descending order 
        this.leaderboardData.sort((a, b) => b.elo1v1 - a.elo1v1);

        // Calculate start and end indices for the current page
        const startIndex = (pageNumber - 1) * pageSize;
        const endIndex = startIndex + pageSize;
        const paginatedLeaderboard = this.leaderboardData.slice(startIndex, endIndex);

        paginatedLeaderboard.forEach((player, index) => {
            const rank = startIndex + index + 1; // Calculate global cumulative "rank"
            const row = `
            <tr>
                <td>${rank}</td>
                <td>${player.firstName} ${player.lastName}</td>
                <td>${player.elo1v1}</td>
            </tr>
        `;
            leaderboardBody.innerHTML += row;
        });

        // Update pagination controls
        this.updatePaginationControls(this.leaderboardData.length, pageSize, pageNumber);
    }

    updatePaginationControls(totalItems, pageSize, pageNumber) {
        const paginationContainer = document.querySelector('.pagination');
        paginationContainer.innerHTML = '';

        const totalPages = Math.ceil(totalItems / pageSize);

        if (pageNumber > 1) {
            paginationContainer.innerHTML += `<a href="#" class="previous-page">Previous</a>`;
        }

        paginationContainer.innerHTML += `<span>Page ${pageNumber} of ${totalPages}</span>`;

        if (pageNumber < totalPages) {
            paginationContainer.innerHTML += `<a href="#" class="next-page">Next</a>`;
        }

        // Add event listeners for pagination controls
        document.querySelector('.previous-page')?.addEventListener('click', (event) => {
            event.preventDefault();
            this.updateLeaderboard(pageNumber - 1);
        });

        document.querySelector('.next-page')?.addEventListener('click', (event) => {
            event.preventDefault();
            this.updateLeaderboard(pageNumber + 1);
        });
    }

    handleMatchStart(isMatchStart, teamRed, teamBlue, redScore, blueScore) {
        console.log("ReceiveMatchStart called with data:", { isMatchStart, teamRed, teamBlue, redScore, blueScore });

        if (isMatchStart) {
            this.matchStartTime = new Date();
            if (this.matchTimer) {
                clearInterval(this.matchTimer);
            }
            this.matchTimer = setInterval(() => this.updateMatchTime(), 1000);
        } else {
            if (this.matchTimer) {
                clearInterval(this.matchTimer);
            }
        }

        this.updateMatchInfo(teamRed, teamBlue, redScore, blueScore);
    }

    updateMatchInfo(teamRed, teamBlue, redScore, blueScore) {
        const teamRedContainer = document.querySelector(".team-red");
        teamRedContainer.innerHTML = '<span class="team-red">Team Red</span>';

        if (teamRed && teamRed.user1) {
            const userElement = document.createElement("p");
            userElement.textContent = `${teamRed.user1.firstName} ${teamRed.user1.lastName}`;
            teamRedContainer.appendChild(userElement);
        }
        if (teamRed && teamRed.user2) {
            const userElement = document.createElement("p");
            userElement.textContent = `${teamRed.user2.firstName} ${teamRed.user2.lastName}`;
            teamRedContainer.appendChild(userElement);
        }

        const teamBlueContainer = document.querySelector(".team-blue");
        teamBlueContainer.innerHTML = '<span class="team-blue">Team Blue</span>';

        if (teamBlue && teamBlue.user1) {
            const userElement = document.createElement("p");
            userElement.textContent = `${teamBlue.user1.firstName} ${teamBlue.user1.lastName}`;
            teamBlueContainer.appendChild(userElement);
        }
        if (teamBlue && teamBlue.user2) {
            const userElement = document.createElement("p");
            userElement.textContent = `${teamBlue.user2.firstName} ${teamBlue.user2.lastName}`;
            teamBlueContainer.appendChild(userElement);
        }

        const redScoreElement = document.querySelector(".match-score .score:nth-child(1)");
        const blueScoreElement = document.querySelector(".match-score .score:nth-child(3)");
        if (redScoreElement) {
            redScoreElement.textContent = redScore;
        } else {
            console.error('Element for red score not found.');
        }
        if (blueScoreElement) {
            blueScoreElement.textContent = blueScore;
        } else {
            console.error('Element for blue score not found.');
        }
    }

    updateMatchTime() {
        const now = new Date();
        const elapsedTime = Math.floor((now - this.matchStartTime) / 1000);
        const minutes = Math.floor(elapsedTime / 60);
        const seconds = elapsedTime % 60;
        document.querySelector(".match-time").textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    }

    handleMatchEnd(isMatchStart) {
        if (!isMatchStart) {
            if (this.matchTimer) {
                clearInterval(this.matchTimer);
            }
            document.querySelector(".match-time").textContent = "";

            document.querySelector(".team-red").innerHTML = '<span class="team-red">Team Red</span>';
            document.querySelector(".team-blue").innerHTML = '<span class="team-blue">Team Blue</span>';
            document.querySelector(".match-score .score:nth-child(1)").textContent = "0";
            document.querySelector(".match-score .score:nth-child(3)").textContent = "0";
        }
    }
}


// Fetch the API URL from the backend endpoint and initialize the connections
fetch('/config/url')
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(config => {
        console.log("Config: ", config);
        const apiUrl = config.apiUrl; // Use the API URL from the backend endpoint
        const foosballProLeague = new FoosballProLeague(apiUrl); // Create an instance of the class
    })
    .catch(error => console.error('Error fetching configuration:', error));

