#pragma once

#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>

#include "servo_service.h"

class ServoService;

class BoardWriteCallbacks : public BLECharacteristicCallbacks {
   private:
    ServoService* _servo_service;

   public:
    BoardWriteCallbacks(ServoService* servo_service);
    void onWrite(BLECharacteristic* characteristic,
                 esp_ble_gatts_cb_param_t* param) override;
};