#include "LcdHandler.h"

namespace LcdHandler {
    LiquidCrystal_I2C lcd(0x3F, 16, 2); // Default I2C address; adjust if necessary

    void initializeLCD(uint8_t address, uint8_t columns, uint8_t rows) {
        Wire.begin(17, 3); // SDA = GPIO 17, SCL = GPIO 3
        lcd = LiquidCrystal_I2C(address, columns, rows);
        lcd.init();
        lcd.backlight();
        lcd.clear();
        lcd.print("Starting...");
    }

    void UpdateLcdSetScores(int redScore, int blueScore) {
        // Clear the LCD screen
        lcd.clear();

        // Display the first row
        lcd.setCursor(1, 0); // Column 1, Row 0
        lcd.print("Red");

        lcd.setCursor(7, 0); // Column 7, Row 0
        lcd.print("VS");

        lcd.setCursor(11, 0); // Column 11, Row 0
        lcd.print("Blue");

        // Display the second row with scores
        lcd.setCursor(2, 1); // Column 2, Row 1
        lcd.print(redScore);

        lcd.setCursor(12, 1); // Column 12, Row 1
        lcd.print(blueScore);
    }
}
