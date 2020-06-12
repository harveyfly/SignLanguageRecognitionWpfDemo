# 项目介绍
+ **使用C#基于.Net开发的WPF中文手语词识别应用程序**

## 应用界面
![](https://yesgithub-1254021701.cos.ap-beijing.myqcloud.com/ui.png)

## 应用程序整体功能模块
![](https://yesgithub-1254021701.cos.ap-beijing.myqcloud.com/SLRAPP.png)

## 开发环境
+ Windows 10
+ Visual Studio 2019
+ .Net Framework 4.7.2

## Nuget Packages
使用 VS Nuget 安装以下包
```
"Microsoft.Azure.Kinect.BodyTracking" version="1.0.1" targetFramework="net472" 
"Microsoft.Azure.Kinect.BodyTracking.Dependencies" version="0.9.1" targetFramework="net472" 
"Microsoft.Azure.Kinect.BodyTracking.Dependencies.cuDNN" version="0.9.1" targetFramework="net472" 
"Microsoft.Azure.Kinect.Sensor" version="1.4.0" targetFramework="net472" 
"Microsoft.Kinect" version="2.0.1410.19000" targetFramework="net472" 
"Newtonsoft.Json" version="12.0.3" targetFramework="net472" 
"System.Buffers" version="4.4.0" targetFramework="net472" 
"System.Memory" version="4.5.3" targetFramework="net472" 
"System.Numerics.Vectors" version="4.5.0" targetFramework="net472" 
"System.Reflection.Emit.Lightweight" version="4.6.0" targetFramework="net472" 
"System.Runtime.CompilerServices.Unsafe" version="4.5.2" targetFramework="net472"
```

# 项目说明
## 界面
+ 左上为系统控制区，可以选择Kinect设备ID、服务器地址、关键帧算法阈值等，并且显示识别结果；
+ 中上区为Kinect视频显示区域，可以切换RGB和深度图像模式；
+ 右上分为两块，分别显示人体骨骼关节点的相对位置图像和帧间差分距离的实时数据跟踪；
+ 底部为日志显示区域，显示系统输出的日志信息。
  
## 操作方法
+ 连接服务器(服务端见项目[SignLanguageRecognition](https://github.com/dluthwy/SignLanguageRecognition))
+ 打开Kinect摄像头
+ `Start`按钮开始识别

## 注意
**由于本项目只是一个中文手语词的分类识别，所以为了进行时域分割，我们设置人体身体保持不动一段时间为开始和结束的标志**