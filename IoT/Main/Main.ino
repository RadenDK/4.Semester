#include "ConfigMode.h"
#include "WifiConnector.h"
#include "SensorHandler.h"
#include "ApiRequester.h"
#include "LedHandler.h"
#include "MqttHandler.h"
#include "LcdHandler.h"

void setup() {
    Serial.begin(115200);
    delay(2000);

    Serial.println("Starting setup...");
    ConfigMode::initializeConfigButton();
    WifiConnector::disconnectWiFi(true);
    WifiConnector::setWiFiMode(WIFI_OFF);

    if (digitalRead(ConfigMode::configButtonPin) == LOW) {
        // Force config mode if button is pressed
        ConfigMode::enterConfigMode();
    } 
    else {
        // Load saved settings
        ConfigMode::loadSettings();

        // If settings are incomplete, go to config mode
        if (ConfigMode::settingsInvalid()) {
            ConfigMode::enterConfigMode();
        } 
        else {
            // Attempt to connect using the loaded settings
            bool connected = WifiConnector::connectToWiFi();
            if (connected) {
                // Only initialize MQTT if the station actually connected
                MqttHandler::initialize();
            } else {
                // If Wi-Fi fails, enter config mode
                ConfigMode::enterConfigMode();
            }
        }
    }

    // Initialize the LCD at any time
    LcdHandler::initializeLCD(0x3F, 16, 2);

    // Initialize sensors
    sensorHandler::initializeSensors();
}

void loop() {
    if (ConfigMode::configModeActive()) {
        // We are in config mode: serve the form, etc.
        ConfigMode::handleConfigMode();
    } else {
        // Normal operation:
        WifiConnector::maintainWiFiConnection();

        // If Wi-Fi is up, handle sensors & MQTT
        if (WifiConnector::isWiFiConnected()) {
            sensorHandler::monitorSensors();
            MqttHandler::loop();  // Maintain MQTT connection
        }

        // If user presses config button mid-run:
        if (ConfigMode::isConfigButtonPressed()) {
            ConfigMode::enterConfigMode();
        }
    }
}
