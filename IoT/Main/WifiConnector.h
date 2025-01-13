// WifiConnector.h

#ifndef WIFI_CONNECTOR_H
#define WIFI_CONNECTOR_H

#include <WiFi.h>
#include <ESPmDNS.h> // Include mDNS

namespace WifiConnector {
    bool connectToWiFi();
    void maintainWiFiConnection();
    bool isWiFiConnected();

    void setWiFiMode(wifi_mode_t mode);
    void disconnectWiFi(bool reset);
    bool startAP(const char* ssid, const char* password);
    void stopAP();
    IPAddress getAPIP();

    bool startMDNS(const char* hostname);
    void stopMDNS();
}

#endif // WIFI_CONNECTOR_H
