#pragma once

#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include <Wire.h>

namespace constants {
static const short screen_width = 128;
static const short screen_height = 64;
}  // namespace constants

class DisplayControl {
   private:
    Adafruit_SSD1306 driver;

   public:
    void initialize();
    void DisplayControl::display_servo_values(uint32_t* values, uint8_t length);
};