// ConfigMode.ino

#include "ConfigMode.h"
#include "LedHandler.h"
#include "WifiConnector.h" // Include WifiConnector to use Wi-Fi functions

namespace ConfigMode {
    const int configButtonPin = 0; // Using GPIO0 (Boot button)
    WebServer* server = nullptr;
    Preferences preferences;
    bool configMode = false;

    String ssid = "";
    String wifiPassword = "";
    String tableId = "";
    String companyId = "";
    String departmentId = "";
    String side = ""; // Variable to store "Red" or "Blue" side
    String apiKey = "";
    String mqttUsername = "";
    String mqttPassword = "";

    void initializeConfigButton() {
        pinMode(configButtonPin, INPUT_PULLUP); // Initialize GPIO0 with internal pull-up
        Serial.println("Config button initialized on GPIO " + String(configButtonPin));
    }

    void loadSettings() {
        Serial.println("Attempting to load saved settings...");

        // Initialize preferences here
        if (!preferences.begin("config", false)) {
            Serial.println("Failed to initialize preferences.");
            return;
        }

        // Load the settings
        ssid = preferences.getString("ssid", "");
        wifiPassword = preferences.getString("wifiPassword", "");
        tableId = preferences.getString("tableId", "");
        companyId = preferences.getString("companyId", "");
        departmentId = preferences.getString("departmentId", "");
        side = preferences.getString("side", "");
        apiKey = preferences.getString("apiKey", "");
        mqttUsername = preferences.getString("mqttUsername", "");
        mqttPassword = preferences.getString("mqttPassword", "");


        // Print loaded settings for debugging
        Serial.println("Loading saved settings:");
        Serial.print("SSID: "); Serial.println(ssid);
        Serial.print("WiFi Password: "); Serial.println(wifiPassword);
        Serial.print("Table ID: "); Serial.println(tableId);
        Serial.print("Company ID: "); Serial.println(companyId);
        Serial.print("Department ID: "); Serial.println(departmentId);
        Serial.print("Side: "); Serial.println(side);
        Serial.print("apiKey: "); Serial.println(apiKey);
        Serial.print("mqttUsername: "); Serial.println(mqttUsername);
        Serial.print("mqttPassword: "); Serial.println(mqttPassword);

        preferences.end();
    }

    bool settingsInvalid() {
        bool invalid = ssid.isEmpty() || wifiPassword.isEmpty() || tableId.isEmpty() || companyId.isEmpty() || departmentId.isEmpty() || side.isEmpty() || apiKey.isEmpty() || mqttUsername.isEmpty() || mqttPassword.isEmpty(); 

        if (invalid) {
            Serial.println("Settings are invalid.");
        } else {
            Serial.println("Settings are valid.");
        }

        return invalid;
    }

    void handleConfigForm() {
        Serial.println("Serving configuration form...");

        String html = "<html><head><title>ESP32 Config</title>";
        html += "<style>";
        html += "body { font-family: Arial, sans-serif; background-color: #f2f2f2; }";
        html += "h1 { color: #333; }";
        html += "form { background-color: #fff; padding: 20px; border-radius: 5px; max-width: 400px; margin: auto; }";
        html += "input[type='text'], input[type='password'], select { width: 100%; padding: 12px; margin: 8px 0; border: 1px solid #ccc; border-radius: 4px; }";
        html += "input[type='submit'] { background-color: #4CAF50; color: white; padding: 14px 20px; margin: 8px 0; border: none; border-radius: 4px; cursor: pointer; width: 100%; }";
        html += "input[type='submit']:hover { background-color: #45a049; }";
        html += "label { display: block; margin-bottom: 8px; }";
        html += "</style>";
        html += "</head><body>";
        html += "<h1 style='text-align: center;'>ESP32 Configuration</h1>";
        html += "<form action='/save' method='POST'>";
        html += "<label for='ssid'>SSID:</label>";
        html += "<input type='text' id='ssid' name='ssid' value='" + ssid + "' required><br>";
        html += "<label for='wifiPassword'>WiFi Password:</label>";
        html += "<input type='password' id='wifiPassword' name='wifiPassword' value='" + wifiPassword + "' required><br>";
        html += "<label for='tableId'>Table ID:</label>";
        html += "<input type='text' id='tableId' name='tableId' value='" + tableId + "' required><br>";
        html += "<label for='companyId'>Company ID:</label>";
        html += "<input type='text' id='companyId' name='companyId' value='" + companyId + "' required><br>";
        html += "<label for='departmentId'>Department ID:</label>";
        html += "<input type='text' id='departmentId' name='departmentId' value='" + departmentId + "' required><br>";
        html += "<label for='side'>Side:</label>";
        html += "<select id='side' name='side'>";
        html += "<option value='red'" + String((side == "red") ? " selected" : "") + ">red</option>";
        html += "<option value='blue'" + String((side == "blue") ? " selected" : "") + ">blue</option>";
        html += "</select><br>";
        html += "<label for='apiKey'>API Key:</label>";
        html += "<input type='text' id='apiKey' name='apiKey' value='" + apiKey + "' required><br>";
        html += "<label for='mqttUsername'>MQTT Username:</label>";
        html += "<input type='text' id='mqttUsername' name='mqttUsername' value='" + mqttUsername + "' required><br>";
        html += "<label for='mqttPassword'>MQTT Password:</label>";
        html += "<input type='password' id='mqttPassword' name='mqttPassword' value='" + mqttPassword + "' required><br>";
        html += "<input type='submit' value='Save'>";
        html += "</form>";
        html += "</body></html>";

        server->send(200, "text/html", html);
    }


