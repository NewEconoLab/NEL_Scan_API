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

修改配置文件放在执行文件瞎，配置文件大致如下 :
```json
{
  "block_mongodbConnStr_testnet": "",
  "block_mongodbDatabase_testnet": "",
  "analy_mongodbConnStr_testnet": "",
  "analy_mongodbDatabase_testnet": "",
  "notify_mongodbConnStr_testnet": "",
  "notify_mongodbDatabase_testnet": "",
  "snapshot_mongodbConnStr_testnet": "",
  "snapshot_mongodbDatabase_testnet": "",
  "nelJsonRPCUrl_testnet": "",
  "block_mongodbConnStr_mainnet": "",
  "block_mongodbDatabase_mainnet": "",
  "analy_mongodbConnStr_mainnet": "",
  "analy_mongodbDatabase_mainnet": "",
  "notify_mongodbDatabase_mainnet": "",
  "snapshot_mongodbConnStr_mainnet": "",
  "snapshot_mongodbDatabase_mainnet": "",
  "nelJsonRPCUrl_mainnet": "",
   "dao_mongodbConnStr_testnet": "",
  "dao_mongodbDatabase_testnet": "",
  "auctionStateColl_testnet": "",
  "auctionStateColl_mainnet": "",
  "bonusAddress_testnet": "",
  "bonusAddress_mainnet": "",
  "bonusStatisticCol_testnet": "",
  "bonusStatisticCol_mainnet": "",
  "NNsfixedSellingAddr_testnet": "",
  "NNsfixedSellingAddr_mainnet": "",
  "NNSfixedSellingColl_testnet": "",
  "NNSfixedSellingColl_mainnet": "",
  "domainCenterColl_testnet": "",
  "domainCenterColl_mainnet": "",
  "bonusSgasCol_testnet": "",
  "bonusSgasCol_mainnet": "",
  "id_sgas_testnet": "",
  "id_sgas_mainnet": "",
  "notifyCodeColl_testnet": "",
  "notifySubsColl_testnet": "",
  "bonusSgas_mongodbConnStr_testnet": "",
  "bonusSgas_mongodbDatabase_testnet": "",
  "bonusSgas_mongodbConnStr_mainnet": "",
  "bonusSgas_mongodbDatabase_mainnet": "",
  "startMonitorFlag": "1",
  "test":""
}
```


编译并运行
```
dotnet publish
cd NEL_Scan_API/NEL_Scan_API/bin/Debug/netcoreapp2.0
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

After modifying the configuration file and putting it in the execution file, the configuration file is roughly as follows :
```json
{
  "block_mongodbConnStr_testnet": "",
  "block_mongodbDatabase_testnet": "",
  "analy_mongodbConnStr_testnet": "",
  "analy_mongodbDatabase_testnet": "",
  "notify_mongodbConnStr_testnet": "",
  "notify_mongodbDatabase_testnet": "",
  "snapshot_mongodbConnStr_testnet": "",
  "snapshot_mongodbDatabase_testnet": "",
  "nelJsonRPCUrl_testnet": "",
  "block_mongodbConnStr_mainnet": "",
  "block_mongodbDatabase_mainnet": "",
  "analy_mongodbConnStr_mainnet": "",
  "analy_mongodbDatabase_mainnet": "",
  "notify_mongodbDatabase_mainnet": "",
  "snapshot_mongodbConnStr_mainnet": "",
  "snapshot_mongodbDatabase_mainnet": "",
  "nelJsonRPCUrl_mainnet": "",
   "dao_mongodbConnStr_testnet": "",
  "dao_mongodbDatabase_testnet": "",
  "auctionStateColl_testnet": "",
  "auctionStateColl_mainnet": "",
  "bonusAddress_testnet": "",
  "bonusAddress_mainnet": "",
  "bonusStatisticCol_testnet": "",
  "bonusStatisticCol_mainnet": "",
  "NNsfixedSellingAddr_testnet": "",
  "NNsfixedSellingAddr_mainnet": "",
  "NNSfixedSellingColl_testnet": "",
  "NNSfixedSellingColl_mainnet": "",
  "domainCenterColl_testnet": "",
  "domainCenterColl_mainnet": "",
  "bonusSgasCol_testnet": "",
  "bonusSgasCol_mainnet": "",
  "id_sgas_testnet": "",
  "id_sgas_mainnet": "",
  "notifyCodeColl_testnet": "",
  "notifySubsColl_testnet": "",
  "bonusSgas_mongodbConnStr_testnet": "",
  "bonusSgas_mongodbDatabase_testnet": "",
  "bonusSgas_mongodbConnStr_mainnet": "",
  "bonusSgas_mongodbDatabase_mainnet": "",
  "startMonitorFlag": "1",
  "test":""
}
```

Compile and run :
```
dotnet publish
cd NEL_Scan_API/NEL_Scan_API/bin/Debug/netcoreapp2.0
dotnet NEL_Scan_API.dll
```
