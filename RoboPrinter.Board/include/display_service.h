#pragma once

#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include <Wire.h>
#include <array>

namespace constants {
static const short screen_width = 128;
static const short screen_height = 64;
}  // namespace constants

class DisplayService {
   private:
    Adafruit_SSD1306 driver;

   public:
    DisplayService();
    void initialize();
    void display_servo_values(std::array<float, 5> values);
};