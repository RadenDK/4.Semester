// sensorHandler.ino

#include "sensorHandler.h"
#include "LedHandler.h"      // To handle LED interaction
#include "ApiRequester.h"    // To send HTTP requests

namespace sensorHandler {
  const int sensorPin = 23;          // Sensor pin
  unsigned long lastTriggerTime = 0; // Time of the last trigger
  const unsigned long debounceDelay = 500; // Debounce delay in milliseconds

  void initializeSensor() {
    pinMode(sensorPin, INPUT_PULLDOWN);  // Enable internal pull-down resistor
    LedHandler::initializeLed();         // Initialize the LED
    Serial.println("Sensor and LED initialized.");
  }

  void monitorSensor() {
    int sensorState = digitalRead(sensorPin);
    unsigned long currentTime = millis();

    if (sensorState == HIGH && (currentTime - lastTriggerTime >= debounceDelay)) {
      lastTriggerTime = currentTime;
      Serial.println("Sensor triggered, sending HTTP request...");
      LedHandler::shortFlash();
      ApiRequester::sendHttpRequest();
      Serial.println("HTTP request sent.");
    }
  }
}
