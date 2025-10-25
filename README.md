# SurvivalCraft 项目

## 重要通知
**注意：该iOS项目的Windows支持已停止，现在仅支持在macOS系统上编译。请所有开发者尽快升级到macOS环境进行开发。**

**紧急通知：Xamarin已不再受官方支持。微软已停止对Xamarin的更新和维护，建议项目尽快迁移到.NET MAUI或其他现代跨平台框架。**

SurvivalCraft 是一个跨平台的生存类游戏项目，特别优化了 iOS 平台的性能和用户体验。

## 项目概述

该项目是一个使用 .NET 技术栈和 Xamarin 开发的生存游戏，包含完整的游戏引擎、实体系统、渲染系统和平台适配层。游戏提供了丰富的游戏玩法，包括资源收集、建造、生存和探索等元素。本项目特别注重 iOS 平台的性能优化和用户体验。

## 主要功能

- 完整的 3D 游戏引擎
- 实体系统和组件架构
- 物理碰撞检测
- 资源管理和加载
- 多平台支持（iOS、Android），iOS版本特别优化
- 音频系统
- 输入控制系统
- iOS特有功能：Touch ID/Face ID支持、ARKit集成预留接口、iCloud存档同步

## 技术栈

- **编程语言**：C#
- **游戏引擎**：自定义引擎（Engine 目录）
- **跨平台框架**：Xamarin（已不再受官方支持，建议迁移到.NET MAUI）
- **渲染技术**：OpenTK，iOS平台使用Metal渲染优化
- **音频处理**：NVorbis
- **图像格式支持**：PNG、JPG、BMP
- **音频格式支持**：Ogg、Wav
- **iOS特定技术**：Xamarin.iOS（已不再受官方支持）、ARKit预留接口、iOS Keychain、iCloud API

## iOS版本详细说明

### iOS平台架构

- `SurvivalCraftIOS/` - iOS游戏实现，包含游戏核心逻辑的iOS特定绑定
- `Engine_IOS/` - iOS平台特定的引擎实现，针对iOS设备优化
- `SurvivalCraftAPI/` - iOS API层，处理iOS特定功能（如Touch ID/Face ID、iCloud等）
- `EntitySystem_IOS/` - iOS平台的实体系统适配

### iOS平台设计模式

- **MVC架构**：iOS版本采用Model-View-Controller设计模式，分离游戏逻辑、UI和控制流
- **平台适配器模式**：通过`Engine_IOS`和`EntitySystem_IOS`实现平台特定功能的适配
- **委托模式**：大量使用iOS原生的委托模式处理用户交互和系统回调
- **观察者模式**：用于游戏状态变化和UI更新的同步
- **单例模式**：适用于全局游戏管理器和资源访问点

### iOS平台与Android平台差异

- **渲染优化**：iOS使用Metal渲染管线，Android使用OpenGL ES
- **触摸处理**：iOS和Android的触摸事件模型有差异，需在`Engine.Input`中分别处理
- **资源加载**：iOS设备存储结构与Android不同，资源加载路径需要平台特定处理
- **生命周期管理**：iOS应用生命周期（如进入后台、前台）与Android有显著差异
- **权限模型**：iOS权限系统更为严格，需要更精细的权限请求处理

### iOS开发环境要求

- macOS系统（推荐macOS 11或更高版本）
- Visual Studio for Mac 2022或更高版本
- Xcode 13或更高版本
- iOS SDK 15.0或更高版本
- Apple开发者账号（用于真机测试和发布）
- iOS设备（用于真机测试，推荐iPhone 8或更新机型）

### iOS特定优化

- **Metal渲染管线**：针对Apple GPU优化的渲染路径，提供比OpenGL更好的性能
  - 使用Metal着色语言(MSL)重写关键着色器
  - 实现Metal特有的资源管理和内存优化
  - 针对A系列处理器进行优化的矩阵运算
- **内存管理**：使用iOS自动引用计数(ARC)和手动内存管理相结合的策略
  - 实现资源池化机制减少内存碎片
  - 使用`WeakReference`避免内存泄漏
  - 实现智能对象池管理临时对象
- **触摸控制系统**：针对iOS多点触控优化的输入系统
  - 支持3D Touch（如果设备支持）
  - 优化多点触控手势识别
  - 实现iOS风格的手势反馈
- **电池优化**：智能渲染循环和资源加载策略
  - 实现动态帧率调节
  - 后台时自动释放非必要资源
  - 使用iOS能源诊断工具优化电池使用
