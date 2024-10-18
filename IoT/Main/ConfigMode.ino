// ConfigMode.ino

#include "ConfigMode.h"
#include "LedHandler.h"  // To control LED during config mode

namespace ConfigMode {
    const int configButtonPin = 0; // Use GPIO0 (BOOT button) for config button
    WebServer* server = nullptr;
    Preferences preferences;
    bool configMode = false;

    String ssid = "";
    String password = "";
    String tableId = "";
    String companyId = "";
    String departmentId = "";

    void initializeConfigButton() {
        pinMode(configButtonPin, INPUT_PULLUP);
        Serial.println("Config button initialized.");
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
        password = preferences.getString("password", "");
        tableId = preferences.getString("tableId", "");
        companyId = preferences.getString("companyId", "");
        departmentId = preferences.getString("departmentId", "");

        // Print loaded settings for debugging
        Serial.println("Loading saved settings:");
        Serial.print("SSID: "); Serial.println(ssid);
        Serial.print("Password: "); Serial.println(password);
        Serial.print("Table ID: "); Serial.println(tableId);
        Serial.print("Company ID: "); Serial.println(companyId);
        Serial.print("Department ID: "); Serial.println(departmentId);
    }

    bool settingsInvalid() {
        bool invalid = ssid.isEmpty() || password.isEmpty() || tableId.isEmpty() || companyId.isEmpty() || departmentId.isEmpty();

        if (invalid) {
            Serial.println("Settings are invalid.");
        } else {
            Serial.println("Settings are valid.");
        }

        return invalid;
    }

    void handleConfigForm() {
        Serial.println("Serving configuration form...");
        String html = "<html><body><h1>ESP32 Config</h1><form action='/save' method='POST'>";
        html += "SSID: <input type='text' name='ssid'><br>Password: <input type='password' name='password'><br>";
        html += "Table ID: <input type='text' name='tableId'><br>Company ID: <input type='text' name='companyId'><br>";
        html += "<input type='submit' value='Save'></form></body></html>";
        server->send(200, "text/html", html);
    }

    void handleSaveSettings() {
        Serial.println("Saving new settings from form...");

        ssid = server->arg("ssid");
        password = server->arg("password");
        tableId = server->arg("tableId");
        companyId = server->arg("companyId");

        preferences.putString("ssid", ssid);
        preferences.putString("password", password);
        preferences.putString("tableId", tableId);
        preferences.putString("companyId", companyId);

        Serial.println("New settings saved:");
        Serial.print("SSID: "); Serial.println(ssid);
        Serial.print("Password: "); Serial.println(password);
        Serial.print("Table ID: "); Serial.println(tableId);
        Serial.print("Company ID: "); Serial.println(companyId);

        server->send(200, "text/html", "Settings saved! Rebooting...");
        delay(1000);
        ESP.restart();
    }

    void enterConfigMode() {
        Serial.println("Entering configuration mode...");

        configMode = true;
        WiFi.softAP("ESP32_Config", "12345678");

        IPAddress IP = WiFi.softAPIP();
        Serial.print("AP IP Address: ");
        Serial.println(IP);

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
    }

    void exitConfigMode() {
        Serial.println("Exiting configuration mode...");
        configMode = false;
        WiFi.softAPdisconnect(true);
        if (server != nullptr) {
            server->stop();
            delete server;  // Clean up the server
            server = nullptr;
        }
        LedHandler::disableFlashing();  // Stop the LED from flashing
    }

    bool configModeActive() {
        return configMode;
    }

    String getTableId() {
        return tableId;
    }

    String getCompanyId() {
        return companyId;
    }

    String getSsid() {
        return ssid;
    }

    String getPassword() {
        return password;
    }
}
