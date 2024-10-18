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

    // Check if config button is pressed during startup
    if (digitalRead(ConfigMode::configButtonPin) == LOW) {
        Serial.println("Config button pressed during startup. Entering configuration mode...");
        ConfigMode::enterConfigMode();
    } else {
        // Load saved settings and attempt to connect to Wi-Fi
        Serial.println("Calling loadSettings...");
        ConfigMode::loadSettings();  // Ensure this is called

        // Print debug message for checking settings
        Serial.println("Checking if settings are valid or invalid...");
        if (ConfigMode::settingsInvalid()) {
            // If settings are invalid, print message and enter configuration mode
            Serial.println("Settings invalid. Entering configuration mode...");
            ConfigMode::enterConfigMode();  
        } else {
            // If settings are valid, print message and attempt Wi-Fi connection
            Serial.println("Settings valid. Attempting to connect to Wi-Fi...");
            WifiConnector::connectToWiFi();  // Connect to Wi-Fi
        }
    }

    // Initialize the sensor handler
    Serial.println("Initializing sensor and LED...");
    sensorHandler::initializeSensor();
}

void loop() {
    // Debounce variables
    static unsigned long lastButtonPress = 0;
    const unsigned long debounceDelay = 200; // 200 milliseconds

    // Read the button state
    int buttonState = digitalRead(ConfigMode::configButtonPin);

    // Check if the button is pressed and debounce
    if (buttonState == LOW && !ConfigMode::configModeActive()) {
        unsigned long currentTime = millis();
        if (currentTime - lastButtonPress >= debounceDelay) {
            lastButtonPress = currentTime;
            Serial.println("Config button pressed. Entering configuration mode...");
            ConfigMode::enterConfigMode();
        }
    }

    if (ConfigMode::configModeActive()) {
        ConfigMode::handleConfigMode();  // Handle configuration via web interface
    } else {
        // Maintain Wi-Fi connection and reconnect if necessary
        WifiConnector::maintainWiFiConnection();

        // Only call sensorHandler if we are connected to Wi-Fi
        if (WifiConnector::isWiFiConnected()) {
            sensorHandler::monitorSensor();  // Monitor sensor and trigger actions only when Wi-Fi is connected
        }
    }
}
