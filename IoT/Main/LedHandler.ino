#include "LedHandler.h"

namespace LedHandler {
  int ledState = LOW;  // Current state of the LED (HIGH or LOW)
  unsigned long previousMillis = 0;  // Last time the LED was updated
  const int ledPin = 2;  // Define the LED pin (same as sensorHandler)
  bool shouldFlash = false;  // Toggle to control whether the LED should flash

  void initializeLed() {
    pinMode(ledPin, OUTPUT);  // Set the LED pin as an output
    digitalWrite(ledPin, LOW);  // Ensure the LED starts off
  }

  // This function is called repeatedly during config mode to manage flashing
  void handleFlashing() {
    if (shouldFlash) {
      flashLedConstant(500);  // Flash the LED every 500ms (adjust the interval if needed)
    }
  }

  // Function to start the flashing
  void enableFlashing() {
    shouldFlash = true;
    Serial.println("LED flashing enabled.");
  }

  // Function to stop the flashing
  void disableFlashing() {
    shouldFlash = false;
    digitalWrite(ledPin, LOW);  // Turn off the LED when flashing stops
    Serial.println("LED flashing disabled.");
  }

  // Function to flash the LED at a constant interval
  void flashLedConstant(int interval) {
    unsigned long currentMillis = millis();  // Get the current time

    if (currentMillis - previousMillis >= interval) {
      previousMillis = currentMillis;  // Save the last time the LED was toggled

      // Toggle the LED state
      if (ledState == LOW) {
        ledState = HIGH;
      } else {
        ledState = LOW;
      }

      digitalWrite(ledPin, ledState);  // Update the LED with the new state
    }
  }

  void shortFlash() {
    digitalWrite(ledPin, HIGH);  // Turn the LED on
    delay(100);  // Delay for 100ms (this is a short flash)
    digitalWrite(ledPin, LOW);  // Turn the LED off
  }
}
