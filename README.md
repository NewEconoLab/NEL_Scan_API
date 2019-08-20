# NEL_Scan_API
[简体中文](#zh) |    [English](#en) 

<a name="zh">简体中文</a>
## 概述 :
本项目主要是给 _[NEL浏览器](https://scan.nel.group/)_ 提供接口服务。

## 接口详情 :
我们将接口文档用小幺鸡进行了整理,详细可以参阅 _[接口文档](http://www.xiaoyaoji.cn/doc/2veptPpn9o)_

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

修改配置文件放在执行文件下，配置文件大致如下 :
```json
{
  "block_mongodbConnStr_testnet": "测试网基础数据库连接地址",
  "block_mongodbDatabase_testnet": "测试网基础数据库名称", 
  "analy_mongodbConnStr_testnet": "测试网分析数据库连接地址",
  "analy_mongodbDatabase_testnet": "测试网分析数据库名称",
  "notify_mongodbConnStr_testnet": "测试网合约通知数据库连接地址",
  "notify_mongodbDatabase_testnet": "测试网合约通知数据库名称",
  "snapshot_mongodbConnStr_testnet": "测试网快照数据连接地址",
  "snapshot_mongodbDatabase_testnet": "测试网快照数据库名称",
  "nelJsonRPCUrl_testnet": "测试网基础api请求地址",
  "block_mongodbConnStr_mainnet": "主网基础数据库连接地址",
  "block_mongodbDatabase_mainnet": "主网基础数据库名称",
  "analy_mongodbConnStr_mainnet": "主网分析数据库连接地址",
  "analy_mongodbDatabase_mainnet": "主网分析数据库名称",
  "notify_mongodbDatabase_mainnet": "主网合约通知数据库连接地址",
  "snapshot_mongodbConnStr_mainnet": "主网合约通知数据库名称",
  "snapshot_mongodbDatabase_mainnet": "主网快照数据库名称",
  "nelJsonRPCUrl_mainnet": "主网基础api请求地址",
  "dao_mongodbConnStr_testnet": "测试网dao数据库连接地址，这里可为空",
  "dao_mongodbDatabase_testnet": "测试网dao数据库名称，这里可为空",
  "auctionStateColl_testnet": "测试网竞拍状态表",
  "auctionStateColl_mainnet": "主网竞拍状态表",
  "bonusAddress_testnet": "测试网分红地址",
  "bonusAddress_mainnet": "主网分红地址",
  "bonusStatisticCol_testnet": "测试网分红统计表",
  "bonusStatisticCol_mainnet": "主网分红统计表",
  "NNsfixedSellingAddr_testnet": "测试网出售合约地址",
  "NNsfixedSellingAddr_mainnet": "主网出售合约地址",
  "NNSfixedSellingColl_testnet": "测试网出售合约表",
  "NNSfixedSellingColl_mainnet": "主网出售合约表",
  "domainCenterColl_testnet": "测试网域名中心表",
  "domainCenterColl_mainnet": "主网域名中心表",
  "bonusSgasCol_testnet": "测试网分红sgas表",
  "bonusSgasCol_mainnet": "主网分红sgas表",
  "id_sgas_testnet": "测试网sgas哈希",
  "id_sgas_mainnet": "主网sgas哈希"
}
```


编译并运行
```
dotnet publish
cd NEL_Scan_API/NEL_Scan_API/bin/Debug/netcoreapp2.0
dotnet NEL_Scan_API.dll
```

## 依赖工程 :
- [爬虫工程](https://github.com/NewEconoLab/NeoBlock-Mongo-Storage)
- [分析工程](https://github.com/NewEconoLab/NeoBlockAnalysis)
- [合约通知工程](https://github.com/NewEconoLab/ContractNotifyCollector)

<a name="en">English</a>
## Overview :
This project mainly provides interface services for _[NEL-Scan](https://scan.nel.group/)_ .

## Interface details
We have compiled the interface documentation. For details, please refer to _[Interface details](http://www.xiaoyaoji.cn/doc/2veptPpn9o)_

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

Modify the configuration file under the execution file, the configuration file is roughly as follows:
```json
{
  "block_mongodbConnStr_testnet": "basic database connnectString at testnet",
  "block_mongodbDatabase_testnet": "basic database name at testnet",
  "analy_mongodbConnStr_testnet": "analysis database connectString at testnet",
  "analy_mongodbDatabase_testnet": "analysis database name at testnet",
  "notify_mongodbConnStr_testnet": "contract notify database connectString at testnet",
  "notify_mongodbDatabase_testnet": "contract notify database name at testnet",
  "snapshot_mongodbConnStr_testnet": "bonus snapshot database connectString at testnet",
  "snapshot_mongodbDatabase_testnet": "bonus snapshot database connectString at testnet",
  "nelJsonRPCUrl_testnet": "basic api request url at testnet",
  "block_mongodbConnStr_mainnet": "basic database connectString at mainnet",
  "block_mongodbDatabase_mainnet": "basic database name at mainnet",
  "analy_mongodbConnStr_mainnet": "analysis database connectString at mainnet",
  "analy_mongodbDatabase_mainnet": "analysis database name at mainnet",
  "notify_mongodbConnStr_mainnet": "contract notify database connectString at mainnet",
  "notify_mongodbDatabase_mainnet": "contract notify database name at mainnet",
  "snapshot_mongodbConnStr_mainnet": "bonus snapshot database connectString at mainnet",
  "snapshot_mongodbDatabase_mainnet": "bonus snapshot database connectString at mainnet",
  "nelJsonRPCUrl_mainnet": "basic api request url at mainnet",
  "dao_mongodbConnStr_testnet": "don't care about it",
  "dao_mongodbDatabase_testnet": "don't care about it",
  "auctionStateColl_testnet": "auction state collection at testnet",
  "auctionStateColl_mainnet": "auction state collection at mainnet",
  "bonusAddress_testnet": "bonus contract's address at testnet",
  "bonusAddress_mainnet": "bonus contract's address at mainnet",
  "bonusStatisticCol_testnet": "bonus statistic collection at testnet",
  "bonusStatisticCol_mainnet": "bonus statistic collection at mainnet",
  "NNSfixedSellingAddr_testnet": "nns selling contract's address at testnet",
  "NNSfixedSellingAddr_mainnet": "nns selling contract's address at mainnet",
  "NNSfixedSellingColl_testnet": "nns selling statistic collection at testnet",
  "NNSfixedSellingColl_mainnet": "nns selling statistic collection at mainnet",
  "domainCenterColl_testnet": "domain center collection at testnet",
  "domainCenterColl_mainnet": "domain center collection at mainnet",
  "bonusSgasCol_testnet": "bonus contract notify hash at testnet",
  "bonusSgasCol_mainnet": "bonus contract notify hash at mainnet",
  "id_sgas_testnet": "cgas hash at testnet",
  "id_sgas_mainnet": "cgas hash at mainnet",
  "startMonitorFlag": "0",
  "test":""
}
```

Compile and run :
```
dotnet publish
cd NEL_Scan_API/NEL_Scan_API/bin/Debug/netcoreapp2.0
dotnet NEL_Scan_API.dll
```

## dependency project :
- [reptile project](https://github.com/NewEconoLab/NeoBlock-Mongo-Storage)
- [analysis project](https://github.com/NewEconoLab/NeoBlockAnalysis)
- [contract notify project](https://github.com/NewEconoLab/ContractNotifyCollector)
