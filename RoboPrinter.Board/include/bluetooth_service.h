#pragma once

#include <BLE2902.h>
#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>

#include <servo_service.h>
#include "board_server_callbacks.h"
#include "board_write_callbacks.h"

namespace constants { 
static const char* service_uuid = "e31ba033-e901-4566-afb2-8c545c554c8b";
static const char* sensor_uuid = "d8593ea1-0153-4397-9f2e-13703b529215";
static const char* latency_test_uuid = "221e315e-37f6-11ec-8d3d-0242ac130003";
static const char* position_uuid = "944eedc2-3725-425e-9817-0ab0bfda64fe";
}  // namespace constants

class BluetoothService {
   private:
    ServoService* _servo_service;
    BLEServer* ble_server;
    BLECharacteristic* sensor_characteristic;
    BLECharacteristic* position_characteristic;

   public:
    bool central_connected;
    bool old_central_connected;

    BluetoothService(ServoService* servo_service);
    void initialize();
    void on_loop();
};