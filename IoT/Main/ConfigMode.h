// ConfigMode.h

#ifndef CONFIG_MODE_H
#define CONFIG_MODE_H

#include <WebServer.h>
#include <Preferences.h>
#include "WifiConnector.h"  // Include WifiConnector

namespace ConfigMode {
    extern const int configButtonPin; // Now accessible globally

    void loadSettings();
    bool settingsInvalid();
    void enterConfigMode();
    void handleConfigMode();
    void exitConfigMode();
    void initializeConfigButton();
    bool configModeActive();
    bool isConfigButtonPressed();

    // Getter functions
    String getTableId();
    String getCompanyId();
    String getDepartmentId();
    String getSide();
    String getSsid();
    String getWifiPassword();
    String getApiKey();
}

#endif // CONFIG_MODE_H
