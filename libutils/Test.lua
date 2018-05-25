--package.cpath = "D:/workspace/visual-studio/2017/repos/libutils/x64/Debug/?.dll"
package.cpath = "D:/workspace/github/libutils/x64/Debug/?.dll"

local libutils = require("libutils")

--local hello = libutils.showHello()
--print(hello)

local ret, errMsg
-- WVVoa2FrMVVTWHBPUkZVeQ==		(hwc123456)
--ret, errMsg = libutils.setFtpInfo("127.0.0.1", "hwc", "WVVoa2FrMVVTWHBPUkZVeQ==")
--print("Set FTP Info", ret, errMsg)

local sessionId = libutils.uploadToRemote("D:\\demo.rar", "/promptboard/sed", true)
print("Upload File: sessionId is ", sessionId)

sessionId = libutils.downloadFromRemote("/background.webp", "D:\\ABCD.webp")
print("Download File: sessionId is ", sessionId)