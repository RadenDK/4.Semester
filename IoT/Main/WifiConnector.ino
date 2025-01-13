// WifiConnector.ino

#include "WifiConnector.h"
#include "ConfigMode.h"  // Include ConfigMode to access Wi-Fi credentials

namespace WifiConnector {

    // WifiConnector.ino

bool connectToWiFi() {
    String ssid = ConfigMode::getSsid();
    String wifiPassword = ConfigMode::getWifiPassword();

    WiFi.mode(WIFI_STA);
    WiFi.disconnect(true);

    delay(100);

    Serial.println("\nAttempting to connect to Wi-Fi:");
    Serial.print("SSID: "); Serial.println(ssid);
    Serial.print("Wi-Fi Password: "); Serial.println(wifiPassword);

    WiFi.begin(ssid.c_str(), wifiPassword.c_str());

    unsigned long startAttemptTime = millis();
    const unsigned long connectionTimeout = 30000; // 30 seconds

    while (WiFi.status() != WL_CONNECTED && (millis() - startAttemptTime) < connectionTimeout) {
        delay(500);
        Serial.print(".");

        // Check for config button press
        if (ConfigMode::isConfigButtonPressed()) {
            Serial.println("\nConfig button pressed during Wi-Fi connection attempt. Entering configuration mode...");
            ConfigMode::enterConfigMode();
            return false;  // Return false because we never connected to Wi-Fi
        }
    }

    if (WiFi.status() == WL_CONNECTED) {
        Serial.println("\nConnected to Wi-Fi");
        Serial.print("IP Address: ");
        Serial.println(WiFi.localIP());
        return true;  // Return true on success
    } else {
        Serial.println("\nFailed to connect to Wi-Fi within the timeout period.");
        ConfigMode::enterConfigMode();
        return false;  // Return false on failure
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

    void setWiFiMode(wifi_mode_t mode) {
        WiFi.mode(mode);
    }

    void disconnectWiFi(bool reset) {
        WiFi.disconnect(reset);
    }

    bool startAP(const char* ssid, const char* password) {
        WiFi.softAPdisconnect(true); // Ensure previous AP is stopped
        setWiFiMode(WIFI_AP);
        delay(100);
        bool result = WiFi.softAP(ssid, password);
        return result;
    }

    void stopAP() {
        WiFi.softAPdisconnect(true);
    }

    IPAddress getAPIP() {
        return WiFi.softAPIP();
    }

    bool startMDNS(const char* hostname) {
        bool result = MDNS.begin(hostname);
        return result;
    }

    void stopMDNS() {
        MDNS.end();
    }

}
