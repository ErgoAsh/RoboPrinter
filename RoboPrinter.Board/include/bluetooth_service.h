#pragma once

#include <BLE2902.h>
#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>

#include <servo_service.h>

namespace constants {
static const char* service_uuid = "e31ba033-e901-4566-afb2-8c545c554c8b";
static const char* sensor_uuid = "d8593ea1-0153-4397-9f2e-13703b529215";
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

class CustomServerCallbacks : public BLEServerCallbacks {
   private:
    BluetoothService* _service;

   public:
    CustomServerCallbacks(BluetoothService* service);
    void onConnect(BLEServer* ble_server) override;
    void onDisconnect(BLEServer* ble_server) override;
    void onMtuChanged(BLEServer* pServer,
                      esp_ble_gatts_cb_param_t* param) override;
};

class CustomResponseCallbacks : public BLECharacteristicCallbacks {
   private:
    ServoService* _servo_service;

   public:
    CustomResponseCallbacks(ServoService* servo_service);
    void onWrite(BLECharacteristic* characteristic,
                 esp_ble_gatts_cb_param_t* param) override;
};