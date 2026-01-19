#include <iostream>
#include <dlfcn.h>
#include <unistd.h>

// Defines based on standard openvr_api.json
typedef void* (*VR_InitInternal_Func)(int*, int);
typedef void* (*VR_InitInternal2_Func)(int*, int, const char*);
typedef bool (*VR_IsHmdPresent_Func)();

int main() {
    const char* libPath = "./Packages/com.valvesoftware.unity.openvr/Runtime/x64/libopenvr_api.so"; 
    
    std::cout << "[1] Loading Library: " << libPath << std::endl;
    void* handle = dlopen(libPath, RTLD_LAZY);

    if (!handle) {
        std::cerr << "FATAL: Failed to load library: " << dlerror() << std::endl;
        return 1;
    }

    // Try to find the Init function (it changes names between versions)
    VR_InitInternal_Func VR_Init = (VR_InitInternal_Func)dlsym(handle, "VR_InitInternal");
    VR_InitInternal2_Func VR_Init2 = (VR_InitInternal2_Func)dlsym(handle, "VR_InitInternal2");
    
    // Also check for IsHmdPresent
    VR_IsHmdPresent_Func VR_IsHmdPresent = (VR_IsHmdPresent_Func)dlsym(handle, "VR_IsHmdPresent");

    if (!VR_Init && !VR_Init2) {
        std::cerr << "FATAL: Could not find 'VR_InitInternal' OR 'VR_InitInternal2'." << std::endl;
        std::cerr << "Run 'nm -D libopenvr_api.so | grep VR_' to see actual symbol names." << std::endl;
        return 1;
    }

    // ---------------------------------------------------------
    // TEST 1: Check if the API thinks a headset is there
    // ---------------------------------------------------------
    if (VR_IsHmdPresent) {
        std::cout << "[2] Checking VR_IsHmdPresent()..." << std::endl;
        bool present = VR_IsHmdPresent();
        std::cout << "    Result: " << (present ? "TRUE (Headset Found)" : "FALSE (No Headset)") << std::endl;
    } else {
        std::cout << "[!] VR_IsHmdPresent symbol missing. Skipping check." << std::endl;
    }

    // ---------------------------------------------------------
    // TEST 2: Attempt Initialization
    // ---------------------------------------------------------
    std::cout << "[3] Attempting VR_Init..." << std::endl;
    int err = 0;
    void* system = nullptr;

    if (VR_Init2) {
        std::cout << "    Using VR_InitInternal2..." << std::endl;
        // 1 = Scene Application (Game)
        system = VR_Init2(&err, 1, nullptr);
    } else {
        std::cout << "    Using VR_InitInternal..." << std::endl;
        system = VR_Init(&err, 1);
    }

    if (err != 0) {
        std::cout << "FAIL: Init returned error code: " << err << std::endl;
        
        // Common Error Codes Decoder:
        if (err == 108) std::cout << "      -> 108 (HmdNotFound): Drivers loaded, but headset not detected." << std::endl;
        if (err == 109) std::cout << "      -> 109 (NotInitialized): Generic failure." << std::endl;
        if (err == 112) std::cout << "      -> 112 (DriverFailed): Driver loaded but crashed." << std::endl;
        if (err == 126) std::cout << "      -> 126 (InstallationNotFound): Could not load the driver DLL (Dependency missing?)." << std::endl;
    } else {
        std::cout << "SUCCESS: VR System Initialized!" << std::endl;
    }

    return 0;
}
