# DF3D - 3D 晕动症缓解工具

🛡️ 一款 Windows 桌面叠加工具，通过边缘遮罩和固定参照点缓解玩 3D 游戏时的眩晕感。

## 原理

3D 晕动症（模拟器眩晕）的根本原因是**视觉-前庭感觉冲突**：

```
眼睛看到 → "你在移动"
内耳前庭 → "你没动"
大脑处理 → 矛盾信号 → 恶心、头晕
```

DF3D 通过两种方式缓解：

1. **边缘遮罩（Vignette）** — 遮挡屏幕周边区域，减少周边视觉中的运动信息。研究表明周边视觉是诱发晕动症的主要因素。
2. **固定参考点（Crosshair）** — 在屏幕中心提供一个"静止"的视觉锚点，帮助大脑建立稳定的参照系。

## 功能

### 叠加层（Vignette）
- 3 种形状：Box（矩形）、Dome（椭圆）、Flag（旗帜）
- 7 种尺寸：XXS ~ XXL
- 7 种渐变长度：+0 ~ +6
- 5 种颜色 + 自定义颜色
- 不透明度滑块（0% ~ 100%）
- 支持 16:9 和 21:9 宽高比
- 分屏模式（垂直/水平）

### 十字线（Crosshair）
- 3 种形状：Circle（圆环）、Cross（十字）、Diamond（菱形）
- 7 种尺寸 + 可调粗细
- 可调位置（X/Y 偏移）
- 5 种颜色 + 自定义颜色
- 不透明度滑块

### 热键（全局生效）
| 热键 | 功能 |
|------|------|
| `Ctrl+Shift+F1` | 切换叠加层显示/隐藏 |
| `Ctrl+Shift+F2` | 切换十字线显示/隐藏 |
| `Ctrl+Shift+F3` | 切换分屏模式 |
| `Ctrl+Shift+F4` | 切换显示模式 |
| `Ctrl+Shift+F5` | 切换遮罩大小 |
| `Ctrl+Shift+F6` | 切换预设 |

> 所有热键支持自定义录制。

### 预设系统
- **默认** — 通用设置
- **FPS 游戏** — Dome 遮罩 + 小十字线
- **赛车游戏** — Flag 遮罩 + 大遮罩
- **轻度防护** — 轻微遮罩效果

### 其他
- 🖥️ 多显示器支持（PerMonitorV2 DPI 感知）
- 🔒 单实例运行
- 🚀 开机自启动
- 💾 设置自动保存（%AppData%/DF3D/settings.json）
- 🎯 系统托盘常驻

## 技术栈

- **语言**：C# (.NET 8)
- **UI 框架**：WPF
- **系统托盘**：H.NotifyIcon.Wpf
- **目标平台**：Windows 10/11

## 构建与运行

```bash
# 克隆仓库
git clone https://github.com/YOUR_USERNAME/df3d.git
cd df3d

# 构建
dotnet build

# 运行
dotnet run --project DF3D

# 发布
dotnet publish DF3D -c Release -r win-x64 --self-contained
```

## 项目结构

```
DF3D/
├── App.xaml/cs              # 应用入口、系统托盘
├── Windows/
│   ├── OverlayWindow        # 透明叠加层（核心）
│   ├── SettingsWindow       # 暗色主题设置界面
│   └── HotkeyRecordWindow   # 热键录制对话框
├── Controls/
│   ├── VignetteControl      # 边缘遮罩渲染
│   └── CrosshairControl     # 十字线渲染
├── Models/                  # 数据模型
├── Services/                # 业务逻辑服务
└── Helpers/                 # Win32 API 封装
```

## 已知限制

- **独占全屏游戏不支持**：WPF 叠加层无法覆盖独占全屏的 DirectX/Vulkan 游戏。绝大多数现代游戏默认使用无边框全屏模式，叠加层可以正常工作。如遇问题，请在游戏设置中切换为"无边框窗口"模式。

## 许可证

MIT License
