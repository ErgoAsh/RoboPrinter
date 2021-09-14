#include "sensor_control.h"

#include <Arduino.h>
#include <string>
#include <bitset>

std::string SensorControl::get_values_combined() {
    std::bitset<60> set;
    for (int i = 0; i < 5; i++) {
        set |= get_value(i);
        set <<= 12;
    }
    return set.to_string();
}

uint16_t SensorControl::get_value(uint16_t sensor_number) {
    return analogRead(constants::sensor_pins[sensor_number]);
}