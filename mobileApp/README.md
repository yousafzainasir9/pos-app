# Cookie Barrel Mobile App

React Native mobile ordering application for Cookie Barrel POS system.

## 📁 Project Structure

```
CookieBarrelMobile/
├── src/
│   ├── api/              # API services and client
│   ├── components/       # Reusable UI components
│   ├── screens/          # Screen components
│   ├── store/            # Redux state management
│   ├── navigation/       # Navigation configuration
│   ├── types/            # TypeScript type definitions
│   ├── utils/            # Utility functions
│   ├── constants/        # App constants and theme
│   └── App.tsx           # Main app component
├── android/              # Android native code
├── ios/                  # iOS native code (Mac only)
└── package.json          # Dependencies
```

## 🚀 Quick Start

### Prerequisites

- Node.js 18+
- React Native CLI
- Android Studio (for Android development)
- Java JDK 17

### Installation

1. **First Time Setup:**
   ```bash
   # Navigate to project
   cd D:\pos-app\CookieBarrelMobile
   
   # Install dependencies
   npm install
   ```

2. **Run the App:**
   
   **Option A: Use batch file (Windows)**
   ```bash
   # Double-click run-android.bat
   # OR run from terminal:
   run-android.bat
   ```
   
   **Option B: Manual**
   ```bash
   # Terminal 1: Start Metro bundler
   npm start
   
   # Terminal 2: Run on Android
   npx react-native run-android
   ```

## 🔧 Configuration

### Backend API URL

Update the API URL in `src/api/client.ts`:

```typescript
// For Android Emulator:
const API_BASE_URL = 'http://10.0.2.2:5000/api';

// For Physical Device (replace with your computer's IP):
const API_BASE_URL = 'http://192.168.1.XXX:5000/api';
```

To find your IP address:
```bash
ipconfig
# Look for IPv4 Address
```

## 📱 Features

### Week 1 (Current)
- ✅ Project setup and configuration
- ✅ Navigation (Bottom tabs + Stack)
- ✅ Redux state management
- ✅ API integration setup
- 🚧 Product browsing (Coming Day 2-3)
- 🚧 Shopping cart (Coming Day 4)
- 🚧 Order placement (Coming Day 5)

### Week 2 (Planned)
- Order history
- Order status tracking
- UI/UX polish
- Testing

### Week 3 (Planned)
- POS integration
- Order notifications
- Final testing

## 🛠️ Development Commands

```bash
# Start Metro bundler
npm start

# Run on Android
npx react-native run-android

# Run on iOS (Mac only)
npx react-native run-ios

# Clear cache
npm start -- --reset-cache

# Check setup
npx react-native doctor

# View Android logs
npx react-native log-android
```

## 🐛 Troubleshooting

### Metro Bundler Issues
```bash
npm start -- --reset-cache
```

### Build Errors
```bash
cd android
./gradlew clean
cd ..
npx react-native run-android
```

### Cannot Connect to Backend
1. Ensure backend is running on port 5000
2. Check API URL in `src/api/client.ts`
3. For physical device, use computer's IP address
4. Make sure firewall allows connections

### Vector Icons Not Showing
Make sure `android/app/build.gradle` includes:
```gradle
apply from: "../../node_modules/react-native-vector-icons/fonts.gradle"
```

Then rebuild the app.

## 📦 Tech Stack

- **React Native 0.73** - Mobile framework
- **TypeScript** - Type safety
- **Redux Toolkit** - State management
- **React Navigation** - Navigation
- **Axios** - API calls
- **React Native Paper** - UI components
- **AsyncStorage** - Local storage

## 📖 Documentation

See `SETUP_INSTRUCTIONS.md` for detailed setup guide.

## 🎯 Current Status

**Day 1: ✅ COMPLETE**
- All project files created
- Configuration complete
- Ready for development

**Day 2-3: NEXT**
- Implement product browsing
- Add search and filters
- Create product cards

## 🔗 Related Projects

- Backend API: `D:\pos-app\backend`
- Web Frontend: `D:\pos-app\frontend`

## 📄 License

Private - Cookie Barrel POS System
