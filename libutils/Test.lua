--package.cpath = "D:/workspace/visual-studio/2017/repos/libutils/x64/Debug/?.dll"
package.cpath = "D:/workspace/github/libutils/x64/Debug/?.dll"

local libutils = require("libutils")

--local hello = libutils.showHello()
--print(hello)

-- WVVoa2FrMVVTWHBPUkZVeQ==		(hwc123456)
local ret, errMsg = libutils.setFtpInfo("127.0.0.1", "hwc", "WVVoa2FrMVVTWHBPUkZVeQ==")
print("Set FTP Info", ret, errMsg)

ret, errMsg = libutils.uploadToRemote("D:\\demo.rar", "/promptboard/sed", false)
print("Upload File", ret, errMsg)

ret, errMsg = libutils.downloadFromRemote("/background.webp", "D:\\ABCD.webp")
print("Download File", ret, errMsg)