class FoosballProLeague {
    constructor(apiUrl, initialLeaderboardData) {
        this.apiUrl = apiUrl;
        this.leaderboardData = initialLeaderboardData;
        this.homepageConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${this.apiUrl}homepageHub`, {
                skipNegotiation: false,
                transport: signalR.HttpTransportType.WebSockets,
                withCredentials: true, // Include credentials
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.matchTimer = null;
        this.matchStartTime = new Date(sessionStorage.getItem('matchStartTime')) || null;
        this.currentPageNumber = 1;
        this.pageSize = 10;
        this.currentMatch = JSON.parse(sessionStorage.getItem('currentMatch')) || null;
        this.currentMode = sessionStorage.getItem('selectedLeaderboard') || '1v1'; // default mode

        this.initializeConnections();
        this.updateMatchInfoFromStorage();
        this.updateMatchTimeFromStorage();
        this.initializeEventListeners();
    }

    initializeEventListeners() {
        document.querySelector('.elo-button[data-mode="2v2"]').addEventListener('click', (event) => {
            event.preventDefault();
            this.currentMode = '2v2';
            sessionStorage.setItem('selectedLeaderboard', '2v2');
            this.currentPageNumber = 1; // Reset to first page
            const url = `/HomePage/2v2?pageNumber=${this.currentPageNumber}`;
            window.history.pushState({ path: url }, '', url); // Update the URL
            this.updateActiveButton();
            this.fetchLeaderboard(this.currentMode, this.currentPageNumber);
        });

        document.querySelector('.elo-button[data-mode="1v1"]').addEventListener('click', (event) => {
            event.preventDefault();
            this.currentMode = '1v1';
            sessionStorage.setItem('selectedLeaderboard', '1v1');
            this.currentPageNumber = 1; // Reset to first page
            const url = `/HomePage/1v1?pageNumber=${this.currentPageNumber}`;
            window.history.pushState({ path: url }, '', url); // Update the URL
            this.updateActiveButton();
            this.fetchLeaderboard(this.currentMode, this.currentPageNumber);
        });
        
        document.addEventListener("DOMContentLoaded", () => {
            const matchTimeElement = document.querySelector(".match-time");
            if (matchTimeElement){
                const startTime = new Date(matchTimeElement.getAttribute("data-start-time"));
                this.updateOngoingTime(matchTimeElement, startTime);
                setInterval(() => this.updateOngoingTime(matchTimeElement, startTime), 1000);
            }
        });
    }
    
    updateOngoingTime(element, startTime){
        const now = new Date();
        const diff = now - startTime;
        const hours = Math.floor(diff / (1000 * 60 * 60));
        const minutes = Math.floor((diff % (1000 * 60)) / 1000);
        const seconds = Math.floor((diff % (1000 * 60)) / 1000);
        element.textContent = '${hours}h ${minutes}m ${seconds}s';
    }

    updateActiveButton() {
        document.querySelectorAll('.elo-button').forEach(button => {
            button.classList.remove('active');
        });
        document.querySelector(`.elo-button[data-mode="${this.currentMode}"]`).classList.add('active');
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

        try {
            const response = await fetch(`/web/user?mode=${mode}&pageNumber=${pageNumber}&pageSize=${this.pageSize}`);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            const paginatedData = data.users;
            const totalItems = data.totalUserCount;

            this.updateLeaderboard(paginatedData, pageNumber);
            this.updatePaginationControls(totalItems, this.pageSize, pageNumber);
        } catch (error) {
            console.error('Error fetching leaderboard:', error);
        }
    }

    updateLeaderboard(paginatedData, pageNumber = 1) {
        this.currentPageNumber = pageNumber;
        const leaderboardBody = document.querySelector('#leaderboardBody');
        leaderboardBody.innerHTML = '';

        paginatedData.forEach((player, index) => {
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
        const url = `/HomePage/${this.currentMode}?pageNumber=${this.currentPageNumber}`;
        window.history.pushState({ path: url }, '', url); // Update the URL
        this.fetchLeaderboard(this.currentMode, this.currentPageNumber).wait();
    }

    handleNextPageClick(event) {
        event.preventDefault();
        this.currentPageNumber += 1; // Update currentPageNumber
        const url = `/HomePage/${this.currentMode}?pageNumber=${this.currentPageNumber}`;
        window.history.pushState({ path: url }, '', url); // Update the URL
        this.fetchLeaderboard(this.currentMode, this.currentPageNumber).wait();
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
        const teamName = selector.includes('red') ? 'Red Team' : 'Blue Team';
        teamContainer.innerHTML = `<span class="${selector.slice(1)}">${teamName}</span>`;

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
        const initialMode = sessionStorage.getItem('selectedLeaderboard') || '1v1'; // default mode
        foosballProLeague.fetchLeaderboard(initialMode); // fetch initial leaderboard with correct mode
    })
    .catch(error => console.error('Error fetching configuration:', error));