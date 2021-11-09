# SharpNet
测试Elastic EDR写的小工具。直接调用API实现net user / net group功能，绕过edr对net.exe/net1.exe的检测。同时利用修改windows目录的方式实现添加用户、激活guest用户，由于没有使用到API，逃避了对敏感API的检测，实测过火绒、360、Elastic EDR。
使用方法：
  sharpnet.exe user //枚举本地用户
  sharpnet.exe user /do //枚举域用户
  sharpnet.exe user username //查看本地用户信息
  sharpnet.exe user username /do 查看域用户信息

  group同上

  sharpnet.exe /active 激活guest并添加到管理组
  sharpnet.exe user pass /add 添加账户并加入到管理组