- **热启动优化**：应用快速启动机制
  - 实现资源预加载和缓存策略
  - 优化启动时的渲染优先级
  - 后台任务调度优化
- **后台处理**：符合iOS后台执行政策的实现
  - 实现后台存档同步
  - 支持后台下载资源
  - 适配iOS的后台刷新机制

### iOS构建步骤

1. **准备环境**：
   - 确保已安装最新版本的Xcode和Visual Studio for Mac
   - 配置Apple开发者账号和证书
   - 下载并安装iOS SDK

2. **配置项目**：
   - 打开`SurvivalCraft.sln`解决方案
   - 选择`SurvivalCraftAPI`作为启动项目
   - 在项目属性中配置Bundle Identifier和签名信息
   - 确保Entitlements.plist正确配置（用于iCloud、Keychain等功能）

3. **构建选项**：
   - **模拟器构建**：选择`iPhoneSimulator`平台，可在Mac上的iOS模拟器中运行
   - **真机调试构建**：选择`iPhone`平台，配置好开发者证书后可在连接的iOS设备上调试
   - **发布构建**：选择`Release`配置，用于App Store提交

4. **运行和调试**：
   - 模拟器测试：直接运行到iOS模拟器
   - 真机测试：确保设备已连接并信任，选择设备后运行
   - 使用Xcode Instruments进行性能分析和调试

### iOS特有功能实现

- **Touch ID/Face ID认证**：使用LocalAuthentication框架实现
  - 实现安全的用户身份验证
  - 保护游戏内敏感数据
  - 支持多因素认证流程
- **iCloud存档同步**：通过NSFileManager的Ubiquity功能实现
  - 实现游戏存档的自动备份和恢复
  - 支持多设备间的存档同步
  - 解决冲突和数据一致性问题
- **推送通知**：集成APNS服务，支持游戏内事件推送
  - 实现本地和远程通知
  - 支持交互式通知和自定义动作
  - 实现通知分组和优先级管理
- **游戏中心集成**：预留Game Center接口，支持成就和排行榜
  - 实现游戏分数和排名
  - 支持玩家成就和进度跟踪
  - 集成多人游戏功能（预留）
- **ARKit集成**：预留AR功能接口，为未来AR特性扩展做准备
  - 支持AR场景识别和平面检测
  - 实现虚拟对象与现实环境交互
  - 集成环境光检测和物理模拟
- **Haptic Feedback**：使用Core Haptics实现触觉反馈
  - 为游戏操作提供振动反馈
  - 支持不同强度和模式的触觉效果
  - 针对不同iOS设备优化振动体验
- **ReplayKit集成**：支持游戏录制和分享
  - 实现游戏内容的录制功能
  - 支持直播游戏过程
  - 提供分享到社交媒体的功能

### iOS调试工具与技巧

- **Xcode Instruments**：性能分析和调试工具
  - **Time Profiler**：分析CPU使用情况，找出性能瓶颈
  - **Allocations**：内存分配分析，检测内存泄漏
  - **Leaks**：自动检测内存泄漏
  - **Energy Log**：分析电池使用情况
  - **Metal System Trace**：分析GPU和Metal性能
- **Console App**：查看应用日志和系统日志
- **Crashlytics**：集成崩溃报告分析
- **Debugging技巧**：
  - 使用条件断点和日志断点
  - 实现自定义调试控件
  - 利用iOS模拟器的模拟位置和网络条件

### iOS用户体验设计指南

- **人机界面指南（HIG）**：遵循Apple的设计规范
  - 确保界面元素大小符合可点击标准（至少44x44pt）
  - 使用iOS标准的导航模式（如标签栏、导航栏）
  - 遵循iOS的色彩和排版指南
- **适配不同屏幕尺寸**：
  - 支持所有iOS设备屏幕尺寸
  - 为iPad实现特定的界面布局
  - 适配深色模式
- **无障碍设计**：
  - 支持VoiceOver
  - 确保文本对比度符合标准
  - 支持动态字体大小
- **交互设计**：
  - 提供清晰的视觉反馈
  - 实现直观的手势操作
  - 优化触摸目标大小和位置

### iOS应用分析与监控

- **App Analytics**：使用App Store Connect的分析功能
- **Firebase Analytics**：集成用户行为分析
- **自定义遥测**：实现关键游戏事件的跟踪
  - 游戏启动时间
  - 关卡完成率
  - 资源加载时间
  - 用户交互模式
- **性能监控**：
  - 帧率监控（确保稳定60fps）
  - 内存使用监控
  - 电池消耗监控

