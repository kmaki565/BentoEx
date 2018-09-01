@echo off
pushd "C:\Program Files (x86)\Cisco\Cisco AnyConnect Secure Mobility Client"
echo state | vpncli -s | find /C "state: Connected"
if errorlevel 1 goto done

echo disconnect | vpncli -s

:done
popd
