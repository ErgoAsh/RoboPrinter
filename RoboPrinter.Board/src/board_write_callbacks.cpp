#include "board_write_callbacks.h"
#include <Arduino.h>

BoardWriteCallbacks::BoardWriteCallbacks(ServoService* servo_service) {
    _servo_service = servo_service;
}

void BoardWriteCallbacks::onWrite(BLECharacteristic* characteristic,
                                      esp_ble_gatts_cb_param_t* param) {
    auto input_data = characteristic->getValue().c_str();
    auto length = characteristic->getValue().length();
    
    //characteristic->

    Serial.print("Received message: ");
    Serial.print(input_data);
    Serial.println();

    _servo_service->process_data(characteristic->getValue(), length);
}