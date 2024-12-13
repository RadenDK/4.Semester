
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
            this.updateTableLogin(user);
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


    updateTableLogin(user) {
        const displayUsers = document.querySelector(".display-users");

        // Check if there are already two players
        if (displayUsers.children.length >= 2) {
            console.log("Maximum number of players reached.");
            return;
        }

        // Create a form for each user
        const userForm = document.createElement("form");
        userForm.method = "post";
        userForm.action = "/TableLogin/RemoveUser";

        // Create a hidden input to store the user's information
        const userInput = document.createElement("input");
        userInput.type = "hidden";
        userInput.name = "user";
        userInput.value = `${user.firstName} ${user.lastName}`;

        // Create a button for the user
        const userButton = document.createElement("button");
        userButton.type = "submit";
        userButton.classList.add("user-button");
        userButton.textContent = `${user.firstName} ${user.lastName}`;

        // Append the hidden input and button to the form
        userForm.appendChild(userInput);
        userForm.appendChild(userButton);

        // Add an event listener to the form to prevent default submission
        userForm.addEventListener("submit", (event) => {
            event.preventDefault();
            // Handle user removal logic here
            displayUsers.removeChild(userForm);
            console.log("User removed:", user);
        });

        // Append the form to the displayUsers container
        displayUsers.appendChild(userForm);
        console.log("User form added to displayUsers container for user:", user);

        // Log the current state of displayUsers
        console.log("Current displayUsers content:", displayUsers.innerHTML);
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