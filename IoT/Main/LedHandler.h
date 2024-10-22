#ifndef LEDHANDLER_H
#define LEDHANDLER_H

namespace LedHandler {
  extern int ledState;
  extern unsigned long previousMillis;
  extern bool shouldFlash;  // Flag for controlling flash

  void initializeLed();  
  void flashLedConstant(int interval);
  void shortFlash();
  void enableFlashing();   // Function to start the flashing
  void disableFlashing();  // Function to stop the flashing
  void handleFlashing();   // Function to handle the flashing based on the toggle
}

#endif
