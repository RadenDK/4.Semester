// Main.ino

#include "ConfigMode.h"
#include "WifiConnector.h"
#include "SensorHandler.h"
#include "ApiRequester.h"
#include "LedHandler.h"

void setup() {
    Serial.begin(115200);
    delay(2000); // Wait for 2 seconds to ensure the serial port is ready

    Serial.println("Starting setup...");

    // Initialize the config button
    ConfigMode::initializeConfigButton();

    // Ensure Wi-Fi is off before starting
    WifiConnector::disconnectWiFi(true);
    WifiConnector::setWiFiMode(WIFI_OFF);
    delay(100);

    // Check if config button is pressed during startup
    if (digitalRead(ConfigMode::configButtonPin) == LOW) {
        Serial.println("Config button pressed during startup. Entering configuration mode...");
        ConfigMode::enterConfigMode();
    } else {
        // Load saved settings and attempt to connect to Wi-Fi
        Serial.println("Calling loadSettings...");
        ConfigMode::loadSettings();

        Serial.println("Checking if settings are valid or invalid...");
        if (ConfigMode::settingsInvalid()) {
            Serial.println("Settings invalid. Entering configuration mode...");
            ConfigMode::enterConfigMode();
        } else {
            Serial.println("Settings valid. Attempting to connect to Wi-Fi...");
            WifiConnector::connectToWiFi();
        }
    }

    // Initialize the sensor handler
    Serial.println("Initializing sensor and LED...");
    sensorHandler::initializeSensor();
}

void loop() {
    if (ConfigMode::configModeActive()) {
        ConfigMode::handleConfigMode();  // Handle configuration via web interface
    } else {
        // Maintain Wi-Fi connection and reconnect if necessary
        WifiConnector::maintainWiFiConnection();

        // Only call sensorHandler if we are connected to Wi-Fi
        if (WifiConnector::isWiFiConnected()) {
            sensorHandler::monitorSensor();  // Monitor sensor and trigger actions only when Wi-Fi is connected
        }

        // Check for config button press
        if (ConfigMode::isConfigButtonPressed()) {
            Serial.println("Config button pressed. Entering configuration mode...");
            ConfigMode::enterConfigMode();
        }
    }
}
