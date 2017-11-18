@echo OFF

REM This script expects to be run from the clients folder.

if not exist ".\ReactWindows\Playground\ReactAssets" mkdir .\ReactWindows\Playground\ReactAssets

node .\node_modules\react-native\local-cli\cli.js bundle ^
  --entry-file .\ReactWindows\Playground\index.windows.js ^
  --platform windows ^
  --reset-cache ^
  --dev true ^
  --bundle-output .\ReactWindows\Playground\ReactAssets\index.windows.bundle ^
  --assets-dest ..\ReactWindows\Playground\ReactAssets\

REM --sourcemap-output TBD ^