    void handleSaveSettings() {
        Serial.println("Saving new settings from form...");

        if (!preferences.begin("config", false)) {
            Serial.println("Failed to initialize preferences.");
            server->send(500, "text/html", "Internal Server Error: Failed to initialize preferences.");
            return;
        }

        ssid = server->arg("ssid");
        wifiPassword = server->arg("wifiPassword");
        tableId = server->arg("tableId");
        companyId = server->arg("companyId");
        departmentId = server->arg("departmentId");
        side = server->arg("side");
        apiKey = server->arg("apiKey");
        mqttUsername = server->arg("mqttUsername");
        mqttPassword = server->arg("mqttPassword");


        preferences.putString("ssid", ssid);
        preferences.putString("wifiPassword", wifiPassword);
        preferences.putString("tableId", tableId);
        preferences.putString("companyId", companyId);
        preferences.putString("departmentId", departmentId);
        preferences.putString("side", side);
        preferences.putString("apiKey", apiKey);
        preferences.putString("mqttUsername", mqttUsername);
        preferences.putString("mqttPassword", mqttPassword);


        preferences.end(); // Close preferences before restarting

        Serial.println("New settings saved:");
        Serial.print("SSID: "); Serial.println(ssid);
        Serial.print("WiFi Password: "); Serial.println(wifiPassword);
        Serial.print("Table ID: "); Serial.println(tableId);
        Serial.print("Company ID: "); Serial.println(companyId);
        Serial.print("Department ID: "); Serial.println(departmentId);
        Serial.print("Side: "); Serial.println(side);
        Serial.print("apiKey:"); Serial.println(apiKey);
        Serial.print("mqttUsername:"); Serial.println(mqttUsername);
        Serial.print("mqttPassword:"); Serial.println(mqttPassword);


        server->send(200, "text/html", "Settings saved! Rebooting...");
        delay(1000);
        // Do not call exitConfigMode(); here
        ESP.restart(); // Restart the ESP32
    }

    void enterConfigMode() {
        Serial.println("Entering configuration mode...");

        configMode = true;

        WifiConnector::disconnectWiFi(true);  // Disconnect from any previous Wi-Fi connections
        WifiConnector::setWiFiMode(WIFI_OFF); // Ensure Wi-Fi is off
        delay(100);

        WifiConnector::setWiFiMode(WIFI_AP);     // Set Wi-Fi mode to AP only

        const char* apSSID = "ESP32_Config";
        const char* apPassword = "12345678";

        if (!WifiConnector::startAP(apSSID, apPassword)) {
            Serial.println("Failed to start AP mode");
            return;
        }

        IPAddress IP = WifiConnector::getAPIP();
        Serial.print("AP IP Address: ");
        Serial.println(IP);

        // Initialize mDNS
        if (WifiConnector::startMDNS("espconfig")) {
            Serial.println("mDNS responder started");
        } else {
            Serial.println("Error setting up mDNS responder!");
        }

        LedHandler::enableFlashing();  // Enable LED flashing when entering config mode

        // Initialize the server here
        server = new WebServer(80);

        server->on("/", handleConfigForm);
        server->on("/save", handleSaveSettings);
        server->begin();

        Serial.println("Web server started for configuration.");
    }

    void handleConfigMode() {
        // Serve the configuration form and handle HTTP requests
        if (server != nullptr) {
            server->handleClient();  // Maintain the web server for configuration
        }
        LedHandler::handleFlashing();  // Keep flashing the LED while in config mode

        // Check for config button press to exit config mode
        if (isConfigButtonPressed()) {
            Serial.println("Config button pressed during configuration mode. Exiting configuration mode...");
            exitConfigMode();
        }
    }

    void exitConfigMode() {
        Serial.println("Exiting configuration mode...");
        configMode = false;

        // Stop mDNS responder
        WifiConnector::stopMDNS();

        if (server != nullptr) {
            server->stop();
            delete server;  // Clean up the server
            server = nullptr;
        }

        WifiConnector::stopAP();
        WifiConnector::setWiFiMode(WIFI_OFF);  // Ensure Wi-Fi is off
        delay(100);

        LedHandler::disableFlashing();  // Stop the LED from flashing
    }

    bool configModeActive() {
        return configMode;
    }

    // Simplified function to check if the config button is pressed
    bool isConfigButtonPressed() {
        if (digitalRead(configButtonPin) == LOW) {
            Serial.println("Config button press detected.");
            return true;
        } else {
            return false;
        }
    }

    // Getter functions
    String getTableId() {
        return tableId;
    }

    String getCompanyId() {
        return companyId;
    }

    String getDepartmentId() {
        return departmentId;
    }

    String getSide() {
        return side;
    }

    String getSsid() {
        return ssid;
    }

    String getWifiPassword() {
        return wifiPassword;
    }

    String getApiKey() {
      return apiKey;
    }

    String getMqttUsername() {
      return mqttUsername;
    }

    String getMqttPassword() {
      return mqttPassword;
    }
}
