// sensorHandler.ino

#include "sensorHandler.h"
#include "LedHandler.h"      // To handle LED interaction
#include "ApiRequester.h"    // To send HTTP requests

namespace sensorHandler {
    const int numSensors = 4;
    const int sensorPins[numSensors] = {1, 7, 11}; // Sensor pins
    unsigned long lastTriggerTimes[numSensors] = {0};   // Time of the last trigger for each sensor
    const unsigned long debounceDelay = 1500;            // Debounce delay in milliseconds

    void initializeSensors() {
        for (int i = 0; i < numSensors; i++) {
            pinMode(sensorPins[i], INPUT_PULLUP);  // Enable internal pull-up resistor / register if sensorstate is LOW
        }
        LedHandler::initializeLed();         // Initialize the LED
        Serial.println("Sensors and LED initialized.");
    }

    void monitorSensors() {
        unsigned long currentTime = millis();

        for (int i = 0; i < numSensors; i++) {
            int sensorState = digitalRead(sensorPins[i]);

            if (sensorState == LOW && (currentTime - lastTriggerTimes[i] >= debounceDelay)) {
                lastTriggerTimes[i] = currentTime;
                Serial.print("Sensor on pin ");
                Serial.print(sensorPins[i]);
                Serial.println(" triggered, sending HTTP request...");
                LedHandler::shortFlash();

                // Trigger the corresponding API request based on the pin
                switch(sensorPins[i]) {
                    case 1:
                        ApiRequester::sendRegisterGoal();
                        break;
                    case 7:
                        ApiRequester::sendInterruptMatch();
                        break;
                    case 11:
                        ApiRequester::sendStartMatch();
                        break;
                    default:
                        Serial.println("Unknown sensor pin!");
                        break;
                }
            }
        }
    }
}
