// WifiConnector.ino

#include "WifiConnector.h"
#include <WiFi.h>
#include "ConfigMode.h"  // Include ConfigMode to access Wi-Fi credentials

namespace WifiConnector {
    void connectToWiFi() {
        String ssid = ConfigMode::getSsid();      // Access the SSID from ConfigMode
        String password = ConfigMode::getPassword();  // Access the password from ConfigMode

        WiFi.begin(ssid.c_str(), password.c_str());
        int attempts = 0;

        // Attempt to connect to Wi-Fi
        while (WiFi.status() != WL_CONNECTED && attempts < 20) {
            delay(1000);
            Serial.print(".");

            // Check for config button press during Wi-Fi connection attempt
            int buttonState = digitalRead(ConfigMode::configButtonPin);
            if (buttonState == LOW && !ConfigMode::configModeActive()) {
                Serial.println("\nConfig button pressed during Wi-Fi connection attempt. Entering configuration mode...");
                ConfigMode::enterConfigMode();
                return; // Exit the function to stop trying to connect to Wi-Fi
            }

            attempts++;
        }

        if (WiFi.status() == WL_CONNECTED) {
            Serial.println("\nConnected to Wi-Fi");
            Serial.print("IP Address: ");
            Serial.println(WiFi.localIP());
        } else {
            Serial.println("\nFailed to connect to Wi-Fi. Entering configuration mode...");
            ConfigMode::enterConfigMode();  // Fallback to configuration mode if connection fails
        }
    }

    void maintainWiFiConnection() {
        if (WiFi.status() != WL_CONNECTED && !ConfigMode::configModeActive()) {
            Serial.println("Wi-Fi disconnected. Attempting to reconnect...");
            connectToWiFi();  // Try to reconnect if the connection is lost
        }
    }

    bool isWiFiConnected() {
        return WiFi.status() == WL_CONNECTED;
    }
}
