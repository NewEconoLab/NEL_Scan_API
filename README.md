# NEL_Scan_API
[简体中文](#zh) |    [English](#en) 

<a name="zh">简体中文</a>
## 概述 :
本项目主要是给_[NEL浏览器](https://scan.nel.group/)提供接口服务。

## 接口详情 :
我们将接口文档用小幺鸡进行了整理,详细可以参阅 _[接口文档](http://www.xiaoyaoji.cn/doc/2veptPpn9o/edit)_

## 部署演示 :

安装git（如果已经安装则跳过） :
```
yum install git -y
```

安装 dotnet sdk :
```
rpm -Uvh https://packages.microsoft.com/config/rhel/7/packages-microsoft-prod.rpm
yum update
yum install libunwind libicu -y
yum install dotnet-sdk-2.1.200 -y
```

通过git将本工程下载到服务器 :
```
git clone https://github.com/NewEconoLab/NEL_Scan_API.git
```

修改配置文件 :
```
cd NEO_Block_API/NEO_Block_API/NEO_Block_API
vi mongodbsettings.json
```
配置文件大致如下 :
```json
{
  "mongodbConnStr_testnet": "mongodb://**", //测试网数据库地址
  "mongodbDatabase_testnet": "NeoBlockBaseData", //测试网数据库库名
  "neoCliJsonRPCUrl_testnet": ", "//测试网节点所在服务器地址
  "mongodbConnStr_mainnet": "mongodb://**", //主网数据库地址
  "mongodbDatabase_mainnet": "NeoBlockData_mainnet", //主网数据库库名
  "neoCliJsonRPCUrl_mainnet": "" //主网节点所在服务器地址
}
```

编译并运行
```
dotnet publish
cd NEL_Scan_API/NEL_Scan_API/NEL_Scan_API/bin/Debug/netcoreapp2.0
dotnet NEL_Scan_API.dll
```


<a name="en">English</a>
## Overview :
This project mainly provides interface services for _[NEL-Scan](https://scan.nel.group/)_ .

## Interface details
We have compiled the interface documentation. For details, please refer to _[Interface details](http://www.xiaoyaoji.cn/doc/2veptPpn9o/edit)_

## Deployment

install git（Skip if already installed） :
```
yum install git -y
```

install dotnet sdk :
```
rpm -Uvh https://packages.microsoft.com/config/rhel/7/packages-microsoft-prod.rpm
yum update
yum install libunwind libicu -y
yum install dotnet-sdk-2.1.200 -y
```

clone to the server :
```
git clone https://github.com/NewEconoLab/NEL_Scan_API.git
```

vim config :
```
cd NEO_Block_API/NEO_Block_API/NEO_Block_API
vi mongodbsettings.json
```
Introduction of the configuration file :
```json
{
  "mongodbConnStr_testnet": "mongodb://**", //Test network database address
  "mongodbDatabase_testnet": "NeoBlockBaseData", //Test network database name
  "neoCliJsonRPCUrl_testnet": ", "//Test network cli ip
  "mongodbConnStr_mainnet": "mongodb://**", //Main network database address
  "mongodbDatabase_mainnet": "NeoBlockData_mainnet",//Main network database name
  "neoCliJsonRPCUrl_mainnet": "" //Main network cli ip
}
```

Compile and run :
```
dotnet publish
cd NEL_Scan_API/NEL_Scan_API/NEL_Scan_API/bin/Debug/netcoreapp2.0
dotnet NEL_Scan_API.dll
```
