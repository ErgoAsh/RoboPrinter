#pragma once

#include <display_service.h>

#include <Adafruit_PWMServoDriver.h>
#include <string>
#include <array>

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

class ServoService {
   private:
    Adafruit_PWMServoDriver _pwm_driver;
    DisplayService* _display_service;

   public:
    ServoService(DisplayService* display_service);

    std::array<float, 5> parse_data(const std::string& data, const short length);

    void initialize();
    void process_data(const std::string&, const short length);
    void set_servo_position(const short number, const float position);
};
