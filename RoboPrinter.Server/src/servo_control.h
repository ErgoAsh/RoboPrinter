#pragma once

#include <Adafruit_PWMServoDriver.h>
#include "display_control.h"

namespace constants {
// This is the 'minimum' pulse length count (out of 4096)
static const unsigned int servo_min = 150;

// This is the 'maximum' pulse length count (out of 4096)
static const unsigned int servo_max = 600;

// This is the rounded 'minimum' microsecond length based on the minimum pulse
// of 150
static const unsigned int servo_us_min = 600;

// This is the rounded 'maximum' microsecond length based on the maximum pulse
// of 600
static const unsigned int servo_us_max = 2400;

// Analog servos run at ~50 Hz updates
static const unsigned int servo_freq = 50;
}  // namespace constants

struct Servo {
    short id_number;
    float position_min;
    float position_max;
};

class ServoControl {
   private:
    Adafruit_PWMServoDriver _pwm_driver;
    DisplayControl* _display_control;

   public:
    ServoControl(DisplayControl* display_control);
    void initialize();
    void process_data(const std::string&, const short length);
    void set_servo_position(const short number, const float position);
};
