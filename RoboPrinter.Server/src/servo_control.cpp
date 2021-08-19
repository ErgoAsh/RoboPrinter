#include "servo_control.h"

#include <Arduino.h>
#include <stdlib.h>

ServoControl::ServoControl() { pwm_driver = Adafruit_PWMServoDriver(); }

void ServoControl::initialize() {
    pwm_driver.begin();  // TODO adjust values
    pwm_driver.setOscillatorFrequency(27000000);
    pwm_driver.setPWMFreq(
        constants::servo_freq);  // Analog servos run at ~50 Hz updates
}

void ServoControl::process_serial_data(const char* data, const short length) {
    if (data == NULL) return;  // TODO return bool
    if (length < 0) return;

    short id_number = data[0] - 65;
    float position = atof(data + 1);

    Serial.println(id_number);
    Serial.println(position);

    set_servo_position(id_number, position);
}

void ServoControl::set_servo_position(const short servo_number,
                                      const float position) {
    if (servo_number < 0 ||
        servo_number > 5)  // TODO save & retrive value from somewhere
        return;            // TODO Return bool or throw error

    if (position < 0 || position > 180) return;

    pwm_driver.setPWM(
        servo_number, 0,  // TODO save servo constants in servo structure
        map(position, 0, 180, constants::servo_min, constants::servo_max));
}