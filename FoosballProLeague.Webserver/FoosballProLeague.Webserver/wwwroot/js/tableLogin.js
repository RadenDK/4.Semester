
class FoosballProLeague {
    // Initializes the FoosballProLeague class with API URL.
    // Sets up SignalR connection.
    constructor(apiUrl) {
        this.apiUrl = apiUrl;
        this.tableLoginConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${this.apiUrl}tableLoginHub`, {
                skipNegotiation: false,
                transport: signalR.HttpTransportType.WebSockets,
                withCredentials: true
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();

        console.log("FoosballProLeague initialized with API URL:", this.apiUrl);
        this.initializeConnections();
    }


    // Sets up SignalR connection handlers for receiving real-time updates.
    async initializeConnections() {
        this.tableLoginConnection.on("ReceiveTableLogin", (user) => {
            console.log("Received table login for user:", user);
            this.updateTableLogin(user, false);
        });

        this.tableLoginConnection.on("ReceiveRemoveUser", (email) => {
            console.log("Received table login for user:", email);
            this.updateTableLogin({ email: email }, true);
        });


        this.tableLoginConnection.onclose(async () => {
            console.log("SignalR connection closed. Attempting to reconnect...");
            await this.startConnection(this.tableLoginConnection);
        });

        await this.startConnection(this.tableLoginConnection);
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


    updateTableLogin(user, isRemove) {
        const userList = document.querySelector(".display-users .user-list");

        // Check if the user already exists in the list based on their email
        const existingUser = Array.from(userList.children).find(
            (child) => child.dataset.email === user.email
        );

        if (isRemove) {
            if (existingUser) {
                userList.removeChild(existingUser);
            }
        } else {
            if (existingUser) {
                // Update the user's side and team color if they already exist
                existingUser.textContent = `${user.firstName} ${user.lastName}`;
                existingUser.className = `user ${user.side} ${user.side === "red" ? "red-team" : "blue-team"}`;
            } else {
                // If the user does not exist, add them to the list
                const newParagraph = document.createElement("p");
                newParagraph.textContent = `${user.firstName} ${user.lastName}`;
                newParagraph.classList.add("user", user.side); // Add the side as a class
                newParagraph.classList.add(user.side === "red" ? "red-team" : "blue-team"); // Add the team color class
                newParagraph.dataset.email = user.email; // Store the email in a data attribute
                userList.appendChild(newParagraph);
            }
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
    })
    .catch(error => console.error('Error fetching configuration:', error));

document.addEventListener("DOMContentLoaded", function () {
    const userElements = document.querySelectorAll(".user");
    const hiddenEmailInput = document.getElementById("selectedEmail");
    const emailInput = document.getElementById("email");

    // Clear the email input field
    if (emailInput) {
        emailInput.value = '';
    }

    userElements.forEach(user => {
        user.addEventListener("click", function () {
            userElements.forEach(u => u.classList.remove("selected"));
            this.classList.add("selected");
            hiddenEmailInput.value = this.dataset.email;
        });
    });
});