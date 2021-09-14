#pragma once

#include <stdint.h>
#include <string>

namespace constants {
    static const unsigned int sensor_pins[5] = { 32, 33, 34, 35, 36 };
};

class SensorControl {
    std::string get_values_combined();
    uint16_t get_value(uint16_t sensor_number);
};