## 项目结构

- `Engine/` - 核心游戏引擎
- `Engine_IOS/` - iOS平台特定的引擎实现，包含iOS优化的渲染、音频和输入处理
- `Engine_android/` - Android平台特定的引擎实现
- `EntitySystem/` - 实体系统核心
- `EntitySystem_IOS/` - iOS平台的实体系统适配
- `Survivalcraft/` - 游戏核心逻辑，包含通用游戏玩法实现
- `SurvivalCraftIOS/` - iOS游戏实现，包含游戏核心逻辑的iOS特定绑定
- `SurvivalCraftAPI/` - iOS API层，处理iOS特定功能和原生集成
- `Launch_android/` - Android启动器
- `packages/` - NuGet包依赖

## 开发环境要求

### iOS开发环境（主要）
- macOS 11或更高版本
- Visual Studio for Mac 2022或更高版本（注意：虽然仍支持Xamarin，但微软已不再更新Xamarin）
- Xcode 13或更高版本
- iOS SDK 15.0或更高版本
- Apple开发者账号
- iOS设备（推荐iPhone 8或更新机型）
- **迁移准备**：为未来迁移到.NET MAUI，建议安装.NET 7.0 SDK或更高版本

### 其他环境（已停止支持）
- **Windows环境支持已停止**：不再支持Windows平台开发和构建
- 之前使用Windows环境的开发者请尽快迁移到macOS环境

## iOS构建说明

### 模拟器构建
1. 打开`SurvivalCraft.sln`解决方案
2. 选择`SurvivalCraftAPI`作为启动项目
3. 在解决方案配置中选择`Debug`和`iPhoneSimulator`平台
4. 选择目标模拟器设备（如iPhone 14）
5. 点击构建并运行按钮

### 真机构建与调试
1. 确保Apple开发者账号已配置，并且设备已添加到开发者账号中
2. 在Visual Studio for Mac中配置签名信息：
   - 右键点击`SurvivalCraftAPI`项目，选择"选项"
   - 导航到"iOS Bundle Signing"
   - 选择正确的"Signing Identity"和"Provisioning Profile"
3. 确保`Entitlements.plist`包含所需的权限（如iCloud、Keychain访问等）
4. 连接iOS设备到Mac
5. 在设备选择器中选择已连接的设备
6. 点击构建并运行按钮

### 发布构建
1. 在解决方案配置中选择`Release`和`iPhone`平台
2. 右键点击`SurvivalCraftAPI`项目，选择"存档"
3. 完成存档后，按照向导进行App Store提交准备
4. 确保所有图标和启动屏幕图像符合App Store要求
5. 生成并上传IPA文件到App Store Connect

### 免费开发者账号构建说明

对于使用免费Apple开发者账号的开发者，无法通过标准的Xcode或Visual Studio for Mac发布流程直接生成可分发的IPA文件。以下是免费开发者账号的构建方案：

#### 使用命令行脚本构建
1. **创建构建脚本**：
   ```bash
   #!/bin/bash
   # 保存为build_ios_free.sh
   
   # 设置项目路径和输出目录
   PROJECT_PATH="$(pwd)/SurvivalCraft.sln"
   OUTPUT_DIR="$(pwd)/output"
   
   # 创建输出目录
   mkdir -p "$OUTPUT_DIR"
   
   # 使用msbuild构建项目
   msbuild "$PROJECT_PATH" /t:Build /p:Configuration=Release /p:Platform=iPhone /p:OutputPath="$OUTPUT_DIR"
   
   echo "构建完成，输出目录: $OUTPUT_DIR"
   ```

2. **生成IPA文件**：
   ```bash
   #!/bin/bash
   # 保存为package_ipa_free.sh
   
   # 设置参数
   APP_DIR="$(pwd)/output/device-builds/iphoneos16.4-iphoneos"
   PROFILE_NAME="Free_Development"
   BUNDLE_ID="com.yourcompany.survivalcraft"
   
   # 创建IPA包
   xcrun -sdk iphoneos PackageApplication -v "$APP_DIR/SurvivalCraftAPI.app" -o "$(pwd)/SurvivalCraftAPI.ipa" --sign "iPhone Developer" --embed "$HOME/Library/MobileDevice/Provisioning\ Profiles/$PROFILE_NAME.mobileprovision"
   
   echo "IPA文件已生成: SurvivalCraftAPI.ipa"
   ```

#### 使用Fastlane自动化构建

1. **安装Fastlane**：
   ```bash
   brew install fastlane
   ```

