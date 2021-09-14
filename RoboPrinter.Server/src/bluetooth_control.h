#pragma once

#include <BLE2902.h>
#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>

#include "servo_control.h"

namespace constants {
static const char* service_uuid = "e31ba033-e901-4566-afb2-8c545c554c8b";
static const char* sensor_uuid = "d8593ea1-0153-4397-9f2e-13703b529215";
static const char* position_uuid = "944eedc2-3725-425e-9817-0ab0bfda64fe";
}  // namespace constants

class CustomServerCallbacks : public BLEServerCallbacks {
   private:
    BluetoothControl* _control;

   public:
    CustomServerCallbacks(BluetoothControl* control);
    void onConnect(BLEServer* ble_server) override;
    void onDisconnect(BLEServer* ble_server) override;
    void onMtuChanged(BLEServer* pServer,
                      esp_ble_gatts_cb_param_t* param) override;
};

class CustomResponseCallbacks : public BLECharacteristicCallbacks {
   private:
    ServoControl* _servo_control;

   public:
    CustomResponseCallbacks(ServoControl* servo_control);
    void onWrite(BLECharacteristic* characteristic,
                 esp_ble_gatts_cb_param_t* param) override;
};

class BluetoothControl {
   private:
    ServoControl* _servo_control;
    BLEServer* ble_server;
    BLECharacteristic* sensor_characteristic;
    BLECharacteristic* position_characteristic;

   public:
    bool central_connected;
    bool old_central_connected;

    BluetoothControl(ServoControl* servo_control);
    void initialize();
    void on_loop();
};
