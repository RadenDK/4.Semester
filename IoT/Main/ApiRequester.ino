// ApiRequester.ino

#include "ApiRequester.h"
#include <WiFi.h>
#include <HTTPClient.h>
#include "ConfigMode.h"  // Include ConfigMode to access settings

namespace ApiRequester {

    void sendRegisterGoal() {
        if (WiFi.status() == WL_CONNECTED) {
            HTTPClient http;

            // Retrieve necessary data using ConfigMode getter functions
            String tableId = ConfigMode::getTableId();
            String companyId = ConfigMode::getCompanyId();
            String departmentId = ConfigMode::getDepartmentId();
            String side = ConfigMode::getSide();

            // Prepare the request
            String url = "https://api.foosballproleague.live/test";
            http.begin(url);
            http.addHeader("Content-Type", "application/json");

            // Create the JSON payload
            String jsonPayload = "{\"tableId\":\"" + tableId + "\", \"companyId\":\"" + companyId + "\", \"departmentId\":\"" + departmentId + "\", \"side\":\"" + side + "\"}";

            Serial.println("Sending RegisterGoal HTTP POST request with payload:");
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

    void sendInterruptMatch() {
        if (WiFi.status() == WL_CONNECTED) {
            HTTPClient http;

            // Retrieve necessary data using ConfigMode getter functions
            String tableId = ConfigMode::getTableId();

            // Prepare the request
            String url = "https://api.foosballproleague.live/test";
            http.begin(url);
            http.addHeader("Content-Type", "application/json");

            // Create the JSON payload
            String jsonPayload = "{\"tableId\":\"" + tableId + "\"}";

            Serial.println("Sending InterruptMatch HTTP POST request with payload:");
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

    void sendStartMatch() {
        if (WiFi.status() == WL_CONNECTED) {
            HTTPClient http;

            // Retrieve necessary data using ConfigMode getter functions
            String tableId = ConfigMode::getTableId();

            // Prepare the request
            String url = "https://api.foosballproleague.live/test";
            http.begin(url);
            http.addHeader("Content-Type", "application/json");

            // Create the JSON payload
            String jsonPayload = "{\"tableId\":\"" + tableId + "\"}";

            Serial.println("Sending StartMatch HTTP POST request with payload:");
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

    void sendLoginOnTable() {
        if (WiFi.status() == WL_CONNECTED) {
            HTTPClient http;

            // Placeholder for userId since it's not retrieved from ConfigMode yet
            String userId = "exampleUserId";  // Replace with actual method when available
            String tableId = ConfigMode::getTableId();
            String side = ConfigMode::getSide();

            // Prepare the request
            String url = "https://api.foosballproleague.live/test";
            http.begin(url);
            http.addHeader("Content-Type", "application/json");

            // Create the JSON payload
            String jsonPayload = "{\"userId\":\"" + userId + "\", \"tableId\":\"" + tableId + "\", \"side\":\"" + side + "\"}";

            Serial.println("Sending LoginOnTable HTTP POST request with payload:");
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
