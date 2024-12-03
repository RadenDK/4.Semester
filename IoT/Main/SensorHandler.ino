// sensorHandler.ino

#include "sensorHandler.h"
#include "LedHandler.h"      // To handle LED interaction
#include "ApiRequester.h"    // To send HTTP requests
#include <TinyUSB.h>         // Include TinyUSB library
#include <usbhub.h>

namespace sensorHandler {
    const int numSensors = 4;
    const int sensorPins[numSensors] = {1, 7, 11}; // Sensor pins
    unsigned long lastTriggerTimes[numSensors] = {0};   // Time of the last trigger for each sensor
    const unsigned long debounceDelay = 1500;            // Debounce delay in milliseconds

    USBHost usbHost;  // USB host object
    USBHub hub(&usbHost);  // USB hub if using multiple devices (optional)
    TinyUSB_Device *usbSerial;  // Pointer to the USB serial device

    void initializeSensors() {
        for (int i = 0; i < numSensors; i++) {
            pinMode(sensorPins[i], INPUT_PULLUP);  // Enable internal pull-up resistor / register if sensorstate is LOW
        }
        LedHandler::initializeLed();         // Initialize the LED
        Serial.println("Sensors and LED initialized.");


        if (usbHost.begin()) {
            Serial.println("USB Host initialized.");
        } else {
            Serial.println("USB Host initialization failed.");
        }
    }

    void monitorRFIDScanner(){
      usbHost.Task();

      // Check if the RFID scanner is connected and read data from it
        if (usbSerial != nullptr && usbSerial->available() > 0) {
            String rfidData = ""; // To store RFID data
            while (usbSerial->available() > 0) {
                char c = usbSerial->read(); // Read character from USB serial
                rfidData += c;              // Append to string
                delay(5);                   // Allow data to stream in
            }

            // Process the RFID data
            rfidData.trim();  // Remove any leading/trailing whitespace
            Serial.print("RFID card detected: ");
            Serial.println(rfidData);

            // Example: Send the RFID data via API
            ApiRequester::sendLoginOnTable(rfidData);
        }
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

        monitorRFIDScanner();
    }
}
