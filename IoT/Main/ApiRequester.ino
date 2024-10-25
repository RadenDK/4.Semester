// ApiRequester.ino

#include "ApiRequester.h"
#include <WiFi.h>
#include <HTTPClient.h>
#include "ConfigMode.h"  // Include ConfigMode to access settings

namespace ApiRequester {
    void sendHttpRequest() {
        if (WiFi.status() == WL_CONNECTED) {
            HTTPClient http;

            // Retrieve the necessary data using ConfigMode getter functions
            String tableId = ConfigMode::getTableId();
            String companyId = ConfigMode::getCompanyId();
            String departmentId = ConfigMode::getDepartmentId();
            String side = ConfigMode::getSide();

            // Prepare the request
            String url = "https://api.foosballproleague.live/test";
            http.begin(url);
            http.addHeader("Content-Type", "application/json");

            // Create the JSON payload including departmentId and side
            String jsonPayload = "{\"tableId\":\"" + tableId + "\", \"companyId\":\"" + companyId + "\", \"departmentId\":\"" + departmentId + "\", \"side\":\"" + side + "\"}";

            Serial.println("Sending HTTP POST request with payload:");
            Serial.println(jsonPayload);

            int httpResponseCode = http.POST(jsonPayload);

            if (httpResponseCode > 0) {
                String response = http.getString();
                Serial.println("Response: " + response);
            } else {
                Serial.println("Error: Failed to send POST request. HTTP response code: " + String(httpResponseCode));
            }

            http.end();  // Free resources
        } else {
            Serial.println("Error: Not connected to WiFi");
        }
    }
}
