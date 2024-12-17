document.addEventListener("DOMContentLoaded", async () => {
    const tableIdElem = document.getElementById("tableId");
    const sideElem = document.getElementById("side");

    if (!tableIdElem || !sideElem) {
        console.error("Missing #tableId or #side elements in the HTML.");
        return;
    }

    const tableId = tableIdElem.value;
    const side = sideElem.value;

    // Render initial team data
    if (typeof initialPendingUsers !== 'undefined' && initialPendingUsers) {
        updateTeamUsers(initialPendingUsers);
    }

    // Initialize SignalR
    let apiUrl = await getApiUrl();
    await initializeSignalRConnection(apiUrl);

    // Intercept form submission to prevent redirection
    const loginForm = document.getElementById("loginForm");

    if (loginForm) {
        loginForm.addEventListener("submit", async (event) => {
            event.preventDefault(); // Stop default browser submission

            const emailInput = loginForm.querySelector("input[name='Email']");
            const email = emailInput.value.trim();

            if (!email) {
                console.error("Email is required.");
                return;
            }

            try {
                // Send POST request to the webserver controller
                const response = await fetch(`/TableLogin`, {
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded" },
                    body: new URLSearchParams({
                        tableId: tableId,
                        side: side,
                        email: email
                    })
                });

                if (response.ok) {
                    console.log("User successfully joined the team.");
                    emailInput.value = ""; // Clear the input field
                } else {
                    console.error("Failed to log in user.");
                }
            } catch (error) {
                console.error("Error during form submission:", error);
            }
        });
    }

});

// Helper function to fetch API URL
async function getApiUrl() {
    const response = await fetch('/config/url');
    if (!response.ok) throw new Error(`HTTP error: ${response.status}`);
    const config = await response.json();
    return config.apiUrl;
}

// SignalR initialization (unchanged)
async function initializeSignalRConnection(apiUrl) {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`${apiUrl}tableLoginHub`)
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on("ReceivePendingTableLoginUsers", (data) => {
        updateTeamUsers(data);
    });

    try {
        await connection.start();
        console.log("SignalR connected.");
    } catch (err) {
        console.error("Error connecting to SignalR:", err);
    }
}

// Function to update the user lists (unchanged)
function updateTeamUsers(data) {
    const redTeamContainer = document.getElementById("red-team-users");
    const blueTeamContainer = document.getElementById("blue-team-users");

    redTeamContainer.innerHTML = "";
    blueTeamContainer.innerHTML = "";

    if (data.red) data.red.forEach(user => redTeamContainer.appendChild(createUserButton(user)));
    if (data.blue) data.blue.forEach(user => blueTeamContainer.appendChild(createUserButton(user)));
}

function createUserButton(user) {
    const button = document.createElement("button");
    button.className = "user-button";
    button.textContent = `${user.firstName || "Unknown"} ${user.fastName || "User"}`;
    button.onclick = () => removePlayer(user.id);
    return button;
}

async function removePlayer(userId) {
    const tableId = document.getElementById("tableId").value;
    const side = document.getElementById("side").value;

    try {
        const response = await fetch(`/TableLogin/RemoveUser?userId=${userId}&tableId=${tableId}`, {
            method: "POST",
        });

        if (response.ok) {
            console.log("Player removed successfully.");
        } else {
            console.error("Failed to remove player.");
        }
    } catch (error) {
        console.error("Error removing player:", error);
    }
}
