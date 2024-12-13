
class FoosballProLeague {
    // Initializes the FoosballProLeague class with API URL, leaderboard data, and default state.
    // Sets up SignalR connection, match tracking, and event listeners.
    constructor(apiUrl, initialLeaderboardData) {
        this.apiUrl = apiUrl;
        this.leaderboardData = initialLeaderboardData;
        this.homepageConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${this.apiUrl}homepageHub`, {
                skipNegotiation: false,
                transport: signalR.HttpTransportType.WebSockets,
                withCredentials: true
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.matchTimer = null;
        this.matchStartTime = new Date(sessionStorage.getItem('matchStartTime')) || null;
        this.currentPageNumber = 1;
        this.pageSize = 10;
        this.currentMatch = null;
        this.currentMode = sessionStorage.getItem('selectedLeaderboard') || '1v1';

        // Only initialize connections, don't fetch leaderboard immediately since we have server-side data
        this.initializeConnections();
        this.updateMatchTimeFromStorage();
        this.initializeEventListeners();
        this.updateActiveButton(); // Update the active button state to match current mode
    }


    // Attaches event listeners for leaderboard mode selection (1v1 or 2v2).
    initializeEventListeners() {
        // Attach event listeners for mode selection buttons
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
    }

    // Initializes the match timer based on stored start time and updates the UI every second.
    initializeMatchTimer() {
        const matchTimeElement = document.querySelector(".match-time");

        if (matchTimeElement) {
            const startTimeString = matchTimeElement.getAttribute("data-start-time");
            const startTime = startTimeString ? new Date(startTimeString) : null;

            if (startTime === null) {
                return;
            }

            this.matchStartTime = startTime;

            if (isNaN(this.matchStartTime.getTime())) {
                console.error("Invalid start time:", startTimeString);
                return;
            }

            this.updateOngoingTime(matchTimeElement, this.matchStartTime);
            this.matchTimer = setInterval(() => this.updateOngoingTime(matchTimeElement, this.matchStartTime), 1000);
        } else {
            if (this.matchTimer) {
                clearInterval(this.matchTimer);
            }
        }
    }

    // Updates the ongoing match time displayed in the UI based on the start time.
    updateOngoingTime(matchTimeElement, startTime) {
        const now = new Date();
        const elapsedTime = Math.floor((now - startTime) / 1000);
        const minutes = Math.floor(elapsedTime / 60);
        const seconds = elapsedTime % 60;
        matchTimeElement.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    }

    // Updates the visual state of the leaderboard mode buttons.
    updateActiveButton() {
        document.querySelectorAll('.elo-button').forEach(button => {
            button.classList.remove('active');
        });
        document.querySelector(`.elo-button[data-mode="${this.currentMode}"]`).classList.add('active');
    }

    // Sets up SignalR connection handlers for receiving real-time updates.
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


    // Fetches leaderboard data from the server for the specified mode and page number.
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

    // Updates the leaderboard table in the UI with new data.
    updateLeaderboard(paginatedData, pageNumber = 1) {
        this.currentPageNumber = pageNumber;
        const leaderboardBody = document.querySelector('#leaderboardBody');
        leaderboardBody.innerHTML = '';

        paginatedData.forEach((player, index) => {
            const rank = (pageNumber - 1) * this.pageSize + index + 1;
            const elo = this.currentMode === '1v1' ? player.elo1v1 : player.elo2v2;
            const fullName = `${player.firstName} ${player.lastName}`;
            const row = `
              <tr>
                 <td>${rank}</td>
                 <td class="truncate-name">${fullName}</td>
                 <td>${elo}</td>
              </tr>
            `;
            leaderboardBody.innerHTML += row;
        });
    }

    // Updates the pagination controls based on the total items and current page.
    updatePaginationControls(totalItems, pageSize, pageNumber) {
        const paginationContainer = document.querySelector('.pagination');
        paginationContainer.innerHTML = '';

        const totalPages = Math.ceil(totalItems / pageSize);

        // Always add Previous button
        const prevButton = `<a href="#" class="previous-page${pageNumber <= 1 ? ' disabled' : ''}">Previous</a>`;
        paginationContainer.innerHTML += prevButton;

        paginationContainer.innerHTML += `<span>Page ${pageNumber} of ${totalPages}</span>`;

        // Always add Next button
        const nextButton = `<a href="#" class="next-page${pageNumber >= totalPages ? ' disabled' : ''}">Next</a>`;
        paginationContainer.innerHTML += nextButton;

        // Re-attach event listeners only to enabled buttons
        const previousPageButton = document.querySelector('.previous-page:not(.disabled)');
        const nextPageButton = document.querySelector('.next-page:not(.disabled)');

        if (previousPageButton) {
            previousPageButton.addEventListener('click', this.handlePreviousPageClick.bind(this));
        }

        if (nextPageButton) {
            nextPageButton.addEventListener('click', this.handleNextPageClick.bind(this));
        }
    }

    // Handles the click event for the "Previous" page button and fetches the previous page of the leaderboard.
    handlePreviousPageClick(event) {
        event.preventDefault();
        if (this.currentPageNumber <= 1) return;
        
        const prevButton = event.target;
        if (prevButton.classList.contains('disabled')) return;
        
        prevButton.classList.add('disabled');
        this.currentPageNumber -= 1;
        const url = `/HomePage/${this.currentMode}?pageNumber=${this.currentPageNumber}`;
        window.history.pushState({ path: url }, '', url);
        this.fetchLeaderboard(this.currentMode, this.currentPageNumber);
    }

    // Handles the click event for the "Next" page button and fetches the next page of the leaderboard.
    handleNextPageClick(event) {
        event.preventDefault();
        
        const nextButton = event.target;
        if (nextButton.classList.contains('disabled')) return;
        
        nextButton.classList.add('disabled');
        this.currentPageNumber += 1;
        const url = `/HomePage/${this.currentMode}?pageNumber=${this.currentPageNumber}`;
        window.history.pushState({ path: url }, '', url);
        this.fetchLeaderboard(this.currentMode, this.currentPageNumber);
    }

    // Handles the start of a new match, displaying the teams and score.
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
        this.updateMatchInfo(teamRed, teamBlue, redScore, blueScore);
    }

    // Updates the match score in real-time.
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

    // Updates the team information in the UI
    updateTeamInfo(selector, team) {
        const teamContainer = document.querySelector(selector);
        let user1 = teamContainer.querySelector(".user1");
        let user2 = teamContainer.querySelector(".user2");

        if (team && team.user1 && user1) {
            console.log(`${selector} User1:`, team.user1);
            user1.textContent = `${team.user1.firstName} ${team.user1.lastName}`;
        }
        if (team && team.user2 && user2) {
            console.log(`${selector} User2:`, team.user2);
            user2.textContent = `${team.user2.firstName} ${team.user2.lastName}`;
        }
    }

    // Updates the match time on the UI, showing elapsed time since the match started
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

    // Updates the match timer from storage if the match has started
    updateMatchTimeFromStorage() {
        if (this.currentMatch && this.matchStartTime && !isNaN(this.matchStartTime.getTime())) {
            this.matchTimer = setInterval(() => this.updateMatchTime(), 1000);
        }
        else {
            this.initializeMatchTimer();
        }
    }

    // Handles the end of a match, resetting match data and displaying the final score.
    handleMatchEnd(isMatchStart) {
        const matchTimeElement = document.querySelector(".match-time");
        if (matchTimeElement) {
            matchTimeElement.removeAttribute("data-start-time");
            matchTimeElement.textContent = "";
            if (!isMatchStart) {
                if (this.matchTimer) {
                    clearInterval(this.matchTimer);
                    this.matchTimer = null;
                }
            }

            document.querySelector(".match-time").textContent = "";

            document.querySelector(".team-red .user1").textContent = "";
            document.querySelector(".team-red .user2").textContent = "";
            document.querySelector(".team-blue .user1").textContent = "";
            document.querySelector(".team-blue .user2").textContent = "";

            document.querySelector(".match-score .score:nth-child(1)").textContent = "0";
            document.querySelector(".match-score .score:nth-child(3)").textContent = "0";

            this.currentMatch = null;
            sessionStorage.removeItem('currentMatch');
            sessionStorage.removeItem('matchStartTime');

            this.fetchLeaderboard(this.currentMode, this.currentPageNumber);
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
        const initialMode = sessionStorage.getItem('selectedLeaderboard') || '1v1';
        
        // Clear any page number from URL on initial load to prevent flash of wrong page
        const currentUrl = new URL(window.location.href);
        currentUrl.searchParams.delete('pageNumber');
        window.history.replaceState({}, '', currentUrl);
        
        foosballProLeague.fetchLeaderboard(initialMode, 1); // Always start at page 1 on fresh load
    })
    .catch(error => console.error('Error fetching configuration:', error));