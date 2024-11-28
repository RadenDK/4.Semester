#include <Adafruit_NeoPixel.h>  // Include the NeoPixel library
#include "LedHandler.h"

namespace LedHandler {
  int ledState = LOW;  // Current state of the LED (HIGH or LOW)
  unsigned long previousMillis = 0;  // Last time the LED was updated
  const int ledPin = 38;  // Pin connected to the onboard RGB LED
  const int numPixels = 1;  // Number of LEDs (1 for the onboard LED)
  bool shouldFlash = false;  // Toggle to control whether the LED should flash

  Adafruit_NeoPixel pixels(numPixels, ledPin, NEO_GRB + NEO_KHZ800);

  void initializeLed() {
    pixels.begin();  // Initialize the NeoPixel
    pixels.setBrightness(50);  // Set global brightness to 20% (adjust as needed)
    pixels.setPixelColor(0, pixels.Color(0, 0, 0));  // Ensure the LED starts off (black)
    pixels.show();
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
    pixels.setPixelColor(0, pixels.Color(0, 0, 0));  // Turn off the LED
    pixels.show();
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
        pixels.setPixelColor(0, pixels.Color(100, 100, 100));  // Dim white (RGB: 100, 100, 100)
      } else {
        ledState = LOW;
        pixels.setPixelColor(0, pixels.Color(0, 0, 0));  // Turn the LED off
      }

      pixels.show();  // Send the updated color to the LED
    }
  }

  void shortFlash() {
    pixels.setPixelColor(0, pixels.Color(0, 128, 0));  // Dim green (RGB: 0, 128, 0)
    pixels.show();
    delay(100);  // Delay for 100ms (this is a short flash)
    pixels.setPixelColor(0, pixels.Color(0, 0, 0));  // Turn the LED off
    pixels.show();
  }
}
