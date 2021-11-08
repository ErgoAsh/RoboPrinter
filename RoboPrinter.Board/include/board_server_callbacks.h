#pragma once

#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>

#include "bluetooth_service.h"

class BluetoothService;

class BoardServerCallbacks : public BLEServerCallbacks {
   private:
    BluetoothService* _service;

   public:
    BoardServerCallbacks(BluetoothService* service);
    void onConnect(BLEServer* ble_server) override;
    void onDisconnect(BLEServer* ble_server) override;
    void onMtuChanged(BLEServer* pServer,
                      esp_ble_gatts_cb_param_t* param) override;
};