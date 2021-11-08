#pragma once

#include <display_service.h>

#include <Adafruit_PWMServoDriver.h>
#include <string>
#include <array>

namespace constants {
// Rounded 'minimum' microsecond length
static const unsigned int servo_us_min = 100;

// Rounded 'maximum' microsecond length
static const unsigned int servo_us_max = 2000;

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
