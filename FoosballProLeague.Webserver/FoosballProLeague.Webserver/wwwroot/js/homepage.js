class FoosballProLeague {
    constructor(apiUrl) {
        this.apiUrl = apiUrl;
        this.homepageConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${this.apiUrl}homepagehub`, {
                transport: signalR.HttpTransportType.WebSockets,
                withCredentials: true
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.matchTimer = null;
        this.matchStartTime = new Date(sessionStorage.getItem('matchStartTime')) || null;
        this.currentPageNumber = 1;
        this.pageSize = 10;
        this.currentMatch = JSON.parse(sessionStorage.getItem('currentMatch')) || null;
        this.currentMode = '1v1'; // default mode

        this.initializeConnections();
        this.updateMatchInfoFromStorage();
        this.updateMatchTimeFromStorage();
    }

    async initializeConnections() {
        this.homepageConnection.on("ReceiveLeaderboardUpdate", (leaderboard) => {
            this.leaderboardData = leaderboard;
            this.updateLeaderboard(this.currentPageNumber);
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
            console.log("SignalR connected.");
        } catch (err) {
            console.error("Error establishing SignalR connection: ", err);
            setTimeout(() => this.startConnection(connection), 5000);
        }
    }

    async fetchLeaderboard(mode, pageNumber) {
        if (!pageNumber) {
            pageNumber = this.currentPageNumber; // Use currentPageNumber if pageNumber is not provided
        }
        const url = `/api/User?mode=${mode}&pageNumber=${pageNumber}&pageSize=${this.pageSize}`;

        try {
            const response = await fetch(url);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json(); 
            this.leaderboardData = data.users;
            this.updateLeaderboard(pageNumber);
            this.updatePaginationControls(data.totalUserCount, this.pageSize, pageNumber); 
        } catch (error) {
            console.error('Error fetching leaderboard:', error);
        }
    }


    updateLeaderboard(pageNumber = 1) {
        this.currentPageNumber = pageNumber;
        const leaderboardBody = document.querySelector('#leaderboardBody');
        leaderboardBody.innerHTML = '';

        const paginatedLeaderboard = this.leaderboardData;

        paginatedLeaderboard.forEach((player, index) => {
            const rank = (pageNumber - 1) * this.pageSize + index + 1;
            const elo = this.currentMode === '1v1' ? player.elo1v1 : player.elo2v2;
            const row = `
        <tr>
            <td>${rank}</td>
            <td>${player.firstName} ${player.lastName}</td>
            <td>${elo}</td>
        </tr>
        `;
            leaderboardBody.innerHTML += row;
        });

        this.updatePaginationControls(this.leaderboardData.length, this.pageSize, pageNumber);
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

        // Re-attach event listeners
        const previousPageButton = document.querySelector('.previous-page');
        const nextPageButton = document.querySelector('.next-page');

        if (previousPageButton) {
            previousPageButton.addEventListener('click', this.handlePreviousPageClick.bind(this));
        }

        if (nextPageButton) {
            nextPageButton.addEventListener('click', this.handleNextPageClick.bind(this));
        }
    }

    handlePreviousPageClick(event) {
        event.preventDefault();
        this.currentPageNumber -= 1; // Update currentPageNumber
        this.fetchLeaderboard(this.currentMode, this.currentPageNumber);
    }

    handleNextPageClick(event) {
        event.preventDefault();
        this.currentPageNumber += 1; // Update currentPageNumber
        this.fetchLeaderboard(this.currentMode, this.currentPageNumber);
    }

    handleMatchStart(isMatchStart, teamRed, teamBlue, redScore, blueScore) {
        console.log("ReceiveMatchStart called with data:", { isMatchStart, teamRed, teamBlue, redScore, blueScore });

        if (isMatchStart) {
            this.matchStartTime = new Date();
            sessionStorage.setItem('matchStartTime', this.matchStartTime.toISOString());
            if (this.matchTimer) {
                clearInterval(this.matchTimer);
            }
            this.matchTimer = setInterval(() => this.updateMatchTime(), 1000);
        } else {
            if (this.matchTimer) {
                clearInterval(this.matchTimer);
            }
        }

        this.currentMatch = { teamRed, teamBlue, redScore, blueScore };
        sessionStorage.setItem('currentMatch', JSON.stringify(this.currentMatch));
        this.updateMatchInfo(teamRed, teamBlue, redScore, blueScore);
    }

    updateMatchInfo(teamRed, teamBlue, redScore, blueScore) {
        this.updateTeamInfo(".team-red", teamRed);
        this.updateTeamInfo(".team-blue", teamBlue);

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

        // Check if either team has scored 10 goals
        if (redScore >= 10 || blueScore >= 10) {
            this.handleMatchEnd(false);
        }
    }

    updateTeamInfo(selector, team) {
        const teamContainer = document.querySelector(selector);
        teamContainer.innerHTML = `<span class="${selector.slice(1)}">${selector.slice(1).replace('-', ' ')}</span>`;

        if (team && team.user1) {
            const userElement = document.createElement("p");
            userElement.textContent = `${team.user1.firstName} ${team.user1.lastName}`;
            teamContainer.appendChild(userElement);
        }
        if (team && team.user2) {
            const userElement = document.createElement("p");
            userElement.textContent = `${team.user2.firstName} ${team.user2.lastName}`;
            teamContainer.appendChild(userElement);
        }
    }

    updateMatchTime() {
        if (!this.matchStartTime) {
            document.querySelector(".match-time").textContent = "";
            return;
        }

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
                this.matchTimer = null;
            }
            document.querySelector(".match-time").textContent = "";

            this.updateTeamInfo(".team-red", null);
            this.updateTeamInfo(".team-blue", null);
            document.querySelector(".match-score .score:nth-child(1)").textContent = "0";
            document.querySelector(".match-score .score:nth-child(3)").textContent = "0";

            this.currentMatch = null;
            sessionStorage.removeItem('currentMatch');
            sessionStorage.removeItem('matchStartTime');
        }
    }

    updateMatchInfoFromStorage() {
        if (this.currentMatch) {
            this.updateMatchInfo(
                this.currentMatch.teamRed,
                this.currentMatch.teamBlue,
                this.currentMatch.redScore,
                this.currentMatch.blueScore
            );
        }
    }

    updateMatchTimeFromStorage() {
        if (this.currentMatch && this.matchStartTime && !isNaN(this.matchStartTime.getTime())) {
            this.matchTimer = setInterval(() => this.updateMatchTime(), 1000);
        } else {
            document.querySelector(".match-time").textContent = "";
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
        const apiUrl = config.apiUrl;
        const foosballProLeague = new FoosballProLeague(apiUrl);
        foosballProLeague.fetchLeaderboard('1v1'); // fetch initial leaderboard
    })
    .catch(error => console.error('Error fetching configuration:', error));
