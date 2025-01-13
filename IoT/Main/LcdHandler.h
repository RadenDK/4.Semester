#ifndef LCDHANDLER_H
#define LCDHANDLER_H

#include <LiquidCrystal_I2C.h>

namespace LcdHandler {
    void initializeLCD(uint8_t address, uint8_t columns, uint8_t rows);
    void UpdateLcdSetScores(int redScore, int blueScore);
}

#endif
