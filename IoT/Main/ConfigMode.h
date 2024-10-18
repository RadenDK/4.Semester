// ConfigMode.h

#ifndef CONFIG_MODE_H
#define CONFIG_MODE_H

#include <WiFi.h>
#include <WebServer.h>
#include <Preferences.h>

namespace ConfigMode {
    extern const int configButtonPin; // Now accessible globally

    void loadSettings();
    bool settingsInvalid();
    void enterConfigMode();
    void handleConfigMode();
    void exitConfigMode();
    void initializeConfigButton();
    bool configModeActive();
    String getTableId();
    String getCompanyId();
    String getSsid();
    String getPassword();
}

#endif // CONFIG_MODE_H