2. **配置Fastfile**：
   ```ruby
   # Fastfile示例
   default_platform(:ios)
   
   platform :ios do
     desc "为免费开发者账号构建应用"
     lane :build_free do
       # 设置工作目录
       cocoapods(
         clean_install: true,
         use_bundle_exec: false
       )
       
       # 构建项目
       gym(
         scheme: "SurvivalCraftAPI",
         clean: true,
         export_method: "development",
         export_options: {
           provisioningProfiles: {
             "com.yourcompany.survivalcraft" => "Free_Development"
           }
         },
         output_directory: "./build_output"
       )
     end
   end
   ```

#### 注意事项
- 免费开发者账号构建的应用有效期为7天
- 每个免费账号最多只能在10台设备上安装应用
- 需要在目标设备上信任开发者证书
- 构建的应用无法提交到App Store，只能用于测试和开发

### iOS应用提交与发布流程

1. **预提交准备**：
   - 完成App Store Connect应用信息填写
   - 准备应用截图和预览视频（适用于不同设备尺寸）
   - 编写应用描述、关键词和更新说明
   - 设置应用价格和地区可用性
2. **提交审核**：
   - 使用Application Loader或Xcode上传构建版本
   - 在App Store Connect中添加构建版本到审核队列
   - 回答Apple的审核问题（如内容类型、使用的API等）
3. **审核过程**：
   - 监控审核状态（通常需要1-3个工作日）
   - 准备回应可能的审核反馈
   - 如有拒审情况，根据Apple的反馈进行修改并重新提交
4. **发布管理**：
   - 设置手动发布或自动发布日期
   - 监控应用发布后的崩溃报告和用户评论
   - 准备后续更新的计划

### iOS测试策略

- **单元测试**：使用XCTest框架测试核心功能
- **UI测试**：自动化UI交互测试
- **设备兼容性测试**：
  - 确保在不同iOS版本上正常运行
  - 测试不同屏幕尺寸的适配
  - 测试在低性能设备上的性能表现
- **用户体验测试**：
  - 进行用户测试收集反馈
  - 分析用户行为数据
  - 优化游戏控制和界面
- **性能基准测试**：
  - 建立性能基准指标
  - 定期进行性能回归测试
  - 针对不同iOS设备设置性能配置文件

## 许可证

项目使用自定义许可证，仅供内部使用。iOS版本的分发需遵守Apple App Store审核指南。

## iOS开发注意事项

### Xamarin迁移建议
- **迁移到.NET MAUI**：微软推荐的Xamarin继任者，提供现代化的开发体验
- **迁移步骤**：
  1. 确保项目使用.NET 6或更高版本
  2. 创建新的.NET MAUI项目
  3. 逐步迁移UI组件和业务逻辑
  4. 更新第三方依赖到兼容.NET MAUI的版本
  5. 使用.NET MAUI的新特性重写特定平台代码
- **时间紧迫性**：建议在6个月内完成迁移，以避免安全风险
- **资源**：可参考微软官方迁移文档和社区迁移指南

### 性能优化
- **内存管理**：iOS设备内存有限，需特别注意纹理和模型资源的加载和释放
- **电池消耗**：避免长时间占用CPU和GPU，合理使用iOS的电源管理API
- **渲染优化**：针对不同iOS设备（iPhone/iPad）使用不同的渲染质量设置
- **启动时间**：确保首次启动时间在App Store要求的范围内（通常不超过5秒）

### App Store合规
- 确保所有图标和启动屏幕符合Apple Human Interface Guidelines
- 实现正确的隐私政策，特别是如果应用访问用户数据
- 确保应用不包含私有API调用
- 所有第三方库必须符合App Store许可要求

### 证书和配置
- 定期更新开发者证书和配置文件
- 确保App ID包含所有必要的功能标识符（如iCloud、推送通知等）
- 为开发、测试和发布环境使用不同的配置文件

### 常见问题解决
- **构建失败**：检查Xcode版本兼容性和SDK路径
- **签名错误**：验证证书和配置文件是否有效且匹配
- **性能问题**：使用Instruments进行性能分析，特别关注内存泄漏和CPU/GPU使用率
- **崩溃问题**：配置正确的崩溃报告工具，分析崩溃日志

## 版本兼容性

iOS版本支持：
- **最低支持版本**：iOS 14.0
- **推荐版本**：iOS 16.0及以上
- **硬件支持**：iPhone 7及更新机型，iPad Air 2及更新机型，所有iPad Pro型号