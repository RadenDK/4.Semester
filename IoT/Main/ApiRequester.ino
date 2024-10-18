#include "ApiRequester.h"
#include <WiFi.h>
#include <HTTPClient.h>
#include "ConfigMode.h"  // Include ConfigMode to access settings like tableId and companyId

namespace ApiRequester {
  void sendHttpRequest() {
    if (WiFi.status() == WL_CONNECTED) {
      HTTPClient http;

      // Retrieve the necessary data using ConfigMode getter functions
      String tableId = ConfigMode::getTableId();
      String companyId = ConfigMode::getCompanyId();

      // Prepare the request
      String url = "https://api.foosballproleague.live/test";
      http.begin(url);
      http.addHeader("Content-Type", "application/json");

      // Create the JSON payload
      String jsonPayload = "{\"tableId\":\"" + tableId + "\", \"companyId\":\"" + companyId + "\"}";
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
