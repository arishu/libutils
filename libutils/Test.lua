package.cpath = "D:/workspace/github/libutils/x64/Debug/?.dll"

require("libutils")

--local hello = libutils.showHello()
--print(hello)

-- WVVoa2FrMVVTWHBPUkZVeQ==		(hwc123456)
--[[
local ftpInfo = {
	host="127.0.0.1", 
	user="hwc", 
	passwd="WVVoa2FrMVVTWHBPUkZVeQ=="
}
--]]

local ret, errMsg, operationIdOrError

--local ret, errMsg = libutils.setFtpInfo(ftpInfo)
--print("Set FTP Info", ret, errMsg)

--[[
ret, operationIdOrError = libutils.uploadToRemote("D:\\Ice-3.6.3.msi", "/promptboard/", true)
--ret, operationIdOrError = libutils.uploadToRemote("D:\\Trip.jpg", "/promptboard/", true)
print("Upload File: ", ret, operationIdOrError)
--]]


---[[
ret, operationIdOrError = libutils.downloadFromRemote("/promptboard/Ice-3.6.3.msi", "D:\\Ice-3.6.3-bak.msi")
print("Download File: ", ret, operationIdOrError)
--]]